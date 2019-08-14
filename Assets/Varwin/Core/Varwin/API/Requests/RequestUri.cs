using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Varwin.Data;

//#pragma warning disable 618

namespace Varwin.WWW
{
    public class RequestUri : Request
    {
        public RequestUri(string uri, object[] userData = null)
        {
            Uri = uri;
            UserData = userData;
            RequestManager.AddRequest(this);
        }

        //public RequestUri(List<string> uris, object[] userData = null)
        //{
        //    Uris = uris;
        //    UserData = userData;
        //    RequestManager.AddRequest(this);
        //}

        protected override IEnumerator SendRequest()
        {
           // if (Uri != String.Empty)
                yield return Get();
           // if (Uris != null)
        }

        #region GET METHOD

        IEnumerator Get()
        {
            string uri = Settings.Instance().ApiHost;
            if (!uri.EndsWith("/"))
            {
                uri += "/";
            }
            uri += Uri;
            
            var data = UnityWebRequest.Get(uri);
            data.SendWebRequest();

            float timer = 0;
            bool failed = false;

            while (!data.isDone)
            {
                if (timer > TimeOut) { failed = true; break; }
                timer += Time.deltaTime;
                yield return null;
            }

            if (data.isNetworkError) { failed = true; }

            if (failed)
            {
                ((IRequest)this).OnResponseError($"{this} Timeout error", data.responseCode);
                data.Dispose();
            }
            else
            {
                ResponseUri response = new ResponseUri(){ResponseCode = data.responseCode, TextData = data.downloadHandler.text, ByteData = data.downloadHandler.data};
                ((IRequest)this).OnResponseDone(response, data.responseCode);
            }
        }

        #endregion



    }

}