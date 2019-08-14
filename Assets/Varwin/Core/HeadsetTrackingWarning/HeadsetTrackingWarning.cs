using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using TMPro;

namespace Varwin.UI
{
    public class HeadsetTrackingWarning : MonoBehaviour
    {
        [SerializeField] private GameObject _widgetRoot;
        [SerializeField] private TMP_Text _text;

        private const float ShowDelay = 3f;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SetWidgetActivity(false);
        }

        private void OnEnable()
        {
            InputTracking.trackingLost += InputTrackingOnTrackingLost;
            InputTracking.trackingAcquired += InputTrackingOnTrackingAcquired;
            
            ProjectData.GameModeChanged += WorldDataGameModeChanged;
            ProjectData.PlatformModeChanged += WorldDataPlatformModeChanged;
            
            CheckWidgetActivity();
        }
        
        private void OnDisable()
        {
            InputTracking.trackingLost -= InputTrackingOnTrackingLost;
            InputTracking.trackingAcquired -= InputTrackingOnTrackingAcquired;
            
            ProjectData.GameModeChanged -= WorldDataGameModeChanged;
            ProjectData.PlatformModeChanged -= WorldDataPlatformModeChanged;
        }

        private void Start()
        {
            SceneManager.sceneLoaded += (s, m) => StartCoroutine(CheckWithDelay());
            SceneManager.sceneUnloaded += (s) => StartCoroutine(CheckWithDelay());
        }

        private void InputTrackingOnTrackingLost(XRNodeState obj)
        {
            CheckWidgetActivity();
        }
        
        private void InputTrackingOnTrackingAcquired(XRNodeState obj)
        {
            CheckWidgetActivity();
        }
        
        private void WorldDataGameModeChanged(GameMode gameMode)
        {
            CheckWidgetActivity();
        }
        
        private void WorldDataPlatformModeChanged(PlatformMode platformMode)
        {
            CheckWidgetActivity();
        }

        private void CheckWidgetActivity()
        {
            if (ProjectData.PlatformMode == PlatformMode.Desktop)
            {
                SetWidgetActivity(false);
                return;
            }
            
            var list = new List<XRNodeState>();
            InputTracking.GetNodeStates(list);

            if (list.Count == 0)
            {
                SetWidgetActivity(false);
                return;
            }
            
            var node = list.FirstOrDefault(x => x.nodeType == XRNode.Head);            
            SetWidgetActivity(!node.tracked);
        }
        
        private IEnumerator CheckWithDelay()
        {
            yield return new WaitForSeconds(ShowDelay);
            CheckWidgetActivity();
        }

        private void SetWidgetActivity(bool active)
        {
            _widgetRoot.SetActive(active);
            _text.text = SmartLocalization.LanguageManager.Instance.GetTextValue("HEADSET_TRACKING_LOST");
        }
    }
}
