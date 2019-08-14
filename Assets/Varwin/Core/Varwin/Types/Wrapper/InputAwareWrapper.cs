using UnityEngine;

namespace Varwin
{
    public class InputAwareWrapper : Wrapper
    {
        private bool _switchInputUse;
        private bool _switchInputGrab;
        private bool _switchInputTouch;

        public InputAwareWrapper(GameEntity entity) : base(entity)
        {
        }

        public InputAwareWrapper(GameObject gameObject) : base(gameObject)
        {
        }


        [Setter("InputActionSetter")]
        [Getter("InputActionGetter")]
        [Locale(SystemLanguage.English, "can be used")]
        [Locale(SystemLanguage.Russian, "возможно использовать")]
        public bool SwitchInputUse
        {
            get { return _switchInputUse; }
            set
            {
                if (value)
                {
                    GetInputController(GetGameObject())?.EnableViewUsing();
                }
                else
                {
                    GetInputController(GetGameObject())?.DisableViewUsing();
                }

                _switchInputUse = value;
            }
        }

        [Setter("InputActionSetter")]
        [Getter("InputActionGetter")]
        [Locale(SystemLanguage.English, "can be grabbed")]
        [Locale(SystemLanguage.Russian, "возможно держать в руке")]
        public bool SwitchInputGrab
        {
            get { return _switchInputGrab; }
            set
            {
                if (value)
                {
                    GetInputController(GetGameObject())?.EnableViewGrab();
                }
                else
                {
                    GetInputController(GetGameObject())?.DisableViewGrab();
                }

                _switchInputGrab = value;
            }
        }

        [Setter("InputActionSetter")]
        [Getter("InputActionGetter")]
        [Locale(SystemLanguage.English, "can be touched")]
        [Locale(SystemLanguage.Russian, "возможно касаться")]
        public bool SwitchInputTouch
        {
            get { return _switchInputTouch; }
            set
            {
                if (value)
                {
                    GetInputController(GetGameObject())?.EnableViewTouch();
                }
                else
                {
                    GetInputController(GetGameObject())?.DisableViewTouch();
                }

                _switchInputTouch = value;
            }
        }
    }
}