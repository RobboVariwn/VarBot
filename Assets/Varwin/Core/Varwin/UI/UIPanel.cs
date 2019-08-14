using Varwin.VRInput;

namespace Varwin.UI
{
    using UnityEngine;

    namespace Varwin.UI
    {
        // ReSharper disable once InconsistentNaming
        public class UIPanel : PointableObject
        {
            public virtual void OnHover()
            {
            }

            public virtual void OnOut()
            {
            }

            public virtual void OnClick()
            {
            }

            public override void OnPointerIn()
            {
                OnHover();
            }

            public override void OnPointerOut()
            {
                OnOut();
            }

            public override void OnPointerClick()
            {
            }
        }
    }
}