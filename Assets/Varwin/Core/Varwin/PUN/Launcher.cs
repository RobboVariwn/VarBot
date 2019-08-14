using System;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Tar;
using Varwin.Errors;
using NLog;
using NLogger;
using Photon;
using SmartLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Varwin.Data;
using Varwin.Data.ServerData;
using Varwin.ECS.Systems.Loader;
using Varwin.UI;
using Varwin.VRInput;
using Varwin.WWW;

namespace Varwin.PUN
{
    public class Launcher : PunBehaviour
    {
        #region PUBLIC VARS

        public static Launcher Instance;

        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        public GameObject ControlPanel;

        [Tooltip("The Ui Text to inform the user about the connection progress")]
        public TMP_Text FeedbackText;

        [Tooltip("CommandLine Arguments")]
        public Text ArgsText;

        [Tooltip("The maximum number of players per room")]
        public byte MaxPlayersPerRoom = 4;

        [Tooltip("The UI Loader Anime")]
        public LoaderAnime LoaderAnime;


        public string Language;
        public bool LoadTarFile;
        public string TarFilePath;

        #endregion

        #region PRIVATE VARS

        private bool _isConnecting;
        string _gameVersion = "1";
        private NLog.Logger _logger;

        #endregion

        private void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + Time.time + " seconds");
            AMQPClient.CloseConnection();
        }

        private void Awake()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Launcher started");

            #region Magic code

#if !NET_STANDARD_2_0
#pragma warning disable 219
            dynamic inside = "Create Microsoft.CSharp.dll in project folder";
            _logger.Info(inside);
