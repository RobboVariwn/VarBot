using UnityEngine;

namespace Varwin.UI
{
    public static class NotificationWindowManager
    {
        private const string PrefabPath = "NotificationWindow/NotificationWindow";
        
        private static NotificationWindow _window;

        private static NotificationWindow Window
        {
            get
            {
                if (_window == null)
                {
                    var prefab = Resources.Load(PrefabPath);
                    var go = (GameObject)Object.Instantiate(prefab);
                    _window = go.GetComponent<NotificationWindow>();
                }
                
                return _window;
            }
        }
        
        public static void Show(string message, float duration)
        {
            Window.Show(message, duration);
        }
    }
}