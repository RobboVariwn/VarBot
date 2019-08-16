using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltraSonicSensor : VirtualModule.VirtualMagnetModule
{
    public override void OnConnected() { }

    public override void OnDisconnected() { }

    public override T Read<T>()
    {
        throw new System.NotImplementedException();
    }

    public override void Write<T>(T data)
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
