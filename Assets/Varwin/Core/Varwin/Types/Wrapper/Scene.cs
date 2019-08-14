using System;
using NLog;
using Varwin.Data.ServerData;

// ReSharper disable once CheckNamespace
namespace Varwin 
{
    public static class Scene
    {
        private static string _lastSid = String.Empty;

        public static void Load(string sid)
        {
            if (_lastSid == sid)
            {
                return;
            }
            
            var location = ProjectData.ProjectStructure.GetProjectScene(sid);
            
            if (location == null)
            {
                LogManager.GetCurrentClassLogger().Error("Location not found!");
                return;
            }
            
            _lastSid = sid;
            LoaderAdapter.LoadProject(ProjectData.ProjectId, location.Id, ProjectData.ProjectConfigurationId);
            ProjectData.SceneCleared += ResetLastSid;

            void ResetLastSid()
            {
                ProjectData.SceneCleared -= ResetLastSid;
                _lastSid = string.Empty;
            }
        }

        
    }
    
    public static class Configuration
    {
        private static string _lastSid = String.Empty;

        public static void Load(string sid)
        {
            if (_lastSid == sid)
            {
                return;
            }
             
            var projectConfiguration = ProjectData.ProjectStructure.GetConfiguration(sid);
            
            if (projectConfiguration == null)
            {
                LogManager.GetCurrentClassLogger().Error("Location not found!");
                return;
            }
            
            _lastSid = sid;
            LoaderAdapter.LoadProjectConfiguration(projectConfiguration.Id);
            ProjectData.SceneCleared += ResetLastSid;
            
            void ResetLastSid()
            {
                ProjectData.SceneCleared -= ResetLastSid;
                _lastSid = string.Empty;
            }
            
        }  

        
    }
}
