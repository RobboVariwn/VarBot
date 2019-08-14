using SmartLocalization;
using Varwin.UI;

namespace Varwin.Onboarding
{
    public class SavingState : State
    {
        private bool _conditionsSaving;
        private bool _conditionNotSaving;

        public SavingState(OnboardingStateMachine tutorialStateMachine) : base(tutorialStateMachine)
        {
        }

        public override void OnUpdate()
        {
            if (_conditionsSaving)
            {
                StateMachine.ChangeState(new BackToRmsState(StateMachine));
            }

            if (_conditionNotSaving)
            {
                StateMachine.ChangeState(new MenuOpeningState(StateMachine, MenuOpeningState.TargetObject.Save));
            }
        }

        public override void OnEnter()
        {
            StateMachine.UiMenu.HighlightSaveButton();
            PopupWindowManager.ClosePopup();
            string pressOk = LanguageManager.Instance.GetTextValue("TUTORIAL_TOOLTIP_POINTER_CLICK");
            pressOk = string.Format(pressOk, LanguageManager.Instance.GetTextValue("SAVE"));

            TooltipManager.ShowControllerTooltip(pressOk,
                ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);
            StateMachine.UiMenu.OnMenuClosed += UiMenuOnOnMenuClosedWhenSaving;
            ProjectData.OnSave += () => { _conditionsSaving = true; };
        }

        public override void OnExit()
        {
            StateMachine.UiMenu.ResetMenuHighlight();

            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);

            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Left,
                ControllerTooltipManager.TooltipButtons.ButtonTwo);
        }

        private void UiMenuOnOnMenuClosedWhenSaving()
        {
            if (_conditionsSaving)
            {
                return;
            }

            _conditionNotSaving = true;

            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);
            StateMachine.UiMenu.OnMenuClosed -= UiMenuOnOnMenuClosedWhenSaving;
        }
    }
}
