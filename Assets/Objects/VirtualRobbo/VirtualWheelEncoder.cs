using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualWheelEncoder : MonoBehaviour
{
    public int Steps;
    

    private float previousStepAngle;
    private float realSteps;

    void Start()
    {
        Steps = 0;
        previousStepAngle = transform.localEulerAngles.z;
    }

    void Update()
    {
        float currentAngle = transform.localEulerAngles.z;

        if (Mathf.Abs(Mathf.DeltaAngle(previousStepAngle, currentAngle)) >= 1F)
        {
            realSteps += Mathf.Abs(Mathf.DeltaAngle(previousStepAngle, currentAngle)) / 15F;
            previousStepAngle = currentAngle;
            Steps = (int)realSteps;
        }
    }
}
