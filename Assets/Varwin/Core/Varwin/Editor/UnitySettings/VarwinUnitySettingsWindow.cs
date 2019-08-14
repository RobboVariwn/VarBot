using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    [InitializeOnLoad]
    public class VarwinUnitySettingsWindow : EditorWindow
    {
        const bool forceShow = false;
        
        static VarwinUnitySettingsWindow _window;

        static VarwinUnitySettingsWindow()
        {
            EditorApplication.update += Update;
        }

        static void Update()
        {
            if (CheckIsClient())
            {
                EditorApplication.update -= Update;
                return;
            }
            
            bool show = forceShow || VarwinUnitySettings.Options.Any(x => x.IsNeedToDraw);
            bool layersShow = VarwinUnitySettings.Layers.Any(x => !string.Equals(VarwinUnitySettings.GetLayer(x.Key), x.Value));
            bool tagsShow = !VarwinUnitySettings.TagsAreValid();
            bool bindings = !VarwinUnitySettings.CheckSteamVRBindings();

            if (show || layersShow || tagsShow || bindings)
            {
                ShowWindow();
            }
            
            EditorApplication.update -= Update;
        }

        private static bool CheckIsClient()
        {
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            return defines.Contains("VARWINCLIENT");
        }

        [MenuItem("VARWIN SDK/Settings/Project Settings", false, 800)]
        public static void ShowWindow()
        {
            _window = GetWindow<VarwinUnitySettingsWindow>(true);
            _window.minSize = new Vector2(400, 480);
            _window.titleContent = new GUIContent("Varwin Unity Project Settings");
        }

        Vector2 scrollPosition;

        public void OnGUI()
        {
            DrawLogo();
            EditorGUILayout.HelpBox("Recommended project settings for Varwin:", MessageType.Warning);

            DrawScrollView();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (VarwinUnitySettings.Options.Any(x => x.IsNeedToDraw))
            {
                DrawAcceptAll();
                DrawIgnoreAll();
            }
            else if (GUILayout.Button("Close"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
 
        }

        public void DrawLogo()
        {
            var logo = AssetDatabase.LoadAssetAtPath<Texture2D>(VarwinAboutWindow.VarwinLogoPath);
            var rect = GUILayoutUtility.GetRect(position.width, 90, GUI.skin.box);
            if (logo)
                GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
        }

        public void DrawScrollView()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var option in VarwinUnitySettings.Options)
            {
                option.Draw();
            }

            DrawBindingFiles();

            DrawLayersList();
            DrawTagsList();
            
            DrawClearAllIgnores();
            
            GUILayout.EndScrollView();
        }

        public void DrawBindingFiles()
        {
            if (!VarwinUnitySettings.CheckSteamVRBindings())
            {
                EditorGUILayout.Space();
                
                GUILayout.Label("SteamVR binding files:");
                
                var buttonStyle = new GUIStyle(EditorStyles.miniButtonLeft);
                
                if (GUILayout.Button($"Copy bindings files", buttonStyle))
                {
                    CopyAllBindings();
                }
            }
        }

        public void DrawLayersList()
        {
            int count = VarwinUnitySettings.Layers.Count(x => (!string.Equals(VarwinUnitySettings.GetLayer(x.Key), x.Value)));

            if (count > 0)
            {
                EditorGUILayout.Space();
                
                GUILayout.Label("Layers:");
                
                var layerButtonStyle = new GUIStyle(EditorStyles.miniButtonLeft);
                
                foreach (var layer in VarwinUnitySettings.Layers)
                {
                    if (!string.Equals(VarwinUnitySettings.GetLayer(layer.Key), layer.Value))
                    {
                        if (GUILayout.Button($"Set Layer{layer.Key} as \"{layer.Value}\"", layerButtonStyle))
                        {
                            VarwinUnitySettings.SetLayer(layer.Key, layer.Value);
                            VarwinUnitySettings.Save();
                        }
                    }
                }

                if (count > 1)
                {
                    EditorGUILayout.Space();
                    
                    if (GUILayout.Button($"Apply all layers"))
                    {
                        ApplyAllLayers();
                    }
                }
                
                EditorGUILayout.Space();
            }
        }

        public void DrawTagsList()
        {
            bool isValid = VarwinUnitySettings.TagsAreValid();

            if (!isValid)
            {
                EditorGUILayout.Space();
                    
                if (GUILayout.Button($"Setup all tag"))
                {
                    ApplyAllTags();
                }
                
                EditorGUILayout.Space();
            }
        }
        
        public void DrawClearAllIgnores()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear All Ignores"))
            {
                foreach (var option in VarwinUnitySettings.Options)
                {
                    option.ClearIgnore();
                }
            }
            GUILayout.EndHorizontal();
        }

        public void DrawAcceptAll()
        {
            if (GUILayout.Button("Accept All"))
            {
                foreach (var option in VarwinUnitySettings.Options)
                {
                    option.UseRecommended();
                }

                ApplyAllLayers();
                ApplyAllTags();
                CopyAllBindings();
                    
                EditorUtility.DisplayDialog("Accept All", "You made the right choice!", "Ok");
                
                Close();
            }
        }

        public void DrawIgnoreAll()
        {
            if (GUILayout.Button("Ignore All"))
            {
                if (EditorUtility.DisplayDialog("Ignore All", "Are you sure?", "Yes, Ignore All", "Cancel"))
                {
                    foreach (var option in VarwinUnitySettings.Options)
                    {
                        // Only ignore those that do not currently match our recommended settings.
                        option.Ignore();
                    }
                    Close();
                }
            }
        }

        private void ApplyAllTags()
        {
            VarwinUnitySettings.SetupTags();
            VarwinUnitySettings.Save();
        }

        private void ApplyAllLayers()
        {
            foreach (var layer in VarwinUnitySettings.Layers)
            {
                VarwinUnitySettings.SetLayer(layer.Key, layer.Value);
            }

            VarwinUnitySettings.Save();
        }

        private void CopyAllBindings()
        {
            string root = Application.dataPath + "/../";
            string bindingsPath = Application.dataPath + "/Varwin/SteamVR_Bindings/";

            foreach (string bindingsFile in VarwinUnitySettings.SteamVrBindingsFiles)
            {
                File.Copy(Path.Combine(bindingsPath, bindingsFile), Path.Combine(root, bindingsFile), true);
            }
        }
    }
}