using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using Varwin.Data;
using ThreadPriority = System.Threading.ThreadPriority;

namespace Varwin.WWW
{
    public class RequestTread : Request
    {
        public delegate void ActionVoid();

        private event ActionVoid OnAction;


        public RequestTread(ActionVoid action)
        {
            OnAction = action;
            RequestManager.AddRequest(this);
            Uri = "Action";
        }

        protected override IEnumerator SendRequest()
        {
            yield return Get();
        }

        #region GET

        private IEnumerator Get()
        {
             
            bool done = false;
            bool error = false;
            string message = "";

            Thread thread = new Thread(() =>
            {
                try
                {
                    OnOnAction();
                    done = true;
                }
                catch (Exception ex)
                {
                    done = true;
                    error = true;
                    message = ex.Message + " " + ex.StackTrace;
                }


            });
            thread.IsBackground = true;
            thread.Start();

            while (!done)
            {
                yield return null;
            }

            if (error)
            {
                ((IRequest)this).OnResponseError($"Tread action invoke error! {message} ");
            }
            else
            {
                ResponseTread response = new ResponseTread {Done = true, Message = "ok!"};
                ((IRequest)this).OnResponseDone(response);
            }

            yield return true;

        }

        #endregion


        protected virtual void OnOnAction()
        {
            OnAction?.Invoke();
        }
    }
}