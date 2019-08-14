using Newtonsoft.Json;
using Varwin.WWW;

// ReSharper disable once CheckNamespace
namespace Varwin.Data
{
    public class ResponseApi : IResponse
    {
        public string Status;
        public string Message;
        public string Code;
        public object Data;
        public long ResponseCode;
    }

    #region CUSTOM SERIALIZATOR
    public static class JSendEx
    {
        public static string ToJson(this ResponseApi self)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var json = JsonConvert.SerializeObject(self, settings);
            return json;
        }

        public static ResponseApi Deserialize(string json)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                return JsonConvert.DeserializeObject<ResponseApi> (json, settings);
            }
            catch
            {
                return new ResponseApi() {Message = "not string"};
            }

        }
    }

    #endregion

}
