using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using Varwin.Data;

namespace Varwin.Onboarding
{
    public class OnboardingManager : MonoBehaviour
    {
        public static OnboardingManager Instance;
        
        private OnboardingStateMachine _stateMachine;
        private GameMode _gm;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _stateMachine = new OnboardingStateMachine(this);
            StartCoroutine(WaitingXrAndStart());
            TurnOnBoxes();
            
            if (_gm == GameMode.Edit)
            {
                return;
            }

            StartCoroutine(FindObjectInstancesAndContinue());
        }

        private void TurnOnBoxes()
        {
            GameObject blocks = GameObject.Find("/_Level/BlockColliders");

            foreach (Transform child in blocks.transform)
            {
                child.gameObject.SetActive(true);
                Collider collider = child.gameObject.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }

            GameObject teleportArea = GameObject.Find("/_Level/Floor_teleport_zone");
            teleportArea.transform.localScale = new Vector3(1.138234f, 1, 1.150721f);
            teleportArea.transform.position = new Vector3(-20.096f, 0.01f, -25.16f);
        }

        private IEnumerator WaitingXrAndStart()
        {
            while (string.IsNullOrEmpty(XRDevice.model))
            {
                yield return new WaitForEndOfFrame();
            }

            GameObject tutorialImages = null;

            if (XRDevice.model.Contains("Vive"))
            {
                tutorialImages = Resources.Load<GameObject>("Tutorial/TutorialVive");
            }

            else if (XRDevice.model.Contains("Oculus"))
            {
                tutorialImages = Resources.Load<GameObject>("Tutorial/TutorialOculus");
            }

            else
            {
                tutorialImages = Resources.Load<GameObject>("Tutorial/TutorialWMR");
            }

            GameObject common = Resources.Load<GameObject>("Tutorial/TutorialCommon");
            TutorialImageContainerCommon imageContainerCommon = common.GetComponent<TutorialImageContainerCommon>();

            GameObject firework = Resources.Load<GameObject>("Tutorial/Firework");

            _stateMachine.Firework = firework;
            _stateMachine.ImageContainer = tutorialImages.GetComponent<TutorialImageContainer>();
            _stateMachine.ImageContainerCommon = imageContainerCommon;

            if (ProjectData.GameMode == GameMode.Edit)
            {
                _stateMachine.ChangeState(new HelloState(_stateMachine));
            }
        }

        private IEnumerator FindObjectInstancesAndContinue()
        {
            var prefabList = GameStateData.GetPrefabsData();
            int buttonId = 0;

            foreach (PrefabObject selectedObject in prefabList)
            {
                if (selectedObject.Guid == OnboardingStateMachine.ButtonGuid)
                {
                    buttonId = selectedObject.Id;
                }
            }

            ;

            var group = Contexts.sharedInstance.game.GetGroup(GameMatcher.AllOf(GameMatcher.Wrapper,
                GameMatcher.IdObject));

            while (group.GetEntities().Length == 0)
            {
                yield return new WaitForEndOfFrame();
            }

            foreach (var gameEntity in group.GetEntities())
            {
                if (gameEntity.idObject.Value != buttonId)
                {
                    continue;
                }

                _stateMachine.ButtonGameObject = gameEntity.gameObject.Value;
                _stateMachine.ButtonInstanceId = gameEntity.id.Value;
                break;
            }

            _stateMachine.ChangeState(new ButtonUseState(_stateMachine));
        }

        private void Update()
        {
            _stateMachine?.OnUpdate();
        }


        public void Init(GameMode gameMode)
        {
            _gm = gameMode;
        }
    }
}
