#define VRMAKER

#if VRMAKER
using Ionic.Zip;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DesperateDevs.Utils;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Varwin.Data;
using Varwin.Models.Data;
using Varwin.Public;


namespace Varwin.Editor
{
    public class ObjectBuildDescription : IJsonSerializable
    {
        public string ObjectName;
        public string ObjectGuid;
        public string PrefabPath;

        [JsonIgnore]
        public VarwinObjectDescriptor ContainedObjectDescriptor;

        public string FolderPath;

        public string ConfigBlockly;
        public string ConfigAssetBundle;

        public string TagsPath
        {
            get { return FolderPath + "/tags.txt"; }
        }

        public string IconPath
        {
            get { return FolderPath + "/icon.png"; }
        }

        public ObjectBuildDescription()
        {
        }

        public ObjectBuildDescription(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            ObjectName = varwinObjectDescriptor.Name;
            ObjectGuid = varwinObjectDescriptor.RootGuid;
            PrefabPath = varwinObjectDescriptor.Prefab;
            FolderPath = Path.GetDirectoryName(varwinObjectDescriptor.Prefab);
            
            ContainedObjectDescriptor =
                AssetDatabase.LoadAssetAtPath<GameObject>(varwinObjectDescriptor.Prefab).GetComponent<VarwinObjectDescriptor>() ??
                varwinObjectDescriptor;
            ConfigBlockly = varwinObjectDescriptor.ConfigBlockly;
            ConfigAssetBundle = varwinObjectDescriptor.ConfigAssetBundle;
        }
    }

    public class ObjectsBuilderWindow : EditorWindow
    {
        enum ObjectBuilderState
        {
            Preparing,
            GeneratingWrapper,
            WaitingForCompile,
            CreatingIcons,
            CreatingBundleConfigs,
            BuildingAssetBundles,
            CreatingFiles,
            OpeningFolder
        }

        private class AssemblyDefinitionData
        {
            public string name;
            public string[] references;
            public string[] includePlatforms;
            public string[] excludePlatforms;
        }

        private float _compileWaitingTimeout = 0.0f;

        private List<ObjectBuildDescription> _objectsToBuild;
        private ObjectBuilderState _state;
        private int _currentObjectIndex;
        private string _currentStatusString;

        private static int MAX_BAD_ATTEMPTS = 150;
        private static string TEMP_FILE_NAME = "build_list_tmp.txt";
        private int _bad_attempts;

        private static string DEFAULT_ICON_PATH = "Assets/Varwin/Core/Varwin/Editor/Resources/DefaultObjectIcon.png";

        private bool _stopAfterOneTry = false;

        public event EventHandler ObjectBuilt;

        [MenuItem("VARWIN SDK/Build All Objects", false, 2)]
        public static void BuildAll()
        {
            if (!EditorUtility.DisplayDialog("Confirm Build All Objects",
                "Building all objects might take some time, do you want to proceed?",
                "Yes",
                "Cancel"))
            {
                return;
            }

            ObjectsBuilderWindow window = GetWindow<ObjectsBuilderWindow>(true, "Building all objects", true);
            window.minSize = new Vector2(350, 110);
            window.Show();
            window.RunBuildAllObjects();
        }


        public static void BuildVarwinObject(VarwinObjectDescriptor varwinObj)
        {
            ObjectsBuilderWindow window =
                GetWindow<ObjectsBuilderWindow>(true, "Building object: " + varwinObj.Name, true);
            window.minSize = new Vector2(350, 110);
            window.Show();
            window.RunBuildVarwinObject(varwinObj);
        }


        public static void BuildObjects(CreateObjectModel[] objects)
        {
            ObjectsBuilderWindow window = GetWindow<ObjectsBuilderWindow>(true, "Building objects", true);
            window.minSize = new Vector2(350, 110);
            window.Show();
            window.RunBuildObjects(objects);
        }

        public static void BuildOnlyWrapper(VarwinObjectDescriptor varwinObj)
        {
            ObjectsBuilderWindow window =
                GetWindow<ObjectsBuilderWindow>(true, "Building wrapper for " + varwinObj.Name, true);
            window.minSize = new Vector2(350, 110);
            window.Show();
            window.BuildOnlyWrapper(new ObjectBuildDescription(varwinObj));
        }

