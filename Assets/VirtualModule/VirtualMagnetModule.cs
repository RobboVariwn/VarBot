using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin;
using Varwin.Public;

namespace VirtualModule
{
    public abstract class VirtualMagnetModule : MonoBehaviour
    {
        [HideInInspector]
        public VirtualMagnetPlate ConnectedTo;

        private VirtualMagnetPlate hoveredPlate;
               
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        

        public void OnGrabStart()
        {
            ConnectedTo?.Disconnect();
            OnDisconnected();
            ConnectedTo = null;
            hoveredPlate = null;

            foreach(var collider in this.GetComponents<Collider>())
            {
                collider.isTrigger = true;
            }
        }

        public void OnGrabEnd()
        {
            if (hoveredPlate != null && hoveredPlate.IsAvailable())
            {
                ConnectedTo = hoveredPlate;
                ConnectedTo.Connect(this);
                hoveredPlate = null;
                OnConnected();
            }

            foreach (var collider in this.GetComponents<Collider>())
            {
                collider.isTrigger = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var probablyPlate = other.GetComponent<VirtualMagnetPlate>();

            if (probablyPlate != null)
            {
                hoveredPlate = probablyPlate;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (hoveredPlate != null && hoveredPlate.gameObject == other.gameObject)
            {
                hoveredPlate = null;
            }
        }

        public abstract T Read<T>();

        public abstract void Write<T>(T data);

        public abstract void OnConnected();

        public abstract void OnDisconnected();
    }
}
