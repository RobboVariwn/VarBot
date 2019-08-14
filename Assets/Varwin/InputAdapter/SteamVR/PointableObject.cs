using UnityEngine.Events;

namespace Varwin.VRInput
{
    public class SteamVRPointableObject : PointableObject
    {
        public UnityEvent OnHover;
        public UnityEvent OnOut;
        public UnityEvent OnClick;
        public override void OnPointerIn()
        {
            OnHover?.Invoke();
        }

        public override void OnPointerOut()
        {
             OnOut?.Invoke();
        }

        public override void OnPointerClick()
        {
             OnClick?.Invoke();
        }
    }
}
