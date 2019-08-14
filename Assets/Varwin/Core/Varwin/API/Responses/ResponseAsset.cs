using Varwin.WWW;

namespace Varwin.Data.ServerData
{
    public class ResponseAsset : IResponse
    {
        public UnityEngine.Object Asset;
        public string Path;
        public object[] UserData;
    }
}
