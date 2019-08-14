using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Commands;
using Varwin.Data.ServerData;
using Varwin.Models.Data;
using Varwin.Public;
using Varwin.VRInput;
using Object = UnityEngine.Object;

namespace Varwin
{
    public class GrabAction : InputAction
    {
        private readonly List<IGrabStartAware> _grabStartList;
        private readonly List<IGrabEndAware> _grabEndList;
        private readonly IGrabPointAware _grabPoint;
        private Action _onEditorGrabStart = delegate { };
        private Action _onEditorGrabEnd = delegate { };
        private TransformDT _saveTransform;
        private JointData _saveJointData;
        private TransformDT _onInitTransform;
        private GameObject _gameObject;

        public GrabAction(
            ObjectController objectController,
            GameObject gameObject,
            ObjectInteraction.InteractObject vio) : base(objectController, gameObject, vio)
        {
            _grabStartList = GameObject.GetComponents<IGrabStartAware>().ToList();

            if (_grabStartList.Count > 0)
            {
                AddGrabStartBehaviour();
            }

            _grabEndList = GameObject.GetComponents<IGrabEndAware>().ToList();

            if (_grabEndList.Count > 0)
            {
                AddGrabEndBehaviour();
            }

            _grabPoint = GameObject.GetComponent<IGrabPointAware>();
            _gameObject = gameObject;

            _onEditorGrabStart += OnGrabInit;
            _onEditorGrabEnd += OnUngrabInit;
        }

        #region OVERRIDES

        public override void DisableViewInput()
        {
            Vio.isGrabbable = false;

            if (_grabStartList.Count > 0)
            {
                Vio.InteractableObjectGrabbed -= OnGrabStartVoid;
            }

            if (_grabEndList.Count > 0)
            {
                Vio.InteractableObjectUngrabbed -= OnGrabEndVoid;
            }
        }

        public override void EnableViewInput()
        {
            InteractableObjectBehaviour interactableObjectBehaviour =
                GameObject.GetComponent<InteractableObjectBehaviour>();
            bool canGrab;

            if (interactableObjectBehaviour != null)
            {
                canGrab = interactableObjectBehaviour.IsGrabbable;
            }

            else
            {
                canGrab = _grabStartList.Count > 0 || _grabEndList.Count > 0;
            }

            Vio.isGrabbable = canGrab;

            if (!canGrab)
            {
                return;
            }

            if (_grabStartList.Count > 0)
            {
                Vio.InteractableObjectGrabbed += OnGrabStartVoid;
            }

            if (_grabEndList.Count > 0)
            {
                Vio.InteractableObjectUngrabbed += OnGrabEndVoid;
            }
        }

        protected override void EnableEditorInput()
        {
            if (!IsRootGameObject)
            {
                return;
            }

            Vio.isGrabbable = true;

            PlayerAppearance.InteractControllerAppearance grab =
                InputAdapter.Instance.PlayerAppearance.ControllerAppearance.GetFrom(GameObject)
                ?? InputAdapter.Instance.PlayerAppearance.ControllerAppearance.AddTo(GameObject);

            grab.HideControllerOnGrab = true;

            Vio.SwapControllersFlag = true;

            Vio.grabOverrideButton = ControllerInput.ButtonAlias.GripPress;
            Vio.InteractableObjectGrabbed += EditorGrabbed;
            Vio.InteractableObjectUngrabbed += EditorUngrabbed;

            JointBehaviour jointBehaviour = GameObject.GetComponent<JointBehaviour>();

            if (jointBehaviour != null)
            {
                _onEditorGrabStart += jointBehaviour.OnGrabStart;
                _onEditorGrabEnd += jointBehaviour.OnGrabEnd;
            }
        }

        protected override void DisableEditorInput()
        {
            Vio.isGrabbable = false;
            Vio.InteractableObjectGrabbed -= EditorGrabbed;
            Vio.InteractableObjectUngrabbed -= EditorUngrabbed;
        }

        #endregion

