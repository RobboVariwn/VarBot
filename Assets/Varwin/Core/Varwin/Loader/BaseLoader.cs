using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NLog;
using SmartLocalization;
using UnityEngine;
using Varwin.Data.ServerData;
using Varwin.Errors;
using Varwin.UI;
using Varwin.UI.VRErrorManager;
using Varwin.WWW;
using Logger = NLog.Logger;
using Object = System.Object;

namespace Varwin.Data
{
    public abstract class BaseLoader
    {
        #region ABSTRACT METHODS
        /// <summary>
        /// Load list of objects
        /// </summary>
        /// <param name="objects">Prefab object data with resources links</param>
        public abstract void LoadObjects(List<PrefabObject> objects);
        
        /// <summary>
        /// Load scene asset
        /// </summary>
        /// <param name="locationId">Location Id (location prefab)</param>
        public abstract void LoadLocation(int locationId);
        
        /// <summary>
        /// Load/Get world structure data
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="onFinish"></param>
        public abstract void LoadProjectStructure(int projectId, Action<ProjectStructure> onFinish);
        #endregion

        #region PROTECTED FIELDS
        /// <summary>
        /// Current class logger
        /// </summary>
        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Count of current loaded (spawn ready) prefabs (LoadObjectsCounter.cs)
        /// </summary>
        protected GameEntity LoadCounter { get; private set; }
        
        /// <summary>
        /// Count of current loaded assets 
        /// </summary>
        protected int CountLoadedObjects;

        #endregion
        
        /// <summary>
        /// Loader processes realtime feedback
        /// </summary>
        public string FeedBackText { get; set; }
        
