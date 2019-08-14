namespace Varwin.VRInput
{
    public class VRTKPlayerAppearance : PlayerAppearance
    {
        public VRTKPlayerAppearance()
        {
            ControllerAppearance = new ComponentWrapFactory<InteractControllerAppearance,
                VRTKInteractControllerAppearance, SteamVRInteractControllerAppearance>();
        }


        private class VRTKInteractControllerAppearance : InteractControllerAppearance,
            IInitializable<SteamVRInteractControllerAppearance>
        {
            private SteamVRInteractControllerAppearance _objectAppearance;

            public override bool HideControllerOnGrab
            {
                set => _objectAppearance.hideControllerOnGrab = value;
            }

            public void Init(SteamVRInteractControllerAppearance monoBehaviour)
            {
                _objectAppearance = monoBehaviour;
            }
        }
    }
}