using System;
using System.Collections.Generic;
using NLog;
using UnityEngine;
using Object = UnityEngine.Object;
using Varwin.Models.Data;
using Varwin.ObjectsInteractions;
using Varwin.Public;
using Varwin.VRInput;

#pragma warning disable 618

namespace Varwin
{
    public class InputController
    {
        private readonly ObjectInteraction.InteractObject _vio;
        private readonly GameObject _gameObject;
        private readonly PhotonView _photonView;
        public ControllerInput.ControllerEvents ControllerEvents;

        private IHighlightAware _highlight;

        // ReSharper disable once NotAccessedField.Local
        private IHapticsAware _haptics;
        private readonly List<InputAction> _inputActions = new List<InputAction>();
        private readonly bool _isRoot;
        private readonly ObjectController _objectController;

        private static InputController _lastHighlightedInputController;
        private static InputController _nextToHighlightInputController;
        private GameObject _highlightOverriderGameObject;

        public HightLightConfig DefaultHighlightConfig;
        private HightLightConfig _useHighlightConfig;

        private bool _highlightEnabled;

        public InputController(
            ObjectController objectController,
            GameObject gameObject,
            PhotonView photonView,
            bool isRoot = false)
        {
            _objectController = objectController;
            _gameObject = gameObject;
            _photonView = photonView;

            _vio = InputAdapter.Instance.ObjectInteraction.Object.AddTo(_gameObject);

            _isRoot = isRoot;
            Init();
            EnableMultiplayer();

            _vio.InteractableObjectGrabbed += OnAnyGrabStart;
            _vio.InteractableObjectUngrabbed += OnAnyGrabEnd;
        }

