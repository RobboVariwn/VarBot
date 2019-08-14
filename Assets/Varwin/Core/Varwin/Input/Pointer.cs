using System.Collections;
using UnityEngine;
using Varwin.UI;
using Varwin.UI.VRErrorManager;
using Varwin.UI.VRMessageManager;
#if UNITY_STANDALONE_WIN && !VRMAKER
using ZenFulcrum.EmbeddedBrowser;
#endif
using UnityEngine.XR;
using Varwin.VRInput;

namespace Varwin.OLD.VRTKControls
{
    public class Pointer : MonoBehaviour
    {
        private UIMenu _uiMenu;
        private ControllerInput.ControllerEvents _controllerEvents;
        private GameObject _toolTip;

        private static bool _menuOpened;
        private static bool _teleportMenuCloseMode;

        public PointerController PointerController => InputAdapter.Instance.PointerController;

        private bool _pointerWasInitialized;

        [SerializeField]
        private bool isRightHandPointer;

        private bool _rightHandTracked;
        private bool _popupButtonPointerResetDone;
        private float _resetTimer = 1.0f;
        private int _resetCounter;

        private void Start()
        {
            _uiMenu = FindObjectOfType<UIMenu>();
            _controllerEvents = InputAdapter.Instance.ControllerInput.ControllerEventFactory.GetFrom(gameObject);
            _controllerEvents.TriggerPressed += DoTriggerRightPressed;
            _controllerEvents.TouchpadReleased += TouchpadReleased;
            _controllerEvents.TouchpadPressed += TouchPadPressed;
            _controllerEvents.ButtonTwoPressed += StartMenuPressed;
            _controllerEvents.ButtonTwoReleased += StartMenuLeftReleased;
            _controllerEvents.GripPressed += ControllerEventsOnGripPressed;
            _toolTip = Instantiate(GameObjects.Instance.UIToolTip);
            _toolTip.SetActive(false);

            StartCoroutine(SubscribeForMenuEventsCoroutine());

            ProjectData.GameModeChanged += ManagePointerRayGameModeChanged;

            InputTracking.trackingLost += OnTrackingChange;
            InputTracking.trackingAcquired += OnTrackingChange;
        }

        private void Update()
        {
            /*
                        if (!_pointerWasInitialized)
                        {
                            ResetRightHandPointer();
                            _pointerWasInitialized = true;
                        }*/

            bool rightHandIsValid = InputDevices.GetDeviceAtXRNode(XRNode.RightHand).IsValid;
            bool leftHandIsValid = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).IsValid;

            if (!leftHandIsValid || !rightHandIsValid)
            {
                _pointerWasInitialized = false;
            }

            if (isRightHandPointer)
            {
                if (PopupWindowManager.PopupButtonIsEnabled
                    && !_menuOpened
                    && _rightHandTracked
                    && !_popupButtonPointerResetDone)
                {
                    _resetTimer -= Time.unscaledDeltaTime;

                    if (_resetTimer <= 0)
                    {
                        ResetRightHandPointer();

                        _resetTimer = 1.0f;
                        _resetCounter++;
                    }

                    if (_resetCounter >= 3)
                    {
                        _popupButtonPointerResetDone = true;
                    }
                }

                return;
            }

            if (_uiMenu == null)
            {
                _uiMenu = FindObjectOfType<UIMenu>();
            }

            if (_menuOpened || PopupWindowManager.PopupButtonIsEnabled)
            {
                PointerController.IsMenuOpened = true;
            }
        }

        private IEnumerator SubscribeForMenuEventsCoroutine()
        {
            while (_uiMenu == null)
            {
                _uiMenu = FindObjectOfType<UIMenu>();

                yield return new WaitForEndOfFrame();
            }

            _uiMenu.OnMenuClosed += DisableUiOnMenuClose;
        }

