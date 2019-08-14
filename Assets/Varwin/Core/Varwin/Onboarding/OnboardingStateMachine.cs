using System.Collections;
using UnityEngine;
using Varwin.UI;
using Varwin.WWW;

namespace Varwin.Onboarding
{
    public class OnboardingStateMachine
    {
        private State _currentState;
        public TutorialImageContainer ImageContainer;
        public TutorialImageContainerCommon ImageContainerCommon;
        public UIMenu UiMenu;
        public int SpawningObjectId = 0;
        
        public GameObject ButtonGameObject;
        public GameObject LampGameObject;
        public GameObject DisplayGameObject;
        public GameObject Firework;

        private OnboardingManager _stateMachine;
            
        public const string ButtonGuid = "3bff7109-e41d-432c-a5b3-b8e4f953d9ab";
        public const string DisplayGuid = "e7af0b91-306a-4e40-910a-1b2252d54f95";
        public const string LampGuid = "db12326f-1dc5-45eb-b78d-6d5e29e2518b";

        public int ButtonInstanceId = 0;

        private bool _isChangingState = false;
        
        public OnboardingStateMachine(OnboardingManager onboardingManager)
        {
            _stateMachine = onboardingManager;
            _stateMachine.StartCoroutine(WaitingUiMenu());
        }
        
        public void OnUpdate()
        {
            _currentState?.OnUpdate();
        }

        public void ChangeState(State state)
        {
            if (_isChangingState)
            {
                return;
            }

            _isChangingState = true;
            RequestManager.Instance.StartCoroutine(WaitForFrame(state));
        }

        private IEnumerator WaitForFrame(State state)
        {
            if (_currentState != null)
            {
                _currentState.OnExit();
            }

            yield return new WaitForSeconds(0.1f);
            _currentState = state;
            _currentState.OnEnter();
            _isChangingState = false;
        }

        private IEnumerator WaitingUiMenu()
        {
            while (UiMenu == null)
            {
                UiMenu = Object.FindObjectOfType<UIMenu>();
                yield return new WaitForEndOfFrame();
            }

            yield return true;
        }
    }
}
