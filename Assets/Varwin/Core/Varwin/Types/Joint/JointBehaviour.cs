using System;
using System.Collections.Generic;
using System.Linq;
using HighlightPlus;
using UnityEngine;
using UnityEngine.Events;
using Varwin.Public;

namespace Varwin
{
    public class JointBehaviour : MonoBehaviour
    {
        public bool AutoLock = true;
        public List<JointPoint> JointPoints;
        private bool IsGrabbed { get; set; }
        public bool IsJoined => CountConnections > 0;
        public bool IsFree => CountConnections == 0;
        public GameObject ConnectionPreview;
        public Wrapper Wrapper;
        public static bool IsTempConnectionCreated;

        private static readonly Dictionary<string, GameObject> ConnectionPreviews = new Dictionary<string, GameObject>();
        private static JointPreviewCollection _jointPreviewCollection;
        private readonly List<Wrapper> _connectedWrappers = new List<Wrapper>();
        [SerializeField] private List<JointBehaviour> _connectedJointBehaviours = new List<JointBehaviour>();
        private static GameObject _shownDrawConnectionObject;
        private int _saveIter;
        private JointPoint _senderJointPoint;
        private JointPoint _nearJointPoint;
        private JointBehaviour _nearJointBehaviour;
        private Rigidbody _rigidbody;
        private HighlightEffect _highlightEffect;
        private Transform _parentOnStart;

        public delegate void ConnectedHandler();
        public delegate void DisconnectedHandler();
        public event ConnectedHandler OnConnect;
        public event DisconnectedHandler OnDisconnect;

        public bool IsNearJointPointHighlighted => _nearJointPoint != null;
        
        public bool ForceLock
        {
            get
            {
                return IsForceLocked();
            }
            set
            {
                ForceLockConnections(value);
            }
        }

        public void Init()
        {
            JointPoints = gameObject.GetComponentsInChildren<JointPoint>().ToList();

            foreach (JointPoint jointPoint in JointPoints)
            {
                jointPoint.Init(this);
                jointPoint.OnJointEnter += JointPointOnJointEnter;
                jointPoint.OnJointExit += JointPointOnJointExit;
            }
            
            _highlightEffect = GetComponent<HighlightEffect>();

            if (_highlightEffect != null)
            {
                _savedColor = _highlightEffect.outlineColor;
            }
        }

        private void Start()
        {
            CreateConnectionPreview();
            RegisterIntractableEvents();

            IWrapperAware wrapperAware = gameObject.GetComponentInChildren<IWrapperAware>();
            Wrapper = wrapperAware != null ? wrapperAware.Wrapper() : new NullWrapper(gameObject);
            _rigidbody = gameObject.GetComponent<Rigidbody>();
            _parentOnStart = transform.parent;
        }

        /// <summary>
        /// Mapped to intractable object
        /// </summary>
        public void OnGrabStart()
        {
            IsGrabbed = true;

            if (CountConnections > 1 && _rigidbody.isKinematic)
            {
                CreateTempConnections();
            }

            if (CountConnections <= 1 && !ForceLock)
            {
                UnLockAndDisconnectPoints();
            }
            else
            {
                MarkJointsGrabbed(true);
                transform.parent = _parentOnStart;
            }
        }

        /// <summary>
        /// Mapped to intractable object
        /// </summary>
        public void OnGrabEnd()
        {
            Debug.Log("Joint un grabbed!");

            if (_nearJointBehaviour != null)
            {
                _nearJointBehaviour.ConnectCandidate();
            }

            HideConnectionJoint();

            if (IsTempConnectionCreated)
            {
                DestroyTempConnection();
                IsTempConnectionCreated = false;
            }

            MarkJointsGrabbed(false);
        }

        private Color _savedColor;


        /// <summary>
        /// Mapped to intractable object
        /// </summary>
        public void OnTouchStart()
        {
            SetValidHighLightColor();
        }
        
        /// <summary>
        /// Mapped to intractable object
        /// </summary>
        public void OnTouchEnd()
        {
            SetDefaultHighLight();
        }

        private void SetValidHighLightColor()
        {
            if (CountConnections == 0)
            {
                return;
            }

            if (_highlightEffect == null)
            {
                return;
            }

            if (CountConnections != 1 || ForceLock)
            {
                _highlightEffect.outlineColor = Color.red;
            }

            else
            {
                _highlightEffect.outlineColor = Color.green;
            }
        }

       

