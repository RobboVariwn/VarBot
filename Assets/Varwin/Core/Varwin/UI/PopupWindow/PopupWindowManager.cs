using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Varwin.UI
{
    internal struct PopupParameters
    {
        public string MessageText;
        public bool EnableButton;
        public string ButtonText;
        public Sprite PopupImage;
        public Action OnClose;

        public PopupParameters(
            string messageText,
            bool enableButton,
            string buttonText,
            Sprite popupImage,
            Action onClose)
        {
            MessageText = messageText;
            EnableButton = enableButton;
            ButtonText = buttonText;
            PopupImage = popupImage;
            OnClose = onClose;
        }
    };

    public static class PopupWindowManager
    {
        private static List<PopupParameters> _popupQueue;
        private static PopupWindow _popupWindow;

        public static bool PopupIsOpened
        {
            get
            {
                if (_popupWindow != null)
                {
                    return _popupWindow.IsOpened;
                }

                return false;
            }
        }
        
        public static bool PopupButtonIsEnabled
        {
            get
            {
                if (_popupWindow != null)
                {
                    return _popupWindow.ButtonIsEnabled;
                }

                return false;
            }
        }

        public static void ShowPopup(
            string messageText,
            bool enableButton = false,
            string buttonText = "OK",
            Sprite popupImage = null,
            Action onClose = null)
        {
            if (_popupWindow == null)
            {
                GameObject popupObject = Object.Instantiate(Resources.Load("PopupWindow") as GameObject);
                _popupWindow = popupObject.GetComponent<PopupWindow>();
            }

            if (_popupQueue == null)
            {
                _popupQueue = new List<PopupParameters>();
            }

            Action onCloseOverride = onClose;

            onCloseOverride += UpdatePopupQueue;

            PopupParameters popupPars = new PopupParameters(messageText,
                enableButton,
                buttonText,
                popupImage,
                onCloseOverride);

            _popupQueue.Add(popupPars);

            if (_popupQueue.Count == 1)
            {
                _popupWindow.Show(popupPars.MessageText,
                    popupPars.EnableButton,
                    popupPars.ButtonText,
                    popupPars.PopupImage,
                    popupPars.OnClose);
            }
        }

        private static void UpdatePopupQueue()
        {
            if (_popupQueue.Count == 0)
            {
                return;
            }

            _popupQueue.Remove(_popupQueue.First());

            if (_popupQueue.Count == 0)
            {
                return;
            }

            PopupParameters popupPars = _popupQueue.First();

            _popupWindow.Show(popupPars.MessageText,
                popupPars.EnableButton,
                popupPars.ButtonText,
                popupPars.PopupImage,
                popupPars.OnClose);
        }

        public static void ClosePopup()
        {
            if (_popupWindow == null || _popupQueue.Count == 0)
            {
                return;
            }

            _popupWindow.Close();
        }
    }
}
