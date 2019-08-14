using SmartLocalization;
using Varwin.UI;

namespace Varwin.Onboarding
{
    public class HelloState : State
    {
        private bool _conditionsHello;

        public HelloState(OnboardingStateMachine tutorialStateMachine) : base(tutorialStateMachine)
        {
        }

        public override void OnUpdate()
        {
            if (_conditionsHello)
            {
                StateMachine.ChangeState(new MoveState(StateMachine));
            }
        }

        public override void OnEnter()
        {
            UIMenu.Instance.OnboardingBlock = true;

            PopupHelper.Show(LanguageManager.Instance.GetTextValue("TUTORIAL_POPUP_HELLO"),
                () =>
                {
                    _conditionsHello = true;

                    TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Right,
                        ControllerTooltipManager.TooltipButtons.Trigger);
                },
                StateMachine.ImageContainerCommon.Hello,
                true,
                LanguageManager.Instance.GetTextValue("ONBOARDING_POPUP_START_BUTTON"));
            string pressOk = LanguageManager.Instance.GetTextValue("TUTORIAL_TOOLTIP_POINTER_CLICK");
            pressOk = string.Format(pressOk, LanguageManager.Instance.GetTextValue("ONBOARDING_POPUP_START_BUTTON"));

            TooltipManager.ShowControllerTooltip(pressOk,
                ControllerTooltipManager.TooltipControllers.Right,
                ControllerTooltipManager.TooltipButtons.Trigger);
        }

        public override void OnExit()
        {
            UIMenu.Instance.OnboardingBlock = false;
        }
    }
}