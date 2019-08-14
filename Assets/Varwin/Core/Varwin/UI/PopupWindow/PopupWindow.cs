using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Varwin;

namespace Varwin.UI
{
    [RequireComponent(typeof(Animator))]
    public class PopupWindow : FloatWindow
    {
        [SerializeField]
        private PopupWindowButton _button;

        [SerializeField]
        private Text _messageText;

        [SerializeField]
        private RectTransform _background;

        [SerializeField]
        private Image _image;

        [SerializeField]
        private float _maxTextHeight = 125.0f;

        [SerializeField]
        private Animator _animator;

        public delegate void PopupCloserDelegate();

        public bool IsOpened;
        public bool ButtonIsEnabled;
        
        private Action _onCloseAction;

        private void Reset()
        {
            _button = GetComponentInChildren<PopupWindowButton>(true);
            _messageText = GetComponentInChildren<Text>(true);
            _background = GetComponentInChildren<Image>(true).rectTransform;

            _image = transform.Find("Canvas").Find("Background").Find("Image").GetComponent<Image>();

            _animator = GetComponent<Animator>();
        }

        private void Awake()
        {
            _button.ButtonClickedEvent += Close;
            IsOpened = true;
            ButtonIsEnabled = false;
        }

        private void Update()
        {
            UpdateWindowPosition();
        }

        private void SetImage(Sprite sprite)
        {
            if (sprite != null)
            {
                _image.sprite = sprite;
                ResizeImage();
                _image.gameObject.SetActive(true);
            }
            else
            {
                _image.gameObject.SetActive(false);
            }
        }

        private void ResizeImage()
        {
            Sprite sprite = _image.sprite;
            RectTransform rectTransform = _image.rectTransform;

            Vector2 imageSize = rectTransform.sizeDelta;

            imageSize.y = imageSize.x * sprite.rect.height / sprite.rect.width;
            rectTransform.sizeDelta = imageSize;
        }

        private void ResizeTextField()
        {
            _messageText.verticalOverflow = VerticalWrapMode.Overflow;

            float textHeight = _messageText.preferredHeight;

            if (textHeight > _maxTextHeight)
            {
                textHeight = _maxTextHeight;

                _messageText.resizeTextMinSize = 1;
                _messageText.resizeTextMaxSize = 10;
                _messageText.resizeTextForBestFit = true;
            }
            else
            {
                _messageText.resizeTextForBestFit = false;
            }

            RectTransform textRectTransform = _messageText.rectTransform;

            textRectTransform.sizeDelta = new Vector2(textRectTransform.sizeDelta.x, textHeight);

            _messageText.verticalOverflow =
                (textHeight == _maxTextHeight) ? VerticalWrapMode.Truncate : VerticalWrapMode.Overflow;
        }

        public void Close()
        {
            _animator.SetBool("Opened", false);
            DoOnClose();
        }

        private void DoOnClose()
        {
            IsOpened = false;
            ButtonIsEnabled = false;
            _onCloseAction?.Invoke();
        }

        public void Show(
            string messageText,
            bool enableButton,
            string buttonText,
            Sprite image,
            Action onClose)
        {
            StartCoroutine(ClosePreviousPopupAndOpenNewCoroutine(messageText,
                enableButton,
                buttonText,
                image,
                onClose));
        }

        private IEnumerator ClosePreviousPopupAndOpenNewCoroutine(
            string messageText,
            bool enableButton,
            string buttonText,
            Sprite image,
            Action onClose)
        {
            if (IsOpened)
            {
                DoOnClose();
                _animator.SetBool("Opened", false);
            }

            while (IsOpened)
            {
                yield return new WaitForEndOfFrame();
            }

            OpenPopup(messageText,
                enableButton,
                buttonText,
                image,
                onClose);
        }

        private void OpenPopup(
            string messageText,
            bool enableButton,
            string buttonText,
            Sprite image,
            Action onClose)
        {
            _messageText.text = messageText;
            _button.ButtonText = buttonText;
            ResizeTextField();
            
            SetImage(image);

            ButtonIsEnabled = enableButton;
            _button.gameObject.SetActive(enableButton);
            
            SetWindowPosition();

            _onCloseAction = onClose;

            _animator.SetBool("Opened", true);

            
            
            IsOpened = true;
        }
    }
}
