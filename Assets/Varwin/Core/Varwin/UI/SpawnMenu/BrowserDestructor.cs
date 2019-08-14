using System.Collections;
using UnityEngine;
#if UNITY_STANDALONE_WIN && !VRMAKER
using ZenFulcrum.EmbeddedBrowser;
#endif

namespace Varwin
{
    public class BrowserDestructor : MonoBehaviour
    {
        public static BrowserDestructor Instance;
#if UNITY_STANDALONE_WIN && !VRMAKER
        private VRMainControlPanel _vrMainControlPanel;
#endif

        private void Awake()
        {
            Instance = this;
        }

#if UNITY_STANDALONE_WIN && !VRMAKER
        public void Init(VRMainControlPanel panel)
        {
            _vrMainControlPanel = panel;
        }

        public void DestroyPad(VRBrowserPanel pane)
        {
            StartCoroutine(DestroyBrowser(pane));
        }

        private IEnumerator DestroyBrowser(VRBrowserPanel pane)
        {
            if (_vrMainControlPanel == null)
            {
                yield break;
            }
            
            //drop the pane and destroy it
            _vrMainControlPanel.allBrowsers.Remove(pane);
            if (!pane) yield break;

            var t0 = Time.time;
            while (Time.time < t0 + 3)
            {
                if (!pane) yield break;
                _vrMainControlPanel.MoveToward(pane.transform, Vector3.zero);
                yield return null;
            }

            Destroy(pane.gameObject);
        }
#endif
    }
}
