using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Varwin.Errors;
using NLog;
using SmartLocalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Varwin.Data;
using Varwin.Data.ServerData;
using Varwin.Public;
using Varwin.PUN;
using Varwin.UI;
using Varwin.UI.VRErrorManager;
using Varwin.VRInput;
using Varwin.WWW;
using Logger = NLog.Logger;

namespace Varwin
{
    public class ProjectDataListener : MonoBehaviour
    {
        #region PUBLIC VARS
        public static ProjectDataListener Instance;
        public LaunchArguments LaunchArguments { get; set; }
        public ProjectSceneArguments ProjectSceneArguments { get; set; }
        public ProjectConfigurationArguments ProjectConfigurationArguments { get; set; }
        public static Action OnUpdate { get; set; }
        #endregion

        #region PRIVATE VARS
        private int _lastLocationId;
        private bool _launch;
        private Logger _logger;
        //private string _currentLoadedScene;
        #endregion

        private void Awake()
        {           
            if (Instance == null) 
            {
                Instance = this;
            } 
            else if (Instance != this) 
            {
                Destroy (gameObject);
            }
        }

        private void Start()
        {
            Application.logMessageReceived += ErrorHelper.ErrorHandler;
            _logger = LogManager.GetCurrentClassLogger();
        }

        private void Update()
        {
            DoOnUpdate();

            if (LoaderAdapter.LoaderType == null || LoaderAdapter.LoaderType == typeof(StorageLoader))
            {
                return;
            }

            if (LaunchArguments != null && !_launch)
            {
                _launch = true;
                AskUserToSave();
            }

            if (ProjectSceneArguments != null)
            {
                switch (ProjectSceneArguments.State)
                {
                    case ProjectSceneArguments.StateProjectScene.Added:
                        LocationAdded(ProjectSceneArguments);

                        break;

                    case ProjectSceneArguments.StateProjectScene.Deleted:
                        LocationDeleted(ProjectSceneArguments);

                        break;

                    case ProjectSceneArguments.StateProjectScene.Changed:
                        LocationChanged(ProjectSceneArguments);

                        break;
                }

                ProjectSceneArguments = null;
            }

            if (ProjectConfigurationArguments != null)
            {
                switch (ProjectConfigurationArguments.State)
                {
                    case ProjectConfigurationArguments.StateConfiguration.Added:
                        ConfigurationAdded(ProjectConfigurationArguments.ProjectConfiguration);

                        break;

                    case ProjectConfigurationArguments.StateConfiguration.Deleted:
                        ConfigurationDeleted(ProjectConfigurationArguments.ProjectConfiguration);

                        break;

                    case ProjectConfigurationArguments.StateConfiguration.Changed:
                        ConfigurationChanged(ProjectConfigurationArguments.ProjectConfiguration);

                        break;
                }

                ProjectConfigurationArguments = null;
            }

        }

        private void AskUserToSave()
        {
            if (ProjectData.GameMode == GameMode.Edit && ProjectData.ObjectsAreChanged)
            {
                Helper.AskUserToDo(LanguageManager.Instance.GetTextValue("GROUP_NOT_SAVED"),
                    //YES 
                    () =>
                    {
                        ProjectData.OnSave = ApplyConfig;
                        Helper.SaveSceneObjects();
                    },
                    //NO
                    ApplyConfig,
                    () =>
                    //CANCEL
                    {
                        LaunchArguments = null;
                        ReadyToGetNewMessages();
                    });
            }
            else 
            {
                ApplyConfig();
            }
          
        }

        private void DoOnUpdate()
        {
            try
            {
                OnUpdate?.Invoke();
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error("Error Invoke OnUpdate! " + e.Message + " " + e.StackTrace);
            }
        }

        private void ApplyConfig()
        {
            if (LaunchArguments == null)
            {
                _launch = true;

                return;
            }

            LoaderAdapter.LoadProject(LaunchArguments);
            UpdateSettings(LaunchArguments);
            LaunchArguments = null;
        }

        public void ReadyToGetNewMessages()
        {
            Debug.Log("<Color=Lime><b></b>Listener ready to listen!</Color>");
            _logger.Info($"Listener ready to listen!");
            _launch = false;
        }

        public void UpdateSettings(LaunchArguments launchArguments)
        {
            LogManager.GetCurrentClassLogger().Info("New launch arguments: " + launchArguments.ToJson());
            Settings.ReadLauncherSettings(launchArguments);
                
            if (Settings.Instance().Multiplayer)
            {
                PhotonNetwork.autoJoinLobby = false;
                PhotonNetwork.automaticallySyncScene = true;
                PhotonNetwork.PhotonServerSettings.ServerAddress = Settings.Instance().PhotonHost;
                PhotonNetwork.PhotonServerSettings.ServerPort = Convert.ToInt32(Settings.Instance().PhotonPort);
            }
            else
            {
                PhotonNetwork.offlineMode = true;
            }

            if (!_licenseGot)
            {
                GetLicense();
            }
        }

