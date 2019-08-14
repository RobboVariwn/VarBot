using System;
using UnityEngine;
using Varwin.UI;

namespace Varwin.Onboarding
{
    public static class PopupHelper
    {
        public static void Show(string text, Action ok = null, Sprite image = null, bool buttonIsEnabled = false, string buttonText = "OK")
        {
            PopupWindowManager.ShowPopup(text, buttonIsEnabled, buttonText, image, ok);
        }
    }
}