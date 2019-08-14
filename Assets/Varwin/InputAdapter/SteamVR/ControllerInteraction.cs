using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace Varwin.VRInput
{
    public class SteamVRControllerInteraction : ControllerInteraction
    {
        public SteamVRControllerInteraction()
        {
            Controller = new ComponentWrapFactory<ControllerSelf, SteamVRController, Hand>();
            Haptics = new SteamVRControllerHaptics();
        }

        private class SteamVRController : ControllerSelf, IInitializable<Hand>
        {
            private Hand _hand;

            private HashSet<Collider> _colliders;

            public override void TriggerHapticPulse(float strength, float duration, float interval)
            {
                _hand.TriggerHapticPulse(duration, interval, strength);
            }

            public override GameObject GetGrabbedObject()
            {
                try
                {
                    return _hand.AttachedObjects[0].attachedObject;
                }
                catch
                {
                    // ignored
                }

                return null;
            }

            public override void AddColliders(Collider[] colliders)
            {
                if (colliders == null)
                {
                    _colliders = null;
                }
                else
                {
                    _colliders = new HashSet<Collider>(colliders);
                }
            }

            public override bool CheckIfColliderPresent(Collider coll)
            {
                if (_colliders == null)
                {
                    return false;
                }

                return _colliders.Contains(coll);
            }

            public void Init(Hand monoBehaviour)
            {
                _hand = monoBehaviour;
            }
        }

        private class SteamVRControllerHaptics : ControllerHaptics
        {
            public override void TriggerHapticPulse(
                PlayerController.PlayerNodes.ControllerNode playerController,
                float strength,
                float duration,
                float interval)
            {
                playerController.Controller.TriggerHapticPulse(strength, duration, interval);
            }
        }

        public class SteamVRControllerInteractionComponent : ControllerInteractionComponent
        {
        }
    }

    public static class ControllerInteractionEx
    {
        public static ControllerInteraction.ControllerHand GetHand(this SteamVR_Input_Sources self)
        {
            switch (self)
            {
                case SteamVR_Input_Sources.RightHand: return ControllerInteraction.ControllerHand.Right;

                case SteamVR_Input_Sources.LeftHand: return ControllerInteraction.ControllerHand.Left;

                default: return ControllerInteraction.ControllerHand.None;
            }
        }
    }
}
