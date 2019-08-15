using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualWheelEncoder : MonoBehaviour
{
    public int Steps;
    
    private float previousStepAngle;
    public float traveledAngles;

    void Start()
    {
        previousStepAngle = transform.localEulerAngles.z;
    }

    public void Reset()
    {
        Steps = 0;
        previousStepAngle = transform.localEulerAngles.z;
        traveledAngles = 0;
    }

    public void CalculateSteps()
    {
        var currentAngle = transform.localEulerAngles.z;
        traveledAngles += Mathf.Abs(Mathf.DeltaAngle(previousStepAngle, currentAngle));
        previousStepAngle = currentAngle;

        Steps = Mathf.RoundToInt(traveledAngles / 15F);
    }

    private void Update()
    {
        CalculateSteps();
    }

    void LateUpdate()
    {
        CalculateSteps();
    }
}
