using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceModule : VirtualModule.VirtualMagnetModule
{
    public const float MAX_DISTANCE = 1000F;
    
    public GameObject Emitter;

    public override void OnConnected() { }

    public override void OnDisconnected() { }

    private float Raycast(GameObject emitter)
    {
        RaycastHit hit;

        if (Physics.Raycast(emitter.transform.position, emitter.transform.TransformDirection(Vector3.forward), out hit, MAX_DISTANCE, Physics.AllLayers))
        {
            return Vector3.Distance(hit.point, emitter.transform.position);
        }

        return -1F;
    }

    public override T Read<T>()
    {
        var distance = Raycast(Emitter);

        if (distance <= 0F)
            return (T)(object)-1F;

        return (T)(object)(6 / distance);
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
