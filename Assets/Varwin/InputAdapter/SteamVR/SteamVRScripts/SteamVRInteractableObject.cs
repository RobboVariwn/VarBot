using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Varwin;
using Varwin.VRInput;


public class SteamVRInteractableObject : MonoBehaviour
{
    private SteamVR_Behaviour_Boolean _triggerInput;
    private Detachable _detachable;
    private Hand _grabbingObject;
    private bool _isUsing;
    private Interactable _interactable;

    public bool IsGrabbable
    {
        get
        {
            if (!_detachable)
            {
                return false;
            }

            return _detachable.IsGrabbable;
        }

        set
        {
            if (!_detachable)
            {
                return;
            }

            _detachable.IsGrabbable = value;
        }
    }

    public bool IsUsable { get; set; }

    public ObjectInteraction.InteractObject.ValidDropTypes validDrop
    {
        set
        {
            if (!_detachable)
            {
                return;
            }

            switch (value)
            {
                case ObjectInteraction.InteractObject.ValidDropTypes.DropAnywhere:
                    _detachable.CanBeDetached = true;

                    break;
                case ObjectInteraction.InteractObject.ValidDropTypes.NoDrop:
                    _detachable.CanBeDetached = false;

                    break;
            }
        }
    }

    public ControllerInput.ButtonAlias useOverrideButton { get; set; }
    public ControllerInput.ButtonAlias grabOverrideButton { get; set; }

    public bool SwapHandsFlag
    {
        get
        {
            if (!_detachable)
            {
                return false;
            }

            return _detachable.attachmentFlags.HasFlag(Hand.AttachmentFlags.DetachOthers);
        }
        set
        {
            if(!_detachable)
            {
                return;
            }

            if (value)
            {
                _detachable.attachmentFlags |= Hand.AttachmentFlags.DetachOthers;
            }
            else
            {
                _detachable.attachmentFlags &= ~Hand.AttachmentFlags.DetachOthers;
            }
        }
    }

    public event ObjectInteraction.InteractObject.InteractableObjectEventHandler InteractableObjectUngrabbed;
    public event ObjectInteraction.InteractObject.InteractableObjectEventHandler InteractableObjectTouched;
    public event ObjectInteraction.InteractObject.InteractableObjectEventHandler InteractableObjectUntouched;
    public event ObjectInteraction.InteractObject.InteractableObjectEventHandler InteractableObjectUsed;
    public event ObjectInteraction.InteractObject.InteractableObjectEventHandler InteractableObjectUnused;
    public event ObjectInteraction.InteractObject.InteractableObjectEventHandler InteractableObjectGrabbed;

    private void Awake()
    {
        _interactable = gameObject.AddComponent<Interactable>();
        _interactable.hideHighlight = new GameObject[] { };

        if (GetComponent<Rigidbody>())
        {
            _detachable = gameObject.AddComponent<Detachable>();
            _detachable.onPickUp = new UnityEvent();
            _detachable.onDetachFromHand = new UnityEvent();
            _detachable.restoreOriginalParent = true;
            SetAttachmentFlags();
            ProjectData.GameModeChanged += mode => { SetAttachmentFlags(); };
        }

        StartCoroutine(SetupAction());
    }

    private void SetAttachmentFlags()
    {
        if (ProjectData.GameMode != GameMode.Edit)
        {
            _detachable.attachmentFlags = 
                Hand.AttachmentFlags.VelocityMovement | 
                Hand.AttachmentFlags.DetachFromOtherHand | 
                Hand.AttachmentFlags.ParentToHand;
        }
    }

    private IEnumerator SetupAction()
    {
        _triggerInput = gameObject.AddComponent<SteamVRBooleanAction>();
        _triggerInput.inputSource = SteamVR_Input_Sources.Any;
        _triggerInput.booleanAction = SteamVR_Actions._default.TriggerUse;
        _triggerInput.enabled = false;

        yield return null;
        _triggerInput.enabled = true;
        _triggerInput.onPressDown = new SteamVR_Behaviour_BooleanEvent();
        _triggerInput.onPressUp = new SteamVR_Behaviour_BooleanEvent();
        _triggerInput.onPressDown.AddListener(Use);
        _triggerInput.onPressUp.AddListener(UseStop);
    }
   

    private void Use(SteamVR_Behaviour_Boolean triggerInput, SteamVR_Input_Sources sources, bool state)
    {
        if (!ValidInput(triggerInput))
        {
            return;
        }

        Debug.Log("Use pressed on " + gameObject.name);

        if (!IsUsable)
        {
            return;
        }

        _isUsing = true;
        
        ControllerInteraction.ControllerHand handContext = triggerInput.booleanAction.activeDevice.GetHand();

        InteractableObjectUsed?.Invoke(this,
            new ObjectInteraction.InteractableObjectEventArgs {interactingObject = gameObject, Hand = handContext});
        Debug.Log("Used " + gameObject.name);
    }

    private bool ValidInput(SteamVR_Behaviour_Boolean triggerInput)
    {
        if (_interactable.hoveringHand == null || !_interactable.isHovering)
        {
            return false;
        }

        SteamVR_Input_Sources usingHandGameObject = triggerInput.booleanAction.activeDevice;
        SteamVR_Input_Sources hoveringHandGameObject = _interactable.hoveringHand.handType;

        return usingHandGameObject == hoveringHandGameObject;
    }

    private void UseStop(SteamVR_Behaviour_Boolean triggerInput, SteamVR_Input_Sources sources, bool state)
    {
        if (!_isUsing)
        {
            return;
        }

        Debug.Log("Use unpressed");

        _isUsing = false;
        
        ControllerInteraction.ControllerHand handContext = triggerInput.booleanAction.activeDevice.GetHand();

        InteractableObjectUnused?.Invoke(this,
            new ObjectInteraction.InteractableObjectEventArgs {interactingObject = gameObject, Hand = handContext});
    }

    private void HandHoverUpdate(Hand hand)
    {
        
    }


    private void OnAttachedToHand(Hand hand)
    {
        _grabbingObject = hand;
        ControllerInteraction.ControllerHand handContext = hand.handType.GetHand();

        InteractableObjectGrabbed?.Invoke(this,
            new ObjectInteraction.InteractableObjectEventArgs {interactingObject = gameObject, Hand = handContext});
        Debug.Log("attached to " + hand.handType);
    }

    private void OnDetachedFromHand(Hand hand)
    {
        _grabbingObject = null;
        ControllerInteraction.ControllerHand handContext = hand.handType.GetHand();

        InteractableObjectUngrabbed?.Invoke(this,
            new ObjectInteraction.InteractableObjectEventArgs {interactingObject = gameObject, Hand = handContext});
    }

    public void ForceStopInteracting()
    {
        if (_grabbingObject)
        {
            _grabbingObject.DetachObject(gameObject);
        }
        else
        {
            Debug.LogWarning("forcing interaction stop while hand is null");
        }
    }

    public GameObject GetGrabbingObject() => _grabbingObject.gameObject;

    public bool IsGrabbed() => _grabbingObject != null;

    public bool IsUsing() => _isUsing;

    public GameObject GetUsingObject()
    {
        if (_grabbingObject != null)
        {
            return _grabbingObject.gameObject;
        }

        if (_interactable.isHovering)
        {
            return _interactable.hoveringHand.gameObject;
        }

        return null;
    }
}
