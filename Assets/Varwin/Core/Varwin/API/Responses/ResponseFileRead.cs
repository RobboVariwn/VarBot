using Varwin.WWW;

namespace Varwin.Data
{
    public class ResponseFileRead : IResponse
    {
        public object[] UserData;
        public byte[] ByteData;
        public string TextData => System.Text.Encoding.Default.GetString(ByteData);
    }
}
