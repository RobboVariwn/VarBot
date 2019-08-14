using SmartLocalization;
using Varwin.UI;

namespace Varwin.Onboarding
{
    public class MoveToEditModeState : State
    {
        public MoveToEditModeState(OnboardingStateMachine tutorialStateMachine) : base(tutorialStateMachine)
        {
        }

        private bool _conditionsChangeGM;
        private bool _conditionNotChangedGM;

        public override void OnUpdate()
        {
            if (_conditionsChangeGM)
            {
                StateMachine.ChangeState(new MoveToViewModeState(StateMachine));
            }

            if (_conditionNotChangedGM)
            {
                StateMachine.ChangeState(new ViewModeMenuOpeningState(StateMachine));
            }
        }

        public override void OnEnter()
        {
            string pressOk = LanguageManager.Instance.GetTextValue("ONBOARDING_TOOLTIP_POINTER_CLICK_EYE");

            TooltipManager.ShowControllerTooltip(pressOk,
                ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);

            TooltipManager.SetObjectTooltip(UIMenu.Instance.ModeButton,
                LanguageManager.Instance.GetTextValue("ONBOARDING_TOOLTIP_PRESS_EYE_ICON"),
                ObjectTooltip.ObjectTooltipSize.Small,
                0.1f);

            StateMachine.UiMenu.OnMenuClosed += UiMenuOnOnMenuClosedWhenSaving;

            ProjectData.GameModeChanged += GmChanged;
        }

        private void GmChanged(GameMode gm)
        {
            if (gm == GameMode.Edit)
            {
                _conditionsChangeGM = true;
            }
        }

        public override void OnExit()
        {
            ProjectData.GameModeChanged -= GmChanged;

            TooltipManager.RemoveObjectTooltip(UIMenu.Instance.ModeButton);

            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);
        }

        private void UiMenuOnOnMenuClosedWhenSaving()
        {
            if (_conditionsChangeGM)
            {
                return;
            }

            _conditionNotChangedGM = true;
            StateMachine.UiMenu.OnMenuClosed -= UiMenuOnOnMenuClosedWhenSaving;
        }
    }
}
