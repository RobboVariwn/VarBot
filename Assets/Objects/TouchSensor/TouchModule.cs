using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchModule : VirtualModule.VirtualMagnetModule
{
    public const float MAX_DISTANCE = 1F;

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

        return 1F;
    }

    public override T Read<T>()
    {
        var distance = Raycast(Emitter);

        if (distance <= 0.01F)
            return (T)(object)100F;
        else
            return (T)(object)0F;
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
