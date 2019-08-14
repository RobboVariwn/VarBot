using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Varwin.Data;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class CreateSceneTemplateWindow : EditorWindow
    {
        private static string _locationName, _locationDesc, _guid, _rootGuid;

        private static Rect _windowRect = new Rect(0,
            0,
            550,
            540);

        private static Camera _camera;
        private static WorldDescriptor _worldDescriptor;
        private static SceneCamera _sceneCamera;

        private const int PreviewSize = 512;
        private const int ImageSize = 200;
        private const int LabelWidth = 42;

        private static string _authorName;
        private static string _authorEmail;
        private static string _authorUrl;

        private static string _licenseCode;
        private static string _licenseVersion;
        private LicenseType _selectedLicense;
        private int _selectedLicenseIndex = -1;
        private int _selectedLicenseVersionIndex = -1;

        private static long _createdAt = -1;
        private static bool _mobileReady, _currentVersionWasBuilt;

        [MenuItem("VARWIN SDK/Create/Scene Template", false, 0)]
        public static void ShowWindow()
        {
            FindOrCreateWorldDescriptor();
            FindOrCreateCamera();
            FillDefaultDataIfNull();

            GetWindowWithRect(typeof(CreateSceneTemplateWindow),
                _windowRect,
                false,
                "Create scene template");
            _locationName = _worldDescriptor.Name;
            _locationDesc = _worldDescriptor.Description;
            _guid = _worldDescriptor.Guid;
            _rootGuid = _worldDescriptor.RootGuid;
            _mobileReady = SdkSettings.MobileFeature.Enabled && _worldDescriptor.MobileReady;
            _currentVersionWasBuilt = _worldDescriptor.CurrentVersionWasBuilt;

            if (string.IsNullOrWhiteSpace(_worldDescriptor.AuthorName))
            {
                AuthorSettings.Initialize();
                _authorName = AuthorSettings.Name;
                _authorEmail = AuthorSettings.Email;
                _authorUrl = AuthorSettings.Url;
            }
            else
            {
                _authorName = _worldDescriptor.AuthorName;
                _authorEmail = _worldDescriptor.AuthorEmail;
                _authorUrl = _worldDescriptor.AuthorUrl;
            }
        }

        private static void FillDefaultDataIfNull()
        {
            if (string.IsNullOrEmpty(_worldDescriptor.Name))
            {
                _worldDescriptor.Name = "Scene Template";
            }

            if (string.IsNullOrEmpty(_worldDescriptor.Description))
            {
                _worldDescriptor.Description = "Scene template description";
            }

            if (string.IsNullOrEmpty(_worldDescriptor.Guid))
            {
                _worldDescriptor.Guid = Guid.NewGuid().ToString();
                _worldDescriptor.RootGuid = _worldDescriptor.Guid;
                _worldDescriptor.CurrentVersionWasBuilt = false;
            }
        }

        private static Camera FindOrCreateCamera()
        {
            _sceneCamera = SceneUtils.GetObjectsInScene<SceneCamera>(true).FirstOrDefault();

            if (_sceneCamera == null)
            {
                GameObject cameraPreview = new GameObject("[Camera Preview]");
                cameraPreview.transform.SetParent(_worldDescriptor.transform, true);
                cameraPreview.transform.position = new Vector3(0, 1, 0);
                
                _sceneCamera = cameraPreview.AddComponent<SceneCamera>();
                _sceneCamera.Width = PreviewSize;
                _sceneCamera.Height = PreviewSize;
                _sceneCamera.GetComponent<Camera>().targetTexture = new RenderTexture(512, 512, 16);
            }
            else
            {
                _sceneCamera.gameObject.name = "[Camera Preview]";
                _sceneCamera.transform.SetParent(_worldDescriptor.transform, true);
            }

            _sceneCamera.gameObject.SetActive(true);

            _camera = _sceneCamera.GetComponent<Camera>();
            _camera.enabled = true;

            return _camera;
        }

        private static void FindOrCreateWorldDescriptor()
        {
            _worldDescriptor = SceneUtils.GetObjectsInScene<WorldDescriptor>(true).FirstOrDefault();
            
            if (_worldDescriptor != null)
            {
                _worldDescriptor.gameObject.SetActive(true);
                
                FindOrCreateSpawnPoint(_worldDescriptor);
                return;
            }

            GameObject worldDescriptorGo = new GameObject("[World Descriptor]");
            worldDescriptorGo.transform.position = Vector3.zero;
            worldDescriptorGo.transform.rotation = Quaternion.identity;
            worldDescriptorGo.transform.localScale = Vector3.one;
            
            _worldDescriptor = worldDescriptorGo.AddComponent<WorldDescriptor>();
            
            FindOrCreateSpawnPoint(_worldDescriptor);
        }

        private static Transform FindOrCreateSpawnPoint(WorldDescriptor worldDescriptor)
        {
            if (worldDescriptor.PlayerSpawnPoint == null)
            {
                var spawnPoint = new GameObject("[Spawn Point]");
                spawnPoint.transform.position = Vector3.zero;
                spawnPoint.transform.rotation = Quaternion.identity;
                spawnPoint.transform.localScale = Vector3.one;
                spawnPoint.transform.SetParent(_worldDescriptor.transform);
                worldDescriptor.PlayerSpawnPoint = spawnPoint.transform;
            }
            else
            {
                worldDescriptor.PlayerSpawnPoint.SetParent(_worldDescriptor.transform, true);
                worldDescriptor.PlayerSpawnPoint.gameObject.name = "[Spawn Point]";
            }

            return worldDescriptor.PlayerSpawnPoint;
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Scene Template Settings", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(GUILayout.Width(300));

            DrawNameField();
            DrawGuidLabel();
            
            EditorGUILayout.Space();
            DrawDescriptionArea();

            if (SdkSettings.MobileFeature.Enabled)
            {
                EditorGUILayout.Space();
                DrawPlatformsArea();
            }
            else
            {
                _mobileReady = false;
            }

            EditorGUILayout.Space();
            DrawAuthorSettings();
            
            EditorGUILayout.Space();
            DrawLicenseSettings();
            
            EditorGUILayout.Space();
            DrawCreateLocationButton();

            EditorGUILayout.EndVertical();

            if (_camera != null && _camera.targetTexture != null)
            {
                var cameraRect = new Rect(330,
                    20,
                    ImageSize,
                    ImageSize);
                EditorGUI.DrawPreviewTexture(cameraRect, _camera.targetTexture);

                var cameraMoveButtonRect = new Rect(cameraRect) {height = 20, y = cameraRect.yMax};

                if (GUI.Button(cameraMoveButtonRect, "Move camera to editor view"))
                {
                    SceneUtils.MoveCameraToEditorView(_camera ?? FindOrCreateCamera());
                }
            }
        }

        private void OnDestroy()
        {
            if (_camera != null)
            {
                _camera.enabled = false;
            }
        }

        private void DrawNameField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", GUILayout.Width(LabelWidth));
            _locationName = EditorGUILayout.TextField(_locationName);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGuidLabel()
        {
            EditorGUILayout.LabelField($"Guid: {_guid}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Root Guid: {_rootGuid}", EditorStyles.miniLabel);
        }

        private void DrawDescriptionArea()
        {
            EditorGUILayout.LabelField("Description");
            _locationDesc = EditorGUILayout.TextArea(_locationDesc, GUILayout.Height(110));
        }

        private void DrawPlatformsArea()
        {
            _mobileReady = EditorGUILayout.Toggle("Mobile Ready", _mobileReady);
        }

        private void DrawCreateLocationButton()
        {
            bool isDisabled = string.IsNullOrWhiteSpace(_authorName)
                              || string.IsNullOrWhiteSpace(_locationName)
                              || string.IsNullOrWhiteSpace(_guid);

            EditorGUI.BeginDisabledGroup(isDisabled);

            if (GUILayout.Button("Build"))
            {
                if (!CheckLocationForTeleport())
                {
                    if (!EditorUtility.DisplayDialog("Varwin Scene Template",
                        "At the scene there is no object with the tag \"TeleportArea\". Continue building?",
                        "Create",
                        "Cancel"))
                    {
                        return;
                    }
                }

                CreateScene();
            }

            if (!string.Equals(_guid, _rootGuid) || _currentVersionWasBuilt)
            {
                DrawCreateNewVersionButton();
            }
            
            EditorGUI.EndDisabledGroup();

            if (!CheckLocationForTeleport())
            {
                EditorGUILayout.HelpBox("At the scene there is no object with the tag \"TeleportArea\"",
                    MessageType.Warning);
            }

            if (!CheckAndroidModuleInstalled())
            {
                EditorGUILayout.HelpBox("Android Module for Unity is not installed",
                    MessageType.Warning);
            }
        }

        private void DrawCreateNewVersionButton()
        {
            bool isDisabled = !_currentVersionWasBuilt;

            EditorGUI.BeginDisabledGroup(isDisabled);

            if (GUILayout.Button("Create new version"))
            {
                string message = @"Create a new version of the scene template?";
                if (EditorUtility.DisplayDialog("Create a new version of the scene template", message, "Yes", "Cancel"))
                {
                    _guid = Guid.NewGuid().ToString();
                    _currentVersionWasBuilt = false;
                    _worldDescriptor.Guid = _guid;
                    _worldDescriptor.CurrentVersionWasBuilt = _currentVersionWasBuilt;
                
                    if (EditorUtility.DisplayDialog("Varwin Scene Template", "Build new version of scene template? Editor will proceed to scene building.", "Yes", "Cancel"))
                    {
                        if (!CheckLocationForTeleport())
                        {
                            if (!EditorUtility.DisplayDialog("Varwin Scene Template",
                                "At the scene there is no object with the tag \"TeleportArea\". Continue creation?",
                                "Create",
                                "Cancel"))
                            {
                                return;
                            }
                        }

                        CreateScene();
                        return;
                    }
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawAuthorSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Author:", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", GUILayout.Width(LabelWidth));
            _authorName = EditorGUILayout.TextField(_authorName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("E-Mail:", GUILayout.Width(LabelWidth));
            _authorEmail = EditorGUILayout.TextField(_authorEmail);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("URL:", GUILayout.Width(LabelWidth));
            _authorUrl = EditorGUILayout.TextField(_authorUrl);
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Reset to default author settings"))
            {
                AuthorSettings.Initialize();

                if (string.IsNullOrWhiteSpace(AuthorSettings.Name))
                {
                    AuthorSettingsWindow.Init();
                }
                else
                {
                    _authorName = AuthorSettings.Name;
                    _authorEmail = AuthorSettings.Email;
                    _authorUrl = AuthorSettings.Url;
                }
            }

            GUILayout.EndHorizontal();

            DrawAuthorCanNotBeEmptyHelpBox();

            EditorGUILayout.EndVertical();
        }

        private void DrawAuthorCanNotBeEmptyHelpBox()
        {
            if (string.IsNullOrWhiteSpace(_authorName))
            {
                EditorGUILayout.HelpBox("Author name can not be empty!", MessageType.Error);
            }
        }


        private void DrawLicenseSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("License:", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();

            string prevSelectedLicenseCode = _licenseCode;

            _selectedLicense = LicenseSettings.Licenses.FirstOrDefault(x => string.Equals(x.Code, _licenseCode));

            if (_selectedLicense == null)
            {
                _selectedLicense = LicenseSettings.Licenses.FirstOrDefault();

                _selectedLicenseIndex = 0;
                _selectedLicenseVersionIndex = 0;
            }
            else
            {
                _selectedLicenseIndex = LicenseSettings.Licenses.IndexOf(_selectedLicense);
                _selectedLicenseVersionIndex = Array.IndexOf(_selectedLicense.Versions, _licenseVersion);
            }

            var licenseNames = LicenseSettings.Licenses.Select(license => license.Name).ToArray();
            _selectedLicenseIndex = EditorGUILayout.Popup(_selectedLicenseIndex, licenseNames);

            _selectedLicense = LicenseSettings.Licenses.ElementAt(_selectedLicenseIndex);
            _licenseCode = _selectedLicense.Code;

            if (!string.Equals(prevSelectedLicenseCode, _licenseCode))
            {
                _selectedLicenseVersionIndex = 0;
            }

            if (_selectedLicense.Versions != null && _selectedLicense.Versions.Length > 0)
            {
                if (_selectedLicenseVersionIndex >= _selectedLicense.Versions.Length
                    || _selectedLicenseVersionIndex < 0)
                {
                    _selectedLicenseVersionIndex = 0;
                }

                _selectedLicenseVersionIndex = EditorGUILayout.Popup(_selectedLicenseVersionIndex,
                    _selectedLicense.Versions,
                    GUILayout.Width(80));
                _licenseVersion = _selectedLicense.Versions[_selectedLicenseVersionIndex];
            }
            else
            {
                _licenseVersion = string.Empty;
            }

            EditorGUILayout.EndHorizontal();

            if (_selectedLicense == null)
            {
                VarwinStyles.Link(LicenseSettings.CreativeCommonsLink);
            }
            else
            {
                VarwinStyles.Link(_selectedLicense.GetLink(_licenseVersion));
            }

            EditorGUILayout.EndVertical();
        }

        private bool CheckLocationForTeleport()
        {
            var areas = GameObject.FindGameObjectsWithTag("TeleportArea");

            return areas.Length > 0;
        }

        private bool CheckAndroidModuleInstalled()
        {
            if (!_mobileReady)
            {
                return true;
            }

            var bindingFlags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
            var moduleManager = System.Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
            var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", bindingFlags);
            var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", bindingFlags);
            return (bool)isPlatformSupportLoaded.Invoke(null,new object[] {(string)getTargetStringFromBuildTarget.Invoke(null, new object[] {BuildTarget.Android})});
        }

        private static void CreateScene()
        {
            Debug.Log("Starting create scene template...");

            _worldDescriptor.Name = _locationName;
            _worldDescriptor.Description = _locationDesc;
            _worldDescriptor.Guid = _guid;
            _worldDescriptor.RootGuid = _rootGuid;
            if (string.IsNullOrEmpty(_worldDescriptor.RootGuid))
            {
                _rootGuid = _guid;
                _worldDescriptor.RootGuid = _guid;
            }
            _worldDescriptor.Image = "bundle.png";
            _worldDescriptor.AssetBundleLabel = "bundle";
            
            _worldDescriptor.AuthorName = _authorName;
            _worldDescriptor.AuthorEmail = _authorEmail;
            _worldDescriptor.AuthorUrl = _authorUrl;

            _worldDescriptor.LicenseCode = _licenseCode;
            _worldDescriptor.LicenseVersion = _licenseVersion;

            _worldDescriptor.BuiltAt = DateTimeOffset.Now.ToString();
            _worldDescriptor.MobileReady = _mobileReady;

            _currentVersionWasBuilt = true;
            _worldDescriptor.CurrentVersionWasBuilt = _currentVersionWasBuilt;
            
            var iconBytes = _sceneCamera.GetIconBytes();

            var cams = FindObjectsOfType<Camera>().ToList();

            var scripts = FindObjectsOfType<MonoBehaviour>();

            var sceneCam = _sceneCamera.GetComponent<Camera>();

            if (sceneCam)
            {
                cams.Remove(sceneCam);
            }

            ToggleCams(cams, sceneCam, false);

            foreach (MonoBehaviour script in scripts)
            {
                script.enabled = false;
            }

            try
            {
                var sb = new SceneBundleBuilder().BuildAndShow(_worldDescriptor, iconBytes);

                if (sb != default(string))
                {
                    Debug.Log("Scene template created!");

                    if (EditorUtility.DisplayDialog("Varwin Scene Template", "Scene template was built and packed.", "OK"))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", Path.GetDirectoryName(sb));
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Varwin Scene Template",
                        "Something went wrong. Please check Unity console output for more info.",
                        "OK");
                }

                GetWindow(typeof(CreateSceneTemplateWindow)).Close();
            }
            catch (Exception e)
            {
                Debug.Log("Encountered exception " + e.Message);
            }


            foreach (MonoBehaviour script in scripts)
            {
                if (script != null)
                {
                    script.enabled = true;
                }
            }

            ToggleCams(cams, sceneCam, true);
        }

        private static void ToggleCams(List<Camera> cams, Camera preview, bool toggle)
        {
            foreach (Camera cam in cams)
            {
                if (cam == null || cam.gameObject == null)
                {
                    continue;
                }

                if (!cam.targetTexture)
                {
                    cam.enabled = toggle;
                }
            }

            if (_sceneCamera != null)
            {
                _sceneCamera.enabled = toggle;
            }

            if (preview != null)
            {
                preview.enabled = toggle;
            }
        }

        private void Update()
        {
            Repaint();
        }

        public class InstallConfig : IJsonSerializable
        {
            public string Name;
            public string Guid;
            public string RootGuid;
            public Author Author;
            public License License;
            public string LicenseCode;
            public string BuiltAt;
            public bool MobileReady;
            public string SdkVersion;
        }

        public class Author : IJsonSerializable
        {
            public string Name;
            public string Email;
            public string Url;
        }

        public class License : IJsonSerializable
        {
            public string Code;
            public string Version;
        }

        public class SceneConfig : IJsonSerializable
        {
            public string name;
            public string description;
            public string image;
            public string assetBundleLabel;
            public string[] dllNames;
        }
    }
}
