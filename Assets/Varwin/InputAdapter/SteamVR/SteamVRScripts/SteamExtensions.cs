using Valve.VR;

namespace Varwin.VRInput.SteamVR
{
    public static class SteamExtensions
    {
        public static ControllerInteraction.ControllerHand SteamSourceToHand(
            this SteamVR_Input_Sources sources)
        {
            switch (sources)
            {
                case SteamVR_Input_Sources.LeftHand: return ControllerInteraction.ControllerHand.Left;

                case SteamVR_Input_Sources.RightHand: return ControllerInteraction.ControllerHand.Right;
                default: return ControllerInteraction.ControllerHand.None;
            }
        }
    }
}
