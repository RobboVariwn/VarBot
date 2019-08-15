using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Varwin;
using Varwin.Public;

public class VirtualStartButton : MonoBehaviour, IUseStartAware, ITouchStartAware, IUseEndAware, ITouchEndAware
{
    public bool StartButtonPressed;

    private Vector3 targetScale;
    private Vector3 targetPosition;

    private Vector3 normalScale;
    private Vector3 hoveredScale;

    private Vector3 normalPositon;
    private Vector3 pressedPosition;

    void Start()
    {
        StartButtonPressed = false;

        normalScale = transform.localScale;
        hoveredScale = new Vector3(normalScale.x * 1.2f, normalScale.y, normalScale.z * 1.2F);
        targetScale = normalScale;

        normalPositon = transform.localPosition;
        pressedPosition = new Vector3(normalPositon.x, normalPositon.y - 0.006F, normalPositon.z);
        targetPosition = normalPositon;
    }

    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 1.7F);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 1.7F);
    }

    public void OnTouchStart()
    {
        targetScale = hoveredScale;
    }

    public void OnTouchEnd()
    {
        targetScale = normalScale;
    }

    public void OnUseStart(UsingContext context)
    {
        targetPosition = pressedPosition;
        StartButtonPressed = true;
    }

    public void OnUseEnd()
    {
        targetPosition = normalPositon;
        StartButtonPressed = false;
    }
}
