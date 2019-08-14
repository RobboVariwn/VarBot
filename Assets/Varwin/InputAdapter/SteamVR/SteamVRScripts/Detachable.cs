using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Detachable : Throwable
{
    public bool CanBeDetached = true;

    public bool IsGrabbable = true;
    
    protected override void HandAttachedUpdate(Hand hand)
    {
        if (CanBeDetached)
        {
            base.HandAttachedUpdate(hand);
        }
    }

    protected override void HandHoverUpdate(Hand hand)
    {
        if (IsGrabbable)
        {
            base.HandHoverUpdate(hand);
        }
    }
}