        public static void BuildOnlyIcon(VarwinObjectDescriptor varwinObj)
        {
            ObjectsBuilderWindow window =
                GetWindow<ObjectsBuilderWindow>(true, "Building icon for " + varwinObj.Name, true);
            window.minSize = new Vector2(350, 110);
            window.Show();
            window.BuildOnlyIcon(new ObjectBuildDescription(varwinObj));
        }


        public static void BuildOnlyConfigs(VarwinObjectDescriptor varwinObj)
        {
            ObjectsBuilderWindow window =
                GetWindow<ObjectsBuilderWindow>(true, "Building configs for " + varwinObj.Name, true);
            window.minSize = new Vector2(350, 110);
            window.Show();
            window.BuildOnlyConfig(new ObjectBuildDescription(varwinObj));
        }


        public static void BuildOnlyBundle(VarwinObjectDescriptor varwinObj)
        {
            ObjectsBuilderWindow window =
                GetWindow<ObjectsBuilderWindow>(true, "Building bundle for " + varwinObj.Name, true);
            window.minSize = new Vector2(350, 110);
            window.Show();
            window.BuildOnlyAssetBundle(new ObjectBuildDescription(varwinObj));
        }


        public static void BuildOnlyZip(VarwinObjectDescriptor varwinObj)
        {
            ObjectsBuilderWindow window =
                GetWindow<ObjectsBuilderWindow>(true, "Building VWO for " + varwinObj.Name, true);
            window.minSize = new Vector2(350, 110);
            window.Show();
            window.BuildOnlyZip(new ObjectBuildDescription(varwinObj));
        }


        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(
                "Please wait until all objects are built.\nThis window will be closed and the folder\nwith your objects will be opened automatically.",
                EditorStyles.boldLabel, GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(_currentStatusString ?? "Compiling scripts...", EditorStyles.largeLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        private void RunBuildAllObjects()
        {
            try
            {
                _objectsToBuild = new List<ObjectBuildDescription>();
                _state = ObjectBuilderState.Preparing;
                _currentStatusString = "Preparing objects...";

                var objectsPath = "Assets/Objects";
                if (!Directory.Exists(objectsPath))
                {
                    Directory.CreateDirectory(objectsPath);
                    try
                    {
                        Directory.CreateDirectory(objectsPath);
                    }
                    catch
                    {
                        Debug.LogError("Can't create directory \"" + objectsPath + "\"");
                        EditorUtility.DisplayDialog("Can't create directory",
                            "Can't create directory \"" + objectsPath + "\"",
                            "OK");
                        return;
                    }
                }
                
                foreach (string objectDir in Directory.EnumerateDirectories("Assets/Objects"))
                {
                    foreach (string objectPath in Directory.EnumerateFiles(objectDir, "*.prefab"))
                    {
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(objectPath);

                        if (prefab != null)
                        {
                            VarwinObjectDescriptor objToBuild = prefab.GetComponent<VarwinObjectDescriptor>();

                            if (objToBuild != null)
                            {
                                PreBuild(prefab);
                                
                                //Finally, it's our prefab to build
                                //Debug.Log("Building object " + objToBuild.Name);

                                ObjectBuildDescription descr = new ObjectBuildDescription()
                                {
                                    ObjectName = objToBuild.Name,
                                    ObjectGuid = objToBuild.RootGuid,
                                    PrefabPath = objectPath,
                                    FolderPath = objectDir,
                                    ContainedObjectDescriptor = objToBuild
                                };

                                _objectsToBuild.Add(descr);
                            }
                        }
                    }
                }

                GenerateWrappers();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!", $"{e.Message}:\nProblem when run build all objects", "OK");
                Debug.LogException(e);
                Close();
            }
        }


        private void RunBuildVarwinObject(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            try
            {
                _objectsToBuild = new List<ObjectBuildDescription>();
                _state = ObjectBuilderState.Preparing;
                _currentStatusString = "Preparing objects...";

                _objectsToBuild.Add(new ObjectBuildDescription(varwinObjectDescriptor));

                GenerateWrappers();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!", $"{e.Message}:\nProblem when run build varwin object \"{varwinObjectDescriptor.Name}\"", "OK");
                Debug.LogException(e);
                Close();
            }
        }


        private void RunBuildObjects(CreateObjectModel[] objects)
        {
            try
            {
                _objectsToBuild = new List<ObjectBuildDescription>();
                _state = ObjectBuilderState.Preparing;
                _currentStatusString = "Preparing objects...";

                foreach (CreateObjectModel model in objects)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(model.PrefabPath);

                    if (prefab != null)
                    {
                        VarwinObjectDescriptor objToBuild = prefab.GetComponent<VarwinObjectDescriptor>();

                        if (objToBuild != null)
                        {
                            PreBuild(prefab);
                            
                            ObjectBuildDescription descr = new ObjectBuildDescription()
                            {
                                ObjectName = objToBuild.Name,
                                ObjectGuid = objToBuild.RootGuid,
                                PrefabPath = model.PrefabPath,
                                FolderPath = model.ObjectFolder,
                                ContainedObjectDescriptor = objToBuild
                            };
                            
                            _objectsToBuild.Add(descr);
                        }
                    }
                }

                GenerateWrappers();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!", $"{e.Message}:\nProblem when build objects", "OK");
                Debug.LogException(e);
                Close();
            }
        }

