using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualModule;

namespace VirtualModule
{
    public class VirtualMagnetPlate : MonoBehaviour
    {
        private VirtualMagnetModule connectedModule;
        private Rigidbody parent;

        public object LastValue;

        // Start is called before the first frame update
        void Start()
        {
            parent = GetComponentInParent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public bool IsAvailable()
        {
            return connectedModule == null;
        }

        public void Disconnect()
        {
            connectedModule.transform.parent = null;
            Destroy(connectedModule.GetComponent<FixedJoint>());
            connectedModule = null;
        }

        public void Connect(VirtualMagnetModule module)
        {
            connectedModule = module;
            connectedModule.transform.parent = this.transform;
            connectedModule.transform.position = this.transform.position;
            connectedModule.transform.rotation = this.transform.rotation;
            (connectedModule.gameObject.AddComponent(typeof(FixedJoint)) as FixedJoint).connectedBody = parent;
        }

        public Nullable<T> Read<T>() where T : struct
        {
            return connectedModule?.Read<T>();
        }

        public void Write<T>(T data)
        {
            LastValue = (object)data;
            connectedModule?.Write(data);
        }
    }
}
