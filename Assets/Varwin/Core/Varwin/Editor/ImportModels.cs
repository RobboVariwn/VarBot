using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GLTF;
using GLTF.Schema;
using Newtonsoft.Json.Linq;
using Sketchfab;
using UnityGLTF;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace Varwin.Editor
{
    public class ImportModelsWindow : EditorWindow
    {
        private static ImportModelsWindow _window;
        
        /// <summary>
        /// Constants to draw a table - in percents!
        /// </summary>
        private const float PathFieldWidthWeight = 0.16f;
        private const float ObjectNameFieldWidthWeight = 0.15f;
        private const float GrabCheckboxWidthWeight = 0.08f;
        private const float PhysicsCheckboxWidthWeight = 0.1f;
        private const float SizeFieldWidthWeight = 0.08f;
        private const float MassFieldWidthWeight = 0.07f;
        private const float TagsFieldWidthWeight = 0.15f;
        
        private const int LangLabelWidth = 22;
        private const int GUISpaceSize = 2;
        
        private readonly Vector2 _waitingWindowSize = new Vector2(450, 200);
        
        private static Vector2 MinWindowSize => new Vector2(1080, 480);

        public static readonly string[] AvailableFileFormats = {
            "fbx",
            "obj",
            "blend",
            "zip",
        };
        
        /// <summary>
        /// List of models
        /// </summary>
        private static List<CreateObjectModel> _modelsList;

        /// <summary>
        /// List of existing object names
        /// </summary>
        private static List<string> _existingObjectNames = new List<string>();
        
        /// <summary>
        /// Build now checker
        /// </summary>
        private static bool _buildNow = true;
        
        private static bool _allObjectTypeNamesIsValid;

        private SketchfabImporter _importer;
        private string _currentStep;

        private List<CreateObjectModel> _gltfQueue;

        private Vector2 _scrollPosition;
        private static string _selectedFolder = "";

        private float PathFieldWidth => position.width * PathFieldWidthWeight;
        private float ObjectNameFieldWidth => position.width * ObjectNameFieldWidthWeight;
        private float GrabCheckboxWidth => position.width * GrabCheckboxWidthWeight;
        private float PhysicsCheckboxWidth => position.width * PhysicsCheckboxWidthWeight;
        private float SizeFieldWidth => position.width * SizeFieldWidthWeight;
        private float MassFieldWidth => position.width * MassFieldWidthWeight;
        private float TagsFieldWidth => position.width * TagsFieldWidthWeight;

        #region MENU OPTIONS

        [MenuItem("VARWIN SDK/Import/Model...", false, 1)]
        private static void InitModelImportWindow()
        {
            var filters = new[] {"Supported models", string.Join(",", AvailableFileFormats), "All files", "*"};
            string selectedFile = EditorUtility.OpenFilePanelWithFilters("Select file", "../", filters);
            ShowWindow(selectedFile, false);
        }

        [MenuItem("VARWIN SDK/Import/Folder...", false, 1)]
        private static void InitFolderImportWindow()
        {
            _selectedFolder = EditorUtility.OpenFolderPanel("Select folder", "../", "");
            ShowWindow(_selectedFolder, true);
        }

        private static void ShowWindow(string path, bool isFolder)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            _window = GetWindow<ImportModelsWindow>(true, "Import models", true);
            _window.minSize = MinWindowSize;
            _window.Initialize(path, isFolder);
        }

        #endregion

        private void Initialize(string path, bool isFolder)
        {
            if (_modelsList != null)
            {
                _modelsList.Clear();
                _modelsList = null;
            }

            _existingObjectNames = ObjectBuildHelper.GetExistingObjectsNames();

            _modelsList = new List<CreateObjectModel>();

            if (isFolder)
            {
                _selectedFolder = path + Path.AltDirectorySeparatorChar;
                AddFolder(path);
            }
            else
            {
                _selectedFolder = path.Substring(0, path.IndexOf(Path.GetFileName(path), StringComparison.Ordinal));
                AddFile(path);
            }

            if (_modelsList.Count == 0)
            {
                _window.Close();
                EditorUtility.DisplayDialog("No models to import", $"No models to import", "Ok");
                return;
            }
            
            _window.Show();
            
            CleanUp();
        }

        private void AddFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            string objectTypeName = Regex.Replace(Path.GetFileNameWithoutExtension(path), "([A-ZА-Я])", " $1");
            string fileName = ObjectBuildHelper.ConvertToNiceName(objectTypeName).Trim();
            CreateObjectModel model = new CreateObjectModel()
            {
                Path = path,
                ObjectName = fileName
                    .Replace(" ", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("-", ""),
                LocalizedName = fileName,
                LocalizedNameRU = fileName,
                IsGrabbable = true,
                IsPhysicsOn = true
            };
            _modelsList.Add(model);
        }

        private void AddFolder(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            FileInfo[] files = dirInfo.GetFiles();

            foreach (FileInfo file in files)
            {
                string ext = file.Extension.Replace(".", "");
                if (!AvailableFileFormats.Any(x => string.Equals(x, ext, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
                
                string objectTypeName = Regex.Replace(file.Name, "([A-ZА-Я])", " $1");
                string fileName = ObjectBuildHelper.ConvertToNiceName(Path.GetFileNameWithoutExtension(objectTypeName)).Trim();
                CreateObjectModel model = new CreateObjectModel()
                {
                    Path = file.FullName,
                    ObjectName = fileName
                        .Replace(" ", "")
                        .Replace("(", "")
                        .Replace(")", "")
                        .Replace("-", ""),
                    LocalizedName = fileName,
                    LocalizedNameRU = fileName,
                    IsGrabbable = true,
                    IsPhysicsOn = true
                };

                //Will prevent .meta files from getting into the import window
                if (!Regex.IsMatch(file.FullName, @".+\.meta$"))
                {
                    _modelsList.Add(model);
                }
            }
        }

        void OnGUI()
        {
            if (File.Exists(CreateObjectTempModel.TempFilePath) || _currentStep != null)
            {
                DrawWaitingWindow();
                return;
            }
            
            GUILayout.Label("Folder path", EditorStyles.boldLabel, GUILayout.Width(PathFieldWidth));
            GUILayout.Label(_selectedFolder);
            GUILayout.BeginHorizontal();

            GUILayout.Label("File", EditorStyles.boldLabel, GUILayout.Width(PathFieldWidth));

            GUILayout.Label("Object class name", EditorStyles.boldLabel,
                GUILayout.Width(ObjectNameFieldWidth + LangLabelWidth + GUISpaceSize));
            GUILayout.Label("Localized name", EditorStyles.boldLabel, GUILayout.Width(ObjectNameFieldWidth));

            GUILayout.Label("Is Grabbable", EditorStyles.boldLabel,
                GUILayout.Width(GrabCheckboxWidth));

            GUILayout.Label("Physics On\n(non-kinematic)", EditorStyles.boldLabel,
                GUILayout.Width(PhysicsCheckboxWidth));

            GUILayout.Label("Longest side\nlength (m)", EditorStyles.boldLabel,
                GUILayout.Width(SizeFieldWidth));
            GUILayout.Label("Mass (kg)", EditorStyles.boldLabel, GUILayout.Width(MassFieldWidth));
            GUILayout.Label("Tags", EditorStyles.boldLabel, GUILayout.Width(TagsFieldWidth));
            GUILayout.EndHorizontal();

            if (_modelsList == null)
            {
                _modelsList = new List<CreateObjectModel>();
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            _allObjectTypeNamesIsValid = true;
            foreach (CreateObjectModel fileModel in _modelsList)
            {
                DrawListItem(fileModel);
            }

            GUILayout.EndScrollView();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.BeginHorizontal();

            GUILayout.Label("Build right after import", EditorStyles.boldLabel);

            _buildNow = EditorGUILayout.Toggle(_buildNow, GUILayout.Width(110));

            GUILayout.FlexibleSpace();

            DrawCreateButton();

            GUILayout.EndHorizontal();
        }
        
        private void DrawWaitingWindow()
        {
            this.minSize = _waitingWindowSize;
            this.maxSize = _waitingWindowSize;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label("Please wait until all objects are created.\nThis window will be closed automatically.",
                EditorStyles.boldLabel, GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(_currentStep ?? "Compiling scripts...", EditorStyles.largeLabel,
                GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawCreateButton()
        {
            var enabled = _modelsList.Any(x => !x.Skip) && _allObjectTypeNamesIsValid;
            EditorGUI.BeginDisabledGroup(!enabled);
            if (GUILayout.Button("Create objects", GUILayout.Width(140)))
            {
                try
                {
                    CreateAll();
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("Error!", $"Can't create objects:\n{e.Message}", "OK");
                    Debug.LogException(e);
                    Close();
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        
        private void DrawListItem(CreateObjectModel fileModel)
        {
            var nameLabelWidth = GUILayout.Width(LangLabelWidth);
            var nameFieldWidth = GUILayout.Width(ObjectNameFieldWidth);
            
            GUILayout.BeginHorizontal();
            
            var subPath = Path.GetFileName(fileModel.Path);
            GUILayout.Label(subPath, EditorStyles.label, GUILayout.Width(PathFieldWidth));
            fileModel.ObjectName = EditorGUILayout.TextField(fileModel.ObjectName, nameFieldWidth);
            
            EditorGUILayout.BeginVertical(GUILayout.Width(ObjectNameFieldWidth + LangLabelWidth + GUISpaceSize));
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("EN", nameLabelWidth);
            fileModel.LocalizedName = EditorGUILayout.TextField(fileModel.LocalizedName, nameFieldWidth);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("RU", nameLabelWidth);
            fileModel.LocalizedNameRU = EditorGUILayout.TextField(fileModel.LocalizedNameRU, nameFieldWidth);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();

            fileModel.IsGrabbable =
                EditorGUILayout.Toggle(fileModel.IsGrabbable, GUILayout.Width(GrabCheckboxWidth));

            fileModel.IsPhysicsOn =
                EditorGUILayout.Toggle(fileModel.IsPhysicsOn, GUILayout.Width(PhysicsCheckboxWidth));

            fileModel.BiggestSideSize =
                EditorGUILayout.FloatField(fileModel.BiggestSideSize, GUILayout.Width(SizeFieldWidth));
            fileModel.Mass = EditorGUILayout.FloatField(fileModel.Mass, GUILayout.Width(MassFieldWidth));

            var tagsStyle = new GUIStyle(EditorStyles.textField)
            {
                wordWrap = true,
            };
            var tagsLayout = new[]
            {
                GUILayout.Width(TagsFieldWidth), 
                GUILayout.MaxHeight(30), 
                GUILayout.ExpandHeight(false),
            };
            fileModel.Tags = EditorGUILayout.TextArea(fileModel.Tags, tagsStyle, tagsLayout);
            GUILayout.EndHorizontal();
            
            GUILayout.Label("", GUILayout.Height(5));

            bool objectTypeNameIsValid = false;
            if (string.IsNullOrEmpty(fileModel.ObjectName))
            {
                EditorGUILayout.HelpBox($"Object Class Name can't be empty", MessageType.Error);
            }
            else if (!ObjectBuildHelper.IsValidTypeName(fileModel.ObjectName))
            {
                EditorGUILayout.HelpBox($"Object Class Name contains unavailable symbols", MessageType.Error);
            }
            else if (_existingObjectNames.Any(x => string.Equals(x, fileModel.ObjectName, StringComparison.OrdinalIgnoreCase)))
            {
                EditorGUILayout.HelpBox($"An object with the same Object Class Name already exists.", MessageType.Error);
            }
            else
            {
                objectTypeNameIsValid = true;
            }

            if (!objectTypeNameIsValid)
            {
                _allObjectTypeNamesIsValid = false;
            }
        }
        
        private void CreateAll()
        {
            _gltfQueue = new List<CreateObjectModel>();

            foreach (CreateObjectModel fileModel in _modelsList)
            {
                if (!ObjectBuildHelper.IsValidTypeName(fileModel.ObjectName))
                {
                    Debug.Log("Can't create object with class name " + fileModel.ObjectName + ": not a valid type name");

                    continue;
                }

                _currentStep = "Importing model from " + fileModel.Path + "...";

                fileModel.Guid = Guid.NewGuid().ToString();
                fileModel.LocalizedName = ObjectBuildHelper.EscapeString(fileModel.LocalizedName);
                fileModel.LocalizedNameRU = ObjectBuildHelper.EscapeString(fileModel.LocalizedNameRU);
                fileModel.ObjectFolder = "Assets/Objects/" + fileModel.ObjectName;
                fileModel.ModelFolder = fileModel.ObjectFolder + "/Model";
                fileModel.ModelImportPath = fileModel.ModelFolder + "/" + Path.GetFileName(fileModel.Path);
                fileModel.PrefabPath = fileModel.ObjectFolder + "/" + fileModel.ObjectName + ".prefab";

                if (Directory.Exists(fileModel.ObjectFolder))
                {
                    EditorUtility.DisplayDialog(fileModel.ObjectName + " already exists!",
                        "Object with this name already exists", "OK");
                    fileModel.Skip = true;
                }
                else
                {
                    Debug.Log("Create folder " + fileModel.ObjectFolder);
                    Directory.CreateDirectory(fileModel.ObjectFolder);
                    Directory.CreateDirectory(fileModel.ObjectFolder + "/Model");

                    CreateTags(fileModel);
                    
                    string extension = Path.GetExtension(fileModel.Path);

                    if (extension == null)
                    {
                        fileModel.Skip = true;

                        EditorUtility.DisplayDialog(fileModel.ObjectName + " can't be imported",
                            "Unsupported file format", "OK");

                        continue;
                    }

                    if (extension.Contains("zip"))
                    {
                        fileModel.ModelImportPath = fileModel.ModelFolder +
                                                    "/" +
                                                    Path.GetFileNameWithoutExtension(fileModel.Path) +
                                                    ".prefab";
                        ImportGLTFQueue(fileModel);
                    }
                    else
                    {
                        ImportUsualAsset(fileModel);
                    }
                }
            }

            if (_gltfQueue.Count == 0)
            {
                CreateCode();
            }
        }
        
        private void CreateTags(CreateObjectModel model)
        {
            if (model.Tags == null)
                return;

            if (model.Tags.Length == 0)
                return;
            var tags = model.Tags.Split(',');
            List<string> tagList = new List<string>();

            foreach (string tag in tags)
            {
                int i = 0;
                tagList.Add(tag[i] == ' ' ? tag.Remove(i, 1) : tag);
            }

            File.WriteAllLines(model.ObjectFolder + "/tags.txt", tagList);
        }

        private void ImportUsualAsset(CreateObjectModel fileModel)
        {
            try
            {
                Debug.Log("Import model for " + fileModel.ObjectName);
                FileUtil.CopyFileOrDirectory(fileModel.Path, fileModel.ModelImportPath);
                AssetDatabase.ImportAsset(fileModel.ModelImportPath, ImportAssetOptions.ForceUpdate);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!", $"Problem when importing a model for \"{fileModel.ObjectName}\" ({fileModel.Path}): {e.Message}", "OK");
                Debug.LogException(e);
            }
        }

        private void ImportGLTFQueue(CreateObjectModel fileModel)
        {
            bool first = _gltfQueue.Count == 0;
            _gltfQueue.Add(fileModel);

            if (first)
            {
                ImportGLTF(_gltfQueue[0]);
            }
        }

        private void ImportGLTF(CreateObjectModel fileModel)
        {
            try
            {
                if (_importer != null)
                {
                    _importer.cleanArtifacts();
                    _importer = null;
                }

                _importer = new SketchfabImporter(UpdateProgress, OnFinishImport);

                _importer.configure(GLTFUtils.getPathAbsoluteFromProject(fileModel.ModelFolder),
                    Path.GetFileNameWithoutExtension(fileModel.Path), false);
                _importer.loadFromFile(fileModel.Path);
                fileModel.Extras = ParseAssetForLicense(_importer.GetGLTFInput());
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!", $"Problem when importing a model for \"{fileModel.ObjectName}\" ({fileModel.Path}):\nNot valid model structure ({e.Message})", "OK");
                Debug.LogException(e);
                
                CleanUp();

                fileModel.Skip = true;

                if (_gltfQueue.Contains(fileModel))
                {
                    _gltfQueue.Remove(fileModel);
                }

                if (Directory.Exists("Assets/Objects/" + fileModel.ObjectName))
                {
                    Directory.Delete("Assets/Objects/" + fileModel.ObjectName, true);
                }
            }
        }

        private CreateObjectModel.AssetExtras ParseAssetForLicense(string gltf)
        {
            var gltfBinary = File.ReadAllBytes(gltf);

            if (gltfBinary.Length == 0)
            {
                throw new GLTFHeaderInvalidException("glTF file cannot be empty.");
            }

            if (IsGLB(gltfBinary))
            {
                return null;
            }

            return ParseGLTF(gltfBinary);
        }

        private static bool IsGLB(byte[] gltfBinary)
        {
            return BitConverter.ToUInt32(gltfBinary, 0) == 1179937895U;
        }


        private CreateObjectModel.AssetExtras ParseGLTF(byte[] gltfBinary)
        {
            var json = Encoding.UTF8.GetString(gltfBinary);
            var jObject = JObject.Parse(json);
            string extras = "";

            try
            {
                extras = jObject["asset"]["extras"].ToString();
            }
            catch
            {
                Debug.LogError("cannot import; no authoring data can be found in the .gltf");
            }

            CreateObjectModel.AssetExtras licenseInfo =
                JsonConvert.DeserializeObject<CreateObjectModel.AssetExtras>(extras);

            return licenseInfo;
        }


        private void Update()
        {
            try
            {
                if (_importer != null)
                {
                    _importer.Update();
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Error!", $"Can't import models:\n{e.Message}", "OK");
                Debug.LogException(e);
            }
        }

        public void UpdateProgress(UnityGLTF.GLTFEditorImporter.IMPORT_STEP step, int current, int total)
        {
            string element = "Importing GLTF model " + Path.GetFileName(_gltfQueue[0].Path) + ": ";

            switch (step)
            {
                case GLTFEditorImporter.IMPORT_STEP.BUFFER:
                    element += "Buffer";

                    break;
                case UnityGLTF.GLTFEditorImporter.IMPORT_STEP.IMAGE:
                    element += "Image";

                    break;
                case UnityGLTF.GLTFEditorImporter.IMPORT_STEP.TEXTURE:
                    element += "Texture";

                    break;
                case UnityGLTF.GLTFEditorImporter.IMPORT_STEP.MATERIAL:
                    element += "Material";

                    break;
                case UnityGLTF.GLTFEditorImporter.IMPORT_STEP.MESH:
                    element += "Mesh";

                    break;
                case UnityGLTF.GLTFEditorImporter.IMPORT_STEP.NODE:
                    element += "Node";

                    break;
                case UnityGLTF.GLTFEditorImporter.IMPORT_STEP.ANIMATION:
                    element += "Animation";

                    break;
                case UnityGLTF.GLTFEditorImporter.IMPORT_STEP.SKIN:
                    element += "Skin";

                    break;
            }

            _currentStep = element;
        }

        private void OnFinishImport()
        {
            _importer.cleanArtifacts();
            _gltfQueue.RemoveAt(0);

            if (_gltfQueue.Count == 0)
            {
                CreateCode();
            }
            else
            {
                ImportGLTF(_gltfQueue[0]);
            }
        }

        private void CreateCode()
        {
            if (_modelsList.All(x => x.Skip))
            {
                CancelCreating();
                Debug.Log("No objects to create code");
                return;
            }
            
            foreach (CreateObjectModel fileModel in _modelsList)
            {
                if (fileModel.Skip)
                {
                    continue;
                }
                
                _currentStep = "Creating code for " + fileModel.ObjectName;
                Debug.Log(_currentStep);

                fileModel.ClassName = CreateObjectWindow.CreateCode(fileModel);
                Debug.Log("Class " + fileModel.ClassName + " created.");
            }

            CreateObjectTempModel temp = new CreateObjectTempModel() {Objects = _modelsList, BuildNow = _buildNow};

            string jsonModels = JsonConvert.SerializeObject(temp, Formatting.None,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

            File.WriteAllText(CreateObjectTempModel.TempFilePath, jsonModels);


            _currentStep = "Compiling scripts...";
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private void CancelCreating()
        {
            EditorUtility.DisplayDialog("Objects creating canceled!", "Objects creating canceled", "OK");
            
            if (File.Exists(CreateObjectTempModel.TempFilePath))
            {
                File.Delete(CreateObjectTempModel.TempFilePath);
            }
            
            this.Close();

            _currentStep = null;
        }

        private void CleanUp()
        {
            CleanUpSketchfabImportFolder();
        }
        
        private void CleanUpSketchfabImportFolder()
        {
            string path = Application.temporaryCachePath + "/unzip";
            try
            {
                if (Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    dir.Delete(true);
                }
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog(
                    "Error with cleaning-up the SketchFab import directory", 
                    $"Unable to clear the SketchFab import directory (\"{path}\")", 
                    "OK");
                Debug.LogError($"Unable to clear the SketchFab import directory (\"{path}\")");
                throw;
            }
        }
    }
}