        private void DisableUiOnMenuClose()
        {
            if (!isRightHandPointer)
            {
                return;
            }

            _menuOpened = false;

            //TODO: fix the custom raycast menu mode switch when a new dialogue window manager will be created 
            bool dialogueWindowIsOpen = VRMessageManager.Instance.Panel.activeInHierarchy
                                        || VRErrorManager.Instance.Panel.activeInHierarchy;
            Helper.HideUi(dialogueWindowIsOpen);

            if (_teleportMenuCloseMode)
            {
                _teleportMenuCloseMode = false;
            }
            else if (!dialogueWindowIsOpen)
            {
                PointerController.IsMenuOpened = false;
            }
        }

        private void ManagePointerRayGameModeChanged(GameMode newGameMode)
        {
            ResetRightHandPointer();

            if (newGameMode == GameMode.Edit)
            {
                if (_menuOpened)
                {
                    PointerController.IsMenuOpened = true;
                }
            }
            else
            {
                HideMenu();
            }
        }

        private void ControllerEventsOnGripPressed(object sender, ControllerInput.ControllerInteractionEventArgs e)
        {
            if (e.controllerReference.hand == ControllerInteraction.ControllerHand.Left)
            {
                return;
            }

            Helper.ResetSpawnObject();
        }


        private void StartMenuLeftReleased(object sender, ControllerInput.ControllerInteractionEventArgs e)
        {
        }

        private void HideMenu()
        {
            if (_uiMenu == null)
            {
                _uiMenu = FindObjectOfType<UIMenu>();
            }

            if (_uiMenu != null)
            {
                _uiMenu.HideMenu();
            }
        }

        private void StartMenuPressed(object sender, ControllerInput.ControllerInteractionEventArgs e)
        {
            if (e.controllerReference.hand == ControllerInteraction.ControllerHand.Right
                || isRightHandPointer)
            {
                return;
            }

            if (!_pointerWasInitialized)
            {
                ResetRightHandPointer();
                _pointerWasInitialized = true;
            }


            if (ProjectData.GameMode == GameMode.View)
            {
                return;
            }

            if (_menuOpened)
            {
                HideMenu();
                PointerController.IsMenuOpened = false;
            }
            else
            {
                _teleportMenuCloseMode = false;
                EnableUi();
                PointerController.IsMenuOpened = true;
            }
        }

        private void EnableUi()
        {
            _menuOpened = true;

            if (_uiMenu == null)
            {
                _uiMenu = FindObjectOfType<UIMenu>();
            }

            _uiMenu.ShowMenu();

            Helper.ShowUi();
        }

        private void ResetRightHandPointer()
        {
            if (PointerController != null)
            {
                //RightHandInputPointer.enabled = false;
                //RightHandInputPointer.enabled = true;
                //PointerController.IsMenuOpened = false;
            }
        }

        private void DoTriggerRightPressed(object sender, ControllerInput.ControllerInteractionEventArgs e)
        {
            if (e.controllerReference.hand == ControllerInteraction.ControllerHand.Left)
            {
                return;
            }

            StartCoroutine(Click());

            if (Helper.CanObjectBeSpawned())
            {
                Helper.SpawnObject(ProjectData.SelectedObjectIdToSpawn);
                Helper.ResetSpawnObject();
            }
        }

        private void TouchpadReleased(object sender, ControllerInput.ControllerInteractionEventArgs e)
        {
            TouchPadReleased();
        }

        private void TouchPadPressed(object sender, ControllerInput.ControllerInteractionEventArgs e)
        {
            if (!_menuOpened || !isRightHandPointer)
            {
                return;
            }

            _teleportMenuCloseMode = true;
            HideMenu();
        }

        private void TouchPadReleased()
        {
        }


        IEnumerator Click()
        {
#if UNITY_STANDALONE_WIN && !VRMAKER
            PointerUIBase.MouseButton = 0;

            yield return new WaitForEndOfFrame();
            PointerUIBase.MouseButton = MouseButton.Left;

            yield return new WaitForEndOfFrame();
            PointerUIBase.MouseButton = 0;
#endif

            yield return true;
        }

        private void OnTrackingChange(XRNodeState obj)
        {
            if (obj.nodeType == XRNode.RightHand)
            {
                _rightHandTracked = obj.tracked;
                ResetRightHandPointer();
            }
        }
    }
}
