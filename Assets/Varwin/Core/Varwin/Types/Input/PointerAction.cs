using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Public;
using Varwin.VRInput;
using Object = UnityEngine.Object;

namespace Varwin
{
    public class PointerAction : InputAction
    {
        private readonly List<IPointerClickAware> _pointerClick;
        private readonly List<IPointerInAware> _pointerIn;
        private readonly List<IPointerOutAware> _pointerOut;
        
        public Action OnPointerClick = delegate { };
        public Action OnPointerIn = delegate { };
        public Action OnPointerOut = delegate { };

        private readonly GameObject _gameObject;
        
        #region OVERRIDES
        public override void DisableViewInput()
        {
            RemovePointerBehaviour();
        }

        public override void EnableViewInput()
        {
            if (_pointerClick.Count > 0 || _pointerIn.Count > 0 || _pointerOut.Count > 0)
            {
                AddPointerBehaviour();
            }
        }

        protected override void DisableEditorInput()
        {
             
        }

        protected override void EnableEditorInput()
        {
             
        }
        

        #endregion

        public PointerAction(ObjectController objectController, GameObject gameObject, ObjectInteraction.InteractObject vio) : base(objectController, gameObject, vio)
        {
            _gameObject = gameObject;
            _pointerClick = _gameObject.GetComponents<IPointerClickAware>().ToList();

            if (_pointerClick.Count > 0)
            {
                AddPointerClickAction();
            }
            
            _pointerIn = _gameObject.GetComponents<IPointerInAware>().ToList();

            if (_pointerIn.Count > 0)
            {
                AddPointerInAction();
            }
            
            _pointerOut = _gameObject.GetComponents<IPointerOutAware>().ToList();
            
            if (_pointerOut.Count > 0)
            {
                AddPointerOutAction();
            }

            if (_pointerClick.Count > 0 || _pointerIn.Count > 0 || _pointerOut.Count > 0)
            {
                AddPointerBehaviour();
            }
        }
        
        private void AddPointerBehaviour()
        {
            
            ObjectPointerBehaviour pointerBehaviour = _gameObject.GetComponent<ObjectPointerBehaviour>();

            if (pointerBehaviour != null)
            {
                return;
            }

            pointerBehaviour = _gameObject.AddComponent<ObjectPointerBehaviour>();
            pointerBehaviour.Init(this);
        }
        
        private void RemovePointerBehaviour()
        {
            ObjectPointerBehaviour pointerBehaviour = _gameObject.GetComponent<ObjectPointerBehaviour>();

            if (pointerBehaviour != null)
            {
                Object.Destroy(pointerBehaviour);
            }
             
        }

        private void AddPointerClickAction()
        {
            OnPointerClick = () =>
            {
                foreach (IPointerClickAware pointerClickAware in _pointerClick)
                {
                    pointerClickAware.OnPointerClick();
                }
            };
        }

        private void AddPointerInAction()
        {
            OnPointerIn = () =>
            {
                foreach (IPointerInAware pointerInAware in _pointerIn)
                {
                    pointerInAware.OnPointerIn();
                }
            };
        }

        private void AddPointerOutAction()
        {
            OnPointerOut = () =>
            {
                foreach (IPointerOutAware pointerOutAware in _pointerOut)
                {
                    pointerOutAware.OnPointerOut();
                }
            };
        }
        
    }
}