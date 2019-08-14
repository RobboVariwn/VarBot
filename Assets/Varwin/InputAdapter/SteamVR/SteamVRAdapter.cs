namespace Varwin.VRInput
{
    public class SteamVRAdapter : SDKAdapter
    {
        public SteamVRAdapter() : base(new VRTKPlayerAppearance(),
            new SteamVRControllerInput(),
            new SteamVRObjectInteraction(),
            new SteamVRControllerInteraction(),
            new SteamVRPlayerController(),
            new SteamVRPointerController(),
            new ComponentFactory<PointableObject, SteamVRPointableObject>(),
            new ComponentWrapFactory<Tooltip, UniversalTooltip, TooltipObject>())
        {
        }
    }
}