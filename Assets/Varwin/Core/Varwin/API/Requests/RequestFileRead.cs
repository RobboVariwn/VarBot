using System;
using System.Collections;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.Tar;
using Varwin.Data;

namespace Varwin.WWW
{
    public class RequestFileRead : Request
    {
        public RequestFileRead(string fileName, object[] userData = null)
        {
            Uri = fileName;
            UserData = userData;
            RequestManager.AddRequest(this);
        }

        protected override IEnumerator SendRequest()
        {
            yield return Get();
        }

        #region GET METHOD

        private IEnumerator Get()
        {
            byte[] file = { };
            bool done = false;
            bool found = false;
            string error = String.Empty;
            string uriFile = Settings.Instance().StoragePath + Uri.Replace('\\', '/');

            if (File.Exists(uriFile))
            {
                found = true;
            }
            
            new Thread(() =>
            {
                try
                {
                    file = File.ReadAllBytes(uriFile);
                    done = true;
                }
                catch (Exception e)
                {
                    error = e.Message;
                    done = true;
                }
            }).Start();

            while (!done)
            {
                yield return null;
            }


            if (!found)
            {
                error = "file not found";
            }

            if (file.Length == 0 || error != String.Empty)
            {
                ((IRequest) this).OnResponseError($"Load file {Uri} from storage error! {error}");
            }
            else
            {
                ResponseFileRead response = new ResponseFileRead {ByteData = file, UserData = UserData};
                ((IRequest) this).OnResponseDone(response);
            }

            yield return true;

            #endregion
        }
    }
}
