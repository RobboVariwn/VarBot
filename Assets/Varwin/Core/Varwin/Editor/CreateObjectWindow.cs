using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Varwin.Public;
using Object = UnityEngine.Object;

namespace Varwin.Editor
{
    public class CreateObjectWindow : EditorWindow
    {
        private static Vector2 MinWindowSize => new Vector2(420, 480);
        private static Vector2 MaxWindowSize => new Vector2(640, 800);

        private static Vector2 _scrollPosition;
        
        private static string _objectClassName = "ObjectClassName";
        private static bool _objectTypeNameIsValid = true;
        
        private static string _localizedName = "Object name";
        private static string _localizedNameRU = "Название объекта";

        private static string _authorName;
        private static string _authorEmail;
        private static string _authorUrl;

        private static string _licenseCode;
        private static string _licenseVersion;
        private LicenseType _selectedLicense;
        private int _selectedLicenseIndex = -1;
        private int _selectedLicenseVersionIndex = -1;

        private static long _createdAt = -1;
        private static bool _mobileReady;

        private static GameObject _gameObject;
        
        private static GameObject _prefab;
        private string _tags;
        
        private static bool _selectedGameObjectIsValid = false;

        /// <summary>
        /// List of existing object names
        /// </summary>
        private static List<string> _existingObjectNames = new List<string>();
        
        /// <summary>
        /// List of models
        /// </summary>
        private static List<CreateObjectModel> _modelsList;

        /// <summary>
        /// Build now checker
        /// </summary>
        private static bool _buildNow;

