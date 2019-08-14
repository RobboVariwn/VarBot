using System.Collections.Generic;
using UnityEngine;

namespace Varwin.ObjectsInteractions
{
    public class CollisionControllerElement : MonoBehaviour
    {
        public delegate void CollisionDelegate(Collider other);
        
        public event CollisionDelegate OnCollisionEnterDelegate;
        public event CollisionDelegate OnTriggerExitDelegate;    
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast") || other.gameObject.layer == LayerMask.NameToLayer("Zones") || other.gameObject.layer == LayerMask.NameToLayer("VRControllers"))
            {
                return;
            }
            OnCollisionEnterDelegate?.Invoke(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast") || other.gameObject.layer == LayerMask.NameToLayer("Zones") || other.gameObject.layer == LayerMask.NameToLayer("VRControllers"))
            {
                return;
            }
            
            OnCollisionEnterDelegate?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast") || other.gameObject.layer == LayerMask.NameToLayer("Zones") || other.gameObject.layer == LayerMask.NameToLayer("VRControllers"))
            {
                return;
            }
            
            OnTriggerExitDelegate?.Invoke(other);
        }
    
    }
}