using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Varwin.UI
{
    public class DialogueWindow : FloatWindow
    {
        [SerializeField] private PopupWindowButton OkButton;
        [SerializeField] private PopupWindowButton CancelButton;
        [SerializeField] private GameObject ButtonsGroup;
        [SerializeField] private Text MessageText;
        [SerializeField] private Image MessageImage;
        [SerializeField] private Animator Animator;
        
        private bool IsAnimOpenProcess;
        private bool IsAnimCloseProcess;
        
        private readonly int OpenedKey = Animator.StringToHash("Opened");
        private readonly int HideKey = Animator.StringToHash("Hide");

        private Action OkButtonAction { get; set; }
        private Action CancelButtonAction { get; set; }

        public event Action Opened;

        public event Action Closed;
        
        public bool IsOpen { get; private set; }
        
        private void OnEnable()
        {
            OkButton.ButtonClickedEvent += OnOkButtonClicked;
            CancelButton.ButtonClickedEvent += OnCancelButtonClicked;
            ProjectData.GameModeChanged += WorldDataGameModeChanged;
        }

        private void OnDisable()
        {
            OkButton.ButtonClickedEvent -= OnOkButtonClicked;
            CancelButton.ButtonClickedEvent -= OnCancelButtonClicked;
            ProjectData.GameModeChanged -= WorldDataGameModeChanged;
            StopAllCoroutines();
            ResetWindow();
        }
        
        private void OnOkButtonClicked()
        {
            OkButtonAction?.Invoke();
            Close();
        }
        
        private void OnCancelButtonClicked()
        {
            CancelButtonAction?.Invoke();
            Close();
        }

        private void WorldDataGameModeChanged(GameMode mode)
        {
            ResetWindow();
        }

        private void ResetWindow()
        {
            IsOpen = false;
            OkButtonAction = null;
            CancelButtonAction = null;
            IsAnimOpenProcess = false;
            IsAnimCloseProcess = false;
            Animator.SetBool(OpenedKey, false);
            Animator.SetTrigger(HideKey);
        }
        
        // Callback for animation event
        private void OnAnimOpenFinish()
        {
            IsAnimOpenProcess = false;
        }

        // Callback for animation event
        private void OnAnimCloseFinish()
        {
            IsAnimCloseProcess = false;
        }
        
        public void Open(DialogueWindowSettings settings)
        {           
            if (gameObject.activeInHierarchy)
            {
                SetupMessageText(settings.MessageText);
                SetupMessageImage(settings.MessageSprite);
                
                SetupOkButton(settings.OkButtonText, settings.OkButtonAction);
                SetupCancelButton(settings.CancelButtonText, settings.CancelButtonAction);
                SetupButtonsGroup();
                
                StartCoroutine(DoOpen());
            }
        }

        public void Close()
        {
            if (gameObject.activeInHierarchy)
            {
                OkButtonAction = null;
                StartCoroutine(DoClose());
            }
        }

        private void SetupMessageText(string text)
        {
            MessageText.text = text;
        }
        
        private void SetupMessageImage(Sprite sprite)
        {
            if (sprite != null)
            {
                MessageImage.sprite = sprite;
                ResizeMessageImage();
                MessageImage.gameObject.SetActive(true);
            }
            else
            {
                MessageImage.gameObject.SetActive(false);
            }
        }
        
        private void ResizeMessageImage()
        {
            Sprite sprite = MessageImage.sprite;
            RectTransform rectTransform = MessageImage.rectTransform;

            Vector2 imageSize = rectTransform.sizeDelta;

            imageSize.y = imageSize.x * sprite.rect.height / sprite.rect.width;
            rectTransform.sizeDelta = imageSize;
        }

        private void SetupOkButton(string text, Action action)
        {
            OkButton.ButtonText = !string.IsNullOrWhiteSpace(text) ? text : "Ok";
            OkButtonAction = action;
            OkButton.gameObject.SetActive(action != null);
        }

        private void SetupCancelButton(string text, Action action)
        {
            CancelButton.ButtonText = !string.IsNullOrWhiteSpace(text) ? text : "Cancel";
            CancelButtonAction = action;
            CancelButton.gameObject.SetActive(action != null);
        }

        private void SetupButtonsGroup()
        {
            var activity = OkButton.gameObject.activeSelf || CancelButton.gameObject.activeSelf;
            ButtonsGroup.SetActive(activity);
        }
        
        private IEnumerator DoOpen()
        {
            SetWindowPosition();

            IsOpen = true;
            Opened?.Invoke();
            
            IsAnimOpenProcess = true;
            Animator.ResetTrigger(HideKey);
            Animator.SetBool(OpenedKey, true);     
            
            while (IsAnimOpenProcess)
            {
                UpdateWindowPosition();
                yield return null;
            }
            
            while (IsOpen)
            {
                UpdateWindowPosition();
                yield return null;
            }
        }

        private IEnumerator DoClose()
        {
            IsAnimCloseProcess = true;
            Animator.SetBool(OpenedKey, false);      
            
            while (IsAnimCloseProcess)
            {
                UpdateWindowPosition();
                yield return null;
            }

            IsOpen = false;
            Closed?.Invoke();
            
            Animator.SetTrigger(HideKey);
            ResetWindow();
        }
    }
}