namespace Varwin.VRInput
{
    public abstract class PlayerAppearance
    {
        public IMonoComponent<InteractControllerAppearance> ControllerAppearance;

        public abstract class InteractControllerAppearance
        {
            public abstract bool HideControllerOnGrab { set; }
        }
    }
}