        private void PreBuild(GameObject prefab)
        {
            var go = Instantiate(prefab);
            var varwinObjectDescriptor = go.GetComponent<VarwinObjectDescriptor>();
            varwinObjectDescriptor.PreBuild();
            CreateObjectUtils.ApplyPrefabInstanceChanges(go);

            var vods = FindObjectsOfType<VarwinObjectDescriptor>();
            foreach (var vod in vods)
            {
                if (vod.Guid == varwinObjectDescriptor.Guid)
                {
                    CreateObjectUtils.RevertPrefabInstanceChanges(vod.gameObject);
                }
            }
            
            CreateObjectUtils.SafeDestroy(go);
        }


        #region SEPARATE BUILDS

        private void BuildOnlyWrapper(ObjectBuildDescription buildObject)
        {
            _state = ObjectBuilderState.GeneratingWrapper;
            _stopAfterOneTry = true;
            _objectsToBuild = new List<ObjectBuildDescription>();
            _objectsToBuild.Add(buildObject);
        }

        private void BuildOnlyIcon(ObjectBuildDescription buildObject)
        {
            _state = ObjectBuilderState.CreatingIcons;
            _stopAfterOneTry = true;
            _objectsToBuild = new List<ObjectBuildDescription>();
            _objectsToBuild.Add(buildObject);
        }


        private void BuildOnlyConfig(ObjectBuildDescription buildObject)
        {
            _state = ObjectBuilderState.CreatingBundleConfigs;
            _stopAfterOneTry = true;
            _objectsToBuild = new List<ObjectBuildDescription>();
            _objectsToBuild.Add(buildObject);
        }


        private void BuildOnlyAssetBundle(ObjectBuildDescription buildObject)
        {
            _state = ObjectBuilderState.BuildingAssetBundles;
            _stopAfterOneTry = true;
            _objectsToBuild = new List<ObjectBuildDescription>();
            _objectsToBuild.Add(buildObject);
        }


        private void BuildOnlyZip(ObjectBuildDescription buildObject)
        {
            _state = ObjectBuilderState.CreatingFiles;
            _stopAfterOneTry = true;
            _objectsToBuild = new List<ObjectBuildDescription>();
            _objectsToBuild.Add(buildObject);
        }

        #endregion


        private void GoToTheNextBuildState()
        {
            switch (_state)
            {
                case ObjectBuilderState.Preparing: _state = ObjectBuilderState.GeneratingWrapper; break;
                case ObjectBuilderState.GeneratingWrapper: _state = ObjectBuilderState.WaitingForCompile; break;
                case ObjectBuilderState.WaitingForCompile: _state = ObjectBuilderState.CreatingIcons; break;
                case ObjectBuilderState.CreatingIcons: _state = ObjectBuilderState.CreatingBundleConfigs; break;
                case ObjectBuilderState.CreatingBundleConfigs: _state = ObjectBuilderState.BuildingAssetBundles; break;
                case ObjectBuilderState.BuildingAssetBundles: _state = ObjectBuilderState.CreatingFiles; break;
                case ObjectBuilderState.CreatingFiles: _state = ObjectBuilderState.OpeningFolder; break;
                case ObjectBuilderState.OpeningFolder: //Do some magic and close this window
                                                       break;
            }
        }

