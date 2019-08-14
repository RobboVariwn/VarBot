using System.Collections;
using System.Collections.Generic;
using HighlightPlus;
using UnityEngine;
using Varwin.VRInput;

namespace Varwin.UI
{
    public class ControllerTooltipManager : MonoBehaviour
    {
        public enum TooltipButtons
        {
            Trigger,
            Grip,
            Touchpad,
            ButtonOne,
            ButtonTwo,
            StartMenu
        }

        public enum TooltipControllers
        {
            Left,
            Right,
            Both
        }

        public ControllerInput.ControllerEvents LeftControllerEvents, RightControllerEvents;
        private TooltipTransformAdapterBase _leftTooltipAdapter, _rightTooltipAdapter;

        public GameObject LeftController, RightController;

        public GameObject TooltipTemplate;

        private bool _leftControllerReady, _rightControllerReady;

        private bool _showTooltip = true;

        private Dictionary<TooltipButtons, GameObject> _tooltipObjectsLeft, _tooltipObjectsRight;

        
        void Start()
        {
            LeftControllerEvents = InputAdapter.Instance.ControllerInput.ControllerEventFactory.GetFrom(LeftController);
            RightControllerEvents = InputAdapter.Instance.ControllerInput.ControllerEventFactory.GetFrom(RightController);

            if (LeftControllerEvents == null || RightController == null)
            {
                Debug.LogError	("Missing ControllerEvents!");
                return;
            }

            _leftTooltipAdapter = LeftController.GetComponent<TooltipTransformAdapterBase>();
            _rightTooltipAdapter = RightController.GetComponent<TooltipTransformAdapterBase>();
            
            if (_leftTooltipAdapter == null || _rightTooltipAdapter == null)
            {
                Debug.LogError	("Missing TooltipTransformAdapterBase!");
                return;
            }
            
            _leftTooltipAdapter.Init(TooltipControllers.Left);
            _rightTooltipAdapter.Init(TooltipControllers.Right);

            StartCoroutine(ControllerEventsSubscriptionSafe());

            _tooltipObjectsLeft = new Dictionary<TooltipButtons, GameObject>();
            _tooltipObjectsRight = new Dictionary<TooltipButtons, GameObject>();

        }

        public void SetTooltip(
            string text,
            TooltipControllers controller,
            TooltipButtons button,
            bool buttonGlow,
            bool vibration = true)
        {
            StartCoroutine(TooltipCoroutine(text,
                controller,
                button,
                buttonGlow,
                vibration));
        }

        public void HideTooltip(TooltipControllers controller, TooltipButtons button)
        {
            SetTooltip("",
                controller,
                button,
                false);
        }

        private IEnumerator TooltipCoroutine(
            string text,
            TooltipControllers controller,
            TooltipButtons button,
            bool buttonGlow,
            bool vibration = true)
        {
            while (!UpdateTooltip(text,
                controller,
                button,
                buttonGlow,
                vibration))
            {
                yield return new WaitForEndOfFrame();
            }
        }

        private bool UpdateTooltip(
            string text,
            TooltipControllers controller,
            TooltipButtons button,
            bool buttonGlow,
            bool vibration = true)
        {
            if ((controller == TooltipControllers.Left && !_leftControllerReady)
                || (controller == TooltipControllers.Right && !_rightControllerReady))
            {
                return false;
            }

            GameObject tooltipObject;
            Tooltip tooltip;

            Transform buttonAttachTransform = null;
            HighlightEffect highlight = null;


            if ((controller == TooltipControllers.Left && !_tooltipObjectsLeft.ContainsKey(button))
                || (controller == TooltipControllers.Right && !_tooltipObjectsRight.ContainsKey(button)))
            {
                buttonAttachTransform = GetButtonTransform(controller, button);
                
                if (buttonAttachTransform == null)
                {
                    return false;
                }

                tooltipObject = Instantiate(TooltipTemplate, GetTooltipInstantiationTransform(controller, button));

                if (controller == TooltipControllers.Left)
                {
                    _tooltipObjectsLeft.Add(button, tooltipObject);
                }
                else
                {
                    _tooltipObjectsRight.Add(button, tooltipObject);
                }

                tooltip = InputAdapter.Instance.Tooltip.GetFromChildren(tooltipObject, true);

                tooltip.drawLineTo = buttonAttachTransform;

                buttonAttachTransform.parent.gameObject.AddComponent<HighlightEffect>();

                highlight = buttonAttachTransform.GetComponentInParent<HighlightEffect>();

                SetupHighlighter(ref highlight);

                highlight.enabled = false;
            }
            else
            {
                if (controller == TooltipControllers.Left)
                {
                    tooltipObject = _tooltipObjectsLeft[button];
                }
                else
                {
                    tooltipObject = _tooltipObjectsRight[button];
                }

                tooltip = InputAdapter.Instance.Tooltip.GetFromChildren(tooltipObject, true);
            }

            tooltip.displayText = text;

            tooltip.ResetTooltip();

            if (highlight == null)
            {
                buttonAttachTransform = GetButtonTransform(controller, button);
                highlight = buttonAttachTransform.GetComponentInParent<HighlightEffect>();
                if (highlight != null)
                {
                    highlight.enabled = false;
                }
            }

            if (text.Trim().Length != 0 || buttonGlow)
            {
                tooltipObject.SetActive(true);

                if (highlight != null)
                {
                    highlight.enabled = buttonGlow;
                }

                if (vibration)
                {
                    Vibrate(controller);
                }
            }
            else
            {
                tooltipObject.SetActive(false);
            }


            return true;
        }

