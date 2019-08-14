using UnityEngine;

namespace Varwin.VRInput
{
    public abstract class ControllerInteraction
    {
        public ControllerHaptics Haptics;
        public IMonoComponent<ControllerSelf> Controller;

        public abstract class ControllerSelf
        {
            public ControllerInteractionComponent Controller;

            public abstract void TriggerHapticPulse(
                float strength,
                float duration,
                float interval);

            public abstract GameObject GetGrabbedObject();

            public abstract void AddColliders(Collider[] colliders);

            public abstract bool CheckIfColliderPresent(Collider coll);
        }

        public enum ControllerHand
        {
            /// <summary>
            /// No hand is assigned.
            /// </summary>
            None,

            /// <summary>
            /// The left hand is assigned.
            /// </summary>
            Left,

            /// <summary>
            /// The right hand is assigned.
            /// </summary>
            Right
        }

        public enum ControllerElements
        {
            /// <summary>
            /// The default point on the controller to attach grabbed objects to.
            /// </summary>
            AttachPoint,

            /// <summary>
            /// The trigger button.
            /// </summary>
            Trigger,

            /// <summary>
            /// The left part of the grip button collection.
            /// </summary>
            GripLeft,

            /// <summary>
            /// The right part of the grip button collection.
            /// </summary>
            GripRight,

            /// <summary>
            /// The touch pad/stick.
            /// </summary>
            Touchpad,

            /// <summary>
            /// The first generic button.
            /// </summary>
            ButtonOne,

            /// <summary>
            /// The second generic button.
            /// </summary>
            ButtonTwo,

            /// <summary>
            /// The system menu button.
            /// </summary>
            SystemMenu,

            /// <summary>
            /// The encompassing mesh of the controller body.
            /// </summary>
            Body,

            /// <summary>
            /// The start menu button.
            /// </summary>
            StartMenu,

            /// <summary>
            /// The touch pad/stick two.
            /// </summary>
            TouchpadTwo
        }
 
        public abstract class ControllerFixedGrab
        {
            public abstract float breakForce { set; }
            public abstract bool precisionGrab { set; }
            public abstract Transform leftSnapHandle { set; }
            public abstract Transform rightSnapHandle { set; }
        }

        public abstract class ControllerHaptics
        {
            public abstract void TriggerHapticPulse(
                PlayerController.PlayerNodes.ControllerNode playerController,
                float strength,
                float duration,
                float interval);
        }
    }
}