        private void SetDefaultHighLight()
        {
            HighlightEffect highlightEffect = GetComponent<HighlightEffect>();

            if (highlightEffect == null)
            {
                return;
            }

            highlightEffect.outlineColor = _savedColor;
        }

        public void RegisterIntractableEvents()
        {
            InteractableObjectBehaviour intractableObjectBehaviour = gameObject.GetComponent<InteractableObjectBehaviour>();

            if (intractableObjectBehaviour == null)
            {
                return;
            }

            if (intractableObjectBehaviour.OnGrabStarted == null)
            {
                intractableObjectBehaviour.OnGrabStarted = new UnityEvent();
            }

            if (intractableObjectBehaviour.OnGrabEnded == null)
            {
                intractableObjectBehaviour.OnGrabEnded = new UnityEvent();
            }

            if (intractableObjectBehaviour.OnTouchStarted == null)
            {
                intractableObjectBehaviour.OnTouchStarted = new UnityEvent();
            }

            if (intractableObjectBehaviour.OnTouchEnded == null)
            {
                intractableObjectBehaviour.OnTouchEnded = new UnityEvent();
            }

            intractableObjectBehaviour.OnGrabStarted.AddListener(OnGrabStart);
            intractableObjectBehaviour.OnGrabEnded.AddListener(OnGrabEnd);
            intractableObjectBehaviour.OnTouchStarted.AddListener(OnTouchStart);
            intractableObjectBehaviour.OnTouchEnded.AddListener(OnTouchEnd);
        }

        private void CreateConnectionPreview()
        {

            if (_jointPreviewCollection == null)
            {
                GameObject o = new GameObject("Joint PreviewCollection");
                _jointPreviewCollection = o.AddComponent<JointPreviewCollection>();
            }

            if (ConnectionPreviews.ContainsKey(gameObject.name))
            {
                ConnectionPreview = ConnectionPreviews[gameObject.name];
            }

            else
            {
                ConnectionPreview = Instantiate(gameObject, _jointPreviewCollection.gameObject.transform, true);
                _saveIter = 10;
                DestroyComponents();
                HighlightEffect highlightEffect = ConnectionPreview.GetComponent<HighlightEffect>();

                if (highlightEffect != null)
                {
                    highlightEffect.highlighted = true;
                    highlightEffect.overlay = 0.75f;
                    highlightEffect.overlayColor = Color.white;
                    highlightEffect.overlayAnimationSpeed = 0.6f;
                    highlightEffect.outline = 0;
                    highlightEffect.glowDithering = false;
                    highlightEffect.glowPasses = null;
                    highlightEffect.glow = 0;
                    highlightEffect.seeThrough = SeeThroughMode.Never;
                }
                
                ConnectionPreviews.Add(gameObject.name, ConnectionPreview);
                _jointPreviewCollection.DrawGameObjects.Add(ConnectionPreview);
                ConnectionPreview.SetActive(false);
            }

        }

        private void DestroyComponents()
        {
            while (true)
            {
                var components = ConnectionPreview.GetComponentsInChildren<Component>(true);
                List<Component> requiresComponents = new List<Component>();

                foreach (Component component in components)
                {
                    if (component == null || component is HighlightEffect || component is MeshFilter || component is MeshRenderer || component is SkinnedMeshRenderer || component is Transform)
                    {
                        continue;
                    }

                    try
                    {
                        if (component.gameObject.CanDestroy(component.GetType()))
                        {
                            DestroyImmediate(component);
                        }

                        else
                        {
                            requiresComponents.Add(component);
                        }
                    }
                    catch
                    {
                        TryFixDestroy(component);
                    }
                }

                if (requiresComponents.Count <= 0 || _saveIter <= 0)
                {
                    return;
                }

                _saveIter--;
            }
        }

        private static void TryFixDestroy(Component component)
        {
            Behaviour behaviour = component as Behaviour;

            if (behaviour != null)
            {
                behaviour.enabled = false;
            }

            Collider collider1 = component as Collider;

            if (collider1 != null)
            {
                collider1.enabled = false;
            }
        }

