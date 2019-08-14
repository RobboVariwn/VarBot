using UnityEngine;
using UnityEngine.XR;

namespace Varwin.VRInput
{
    [RequireComponent(typeof(Arc))]
    public class TeleportPointer : MonoBehaviour, PointerController.IBasePointer
    {
        public float Velocity = 10f;
        public float DotUp = 0.9f;

        public bool CanClick() => _events.IsTouchpadReleased()
                                  && _arc.PlayerTeleportTransformCandidate != Vector3.zero
                                  && _arc.IsArcValid();

        public bool CanToggle() => _events.IsTouchpadPressed();

        public void Toggle(bool value)
        {
            _active = value;
            UpdateArc(_active);
        }

        public void Toggle()
        {
            _active = !_active;
            UpdateArc(_active);
        }

        public bool IsActive() => _active;


        private void UpdateArc(bool state)
        {
            if (state)
            {
                _arc.Show();

                return;
            }

            _arc.Hide();
            DestinationReticle.SetActive(false);
        }

        public void Click()
        {
            TeleportPlayer();
        }

        private ControllerInput.ControllerEvents _events;

        private Arc _arc;
        private bool _active;

        private GameObject DestinationReticle
        {
            get
            {
                if (_destinationReticle == null)
                {
                    _destinationReticle = Instantiate(Resources.Load<GameObject>("Teleport/DestinationReticle"));
                }

                return _destinationReticle;
            }
        }
        private GameObject _destinationReticle;
        private bool _click;

        private Transform _pointerOrigin;

        public void Init()
        {
            Transform origins = transform.Find("PointerOrigins");

            if (origins != null)
            {
                if (XRDevice.model.Contains("Oculus"))
                {
                    _pointerOrigin = origins.Find("Oculus");
                }
                else
                {
                    _pointerOrigin = origins.Find("Generic");
                }

                if (_pointerOrigin == null)
                {
                    _pointerOrigin = transform;
                }
            }
            else
            {
                _pointerOrigin = transform;
            }

            _arc = GetComponent<Arc>();
            _arc.ControllerHand = InputAdapter.Instance.PlayerController.Nodes.GetControllerHand(gameObject);
            //Ignore raycast and Zones
            _arc.TraceLayerMask = ~((1 << 2) | (1 << 13));  
            _events = InputAdapter.Instance.ControllerInput.ControllerEventFactory.GetFrom(gameObject);
        }

        public void UpdateState()
        {
            
            Vector3 originPosition = _pointerOrigin != null ? _pointerOrigin.position : transform.position;
            
            Vector3 forward = _pointerOrigin != null ? _pointerOrigin.forward : transform.forward;
            Vector3 pointerDir = forward;
            Vector3 arcVelocity = pointerDir * Velocity;
            float dotUp = Vector3.Dot(pointerDir, Vector3.up);
            float dotForward = Vector3.Dot(pointerDir, forward);
            bool pointerAtBadAngle = dotForward > 0 && dotUp > DotUp || dotForward < 0.0f && dotUp > 0.5f;

            _arc.SetArcData(originPosition,
                arcVelocity,
                true,
                pointerAtBadAngle);
            bool hitSomeThing = false;

            if (!_active)
            {
                return;
            }

            if (_arc.DrawArc(out RaycastHit hitInfo))
            {
                hitSomeThing = true;
            }

            if (hitSomeThing && _arc.PlayerTeleportTransformCandidate != Vector3.zero && _arc.IsArcValid())
            {
                DestinationReticle.SetActive(true);
                DestinationReticle.transform.position = _arc.PlayerTeleportTransformCandidate;
            }
            else
            {
                DestinationReticle.SetActive(false);
            }
        }


        private void TeleportPlayer()
        {
            if (_arc.PlayerTeleportTransformCandidate == Vector3.zero)
            {
                return;
            }

            InputAdapter.Instance.PlayerController.Teleport(_arc.PlayerTeleportTransformCandidate);
        }
    }
}
