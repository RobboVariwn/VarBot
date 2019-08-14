using System;
using UnityEngine;

namespace Varwin.UI
{
    public struct DialogueWindowSettings
    {
        public string MessageText;
        public Sprite MessageSprite;
        
        public string OkButtonText;
        public Action OkButtonAction;
        
        public string CancelButtonText;
        public Action CancelButtonAction;
    }
}