        private void JointPointOnJointExit(JointPoint senderJoint, JointPoint nearJoint)
        {
            if (IsGrabbed)
            {
                return;
            }

            HideConnectionJoint();
            _senderJointPoint = null;
            _nearJointBehaviour = null;
            _nearJointPoint = null;
            Debug.Log($"Joint exit! {senderJoint} {nearJoint}");

        }

        private void JointPointOnJointEnter(JointPoint senderJoint, JointPoint nearJoint)
        {
            _senderJointPoint = senderJoint;
            _nearJointPoint = nearJoint;
            _nearJointBehaviour = nearJoint.JointBehaviour;

            if (!IsGrabbed  || !senderJoint.IsFree || !nearJoint.IsFree)
            {
                return;
            }
            
            if (IsGrabbed && nearJoint.JointBehaviour.IsGrabbed)
            {
                //ToDo case 3 here
                return;
            }
             
            nearJoint.JointBehaviour.DrawConnectionJoint(nearJoint, senderJoint);
            Debug.Log($"Joint enter! {senderJoint} {nearJoint}");

        }


        private void DrawConnectionJoint(JointPoint senderJoint, JointPoint nearJoint)
        {
            if (_shownDrawConnectionObject == null)
            {
                _shownDrawConnectionObject = Instantiate(nearJoint.JointBehaviour.ConnectionPreview);
            }

            TransformToJoint(_shownDrawConnectionObject, nearJoint, senderJoint);
            _shownDrawConnectionObject.transform.SetParent(senderJoint.transform);
            _shownDrawConnectionObject.SetActive(true);
        }

        private static void TransformToJoint(GameObject go, JointPoint nearJointPoint, JointPoint senderJoint)
        {
            GameObject temp = new GameObject("temp");
            Transform saveTransform = go.transform.parent;
            var var = nearJointPoint.transform;
            temp.transform.position = var.position;
            temp.transform.rotation = var.rotation;
            var o = nearJointPoint.JointBehaviour.gameObject;
            go.transform.position = o.transform.position;
            go.transform.rotation = o.transform.rotation;
            go.transform.SetParent(temp.transform);
            var var2 = senderJoint.transform;
            temp.transform.position = var2.position;
            temp.transform.rotation = var2.rotation;
            temp.transform.Rotate(new Vector3(180, 0, 0));
            go.transform.SetParent(saveTransform);
            Destroy(temp);
        }

        private void HideConnectionJoint()
        {
            Destroy(_shownDrawConnectionObject);
        }


        public ConfigurableJoint CreateJoint()
        {
            ConfigurableJoint configurableJoint = gameObject.AddComponent<ConfigurableJoint>();
            configurableJoint.xMotion = ConfigurableJointMotion.Locked;
            configurableJoint.yMotion = ConfigurableJointMotion.Locked;
            configurableJoint.zMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
            configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
            configurableJoint.projectionDistance = 0;
            configurableJoint.projectionAngle = 0;

            return configurableJoint;
        }

        private bool _createdTempConnection;

        private void CreateTempConnections()
        {
            IsTempConnectionCreated = true;

            if (_createdTempConnection)
            {
                return;
            }

            _rigidbody.isKinematic = false;

            foreach (JointPoint jointPoint in JointPoints)
            {
                if (!jointPoint.IsFree)
                {
                    ConfigurableJoint joint = jointPoint.ConnectedJointPoint.JointBehaviour.gameObject.GetComponent<ConfigurableJoint>();

                    if (joint != null)
                    {
                        continue;
                    }

                    joint = jointPoint.ConnectedJointPoint.JointBehaviour.CreateJoint();
                    joint.connectedBody = _rigidbody;
                    jointPoint.ConnectedJointPoint.JointBehaviour.CreateTempConnections();
                }
            }

            _createdTempConnection = true;
        }

        public void UnLockPoints()
        {
            foreach (JointPoint jointPoint in JointPoints)
            {
                jointPoint.UnLock();
            }
        }

        public void UnLockAndDisconnectPoints()
        {
            foreach (JointPoint jointPoint in JointPoints)
            {
                jointPoint.UnLock();
                jointPoint.Disconnect();
            }
        }

