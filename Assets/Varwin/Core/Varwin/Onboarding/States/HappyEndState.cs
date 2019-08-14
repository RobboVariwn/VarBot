using SmartLocalization;
using UnityEngine;

namespace Varwin.Onboarding
{
    public class HappyEndState : State
    {
        private GameObject _fireWork;

        public HappyEndState(OnboardingStateMachine tutorialStateMachine) : base(tutorialStateMachine)
        {
        }

        public override void OnUpdate()
        {
        }

        public override void OnEnter()
        {
            string message = LanguageManager.Instance.GetTextValue("TUTORIAL_POPUP_HAPPY_END");

            PopupHelper.Show(message,
                () => { StateMachine.ChangeState(new OffState(StateMachine)); },
                StateMachine.ImageContainerCommon.Fin, true);
            _fireWork = Object.Instantiate(StateMachine.Firework, GameObjects.Instance.Head);
            _fireWork.transform.SetParent(null);
        }

        public override void OnExit()
        {
            Object.Destroy(_fireWork);
        }
    }
}
