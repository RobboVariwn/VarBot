using UnityEngine;

namespace Varwin.VRInput
{
    public abstract class ObjectInteraction
    {
        public IMonoComponent<InteractHaptics> Haptics;
        public IMonoComponent<InteractObject> Object;

        public abstract class InteractObject
        {
            public abstract bool isGrabbable { set; }
            public abstract bool isUsable { set; }
            public abstract ValidDropTypes validDrop { set; }
            public abstract ControllerInput.ButtonAlias useOverrideButton { set; }
            public abstract ControllerInput.ButtonAlias grabOverrideButton { set; }
            public abstract bool SwapControllersFlag { get; set; }

            public enum ValidDropTypes
            {
                /// <summary>
                /// The object cannot be dropped via the controller.
                /// </summary>
                NoDrop,

                /// <summary>
                /// The object can be dropped anywhere in the scene via the controller.
                /// </summary>
                DropAnywhere,
            }


            public delegate void InteractableObjectEventHandler(object sender, InteractableObjectEventArgs e);

            public abstract event InteractableObjectEventHandler InteractableObjectGrabbed;
            public abstract event InteractableObjectEventHandler InteractableObjectUngrabbed;

            public abstract event InteractableObjectEventHandler InteractableObjectTouched;
            public abstract event InteractableObjectEventHandler InteractableObjectUntouched;

            public abstract event InteractableObjectEventHandler InteractableObjectUsed;
            public abstract event InteractableObjectEventHandler InteractableObjectUnused;


            public abstract GameObject GetGrabbingObject();

            public abstract void ForceStopInteracting();

            public abstract bool IsGrabbed();

            public abstract bool IsUsing();

            public abstract GameObject GetUsingObject();
        }

        public struct InteractableObjectEventArgs
        {
            public GameObject interactingObject;
            public ControllerInteraction.ControllerHand Hand; 
        }


        public abstract class InteractHaptics
        {
            public abstract float StrengthOnUse { set; }
            public abstract float IntervalOnUse { set; }
            public abstract float DurationOnUse { set; }
            public abstract float StrengthOnTouch { set; }
            public abstract float IntervalOnTouch { set; }
            public abstract float DurationOnTouch { set; }
            public abstract float StrengthOnGrab { set; }
            public abstract float IntervalOnGrab { set; }
            public abstract float DurationOnGrab { set; }
        }
    }
}
