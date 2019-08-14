using UnityEngine;

namespace Varwin.Public
{
    public interface IGrabStartAware
    {
        void OnGrabStart(GrabingContext context);
    }

    public interface IGrabEndAware
    {
        void OnGrabEnd();
    }

    public interface IGrabPointAware
    {
        Transform GetLeftGrabPoint();
        Transform GetRightGrabPoint();
    }
}
