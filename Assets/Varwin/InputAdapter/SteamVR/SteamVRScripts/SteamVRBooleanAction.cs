using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SteamVRBooleanAction : SteamVR_Behaviour_Boolean
{
    protected override void OnEnable()
    {
        if (booleanAction != null)
        {
            base.OnEnable();
        }
    }

    public void Init()
    {
        base.OnEnable();
    }
}
