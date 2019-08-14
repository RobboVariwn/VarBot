using UnityEngine;
using UnityEngine.XR;

namespace Varwin.VRInput
{
    public class UiPointer : MonoBehaviour, PointerController.IBasePointer
    {
        
        public bool CanClick() => _events.IsTriggerReleased() && CheckIfOverPointable();

        public bool CanToggle() => CheckIfOverPointable() || IsPriority;

        public void Click()
        {
            OnPointerClick();
            Pointer.GetComponent<MeshRenderer>().material.color = ClickColor;
            Pointer.transform.localScale = new Vector3(Thickness * 3.5f, Thickness * 3.5f, dist);
        }

        public void Toggle(bool value)
        {
            if (Pointer)
            {
                Pointer.SetActive(value);
            }
        }

        public void Toggle()
        {
            if (!Pointer)
            {
                return;
            }

            Pointer.SetActive(!Pointer.activeSelf);
        }
        
        public bool IsActive() => Pointer && Pointer.activeSelf;

        private bool _active;
        private bool _canClick;
        private ControllerInput.ControllerEvents _events;

        public Color DefaultColor = Color.cyan;
        public float Thickness = 0.002f;
        public Color ClickColor = Color.white;
        public GameObject Holder;
        public Color HoverColor = Color.green;
        public GameObject Pointer;
        public bool AddRigidBody;
        public bool IsPriority = false;
        
        private Transform _previousContact;
        public Transform _pointerOrigin;
        private bool _isActive;
        private static readonly int ColorShader = Shader.PropertyToID("_Color");
        private Color _currentColor = Color.cyan;
        private bool bHit;
        private Ray raycast;
        private RaycastHit hit;
        private RaycastHit _previousHit;
        private float dist;
        
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
            
            Holder = new GameObject();
            Holder.transform.parent = _pointerOrigin;
            Holder.transform.localPosition = Vector3.zero;
            Holder.transform.localRotation = Quaternion.identity;

            Pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Pointer.transform.parent = Holder.transform;
            Pointer.transform.localScale = new Vector3(Thickness, Thickness, 100f);
            Pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
            Pointer.transform.localRotation = Quaternion.identity;

            BoxCollider boxCollider = Pointer.GetComponent<BoxCollider>();

            if (AddRigidBody)
            {
                if (boxCollider)
                {
                    boxCollider.isTrigger = true;
                }

                Rigidbody rigidBody = Pointer.AddComponent<Rigidbody>();
                rigidBody.isKinematic = true;
            }
            else
            {
                if (boxCollider)
                {
                    Destroy(boxCollider);
                }
            }

            Material newMaterial = new Material(Shader.Find("Unlit/TransparentColor"));
            newMaterial.SetColor(ColorShader, _currentColor);
            newMaterial.renderQueue = 3000;
            Pointer.GetComponent<MeshRenderer>().material = newMaterial;
            Pointer.SetActive(false);

            _events = InputAdapter.Instance.ControllerInput.ControllerEventFactory.GetFrom(gameObject);
        }

        public void OnPointerIn(Transform target)
        {
            if (hit.collider == null)
            {
                return;
            }
            
            PointableObject pointableObject = hit.collider.gameObject.GetComponent<PointableObject>();

            if (pointableObject == null)
            {
                return;
            }

            try
            {
                _currentColor = HoverColor;
                pointableObject.OnPointerIn();
            }
            catch
            {
                Debug.LogError($"On pointer in error in {pointableObject.name}");
            }
        }

        public void OnPointerClick()
        {
            if (hit.collider == null)
            {
                return;
            }
            
            PointableObject pointableObject = hit.collider.gameObject.GetComponent<PointableObject>();

            if (pointableObject == null)
            {
                return;
            }

            try
            {
                pointableObject.OnPointerClick();
            }
            catch
            {
                Debug.LogError($"On pointer click error in {pointableObject.name}");
            }
        }

        public void OnPointerOut(Transform target)
        {
            if (_previousHit.collider == null)
            {
                return;
            }
            
            PointableObject pointableObject = _previousHit.collider.gameObject.GetComponent<PointableObject>();
            _currentColor = DefaultColor;

            if (pointableObject == null)
            {
                return;
            }

            try
            {
                pointableObject.OnPointerOut();
            }
            catch
            {
                Debug.LogError($"On pointer out error in {pointableObject.name}");
            }
        }


        private bool GetPointable()
        {
            if (hit.collider == null)
            {
                return false;
            }
            
            PointableObject pointableObject = hit.collider.gameObject.GetComponent<PointableObject>();

            return pointableObject != null;
        }
            

        private bool CheckIfOverPointable() => GetPointable();

        public void UpdateState()
        {
            dist = 100f;

            Vector3 originPosition = _pointerOrigin != null ? _pointerOrigin.position : transform.position;
            Vector3 originDirection = _pointerOrigin != null ? _pointerOrigin.forward : transform.forward;
            
            // ReSharper disable once Unity.InefficientPropertyAccess
            raycast = new Ray(originPosition, originDirection);

            var mask = LayerMask.GetMask("UI");
 
            bHit = Physics.Raycast(raycast, out hit, 50, mask);

            if (_previousContact && _previousContact != hit.transform)
            {
                OnPointerOut(_previousContact);
                _previousContact = null;
            }

            if (bHit && _previousContact != hit.transform)
            {
                OnPointerIn(hit.transform);
                _previousContact = hit.transform;
                _previousHit = hit;
            }

            if (!bHit)
            {
                _previousContact = null;
            }

            if (bHit && hit.distance < 100f)
            {
                dist = hit.distance;
            }

            Pointer.transform.localScale = new Vector3(Thickness, Thickness, dist);
            Pointer.GetComponent<MeshRenderer>().material.color = _currentColor;

            Pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
        }
    }
}
