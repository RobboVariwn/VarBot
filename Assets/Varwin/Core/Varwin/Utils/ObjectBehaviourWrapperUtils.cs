using System;
using System.Collections;
using System.Collections.Generic;
using NLog;
using Varwin.Commands;
using Varwin.Data;
using Varwin.Errors;
using Varwin.WWW;
using Varwin.UI.VRErrorManager;

namespace Varwin
{
    public static class ObjectBehaviourWrapperUtils
    {
        public static Action ObjectCannotDelete;

        public static IEnumerator DeleteAllChildren(ObjectController objectController, ObjectBehaviourWrapper[] wrappers = null)
        {
            if (wrappers == null)
            {
                wrappers = objectController.RootGameObject.GetComponentsInChildren<ObjectBehaviourWrapper>();
            }
            
            List<bool> canDelete = new List<bool>();
            foreach (ObjectBehaviourWrapper wrapper in wrappers)
            {
                if (wrapper.OwdObjectController.IdServer != 0)
                {
                    var request = new RequestApi(string.Format(ApiRoutes.CanDeleteObjectRequest, wrapper.OwdObjectController.IdServer));
                    request.OnFinish += response =>
                    {
                        ResponseApi jsend = (ResponseApi)response;
                        if (jsend.Data == null || (bool)jsend.Data)
                        {
                            canDelete.Add(true);
                        }
                        else
                        {
                            canDelete.Add(false);
                        }
                    };
                    request.OnError += s =>
                    {
                        VRErrorManager.Instance.Show("!!!");
                    };
                }
                else
                {
                    canDelete.Add(true);
                }
            }

            while (canDelete.Count != wrappers.Length)
            {
                yield return null;
            }

            bool ok = true;
            foreach (bool can in canDelete)
            {
                if (!can)
                {
                    ok = false;
                    break;
                }
            }

            if (!ok)
            {
                LogManager.GetCurrentClassLogger()
                    .Info($"Can not delete object IdServer = {objectController.IdServer}");
                VRErrorManager.Instance.Show(
                    ErrorHelper.GetErrorDescByCode(Varwin.Errors.ErrorCode.CannotDeleteObjectLogic));
                ObjectCannotDelete?.Invoke();
                    
            }
            else
            {
                DeleteCommand(objectController);
            }

            yield return true;
        }

        public static void DeleteCommand(ObjectController objectController)
        {
            ICommand command = new DeleteCommand(objectController);
            command.Execute();
        }
    }
}