        private Transform  GetButtonTransform(TooltipControllers controller, TooltipButtons button)
        {
            ControllerInteraction.ControllerElements findElement;

            TooltipTransformAdapterBase adapter = null;

            Transform returnedTransform = null;

            if (controller == TooltipControllers.Left)
            {
                adapter = _leftTooltipAdapter;
            }
            else if (controller == TooltipControllers.Right)
            {
                adapter = _rightTooltipAdapter;
            }

            if (!adapter) return null;


            switch (button)
            {
                case TooltipButtons.Trigger:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.Trigger);
                    break;
                case TooltipButtons.Grip:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.GripLeft);
                    break;
                case TooltipButtons.Touchpad:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.Touchpad);
                    break;
                case TooltipButtons.ButtonOne:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.ButtonOne);
                    break;
                case TooltipButtons.ButtonTwo:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.ButtonTwo);
                    break;
                case TooltipButtons.StartMenu:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.StartMenu);
                    break;
                default:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.Touchpad);
                    break;
            }

            return returnedTransform;
        }

        public Transform GetTooltipInstantiationTransform(TooltipControllers controller, TooltipButtons button)
        {
            Transform tooltipTransform;

            if (controller == TooltipControllers.Left)
            {
                tooltipTransform = LeftControllerEvents.transform.Find("TooltipPositions");
            }
            else
            {
                tooltipTransform = RightControllerEvents.transform.Find("TooltipPositions");
            }

            switch (button)
            {
                case TooltipButtons.Trigger:
                    tooltipTransform = tooltipTransform.Find("Trigger");

                    break;
                case TooltipButtons.Grip:
                    tooltipTransform = tooltipTransform.Find("Grip");

                    break;
                case TooltipButtons.Touchpad:
                    tooltipTransform = tooltipTransform.Find("Touchpad");

                    break;
                case TooltipButtons.ButtonOne:
                    tooltipTransform = tooltipTransform.Find("ButtonOne");

                    break;
                case TooltipButtons.ButtonTwo:
                    tooltipTransform = tooltipTransform.Find("ButtonTwo");

                    break;
                case TooltipButtons.StartMenu:
                    tooltipTransform = tooltipTransform.Find("StartMenu");

                    break;
            }

            return tooltipTransform;
        }

        private void SetupHighlighter(ref HighlightEffect highlight)
        {
            highlight.overlayColor = Color.yellow;
            highlight.overlayAnimationSpeed = 0.3f;
            highlight.overlay = 1f;


            highlight.outlineColor = Color.yellow;
            highlight.outline = 1;

            highlight.glow = 0;

            highlight.seeThrough = SeeThroughMode.WhenHighlighted;
            highlight.seeThroughTintColor = Color.yellow;
            highlight.seeThroughTintAlpha = 1.0f;
            highlight.seeThroughIntensity = 1;

            highlight.highlighted = true;
        }

        private void SetLeftControllerReady(object sender, ControllerInput.ControllerInteractionEventArgs e)
        {
            _leftControllerReady = true;
        }

        private void SetRightControllerReady(object sender, ControllerInput.ControllerInteractionEventArgs e)
        {
            _rightControllerReady = true;
        }


        IEnumerator ControllerEventsSubscriptionSafe()
        {
            while (LeftControllerEvents == null || RightControllerEvents == null)
            {
                yield return new WaitForEndOfFrame();
            }

            LeftControllerEvents.ControllerEnabled += SetLeftControllerReady;
            RightControllerEvents.ControllerEnabled += SetRightControllerReady;
        }

        public void Vibrate(
            TooltipControllers controller,
            float strength = 0.05f,
            float duration = 0.1f,
            float interval = 0.005f)
        {
            if (controller == TooltipControllers.Left || controller == TooltipControllers.Both)
            {
                var hand = InputAdapter.Instance.PlayerController.Nodes.LeftHand;

                hand.Controller.TriggerHapticPulse(strength, duration, interval);
            }

            if (controller == TooltipControllers.Right || controller == TooltipControllers.Both)
            {
                var hand = InputAdapter.Instance.PlayerController.Nodes.RightHand;

                hand.Controller.TriggerHapticPulse(strength, duration, interval);
            }
        }
    }
}