        public void ReturnPosition()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is GrabAction action)
                {
                    action.ReturnPosition();
                }
            }
        }

        public void Destroy()
        {
            Object.Destroy(_gameObject);
        }

        private void EnableMultiplayer()
        {
            if (!Settings.Instance().Multiplayer)
            {
                return;
            }

            RpcInputControllerMethods rpc = _gameObject.AddComponent<RpcInputControllerMethods>();
            rpc.Init(this);
            _photonView.ObservedComponents.Add(rpc);
        }

        private bool IsObjectInteractable()
        {
            InteractableObjectBehaviour behaviour = _gameObject.GetComponent<InteractableObjectBehaviour>();

            if (behaviour != null)
            {
                return behaviour.IsUsable || behaviour.IsGrabbable || behaviour.IsTouchable;
            }
            
            return _gameObject.GetComponent<IUseStartAware>() != null
                   || _gameObject.GetComponent<IUseEndAware>() != null
                   || _gameObject.GetComponent<ITouchStartAware>() != null
                   || _gameObject.GetComponent<ITouchEndAware>() != null
                   || _gameObject.GetComponent<IGrabStartAware>() != null
                   || _gameObject.GetComponent<IGrabEndAware>() != null
                   || _gameObject.GetComponent<IGrabPointAware>() != null
                   || _gameObject.GetComponent<IPointerInAware>() != null
                   || _gameObject.GetComponent<IPointerOutAware>() != null
                   || _gameObject.GetComponent<IPointerClickAware>() != null;
        }

        private void Init()
        {
            try
            {
                _inputActions.Add(new UseAction(_objectController, _gameObject, _vio));
                _inputActions.Add(new GrabAction(_objectController, _gameObject, _vio));
                _inputActions.Add(new TouchAction(_objectController, _gameObject, _vio));
                _inputActions.Add(new PointerAction(_objectController, _gameObject, _vio));
            }
            catch (Exception e)
            {
                Debug.LogError($"Can't create input actions for {_gameObject.name} Error: {e.Message}");

                return;
            }


            IHighlightAware highlightAware = _gameObject.GetComponent<IHighlightAware>();

            if (Settings.Instance().HighlightEnabled && highlightAware == null)
            {
                highlightAware = _gameObject.AddComponent<DefaultHightlighter>();
            }

            HighlightOverrider highlightOverrider = _gameObject.GetComponent<HighlightOverrider>();

            if (highlightOverrider != null)
            {
                _highlightOverriderGameObject = highlightOverrider.ObjectToHightlight;
            }

            if (highlightAware != null)
            {
                _highlight = highlightAware;
                AddHighLighter(highlightAware);
            }

            if (Settings.Instance().HighlightEnabled && IsObjectInteractable())
            {
                EnableHighlight();
            }

            IHapticsAware hapticsAware = _gameObject.GetComponent<IHapticsAware>();

            if (hapticsAware == null
                && (Settings.Instance().TouchHapticsEnabled
                    || Settings.Instance().GrabHapticsEnabled
                    || Settings.Instance().UseHapticsEnabled))
            {
                hapticsAware = _gameObject.AddComponent<DefaultHaptics>();
            }

            if (hapticsAware != null)
            {
                AddHaptics(hapticsAware);
                _haptics = hapticsAware;
            }
            
            
            var highlightColor = new Color(255f,235f,0f,255f);
			
            HightLightConfig config = new HightLightConfig(true,
                0.3f,
                highlightColor, 
                false,
                0.2f,
                0.1f,
                0.3f,
                Color.yellow,
                true,
                0f,
                0f,
                Color.red,
                false,
                0f,
                1.0f,
                Color.red);


            _useHighlightConfig = config;
        }

        private void OnAnyGrabStart(object sender, ObjectInteraction.InteractableObjectEventArgs e)
        {
            if (!_isRoot)
            {
                return;
            }

            CollisionController collisionController = _gameObject.GetComponent<CollisionController>();

            if (collisionController == null || !collisionController.enabled)
            {
                _gameObject.AddComponent<CollisionController>().InitializeController(this);
            }

            //all I can say is OOF; no regrets
            var hc = InputAdapter.Instance.PlayerController.Nodes.GetControllerReference(e.Hand);
            hc.Controller.AddColliders(hc.Controller.GetGrabbedObject().GetComponentsInChildren<Collider>());
            
            ControllerEvents = InputAdapter.Instance.ControllerInput.ControllerEventFactory.GetFrom(_vio.GetGrabbingObject());
        }

        private void OnAnyGrabEnd(object sender, ObjectInteraction.InteractableObjectEventArgs e)
        {
            if (!_isRoot)
            {
                return;
            }

            CollisionController collisionController = _gameObject.GetComponent<CollisionController>();

            if (collisionController != null)
            {
                collisionController.enabled = false;
                Object.Destroy(collisionController);
            }

            InputAdapter.Instance.PlayerController.Nodes.GetControllerReference(e.Hand).Controller.AddColliders(null);
        }

        private void EnableHighlight()
        {
            if (_highlight != null)
            {
                _vio.InteractableObjectTouched += HighlightObject;
                _vio.InteractableObjectUntouched += UnHighlightObject;
                _vio.InteractableObjectGrabbed += UnHighlightObject;
                //Wrong behavior of the highlight
                //Now we use default highlight from steam
                //For Collision controller use varwin highlight effect
                _vio.InteractableObjectUsed += HighlightObjectOnUse;
                _vio.InteractableObjectUnused += HighlightObjectOnUnUse;

                _highlightEnabled = true;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void DisableHighlight()
        {
            if (_highlight != null)
            {
                UnHighlightObject(null, new ObjectInteraction.InteractableObjectEventArgs());
                _vio.InteractableObjectTouched -= HighlightObject;
                _vio.InteractableObjectUntouched -= UnHighlightObject;
                _vio.InteractableObjectGrabbed -= UnHighlightObject;
                //Wrong behavior of the highlight
                //Now we use default highlight from steam
                //For Collision controller use varwin highlight effect
                _vio.InteractableObjectUsed -= HighlightObjectOnUse;
                _vio.InteractableObjectUnused -= HighlightObjectOnUnUse;

                _highlightEnabled = false;
            }
        }

        public void EnableDrop()
        {
            _vio.validDrop = ObjectInteraction.InteractObject.ValidDropTypes.DropAnywhere;
        }

        public void DisableDrop()
        {
            _vio.validDrop = ObjectInteraction.InteractObject.ValidDropTypes.NoDrop;
        }

        public void ForceDropIfNeeded()
        {
            if (!ControllerEvents.IsButtonPressed(ControllerInput.ButtonAlias.GripPress))
            {
                _vio.ForceStopInteracting();
            }
        }

        private void AddHighLighter(IHighlightAware highlight)
        {
            DefaultHighlightConfig = highlight.HightLightConfig();

            VarwinHighlightEffect highlighter = _highlightOverriderGameObject
                ? _highlightOverriderGameObject.GetComponent<VarwinHighlightEffect>()
                : _gameObject.GetComponent<VarwinHighlightEffect>();

            if (highlighter == null)
            {
                highlighter = _highlightOverriderGameObject
                    ? _highlightOverriderGameObject.AddComponent<VarwinHighlightEffect>()
                    : _gameObject.AddComponent<VarwinHighlightEffect>();
            }

            try
            {
                highlighter.SetConfiguration(DefaultHighlightConfig);
            }
            catch (Exception)
            {
                LogManager.GetCurrentClassLogger().Error($"Can not add highlight to game object = {_gameObject}");
            }
        }

        private void AddHaptics(IHapticsAware haptics)
        {
            HapticsConfig onUse = haptics.HapticsOnUse();
            HapticsConfig onTouch = haptics.HapticsOnTouch();
            HapticsConfig onGrab = haptics.HapticsOnGrab();

            ObjectInteraction.InteractHaptics interactHaptic =
                InputAdapter.Instance.ObjectInteraction.Haptics.GetFrom(_gameObject)
                ?? InputAdapter.Instance.ObjectInteraction.Haptics.AddTo(_gameObject);

            if (onUse != null)
            {
                interactHaptic.StrengthOnUse = onUse.Strength;
                interactHaptic.IntervalOnUse = onUse.Interval;
                interactHaptic.DurationOnUse = onUse.Duration;
            }
            else
            {
                interactHaptic.StrengthOnUse = 0;
                interactHaptic.DurationOnUse = 0;
            }

            if (onTouch != null)
            {
                interactHaptic.StrengthOnTouch = onTouch.Strength;
                interactHaptic.IntervalOnTouch = onTouch.Interval;
                interactHaptic.DurationOnTouch = onTouch.Duration;
            }
            else
            {
                interactHaptic.StrengthOnTouch = 0;
                interactHaptic.DurationOnTouch = 0;
            }

            if (onGrab != null)
            {
                interactHaptic.StrengthOnGrab = onGrab.Strength;
                interactHaptic.IntervalOnGrab = onGrab.Interval;
                interactHaptic.DurationOnGrab = onGrab.Duration;
            }
            else
            {
                interactHaptic.StrengthOnGrab = 0;
                interactHaptic.DurationOnGrab = 0;
            }
        }

        public void Vibrate(
            GameObject controllerObject,
            float strength,
            float duration,
            float interval)
        {
            if (!_vio.IsGrabbed() && !_vio.IsUsing())
            {
                Debug.LogWarning("Can't vibrate with object not grabbed or in use");

                return;
            }

            var playerController =
                InputAdapter.Instance.PlayerController.Nodes.GetControllerReference(controllerObject);

            if (playerController == null)
            {
                Debug.LogWarning("Can't vibrate: " + controllerObject + " is not a controller");

                return;
            }

            playerController.Controller.TriggerHapticPulse(strength, duration, interval);
        }

        private void UnHighlightObject(object sender, ObjectInteraction.InteractableObjectEventArgs e)
        {
            if (_gameObject == null)
            {
                return;
            }

            VarwinHighlightEffect highlighter = _highlightOverriderGameObject
                ? _highlightOverriderGameObject.GetComponent<VarwinHighlightEffect>()
                : _gameObject.GetComponent<VarwinHighlightEffect>();

            if (highlighter == null)
            {
                return;
            }

            highlighter.SetHighlightEnabled(false);

            if (_lastHighlightedInputController == this)
            {
                _lastHighlightedInputController = null;
            }
            else
            {
                _nextToHighlightInputController = this;
            }

            if (_nextToHighlightInputController != null)
            {
                if (_nextToHighlightInputController != this)
                {
                    _nextToHighlightInputController.HighlightObject(this, e);
                }

                _nextToHighlightInputController = null;
            }

            if (sender == null)
            {
                return;
            }

            if (sender.GetType() == typeof(InputController))
            {
                _nextToHighlightInputController = this;
            }
        }

        private void HighlightObject(object sender, ObjectInteraction.InteractableObjectEventArgs e)
        {
            if (_gameObject == null)
            {
                return;
            }

            VarwinHighlightEffect highlighter = _highlightOverriderGameObject
                ? _highlightOverriderGameObject.GetComponent<VarwinHighlightEffect>()
                : _gameObject.GetComponent<VarwinHighlightEffect>();

            if (highlighter == null)
            {
                return;
            }

            _lastHighlightedInputController?.UnHighlightObject(this, e);
            highlighter.SetHighlightEnabled(true);
            _lastHighlightedInputController = this;
        }

        private void HighlightObjectOnUse(object sender, ObjectInteraction.InteractableObjectEventArgs e)
        {
            if (_gameObject == null)
            {
                return;
            }

            VarwinHighlightEffect highlighter = _highlightOverriderGameObject
                ? _highlightOverriderGameObject.GetComponent<VarwinHighlightEffect>()
                : _gameObject.GetComponent<VarwinHighlightEffect>();

            if (highlighter == null)
            {
                return;
            }

            _lastHighlightedInputController?.UnHighlightObject(this, e);
            highlighter.SetHighlightEnabled(true);
            highlighter.SetConfiguration(_useHighlightConfig);
            _lastHighlightedInputController = this;
        }
        
        private void HighlightObjectOnUnUse(object sender, ObjectInteraction.InteractableObjectEventArgs e)
        {
            if (_gameObject == null)
            {
                return;
            }

            VarwinHighlightEffect highlighter = _highlightOverriderGameObject
                ? _highlightOverriderGameObject.GetComponent<VarwinHighlightEffect>()
                : _gameObject.GetComponent<VarwinHighlightEffect>();

            if (highlighter == null)
            {
                return;
            }

            if (highlighter.IsHighlightEnabled)
            {
                _lastHighlightedInputController?.UnHighlightObject(this, e);
                highlighter.SetHighlightEnabled(true);
            }
            
            highlighter.SetConfiguration(DefaultHighlightConfig);
            _lastHighlightedInputController = this;
        }

        public TransformDto GetTransform()
        {
            if (_gameObject.GetComponent<ObjectId>() == null)
            {
                return null;
            }

            int id = _gameObject.GetComponent<ObjectId>().Id;
            TransformDT transform = _gameObject.transform.ToTransformDT();

            return new TransformDto {Id = id, Transform = transform};
        }

        public bool IsConnectedToGameObject(GameObject go) => go == _gameObject;

        public void DisableViewInput()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                inputAction.DisableViewInput();
            }
        }

        public void EnableViewInput()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                inputAction.EnableViewInput();
            }
        }

        public void GameModeChanged(GameMode newGameMode)
        {
            foreach (InputAction inputAction in _inputActions)
            {
                inputAction.GameModeChanged(newGameMode);
            }
            
            if (newGameMode == GameMode.Edit && !_highlightEnabled)
            {
                EnableHighlight();
                return;
            }

            if (!IsObjectInteractable() && _highlightEnabled)
            {
                DisableHighlight();
            }
        }

        public void EnableViewUsing()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is UseAction)
                {
                    inputAction.EnableViewInput();
                }
            }
        }

        public void DisableViewUsing()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is UseAction)
                {
                    inputAction.DisableViewInput();
                }
            }
        }

        public void EnableViewGrab()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is GrabAction)
                {
                    inputAction.EnableViewInput();
                }
            }
        }

        public void DisableViewGrab()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is GrabAction)
                {
                    inputAction.DisableViewInput();
                }
            }
        }

        public void EnableViewTouch()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is TouchAction)
                {
                    inputAction.EnableViewInput();
                }
            }
        }

        public void DisableViewTouch()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is TouchAction)
                {
                    inputAction.DisableViewInput();
                }
            }
        }
    }
}

public class TransformDto
{
    public int Id;
    public TransformDT Transform;
}
