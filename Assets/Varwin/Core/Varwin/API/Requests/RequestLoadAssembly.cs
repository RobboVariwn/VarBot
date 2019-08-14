using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Varwin.Errors;
using Varwin.Data;
using Varwin.UI.VRErrorManager;

namespace Varwin.WWW
{
    public class RequestLoadAssembly : Request
    {
        public RequestLoadAssembly(byte[] bytes)
        {
            UserData = new object[]{bytes};
            RequestManager.AddRequest(this);
            Uri = "Compiler";
        }

        protected override IEnumerator SendRequest()
        {
            yield return Get();
        }

        #region GET

        private IEnumerator Get()
        {
            Assembly assembly = null;
            Type type = null;
            
            byte[] bytes = (byte[]) UserData[0];
            bool done = false;
            
            DateTime start = DateTime.Now;
            int milliseconds = 0;
            
            string errorMessage = null;

            if (bytes == null)
            {
                VRErrorManager.Instance.Show(ErrorHelper.GetErrorDescByCode(Varwin.Errors.ErrorCode.CompileCodeError));
                
                var response = new ResponseLoadAssembly()
                {
                    LoadedAssembly = null,
                    CompiledType = null, 
                    Milliseconds = milliseconds
                };
                ((IRequest) this).OnResponseDone(response);
            }
            else
            {
                new Thread(() =>
                {
                    try
                    {
                        assembly = Assembly.Load(bytes);
                        type = assembly.ExportedTypes.FirstOrDefault();
                        milliseconds = (DateTime.Now - start).Milliseconds;
                    }
                    catch (Exception e)
                    {
                        errorMessage = e.Message;
                    }
                    finally
                    {
                        done = true;
                    }
                }).Start();
                
                while (!done)
                {
                    yield return null;
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    VRErrorManager.Instance.Show(ErrorHelper.GetErrorDescByCode(Varwin.Errors.ErrorCode.RuntimeCodeError));
                }

                if (type == null && !string.IsNullOrEmpty(errorMessage))
                {
                    ((IRequest) this).OnResponseError($"Runtime code error! {errorMessage}");
                    AMQPClient.SendRuntimeErrorMessage(ProjectData.SceneId, 0, 0, errorMessage);
                }
                else
                {
                    var response = new ResponseLoadAssembly()
                    {
                        LoadedAssembly = assembly,
                        CompiledType = type, 
                        Milliseconds = milliseconds
                    };
                    ((IRequest) this).OnResponseDone(response);
                }
            }
            yield return true;

        }

        #endregion

    }
}