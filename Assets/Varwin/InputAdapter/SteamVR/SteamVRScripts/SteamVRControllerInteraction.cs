using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Varwin.VRInput;

public class SteamVRControllerInteraction : ControllerInteractionComponent
{
    private Hand _hand;

    private void Start()
    {
        _hand = GetComponent<Hand>();

        switch (_hand.handType)
        {
            case SteamVR_Input_Sources.LeftHand:
                InputAdapter.Instance.PlayerController.Nodes.LeftHand.SetNode(gameObject);

                break;
            case SteamVR_Input_Sources.RightHand:
                InputAdapter.Instance.PlayerController.Nodes.RightHand.SetNode(gameObject);

                break;
        }
    }
}