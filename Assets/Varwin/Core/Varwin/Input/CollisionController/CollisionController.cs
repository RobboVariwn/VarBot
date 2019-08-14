using System.Collections.Generic;
using HighlightPlus;
using UnityEngine;
using Varwin.Public;
using Varwin.VRInput;

namespace Varwin.ObjectsInteractions
{
    public class CollisionController : MonoBehaviour
    {
        [SerializeField]
        private List<Collider> _childColliders;

        [SerializeField]
        private bool _isBlocked;

        private List<CollisionControllerElement> _tempColliders;
        private VarwinHighlightEffect _highlight;
        
        private HightLightConfig _defaultHighlightConfig;
        private HightLightConfig _collisionControllerHighlightConfig;
        
        private InputController _inputController;

        public bool IsBlocked() => _isBlocked;

        private float fallDistance = 10f;

        public void InitializeController(InputController inputController = null)
        {
            _inputController = inputController;

            if (_inputController != null)
            {
                _defaultHighlightConfig = _inputController.DefaultHighlightConfig;
            }
            else
            {
                var defHighlighter = gameObject.AddComponent<DefaultHightlighter>();
                _defaultHighlightConfig = defHighlighter.HightLightConfig();
            }
    
            _highlight = gameObject.GetComponent<VarwinHighlightEffect>();

            if (_highlight == null)
            {
                _highlight = gameObject.AddComponent<VarwinHighlightEffect>();
            }


            HightLightConfig config = new HightLightConfig(true,
                0.3f,
                Color.red, 
                false,
                0.2f,
                0.1f,
                0.3f,
                Color.red,
                false,
                0.8f,
                0.5f,
                Color.red,
                true,
                0.5f,
                1.0f,
                Color.red);
            

            _collisionControllerHighlightConfig = config;
            
            _highlight.SetConfiguration(_collisionControllerHighlightConfig);

            _tempColliders = new List<CollisionControllerElement>();
            _childColliders = new List<Collider>();

            List<GameObject> objectsWithColliders = new List<GameObject>();
            Collider[] children = GetComponentsInChildren<Collider>();

            foreach (Collider child in children)
            {
                if (!child.isTrigger && child.enabled)
                {
                    _childColliders.Add(child);

                    if (!objectsWithColliders.Contains(child.gameObject))
                    {
                        objectsWithColliders.Add(child.gameObject);
                    }
                }
            }

            foreach (GameObject objectWithCollider in objectsWithColliders)
            {
                CollisionControllerElement triggerObject = CreateTriggersColliders(objectWithCollider);
                triggerObject.OnCollisionEnterDelegate += OnCollide;
                triggerObject.OnTriggerExitDelegate += OnColliderExit;
                _tempColliders.Add(triggerObject);
            }

            ProjectData.GameModeChanged += GameModeChanged;
        }

        private void GameModeChanged(GameMode gm)
        {
            Unblock();
            _inputController.EnableDrop();
            _inputController.ForceDropIfNeeded();
            _inputController.ReturnPosition();
            _inputController.ControllerEvents.OnGripReleased(_controllerInteractionEventArgs);
        }

        private CollisionControllerElement CreateTriggersColliders(GameObject originalColliderHolder)
        {
            GameObject collidersContainer = new GameObject("TempCollidersContainer");
            collidersContainer.transform.parent = originalColliderHolder.transform;
            collidersContainer.transform.localPosition = Vector3.zero;
            collidersContainer.transform.localRotation = Quaternion.identity;
            collidersContainer.transform.localScale = Vector3.one * 1.1f;

            Collider[] colliders = originalColliderHolder.GetComponents<Collider>();

            foreach (Collider collider in colliders)
            {
                if (!collider.isTrigger)
                {
                    Collider newCollider = DuplicateComponent(collider, collidersContainer);

                    if (newCollider.GetType() == typeof(MeshCollider))
                    {
                        MeshCollider meshCollider = (MeshCollider) newCollider;
                        meshCollider.convex = true;
                    }

                    newCollider.isTrigger = true;

                    if (ProjectData.GameMode == GameMode.Edit)
                    {
                        collider.enabled = false;
                    }
                }
            }

            CollisionControllerElement element = collidersContainer.AddComponent<CollisionControllerElement>();

            return element;
        }

        T DuplicateComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            var dst = destination.AddComponent(type) as T;
            var fields = type.GetFields();

            foreach (var field in fields)
            {
                if (field.IsStatic)
                    continue;
                field.SetValue(dst, field.GetValue(original));
            }

            var props = type.GetProperties();

