using UnityEngine;

namespace Varwin.VRInput
{
    public abstract class PointableObject : MonoBehaviour
    {
        public abstract void OnPointerIn();
        
        public abstract void OnPointerOut();
        
        public abstract void OnPointerClick();

        public void Awake()
        {
            gameObject.layer = 5;
        }
    }
}
