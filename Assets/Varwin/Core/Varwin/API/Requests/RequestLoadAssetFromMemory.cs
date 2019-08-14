using System.Collections;

namespace Varwin.WWW
{
    public class RequestLoadAssetFromMemory : Request
    {
        public string AssetName { get; }
        public byte[] Bytes;

        public RequestLoadAssetFromMemory(string assetName, byte[] bytes, object[] userObjects = null)
        {
            AssetName = assetName;
            UserData = userObjects;
            Bytes = bytes;
            Uri = "";
            RequestManager.AddRequest(this);
        }

        protected override IEnumerator SendRequest()
        {
            yield return AssetBundleManager.Instance.InstantiateAssetFromMemoryAsync(this);
        }

    }

}


