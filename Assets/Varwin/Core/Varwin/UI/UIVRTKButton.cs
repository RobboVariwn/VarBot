/*
using System;
using UnityEngine;
using VRTK;

namespace Varwin.UI
{
    // ReSharper disable once InconsistentNaming
    [RequireComponent(typeof(VRTK_InteractableObject))]
    [RequireComponent(typeof(BoxCollider))]
    public class UIVRTKButton : MonoBehaviour
    {
        private VRTK_InteractableObject _vio;
        private bool _isAction;

        public Action OnActivate = delegate {  };

        void Awake()
        {
            _vio = GetComponent<VRTK_InteractableObject>();
            _vio.isUsable = true;
            _vio.useOverrideButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
            GetComponent<BoxCollider>().isTrigger = true;
        }

        void Update()
        {
            if (_vio == null) _vio = GetComponent<VRTK_InteractableObject>();
            if (_vio == null) return;

            if (_vio.IsUsing() && !_isAction)
            {
                try
                {
                    OnActivate();
                }
                catch (Exception ex)
                {
                   // DebugVR.Log("Ошибка активации объекта " + VRAO + ex.Message);
                }
            }

            _isAction = _vio.IsUsing();

        }
    }
}
*/
