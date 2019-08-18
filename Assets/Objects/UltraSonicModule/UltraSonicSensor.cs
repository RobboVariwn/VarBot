using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltraSonicSensor : VirtualModule.VirtualMagnetModule
{
    public GameObject LeftEmitter;
    public GameObject RightEmitter;

    public override void OnConnected() { }

    public override void OnDisconnected() { }

    private float Raycast(GameObject emitter)
    {
        RaycastHit hit;

        if(Physics.Raycast(emitter.transform.position, emitter.transform.TransformDirection(Vector3.forward), out hit, 1000F, Physics.AllLayers))
        {
            return Vector3.Distance(hit.point, emitter.transform.position);
        }

        return -1F;
    }

    public override T Read<T>()
    {
        var leftDistance = Raycast(LeftEmitter);
        var rightDistance = Raycast(RightEmitter);

        if (leftDistance < 0 || rightDistance < 0)
        {
            return (T)(object)(-1F);
        } else
        {
            return (T)(object)((leftDistance + rightDistance) / 2F);
        }
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
