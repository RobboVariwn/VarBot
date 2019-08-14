using System;
using System.Collections.Generic;
using NLog;
using UnityEngine;
using Varwin.Data;
using Varwin.Data.ServerData;
using Varwin.WWW;

namespace Varwin
{
    
    public static class ProjectData
    {
        /// <summary>
        /// Current project id
        /// </summary>
        public static int ProjectId { get; private set; }

        /// <summary>
        /// Current scene id
        /// </summary>
        public static int SceneId { get; private set; }
        
        /// <summary>
        /// Current location id
        /// </summary>
        public static int LocationId => ProjectStructure.Scenes.GetLocationId(SceneId);

        /// <summary>
        /// Current project configuration id
        /// </summary>
        public static int ProjectConfigurationId { get; private set; }

        /// <summary>
        /// Object Id in hand
        /// </summary>
        public static int SelectedObjectIdToSpawn { get; set; }

        /// <summary>
        /// All objects are loaded
        /// </summary>
        public static bool ObjectsAreLoaded { get;  set; }

        /// <summary>
        /// User modify something?
        /// </summary>
        public static bool ObjectsAreChanged { get; set; }

        /// <summary>
        /// Current project structure data
        /// </summary>
        public static ProjectStructure ProjectStructure { get; set; }

        public delegate void ObjectsLoadedHandler ();
        /// <summary>
        /// Action when objects are loaded
        /// </summary>
        public static event ObjectsLoadedHandler ObjectsLoaded;

        
        public delegate void SceneLoadedHandler ();

        /// <summary>
        /// Action when location is loaded
        /// </summary>
        public static event SceneLoadedHandler SceneLoaded;
        
        public delegate void SceneClearedHandler ();
        /// <summary>
        /// Action when objects are loaded
        /// </summary>
        public static event SceneClearedHandler SceneCleared;

        /// <summary>
        /// Action when user save data
        /// </summary>
        public static Action OnSave { get; set; }

        public delegate void GameModeChangeHandler(GameMode newGameMode);
        public delegate void PlatformModeChangedHandler(PlatformMode newPlatformMode);

        public static event GameModeChangeHandler GameModeChanged;
        public static event PlatformModeChangedHandler PlatformModeChanged;

        private static GameMode _gm = GameMode.Undefined;
        private static PlatformMode _platform = PlatformMode.Vr;

        public static Dictionary<int, JointData> Joints { get; set; }


        public static GameMode GameMode
        {
            get => _gm;
            set => GameModeChange(value);
        }
        
        public static PlatformMode PlatformMode
        {
            get => _platform;
            set => PlatformModeChange(value);
        }

        public static bool IsMobileVr()
        {
#if UNITY_EDITOR || !UNITY_ANDROID
            return false;
#else
            return ProjectData.ProjectStructure.IsMobile && ProjectData.PlatformMode == PlatformMode.Vr;
#endif
        }

        public static void UpdateSubscribes(RequiredProjectArguments arguments)
        {
            if (ProjectId != arguments.ProjectId && LoaderAdapter.LoaderType == typeof(ApiLoader))
            {
                AMQPClient.UnSubscribeProjectSceneChange(ProjectId);
                AMQPClient.UnSubscribeProjectConfigurationsChange(ProjectId);
                AMQPClient.SubscribeSceneChange(arguments.ProjectId);
                AMQPClient.SubscribeProjectConfigurationChange(arguments.ProjectId);
            }
            
            ProjectId = arguments.ProjectId;
            SceneId = arguments.SceneId;
            ProjectConfigurationId = arguments.ProjectConfigurationId;
            GameMode = arguments.GameMode;
            PlatformMode = arguments.PlatformMode;
            Debug.Log("Project data was updated! " + arguments);
        }

        private static void PlatformModeChange(PlatformMode newValue)
        {
            if (newValue == _platform)
            {
                return;
            }

            _platform = newValue;
            PlatformModeChanged?.Invoke(_platform);

        }

        private static void GameModeChange(GameMode newValue)
        {
            if (newValue == _gm)
            {
                return;
            }

            GameMode oldGm = _gm;
            _gm = newValue;
            Helper.HideUi();
           
            GameStateData.GameModeChanged(_gm, oldGm);

            if (ProjectDataListener.Instance != null)
            {
                ProjectDataListener.Instance.RestoreJoints(Joints);
            }

            if (oldGm == GameMode.Preview && newValue == GameMode.Edit)
            {
                Helper.ReloadSceneObjects();
            }
            
            GameModeChanged?.Invoke(_gm);
            LogManager.GetCurrentClassLogger().Info($"<Color=Yellow>Game mode changed to {_gm.ToString()}</Color>");
        }


        public static void ObjectsWasLoaded()
        {
            ObjectsLoaded?.Invoke();
        }

        public static void SceneWasLoaded()
        {
            SceneLoaded?.Invoke();
            ProjectDataListener.Instance.ReadyToGetNewMessages();
        }

        public static void OnSceneCleared()
        {
            SceneCleared?.Invoke();
        }
    }

    public enum GameMode
    {
        Undefined = 1,
        Edit = 2,
        Preview = 3,
        View = 0,
    }

    public enum PlatformMode
    {
        Vr = 1,
        Desktop = 2
    }
}
