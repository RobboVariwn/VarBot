using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public static class VarwinStyles
    {
        public static Color LinkColor => Color.Lerp(Color.cyan, Color.blue, 0.5f);
        
        public static readonly GUIStyle LinkStyle = new GUIStyle(EditorStyles.miniBoldLabel)
        {
            normal =
            {
                textColor = LinkColor
            }
        };

        public static bool Link(string url)
        {
            return Link(url, url);
        }
        
        public static bool Link(string text, string url)
        {
            var pressed = GUILayout.Button(text, LinkStyle);
            if (pressed)
            {
                Application.OpenURL(url);
            }
            return pressed;
        }
    }
}
