#define VRMAKER
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
#if VRMAKER
#endif
using UnityEditor;
using UnityEngine;
using Varwin.Editor;
using Varwin.Public;
using Object = UnityEngine.Object;

[CustomEditor(typeof(VarwinObjectDescriptor))]
[CanEditMultipleObjects]
public class VarwinObjectDescriptorEditor : Editor
{
    private VarwinObjectDescriptor _varwinObjectDescriptor;
    private GameObject _gameObject;
    
    private SerializedProperty _nameProperty;
    private SerializedProperty _iconProperty;
    private SerializedProperty _embeddedProperty;
    private SerializedProperty _mobileReadyProperty;
    
    private SerializedProperty _configBlocklyProperty;
    private SerializedProperty _configAssetBundleProperty;
    private SerializedProperty _guidProperty;
    private SerializedProperty _rootGuidProperty;
    private SerializedProperty _prefabProperty;
    
    private SerializedProperty _authorNameProperty;
    private SerializedProperty _authorEmailProperty;
    private SerializedProperty _authorUrlProperty;

    private SerializedProperty _licenseCodeProperty;
    private SerializedProperty _licenseVersionProperty;
    private LicenseType _selectedLicense;
    private int _selectedLicenseIndex = -1;
    private int _selectedLicenseVersionIndex = -1;
    
    private SerializedProperty _builtAtProperty;
    
    private string _bakedObjectDirectory;
    private string _assetBundleDirectory;

    private bool _showDebug;
    private bool _showDeveloperMode;

    private static string _objectName;
    private static string _prefabPath;
    private static string _bundleName;
    private static string _dllFolder;
    private static string _zipFilePath;
    private static string _iconPath;
    private static string _tagsPath;
    private Object _createdObject;

    private const int SaveIter = 50;

    private void OnEnable()
    {
#if VRMAKER

        if (target == null)
        {
            return;
        }
        
        _varwinObjectDescriptor = (VarwinObjectDescriptor) target;

        if (_varwinObjectDescriptor != null)
        {
            _gameObject = _varwinObjectDescriptor.gameObject;
        }

        _nameProperty = serializedObject.FindProperty("Name");
        _iconProperty = serializedObject.FindProperty("Icon");
        _embeddedProperty = serializedObject.FindProperty("Embedded");
        _mobileReadyProperty = serializedObject.FindProperty("MobileReady");
        
        _guidProperty = serializedObject.FindProperty("Guid");
        _rootGuidProperty = serializedObject.FindProperty("RootGuid");
        _configBlocklyProperty = serializedObject.FindProperty("ConfigBlockly");
        _configAssetBundleProperty = serializedObject.FindProperty("ConfigAssetBundle");
        _prefabProperty = serializedObject.FindProperty("Prefab");
        
        _authorNameProperty = serializedObject.FindProperty("AuthorName");
        _authorEmailProperty = serializedObject.FindProperty("AuthorEmail");
        _authorUrlProperty = serializedObject.FindProperty("AuthorUrl");

        _licenseCodeProperty = serializedObject.FindProperty("LicenseCode");
        _licenseVersionProperty = serializedObject.FindProperty("LicenseVersion");
        
        _builtAtProperty = serializedObject.FindProperty("BuiltAt");

        _bakedObjectDirectory = Application.dataPath.Replace("Assets", "BakedObjects");
        _assetBundleDirectory = Application.dataPath.Replace("Assets", "AssetBundles");
        _objectName = _nameProperty.stringValue;
        _bundleName = $"{_objectName}_{_rootGuidProperty.stringValue.Replace("-", "")}";
        _prefabPath = _prefabProperty.stringValue;
        string _prefabName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(_prefabPath));
        _iconPath = $"Assets/Objects/{_prefabName}/icon.png";
        _tagsPath = $"Assets/Objects/{_prefabName}/tags.txt";

        _dllFolder = $"{Application.dataPath.Replace("Assets", "Library")}/ScriptAssemblies";

