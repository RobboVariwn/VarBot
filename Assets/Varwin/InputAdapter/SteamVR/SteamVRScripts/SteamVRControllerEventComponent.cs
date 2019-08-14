using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Varwin.VRInput;
using Varwin.VRInput.SteamVR;

[RequireComponent(typeof(ControllerInteractionComponent))]
public class SteamVRControllerEventComponent : MonoBehaviour
{
    private ControllerInteractionComponent _controllerInteraction;

    private SteamVR_Input_Sources _hand;
    private SteamVRBooleanAction _triggerUse;
    private SteamVRBooleanAction _touchpadPress;
    private SteamVRBooleanAction _menu;
    private SteamVRBooleanAction _grip;
    private SteamVRBooleanAction _touchPad;
    private SteamVRBooleanAction _turnLeft;
    private SteamVRBooleanAction _turnRight;

    private bool _isTouchpadPressed;
    private bool _isTouchpadReleased;
    private bool _isTriggerPressed;
    private bool _isTriggerReleased;


    private void Start()
    {
        _controllerInteraction = GetComponent<ControllerInteractionComponent>();

        _hand = GetComponent<Hand>().handType;

        CreateSetupAction(ref _triggerUse,
            SteamVR_Actions._default.TriggerUse,
            TriggerPressDown,
            TriggerPressUp);

        CreateSetupAction(ref _touchpadPress,
            SteamVR_Actions._default.TouchpadPress,
            TouchpadPressDown,
            TouchpadPressUp);

        CreateSetupAction(ref _menu,
            SteamVR_Actions._default.Menu,
            MenuPressDown,
            MenuPressUp);

        CreateSetupAction(ref _grip,
            SteamVR_Actions._default.GrabGrip,
            GripPressDown,
            GripPressUp);

        CreateSetupAction(ref _touchPad,
            SteamVR_Actions._default.TouchPadTouch,
            TouchPadTouchDown,
            TouchPadTouchUp);

        CreateSetupAction(ref _turnLeft,
            SteamVR_Actions._default.TurnLeft,
            TurnLeftPressDown,
            null);

        CreateSetupAction(ref _turnRight,
            SteamVR_Actions._default.TurnRight,
            TurnRightPressDown,
            null);

        StartCoroutine(SetupAdditionalEvents());
        StartCoroutine(ControllerActivate());
    }

    private void CreateSetupAction(
        ref SteamVRBooleanAction action,
        SteamVR_Action_Boolean usedAction,
        UnityAction<SteamVR_Behaviour_Boolean, SteamVR_Input_Sources, bool> pressDown,
        UnityAction<SteamVR_Behaviour_Boolean, SteamVR_Input_Sources, bool> pressUp)
    {
        action = gameObject.AddComponent<SteamVRBooleanAction>();

        StartCoroutine(SetupAction(action,
            usedAction,
            pressDown,
            pressUp));
    }

    private IEnumerator SetupAction(
        SteamVRBooleanAction action,
        SteamVR_Action_Boolean usedAction,
        UnityAction<SteamVR_Behaviour_Boolean, SteamVR_Input_Sources, bool> pressDown,
        UnityAction<SteamVR_Behaviour_Boolean, SteamVR_Input_Sources, bool> pressUp)
    {
        action.inputSource = _hand;
        action.booleanAction = usedAction;
        action.enabled = false;

        yield return null;
        action.enabled = true;
        action.onPressDown = new SteamVR_Behaviour_BooleanEvent();
        action.onPressUp = new SteamVR_Behaviour_BooleanEvent();

        if (pressDown != null)
        {
            action.onPressDown.AddListener(pressDown);
        }

        if (pressUp != null)
        {
            action.onPressUp.AddListener(pressUp);
        }
    }

    private IEnumerator SetupAdditionalEvents()
    {
        while (!_touchpadPress || !_triggerUse)
        {
            yield return null;
        }

        _touchpadPress.onUpdate = new SteamVR_Behaviour_BooleanEvent();
        _touchpadPress.onUpdate.AddListener((arg0, sources, b) =>
        {
            if (_isTouchpadPressed && !b)
            {
                _isTouchpadReleased = true;
            }
            else
            {
                _isTouchpadReleased = false;
            }

            _isTouchpadPressed = b;
        });
        
        _triggerUse.onUpdate = new SteamVR_Behaviour_BooleanEvent();
        _triggerUse.onUpdate.AddListener((arg0, sources, b) =>
        {
            if (_isTriggerPressed && !b)
            {
                _isTriggerReleased = true;
            }
            else
            {
                _isTriggerReleased = false;
            }
            
            _isTriggerPressed = b;
        });
    }

    private void TriggerPressUp(SteamVR_Behaviour_Boolean behaviour, SteamVR_Input_Sources sources, bool state)
    {
        TriggerReleased?.Invoke(this,
            new ControllerInput.ControllerInteractionEventArgs
            {
                controllerReference =
                    new PlayerController.ControllerReferenceArgs {hand = sources.SteamSourceToHand()}
            });
    }

    private void TriggerPressDown(SteamVR_Behaviour_Boolean behaviour, SteamVR_Input_Sources sources, bool state)
    {
        TriggerPressed?.Invoke(this,
            new ControllerInput.ControllerInteractionEventArgs
            {
                controllerReference =
                    new PlayerController.ControllerReferenceArgs {hand = sources.SteamSourceToHand()}
            });
    }

    private void TouchpadPressUp(SteamVR_Behaviour_Boolean behaviour, SteamVR_Input_Sources sources, bool state)
    {
        Debug.Log("called touch pad press up " + sources);

        TouchpadReleased?.Invoke(this,
            new ControllerInput.ControllerInteractionEventArgs
            {
                controllerReference =
                    new PlayerController.ControllerReferenceArgs {hand = sources.SteamSourceToHand()}
            });
    }

