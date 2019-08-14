using System;
using System.Collections.Generic;
using NLog;
using UnityEngine;
using Varwin.Data;
using Varwin.Data.ServerData;
using Varwin.Onboarding;
using Logger = NLog.Logger;

namespace Varwin
{
    public static class LoaderAdapter
    {
        private static BaseLoader _loader;

        public delegate void ProgressUpdate(float val);

        public static ProgressUpdate OnLoadingUpdate;
        public static ProgressUpdate OnDownLoadUpdate;
        public static Type LoaderType => _loader != null ? _loader.GetType() : null;
        public static string FeedBackText => _loader.FeedBackText;
        private static RequiredProjectArguments _requiredProjectArguments;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        

        public static void Init(BaseLoader loader)
        {
            _loader = loader;
        }

        public static void LoadPrefabObjects(List<PrefabObject> objects)
        {
            ProjectData.ObjectsAreLoaded = false;
            var requiredObjects = _loader.GetRequiredObjects(objects);
            Logger.Info($"<Color=Olive><b>" + "Load SceneObjects started..." + "</b></Color>");
            _loader.LoadObjects(requiredObjects);
        }
 
        public static void LoadProjectStructure(int projectId, Action<ProjectStructure> onFinish)
        {
            _loader.LoadProjectStructure(projectId, onFinish);
        }

        private static void LoadScene(int locationId)
        {
            Logger.Info($"<Color=Olive><b>" + "Load Scene started..." + "</b></Color>");
            _loader.LoadLocation(locationId);
        }

        private static void CheckSceneId()
        {
            if (_requiredProjectArguments.SceneId != 0 || ProjectData.ProjectStructure == null)
            {
                return;
            }

            ProjectConfiguration configuration = ProjectData.ProjectStructure.ProjectConfigurations.GetProjectConfigurationByProjectScene(_requiredProjectArguments.ProjectConfigurationId);
            _requiredProjectArguments.SceneId = configuration.StartSceneId != 0 ? configuration.StartSceneId : ProjectData.ProjectStructure.Scenes[0].Id;
        }

        private static bool _isOtherScene = true;
        private static void LoadProjectScene()
        {
            _isOtherScene = true;
            
            if (ProjectData.ProjectStructure != null)
            {
                CheckSceneId();
                _isOtherScene = ProjectData.SceneId != _requiredProjectArguments.SceneId;
            }
            
            Logger.Info($"<Color=Olive><b>" + $"Other scene: {_isOtherScene}" + "</b></Color>");
            
            ProjectDataListener.Instance.BeforeSceneLoaded(_isOtherScene);
            ProjectData.SceneCleared += OnSceneCleared;
            ProjectData.SceneLoaded += OnBoarding;

            void OnSceneCleared()
            {

                ProjectData.SceneCleared -= OnSceneCleared;
                ProjectData.ObjectsAreLoaded = false;
                ProjectData.ObjectsLoaded += PrefabObjectsLoaded;

                if (ProjectData.ProjectId != _requiredProjectArguments.ProjectId)
                {
                    
                    LoadProjectStructure(_requiredProjectArguments.ProjectId, OnLoadProjectStructure);
                    
                    void OnLoadProjectStructure(ProjectStructure projectStructure)
                    {
                        ProjectData.ProjectStructure = projectStructure;
                        CheckSceneId();
                        List<PrefabObject> usingObjects = GetUsingObjects(_requiredProjectArguments.SceneId);
                        LoadPrefabObjects(usingObjects);
                    }
                }
                else
                {
                    List<PrefabObject> usingObjects = GetUsingObjects(_requiredProjectArguments.SceneId);
                    LoadPrefabObjects(usingObjects);
                }
            }
        }

        private static void OnBoarding()
        {
            if (!Settings.Instance().OnboardingMode || OnboardingManager.Instance != null)
            {
                return;
            }
            
            GameObject tutorial = new GameObject("Tutorial Mode");
            OnboardingManager manager = tutorial.AddComponent<OnboardingManager>();
            manager.Init(_requiredProjectArguments.GameMode);
        }

        private static List<PrefabObject> GetUsingObjects(int sceneId)
        {
            CheckSceneId();
            Data.ServerData.Scene currentLocation = ProjectData.ProjectStructure.Scenes.Find(location => location.Id == sceneId);
            List<PrefabObject> usingObjects = new List<PrefabObject>();

            foreach (PrefabObject prefabObject in ProjectData.ProjectStructure.Objects)
            {
                foreach (ObjectDto objectDto in currentLocation.SceneObjects)
                {
                    if (objectDto.ObjectId != prefabObject.Id)
                    {
                        continue;
                    }

                    if (!usingObjects.Contains(prefabObject))
                    {
                        usingObjects.Add(prefabObject);
                    }
                }
            }

            return usingObjects;
        }

        private static void PrefabObjectsLoaded()
        {
            if (_isOtherScene)
            {
                if (ProjectData.ProjectStructure == null)
                {
                    return;
                }
                int locationId = ProjectData.ProjectStructure.Scenes.GetProjectScene(_requiredProjectArguments.SceneId).LocationId;
                ProjectData.SceneLoaded += OnLoadScene;
            
                LoadScene(locationId);
            
                void OnLoadScene()
                {
                    Helper.SpawnSceneObjects(_requiredProjectArguments.SceneId, ProjectData.SceneId);
                    ProjectData.UpdateSubscribes(_requiredProjectArguments);
                    ProjectData.SceneLoaded -= OnLoadScene;
                }
            }

            else
            {
                ProjectData.SceneWasLoaded();
                Helper.SpawnSceneObjects(_requiredProjectArguments.SceneId, ProjectData.SceneId);
                ProjectData.UpdateSubscribes(_requiredProjectArguments);
            }
            
            ProjectData.ObjectsLoaded -= PrefabObjectsLoaded;
        }

        public static void LoadProject(LaunchArguments launchArguments)
        {
            _requiredProjectArguments.ProjectId = launchArguments.projectId;
            _requiredProjectArguments.SceneId = launchArguments.sceneId;
            _requiredProjectArguments.ProjectConfigurationId = launchArguments.projectConfigurationId;
            _requiredProjectArguments.GameMode = (GameMode) launchArguments.gm;
            _requiredProjectArguments.PlatformMode = (PlatformMode) launchArguments.platformMode;
            //Force it to be VR or Desktop from the beginning
            ProjectData.PlatformMode = (PlatformMode) launchArguments.platformMode;
            
            LoadProjectScene();
        }

        public static void LoadProject(int projectId, int sceneId, int projectConfigurationId)
        {
            _requiredProjectArguments.ProjectId = projectId;
            _requiredProjectArguments.SceneId = sceneId;
            _requiredProjectArguments.ProjectConfigurationId = projectConfigurationId;
            _requiredProjectArguments.GameMode = ProjectData.GameMode;
            _requiredProjectArguments.PlatformMode = ProjectData.PlatformMode;
            LoadProjectScene();
        }

        public static void LoadProjectConfiguration(int projectConfigurationId)
        {
            ProjectConfiguration projectConfiguration = ProjectData.ProjectStructure.ProjectConfigurations.Find(configuration => configuration.Id == projectConfigurationId);
            LoadProject(projectConfiguration.ProjectId, projectConfiguration.StartSceneId, projectConfigurationId);
        }

    }

}
