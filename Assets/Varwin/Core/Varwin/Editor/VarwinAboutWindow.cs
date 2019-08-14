using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class VarwinAboutWindow : EditorWindow
    {
        private static bool _inited;
        private static readonly Vector2 _windowSize = new Vector2(350, 120);
        private static readonly Vector2 _varwinLogoSize = new Vector2(877, 174);
        private static int _varwinLogoMargin = 10;
        
        public const string VarwinUrl = "https://varwin.com";
        public const string VarwinLogoPath = @"Assets/Varwin/Core/Textures/UI/logo_g.png";

        private static Texture _logoImage;
        private static Rect _logoRect;
        

        [MenuItem("VARWIN SDK/About...", false, 900)]
        public static void ShowWinow()
        {
            Init();
            
            var window = GetWindow<VarwinAboutWindow>(true, "About VARWIN SDK", true);
            window.minSize = _windowSize;
            window.maxSize = _windowSize;
            
            window.Show();
        }

        private static void Init()
        {
            InitLogo();
            _inited = true;
        }

        private static void InitLogo()
        {
            var pos = new Vector2(_varwinLogoMargin, _varwinLogoMargin);
            var scale = (_windowSize.x - 2 * _varwinLogoMargin) / _varwinLogoSize.x;
            var size = scale * _varwinLogoSize;
            
            _logoRect = new Rect(pos, size);
            
            _logoImage = (Texture2D) AssetDatabase.LoadAssetAtPath(VarwinLogoPath, typeof(Texture2D));
        }

        private void OnGUI()
        {
            if (!_inited)
            {
                Init();
            }
            
            EditorGUI.DrawRect(new Rect(Vector2.zero, _windowSize), Color.white);
            
            DrawLogo();

            GUILayout.Space(_logoRect.height + 16);

            GUILayout.BeginVertical();

            DrawVersion();
            DrawVarwinLink();
            
            GUILayout.EndVertical();
        }

        private void DrawLogo()
        {
            GUI.DrawTexture(_logoRect, _logoImage);
        }

        private void DrawVersion()
        {
            var versionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    textColor = 0.66f * Color.black
                }
            };
            
            GUILayout.Label(VarwinVersionInfo.VarwinVersion, versionStyle, GUILayout.ExpandWidth(true));
        }

        private void DrawVarwinLink()
        {
            var linkStyle = new GUIStyle(VarwinStyles.LinkStyle)
            {
                alignment = TextAnchor.MiddleCenter
            };
            if (GUILayout.Button(VarwinUrl, linkStyle))
            {
                Application.OpenURL(VarwinUrl);
            }
        }
    }
}
