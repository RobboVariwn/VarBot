using SmartLocalization;
using UnityEngine;
using Varwin.UI;
using Varwin.VRInput;

namespace Varwin.Onboarding
{
    public class MoveState : State
    {
        private bool _conditionsMoving;

        public MoveState(OnboardingStateMachine tutorialStateMachine) : base(tutorialStateMachine)
        {
        }

        public override void OnUpdate()
        {
            if (_conditionsMoving)
            {
                StateMachine.ChangeState(new MenuOpeningState(StateMachine, MenuOpeningState.TargetObject.Button));
            }
        }

        public override void OnEnter()
        {
            UIMenu.Instance.OnboardingBlock = true;

            PopupHelper.Show(LanguageManager.Instance.GetTextValue("TUTORIAL_POPUP_MOVING"),
                null,
                StateMachine.ImageContainer.Teleport);
            string teleport = LanguageManager.Instance.GetTextValue("TUTORIAL_TOOLTIP_MOVING");

            TooltipManager.ShowControllerTooltip(teleport,
                ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Touchpad);

            InputAdapter.Instance.PointerController.IsMenuOpened = false;
            
            // We need to test what GO was taking the teleportarea tag in build
            var go = GameObject.FindWithTag("TeleportArea");

            if (go != null)
            {
                Debug.Log("!!!!!!!!!!!!!" + go.name);
            }

            GameObject teleportArea = GameObject.Find("/_Level/Floor_teleport_zone");

            if (teleportArea != null)
            {
                teleportArea.layer = 5;

                Collider colliderTeleport = teleportArea.GetComponentInChildren<Collider>();

                if (colliderTeleport != null)
                {
                     InputAdapter.Instance.PlayerController.PlayerTeleported += position => 
                    {
                        _conditionsMoving = true;

                        TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Right,
                            ControllerTooltipManager.TooltipButtons.Touchpad);
                    };
                }

                else
                {
                    Debug.LogError("TeleportArea collider not found!");
                }
            }

            else
            {
                Debug.LogError("TeleportArea not found!");
            }
        }

        public override void OnExit()
        {
            UIMenu.Instance.OnboardingBlock = false;
            PopupWindowManager.ClosePopup();
        }
    }
}
