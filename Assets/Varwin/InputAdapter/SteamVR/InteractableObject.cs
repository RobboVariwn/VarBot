using UnityEngine;

namespace Varwin.VRInput
{
    public class SteamVRObjectInteraction : ObjectInteraction
    {
        public SteamVRObjectInteraction()
        {
            Object = new ComponentWrapFactory<InteractObject, SteamVRInteractObject, SteamVRInteractableObject>();

            Haptics = new ComponentWrapFactory<InteractHaptics, SteamVRInteractHaptics, SteamVRHapticsInteraction>();
        }

        private class SteamVRInteractObject : InteractObject, IInitializable<SteamVRInteractableObject>
        {
            private SteamVRInteractableObject _interactableObject;

            public override bool isGrabbable
            {
                set => _interactableObject.IsGrabbable = value;
            }

            public override bool isUsable
            {
                set => _interactableObject.IsUsable = value;
            }

            public override ValidDropTypes validDrop
            {
                set => _interactableObject.validDrop = value;
            }

            public override ControllerInput.ButtonAlias useOverrideButton
            {
                set => _interactableObject.useOverrideButton = value;
            }

            public override ControllerInput.ButtonAlias grabOverrideButton
            {
                set => _interactableObject.grabOverrideButton = value;
            }

            public override bool SwapControllersFlag
            {
                get => _interactableObject.SwapHandsFlag;
                set => _interactableObject.SwapHandsFlag = value;
            }

            public override event InteractableObjectEventHandler InteractableObjectGrabbed;
            public override event InteractableObjectEventHandler InteractableObjectUngrabbed;
            public override event InteractableObjectEventHandler InteractableObjectTouched;
            public override event InteractableObjectEventHandler InteractableObjectUntouched;
            public override event InteractableObjectEventHandler InteractableObjectUsed;
            public override event InteractableObjectEventHandler InteractableObjectUnused;

            public override GameObject GetGrabbingObject() => _interactableObject.GetGrabbingObject();

            public override void ForceStopInteracting()
            {
                _interactableObject.ForceStopInteracting();
            }

            public override bool IsGrabbed() => _interactableObject.IsGrabbed();

            public override bool IsUsing() => _interactableObject.IsUsing();

            public override GameObject GetUsingObject() => _interactableObject.GetUsingObject();

            public void Init(SteamVRInteractableObject monoBehaviour)
            {
                _interactableObject = monoBehaviour;

                _interactableObject.InteractableObjectGrabbed += (sender, args) =>
                {
                    InteractableObjectGrabbed?.Invoke(sender, args);
                };

                _interactableObject.InteractableObjectUngrabbed += (sender, args) =>
                {
                    InteractableObjectUngrabbed?.Invoke(sender, args);
                };

                _interactableObject.InteractableObjectTouched += (sender, args) =>
                {
                    InteractableObjectTouched?.Invoke(sender, args);
                };

                _interactableObject.InteractableObjectUntouched += (sender, args) =>
                {
                    InteractableObjectUntouched?.Invoke(sender, args);
                };

                _interactableObject.InteractableObjectUsed += (sender, args) =>
                {
                    InteractableObjectUsed?.Invoke(sender, args);
                };

                _interactableObject.InteractableObjectUnused += (sender, args) =>
                {
                    InteractableObjectUnused?.Invoke(sender, args);
                };
            }
        }

        private class SteamVRInteractHaptics : InteractHaptics, IInitializable<SteamVRHapticsInteraction>
        {
            private SteamVRHapticsInteraction _hapticsInteraction;

            public override float StrengthOnUse
            {
                set => _hapticsInteraction.StrengthOnUse = value;
            }

            public override float IntervalOnUse
            {
                set => _hapticsInteraction.IntervalOnUse = value;
            }

            public override float DurationOnUse
            {
                set => _hapticsInteraction.DurationOnUse = value;
            }

            public override float StrengthOnTouch
            {
                set => _hapticsInteraction.StrengthOnTouch = value;
            }

            public override float IntervalOnTouch
            {
                set => _hapticsInteraction.IntervalOnTouch = value;
            }

            public override float DurationOnTouch
            {
                set => _hapticsInteraction.DurationOnTouch = value;
            }

            public override float StrengthOnGrab
            {
                set => _hapticsInteraction.StrengthOnGrab = value;
            }

            public override float IntervalOnGrab
            {
                set => _hapticsInteraction.IntervalOnGrab = value;
            }

            public override float DurationOnGrab
            {
                set => _hapticsInteraction.DurationOnGrab = value;
            }

            public void Init(SteamVRHapticsInteraction monoBehaviour)
            {
                _hapticsInteraction = monoBehaviour;
            }
        }
    }
}