#pragma warning restore 219
#endif

            #endregion

            Instance = this;
            NLoggerSettings.Init();
        }

        private void Start()
        {
            PlayerNameInputField inputField = FindObjectOfType<PlayerNameInputField>();
            string userName = Environment.UserName;
            inputField.SetPlayerName(userName);
            LoaderAnime.gameObject.transform.position = new Vector3(0, (float) Screen.height / 1000 - 1, 0);
            LanguageManager.SetDontDestroyOnLoad();
            Contexts.sharedInstance.game.DestroyAllEntities();

#if !UNITY_EDITOR
	        ReadStartUpSettings();
#else
            Settings.ReadTestSettings();

            Rabbitmq amqmSettings = new Rabbitmq
            {
                host = Settings.Instance().RabbitMqHost + ":" + Settings.Instance().RabbitMqPort,
                login = Settings.Instance().RabbitMqUserName,
                password = Settings.Instance().RabbitMqPass,
                key = "debug"
            };

            AMQPClient.Init(amqmSettings);
#endif


#if UNITY_EDITOR
            if (LoadTarFile)
            {
                Settings.CreateStorageSettings(TarFilePath);
                StartLoadFile();

                return;
            }

#endif

            LoaderSystems loaderSystems = new LoaderSystems(Contexts.sharedInstance);
            loaderSystems.Initialize();
            ProjectDataListener.OnUpdate = () => loaderSystems.Execute();

            if (LoaderAdapter.LoaderType != typeof(ApiLoader))
            {
                return;
            }

            try
            {
                AMQPClient.ReadLaunchArgs();
            }
            catch (Exception e)
            {
                LauncherErrorManager.Instance.ShowFatalErrorKey(
                    ErrorHelper.GetErrorKeyByCode(Varwin.Errors.ErrorCode.RabbitNoArgsError),
                    e.StackTrace);
                _logger.Fatal("Can not read launch args in rabbitMq");
            }
        }

        private void StartLoadFile()
        {
            LogManager.GetCurrentClassLogger().Info("File name = " + Settings.Instance().StoragePath);
            ProjectData.GameMode = GameMode.View;

            if (LoaderAnime != null)
            {
                LoaderAnime.StartLoaderAnimation();
            }

            LoaderAdapter.LoadProjectStructure(0, OnProjectStructureRead);


            void OnProjectStructureRead(ProjectStructure projectStructure)
            {
                ProjectData.ProjectStructure = projectStructure;

                if (ProjectData.ProjectStructure.ProjectConfigurations.Count == 0)
                {
                    LauncherErrorManager.Instance.Show(
                        ErrorHelper.GetErrorDescByCode(Errors.ErrorCode.ProjectConfigNullError),
                        null);

                    return;
                }

                if (ProjectData.ProjectStructure.ProjectConfigurations.Count == 1)
                {
                    var projectConfigurationId = ProjectData.ProjectStructure.ProjectConfigurations[0].Id;
                    LoadConfigurationFromStorage(projectConfigurationId);
                }
                else
                {
                    UIChooseProjectConfig.Instance.UpdateDropDown(
                        ProjectData.ProjectStructure.ProjectConfigurations.GetNames(),
                        LoadConfigurationFromStorage);
                    UIChooseProjectConfig.Instance.Show();
                    LoaderAnime.StopLoaderAnimation();
                }
            }
        }

        private void LoadConfigurationFromStorage(int projectConfigurationId)
        {
            LoaderAnime.StartLoaderAnimation();
            LoaderSystems loaderSystems = new LoaderSystems(Contexts.sharedInstance);
            loaderSystems.Initialize();
            ProjectDataListener.OnUpdate = () => loaderSystems.Execute();
            LoaderAdapter.LoadProjectConfiguration(projectConfigurationId);
        }

        // Init rabbit from first launch not on editor!
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private void ReadStartUpSettings()
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                string amqpBase64 = args[1];
                string amqpJson = Base64Decode(amqpBase64);
                _logger.Info(amqpJson);    //currently it's somewhat the only way to debug client
                Rabbitmq rabbitmq = amqpJson.JsonDeserialize<Rabbitmq>();
                AMQPClient.Init(rabbitmq);
                LoaderAdapter.Init(new ApiLoader());
                LogManager.GetCurrentClassLogger().Info("RabbitMQ init settings = " + amqpJson);
            }
            catch (Exception e)
            {
                try
                {
                    ReadStartUpFile();
                }
                catch (Exception exception)
                {
                    string message = "Cant read AMQP settings! Trying to get file params";
                    LogManager.GetCurrentClassLogger().Fatal(message + " stackTrace = " + e.StackTrace);
                    LauncherErrorManager.Instance.ShowFatal(message, exception.StackTrace);
                }
            }
        }

        private void ReadStartUpFile()
        {
            LogManager.GetCurrentClassLogger().Info("Opening file...");
            string[] args = Environment.GetCommandLineArgs();
            string file = args[1];
            Settings.CreateStorageSettings(file);
            StartLoadFile();
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private void LogFeedback(string message)
        {
            if (FeedbackText == null)
            {
                return;
            }

            FeedbackText.text = message;
        }

        #region Photon.PunBehaviour CallBacks

        public override void OnConnectedToMaster()
        {
            Debug.Log("Region:" + PhotonNetwork.networkingPeer.CloudRegion);

            if (_isConnecting)
            {
                LogFeedback(LanguageManager.Instance.GetTextValue("TRY_TO_CONNECT_TO_RANDOM_ROOM"));

                Debug.Log(
                    "DemoAnimator/Launcher: OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room."
                    + "\n Calling: PhotonNetwork.JoinRandomRoom(); Operation will fail if no room found");
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            LogFeedback(LanguageManager.Instance.GetTextValue("ROOM_CREATING"));

            Debug.Log(
                "DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one."
                + "\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
            PhotonNetwork.CreateRoom(null, new RoomOptions() {MaxPlayers = MaxPlayersPerRoom}, null);
        }

        public override void OnDisconnectedFromPhoton()
        {
            LauncherErrorManager.Instance.Show(
                ErrorHelper.GetErrorDescByCode(Errors.ErrorCode.PhotonServerDisconnectError),
                null);
            _logger.Fatal("Photon disconect");
            LoaderAnime.StopLoaderAnimation();
            _isConnecting = false;
            ControlPanel.SetActive(true);
        }

        public override void OnJoinedRoom()
        {
            LogFeedback(LanguageManager.Instance.GetTextValue("ENTER_ROOM"));

            Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room."
                      + "\nFrom here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");
        }

        #endregion
    }
}
