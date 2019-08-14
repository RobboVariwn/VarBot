namespace Varwin.Onboarding
{
    public abstract class State
    {
        protected OnboardingStateMachine StateMachine;

        public State(OnboardingStateMachine tutorialStateMachine)
        {
            StateMachine = tutorialStateMachine;
        }

        public abstract void OnUpdate();
        
        public abstract void OnEnter();
        
        public abstract void OnExit();
        
    }
}