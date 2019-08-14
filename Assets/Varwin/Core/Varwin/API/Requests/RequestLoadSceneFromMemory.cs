using System.Collections;

namespace Varwin.WWW
{
    public class RequestLoadSceneFromMemory : Request
    {
        public string AssetName { get; }
        public byte[] Bytes;

        public RequestLoadSceneFromMemory(string assetName, byte[] bytes, object[] userObjects = null)
        {
            AssetName = assetName;
            UserData = userObjects;
            Bytes = bytes;
            Uri = "";
            RequestManager.AddRequest(this);
        }

        protected override IEnumerator SendRequest()
        {
            AssetBundleManager.Unload(AssetName, 1, true);
            yield return AssetBundleManager.Instance.InstantiateSceneFromMemoryAsync(this);
        }

    }

}