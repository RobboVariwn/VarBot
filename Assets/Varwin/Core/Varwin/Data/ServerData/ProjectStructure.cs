using System;
using System.Collections.Generic;
using NLog;
using Varwin.Models.Data;

namespace Varwin.Data.ServerData
{
    public class ProjectStructure : IJsonSerializable
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public List<Scene> Scenes { get; set; }
        public List<ProjectConfiguration> ProjectConfigurations { get; set; }
        public List<LocationPrefab> Locations { get; set; }
        public List<PrefabObject> Objects { get; set; }
        
        public bool MobileReady;
        
        /// <summary>
        /// Returns whether current platform is Mobile
        /// </summary>
        public bool IsMobile
        {
            get
            {
#if UNITY_EDITOR || !UNITY_ANDROID
                return false;
#else
                return true;
#endif
            }
        }

        /// <summary>
        /// Returns whether current project is Mobile ready
        /// </summary>
        public bool IsMobileReady => MobileReady;
    }

    public class ProjectSceneWithPrefabObjects : IJsonSerializable
    {
        public Scene Scene;
        public LocationPrefab Location;
    }

    public class Scene : IJsonSerializable
    {
        /// <summary>
        /// Instance id of location
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Guid of scene in world structure
        /// </summary>
        public string Sid { get; set; }
        
        /// <summary>
        /// Location name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Location prefab id
        /// </summary>
        public int LocationId { get; set; }
        
        /// <summary>
        /// LocationGuid
        /// </summary>
        public string LocationGuid { get; set; }
        
        /// <summary>
        /// C# logic Code
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// C# logic AssemblyBytes
        /// </summary>
        public byte[] AssemblyBytes { get; set; }
        
        /// <summary>
        /// EditorData (Blockly)
        /// </summary>
        public EditorData EditorData { get; set; }
        
        /// <summary>
        /// Location objects
        /// </summary>
        public List<ObjectDto> SceneObjects { get; set; }
        
        /// <summary>
        /// Resources
        /// </summary>
        public string Resources { get; set; }

        /// <summary>
        /// Resource path to scene logic assembly
        /// </summary>
        public string LogicResource => Resources + "/logic_assembly.dll";
    }

    public class ProjectSceneArguments
    {
        public Scene Scene = null;
        public LocationPrefab Location = null;
        public StateProjectScene State;
         
        public enum StateProjectScene
        {
            Added, Deleted, Changed
        }
    }
    
    public class ProjectConfigurationArguments
    {
        public ProjectConfiguration ProjectConfiguration = null;
        public StateConfiguration State;
         
        public enum StateConfiguration
        {
            Added, Deleted, Changed
        }
    }

    public class ProjectConfiguration : IJsonSerializable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sid { get; set; }
        public int ProjectId { get; set; }
        public int StartSceneId { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }

    public class LocationPrefab
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Resources { get; set; }
        
        public string BundleResource => Resources + "/bundle";
        public string AndroidBundleResource => Resources + "/android_bundle";
        public string ConfigResource => Resources + "/bundle.json";
        public string IconResource => Resources + "/bundle.png";
        public string ManifestResource => Resources + "/bundle.manifest";
        public string AndroidManifestResource => Resources + "/android_bundle.manifest";
    }

    public class ObjectData
    {
        public Dictionary<int, TransformDT> Transform { get; set; }
        public JointData JointData { get; set; }
    }

    public class JointData
    {
        public Dictionary<int, JointConnetionsData> JointConnetionsData { get; set; }
    }

    public class JointConnetionsData
    {
        /// <summary>
        /// Connected object instance id
        /// Какой объект подключен
        /// </summary>
        public int ConnectedObjectInstanceId { get; set; }

        /// <summary>
        /// Connected object point
        /// К какой точке
        /// </summary>
        public int ConnectedObjectJointPointId { get; set; }
        
        /// <summary>
        /// Joint point force lock
        /// </summary>
        public bool ForceLocked { get; set; }
    }

    public class EditorData
    {
        public string Blockly { get; set; }
    }

    


    #region Ex
    public static class ProjectStructureEx
    {
        public static Scene GetProjectScene(this List<Scene> self, int id)
        {
            foreach (Scene location in self)
            {
                if (location.Id == id)
                {
                    return location;
                }
            }

            return null;
        }

        public static int GetLocationId(this List<Scene> self, int projectSceneId)
        {
            Scene scene = self.GetProjectScene(projectSceneId);
            return scene?.LocationId ?? 0;
        }

        public static Scene GetProjectScene(this List<Scene> self, string sid)
        {
            Guid guid = new Guid(sid);

            foreach (Scene projectScene in self)
            {
                Guid sidGuid = new Guid(projectScene.Sid);

                if (sidGuid == guid)
                {
                    return projectScene;
                }
            }

            return null;
        }
        
        public static ProjectConfiguration GetProjectConfigurationByConfigurationSid(this List<ProjectConfiguration> self, string sid)
        {
            Guid guid = new Guid(sid);

            foreach (ProjectConfiguration projectConfiguration in self)
            {
                Guid sidGuid = new Guid(projectConfiguration.Sid);

                if (sidGuid == guid)
                {
                    return projectConfiguration;
                }
            }

            return null;
        }

        public static ProjectConfiguration GetProjectConfigurationByProjectScene(this List<ProjectConfiguration> self, int projectSceneId)
        {
            foreach (ProjectConfiguration projectConfiguration in self)
            {
                if (projectConfiguration.Id == projectSceneId)
                {
                    return projectConfiguration;
                }
            }

            return null;
        }

        public static List<string> GetNames(this List<ProjectConfiguration> self)
        {
            List<string> names = new List<string>();
            foreach (ProjectConfiguration worldConfiguration in self)
            {
                names.Add(worldConfiguration.Name);
            }

            return names;
        }

        public static int GetId(this List<ProjectConfiguration> self, string name)
        {
             
            foreach (ProjectConfiguration worldConfiguration in self)
            {
                if (worldConfiguration.Name == name)
                {
                    return worldConfiguration.Id;
                }
            }

            return 0;
        }

        public static Scene GetProjectScene(this ProjectStructure self, string projectSceneSid)
        {
            Scene scene = self.Scenes.GetProjectScene(projectSceneSid);
            return scene;
        }
        
        public static ProjectConfiguration GetConfiguration(this ProjectStructure self, string configurationId)
        {
            ProjectConfiguration worldConfiguration = self.ProjectConfigurations.GetProjectConfigurationByConfigurationSid(configurationId);
            return worldConfiguration;
        }

        public static void UpdateEntities(List<ObjectDto> objects)
        {
            foreach (ObjectDto dto in objects)
            {
                GameEntity entity = GameStateData.GetEntity(dto.InstanceId);
                entity.ReplaceIdServer(dto.Id);
                entity.ReplaceName(dto.Name);
                ObjectController objectController = GameStateData.GetObjectInLocation(dto.InstanceId);
                objectController.IdServer = dto.Id;
                UpdateEntities(dto.SceneObjects);
            }
        }

        public static void UpdateProjectSceneObjects(this Scene self, List<ObjectDto> objects)
        {
            UpdateEntities(objects);
            self.SceneObjects = objects;
            ProjectData.ObjectsAreChanged = false;
            LogManager.GetCurrentClassLogger().Info($"Project scene objects {ProjectData.SceneId} was updated in structure!");
        }

        public static LocationPrefab GetProjectScene(this List<LocationPrefab> self, int id)
        {
            foreach (LocationPrefab locationPrefab in self)
            {
                if (locationPrefab.Id == id)
                {
                    return locationPrefab;
                }
            }

            return null;
        }

        public static PrefabObject GetById(this List<PrefabObject> self, int id)
        {
            foreach (PrefabObject o in self)
            {
                if (o.Id == id)
                {
                    return o;
                }
            }

            return null;
        }

        public static void RemoveProjectScene(this ProjectStructure self, Scene deletedLocation)
        {
            Scene result = null; 
            foreach (Scene scene in self.Scenes)
            {
                    
                if (scene.Id == deletedLocation.Id)
                {
                    result = scene;
                }
            }

            if (result != null)
            {
                self.Scenes.Remove(result);
            }
        }
        
        public static void UpdateProjectScene(this ProjectStructure self, Scene changedLocation)
        {
            for (int i = 0; i < self.Scenes.Count; i++)
            {
                if (self.Scenes[i].Id == changedLocation.Id)
                {
                    self.Scenes[i] = changedLocation;
                }
            }
        }
        
        public static void UpdateOrAddLocationPrefab(this ProjectStructure self, LocationPrefab changedLocationPrefab)
        {
            for (int i = 0; i < self.Locations.Count; i++)
            {
                if (self.Locations[i].Id == changedLocationPrefab.Id)
                {
                    self.Locations[i] = changedLocationPrefab;
                    return;
                }
            }
            
            self.Locations.Add(changedLocationPrefab);
        }
        
        public static void RemoveProjectConfiguration(this ProjectStructure self, ProjectConfiguration deletedConfiguration)
        {
            ProjectConfiguration result = null; 
            foreach (ProjectConfiguration worldConfiguration in self.ProjectConfigurations)
            {
                    
                if (worldConfiguration.Id == deletedConfiguration.Id)
                {
                    result = worldConfiguration;
                }
            }

            if (result != null)
            {
                self.ProjectConfigurations.Remove(result);
            }
        }
        
        public static void UpdateProjectConfiguration(this ProjectStructure self, ProjectConfiguration changedConfiguration)
        {
            for (int i = 0; i < self.ProjectConfigurations.Count; i++)
            {
                if (self.ProjectConfigurations[i].Id == changedConfiguration.Id)
                {
                    self.ProjectConfigurations[i] = changedConfiguration;
                }
            }
        }
    }
    #endregion

}