        private void AddGrabStartBehaviour()
        {
            InitGrabAction();
            Vio.grabOverrideButton = ControllerInput.ButtonAlias.GripPress;
           
        }

        private void AddGrabEndBehaviour()
        {
            InitGrabAction();
            Vio.grabOverrideButton = ControllerInput.ButtonAlias.GripPress;
        }

        private void InitGrabAction()
        {
            Vio.isGrabbable = true;

            PlayerAppearance.InteractControllerAppearance grab =
                InputAdapter.Instance.PlayerAppearance.ControllerAppearance.GetFrom(GameObject) ?? 
                InputAdapter.Instance.PlayerAppearance.ControllerAppearance.AddTo(GameObject);

            grab.HideControllerOnGrab = true;

            Vio.SwapControllersFlag = true;
        }

        private GameObject _temp;
        private void OnGrabStartVoid(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            if (Settings.Instance().Multiplayer)
            {
                ChangeOwner();
                ObjectController.photonView.RPC("OnGrabStartRpc", PhotonTargets.All);
            }
            else
            {
                GrabingContext context = new GrabingContext {GameObject = Vio.GetGrabbingObject(), Hand = interactableObjectEventArgs.Hand};

                foreach (IGrabStartAware startAware in _grabStartList)
                {
                    startAware?.OnGrabStart(context);
                }

                if (_grabPoint == null)
                {
                    return;
                }
                
                //Fixed joint grab attach in progress...
                /*Transform grabPosition = 
                    context.GameObject.name.Contains("Left") ? 
                        _grabPoint.GetLeftGrabPoint() : 
                        _grabPoint.GetRightGrabPoint();

                PlayerController.PlayerNodes.ControllerNode handNode = context.GameObject.name.Contains("Left") ? 
                    InputAdapter.Instance.PlayerController.Nodes.LeftHand : 
                    InputAdapter.Instance.PlayerController.Nodes.RightHand;

                _gameObject.AddComponent<GrabPointJoint>().Init(handNode, grabPosition);*/
            }
        }

        private void OnGrabEndVoid(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            if (Settings.Instance().Multiplayer)
            {
                ChangeOwner();
                ObjectController.photonView.RPC("OnGrabEndRpc", PhotonTargets.All);
            }
            else
            {
                foreach (IGrabEndAware grabEndAware in _grabEndList)
                {
                    grabEndAware?.OnGrabEnd();
                }
                
                if (_grabPoint == null)
                {
                    return;
                }

                //Fixed joint grab attach in progress...
                /*GrabPointJoint grabAttach = _gameObject.GetComponent<GrabPointJoint>();
                grabAttach.ReturnAttachPoint();
                Object.Destroy(grabAttach);*/
            }
        }


        private void EditorGrabbed(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            if (Settings.Instance().Multiplayer)
            {
                ChangeOwner();
                ObjectController.photonView.RPC("OnGrabbedRpc", PhotonTargets.All);
            }
            else
            {
                _onEditorGrabStart();
            }
        }

        private void EditorUngrabbed(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            if (Settings.Instance().Multiplayer)
            {
                ObjectController.photonView.RPC("OnUngrabbedRpc", PhotonTargets.All);
            }
            else
            {
                _onEditorGrabEnd();
            }
        }

        private void OnGrabInit()
        {
            _saveTransform = GameObject.transform.ToTransformDT();

            _onInitTransform = ObjectController.gameObject.transform.ToTransformDT();

            if (ObjectController != null)
            {
                _saveJointData = ObjectController.GetJointData();
            }
        }

        private void OnUngrabInit()
        {
            TransformDT newTransform = GameObject.transform.ToTransformDT();

            new ModifyCommand(ObjectController,
                GameObject,
                newTransform,
                _saveTransform,
                _saveJointData);
            ProjectData.ObjectsAreChanged = true;
        }

        public void ReturnPosition()
        {
            _onInitTransform.ToTransformUnity(GameObject.transform);
        }
    }
}