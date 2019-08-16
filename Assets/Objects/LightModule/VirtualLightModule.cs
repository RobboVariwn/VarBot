using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualLightModule : VirtualModule.VirtualMagnetModule
{
    private new Light light;

    // Start is called before the first frame update
    void Start()
    {
        this.light = GetComponentInChildren<Light>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override T Read<T>()
    {
        throw new System.NotImplementedException();
    }

    public override void Write<T>(T data)
    {
        light.enabled = (bool)(object)data;
    }

    public override void OnConnected()
    {
        if(this.ConnectedTo.LastValue != null)
        {
            this.light.enabled = (bool)this.ConnectedTo.LastValue;
        }
    }

    public override void OnDisconnected()
    {
        this.light.enabled = false;
    }
}