        /// <summary>
        /// Write assembly to disk and load
        /// </summary>
        /// <param name="dllPath">Path to write dll assembly</param>
        /// <param name="dllName">Dll file name</param>
        /// <param name="byteData">Assembly bytes</param>
        protected bool AddAssembly(string dllPath, string dllName, ref byte[] byteData)
        {
            if (!Directory.Exists(dllPath))
            {
                Directory.CreateDirectory(dllPath);
            }

            string dllFileName = dllPath + "/" + dllName;


            try
            {
                File.WriteAllBytes(dllFileName, byteData);
            }
            catch (Exception e)
            {
                Logger.Fatal($"Can not write assembly {dllName} to cache Error: {e.Message}");

                return false;
            }           

            try
            {
                Assembly assembly = Assembly.Load(byteData);
                GameStateData.AddAssembly(assembly.ManifestModule.Name, dllFileName);

                foreach (Type exportedType in assembly.GetExportedTypes())
                { 
                    Logger.Info(exportedType + " is loaded!");
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Fatal($"Can not load assembly {dllName} Error: {e.Message}");

                return false;
            }
            
        }

        /// <summary>
        /// Sort objects if already loaded
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public List<PrefabObject> GetRequiredObjects(List<PrefabObject> objects)
        {
            List<PrefabObject> result = new List<PrefabObject>(objects);
            List<PrefabObject> alreadyLoaded = GameStateData.GetPrefabsData();
            List<PrefabObject> resultRemove = new List<PrefabObject>();

            foreach (PrefabObject o in result)
            {
                PrefabObject found = alreadyLoaded.Find(prefabObject => prefabObject.Id == o.Id);

                if (found != null)
                {
                    var loaded = result.Find(prefabObject => prefabObject.Id == o.Id);
                    resultRemove.Add(loaded);
                    Debug.Log(o.Config.i18n.en + " is already loaded");
                }

                else
                {
                    Debug.Log(o.Config.i18n.en + " is required");
                }
                
                Debug.Log(o.Config.i18n.en + "was added to WorldStructure.SceneObjects");

            }

            foreach (PrefabObject o in resultRemove)
            {
                result.Remove(o);
            }

            return result;

        }

        protected static string FindMainAssembly(AssetInfo assetInfo, PrefabObject o)
        {
            try
            {
                string result = assetInfo.Assembly.Find(x => Regex.Match(x, $"^{o.Config.type.Split('.')[0]}").Success);
                return result;
            }
            catch 
            {
                LogManager.GetCurrentClassLogger().Fatal("Can not find main assembly");
                return null;
            }
            
        }
        
        protected void ResetObjectsCounter(int count)
        {
            FeedBackText = LanguageManager.Instance.GetTextValue("LOADING") + " " + LanguageManager.Instance.GetTextValue("SCENE_OBJECTS");
            CountLoadedObjects = 0;
                
            if (LoadCounter != null && LoadCounter.hasLoadObjectsCounter)
            {
                LoadCounter.ReplaceLoadObjectsCounter(count, 0, false);
            }
            else
            {
                LoadCounter = Contexts.sharedInstance.game.CreateEntity();
                LoadCounter.AddLoadObjectsCounter(count, 0, false);
            }
        }

        protected bool LoadAssetInfo(string json, ref AssetInfo assetInfo, PrefabObject o)
        {
            LauncherErrorManager launcherErrorManager = LauncherErrorManager.Instance;
            VRErrorManager vrErrorManager = VRErrorManager.Instance;

            try
            {
                assetInfo = json.JsonDeserialize<AssetInfo>();

                if (assetInfo.AssetName != null)
                {
                    return true;
                }

                string message = $"Asset name can not be null. {o.Config.i18n.en} Bundle.json is not actual version!";
                LogManager.GetCurrentClassLogger().Fatal(message);
                RequestManager.Instance.StopRequestsWithError(message);

                if (launcherErrorManager != null)
                {
                    launcherErrorManager.ShowFatal($"{o.Config.i18n.en} not actual!", "null asset");
                }

                if (vrErrorManager != null)
                {
                    vrErrorManager.ShowFatal($"{o.Config.i18n.en} not actual!", "null asset");
                }

                return false;
            }
            catch (Exception e)
            {
                string message = $"AssetInfo can not be loaded. {o.Config.i18n.en} Bundle.json is not actual version! Bundle.json = {json}";
                LogManager.GetCurrentClassLogger().Fatal(message);
                RequestManager.Instance.StopRequestsWithError(message);

                if (launcherErrorManager != null)
                {
                    launcherErrorManager.ShowFatal($"{o.Config.i18n.en} not actual!", e.StackTrace);
                }

                if (vrErrorManager != null)
                {
                    vrErrorManager.ShowFatal($"{o.Config.i18n.en} not actual!", e.StackTrace);
                }

                return false;
            }
        }
        
        protected void CreatePrefabEntity(IResponse response, PrefabObject o, Sprite icon = null)
        {
            var alreadyLoaded = GameStateData.GetPrefabData(o.Id);

            if (alreadyLoaded != null)
            {
                LoadCounter.loadObjectsCounter.PrefabsLoaded++;
                LogManager.GetCurrentClassLogger().Info(o.Config.i18n.en + " was ignored, because it already loaded");
                return;
            }
            
            ResponseAsset responseAsset = (ResponseAsset) response;
            Object unityObject = responseAsset.Asset;
            PrefabObject serverObject = (PrefabObject) responseAsset.UserData[0];
            GameEntity entity = Contexts.sharedInstance.game.CreateEntity();
            entity.AddServerObject(serverObject);
            GameStateData.AddPrefabGameObject(serverObject.Id, responseAsset.Asset, o);
            GameStateData.AddObjectIcon(serverObject.Id, icon);
            entity.AddIcon(icon);

            if (o.Embedded)
            {
                GameStateData.AddToEmbeddedList(serverObject.Id);
            }
            
            GameObject gameObject = unityObject as GameObject;

            if (gameObject != null)
            {
                entity.AddGameObject(gameObject);
            }

            else
            {
                string message = $"Game object is null in asset {o.Config.i18n.en}";
                Logger.Fatal(message);
                RequestManager.Instance.StopRequestsWithError(message);
                return;
            }
            
            LoadCounter.loadObjectsCounter.PrefabsLoaded++;
            LogManager.GetCurrentClassLogger().Info(o.Config.i18n.en + " is loaded");
        }

        protected LocationPrefab GetLocationPrefab(int locationId)
        {
            LocationPrefab location = ProjectData.ProjectStructure.Locations.GetProjectScene(locationId);

            if (location == null)
            {
                string message = string.Format(ErrorHelper.GetErrorDescByCode(Errors.ErrorCode.EnvironmentNotFoundError), "???");
                Logger.Error(message);
                RequestManager.Instance.StopRequestsWithError(message);

                return null;
            }

            else
            {
                string message = string.Format(LanguageManager.Instance.GetTextValue("LOADING_LOCATION"), location.Name);
                Logger.Info(message);
                FeedBackText = message;
            }

            return location;
        }

        protected void ShowErrorLoadScene()
        {
            string message = ErrorHelper.GetErrorDescByCode(Varwin.Errors.ErrorCode.LoadSceneError);
            Logger.Fatal("Location is not loaded");
            RequestManager.Instance.StopRequestsWithError(message);
        }
    }
}