using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace Varwin.UI
{
    public class NotificationWindow : FloatWindow
    {
        [SerializeField] private Text _messageText;
        [SerializeField] private Animator _animator;

        private bool IsAnimOpenProcess;
        private bool IsAnimCloseProcess;
        
        private readonly int OpenedKey = Animator.StringToHash("Opened");
        private readonly int HideKey = Animator.StringToHash("Hide");

        private void OnEnable()
        {
            ProjectData.GameModeChanged += WorldDataGameModeChanged;
        }
        
        private void OnDisable()
        {
            ProjectData.GameModeChanged -= WorldDataGameModeChanged;
            StopAllCoroutines();
            ResetWindow();
        } 

        private void WorldDataGameModeChanged(GameMode mode)
        {
            ResetWindow();
        }

        private void ResetWindow()
        {
            IsAnimOpenProcess = false;
            IsAnimCloseProcess = false;
            _animator.SetBool(OpenedKey, false);
            _animator.SetTrigger(HideKey);
        }
     
        private void OnAnimOpenFinish()
        {
            IsAnimOpenProcess = false;
        }

        private void OnAnimCloseFinish()
        {
            IsAnimCloseProcess = false;
        }

        public void Show(string message, float duration)
        {           
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(DoShow(message, duration));
            }
        }
        
        private IEnumerator DoShow(string message, float duration)
        {
            _messageText.text = message;
            SetWindowPosition();
            
            IsAnimOpenProcess = true;
            _animator.ResetTrigger(HideKey);
            _animator.SetBool(OpenedKey, true);     
            while (IsAnimOpenProcess)
            {
                UpdateWindowPosition();
                yield return null;
            }

            while (duration > 0)
            {
                UpdateWindowPosition();
                duration -= Time.deltaTime;
                yield return null;
            }
            
            IsAnimCloseProcess = true;
            _animator.SetBool(OpenedKey, false);      
            while (IsAnimCloseProcess)
            {
                UpdateWindowPosition();
                yield return null;
            }
            
            _animator.SetTrigger(HideKey);
        }
    }
}