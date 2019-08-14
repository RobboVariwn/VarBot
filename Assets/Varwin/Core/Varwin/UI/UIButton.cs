using UnityEngine;
using Varwin.VRInput;

namespace Varwin.UI
{
    // ReSharper disable once InconsistentNaming
    public class UIButton : PointableObject
    {
        public GameObject OnHoverGo;

        public virtual void OnHover()
        {
            if (OnHoverGo != null)
            {
                OnHoverGo.SetActive(true);
            }
        }

        public virtual void OnOut()
        {
            if (OnHoverGo != null)
            {
                OnHoverGo.SetActive(false);
            }
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
            OnClick();
        }
    }
}