        private bool _licenseGot;
        private void GetLicense()
        {
            RequestApi requestApi = new RequestApi(ApiRoutes.GetLicenseRequest);
            _logger.Info("Starting get license info...");

            requestApi.OnFinish += response =>
            {
                ResponseApi responseApi = (ResponseApi) response;

                try
                {
                    string jsonData = responseApi.Data.ToString();
                    LicenseDto licenseDto = jsonData.JsonDeserialize<LicenseDto>();
                    LicenseInfo.Value = licenseDto.License;

                    if (LauncherErrorManager.Instance)
                    {
                        LauncherErrorManager.Instance.License(LicenseInfo.Value);
                    }

                    _logger.Info("License ok! Edition is " + LicenseInfo.Value.EditionId);
                    _licenseGot = true;
                }
                catch (Exception e)
                {
                    LauncherErrorManager.Instance.ShowFatal(ErrorHelper.GetErrorDescByCode(Varwin.Errors.ErrorCode.LicenseKeyError), e.Message);
                    _logger.Fatal("License information error");
                    _licenseGot = false;
                }
               
            };
            requestApi.OnError += s =>
            {
                LauncherErrorManager.Instance.ShowFatal(ErrorHelper.GetErrorDescByCode(Varwin.Errors.ErrorCode.LicenseKeyError), s);
                _logger.Fatal("License information error");
                _licenseGot = false;
            };

        }
        
        public void BeforeSceneLoaded(bool otherLocation)
        {
            HideUiMenu();
            HidePopUpAndToolTips();
            StartCoroutine(BeforeLoadSceneCoroutine(otherLocation));
        }

        private IEnumerator BeforeLoadSceneCoroutine(bool otherLocation)
        {
            yield return StartCoroutine(FadeIn());
            
            if (otherLocation)
            {
                GameStateData.ClearObjects();
                GameStateData.ClearLogic();
                yield return StartCoroutine(LoadLoaderScene());
                yield return StartCoroutine(UnloadSceneAndResources());
            }
            else
            {
                Helper.ReloadSceneObjects();
            }

            ResetPlayerRigPosition();
            ProjectData.OnSceneCleared();
            yield return StartCoroutine(FadeOut());
        }

        private void ResetPlayerRigPosition()
        {

            if (GameObjects.Instance == null)
            {
                return;
            }

            WorldDescriptor worldDescriptor = FindObjectOfType<WorldDescriptor>();

            if (worldDescriptor == null)
            {
                ReturnLoadError("WorldDescriptor not found!");

                return;
            }


            PlayerAnchorManager playerAnchorManager = FindObjectOfType<PlayerAnchorManager>();

            if (playerAnchorManager == null)
            {
                playerAnchorManager = worldDescriptor.PlayerSpawnPoint.gameObject.AddComponent<PlayerAnchorManager>();
            }

            playerAnchorManager.SetPlayerPosition();

        }

        private void HideUiMenu()
        {
            if (GameObjects.Instance == null)
            {
                return;
            }

            if (GameObjects.Instance.UiMenu != null)
            {
                GameObjects.Instance.UiMenu.HideMenu();
            }
        }


        public void LoadScene(string scenePath)
        {
            StartCoroutine(LoadSceneBeautifulCoroutine(scenePath));
        }

        private IEnumerator LoadSceneBeautifulCoroutine(string scenePath)
        {
            yield return StartCoroutine(FadeIn());
            yield return StartCoroutine(LoadSceneCoroutine(scenePath));

            while (InputAdapter.Instance.PlayerController.Nodes.Head.GameObject == null)
            {
                yield return null;
            }
            
            ProjectData.SceneWasLoaded();
            
            yield return StartCoroutine(WaitAndLoadUiMenu());
            yield return StartCoroutine(FadeOut());
        }

        private void LoadWorldDescriptor()
        {
             
            _logger.Info("Waiting to load WorldDescriptor...");
            WorldDescriptor worldDescriptor = FindObjectOfType<WorldDescriptor>();

            if (worldDescriptor == null)
            {
                ReturnLoadError("WorldDescriptor not found!");
                return;
            }

            if (worldDescriptor.PlayerSpawnPoint != null)
            {

                PlayerAnchorManager playerAnchorManager = FindObjectOfType<PlayerAnchorManager>();

                if (playerAnchorManager == null)
                {
                    playerAnchorManager = worldDescriptor.PlayerSpawnPoint.gameObject.AddComponent<PlayerAnchorManager>();
                }

                playerAnchorManager.RestartPlayer();
            }
            else
            {
                _logger.Error("Player Spawn Point not found in Location Config");
                ReturnLoadError("Player Spawn Point not found in Location Config");
            }

        }

