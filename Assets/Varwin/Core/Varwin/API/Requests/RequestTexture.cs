using System;
using System.Collections;
using UnityEngine;
using Varwin.Data;

namespace Varwin.WWW
{
    public class RequestTexture : Request
    {
        private int _width;
        private int _height;
        private TextureFormat _format;
        
        public RequestTexture(string uri, int width, int height, TextureFormat format)
        {
            _width = width;
            _height = height;
            _format = format;
            Uri = uri;
            RequestManager.AddRequest(this);
        }
        
        protected override IEnumerator SendRequest()
        {
            Texture2D tex;
            tex = new Texture2D(_width, _height, _format, false);
            using (UnityEngine.WWW www = new UnityEngine.WWW(Uri))
            {
                yield return www;

                try
                {
                    www.LoadImageIntoTexture(tex);
                    ResponseTexture responseTexture = new ResponseTexture();
                    responseTexture.Texture = tex;
                    ((IRequest) this).OnResponseDone(responseTexture);
                }
                catch (Exception e)
                {
                    ((IRequest) this).OnResponseError("Texture can not be loaded! " + e.Message);
                }
                
                 
                
            }
        }
    }
}