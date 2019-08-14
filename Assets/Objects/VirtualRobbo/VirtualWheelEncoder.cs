using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualWheelEncoder : MonoBehaviour
{
    public int Steps;

    private float previousStepAngle;

    void Start()
    {
        Steps = 0;
        previousStepAngle = transform.localEulerAngles.z;
    }

    void Update()
    {
        float currentAngle = transform.localEulerAngles.z;

        if (Mathf.Abs(Mathf.DeltaAngle(previousStepAngle, currentAngle)) > 14.99f)
        {
            previousStepAngle = currentAngle;
            Steps++;
        }
    }
}