        private void ReturnLoadError(string message)
        {
            _logger.Error("Error to load scene. " + message);
            GameObject go = new GameObject("Default player rig");
            go.transform.position = new Vector3(0, 1.8f, 0);
            go.transform.rotation = Quaternion.identity;
            go.AddComponent<PlayerAnchorManager>();
            StartCoroutine(ShowSpawnError());
        }

        private IEnumerator ShowSpawnError()
        {
            while (VRErrorManager.Instance == null)
            {
                yield return null;
            }

            VRErrorManager.Instance.Show(ErrorHelper.GetErrorDescByCode(Varwin.Errors.ErrorCode.SpawnPointNotFoundError));
            yield return true;
        }

        private IEnumerator WaitAndLoadUiMenu()
        {
            UIMenu uiMenu = null;

            if (ProjectData.GameMode != GameMode.View)
            {
                while (uiMenu == null)
                {
                    uiMenu = FindObjectOfType<UIMenu>();

                    if (uiMenu != null)
                    {
                        uiMenu.LoadMenu();
                    }

                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                yield return true;
            }
            
        }


        private IEnumerator LoadSceneCoroutine(string scenePath)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(scenePath);

            Debug.Log("Loading new Scene: " + Time.time);

            var activeScene = SceneManager.GetActiveScene();

            if (activeScene.path == scenePath)
            {
                Debug.Log("Scene already loaded");
                yield break;
            }
            
            AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(fileNameWithoutExtension, LoadSceneMode.Additive);

            while (!loadSceneOperation.isDone) 
            {
                yield return null;
            }
            
            Debug.Log("Unloading old Scene: " + Time.time);
            
            AsyncOperation unloadSceneOperation = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(0));
            
            while (!unloadSceneOperation.isDone)
            {
                yield return null;
            }
            
            LoadWorldDescriptor();
            _logger.Info("Location is loaded: " + Time.time);
           
            yield return true;
        }

        private IEnumerator FadeIn()
        {
            if (UIFadeInOutController.Instance == null)
            {
                yield break;
            }
            
            UIFadeInOutController.Instance.FadeIn();

            if (ProjectData.PlatformMode == PlatformMode.Desktop)
            {
                UIFadeInOutController.Instance.FadeInNow();
                yield break;
            }

            while (UIFadeInOutController.Instance.FadeStatus != UIFadeInOutController.FadeInOutStatus.FadingInComplete &&
                   UIFadeInOutController.Instance.FadeStatus != UIFadeInOutController.FadeInOutStatus.FadingOutComplete)
            {
                yield return new WaitForEndOfFrame();
            }

            yield return true;
        }

        private IEnumerator FadeOut()
        {
            if (UIFadeInOutController.Instance == null)
            {
                yield break;
            }

            UIFadeInOutController.Instance.FadeOut();
        }

        private IEnumerator UnloadSceneAndResources()
        {
            _logger.Info("Start unload current scene");
            
            AsyncOperation unloadSceneOperation = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(0));

            if (unloadSceneOperation == null)
            {
                yield break;
            }

            while (!unloadSceneOperation.isDone)
            {
                yield return null;
            }
            
            Resources.UnloadUnusedAssets();
            
