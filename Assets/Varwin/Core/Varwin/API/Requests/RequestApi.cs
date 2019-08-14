using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using Varwin.Data;

#pragma warning disable 618

namespace Varwin.WWW
{
    public class RequestApi : Request
    {
        public string PostData { get; }
        public RequestType RequestType { get; }

        public RequestApi(string uri, RequestType requestType = RequestType.Get, string postData = "{}", object[] userData = null)
        {
            Uri = uri;
            RequestType = requestType;
            PostData = postData;
            UserData = userData;
            RequestManager.AddRequest(this);
        }

        protected override IEnumerator SendRequest()
        {
            if (RequestType == RequestType.Get)
            {
                yield return Get();
            }

            if (RequestType == RequestType.Post)
            {
                yield return Post();
            }
        }

        #region POST GET METHODS

        private IEnumerator Post()
        {
            var data = new UnityWebRequest(Settings.Instance().ApiHost + Uri, "POST");
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(PostData);

            data.uploadHandler = new UploadHandlerRaw(bodyRaw);
            data.downloadHandler = new DownloadHandlerBuffer();
            data.SetRequestHeader("Content-Type", "application/json");
            data.SendWebRequest();

            float timer = 0;
            bool failed = false;

            while (!data.isDone)
            {
                if (timer > TimeOut)
                {
                    failed = true;

                    break;
                }

                timer += Time.deltaTime;

                yield return null;
            }

            if (data.isNetworkError)
            {
                failed = true;
            }

            if (failed)
            {
                ((IRequest) this).OnResponseError($"{this} Timeout error", data.responseCode);
                data.Dispose();
            }
            else
            {
                var jSend = JSendEx.Deserialize(data.downloadHandler.text)
                            ?? new ResponseApi()
                            {
                                Data = data.downloadHandler.text
                            };

                if (jSend.Message == "not string")
                {
                    jSend.Data = data.downloadHandler.data;
                }

                ((IRequest) this).OnResponseDone(jSend, data.responseCode);
            }
        }


        private IEnumerator Get()
        {
            var data = UnityWebRequest.Get(Settings.Instance().ApiHost + Uri);
            data.SendWebRequest();
            float timer = 0;
            bool failed = false;

            while (!data.isDone)
            {
                if (timer > TimeOut)
                {
                    failed = true;

                    break;
                }

                timer += Time.deltaTime;

                yield return null;
            }

            if (data.isNetworkError)
            {
                failed = true;
            }

            if (failed)
            {
                ((IRequest) this).OnResponseError($"{this} Timeout error", data.responseCode);
                data.Dispose();
            }
            else
            {
                ResponseApi responseApi = JSendEx.Deserialize(data.downloadHandler.text);

                if (responseApi.Data == null)
                {
                    responseApi.Data = data.downloadHandler.text;
                }

                ((IRequest) this).OnResponseDone(responseApi, data.responseCode);
            }
        }

        #endregion



    }

    public enum RequestType
    {
        Post,
        Get
    }
}