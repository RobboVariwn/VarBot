using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

namespace Varwin.UI
{
    public static class DialogueWindowManager
    {
        private static DialogueWindow _window;
        
        private const string PrefabPath = "DialogueWindow/DialogueWindow";

        public static bool IsWindowOpen => Window.IsOpen;

        public static DialogueWindow Window
        {
            get
            {
                if (_window == null)
                {
                    var prefab = Resources.Load<GameObject>(PrefabPath);
                    var go = UnityEngine.Object.Instantiate(prefab);
                    _window = go.GetComponent<DialogueWindow>();
                }

                return _window;
            }
        }

        public static void OpenWindow(string messageText, Sprite messageSprite, string okButtonText, Action okButtonAction)
        {
            var settings = new DialogueWindowSettings()
            {
                MessageText = messageText,
                MessageSprite = messageSprite,
                OkButtonText = okButtonText,
                OkButtonAction = okButtonAction,
                CancelButtonText = string.Empty,
                CancelButtonAction = null
            };
            
            OpenWindow(settings);
        }
        
        public static void OpenWindow(DialogueWindowSettings settings)
        {
            Window.Open(settings);
        }

        public static void CloseWindow()
        {
            Window.Close();
        }
    }
}