using System.Collections.Generic;
using UnityEngine;

namespace Varwin.VRInput
{
    public class SteamVRControllerInput : ControllerInput
    {
        public override event ControllerInteractionEventHandler ControllerEnabled;
        public override event ControllerInteractionEventHandler ControllerDisabled;
        public override event ControllerInteractionEventHandler TriggerPressed;
        public override event ControllerInteractionEventHandler TriggerReleased;
        public override event ControllerInteractionEventHandler TouchpadReleased;
        public override event ControllerInteractionEventHandler TouchpadPressed;
        public override event ControllerInteractionEventHandler ButtonTwoPressed;
        public override event ControllerInteractionEventHandler ButtonTwoReleased;
        public override event ControllerInteractionEventHandler GripPressed;
        public event ControllerInteractionEventHandler TurnLeftPressed;
        public event ControllerInteractionEventHandler TurnRightPressed;

        private List<GameObject> _controllerEvents = new List<GameObject>();

        public override void AddController(ControllerEvents events)
        {
            if (_controllerEvents.Contains(events.gameObject))
            {
                return;
            }

            _controllerEvents.Add(events.gameObject);

            events.ControllerEnabled += (sender, args) => { ControllerEnabled?.Invoke(sender, args); };

            events.ControllerDisabled += (sender, args) => { ControllerDisabled?.Invoke(sender, args); };

            events.TriggerReleased += (sender, args) => { TriggerReleased?.Invoke(sender, args); };
            
            events.TriggerPressed += (sender, args) => { TriggerPressed?.Invoke(sender, args); };

            events.TouchpadReleased += (sender, args) => { TouchpadReleased?.Invoke(sender, args); };

            events.TouchpadPressed += (sender, args) => { TouchpadPressed?.Invoke(sender, args); };

            events.ButtonTwoPressed += (sender, args) => { ButtonTwoPressed?.Invoke(sender, args); };

            events.ButtonTwoReleased += (sender, args) => { ButtonTwoReleased?.Invoke(sender, args); };

            events.GripPressed += (sender, args) => { GripPressed?.Invoke(sender, args); };

            ((SteamVRControllerEvents) events).TurnLeftPressed += (sender, args) =>
            {
                TurnLeftPressed?.Invoke(sender, args);
            };

            ((SteamVRControllerEvents) events).TurnRightPressed += (sender, args) =>
            {
                TurnRightPressed?.Invoke(sender, args);
            };
        }

        public SteamVRControllerInput()
        {
            ControllerEventFactory =
                new ComponentWrapFactory<ControllerEvents, SteamVRControllerEvents, SteamVRControllerEventComponent>();
        }

        private class SteamVRControllerEvents : ControllerEvents, IInitializable<SteamVRControllerEventComponent>
        {
            private SteamVRControllerEventComponent _eventComponent = null;
            public override GameObject gameObject => _eventComponent.gameObject;

            public override Transform transform => _eventComponent.transform;
            public override event ControllerInteractionEventHandler ControllerEnabled;
            public override event ControllerInteractionEventHandler ControllerDisabled;
            public override event ControllerInteractionEventHandler TriggerPressed;
            public override event ControllerInteractionEventHandler TriggerReleased;
            public override event ControllerInteractionEventHandler TouchpadReleased;
            public override event ControllerInteractionEventHandler TouchpadPressed;
            public override event ControllerInteractionEventHandler ButtonTwoPressed;
            public override event ControllerInteractionEventHandler ButtonTwoReleased;
            public override event ControllerInteractionEventHandler GripPressed;

            public event ControllerInteractionEventHandler TurnLeftPressed;
            public event ControllerInteractionEventHandler TurnRightPressed;

            public override bool IsTouchpadPressed() => _eventComponent.IsTouchpadPressed();
            public override bool IsTouchpadReleased() => _eventComponent.IsTouchpadReleased();


            public override bool IsTriggerPressed() => _eventComponent.IsTriggerPressed();
            public override bool IsTriggerReleased() => _eventComponent.IsTriggerReleased();

            public override bool IsButtonPressed(ButtonAlias gripPress) => _eventComponent.IsButtonPressed(gripPress);

            public override void OnGripReleased(ControllerInteractionEventArgs controllerInteractionEventArgs)
            {
                _eventComponent.OnGripReleased(controllerInteractionEventArgs);
            }

            public override GameObject GetController() => _eventComponent.GetController().gameObject;

            public void Init(SteamVRControllerEventComponent monoBehaviour)
            {
                _eventComponent = monoBehaviour;

                _eventComponent.ControllerEnabled += (sender, args) => { ControllerEnabled?.Invoke(sender, args); };

                _eventComponent.ControllerDisabled += (sender, args) => { ControllerDisabled?.Invoke(sender, args); };

                _eventComponent.TriggerPressed += (sender, args) => { TriggerPressed?.Invoke(sender, args); };
                
                _eventComponent.TriggerReleased += (sender, args) => { TriggerReleased?.Invoke(sender, args); };

                _eventComponent.TouchpadReleased += (sender, args) => { TouchpadReleased?.Invoke(sender, args); };

                _eventComponent.TouchpadPressed += (sender, args) => { TouchpadPressed?.Invoke(sender, args); };

                _eventComponent.ButtonTwoPressed += (sender, args) => { ButtonTwoPressed?.Invoke(sender, args); };

                _eventComponent.ButtonTwoReleased += (sender, args) => { ButtonTwoReleased?.Invoke(sender, args); };

                _eventComponent.GripPressed += (sender, args) => { GripPressed?.Invoke(sender, args); };

                _eventComponent.TurnLeftPressed += (sender, args) => { TurnLeftPressed?.Invoke(sender, args); };

                _eventComponent.TurnRightPressed += (sender, args) => { TurnRightPressed?.Invoke(sender, args); };

                InputAdapter.Instance.ControllerInput.AddController(this);
            }
        }
    }
}
