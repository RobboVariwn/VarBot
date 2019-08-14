using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using SmartLocalization;
using UnityEngine;
using Varwin.Data.AssetBundle;
using Varwin.Data.ServerData;
using Varwin.Errors;
using Varwin.UI;
using Varwin.WWW;

namespace Varwin.Data
{
    public class ApiLoader : BaseLoader
    {
        public delegate void ProgressUpdate(float val);

        public ProgressUpdate OnLoadingUpdate;

        public override void LoadObjects(List<PrefabObject> objects)
        {
            Logger.Info("Scene objects list loaded (API). Number of objects: " + objects.Count);
            ResetObjectsCounter(objects.Count);
            ObjectResourses.ClearResourses();

            foreach (PrefabObject prefabObject in objects)
            {
                LoadPrefabObject(prefabObject);
            }

            Logger.Info("Required objects list has been loaded (API)");
        }

        private void LoadPrefabObject(PrefabObject o)
        {
            RequestUri request = new RequestUri(o.ConfigResource);

            request.OnFinish += response =>
            {
                string json = ((ResponseUri) response).TextData;
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

                LoadCustomAssetApi(assetInfo, o);
            };

            request.OnError += s => { Helper.ShowErrorLoadObject(o, s); };
        }

        private void LoadDll(PrefabObject o, string dllName)
        {
            string dllCachePath = FileSystemUtils.GetFilesPath(ProjectData.ProjectStructure.IsMobile, "cache/dll/")
                                  + o.Config.i18n.en
                                  + o.Guid;

            string dllPath = o.Resources + "/" + dllName;

            new RequestUri(dllPath).OnFinish += response1 =>
            {
                byte[] byteData = ((ResponseUri) response1).ByteData;

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

        public override void LoadLocation(int locationId)
        {
            LocationPrefab locationPrefab = GetLocationPrefab(locationId);

            RequestUri requestManifest = new RequestUri(ProjectData.IsMobileVr()
                ? locationPrefab.AndroidManifestResource
                : locationPrefab.ManifestResource);

            requestManifest.OnFinish += response =>
            {
                ResponseUri responseManifest = (ResponseUri) response;
                DownloadLocationFiles(locationPrefab, responseManifest.ByteData);
            };

            requestManifest.OnError += s => { Helper.ShowErrorLoadScene(); };
        }

        public override void LoadProjectStructure(int projectId, Action<ProjectStructure> onFinish)
        {
            string structureRequestString = string.Format(ApiRoutes.ProjectStructureRequest, projectId);

            var structureRequest = new RequestApi(structureRequestString);

            Logger.Info("API request: " + structureRequestString);

            structureRequest.OnFinish += response =>
            {
                ResponseApi structureResponseApi = (ResponseApi) response;

                if (!Helper.IsResponseGood(structureResponseApi))
                {
                    Logger.Fatal("Can not get " + structureRequestString);
                    onFinish.Invoke(null);

                    return;
                }

                string projectStructureJson = structureResponseApi.Data.ToString();

                ProjectStructure currentProjectStructure = projectStructureJson.JsonDeserialize<ProjectStructure>();

                foreach (var scene in currentProjectStructure.Scenes)
                {
                    var logicRequest = new RequestUri(scene.LogicResource);

                    logicRequest.OnFinish += response1 =>
                    {
                        ResponseUri logicResponse = (ResponseUri) response1;
                        scene.AssemblyBytes = logicResponse.ByteData;
                    };
                }

                onFinish.Invoke(currentProjectStructure);
            };
        }


        private void LoadCustomAssetApi(AssetInfo assetInfo, PrefabObject o)
        {
            string assetName = assetInfo.AssetName;
            string assetBundleUri = ProjectData.IsMobileVr() ? o.AndroidBundleResource : o.BundleResource;

            RequestAsset requestAsset =
                new RequestAsset(assetName, assetBundleUri, new object[] {o, assetInfo});

            requestAsset.OnFinish += responseAsset =>
            {
                FeedBackText = LanguageManager.Instance.GetTextValue("LOADING")
                               + " "
                               + o.Config.i18n.GetCurrentLocale()
                               + "...";
                CountLoadedObjects++;

                if (LoaderAdapter.OnDownLoadUpdate != null)
                {
                    LoaderAdapter.OnDownLoadUpdate(CountLoadedObjects
                                                   / (float) LoadCounter.loadObjectsCounter.PrefabsCount);
                }

                RequestTexture requestDownLoad = new RequestTexture(Settings.Instance().ApiHost + o.IconResource,
                    128,
                    128,
                    ProjectData.IsMobileVr() ? TextureFormat.ASTC_RGB_6x6 : TextureFormat.DXT1);

                requestDownLoad.OnFinish += responseUri =>
                {
                    ResponseTexture responseTexture = (ResponseTexture) responseUri;
                    Texture2D texture2D = responseTexture.Texture;

                    Sprite sprite = Sprite.Create(texture2D,
                        new Rect(0,
                            0,
                            128,
                            128),
                        Vector2.zero);
                    CreatePrefabEntity(responseAsset, o, sprite);
                };

                requestDownLoad.OnError += s => { CreatePrefabEntity(responseAsset, o); };
            };
        }

        private void DownloadLocationFiles(LocationPrefab locationPrefab, byte[] manifestOnServer)
        {
            Logger.Info($"Starting download location <{locationPrefab.Name}> assets");

            string pathToEnvironmentsStorage =
                FileSystemUtils.GetFilesPath(ProjectData.ProjectStructure.IsMobile, "cache/locations/");
            string environmentDirectory = Path.Combine(pathToEnvironmentsStorage, locationPrefab.Name);

            FileSystemUtils.CreateDirectory(environmentDirectory);

            string manifestStorageFile = Path.Combine(environmentDirectory,
                ProjectData.IsMobileVr() ? "android_bundle.manifest" : "bundle.manifest");

            if (File.Exists(manifestStorageFile))
            {
                var manifestOnStorage = File.ReadAllBytes(Path.Combine(environmentDirectory,
                    ProjectData.IsMobileVr() ? "android_bundle.manifest" : "bundle.manifest"));

                //TODO: Turn back caching
                if (Equal(manifestOnServer, manifestOnStorage))
                {
                    LoadLocationFromStorage(locationPrefab, environmentDirectory);
                }
                else
                {
                    LoadLocationFromWeb(locationPrefab, environmentDirectory);
                }
            }

            else
            {
                LoadLocationFromWeb(locationPrefab, environmentDirectory);
            }
        }

        private static bool Equal(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            return !a.Where((t, i) => t != b[i]).Any();
        }

        private void LoadLocationFromWeb(LocationPrefab locationPrefab, string environmentDirectory)
        {
            DateTime startLoadingTime = DateTime.Now;
            Logger.Info($"Loading location \"{locationPrefab.Name}\" from web");
            RequestUri sceneDataRequest = new RequestUri(locationPrefab.ConfigResource);

            sceneDataRequest.OnFinish += response =>
            {
                string json = ((ResponseUri) response).TextData;
                SceneData sceneData = json.JsonDeserialize<SceneData>();

                List<string> environmentFiles = new List<string>(3)
                {
                    ProjectData.IsMobileVr() ? locationPrefab.AndroidBundleResource : locationPrefab.BundleResource,
                    locationPrefab.IconResource,
                    ProjectData.IsMobileVr()
                        ? locationPrefab.AndroidManifestResource
                        : locationPrefab.ManifestResource,
                };

                RequestDownLoad requestDownloads = new RequestDownLoad(environmentFiles,
                    environmentDirectory,
                    LoaderAdapter.OnLoadingUpdate,
                    this,
                    LanguageManager.Instance.GetTextValue("LOADING_FILE"));

                requestDownloads.OnFinish += response1 =>
                {
                    TimeSpan span = DateTime.Now - startLoadingTime;

                    Logger.Info($"Location \"{locationPrefab.Name}\" is loaded. Time = {span.Seconds} sec.");

                    ResponseDownLoad responseDownLoad = (ResponseDownLoad) response1;

                    string sceneName = sceneData.Name;

                    string assetName = ProjectData.IsMobileVr()
                        ? "android_" + sceneData.AssetBundleLabel
                        : sceneData.AssetBundleLabel;

                    foreach (var key in responseDownLoad.DownLoadedData.Keys)
                    {
                        Logger.Info("Downloaded for scene" + sceneName + ": " + key);
                    }

                    byte[] bytes = responseDownLoad.DownLoadedData[assetName].ToArray();

                    Logger.Info($"Loading location \"{sceneName}\"...");

                    RequestLoadSceneFromMemory request = new RequestLoadSceneFromMemory(sceneName, bytes);

                    request.OnFinish += response2 =>
                    {
                        ResponseAsset responseAsset = (ResponseAsset) response2;
                        string scenePath = Path.GetFileNameWithoutExtension(responseAsset.Path);
                        ProjectDataListener.Instance.LoadScene(scenePath);
                    };

                    request.OnError += s => { ShowErrorLoadScene(); };
                };

                requestDownloads.OnError += s => { ShowErrorLoadScene(); };
            };

            sceneDataRequest.OnError += s => { ShowErrorLoadScene(); };
        }

        private void LoadLocationFromStorage(LocationPrefab locationPrefab, string environmentDirectory)
        {
            string sceneName = locationPrefab.Name;
            Logger.Info($"Loading location \"{sceneName}\" from storage: " + environmentDirectory);
            byte[] bytes = null;

            RequestTread requestTread = new RequestTread(delegate
            {
                bytes = File.ReadAllBytes(environmentDirectory
                                          + "/"
                                          + (ProjectData.IsMobileVr() ? "android_bundle" : "bundle"));
            });

            requestTread.OnFinish += responseTread =>
            {
                RequestLoadSceneFromMemory request = new RequestLoadSceneFromMemory(sceneName, bytes);

                request.OnFinish += response1 =>
                {
                    ResponseAsset response = (ResponseAsset) response1;
                    string scenePath = Path.GetFileNameWithoutExtension(response.Path);
                    ProjectDataListener.Instance.LoadScene(scenePath);
                };

                request.OnError += s =>
                {
                    string message = ErrorHelper.GetErrorDescByCode(Varwin.Errors.ErrorCode.EnvironmentNotFoundError);
                    LogManager.GetCurrentClassLogger().Fatal("Location is not loaded from storage! " + s);
                    LauncherErrorManager.Instance.Show(message, null);
                };
            };
        }
    }
}