        _zipFilePath = $"{_bakedObjectDirectory}/{_objectName}.vwo";
#endif
    }

    public override void OnInspectorGUI()
    {
#if VRMAKER
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(_nameProperty);
        EditorGUILayout.PropertyField(_prefabProperty);
        EditorGUILayout.PropertyField(_iconProperty);
        EditorGUILayout.PropertyField(_embeddedProperty);

        if (SdkSettings.MobileFeature.Enabled)
        {
            EditorGUILayout.PropertyField(_mobileReadyProperty);
        }

        if (EditorApplication.isCompiling)
        {
            EditorGUILayout.HelpBox("Unity is compiling. Please, wait...", MessageType.Info);
            return;
        }
        
        EditorGUILayout.Space();
        DrawAuthorSettings();
        EditorGUILayout.Space();
        DrawLicenseSettings();
        EditorGUILayout.Space();
        DrawBuildInfo();
        EditorGUILayout.Space();
        DrawBuildButton();

        EditorGUILayout.Space();
        DrawDebugInfo();
        EditorGUILayout.Space();
        DrawDeveloperMode();

        serializedObject.ApplyModifiedProperties();

#endif
    }

    private void DrawDebugInfo()
    {
        _showDebug = EditorGUILayout.Foldout(_showDebug, "Debug Info");
        if (_showDebug)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.PropertyField(_guidProperty);
            EditorGUILayout.PropertyField(_rootGuidProperty);
            EditorGUILayout.PropertyField(_configBlocklyProperty);
            EditorGUILayout.PropertyField(_configAssetBundleProperty);
            EditorGUILayout.PropertyField(_authorNameProperty);
            EditorGUILayout.PropertyField(_authorEmailProperty);
            EditorGUILayout.PropertyField(_authorUrlProperty);
            EditorGUILayout.PropertyField(_licenseCodeProperty);
            EditorGUILayout.PropertyField(_licenseVersionProperty);
            
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawDeveloperMode()
    {
        _showDeveloperMode = EditorGUILayout.Foldout(_showDeveloperMode, "Developer Mode");
        if (_showDeveloperMode)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button("Create Wrapper"))
            {
                ObjectsBuilderWindow.BuildOnlyWrapper((VarwinObjectDescriptor) target);
            }
            
            if (GUILayout.Button("Create Icon"))
            {
                ObjectsBuilderWindow.BuildOnlyIcon((VarwinObjectDescriptor) target);
            }

            if (GUILayout.Button("Create Config"))
            {
                ObjectsBuilderWindow.BuildOnlyConfigs((VarwinObjectDescriptor) target);
            }

            if (GUILayout.Button("Build Asset Bundle"))
            {
                ObjectsBuilderWindow.BuildOnlyBundle((VarwinObjectDescriptor) target);
            }

            if (GUILayout.Button("Zip And Show"))
            {
                ObjectsBuilderWindow.BuildOnlyZip((VarwinObjectDescriptor) target);
            }
            
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawAuthorSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
          
        GUILayout.Label("Author:", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(_authorNameProperty);
        EditorGUILayout.PropertyField(_authorEmailProperty);
        EditorGUILayout.PropertyField(_authorUrlProperty);
        
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
                _authorNameProperty.stringValue = AuthorSettings.Name;
                _authorEmailProperty.stringValue = AuthorSettings.Email;
                _authorUrlProperty.stringValue = AuthorSettings.Url;
            }
        }
        GUILayout.EndHorizontal();
        
        DrawAuthorCanNotBeEmptyHelpBox();

        EditorGUILayout.EndVertical();
    }

    private void DrawLicenseSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("License:", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();

        string prevSelectedLicenseCode = _licenseCodeProperty.stringValue;
        
        _selectedLicense = LicenseSettings.Licenses.FirstOrDefault(x => string.Equals(x.Code, _licenseCodeProperty.stringValue));
        if (_selectedLicense == null)
        {
            _selectedLicense = LicenseSettings.Licenses.FirstOrDefault();
            
            _selectedLicenseIndex = 0;
            _selectedLicenseVersionIndex = 0;
        }
        else
        {
            _selectedLicenseIndex = LicenseSettings.Licenses.IndexOf(_selectedLicense);
            _selectedLicenseVersionIndex = Array.IndexOf(_selectedLicense.Versions, _licenseVersionProperty.stringValue);
        }

        var licenseNames = LicenseSettings.Licenses.Select(license => license.Name).ToArray();
        _selectedLicenseIndex = EditorGUILayout.Popup(_selectedLicenseIndex, licenseNames);

        _selectedLicense = LicenseSettings.Licenses.ElementAt(_selectedLicenseIndex);
        _licenseCodeProperty.stringValue = _selectedLicense.Code;

        if (!string.Equals(prevSelectedLicenseCode, _licenseCodeProperty.stringValue))
        {
            _selectedLicenseVersionIndex = 0;
        }

        if (_selectedLicense.Versions != null && _selectedLicense.Versions.Length > 0)
        {
            if (_selectedLicenseVersionIndex >= _selectedLicense.Versions.Length || _selectedLicenseVersionIndex < 0)
            {
                _selectedLicenseVersionIndex = 0;
            }
            
            _selectedLicenseVersionIndex = EditorGUILayout.Popup(_selectedLicenseVersionIndex, _selectedLicense.Versions, GUILayout.Width(80));
            _licenseVersionProperty.stringValue = _selectedLicense.Versions[_selectedLicenseVersionIndex];
        }
        else
        {
            _licenseVersionProperty.stringValue = string.Empty;
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (_selectedLicense == null)
        {
            VarwinStyles.Link(LicenseSettings.CreativeCommonsLink);
        }
        else
        {
            VarwinStyles.Link(_selectedLicense.GetLink(_licenseVersionProperty.stringValue));
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawBuildInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("Build Info:", EditorStyles.miniBoldLabel);

        GUILayout.Label($"Built at {_builtAtProperty.stringValue}", EditorStyles.miniLabel);
        
        EditorGUILayout.EndVertical();
    }

    private void DrawBuildButton()
    {
        bool isDisabled = !IsSelectable() || string.IsNullOrWhiteSpace(_nameProperty.stringValue) || string.IsNullOrWhiteSpace(_authorNameProperty.stringValue);

        EditorGUI.BeginDisabledGroup(isDisabled);
        
        if (GUILayout.Button("Build"))
        {
            if (CreateObjectUtils.ContainsObjectIdDuplicates(_gameObject))
            {
                if (!DisplayDuplicatesObjectIdsDialog())
                {
                    return;
                }
            }

            if (!CheckTypesForValid())
            {
                return;
            }

            if (EditorUtility.DisplayDialog("Varwin Object Build Dialog", "Editor will proceed to object building", "Yes", "Cancel"))
            {
                BuildVarwinObject(_varwinObjectDescriptor);
                return;
            }
        }
        
        if (!string.Equals(_varwinObjectDescriptor.Guid, _varwinObjectDescriptor.RootGuid) || _varwinObjectDescriptor.CurrentVersionWasBuilt)
        {
            DrawCreateNewVersionButton();
        }
        
        EditorGUI.EndDisabledGroup();

        if (string.IsNullOrWhiteSpace(_nameProperty.stringValue))
        {
            EditorGUILayout.HelpBox("Object type name is empty", MessageType.Error);
        }

        if (!IsSelectable())
        {
            EditorGUILayout.HelpBox("Object is not selectable. Add Rigidbody and Collider to the root object", MessageType.Error);
        }

        if (CreateObjectUtils.ContainsObjectIdDuplicates(_gameObject))
        {
            EditorGUILayout.HelpBox("Object contains duplicate Object Ids", MessageType.Warning);
        }
    }

    private void DrawCreateNewVersionButton()
    {
        EditorGUI.BeginDisabledGroup(!_varwinObjectDescriptor.CurrentVersionWasBuilt);
        
        if (GUILayout.Button("Create new version"))
        {
            string message = @"Create a new version of the object?";
            if (EditorUtility.DisplayDialog("Create a new version of the object", message, "Yes", "Cancel"))
            {
                _varwinObjectDescriptor.Guid = Guid.NewGuid().ToString();
                _varwinObjectDescriptor.CurrentVersionWasBuilt = false;
                
                if (EditorUtility.DisplayDialog("Varwin Object Build Dialog", "Build new version of object? Editor will proceed to object building.", "Yes", "Cancel"))
                {
                    if (CreateObjectUtils.ContainsObjectIdDuplicates(_gameObject))
                    {
                        if (!DisplayDuplicatesObjectIdsDialog())
                        {
                            return;
                        }
                    }

                    if (!CheckTypesForValid())
                    {
                        return;
                    }

                    BuildVarwinObject(_varwinObjectDescriptor);
                    return;
                }
            }
        }
        
        EditorGUI.EndDisabledGroup();
    }

    private bool DisplayDuplicatesObjectIdsDialog()
    {
        if (EditorUtility.DisplayDialog("Varwin Object Build Dialog", $"Object contains duplicate Object Ids. Remove duplicates?", "OK", "Cancel"))
        {
            var duplicates = CreateObjectUtils.GetObjectIdDuplicates(_gameObject);
            var rootObjectId = _gameObject.GetComponent<ObjectId>();

            if (rootObjectId != null)
            {
                if (duplicates.Contains(rootObjectId))
                {
                    duplicates.Remove(rootObjectId);
                }
            }

            foreach (var duplicate in duplicates)
            {
                CreateObjectUtils.SafeDestroy(duplicate);
            }

            return true;
        }
        return false;
    }

    private bool CheckTypesForValid()
    {
        var invalidTypes = new List<string>();
        if (!CreateObjectUtils.ValidateAssemblies(ref invalidTypes, _gameObject, _objectName))
        {
            foreach (var invalidType in invalidTypes)
            {
                EditorUtility.DisplayDialog("Invalid script assembly", $"{invalidType} is not a valid assembly! Move this script to the object folder or create a new reference inside the Assembly Definition", "OK");
                return false;
            }
        }
        return true;
    }

    private bool IsSelectable()
    {
        var rbs = _gameObject.GetComponentsInChildren<Rigidbody>();
        var cols = _gameObject.GetComponentsInChildren<Collider>();
        if (rbs.Length > 0 && cols.Length > 0)
        {
            return _gameObject.GetComponent<Rigidbody>() != null || _gameObject.GetComponent<Collider>() != null;
        }
        return false;
    }

    private void DrawAuthorCanNotBeEmptyHelpBox()
    {
        if (string.IsNullOrWhiteSpace(_authorNameProperty.stringValue))
        {
            EditorGUILayout.HelpBox("Author name can not be empty!", MessageType.Error);
        }
    }

    private ObjectsBuilderWindow GetWindow()
    {
        var window = EditorWindow.focusedWindow;
        if (window == null)
        {
            return null;
        }
        
        return window as ObjectsBuilderWindow;
    }
    
    private void OnObjectBuilt(System.Object sender, EventArgs eventArgs)
    {   
        var varwinObject = (VarwinObjectDescriptor) target;
        
        ObjectsBuilderWindow window = GetWindow();
        if (window != null)
        {
            window.ObjectBuilt -= OnObjectBuilt;
        }
        
        CreateObjectUtils.RevertPrefabInstanceChanges(varwinObject.gameObject);
    }

    public void BuildVarwinObject(VarwinObjectDescriptor varwinObjectDescriptor)
    {
        varwinObjectDescriptor.PreBuild();
        
        CreateObjectUtils.ApplyPrefabInstanceChanges(varwinObjectDescriptor.gameObject);
        
        VarwinObjectDescriptor varwinPrefab = CreateObjectUtils.GetPrefab(varwinObjectDescriptor.gameObject).GetComponent<VarwinObjectDescriptor>();
        
        ObjectsBuilderWindow.BuildVarwinObject(varwinObjectDescriptor);
        ObjectsBuilderWindow window = GetWindow();
        if (window != null)
        {
            window.ObjectBuilt += OnObjectBuilt;
        }
    }
}
