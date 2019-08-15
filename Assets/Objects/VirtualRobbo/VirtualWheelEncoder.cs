using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualWheelEncoder : MonoBehaviour
{
    public int Steps;
    
    private float previousStepAngle;

    void Start()
    {
        previousStepAngle = transform.localEulerAngles.z;
    }

    void Update()
    {
        var currentAngle = transform.localEulerAngles.z;

        if (currentAngle % 15 == 0 && currentAngle != previousStepAngle)
        {
            previousStepAngle = currentAngle;
            Steps++;
        }
    }
}