        private void DestroyTempConnection()
        {
            if (!_createdTempConnection)
            {
                return;
            }

            foreach (JointPoint jointPoint in JointPoints)
            {
                if (jointPoint.IsFree)
                {
                    continue;
                }

                ConfigurableJoint joint = jointPoint.ConnectedJointPoint.JointBehaviour.gameObject.GetComponent<ConfigurableJoint>();
                _rigidbody.isKinematic = true;

                if (joint != null)
                {
                    Destroy(joint);
                }

                _createdTempConnection = false;
                jointPoint.ConnectedJointPoint.JointBehaviour.DestroyTempConnection();
            }
        }

        public void AddConnectedJoint(Wrapper wrapper, JointBehaviour jointBehaviour)
        {
            _connectedWrappers.Add(wrapper);
            _connectedJointBehaviours.Add(jointBehaviour);
            Debug.Log($"Wrapper {wrapper} was connected! Connections count = {_connectedWrappers.Count}");
        }

        public void RemoveDisconnectedJoint(Wrapper wrapper, JointBehaviour jointBehaviour)
        {
            if (!_connectedWrappers.Contains(wrapper))
            {
                return;
            }

            _connectedWrappers.Remove(wrapper);
            _connectedJointBehaviours.Remove(jointBehaviour);
            Debug.Log($"Wrapper {wrapper} was disconnected! Connections count = {_connectedWrappers.Count}");
            SetDefaultHighLight();
            OnDisconnect?.Invoke();
        }

        public List<Wrapper> GetConnectedWrappers() => _connectedWrappers;

        private void ConnectCandidate()
        {
            if (_nearJointPoint == null || _senderJointPoint == null)
            {
                return;
            }
            
            if (_nearJointPoint.JointBehaviour._rigidbody.isKinematic != _senderJointPoint.JointBehaviour._rigidbody.isKinematic)
            {
                Debug.Log($"Can not connect joints with different kinematics");
                return;
            }

            Debug.Log($"Joint invoke! {_senderJointPoint}");
            TransformToJoint(_nearJointPoint.JointBehaviour.gameObject, _nearJointPoint, _senderJointPoint);
            _senderJointPoint.Connect(_nearJointPoint.JointBehaviour.gameObject, _nearJointPoint);

            if (AutoLock)
            {
                _senderJointPoint.Lock();
            }

            OnConnect?.Invoke();
            _nearJointPoint.JointBehaviour.OnConnect?.Invoke();
        }

        public void ConnectToJointPoint(JointPoint myPoint, JointPoint otherPoint)
        {
            TransformToJoint(gameObject, myPoint, otherPoint);
            otherPoint.Connect(gameObject, myPoint);
            OnConnect?.Invoke();
            otherPoint.JointBehaviour.OnConnect?.Invoke();

            if (AutoLock)
            {
                otherPoint.Lock();
            }
        }

        public int CountConnections
        {
            get
            {
                int count = 0;

                foreach (JointPoint jointPoint in JointPoints)
                {
                    if (!jointPoint.IsFree)
                    {
                        count++;
                    }
                }

                return count;
            }

        }

        private bool _childMoved;
        private Transform _savedParentTransform;

        public void MakeConnectionsChild()
        {
            if (_childMoved)
            {
                return;
            }

            foreach (JointPoint jointPoint in JointPoints)
            {
                if (jointPoint.IsFree)
                {
                    continue;
                }

                _savedParentTransform = jointPoint.ConnectedJointPoint.JointBehaviour.gameObject.transform.parent;
                jointPoint.ConnectedJointPoint.JointBehaviour.gameObject.transform.SetParent(gameObject.transform);
                _childMoved = true;
                jointPoint.ConnectedJointPoint.JointBehaviour.MakeConnectionsChild();
            }

        }

        public void RestoreParents()
        {
            if (!_childMoved)
            {
                return;
            }

            foreach (JointPoint jointPoint in JointPoints)
            {
                if (jointPoint.IsFree)
                {
                    continue;
                }

                jointPoint.ConnectedJointPoint.JointBehaviour.gameObject.transform.SetParent(_savedParentTransform);
                _childMoved = false;
                jointPoint.ConnectedJointPoint.JointBehaviour.RestoreParents();
            }
        }

        public void MarkJointsGrabbed(bool value)
        {
            if (IsGrabbed == value)
            {
                return;
            }

            IsGrabbed = value;

            foreach (JointPoint jointPoint in JointPoints)
            {
                if (jointPoint.IsFree)
                {
                    continue;
                }

                jointPoint.ConnectedJointPoint.JointBehaviour.IsGrabbed = value;
                jointPoint.ConnectedJointPoint.JointBehaviour.MarkJointsGrabbed(value);

                //ToDo Make @case 11 here
           
            }

        }

