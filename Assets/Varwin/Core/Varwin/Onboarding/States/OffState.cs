using UnityEngine;

namespace Varwin.Onboarding
{
    public class OffState : State
    {
        public OffState(OnboardingStateMachine tutorialStateMachine) : base(tutorialStateMachine)
        {
        }

        public override void OnUpdate()
        {
             
        }

        public override void OnEnter()
        {
            Settings.Instance().OnboardingMode = false;
            TurnOffBoxes();
        }

        public override void OnExit()
        {
             
        }
        
        private void TurnOffBoxes()
        {
            GameObject blocks = GameObject.Find("/_Level/BlockColliders");
            foreach (Transform child in blocks.transform)
            {
                child.gameObject.SetActive(false);
            }
            
            GameObject teleportArea = GameObject.Find("/_Level/Floor_teleport_zone");
            teleportArea.transform.localScale = new Vector3(3.3132f, 1, 5.355653f);
            teleportArea.transform.position = new Vector3(-20.096f, 0.01f, -29.921f);
        }
    }
}
