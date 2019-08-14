using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Public;
using Varwin.UI;
using Varwin.VRInput;

namespace Varwin
{
    public class RayGrab : MonoBehaviour
    {
       // private PointerController.InputPointer _vrtkPointer;
        private ControllerInteraction.ControllerSelf _controllerReference;

        private LineRenderer _lineRenderer;
        private Transform _raycastOrigin;

        private SphereCollider _rayGrabCollider;
        private GameObject _rayGrabColliderObject;

        private GameObject _focusedVarwinObject;
        private Collider _previousHitCollider;

        private RaycastHit _raycastHit;

        private const float MaxRayLength = 0.15f;
        private const int MaxRaycastHits = 10;
        private const float RayWidth = 0.004f;
        private const float GrabColliderRadius = 0.015f;

        void Start()
        {
            _controllerReference = InputAdapter.Instance.ControllerInteraction.Controller.GetFrom(gameObject);
            //GetComponent<VRTK_InteractGrab>();

            _lineRenderer = gameObject.GetComponent<LineRenderer>();

            _lineRenderer.startWidth = RayWidth;
            _lineRenderer.endWidth = RayWidth;

            _lineRenderer.startColor = Color.cyan;
            _lineRenderer.endColor = Color.cyan;

            _lineRenderer.enabled = false;

            _rayGrabColliderObject = new GameObject("RayGrabCollider");

            _rayGrabCollider = _rayGrabColliderObject.AddComponent<SphereCollider>();
            _rayGrabCollider.radius = GrabColliderRadius;
            _rayGrabCollider.isTrigger = true;

            _rayGrabColliderObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            _raycastHit = new RaycastHit();

           // _vrtkPointer = InputAdapter.Instance.PointerController.Pointer.GetFrom(gameObject);
            //GetComponent<VRTK_StraightPointerRenderer>();

            _rayGrabColliderObject.transform.SetParent(_controllerReference.Controller.transform);
            _rayGrabColliderObject.transform.localPosition = Vector3.zero;
        }

        void Update()
        {
            if (_raycastOrigin == null)
            {
                _raycastOrigin = _controllerReference.Controller.transform;
            }

            if (_raycastOrigin == null || _controllerReference.GetGrabbedObject() != null)
            {
                ResetGrabRayAndCollider();

                return;
            }

            LayerMask ignoreRaycastLayer = 1 << LayerMask.NameToLayer("Ignore Raycast");
            LayerMask zonesLayer = 1 << LayerMask.NameToLayer("Zones");

            LayerMask raycastIgnoreLayers = ignoreRaycastLayer | zonesLayer;

            bool raycastSuccessful = Physics.Raycast(_raycastOrigin.position,
                _raycastOrigin.forward,
                out _raycastHit,
                MaxRayLength,
                ~raycastIgnoreLayers);

            if (!raycastSuccessful)
            {
                _previousHitCollider = null;

                ResetGrabRayAndCollider();

                return;
            }

            Collider hitCollider = _raycastHit.collider;

            if (_previousHitCollider != hitCollider)
            {
                _focusedVarwinObject = hitCollider.GetComponentInParent<IWrapperAware>()?.Wrapper()?.GetGameObject();
                _previousHitCollider = hitCollider;
            }

            if (_focusedVarwinObject == null)
            {
                ResetGrabRayAndCollider();

                return;
            }

            _rayGrabColliderObject.transform.position = _raycastHit.point;
            _rayGrabCollider.enabled = true;

            _lineRenderer.enabled = true;

            _lineRenderer.SetPosition(0, _raycastOrigin.position);
            _lineRenderer.SetPosition(1, _rayGrabColliderObject.transform.position);
        }

        private void ResetGrabRayAndCollider()
        {
            _rayGrabCollider.enabled = false;
            _rayGrabColliderObject.transform.localPosition = Vector3.zero;

            _lineRenderer.enabled = false;
        }
    }
}