        /// <summary>
        /// Get list of connected wrappers from all joint points
        /// </summary>
        /// <param name="sender"></param>
        /// <returns>List order by me</returns>
        public List<Wrapper> GetAllConnectedWrappers(List<Wrapper> sender = null)
        {
            var result = sender ?? new List<Wrapper> {Wrapper};

            foreach (JointPoint jointPoint in JointPoints)
            {
                if (jointPoint.IsFree)
                {
                    continue;
                }

                Wrapper candidate = jointPoint.ConnectedJointPoint.JointBehaviour.Wrapper;

                if (result.Contains(candidate))
                {
                    continue;
                }

                result.Add(candidate);
                jointPoint.ConnectedJointPoint.JointBehaviour.GetAllConnectedWrappers(result);
            }


            return result;
        }

        /// <summary>
        /// Get list of connected wrappers from joint point
        /// </summary>
        /// <param name="jointPoint">Target joint point</param>
        /// <param name="sender"></param>
        /// <returns>List order by me</returns>
        public List<Wrapper> GetAllConnectedWrappers(JointPoint jointPoint, List<Wrapper> sender = null)
        {
            if (jointPoint == null)
            {
                return null;
            }

            var result = sender ?? new List<Wrapper>() {Wrapper};

            if (jointPoint.IsFree)
            {
                return result;
            }

            Wrapper candidate = jointPoint.ConnectedJointPoint.JointBehaviour.Wrapper;
            result.Add(candidate);
            jointPoint.ConnectedJointPoint.JointBehaviour.GetAllConnectedWrappers(result);

            return result;
        }
        
        public List<JointBehaviour> GetAllConnectedJoints(List<JointBehaviour> sender = null)
        {
            var result = sender ?? new List<JointBehaviour> {this};

            foreach (JointPoint jointPoint in JointPoints)
            {
                if (jointPoint.IsFree)
                {
                    continue;
                }

                JointBehaviour candidate = jointPoint.ConnectedJointPoint.JointBehaviour;

                if (result.Contains(candidate))
                {
                    continue;
                }

                result.Add(candidate);
                jointPoint.ConnectedJointPoint.JointBehaviour.GetAllConnectedJoints(result);
            }


            return result;
        }
        
        public List<JointBehaviour> GetAllConnectedJoints(JointPoint jointPoint, List<JointBehaviour> sender = null)
        {
            if (jointPoint == null)
            {
                return null;
            }

            var result = sender ?? new List<JointBehaviour>() {this};

            if (jointPoint.IsFree)
            {
                return result;
            }

            JointBehaviour candidate = jointPoint.ConnectedJointPoint.JointBehaviour;
            result.Add(candidate);
            jointPoint.ConnectedJointPoint.JointBehaviour.GetAllConnectedJoints(result);

            return result;
        }

        public void LockPoints()
        {
            foreach (JointPoint jointPoint in JointPoints)
            {
                jointPoint.Lock();
            }
        }
        
        private bool IsForceLocked()
        {
            foreach (JointPoint jointPoint in JointPoints)
            {
                if (jointPoint.IsForceLocked)
                {
                    return true;
                }
            }

            return false;
        }

        private void ForceLockConnections(bool value)
        {
            SetValidHighLightColor();
            foreach (JointPoint point in JointPoints)
            {
                if (point.IsFree)
                {
                    continue;
                }

                point.IsForceLocked = value;
                point.ConnectedJointPoint.IsForceLocked = value;
                point.ConnectedJointPoint.JointBehaviour.SetValidHighLightColor();
            }
        }

    }

    internal static class GameObjectEx
    {
        private static bool Requires(Type obj, Type requirement)
        {
            //also check for m_Type1 and m_Type2 if required
            return Attribute.IsDefined(obj, typeof(RequireComponent))
                   && Attribute.GetCustomAttributes(obj, typeof(RequireComponent))
                       .OfType<RequireComponent>()
                       .Any(rc => rc.m_Type0.IsAssignableFrom(requirement));
        }

        internal static bool CanDestroy(this GameObject go, Type t)
        {
            return !go.GetComponents<Component>().Any(c => Requires(c.GetType(), t));
        }
    }
}