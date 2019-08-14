using UnityEngine;
using Valve.VR.InteractionSystem;
using Varwin.VRInput;


public class SteamVRHapticsInteraction : MonoBehaviour
{
    private Hand _hand;
    private ControllerInput.ControllerEvents _events;
    
    public float StrengthOnUse { get; set; }
    public float IntervalOnUse { get; set; }
    public float DurationOnUse { get; set; }
    public float StrengthOnTouch { get; set; }
    public float IntervalOnTouch { get; set; }
    public float DurationOnTouch { get; set; }
    public float StrengthOnGrab { get; set; }
    public float IntervalOnGrab { get; set; }
    public float DurationOnGrab { get; set; }

    private void Start()
    {
        
    }

    protected virtual void OnHandHoverBegin(Hand hand)
    {
        
    }

    protected virtual void OnAttachedToHand(Hand hand)
    {
        
    }

    protected virtual void OnUse(Hand hand)
    {
        
    }

}