            yield return true;
        }

        private IEnumerator LoadLoaderScene()
        {
            if (InputAdapter.Instance.PlayerController.Nodes.Head.GameObject == null)
            {
                yield break;
            }

            _logger.Info("Load loader scene");
            
            AsyncOperation loadLoading = SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);

            while (!loadLoading.isDone)
            {
                yield return null;
            }

            yield return true;
        }

        public void RestoreJoints(Dictionary<int, JointData> joints)
        {
            if (joints == null)
            {
                return;
            }

            StartCoroutine(RestoreJointsOnNextFrame(joints));

        }

        private IEnumerator RestoreJointsOnNextFrame(Dictionary<int, JointData> joints)
        {
            JointBehaviour.IsTempConnectionCreated = true;

            yield return new WaitForEndOfFrame();

            Debug.Log("<Color=Olive>Restore joints started!</Color>"); 

            var jointsScene = FindObjectsOfType<JointBehaviour>();

            foreach (JointBehaviour joint in jointsScene)
            {
                joint.UnLockAndDisconnectPoints();
            }

            foreach (var joint in joints)
            {
                int instanseId = joint.Key;
                JointData jointData = joint.Value;
                ObjectController objectController = GameStateData.GetObjectInLocation(instanseId);

                if (objectController == null)
                {
                    continue;
                }

                JointBehaviour jointBehaviour = objectController.RootGameObject.GetComponent<JointBehaviour>();

                if (jointBehaviour == null)
                {
                    continue;
                }
                 
                var jointPoints = Helper.GetJointPoints(objectController.RootGameObject);

                foreach (var jointConnectionsData in jointData.JointConnetionsData)
                {
                    int pointId = jointConnectionsData.Key;
                    JointPoint myJointPoint = jointPoints[pointId];
                    JointConnetionsData connectionData = jointConnectionsData.Value;
                    ObjectController otherObjectController = GameStateData.GetObjectInLocation(connectionData.ConnectedObjectInstanceId);
                    var otherJointPoints = Helper.GetJointPoints(otherObjectController.RootGameObject);
                    JointPoint otherJointPoint = otherJointPoints[connectionData.ConnectedObjectJointPointId];
                    jointBehaviour.ConnectToJointPoint(myJointPoint, otherJointPoint);
                    myJointPoint.IsForceLocked = connectionData.ForceLocked;
                    otherJointPoint.IsForceLocked = connectionData.ForceLocked;
                }

            }

            yield return new WaitForEndOfFrame();

            JointBehaviour.IsTempConnectionCreated = false;
            yield return true;
        }

        public delegate void ObjectUpdate(GameEntity entity, ObjectDto dto);
        public event ObjectUpdate OnUpdateObject;
        
        public void UpdateObject(ObjectDto objectDto)
        {
            GameEntity entityObject = GameStateData.GetEntity(objectDto.InstanceId);
            entityObject.name.Value = objectDto.Name;
            OnUpdateObject?.Invoke(entityObject, objectDto);
        }

        public void LocationAdded(ProjectSceneArguments newLocation)
        {
            ProjectData.ProjectStructure.Scenes.Add(newLocation.Scene);
            ProjectData.ProjectStructure.UpdateOrAddLocationPrefab(newLocation.Location);
        }
        
        public void LocationChanged(ProjectSceneArguments changedLocation)
        {
            ProjectData.ProjectStructure.UpdateProjectScene(changedLocation.Scene);
            ProjectData.ProjectStructure.UpdateOrAddLocationPrefab(changedLocation.Location);

            if (ProjectData.SceneId == changedLocation.Scene.Id && ProjectData.LocationId != changedLocation.Scene.LocationId)
            {
                LoaderAdapter.LoadProject(ProjectData.ProjectId, changedLocation.Scene.Id, ProjectData.ProjectConfigurationId);
            }
        }

        public void LocationDeleted(ProjectSceneArguments deletedLocation)
        {
            if (ProjectData.SceneId == deletedLocation.Scene.Id)
            {
                LogManager.GetCurrentClassLogger().Error("Current location was deleted");
                string message = LanguageManager.Instance.GetTextValue("CURRENT_LOCATION_DELETED");
                VRErrorManager.Instance.ShowFatal(message);
            }
            else
            {
                ProjectData.ProjectStructure.RemoveProjectScene(deletedLocation.Scene);
            }
        }

        public void ConfigurationAdded(ProjectConfiguration projectConfiguration)
        {
            ProjectData.ProjectStructure.ProjectConfigurations.Add(projectConfiguration);
        }
        
        public void ConfigurationDeleted(ProjectConfiguration projectConfiguration)
        {
            ProjectData.ProjectStructure.RemoveProjectConfiguration(projectConfiguration);
        }
        
        public void ConfigurationChanged(ProjectConfiguration projectConfiguration)
        {
            ProjectData.ProjectStructure.UpdateProjectConfiguration(projectConfiguration);
        }
        
        private void HidePopUpAndToolTips()
        {
            PopupWindowManager.ClosePopup();
            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Both, ControllerTooltipManager.TooltipButtons.Grip);
            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Both, ControllerTooltipManager.TooltipButtons.Trigger);
            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Both, ControllerTooltipManager.TooltipButtons.Touchpad);
            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Both, ControllerTooltipManager.TooltipButtons.ButtonOne);
            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Both, ControllerTooltipManager.TooltipButtons.ButtonTwo);
            TooltipManager.HideControllerTooltip(ControllerTooltipManager.TooltipControllers.Both, ControllerTooltipManager.TooltipButtons.StartMenu);
        }

       
    }

    
}
