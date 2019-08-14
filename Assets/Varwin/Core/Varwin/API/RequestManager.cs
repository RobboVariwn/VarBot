using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.UI;
using Varwin.UI.VRErrorManager;

namespace Varwin.WWW
{
    public class RequestManager : MonoBehaviour
    {
        private static List<Request> _requestQueue = new List<Request>();
        private static readonly List<Request> RequestQueueToAdd = new List<Request>();
        public bool WorkingWithRequests;
        private static int _requestCounter;
        private bool _stop;
        public static RequestManager Instance;

        private void Awake()
        {           
            if (Instance == null) 
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            } 
            else if (Instance != this) 
            {
                Destroy (gameObject);
            }
        }

        void Update()
        {
            if (!WorkingWithRequests && (_requestQueue.Count > 0 || RequestQueueToAdd.Count > 0))
            {
                StartCoroutine(SendRequests());
            }

            foreach (Request request in _requestQueue)
            {
                request.OnUpdate?.Invoke();
            }
        }

        public static void AddRequest(Request request)
        {
            request.Number = _requestCounter;
            RequestQueueToAdd.Add(request);
            _requestCounter++;
        }

        IEnumerator SendRequests()
        {
            WorkingWithRequests = true;
            _requestQueue.AddRange(RequestQueueToAdd);
            _requestQueue = _requestQueue.OrderBy(request => request.Number).ToList();
            RequestQueueToAdd.Clear();

            foreach (Request request in _requestQueue)
            {
                if (_stop)
                {
                    break;
                }
                
                if (request.Done)
                {
                    continue;
                }

                IRequest r = request;
                yield return StartCoroutine(r.SendRequest());
            }

            List<Request> toRemoveList = new List<Request>();
            foreach (Request request in _requestQueue)
            {
                if (request.Done)
                {
                    toRemoveList.Add(request);
                }

                if (!request.Error)
                {
                    continue;
                }

                WorkingWithRequests = false;
                toRemoveList.Add(request);

            }

            foreach (Request request in toRemoveList)
            {
                _requestQueue.Remove(request);
            }

            WorkingWithRequests = false;
            yield return true;
        }


        public void StopRequestsWithError(string error)
        {
            _stop = true;
            _requestQueue = new List<Request>();
            RequestQueueToAdd.Clear();

            if (VRErrorManager.Instance != null)
            {
                VRErrorManager.Instance.ShowFatal(error);
            }

            if (LauncherErrorManager.Instance != null)
            {
                LauncherErrorManager.Instance.ShowFatal(error, null);
            }
        }
    }

}






