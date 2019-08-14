using System.Collections.Generic;
using NLog;
using SmartLocalization;
using UnityEngine;
using UnityEngine.XR;
using Varwin.Commands;
using Varwin.Data;
using Varwin.Data.ServerData;
using Varwin.Models.Data;
using Varwin.PUN;
using Varwin.UI.Varwin.UI;
using Varwin.VRInput;
#if UNITY_STANDALONE_WIN && !VRMAKER
using ZenFulcrum.EmbeddedBrowser;
#endif

namespace Varwin.UI
{
    // ReSharper disable once InconsistentNaming
    public class UIMenu : MonoBehaviour
    {
        public GameObject BrowserWindow;
        public GameObject SpawnSphere;
        public GameObject ModeButton;
        public GameObject ModeButtonOffset;
        public LoaderAnime Loader;
        public LoaderAnime ObjectLoader;
        private bool _htmlLoaded;
#if UNITY_STANDALONE_WIN && !VRMAKER
        public Browser Browser;
#endif
        public bool OnboardingBlock;
        public static UIMenu Instance;
        private bool _isLoading = true;
        private bool _openAfterLoad;
        private TransformDT _savedTransform;
        private const float ReloadTime = 5f;
        private float _updateLoadTime = ReloadTime;
        private Transform _pointerOrigin;
        
        private const string OnboardingFuncName = "setOnboardingStep";

        public delegate void MenuOpenHandler();

        public delegate void MenuCloseHandler();

        public event MenuOpenHandler OnMenuOpened;
        public event MenuCloseHandler OnMenuClosed;

        private void Awake()
        {
            Instance = this;
            Loader.StopLoaderAnimation();
            ObjectLoader.StopLoaderAnimation();
            ModeButton.SetActive(false);
            _savedTransform = BrowserWindow.transform.ToLocalTransformDT();
            ModeButtonOffset = new GameObject();
            ModeButtonOffset.transform.SetParent(transform.parent);
            ModeButtonOffset.transform.localRotation = Quaternion.identity;
            ModeButtonOffset.transform.localPosition = new Vector3(0f, 0.29f, 0.433f);
        }

        private void Update()
        {
#if !VRMAKER
            if (!_isLoading)
            {
                return;
            }

            if (_updateLoadTime > 0)
            {
                _updateLoadTime -= Time.deltaTime;
            }
            else
            {
                _updateLoadTime = ReloadTime;
                LoadMenu();
            }
#endif
        }

#if UNITY_STANDALONE_WIN && !VRMAKER
        public void InitBrowser(Browser browser)
        {
            Browser = browser;
        }

        private void ShowHtml()
        {
            string uri = Settings.Instance().WebHost + "/mypad?lang=" + Settings.Instance().Language + "&scene_id=" + ProjectData.SceneId;
            //string uri = "http://192.168.33.91:8080" + "/mypad?lang=" + Settings.Instance().Language;

            Browser.LoadURL(uri, true);

            Browser.RegisterFunction("spawnObject",
                args =>
                {
                    var logger = LogManager.GetCurrentClassLogger();
                    string objectString = args[0].AsJSON;

                    PrefabObject prefabObject = objectString.JsonDeserialize<PrefabObject>();

                    ObjectDto existedObject = ProjectData.ProjectStructure.Scenes.GetProjectScene(ProjectData.SceneId)
                        .SceneObjects.Find(p => p.ObjectId == prefabObject.Id);


                    Debug.Log("Object to spawn: " + prefabObject.Guid);
                    int objectId = prefabObject.Id;

                    if (GameStateData.GetPrefabGameObject(objectId) != null)
                    {
                        Helper.SetSpawnedObject(objectId);
                        logger.Info("Spawn existed object: " + objectId);
                    }
                    else if (existedObject != null)
                    {
                        logger.Info("Object " + prefabObject.Guid + " is already loading!");
                        HideMenu();

                        return;
                    }
                    else
                    {
                        ProjectData.ProjectStructure.Objects.Add(prefabObject);
                        var po = new List<PrefabObject> {prefabObject};


                        void OnLoadObjects()
                        {
                            if (GameStateData.GetPrefabGameObject(objectId) != null && !BrowserWindow.activeSelf)
                            {
                                Helper.SetSpawnedObject(objectId);
                                logger.Info("New object spawned: " + objectId);
                            }

                            ObjectLoader.StopLoaderAnimation();
                            ProjectData.ObjectsLoaded -= OnLoadObjects;
                        }


                        LoaderAdapter.LoadPrefabObjects(po);
                        ProjectData.ObjectsLoaded += OnLoadObjects;
                        ObjectLoader.StartLoaderAnimation();
                    }

                    HideMenu();
                });

            Browser.RegisterFunction("undo", args => { CommandsManager.Undo(); });
            Browser.RegisterFunction("redo", args => { CommandsManager.Redo(); });
            Browser.RegisterFunction("switchMode", args => { SwitchGameMode(); });
            Browser.RegisterFunction("save", args => { Helper.SaveSceneObjects(); });

            SetRender();
            _htmlLoaded = true;
        }
#endif

