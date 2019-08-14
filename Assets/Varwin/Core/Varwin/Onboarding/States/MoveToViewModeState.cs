using System.Collections;
using SmartLocalization;
using UnityEngine;
using Varwin.UI;
using Varwin.WWW;

namespace Varwin.Onboarding
{
    public class MoveToViewModeState : State
    {
        private bool _conditionsSaving;
        private bool _conditionNotSaving;

        public MoveToViewModeState(OnboardingStateMachine tutorialStateMachine) : base(tutorialStateMachine)
        {
        }

        public override void OnUpdate()
        {
            if (_conditionsSaving)
            {
                StateMachine.ChangeState(new HappyEndState(StateMachine));
            }

            if (_conditionNotSaving)
            {
                StateMachine.ChangeState(new MenuOpeningState(StateMachine, MenuOpeningState.TargetObject.ViewMode));
            }
        }

        public override void OnEnter()
        {
            StateMachine.UiMenu.HighlightModeButton();
            string pressOk = LanguageManager.Instance.GetTextValue("TUTORIAL_TOOLTIP_POINTER_CLICK");
            pressOk = string.Format(pressOk, LanguageManager.Instance.GetTextValue("MODE"));

            TooltipManager.ShowControllerTooltip(pressOk,
                ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);


            TooltipManager.SetObjectTooltip(UIMenu.Instance.ModeButtonOffset,
                LanguageManager.Instance.GetTextValue("ONBOARDING_POPUP_MENU_CHANGE_MODE"),
                ObjectTooltip.ObjectTooltipSize.Small,
                0.1f);

            StateMachine.UiMenu.OnMenuClosed += UiMenuOnOnMenuClosedWhenSaving;

            ProjectData.GameModeChanged += GmChanged;
        }

        private void GmChanged(GameMode gm)
        {
            if (gm == GameMode.Preview)
            {
                _conditionsSaving = true;
            }
        }

        public override void OnExit()
        {
            ProjectData.GameModeChanged -= GmChanged;

            StateMachine.UiMenu.ResetMenuHighlight();

            TooltipManager.RemoveObjectTooltip(UIMenu.Instance.ModeButtonOffset);

            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);
        }

        private void UiMenuOnOnMenuClosedWhenSaving()
        {
            if (_conditionsSaving)
            {
                return;
            }

            _conditionNotSaving = true;

            StateMachine.UiMenu.OnMenuClosed -= UiMenuOnOnMenuClosedWhenSaving;
        }
    }
}
