using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Varwin.UI;

namespace Varwin.UI
{
    public class PopupWindowButton : UIButton
    {
        [SerializeField] private Text _buttonText;

        [SerializeField] protected Button _uiButton;

        public delegate void ButtonClickDelegate();

        public event ButtonClickDelegate ButtonClickedEvent;

        private void Reset()
        {
            _buttonText = GetComponentInChildren<Text>();
            _uiButton = GetComponentInChildren<Button>();
        }
        
        private void OnEnable()
        {
            _uiButton.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _uiButton.onClick.RemoveListener(OnClick);
        }

        public override void OnClick()
        {
            ButtonClickedEvent?.Invoke();
        }

        public override void OnHover()
        {
            if (_uiButton != null)
            {
                _uiButton.OnSelect(null);
            }
            else
            {
                base.OnHover();
            }
        }

        public override void OnOut()
        {
            if (_uiButton != null)
            {
                _uiButton.OnDeselect(null);
            }
            else
            {
                base.OnOut();
            }
        }

        public string ButtonText
        {
            set => _buttonText.text = value;
            get => _buttonText.text;
        }
    }
}