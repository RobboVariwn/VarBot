using System;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class SdkSettingsWindow : EditorWindow
    {
        private static SdkSettingsWindow _window;
        
        private static readonly Vector2 WindowSize = new Vector2(350, 180);
        
        [MenuItem("VARWIN SDK/Settings/SDK", false, 99)]
        public static void Init()
        {
            _window = GetWindow<SdkSettingsWindow>(false, "SDK Settings", true);
            
            _window.minSize = WindowSize;
            _window.maxSize = WindowSize;
            
            _window.Show();
        }

        
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            DrawFeatureToggling();
            GUILayout.EndVertical();
        }

        private void DrawFeatureToggling()
        {
            GUILayout.BeginVertical();
            
            GUILayout.Label("Experimental Features", EditorStyles.boldLabel);

            DrawFeatureToggle("Mobile build support", SdkSettings.MobileFeature);
            
            GUILayout.EndVertical();
        }
        
        private void DrawFeatureToggle(string label, SdkFeature feature)
        {
            bool featureEnabled = EditorGUILayout.ToggleLeft(label, feature.Enabled);

            if (featureEnabled != feature.Enabled)
            {
                feature.Enabled = featureEnabled;
                feature.Save();
            }
        }
    }
}