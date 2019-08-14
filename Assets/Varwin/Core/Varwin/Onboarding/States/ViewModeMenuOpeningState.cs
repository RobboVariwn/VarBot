using SmartLocalization;
using Varwin.UI;

namespace Varwin.Onboarding
{
    public class ViewModeMenuOpeningState : State
    {
        private bool _conditionsMenuOpening;

        public ViewModeMenuOpeningState(OnboardingStateMachine tutorialStateMachine) : base(tutorialStateMachine)
        {
        }

        public override void OnUpdate()
        {
            if (UIMenu.Instance.ModeButton.activeSelf)
            {
                StateMachine.ChangeState(new MoveToEditModeState(StateMachine));
            }
        }

        public override void OnEnter()
        {
            PopupHelper.Show(LanguageManager.Instance.GetTextValue("ONBOARDING_POPUP_PREVIEW_MENU_CHANGE_MODE"),
                null,
                StateMachine.ImageContainer.Eye);
            string openMenu = LanguageManager.Instance.GetTextValue("TUTORIAL_TOOLTIP_OPEN_MENU");

            TooltipManager.ShowControllerTooltip(openMenu,
                ControllerTooltipManager.TooltipControllers.Left,
                ControllerTooltipManager.TooltipButtons.ButtonTwo);
            StateMachine.UiMenu.OnMenuOpened += () => { _conditionsMenuOpening = true; };
        }

        public override void OnExit()
        {
            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Left,
                ControllerTooltipManager.TooltipButtons.ButtonTwo);
            PopupWindowManager.ClosePopup();
        }
    }
}