using Varwin.VRInput;

namespace Varwin
{
    public class ObjectPointerBehaviour : PointableObject
    {
        private PointerAction _pointerAction;

        public void Init(PointerAction pointerAction)
        {
            _pointerAction = pointerAction;
        }

        public override void OnPointerIn()
        {
            _pointerAction.OnPointerIn?.Invoke();
        }

        public override void OnPointerOut()
        {
            _pointerAction.OnPointerOut?.Invoke();
        }

        public override void OnPointerClick()
        {
            _pointerAction.OnPointerClick?.Invoke();
        }
    }
}