            foreach (var prop in props)
            {
                if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name")
                    continue;
                prop.SetValue(dst, prop.GetValue(original, null), null);
            }

            return dst as T;
        }

        private void Update()
        {
            if (_inputController == null)
            {
                return;
            }

            CheckFalling();

            if (_isBlocked)
            {
                _inputController.DisableDrop();
            }
            else
            {
                _inputController.EnableDrop();
                _inputController.ForceDropIfNeeded();
            }
        }

        private void OnDestroy()
        {
            if (_childColliders != null && _childColliders.Count > 0)
            {
                foreach (Collider childCollider in _childColliders)
                {
                    childCollider.enabled = true;
                }
            }

            if (_tempColliders != null)
            {
                foreach (CollisionControllerElement collisionControllerElement in _tempColliders)
                {
                    collisionControllerElement.OnTriggerExitDelegate -= OnColliderExit;
                    collisionControllerElement.OnCollisionEnterDelegate -= OnCollide;
                    Destroy(collisionControllerElement.gameObject);
                }
            }

            _inputController?.EnableDrop();

            ProjectData.GameModeChanged -= GameModeChanged;
            
            _highlight = gameObject.GetComponent<VarwinHighlightEffect>();
            _highlight.SetConfiguration(_defaultHighlightConfig);
        }

        private void OnCollide(Collider other)
        {
            Collider[] selfColliders = GetComponentsInChildren<Collider>();

            foreach (Collider selfCollider in selfColliders)
            {
                if (other == selfCollider)
                {
                    return;
                }
            }

            Rigidbody otherBody = other.attachedRigidbody;

            if (otherBody)
            {
                JointBehaviour joint = GetComponent<JointBehaviour>();

                if (joint != null && joint.IsNearJointPointHighlighted)
                {
                    return;
                }

                if (otherBody.isKinematic)
                {
                    Block();
                }
            }
            else if (!other.isTrigger)
            {
                Block();
            }
        }

        private void OnColliderExit(Collider other)
        {
            Unblock();
        }

        /// <summary>
        /// Подсвечиваем и убираем коллайдеры
        /// </summary>
        private void Block()
        {
            if (_isBlocked)
            {
                return;
            }
            
            TurnCollidersOn(false);

            if (_highlight != null)
            {
                _highlight.SetHighlightEnabled(true);
                _highlight.SetConfiguration(_collisionControllerHighlightConfig);
            }

            if (_inputController != null
                && ProjectData.GameMode == GameMode.Edit
                && !_isBlocked)
            {
                _inputController.ControllerEvents.GripPressed += OnGripPressed;
            }

            _isBlocked = true;
        }

        private ControllerInput.ControllerInteractionEventArgs _controllerInteractionEventArgs;

        public void OnGripPressed(object sender, ControllerInput.ControllerInteractionEventArgs e)
        {
            _controllerInteractionEventArgs = e;

            if (_inputController != null
                && ProjectData.GameMode == GameMode.Edit
                && _isBlocked)
            {
                _inputController.ControllerEvents.GripPressed -= OnGripPressed;

                Unblock();
                _inputController.EnableDrop();
                _inputController.ForceDropIfNeeded();
                _inputController.ReturnPosition();
                _inputController.ControllerEvents.OnGripReleased(e);
            }
        }

        /// <summary>
        /// Рассвечиваем и возвращаем коллайдеры
        /// </summary>
        private void Unblock()
        {
            if (ProjectData.GameMode != GameMode.Edit)
            {
                TurnCollidersOn(true);
            }

            if (_highlight != null)
            {
                _highlight.SetHighlightEnabled(false);
            }

            _isBlocked = false;

            if (_inputController != null && ProjectData.GameMode == GameMode.Edit)
            {
                _inputController.ControllerEvents.GripPressed -= OnGripPressed;
            }
        }

        private void TurnCollidersOn(bool on)
        {
            foreach (Collider childCollider in _childColliders)
            {
                childCollider.enabled = on;
            }
        }

        void CheckFalling()
        {
            var controller = _inputController.ControllerEvents.GetController();
            var controllerPosition = controller.transform.position;

            var distance = Vector3.Distance(controllerPosition, transform.position);

            if (distance > fallDistance)
            {
                var allRigidBodies = gameObject.GetComponentsInChildren<Rigidbody>();

                foreach (var rb in allRigidBodies)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
                
                transform.position = new Vector3(controllerPosition.x, controllerPosition.y + 0.5f, controllerPosition.z);
                Unblock();
                
                foreach (var rb in allRigidBodies)
                {
                    rb.ResetInertiaTensor();
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
                
                Destroy(this);
            }
        }

        
    }
}
