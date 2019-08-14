using SmartLocalization;
using Varwin.UI;

namespace Varwin.Onboarding
{
    public class BackToRmsState : State
    {
        public BackToRmsState(OnboardingStateMachine tutorialStateMachine) : base(tutorialStateMachine)
        {
        }

        public override void OnUpdate()
        {
            if (ProjectData.GameMode == GameMode.Preview || ProjectData.GameMode == GameMode.View)
            {
                StateMachine.ChangeState(new ButtonUseState(StateMachine));
            }
        }

        public override void OnEnter()
        {
            UIMenu.Instance.HideMenu();
            UIMenu.Instance.OnboardingBlock = true;
            string message = LanguageManager.Instance.GetTextValue("TUTORIAL_POPUP_BACK_TO_RMS");

            PopupHelper.Show(message,
                () => {},
                StateMachine.ImageContainerCommon.Rms);
        }

        public override void OnExit()
        {
            UIMenu.Instance.OnboardingBlock = false;
            PopupWindowManager.ClosePopup();
        }
    }
}
