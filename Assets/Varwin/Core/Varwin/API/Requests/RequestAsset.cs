using System.Collections;

namespace Varwin.WWW
{
    public class RequestAsset : Request
    {
        public string AssetName { get; }

        public RequestAsset(string assetName, string assetBundleUri, object[] userObjects = null)
        {
            AssetName = assetName;
            UserData = userObjects;
            Uri = Settings.Instance().ApiHost + assetBundleUri;
            RequestManager.AddRequest(this);
        }

        protected override IEnumerator SendRequest()
        {
            yield return AssetBundleManager.Instance.InstantiateGameObjectAsync(this);
        }
        
    }
  
}