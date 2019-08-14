using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SmartLocalization;
using UnityEngine;
using Varwin.Data.AssetBundle;
using Varwin.Data.ServerData;
using Varwin.Errors;
using Varwin.UI;
using Varwin.WWW;

namespace Varwin.Data
{
    public class StorageLoader : BaseLoader
    {
        public override void LoadObjects(List<PrefabObject> objects)
        {
            ResetObjectsCounter(objects.Count);

            foreach (PrefabObject prefabObject in objects)
            {
                LoadObject(prefabObject);
            }
        }

        public override void LoadLocation(int locationId)
        {
            LocationPrefab location = ProjectData.ProjectStructure.Locations.GetProjectScene(locationId);
            Logger.Info($"Loading location \"{location.Name}\" from tar file");
            RequestFileRead requestConfig = new RequestFileRead(location.ConfigResource);

            requestConfig.OnFinish += responseConfig =>
            {
                SceneData sceneData = ((ResponseFileRead) responseConfig).TextData.JsonDeserialize<SceneData>();

                RequestFileRead requestFileRead = new RequestFileRead(ProjectData.ProjectStructure.IsMobile
                    ? location.AndroidBundleResource
                    : location.BundleResource);

                requestFileRead.OnFinish += response =>
                {
                    ResponseFileRead responseFileRead = (ResponseFileRead) response;
                    byte[] bundle = responseFileRead.ByteData;

                    RequestLoadSceneFromMemory request =
                        new RequestLoadSceneFromMemory(sceneData.AssetBundleLabel, bundle);

                    request.OnFinish += response1 =>
                    {
                        ResponseAsset responseAsset = (ResponseAsset) response1;
                        string scenePath = Path.GetFileNameWithoutExtension(responseAsset.Path);

                        ProjectDataListener.Instance.LoadScene(scenePath);
                    };

                    Resources.UnloadUnusedAssets();

                    request.OnError += s => { Helper.ShowErrorLoadScene(); };
                };

                requestFileRead.OnError += s => { Helper.ShowErrorLoadScene(); };
            };
        }

        public override void LoadProjectStructure(int projectId, Action<ProjectStructure> onFinish)
        {
            var request = new RequestFileRead("/index.json");

            request.OnFinish += response =>
            {
                string jsonWorldStructure = ((ResponseFileRead) response).TextData;
                var worldStructure = jsonWorldStructure.JsonDeserialize<ProjectStructure>();

                foreach (var scene in worldStructure.Scenes)
                {
                    var logicRequest = new RequestFileRead(scene.LogicResource);

                    logicRequest.OnFinish += response1 =>
                    {
                        ResponseFileRead logicResponse = (ResponseFileRead) response1;
                        scene.AssemblyBytes = logicResponse.ByteData;
                    };
                }

                onFinish.Invoke(worldStructure);
            };

            request.OnError += s =>
            {
                LauncherErrorManager.Instance.ShowFatal(ErrorHelper.GetErrorDescByCode(Errors.ErrorCode.LoadSceneError),
                    null);
                FeedBackText = $"Can't find project folder";
            };
        }

        private void LoadObject(PrefabObject o)
        {
            RequestFileRead request = new RequestFileRead(o.ConfigResource);

            request.OnFinish += response =>
            {
                string json = ((ResponseFileRead) response).TextData;
                AssetInfo assetInfo = null;

                if (!LoadAssetInfo(json, ref assetInfo, o))
                {
                    return;
                }

                string mainAssembly = FindMainAssembly(assetInfo, o);

                foreach (string dllName in assetInfo.Assembly)
                {
                    if (dllName != mainAssembly)
                    {
                        LoadDll(o, dllName);
                    }
                }

                LoadDll(o, mainAssembly);

                LoadAssetFromStorage(assetInfo, o);
            };

            request.OnError += s => { Helper.ShowErrorLoadObject(o, s); };
        }

        private void LoadDll(PrefabObject o, string dllName)
        {
            string dllCachePath = Path.Combine(Application.dataPath, "/cache/dll/" + o.Config.i18n.en + o.Guid);

            string dllPath = o.Resources + "/" + dllName;

            new RequestFileRead(dllPath).OnFinish += response1 =>
            {
                var byteData = ((ResponseFileRead) response1).ByteData;

                bool error = !AddAssembly(dllCachePath, dllName, ref byteData);

                if (!error)
                {
                    return;
                }

                string message = ErrorHelper.GetErrorDescByCode(Varwin.Errors.ErrorCode.LoadObjectError)
                                 + "\n"
                                 + o.Config.i18n.GetCurrentLocale();
                
                Logger.Fatal(message);
                FeedBackText = message;
                RequestManager.Instance.StopRequestsWithError(message);
            };
        }

        private void LoadAssetFromStorage(AssetInfo assetInfo, PrefabObject o)
        {
            string assetName = assetInfo.AssetName;
            string assetBundleUri = ProjectData.ProjectStructure.IsMobile ? o.AndroidBundleResource : o.BundleResource;

            RequestFileRead requestAssetData = new RequestFileRead(assetBundleUri);

            requestAssetData.OnFinish += responseData =>
            {
                ResponseFileRead responseFileRead = (ResponseFileRead) responseData;
                var bundleData = responseFileRead.ByteData;

                RequestLoadAssetFromMemory requestAsset =
                    new RequestLoadAssetFromMemory(assetName, bundleData, new object[] {o, assetInfo});

                requestAsset.OnFinish += response =>
                {
                    FeedBackText = LanguageManager.Instance.GetTextValue("LOADING")
                                   + " "
                                   + o.Config.i18n.GetCurrentLocale()
                                   + "...";
                    CreatePrefabEntity(response, o);
                };
            };
        }
    }
}