        [MenuItem("VARWIN SDK/Create/Object", false, 0)]
        static void Init()
        {
            var window = GetWindow<CreateObjectWindow>(true, "Create object", true);
            window.minSize = MinWindowSize;
            window.maxSize = MaxWindowSize;
            window.Show();
            
            _existingObjectNames = ObjectBuildHelper.GetExistingObjectsNames();

            if (Selection.activeGameObject != null)
            {
                _gameObject = Selection.activeGameObject;
                
                string objectTypeName = Regex.Replace(_gameObject.name, "([A-ZА-Я])", " $1");
                objectTypeName = ObjectBuildHelper.ConvertToNiceName(objectTypeName).Trim();
                _objectClassName = objectTypeName
                    .Replace(" ", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("-", "");

                _localizedName = objectTypeName;
                _localizedNameRU = objectTypeName;
            }

            AuthorSettings.Initialize();
            _authorName = AuthorSettings.Name;
            _authorEmail = AuthorSettings.Email;
            _authorUrl = AuthorSettings.Url;
            
            _scrollPosition = Vector2.zero;
        }

        void OnGUI()
        {
            if (File.Exists(CreateObjectTempModel.TempFilePath))
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.Label("Please wait until all objects are created.\nThis window will be closed automatically.",
                    EditorStyles.boldLabel, GUILayout.ExpandHeight(true));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label("Object Settings", EditorStyles.boldLabel);
            DrawObjectTypeNameField();
            
            EditorGUILayout.Space();
            DrawObjectField();

            if (SdkSettings.MobileFeature.Enabled)
            {
                EditorGUILayout.Space();
                DrawPlatformsSettings();
            }
            else
            {
                _mobileReady = false;
            }

            EditorGUILayout.Space();
            DrawLocalizedName();
            
            EditorGUILayout.Space();
            DrawAuthorSettings();
            
            EditorGUILayout.Space();
            DrawLicenseSettings();
            
            EditorGUILayout.Space();
            _tags = EditorGUILayout.TextField("Tags (by \",\")", _tags);
            
            EditorGUILayout.Space();
            DrawCreateObjectButton();
            
            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }

        private void DrawObjectTypeNameField()
        {
            _objectClassName = EditorGUILayout.TextField("Object Class Name", _objectClassName);
            
            _objectTypeNameIsValid = false;
            if (string.IsNullOrEmpty(_objectClassName))
            {
                EditorGUILayout.HelpBox($"Object Class Name can't be empty", MessageType.Error);
            }
            else if (!ObjectBuildHelper.IsValidTypeName(_objectClassName))
            {
                EditorGUILayout.HelpBox($"Object Class Name contains unavailable symbols", MessageType.Error);
            }
            else if (_existingObjectNames.Any(x => string.Equals(x, _objectClassName, StringComparison.OrdinalIgnoreCase)))
            {
                EditorGUILayout.HelpBox($"An object with the same Object Class Name already exists.", MessageType.Error);
            }
            else
            {
                _objectTypeNameIsValid = true;
            }
        }

        private void DrawPlatformsSettings()
        {
            _mobileReady = EditorGUILayout.Toggle("Mobile Ready", _mobileReady);
        }

        private void DrawLocalizedName()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Localized Name:", EditorStyles.miniBoldLabel);

            var labelOffset = 32;
            var labelWidth = 23;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(labelOffset);
            EditorGUILayout.LabelField("EN", GUILayout.Width(labelWidth));
            _localizedName = EditorGUILayout.TextField(_localizedName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(labelOffset);
            EditorGUILayout.LabelField("RU", GUILayout.Width(labelWidth));
            _localizedNameRU = EditorGUILayout.TextField(_localizedNameRU);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawObjectField()
        {
            _gameObject = (GameObject) EditorGUILayout.ObjectField("Game Object",  _gameObject, typeof(GameObject), true);

            _selectedGameObjectIsValid = false;
            if (_gameObject != null)
            {
                var varwinObject = _gameObject.GetComponentsInChildren<VarwinObjectDescriptor>(true);
                if (varwinObject != null && varwinObject.Length > 0)
                {
                    DrawObjectContainsComponentHelpBox<VarwinObjectDescriptor>();
                    return;
                }
                
                var objectIdComponent = _gameObject.GetComponentsInChildren<ObjectId>(true);
                if (objectIdComponent != null && objectIdComponent.Length > 0)
                {
                    DrawObjectContainsComponentHelpBox<ObjectId>();
                    return;
                }
                
                var iWrapperAware = _gameObject.GetComponentsInChildren<IWrapperAware>(true);
                if (iWrapperAware != null && iWrapperAware.Length > 0)
                {
                    DrawObjectContainsComponentHelpBox<IWrapperAware>();
                    return;
                }

                _selectedGameObjectIsValid = true;
            }
            else
            {
                EditorGUILayout.HelpBox($"Object can't be null", MessageType.Error);
            }
        }

        private void DrawObjectContainsComponentHelpBox<T>(MessageType messageType = MessageType.Error)
        {
            EditorGUILayout.HelpBox($"Object contains <{typeof(T).Name}> component", messageType);
        }
        
        private void DrawAuthorSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Author:", EditorStyles.miniBoldLabel);
            _authorName = EditorGUILayout.TextField("Name", _authorName);
            _authorEmail = EditorGUILayout.TextField("E-Mail", _authorEmail);
            _authorUrl = EditorGUILayout.TextField("URL", _authorUrl);

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
                if (_selectedLicenseVersionIndex >= _selectedLicense.Versions.Length || _selectedLicenseVersionIndex < 0)
                {
                    _selectedLicenseVersionIndex = 0;
                }
                
                _selectedLicenseVersionIndex = EditorGUILayout.Popup(_selectedLicenseVersionIndex, _selectedLicense.Versions, GUILayout.Width(80));
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

        private void DrawCreateObjectButton()
        {
            bool isDisabled = string.IsNullOrWhiteSpace(_authorName) 
                              || !_objectTypeNameIsValid
                              || !_selectedGameObjectIsValid;

            EditorGUI.BeginDisabledGroup(isDisabled);

            if (GUILayout.Button("Create"))
            {
                try
                {
                    Create();
                }
                catch (Exception e)
                {
                    
                    EditorUtility.DisplayDialog("Error!", $"{e.Message}:\nCan't create object", "OK");
                    Debug.LogException(e);
                    Close();
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private void Create()
        {
            if (_gameObject == null)
            {
                EditorUtility.DisplayDialog("Error!", "GameObject can't be null.", "OK");

                return;
            }

            if (!ObjectBuildHelper.IsValidTypeName(_objectClassName, true))
            {
                return;
            }

            _localizedName = ObjectBuildHelper.EscapeString(_localizedName);
            _localizedNameRU = ObjectBuildHelper.EscapeString(_localizedNameRU);

            CreateObjectModel model = new CreateObjectModel()
            {
                Guid = Guid.NewGuid().ToString(),
                ObjectName = _objectClassName.Replace(" ", ""),
                LocalizedName = _localizedName,
                LocalizedNameRU = _localizedNameRU,
                MobileReady =  _mobileReady
            };
            model.ObjectFolder = "Assets/Objects/" + model.ObjectName;
            model.PrefabPath = model.ObjectFolder + "/" + model.ObjectName + ".prefab";

            if (Directory.Exists(model.ObjectFolder))
            {
                EditorUtility.DisplayDialog(model.ObjectName + " already exists!",
                    "Object with this name already exists!", "OK");
            }
            else
            {
                Debug.Log("Create folder " + model.ObjectFolder);
                Directory.CreateDirectory(model.ObjectFolder);

                Debug.Log("Create prefab " + _gameObject.name);

                CreatePrefab(_gameObject, model.PrefabPath, model.Guid,
                    _objectClassName, null, null,
                    false);
                CreateTags(model);

                model.ClassName = CreateCode(model);

                _modelsList = new List<CreateObjectModel>();
                _modelsList.Add(model);

                CreateObjectTempModel temp = new CreateObjectTempModel() {Objects = _modelsList, BuildNow = false};

                string jsonModels = JsonConvert.SerializeObject(temp, Formatting.None,
                    new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
                File.WriteAllText(CreateObjectTempModel.TempFilePath, jsonModels);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }

        private void CreateTags(CreateObjectModel model)
        {
            if (_tags == null)
                return;

            if (_tags.Length == 0)
                return;
            var tags = _tags.Split(',');
            List<string> tagList = new List<string>();

            foreach (string tag in tags)
            {
                int i = 0;
                tagList.Add(tag[i] == ' ' ? tag.Remove(i, 1) : tag);
            }

            File.WriteAllLines(model.ObjectFolder + "/tags.txt", tagList);
        }

        public static string CreateCode(CreateObjectModel model)
        {
            string typeCs = File.ReadAllText(Application.dataPath + @"/Varwin/Core/Templates/ObjectType.txt");
            string asmdef = File.ReadAllText(Application.dataPath + @"/Varwin/Core/Templates/ObjectAsmdef.txt");

            string cleanGuid = model.Guid.Replace("-", "");

            typeCs = typeCs.Replace("{%Object%}", model.ObjectName)
                .Replace("{%Object_Rus%}", model.LocalizedNameRU)
                .Replace("{%Guid%}", cleanGuid)
                .Replace("{%Object_En%}", model.LocalizedName);
            asmdef = asmdef.Replace("{%Object%}", model.ObjectName + "_" + cleanGuid);

            File.WriteAllText(model.ObjectFolder + "/" + model.ObjectName + ".cs", typeCs, Encoding.UTF8);
            File.WriteAllText(model.ObjectFolder + "/" + model.ObjectName + ".asmdef", asmdef, Encoding.UTF8);

            return model.ObjectName + "_" + cleanGuid;
        }


        public static GameObject CreatePrefab(GameObject gameObject, string localPath, string guid,
            string objectName, string classFullName, CreateObjectModel.AssetExtras licenseData,
            bool withMainClass = true)
        {
            if (AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject)))
            {
                if (EditorUtility.DisplayDialog("Are you sure?",
                    $"{objectName} prefab already exists. Do you want to overwrite it?", "Yes", "No"))
                {
                    return CreateNew(gameObject, localPath, guid,
                        objectName, classFullName, licenseData,
                        withMainClass,
                        true);
                }
            }

            else
            {
                Debug.Log("Converting " + gameObject.name + " to prefab.");

                return CreateNew(gameObject, localPath, guid,
                    objectName, classFullName, licenseData,
                    withMainClass);
            }

            return null;
        }

        private static GameObject CreateNew(GameObject obj, string localPath, string guid,
            string objectName, string classFullName, CreateObjectModel.AssetExtras licenseData,
            bool withMainClass = true,
            bool overwrite = false)
        {
            if (withMainClass)
            {
                CreateObjectUtils.AddComponent(obj, localPath, classFullName,
                    $"{objectName}");
            }

            var config = obj.GetComponent<VarwinObjectDescriptor>();

            if (config == null)
            {
                config = obj.AddComponent<VarwinObjectDescriptor>();

                if (config == null)
                {
                    obj = Instantiate(obj);
                    config = obj.AddComponent<VarwinObjectDescriptor>();
                }
            }

            if (string.IsNullOrWhiteSpace(config.Guid))
            {
                config.Guid = guid;
                config.RootGuid = guid;
            
                config.Name = objectName;
                config.Prefab = localPath;
            
                config.AuthorName = _authorName;
                config.AuthorEmail = _authorEmail;
                config.AuthorUrl = _authorUrl;

                config.LicenseCode = _licenseCode;
        
                config.BuiltAt = DateTimeOffset.Now.ToString();

                config.MobileReady = SdkSettings.MobileFeature.Enabled && _mobileReady;
            }

            if (licenseData != null)
            {
                Regex reg = new Regex(@"(.+)\s+\((.+)\)");
                var authorData = reg.Match(licenseData.Author);

                try
                {
                    config.AuthorName = authorData.Groups[1].Value;
                    config.AuthorUrl = authorData.Groups[2].Value;
                }
                catch
                {
                    Debug.LogWarning("cannot read author name and author url properties");
                }

                reg = new Regex(@"([a-zA-Z-]+)-([0-9\.]+)\s+\((.+)\)");
                var license = reg.Match(licenseData.License);

                try
                {
                    config.LicenseCode = license.Groups[1].Value.ToLower();
                }
                catch
                {
                    Debug.LogWarning("cannot read license code property");
                }
            }

            GameObject prefab;

            if (overwrite)
            {
                prefab = obj;
            }
            else
            {
                Object instanceRoot = PrefabUtility.InstantiatePrefab(obj);
                Object prefabInstance = PrefabUtility.GetCorrespondingObjectFromSource(obj);

                bool isPrefabInstance = (instanceRoot == null);
                
                if (isPrefabInstance)
                {
                    instanceRoot = PrefabUtility.InstantiatePrefab(prefabInstance);
                }
                
                bool success = false;                
                
                if (instanceRoot != null)
                {
                    
                    GameObject fixObject = null;

                    if (!isPrefabInstance)
                    {
                        fixObject = (GameObject)PrefabUtility.InstantiatePrefab(obj);;
                    }
                    
                    PrefabUtility.UnpackPrefabInstance((GameObject) instanceRoot,
                        PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    prefab = PrefabUtility.SaveAsPrefabAsset((GameObject) instanceRoot, localPath, out success);
                                       
                    if (!isPrefabInstance)
                    {
                        var objectDescriptor = fixObject.GetComponent<VarwinObjectDescriptor>();

                        if (objectDescriptor != null)
                        {
                            DestroyImmediate(objectDescriptor);
                            PrefabUtility.ApplyPrefabInstance(fixObject, InteractionMode.AutomatedAction);
                        }

                        DestroyImmediate(fixObject);
                    }
                    
                    DestroyImmediate(instanceRoot); 
                }
                else
                {
                    prefab = PrefabUtility.SaveAsPrefabAsset(obj, localPath, out success);
                    DestroyImmediate(obj);
                }

                EditorGUIUtility.PingObject(prefab);

                if (success)
                {
                    if (isPrefabInstance)
                    {
                        DestroyImmediate(obj);
                    }
                    
                    return prefab;
                }

                PrefabUtility.RevertPrefabInstance(obj, InteractionMode.AutomatedAction);

                Debug.LogError("Can not create prefab!");

                return null;
            }

            return prefab;
        }

        /// <summary>
        /// This callback runs when all scripts have been reloaded
        /// But - this window reloads too, so, we just load config from temp file
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            try
            {
                GetWindow<ImportModelsWindow>().Close();

                int createdNum = 0;

                if (File.Exists(CreateObjectTempModel.TempFilePath))
                {
                    string config = File.ReadAllText(CreateObjectTempModel.TempFilePath);

                    CreateObjectTempModel temp = JsonConvert.DeserializeObject<CreateObjectTempModel>(config);

                    File.Delete(CreateObjectTempModel.TempFilePath);

                    if (temp == null)
                    {
                        Debug.LogError("Temp build file is broken! Can't finish objects creation.");

                        return;
                    }

                    _modelsList = temp.Objects;
                    _buildNow = temp.BuildNow;

                    foreach (CreateObjectModel fileModel in _modelsList)
                    {
                        Debug.Log("Creating prefab for " + fileModel.ObjectName);

                        GameObject gameObject, prefab;

                        //Check if it's model import
                        if (fileModel.ModelImportPath != null)
                        {
                            if (fileModel.Skip)
                            {
                                continue;
                            }

                            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fileModel.ModelImportPath);

                            if (modelPrefab == null)
                            {
                                Debug.LogError("Can't create object " +
                                               fileModel.ObjectName +
                                               ". Imported file is incorrect: " +
                                               fileModel.Path);
                                Directory.Delete(fileModel.ObjectFolder, true);

                                continue;
                            }

                            gameObject = Instantiate(modelPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                            
                            //Calculate bounds
                            Bounds bounds = GetBounds(gameObject);
                            float maxBound = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                            float scale = fileModel.BiggestSideSize / maxBound;

                            gameObject.transform.localScale = Vector3.one * scale;
                            Rigidbody objectBody = gameObject.AddComponent<Rigidbody>();
                            objectBody.isKinematic = !fileModel.IsPhysicsOn;
                            objectBody.mass = fileModel.Mass;

                            InteractableObjectBehaviour objectBehaviour =
                                gameObject.AddComponent<InteractableObjectBehaviour>();
                            objectBehaviour.SetIsGrabbable(fileModel.IsGrabbable);
                            objectBehaviour.SetIsUsable(false);
                            objectBehaviour.SetIsTouchable(false);

                            CreateObjectUtils.AddObjectId(gameObject);
                            
                            MeshFilter[] meshes = gameObject.GetComponentsInChildren<MeshFilter>();

                            foreach (MeshFilter meshFilter in meshes)
                            {
                                MeshCollider collider = meshFilter.gameObject.AddComponent<MeshCollider>();
                                collider.sharedMesh = meshFilter.sharedMesh;
                                collider.convex = true;
                            }

                            if (meshes == null || meshes.Length == 0)
                            {
                                BoxCollider box = gameObject.AddComponent<BoxCollider>();

                                box.center = bounds.center;
                                box.size = bounds.size;
                            }

                            prefab = CreatePrefab(gameObject,
                                fileModel.PrefabPath,
                                fileModel.Guid,
                                fileModel.ObjectName,
                                fileModel.ClassName,
                                fileModel.Extras);
                        }
                        else
                        {
                            gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(fileModel.PrefabPath);

                            if (gameObject == null)
                            {
                                Debug.LogError("Can't create " +
                                               fileModel.ObjectName +
                                               ": no prefab and no model. How did it happen? Please try again.");

                                return;
                            }

                            CreateNew(gameObject,
                                fileModel.PrefabPath,
                                fileModel.Guid,
                                fileModel.ObjectName,
                                fileModel.ClassName,
                                fileModel.Extras,
                                true,
                                true);
                        }

                        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                        if (fileModel.ModelImportPath != null)
                        {
                            DestroyImmediate(gameObject);
                        }

                        createdNum++;

                        gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(fileModel.PrefabPath);

                        var instance = PrefabUtility.InstantiatePrefab(gameObject);
                        instance.name = instance.name.Replace("(Clone)", "");
                        EditorGUIUtility.PingObject(instance);
                    }

                    EditorUtility.DisplayDialog("Done!", createdNum + " objects were created!", "OK");

                    if (temp.BuildNow)
                    {
                        ObjectsBuilderWindow.BuildObjects(_modelsList.ToArray());
                    }


                    GetWindow<CreateObjectWindow>().Close();
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!", $"{e.Message}:\nProblem when creating an objects", "OK");
                Debug.LogException(e);
            }
        }

        #region BOUNDS HELPERS

        private static Bounds GetBounds(GameObject obj)
        {
            Bounds bounds;
            Renderer childRender;
            bounds = GetRenderBounds(obj);

            if (bounds.extents.x == 0)
            {
                bounds = new Bounds(obj.transform.position, Vector3.zero);

                foreach (Transform child in obj.transform)
                {
                    childRender = child.GetComponent<Renderer>();

                    if (childRender)
                    {
                        bounds.Encapsulate(childRender.bounds);
                    }
                    else
                    {
                        bounds.Encapsulate(GetBounds(child.gameObject));
                    }
                }
            }

            return bounds;
        }

        private static Bounds GetRenderBounds(GameObject obj)
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            Renderer render = obj.GetComponent<Renderer>();

            if (render != null)
            {
                return render.bounds;
            }

            return bounds;
        }

        #endregion
    }
}
