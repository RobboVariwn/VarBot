using System.Collections.Generic;
using UnityEngine;

namespace Varwin.Public
{
    /// <inheritdoc />
    /// <summary>
    /// Point to join object class
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class JointPoint : MonoBehaviour
    {

        public bool WorksInEditMode = true;
        public bool WorksInViewMode = true;

        public bool IsForceLocked;

        /// <summary>
        /// Connected Point
        /// </summary>
        public JointPoint ConnectedJointPoint { get; private set; }

        /// <summary>
        /// Jointed wrapper
        /// </summary>
        private GameObject _connectedGameObject;

        /// <summary>
        /// My joint behaviour
        /// </summary>
        public JointBehaviour JointBehaviour { get; private set; }

        /// <summary>
        /// Joint key
        /// </summary>
        public string Key = "joint";

        /// <summary>
        /// List of points for connection
        /// </summary>
        public List<string> AcceptedKeys = new List<string>();

        /// <summary>
        /// Is point free
        /// </summary>
        public bool IsFree { get; private set; }

        /// <summary>
        /// Is point locked
        /// </summary>
        public bool IsLocked { get; private set; }

        private ConfigurableJoint _configurableJoint;
        private FixedJoint _fixedJoint;

        public delegate void JointEnterHandler(JointPoint senderJoint, JointPoint nearJoint);

        public delegate void JointExitHandler(JointPoint senderJoint, JointPoint nearJoint);

        /// <summary>
        /// Another joint entered me
        /// </summary>
        public event JointEnterHandler OnJointEnter;

        /// <summary>
        /// Another joint exited me
        /// </summary>
        public event JointExitHandler OnJointExit;

        public void Init(JointBehaviour jointBehaviour)
        {
            JointBehaviour = jointBehaviour;
            IsFree = true;
            GetComponent<Collider>().isTrigger = true;
        }

        private Wrapper _connectedWrapper;

        private void OnTriggerEnter(Collider other)
        {
            JointPoint otherJointPoint = GetValidJoint(other);

            if (otherJointPoint == null)
            {
                return;
            }
            
            OnJointEnter?.Invoke(this, otherJointPoint);
        }

        private void OnTriggerExit(Collider other)
        {

            JointPoint otherJointPoint = other.GetComponent<JointPoint>();

            if (otherJointPoint == null)
            {
                return;
            }

            if (ConnectedJointPoint != null && otherJointPoint != ConnectedJointPoint)
            {
                return;
            }

            if (otherJointPoint.JointBehaviour == null)
            {
                return;
            }

            Rigidbody otherBody = otherJointPoint.JointBehaviour.gameObject.GetComponent<Rigidbody>();
            Rigidbody myBody = JointBehaviour.gameObject.GetComponent<Rigidbody>();

            if (otherBody != null && otherBody.isKinematic && !myBody.isKinematic)
            {
                Debug.Log("Ignore kinematic trigger exit");

                return;
            }

            if (JointBehaviour.IsTempConnectionCreated)
            {
                Debug.Log("Ignore IsTempConnectionCreated");

                return;
            }

            Debug.Log("Joint exit!");
            Disconnect();
            UnLock();
            OnJointExit?.Invoke(this, otherJointPoint);
        }


        public void Lock(bool sender = true)
        {
            if (!IsLocked && !IsFree)
            {
                IsLocked = true;

                if (_fixedJoint != null)
                {
                    Destroy(_fixedJoint);
                }

                if (sender)
                {
                    if (_connectedGameObject != null)
                    {
                        var jointConnectedBody = _connectedGameObject.GetComponent<Rigidbody>();

                        if (jointConnectedBody != null && !jointConnectedBody.isKinematic)
                        {
                            _configurableJoint = JointBehaviour.CreateJoint();
                            _configurableJoint.connectedBody = jointConnectedBody;
                        }
                    }

                }

                if (ConnectedJointPoint != null)
                {
                    ConnectedJointPoint.Lock(false);
                }
                else
                {
                    Debug.Log("Canditate point is null! Unlock!");
                    UnLock();
                }

            }
        }

        public void UnLock()
        {
            if (IsLocked)
            {
                Debug.Log("UnLock Invoked!");

                IsLocked = false;

                if (_configurableJoint != null)
                {
                    Destroy(_configurableJoint);
                }

                if (ConnectedJointPoint != null)
                {
                    ConnectedJointPoint.UnLock();
                }
            }
        }

        public void Connect(GameObject jointGameObject, JointPoint jointPoint, bool sender = true)
        {
            if (!IsFree || jointPoint == null)
            {
                return;
            }

            IsFree = false;
            _connectedGameObject = jointGameObject;
            IWrapperAware wrapperAware = _connectedGameObject.GetComponentInChildren<IWrapperAware>();

            if (wrapperAware != null)
            {
                _connectedWrapper = wrapperAware.Wrapper();
                JointBehaviour.AddConnectedJoint(_connectedWrapper, jointPoint.JointBehaviour);
            }

            if (sender)
            {
                var jointConnectedBody = _connectedGameObject.GetComponent<Rigidbody>();

                if (jointConnectedBody != null && !jointConnectedBody.isKinematic)
                {
                    _fixedJoint = JointBehaviour.gameObject.AddComponent<FixedJoint>();
                    _fixedJoint.connectedBody = jointConnectedBody;
                    _fixedJoint.breakForce = 600f;
                    _fixedJoint.breakTorque = 600f;
                }
            }

            ConnectedJointPoint = jointPoint;
            ConnectedJointPoint.Connect(JointBehaviour.gameObject, this, false);

        }

        public void Disconnect()
        {
            if (!IsFree)
            {
                Debug.Log("UnJoint Invoked!");
                IsFree = true;

                JointBehaviour.RemoveDisconnectedJoint(_connectedWrapper, ConnectedJointPoint.JointBehaviour);

                ConnectedJointPoint.Disconnect();
                ConnectedJointPoint = null;
                _connectedWrapper = null;

                if (_fixedJoint != null)
                {
                    Destroy(_fixedJoint);
                }

                Rigidbody behaviourBody = JointBehaviour.GetComponent<Rigidbody>();

                if (behaviourBody != null)
                {
                    behaviourBody.WakeUp();
                }
            }

        }

        private JointPoint GetValidJoint(Collider other)
        {

            if (!IsFree)
            {
                return null;
            }

            if (IsLocked)
            {
                return null;
            }

            JointPoint otherJointPoint = other.gameObject.GetComponent<JointPoint>();

            if (otherJointPoint == null)
            {
                return null;
            }

            if (!otherJointPoint.IsFree)
            {
                return null;
            }

            if (otherJointPoint.IsLocked)
            {
                return null;
            }

            if (!AcceptedKeys.Contains(otherJointPoint.Key))
            {
                return null;
            }

            if (ProjectData.GameMode == GameMode.Edit)
            {
                if (!WorksInEditMode || !otherJointPoint.WorksInEditMode)
                {
                    return null;
                }
            }

            if (ProjectData.GameMode == GameMode.View || ProjectData.GameMode == GameMode.Preview)
            {
                if (!WorksInViewMode || !otherJointPoint.WorksInViewMode)
                {
                    return null;
                }
            }

            return otherJointPoint;
        }
    }
}