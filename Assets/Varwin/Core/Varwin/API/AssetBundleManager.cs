using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Varwin.Data.ServerData;

namespace Varwin.WWW
{
    public class AssetBundleManager : MonoBehaviour
    {
        public static AssetBundleManager Instance;
        private static readonly Dictionary<string, AssetBundleRef> DictAssetBundleRefs;

        static AssetBundleManager()
        {
            DictAssetBundleRefs = new Dictionary<string, AssetBundleRef>();
        }

        private class AssetBundleRef
        {
            public AssetBundle AssetBundle;

            // ReSharper disable once NotAccessedField.Local
            private int _version;

            // ReSharper disable once NotAccessedField.Local
            private string _url;

            public AssetBundleRef(string strUrlIn, int intVersionIn)
            {
                _url = strUrlIn;
                _version = intVersionIn;
            }
        };

        // Get an AssetBundle
        public static AssetBundle GetAssetBundle(string url, int version)
        {
            string keyName = url + version.ToString();
            AssetBundleRef abRef;
            if (DictAssetBundleRefs.TryGetValue(keyName, out abRef))
            {
                return abRef.AssetBundle;
            }
            else
            {
                return null;
            }
        }

        // Download an AssetBundle
        public static IEnumerator DownloadAssetBundle(string url, int version)
        {
            string keyName = url + version;
            if (DictAssetBundleRefs.ContainsKey(keyName))
            {
                yield return null;
            }
            else
            {
                while (!Caching.ready)
                {
                    yield return null;
                }

                var www = UnityWebRequestAssetBundle.GetAssetBundle(url);
                yield return www.SendWebRequest();
                var myLoadedAssetBundle = DownloadHandlerAssetBundle.GetContent(www);
                AssetBundleRef abRef = new AssetBundleRef(url, version) {AssetBundle = myLoadedAssetBundle};
                DictAssetBundleRefs.Add(keyName, abRef);

            }
        }

        public static IEnumerator LoadAssetBundleFromMemory(byte[] bytes, string bundleName, int version)
        {
            string keyName = bundleName + version;
            if (DictAssetBundleRefs.ContainsKey(keyName))
            {
                yield return null;
            }
            else
            {
                while (!Caching.ready)
                {
                    yield return null;
                }

                var createRequest = AssetBundle.LoadFromMemoryAsync(bytes);

                while (!createRequest.isDone)
                {
                    yield return null;
                }

                var myLoadedAssetBundle = createRequest.assetBundle;
                AssetBundleRef abRef = new AssetBundleRef(bundleName, version) {AssetBundle = myLoadedAssetBundle};
                DictAssetBundleRefs.Add(keyName, abRef);

            }
        }

        // Unload an AssetBundle
        public static void Unload(string url, int version, bool allObjects)
        {
            string keyName = url + version.ToString();
            AssetBundleRef abRef;
            if (DictAssetBundleRefs.TryGetValue(keyName, out abRef))
            {
                abRef.AssetBundle.Unload(allObjects);
                abRef.AssetBundle = null;
                DictAssetBundleRefs.Remove(keyName);
            }
        }

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

        public IEnumerator InstantiateGameObjectAsync(RequestAsset requestAsset)
        {
            string path = requestAsset.Uri;
            int version = 1;
            yield return StartCoroutine(DownloadAssetBundle(path, version));
            AssetBundle bundle = GetAssetBundle(path, version);
            if (bundle != null)
            {
                var loadAssetAsync = bundle.LoadAssetAsync(requestAsset.AssetName);
                yield return loadAssetAsync;
                var response = new ResponseAsset { Asset = loadAssetAsync.asset, UserData = requestAsset.UserData };
                ((IRequest)requestAsset).OnResponseDone(response);
            }
            else
            {
                string message = "Can not load asset " + requestAsset.AssetName;
                ((IRequest)requestAsset).OnResponseError(message);
            }

            yield return true;
        }

        public IEnumerator InstantiateAssetFromMemoryAsync(RequestLoadAssetFromMemory requestAsset)
        {
            string assetName = requestAsset.AssetName;
            byte[] bytes = requestAsset.Bytes;
            int version = 1;
            yield return StartCoroutine(LoadAssetBundleFromMemory(bytes, assetName, version));
            AssetBundle bundle = GetAssetBundle(assetName, version);
            if (bundle != null)
            {
                var loadAssetAsync = bundle.LoadAssetAsync(requestAsset.AssetName);
                yield return loadAssetAsync;
                var response = new ResponseAsset { Asset = loadAssetAsync.asset, UserData = requestAsset.UserData };
                ((IRequest)requestAsset).OnResponseDone(response);
                bundle.Unload(false);
            }
            else
            {
                string message = "Can not load asset " + requestAsset.AssetName;
                ((IRequest)requestAsset).OnResponseError(message);
            }

        }

        public IEnumerator InstantiateSceneFromMemoryAsync(RequestLoadSceneFromMemory requestScene)
        {
            string assetName = requestScene.AssetName;
            byte[] bytes = requestScene.Bytes;
            int version = 1;
            yield return StartCoroutine(LoadAssetBundleFromMemory(bytes, assetName, version));
            AssetBundle bundle = GetAssetBundle(assetName, version);
            if (bundle != null)
            {
                string path = bundle.GetAllScenePaths()[0];
                var response = new ResponseAsset {Asset = bundle, Path = path, UserData = requestScene.UserData};
                ((IRequest) requestScene).OnResponseDone(response);
            }
            else
            {
                string message = "Can not load asset " + requestScene.AssetName;
                ((IRequest) requestScene).OnResponseError(message);
            }

        }

    }

}






