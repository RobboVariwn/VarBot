using System.Collections;
using UnityEngine;

namespace Varwin.UI
{
    public class HeadsetNotFoundWarning : MonoBehaviour
    {
        [SerializeField] private GameObject WarningRoot;
        [SerializeField] private float OpenDelay = 3f;

        private void OnEnable()
        {
            WarningRoot.SetActive(false);
            StartCoroutine(DoShowWinDelay());
        }
        
        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator DoShowWinDelay()
        {
            yield return new WaitForSecondsRealtime(OpenDelay);
            WarningRoot.SetActive(true);
        }
    }
}