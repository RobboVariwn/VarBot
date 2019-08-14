using SmartLocalization;
using Varwin.UI;
using Varwin.VRInput;

namespace Varwin.Onboarding
{
    public class ButtonUseState : State
    {
        public ButtonUseState(OnboardingStateMachine tutorialStateMachine) : base(tutorialStateMachine)
        {
        }

        public override void OnUpdate()
        {
            
            dynamic buttonWrapper = GameStateData.GetWrapperCollection().Get(StateMachine.ButtonInstanceId);
#if !NET_STANDARD_2_0
            if (buttonWrapper.ButtonPressedChecker())
            {
                StateMachine.ChangeState(new ViewModeMenuOpeningState(StateMachine));
            }
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public override void OnEnter()
        {
            UIMenu.Instance.OnboardingBlock = true;
            
            string message = LanguageManager.Instance.GetTextValue("TUTORIAL_POPUP_LOGIC_START");

            PopupHelper.Show(message,
                () =>
                {
                    InputAdapter.Instance.PointerController.IsMenuOpened = false;
                },
                StateMachine.ImageContainer.Use,
                true);
            string trigger = LanguageManager.Instance.GetTextValue("TUTORIAL_TOOL_TIP_BUTTON_USE");

            TooltipManager.ShowControllerTooltip(trigger,
                ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);

            string button = LanguageManager.Instance.GetTextValue("TUTORIAL_TOOL_TIP_PRESS_ME");
            TooltipManager.SetObjectTooltip(StateMachine.ButtonGameObject, button);
            
            
        }

        public override void OnExit()
        {
            UIMenu.Instance.OnboardingBlock = false;
            PopupWindowManager.ClosePopup();
            TooltipManager.RemoveObjectTooltip(StateMachine.ButtonGameObject);

            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);
        }
    }
}