        private void SetRender()
        {
            var renders = GetComponentsInChildren<Renderer>();

            foreach (Renderer render in renders)
            {
                render.material.renderQueue = 3110;
            }
        }

        public void ApplyGameMode()
        {
            ProjectData.GameMode = ProjectData.GameMode == GameMode.Edit ? GameMode.Preview : GameMode.Edit;
            Helper.ResetSpawnObject();
        }

        public void SwitchGameMode()
        {
            BrowserWindow.SetActive(false);

            if (ProjectData.GameMode == GameMode.View)
            {
                return;
            }

            if (ProjectData.GameMode == GameMode.Edit && ProjectData.ObjectsAreChanged)
            {
                Helper.AskUserToDo(LanguageManager.Instance.GetTextValue("GROUP_NOT_SAVED"),
                    () =>
                    {
                        Helper.SaveSceneObjects();
                        ApplyGameMode();
                    },
                    () =>
                    {
                        Helper.ReloadSceneObjects();
                        ApplyGameMode();
                    },
                    HideMenu);
            }
            else
            {
                ApplyGameMode();
            }
        }

        public void LoadMenu()
        {
            if (_htmlLoaded)
            {
                LogManager.GetCurrentClassLogger().Info("Menu is alredy loaded");

                return;
            }

            LogManager.GetCurrentClassLogger().Info("Load menu started");
            _isLoading = true;
            BrowserWindow.transform.position = new Vector3(10000, 100000, 0);
            BrowserWindow.SetActive(true);

            if (!_htmlLoaded)
            {
#if UNITY_STANDALONE_WIN && !VRMAKER
                ShowHtml();
#endif
            }
        }

        public void ShowMenu()
        {
            if (_isLoading && ProjectData.GameMode != GameMode.View)
            {
                Loader.StartLoaderAnimation();
                _openAfterLoad = true;

                return;
            }

            if (VRMessageManager.VRMessageManager.Instance.Showing)
            {
                return;
            }

            if (ProjectData.GameMode != GameMode.View && ProjectData.GameMode != GameMode.Preview && !OnboardingBlock)
            {

                if (_pointerOrigin == null)
                {
                    UpdatePointerOrigns();

                    if (_pointerOrigin == null)
                    {
                        return;
                    }

                }

#if UNITY_STANDALONE_WIN && !VRMAKER
                PointerUIBase.RightHand = _pointerOrigin;
#endif

                SpawnSphere.SetActive(true);
                ModeButton.SetActive(false);
                BrowserWindow.SetActive(true);
                Helper.ResetSpawnObject();
                OnMenuOpened?.Invoke();
            }
            else
            {
                SpawnSphere.SetActive(false);

                if (!OnboardingBlock)
                {
                    ModeButton.SetActive(ProjectData.GameMode != GameMode.View);
                }

                BrowserWindow.SetActive(false);
            }
        }
        
        private void UpdatePointerOrigns()
        {
            var rightHand = InputAdapter.Instance.PlayerController.Nodes.RightHand;
            
            Transform origins = rightHand.Transform.Find("PointerOrigins");

            if (origins == null)
            {
                _pointerOrigin = rightHand.Transform;

                return;
            }
            
            if (XRDevice.model.Contains("Oculus"))
            {
                _pointerOrigin = origins.Find("Oculus");
            }
            else
            {
                _pointerOrigin = origins.Find("Generic");
            }

            if (_pointerOrigin == null)
            {
                _pointerOrigin = rightHand.Transform;
            }
        }

        public void BrowserLoaded()
        {
            _isLoading = false;
            Loader.StopLoaderAnimation();
            _savedTransform.ToLocalTransformUnity(BrowserWindow.transform);
            BrowserWindow.SetActive(false);
            LogManager.GetCurrentClassLogger().Info("Browser is loaded");

            BrowserWindow.transform.GetChild(0).GetChild(0).gameObject.AddComponent<UIPanel>();
            
            if (_openAfterLoad)
            {
                ShowMenu();
            }
        }

        public void HideMenu()
        {
            if (_isLoading && ProjectData.GameMode != GameMode.View)
            {
                _openAfterLoad = false;
                Loader.StopLoaderAnimation();

                return;
            }

            BrowserWindow.SetActive(false);
            OnMenuClosed?.Invoke();
            ModeButton.SetActive(false);
        }


        public void HighlightMenuItem(string guid)
        {
#if UNITY_STANDALONE_WIN && !VRMAKER
            string arguments = $"{{\"step\": 1, \"guid\": \"{guid}\"}}";
            Browser.CallFunction(OnboardingFuncName, arguments);
#endif
        }

        public void HighlightSaveButton()
        {
#if UNITY_STANDALONE_WIN && !VRMAKER
            Browser.CallFunction(OnboardingFuncName, "{\"step\": 2}");
#endif
        }

        public void HighlightModeButton()
        {
#if UNITY_STANDALONE_WIN && !VRMAKER
            Browser.CallFunction(OnboardingFuncName, "{\"step\": 3}");
#endif
        }

        public void ResetMenuHighlight()
        {
#if UNITY_STANDALONE_WIN && !VRMAKER
            Browser.CallFunction(OnboardingFuncName);
#endif
        }
    }
}