        private void Update()
        {
            try
            {
                if (EditorApplication.isCompiling)
                {
                    return;
                }

                if (_objectsToBuild == null)
                {
                    TryToContinueBuild();
                    
                    return;
                }

                if (_state == ObjectBuilderState.WaitingForCompile)
                {
                    _currentStatusString = "Compiling scripts...";
                    _compileWaitingTimeout += Time.deltaTime;

                    if (_compileWaitingTimeout >= 5.0f)
                    {
                        GoToTheNextBuildState();
                    }
                    return;
                }
                
                if (_state == ObjectBuilderState.GeneratingWrapper)
                {
                    GenerateWrappers();
                }
                else if (_state == ObjectBuilderState.CreatingIcons)
                {
                    if (!CreateIcons())
                    {
                        return;
                    }
                }
                else if (_state == ObjectBuilderState.CreatingBundleConfigs)
                {
                    CreateBundlesConfig();
                }
                else if (_state == ObjectBuilderState.BuildingAssetBundles)
                {
                    BuildAssetBundles();
                }
                else if (_state == ObjectBuilderState.CreatingFiles)
                {
                    CreateFiles();

                    return;
                }
                else if (_state == ObjectBuilderState.OpeningFolder)
                {
                    string bakedObjects = Application.dataPath.Replace("Assets", "BakedObjects").Replace(@"/", @"\");
                    Debug.Log("Opening objects folder: " + bakedObjects);
                    File.Delete(TEMP_FILE_NAME);
                    System.Diagnostics.Process.Start("explorer.exe", bakedObjects);
                    Cleanup();
                    Close();
                    
                    return;
                }

                if (_stopAfterOneTry)
                {
                    Cleanup();
                    _objectsToBuild = null;
                    Close();
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!", $"{e.Message}:\nProblem when building objects", "OK");
                Debug.LogException(e);
                Close();
            }
        }

        private void Cleanup()
        {
            foreach (ObjectBuildDescription objectBuildDescription in _objectsToBuild)
            {
                WrapperGenerator.RemoveWrapperIfNeeded(objectBuildDescription.ContainedObjectDescriptor);
            }
        }

        private void GenerateWrappers()
        {
            if (_state != ObjectBuilderState.GeneratingWrapper)
            {
                _state = ObjectBuilderState.GeneratingWrapper;
                _currentObjectIndex = 0;
            }
            
            if (_currentObjectIndex >= _objectsToBuild.Count)
            {
                GoToTheNextBuildState();
                _currentObjectIndex = 0;
                
                string jsonModels = JsonConvert.SerializeObject(_objectsToBuild.ToArray(), Formatting.None,
                    new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

                File.WriteAllText(TEMP_FILE_NAME, jsonModels);
                
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                
                return;
            }
            
            _currentStatusString = "Generating wrappers...";
            
            ObjectBuildDescription buildObject = _objectsToBuild[_currentObjectIndex];
            WrapperGenerator.GenerateWrapper(buildObject.ContainedObjectDescriptor);

            _currentObjectIndex++;
        }

        private bool CreateIcons()
        {
            if (_state != ObjectBuilderState.CreatingIcons)
            {
                _state = ObjectBuilderState.CreatingIcons;
                _currentObjectIndex = 0;
                AssetPreview.SetPreviewTextureCacheSize(_objectsToBuild.Count + 1);
                _bad_attempts = 0;
            }

            if (_currentObjectIndex >= _objectsToBuild.Count)
            {
                GoToTheNextBuildState();
                _currentObjectIndex = 0;

                return true;
            }

            if (_currentObjectIndex == 0)
            {
                AssetPreview.SetPreviewTextureCacheSize(_objectsToBuild.Count + 1);
                _bad_attempts = 0;
            }

            ObjectBuildDescription buildObject = _objectsToBuild[_currentObjectIndex];
            
            if (buildObject.ContainedObjectDescriptor == null)
            {
                buildObject.ContainedObjectDescriptor = AssetDatabase.LoadAssetAtPath<GameObject>(buildObject.PrefabPath).GetComponent<VarwinObjectDescriptor>();
            }
            
            if (buildObject.ContainedObjectDescriptor.Icon != null)
            {
                GoToTheNextBuildState();
                _currentObjectIndex = 0;
                return true;
            }

            _currentStatusString = "Creating icon for " + buildObject.ObjectName;

            EditorGUIUtility.PingObject(buildObject.ContainedObjectDescriptor.gameObject);

            int instanceId = buildObject.ContainedObjectDescriptor.gameObject.GetInstanceID();
            
            Texture2D texture2D = AssetPreview.GetAssetPreview(buildObject.ContainedObjectDescriptor.gameObject);
            if (texture2D == null)
            {
                Repaint();
                _bad_attempts++;

                if (_bad_attempts >= MAX_BAD_ATTEMPTS)
                {
                    if (File.Exists(buildObject.IconPath))
                    {
                        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(buildObject.IconPath);

                        if (icon != null)
                        {
                            buildObject.ContainedObjectDescriptor.Icon = icon;
                            Debug.LogWarning("Existent icon was used for " + buildObject.ObjectName);

                            _bad_attempts = 0;
                            _currentObjectIndex++;

                            return true;
                        }
                    }

                    Debug.LogWarning("Can't create icon for " + buildObject.ObjectName + ". Using default icon.");
                    Texture2D defaultIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(DEFAULT_ICON_PATH);
                    var bytes = defaultIcon.EncodeToPNG();
                    File.WriteAllBytes(buildObject.IconPath, bytes);
                    AssetDatabase.ImportAsset(buildObject.IconPath);
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(buildObject.IconPath);
                    buildObject.ContainedObjectDescriptor.Icon = tex;

                    _bad_attempts = 0;
                    _currentObjectIndex++;

                    return true;
                }

                return false;
            }

            var iconBytes = texture2D.EncodeToPNG();
            File.WriteAllBytes(buildObject.IconPath, iconBytes);
            AssetDatabase.ImportAsset(buildObject.IconPath);
            Texture2D te = AssetDatabase.LoadAssetAtPath<Texture2D>(buildObject.IconPath);
            buildObject.ContainedObjectDescriptor.Icon = te;

            Debug.Log("Icon was created for " + buildObject.ObjectName + "!");

            _currentObjectIndex++;
            
            return true;
        }


        #region CONFIGS CREATION

        private void CreateBundlesConfig()
        {
            if (_currentObjectIndex >= _objectsToBuild.Count)
            {
                GoToTheNextBuildState();
                _currentObjectIndex = 0;

                return;
            }

            ObjectBuildDescription buildObject = _objectsToBuild[_currentObjectIndex];

            if (buildObject.ContainedObjectDescriptor == null)
            {
                buildObject.ContainedObjectDescriptor = AssetDatabase.LoadAssetAtPath<GameObject>(buildObject.PrefabPath).GetComponent<VarwinObjectDescriptor>();
            }
            
            GameObject gameObject = buildObject.ContainedObjectDescriptor.gameObject;

            if (gameObject == null)
            {
                Debug.LogError("Can't create object " + buildObject.ObjectName + ": no game object found!");
                _objectsToBuild.RemoveAt(_currentObjectIndex);

                return;
            }

            _currentStatusString = "Creating configurations for " + buildObject.ObjectName;
            Repaint();

            MonoBehaviour[] myComponents = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
            int wrappers = 0;

            foreach (MonoBehaviour myComp in myComponents)
            {
                if (myComp == null)
                {
                    continue;
                }

                Type myObjectType = myComp.GetType();

                if (myObjectType.Assembly.ManifestModule.Name == "VarwinCore.dll" && myObjectType != typeof(JointPoint))
                {
                    continue;
                }

                if (myObjectType.Assembly.ManifestModule.Name.Contains("UnityEngine"))
                {
                    continue;
                }

                if (myObjectType.ImplementsInterface<IWrapperAware>())
                {
                    Wrapper wrapper = gameObject.GetComponentInChildren<IWrapperAware>().Wrapper();

                    buildObject.ContainedObjectDescriptor.ConfigBlockly =
                        BlocklyBuilder.CreateBlocklyConfig(wrapper, myObjectType, buildObject);
                    buildObject.ConfigBlockly = buildObject.ContainedObjectDescriptor.ConfigBlockly;
                    wrappers++;
                }
            }

            if (wrappers == 0)
            {
                EditorUtility.DisplayDialog("Message",
                    $"IWrapperAware not found on " + buildObject.ContainedObjectDescriptor.Name + "! Please add VarwinObject inheritor script to object.",
                    "OK");
                _objectsToBuild.RemoveAt(_currentObjectIndex);

                return;
            }

            if (wrappers > 1)
            {
                EditorUtility.DisplayDialog("Message",
                    $"Can't build " + buildObject.ContainedObjectDescriptor.Name + ": Only one class with IWapperAware interface can be added to an object",
                    "OK");
                _objectsToBuild.RemoveAt(_currentObjectIndex);

                return;
            }

            CreateAssetBundleConfig(gameObject, buildObject);
            
            _currentObjectIndex++;
        }


        private void CreateAssetBundleConfig(GameObject go, ObjectBuildDescription buildObject)
        {
            AssetInfo assetInfo =
                new AssetInfo {AssetName = go.name, Assembly = GetDLLNamesFromAsmdef(buildObject)};

            string jsonConfig = JsonConvert.SerializeObject(assetInfo, Formatting.None,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            buildObject.ContainedObjectDescriptor.ConfigAssetBundle = jsonConfig;
            buildObject.ConfigAssetBundle = jsonConfig;
        }


        private List<string> GetDLLNamesFromAsmdef(ObjectBuildDescription buildObject)
        {
            string asmdefFilePath = buildObject.FolderPath + "/" + buildObject.ObjectName + ".asmdef";
            var dllNamesList = new List<string>(0);

            if (!File.Exists(asmdefFilePath))
            {
                Debug.LogError("No .asmdef file found for " + buildObject.ObjectName + " at " + asmdefFilePath);

                return dllNamesList;
            }

            string asmdefText = File.ReadAllText(asmdefFilePath);
            AssemblyDefinitionData asmdef = JsonConvert.DeserializeObject<AssemblyDefinitionData>(asmdefText);
            dllNamesList = new List<string>(asmdef.references.Length - 1);
            dllNamesList.Add(buildObject.ObjectName + "_" + buildObject.ObjectGuid.Replace("-", "") + ".dll");

            foreach (string dllName in asmdef.references)
            {
                if (dllName != "VarwinCore")
                {
                    dllNamesList.Add(dllName + ".dll");
                }
            }

            return dllNamesList;
        }

        #endregion


        private void BuildAssetBundles()
        {
            try
            {
                string assetBundleDirectory = Application.dataPath.Replace("Assets", "AssetBundles");
                if (!Directory.Exists(assetBundleDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(assetBundleDirectory);
                    }
                    catch
                    {
                        Debug.LogError("Can't create directory \"" + assetBundleDirectory + "\"");
                        EditorUtility.DisplayDialog("Can't create directory",
                            "Can't create directory \"" + assetBundleDirectory + "\"",
                            "OK");
                        return;
                    }
                }

                _currentStatusString = "Building asset bundles...";
                Repaint();

                string jsonModels = JsonConvert.SerializeObject(_objectsToBuild.ToArray(), Formatting.None,
                    new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

                File.WriteAllText(TEMP_FILE_NAME, jsonModels);

                Debug.Log("Build asset bundles to " + assetBundleDirectory);
                DateTime start = DateTime.Now;

                var buildTargets = new List<BuildTarget>()
                {
                    BuildTarget.StandaloneWindows64, 
                    BuildTarget.Android 
                };
                BuildAssetBundles(buildTargets, assetBundleDirectory);
                
                TimeSpan time = DateTime.Now - start;

                Debug.Log("Building asset bundles to " +
                          assetBundleDirectory +
                          " completed in " +
                          time.Seconds +
                          " sec.");

                GoToTheNextBuildState();
                _currentObjectIndex = 0;
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!", $"{e.Message}:\nProblem when building asset bundles", "OK");
                Debug.LogException(e);
                Close();
            }
        }

        private void BuildAssetBundles(List<BuildTarget> buildTargets, string folder)
        {
            var currentTarget = EditorUserBuildSettings.activeBuildTarget;
            if (buildTargets.Contains(currentTarget))
            {
                var buildHandler = GetBuildBundlesHandler(currentTarget);
                buildHandler?.Invoke(folder);
                buildTargets.Remove(currentTarget);
            }

            foreach (var target in buildTargets)
            {
                var buildHandler = GetBuildBundlesHandler(target);
                buildHandler?.Invoke(folder);
            }
        }

        private Action<string> GetBuildBundlesHandler(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows64: 
                    return BuildWindowsBundles;
                case BuildTarget.Android: 
                    return BuildAndroidBundles;
                default:
                    Debug.LogError($"Build Target \"{buildTarget}\" is not supported");
                    return null;
            }
        }
        
        private void BuildWindowsBundles(string folder)
        {
            BuildTarget activeBuildTarget = BuildTarget.StandaloneWindows64;

            List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();

            foreach (ObjectBuildDescription obj in _objectsToBuild)
            {
                AssetBundleBuild assetBundleBuild = default;

                assetBundleBuild.assetNames = new string[] {obj.PrefabPath};

                assetBundleBuild.assetBundleName = $"{obj.ObjectName}_{obj.ContainedObjectDescriptor.RootGuid.Replace("-", "")}";

                bundles.Add(assetBundleBuild);
            }

            BuildPipeline.BuildAssetBundles(folder,
                bundles.ToArray(),
                0,
                activeBuildTarget);
        }

        private void BuildAndroidBundles(string folder)
        {
            if (!SdkSettings.MobileFeature.Enabled)
            {
                return;
            }
            
            BuildTarget activeBuildTarget = BuildTarget.Android;

            List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();

            foreach (ObjectBuildDescription obj in _objectsToBuild)
            {
                if (!obj.ContainedObjectDescriptor.MobileReady)
                {
                    continue;
                }

                AssetBundleBuild assetBundleBuild = default;

                assetBundleBuild.assetNames = new string[] {obj.PrefabPath};

                assetBundleBuild.assetBundleName = $"android_{obj.ObjectName}_{obj.ContainedObjectDescriptor.RootGuid.Replace("-", "")}";

                bundles.Add(assetBundleBuild);
            }

            BuildPipeline.BuildAssetBundles(folder,
                bundles.ToArray(),
                0,
                activeBuildTarget);
        }

        #region ZIPPING FILES

        private void CreateFiles()
        {
            string assetBundleDirectory = Application.dataPath.Replace("Assets", "AssetBundles");
            string bakedObjectDirectory = Application.dataPath.Replace("Assets", "BakedObjects");
            string dllFolder = Application.dataPath.Replace("Assets", "Library") + $"/ScriptAssemblies";
            
            string tempBundleFile = bakedObjectDirectory + "/bundle";
            string tempBundleManifestFile = bakedObjectDirectory + "/bundle.manifest";
            string tempBundleJsonFile = bakedObjectDirectory + "/bundle.json";
            string tempBundleIconFile = bakedObjectDirectory + "/bundle.png";
            string tempInstallJsonFile = bakedObjectDirectory + "/install.json";
            List<string> tempDllFiles = new List<string>();
            List<string> newDllPaths = new List<string>();
            List<string> filesToZip = new List<string>();

            string tempAndroidBundleFile = bakedObjectDirectory + "/android_bundle";
            string tempAndroidBundleManifestFile = bakedObjectDirectory + "/android_bundle.manifest";
            
            try
            {
                if (_objectsToBuild == null)
                {
                    return;
                }

                if (_currentObjectIndex >= _objectsToBuild.Count)
                {
                    GoToTheNextBuildState();
                    _currentObjectIndex = 0;

                    return;
                }

                if (EditorApplication.isCompiling)
                {
                    return;
                }

                ObjectBuildDescription buildObject = _objectsToBuild[_currentObjectIndex];

                _currentStatusString = "Baking object to " + bakedObjectDirectory;
                Debug.Log("Baking object to " + bakedObjectDirectory);
                Repaint();

                if (!Directory.Exists(bakedObjectDirectory))
                {

                    try
                    {
                        Directory.CreateDirectory(bakedObjectDirectory);
                    }

                    catch
                    {
                        Debug.LogError("Can't create object directory for " + buildObject.ObjectName);

                        EditorUtility.DisplayDialog("Can't create objects",
                            "Please, close all your explorer windows and press OK to try again.",
                            "OK");

                        return;
                    }
                }

                var dllNamesList = GetDLLNamesFromAsmdef(buildObject);
                tempDllFiles = new List<string>(dllNamesList.Count);
                foreach (string dllName in dllNamesList)
                {
                    tempDllFiles.Add(dllFolder + $"/" + dllName);
                }

                string bundlePath = assetBundleDirectory +
                                    "/" +
                                    buildObject.ObjectName.ToLower() +
                                    "_" +
                                    buildObject.ObjectGuid.Replace("-", "");

                string bundleManifestPath = assetBundleDirectory +
                                            "/" +
                                            buildObject.ObjectName.ToLower() +
                                            "_" +
                                            buildObject.ObjectGuid.Replace("-", "") +
                                            ".manifest";
                
                string androidBundlePath = assetBundleDirectory +
                                    "/android_" +
                                    buildObject.ObjectName.ToLower() +
                                    "_" +
                                    buildObject.ObjectGuid.Replace("-", "");

                string androidBundleManifestPath = assetBundleDirectory +
                                            "/android_" +
                                            buildObject.ObjectName.ToLower() +
                                            "_" +
                                            buildObject.ObjectGuid.Replace("-", "") +
                                            ".manifest";
                
                newDllPaths = new List<string>(tempDllFiles.Count);
                foreach (string tempDllFile in tempDllFiles)
                {
                    string dllPath = tempDllFile;

                    string newDllPath = bakedObjectDirectory + $"/" + Path.GetFileName(tempDllFile);
                    newDllPaths.Add(newDllPath);

                    if (!File.Exists(tempDllFile))
                    {
                        string oldSupportedFile =
                            tempDllFile.Replace("_" + buildObject.ObjectGuid.Replace("-", ""), "");

                        if (!File.Exists(oldSupportedFile))
                        {
                            Debug.LogError("Can't find " + tempDllFile);

                            EditorUtility.DisplayDialog(buildObject.ObjectName + " Error!",
                                "Can't build object. File hasn't been compiled: " +
                                tempDllFile +
                                ". Please check your assembly name.", "OK");
                            _currentObjectIndex++;

                            return;
                        }
                        else
                        {
                            dllPath = oldSupportedFile;

                            Debug.LogWarning(
                                "WARNING! Old object detected. Please, change your asmdef to <objectname>_<objectguidwithoutdashes> format! Now using file " +
                                dllPath);
                        }
                    }

                    if (File.Exists(newDllPath))
                    {
                        File.Delete(newDllPath);
                        Debug.LogWarning($"WARNING! Old DLL detected. Dll file delete {dllPath}");
                    }

                    File.Copy(dllPath, newDllPath);
                }

                File.Copy(bundlePath, tempBundleFile);
                File.Copy(bundleManifestPath, tempBundleManifestFile);

                if (SdkSettings.MobileFeature.Enabled && buildObject.ContainedObjectDescriptor.MobileReady)
                {
                    File.Copy(androidBundlePath, tempAndroidBundleFile);
                    File.Copy(androidBundleManifestPath, tempAndroidBundleManifestFile);
                }

                if (File.Exists(buildObject.IconPath))
                {
                    File.Copy(buildObject.IconPath, tempBundleIconFile);
                }

                File.WriteAllText(tempInstallJsonFile, buildObject.ConfigBlockly);
                File.WriteAllText(tempBundleJsonFile, buildObject.ConfigAssetBundle);

                filesToZip = new List<string>()
                {
                    tempBundleFile,
                    tempBundleIconFile,
                    tempBundleJsonFile,
                    tempBundleManifestFile,
                    tempInstallJsonFile
                };
                
                if (SdkSettings.MobileFeature.Enabled && buildObject.ContainedObjectDescriptor.MobileReady)
                {
                    filesToZip.Add(tempAndroidBundleFile);
                    filesToZip.Add(tempAndroidBundleManifestFile);
                }

                filesToZip.AddRange(newDllPaths);

                ZipFiles(filesToZip, bakedObjectDirectory + $"/{buildObject.ObjectName}.vwo");

                ObjectBuilt?.Invoke(this, EventArgs.Empty);

                _currentObjectIndex++;
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!", $"{e.Message}:\nProblem when creating files", "OK");
                Debug.LogException(e);
                Close();
            }
            finally
            {
                var files = new List<string>(filesToZip);
                files.AddRange(newDllPaths);
                foreach (var file in filesToZip)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception e)
                    {
                        EditorUtility.DisplayDialog("Error!", $"Can't delete \"{file}\"", "OK");
                        Debug.LogException(e);
                    }
                }
            }
        }

        
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            //You're a useless scum!
        }

        private void TryToContinueBuild()
        {
            if (_objectsToBuild == null)
            {
                if (File.Exists(TEMP_FILE_NAME))
                {
                    string config = File.ReadAllText(TEMP_FILE_NAME);
                    ObjectBuildDescription[] temp = JsonConvert.DeserializeObject<ObjectBuildDescription[]>(config);

                    if (temp == null)
                    {
                        Debug.LogError("Temp build file is broken! Can't finish objects creation.");

                        return;
                    }

                    _objectsToBuild = new List<ObjectBuildDescription>(temp);
                    _state = ObjectBuilderState.CreatingIcons;
                }
            }

        }


        private void ZipFiles(List<string> files, string zipFilePath)
        {
#if VRMAKER
            try
            {
                using (ZipFile loanZip = new ZipFile())
                {
                    loanZip.AddFiles(files, false, "");
                    loanZip.Save(zipFilePath);
                }

                foreach (string file in files)
                {
                    File.Delete(file);
                }

                Debug.Log($"Zip was created!");
                //  
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!", $"{e.Message}:\nCan not zip files!", "OK");
                Debug.LogException(e);
                Close();
            }
#endif
        }

        #endregion
    }
}
