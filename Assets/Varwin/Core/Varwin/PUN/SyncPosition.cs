using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Varwin.PUN
{
    public class SyncPosition : Photon.PunBehaviour
    {
        private Transform _syncTransform;
        //private Vector3 _deltaPosition = Vector3.zero;

        public void Init(Transform syncTransform)
        {
            _syncTransform = syncTransform;

            if (photonView.isMine)
                RenderGameObject(gameObject, false);
        }

        void Update()
        {
            if (_syncTransform != null)
            {
                transform.position = _syncTransform.position;
                transform.rotation = _syncTransform.rotation;
            }
        }

        void RenderGameObject(GameObject go, bool render)
        {
            List<Transform> childs = go.GetComponentsInChildren<Transform>().ToList();
            foreach (var child in childs)
            {
                var r = child.GetComponent<Renderer>();
                if (r != null) r.enabled = render;
            }
        }
    }
}