    private void TouchpadPressDown(SteamVR_Behaviour_Boolean behaviour, SteamVR_Input_Sources sources, bool state)
    {
        Debug.Log("called touch pad press down " + sources);

        TouchpadPressed?.Invoke(this,
            new ControllerInput.ControllerInteractionEventArgs
            {
                controllerReference =
                    new PlayerController.ControllerReferenceArgs {hand = sources.SteamSourceToHand()}
            });
    }

    private void MenuPressUp(SteamVR_Behaviour_Boolean behaviour, SteamVR_Input_Sources sources, bool state)
    {
        Debug.Log("called menu press up " + sources);

        ButtonTwoReleased?.Invoke(this,
            new ControllerInput.ControllerInteractionEventArgs
            {
                controllerReference =
                    new PlayerController.ControllerReferenceArgs {hand = sources.SteamSourceToHand()}
            });
    }

    private void MenuPressDown(SteamVR_Behaviour_Boolean behaviour, SteamVR_Input_Sources sources, bool state)
    {
        Debug.Log("called menu press down " + sources);

        ButtonTwoPressed?.Invoke(this,
            new ControllerInput.ControllerInteractionEventArgs
            {
                controllerReference =
                    new PlayerController.ControllerReferenceArgs {hand = sources.SteamSourceToHand()}
            });
    }

    private void TouchPadTouchUp(SteamVR_Behaviour_Boolean behaviour, SteamVR_Input_Sources sources, bool state)
    {
        Debug.Log("called touchpad press up " + sources);

        TouchpadTouchReleased?.Invoke(this,
            new ControllerInput.ControllerInteractionEventArgs
            {
                controllerReference =
                    new PlayerController.ControllerReferenceArgs {hand = sources.SteamSourceToHand()}
            });
    }

    private void TouchPadTouchDown(SteamVR_Behaviour_Boolean behaviour, SteamVR_Input_Sources sources, bool state)
    {
        Debug.Log("called touchpad press down " + sources);

        TouchpadTouchPressed?.Invoke(this,
            new ControllerInput.ControllerInteractionEventArgs
            {
                controllerReference =
                    new PlayerController.ControllerReferenceArgs {hand = sources.SteamSourceToHand()}
            });
    }

    private void GripPressUp(SteamVR_Behaviour_Boolean behaviour, SteamVR_Input_Sources sources, bool state)
    {
    }

    private void GripPressDown(SteamVR_Behaviour_Boolean behaviour, SteamVR_Input_Sources sources, bool state)
    {
        GripPressed?.Invoke(this,
            new ControllerInput.ControllerInteractionEventArgs
            {
                controllerReference =
                    new PlayerController.ControllerReferenceArgs {hand = sources.SteamSourceToHand()}
            });
    }


    public bool IsButtonPressed(ControllerInput.ButtonAlias gripPress)
    {
        switch (gripPress)
        {
            case ControllerInput.ButtonAlias.GripPress: return SteamVR_Actions._default.GrabGrip.state;
            default: throw new NotImplementedException();
        }
    }

    public void TurnLeftPressDown(SteamVR_Behaviour_Boolean behaviour, SteamVR_Input_Sources sources, bool state)
    {
        TurnLeftPressed?.Invoke(this,
            new ControllerInput.ControllerInteractionEventArgs
            {
                controllerReference =
                    new PlayerController.ControllerReferenceArgs {hand = sources.SteamSourceToHand()}
            });
    }

    public void TurnRightPressDown(SteamVR_Behaviour_Boolean behaviour, SteamVR_Input_Sources sources, bool state)
    {
        TurnRightPressed?.Invoke(this,
            new ControllerInput.ControllerInteractionEventArgs
            {
                controllerReference =
                    new PlayerController.ControllerReferenceArgs {hand = sources.SteamSourceToHand()}
            });
    }


    public void OnGripReleased(
        ControllerInput.ControllerInteractionEventArgs controllerInteractionEventArgs)
    {
        GripReleased?.Invoke(this, controllerInteractionEventArgs);
    }

    IEnumerator ControllerActivate()
    {
        yield return new WaitForSeconds(0.1f);
        
        ControllerEnabled.Invoke(null);
    }

    public ControllerInteractionComponent GetController() => _controllerInteraction;

    public event ControllerInput.ControllerInteractionEventHandler ControllerEnabled;
    public event ControllerInput.ControllerInteractionEventHandler ControllerDisabled;
    public event ControllerInput.ControllerInteractionEventHandler TriggerPressed;
    public event ControllerInput.ControllerInteractionEventHandler TriggerReleased;
    public event ControllerInput.ControllerInteractionEventHandler TouchpadReleased;
    public event ControllerInput.ControllerInteractionEventHandler TouchpadPressed;
    public event ControllerInput.ControllerInteractionEventHandler TouchpadTouchPressed;
    public event ControllerInput.ControllerInteractionEventHandler TouchpadTouchReleased;
    public event ControllerInput.ControllerInteractionEventHandler ButtonTwoPressed;
    public event ControllerInput.ControllerInteractionEventHandler ButtonTwoReleased;
    public event ControllerInput.ControllerInteractionEventHandler GripPressed;
    public event ControllerInput.ControllerInteractionEventHandler GripReleased;
    public event ControllerInput.ControllerInteractionEventHandler TurnLeftPressed;
    public event ControllerInput.ControllerInteractionEventHandler TurnRightPressed;

    public bool IsTouchpadPressed() => _isTouchpadPressed;
    public bool IsTouchpadReleased() => _isTouchpadReleased;
    public bool IsTriggerPressed() => _isTriggerPressed;

    public bool IsTriggerReleased() => _isTriggerReleased;

}