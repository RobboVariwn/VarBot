using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Public;
using Varwin.VRInput;

namespace Varwin
{
    public class TouchAction: InputAction
    {
        private readonly List<ITouchStartAware> _touchStartList;
        private readonly List<ITouchEndAware> _touchEndList;

        public TouchAction(ObjectController objectController, GameObject gameObject, ObjectInteraction.InteractObject vio) : base(objectController, gameObject, vio)
        {
            _touchStartList = GameObject.GetComponents<ITouchStartAware>().ToList();

            if (_touchStartList.Count > 0)
            {
                AddTouchStartAction();
            }

            _touchEndList = GameObject.GetComponents<ITouchEndAware>().ToList();

            if (_touchEndList.Count > 0)
            {
                AddTouchEndAction();
            }
        }

        #region OVERRIDES
        public override void DisableViewInput()
        {
            if (_touchStartList.Count > 0)
            {
                Vio.InteractableObjectTouched -= OnTouchStartVoid;
            }

            if (_touchEndList.Count > 0)
            {
                Vio.InteractableObjectUntouched -= OnTouchEndVoid;
            }
        }

        public override void EnableViewInput()
        {
            if (_touchStartList.Count > 0)
            {
                Vio.InteractableObjectTouched += OnTouchStartVoid;
            }

            if (_touchEndList.Count > 0)
            {
                Vio.InteractableObjectUntouched += OnTouchEndVoid;
            }
        }

        protected override void DisableEditorInput()
        {
             
        }

        protected override void EnableEditorInput()
        {
              
        }
        
        #endregion

        private void AddTouchStartAction()
        {
            Vio.InteractableObjectTouched += OnTouchStartVoid;
        }

        private void AddTouchEndAction()
        {
            Vio.InteractableObjectUntouched += OnTouchEndVoid;
        }
        
        private void OnTouchStartVoid(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            if (Settings.Instance().Multiplayer)
            {
                ChangeOwner();
                ObjectController.photonView.RPC("OnTouchStartRpc", PhotonTargets.All);
            }
            else
            {
                foreach (ITouchStartAware touchStartAware in _touchStartList)
                {
                    touchStartAware?.OnTouchStart();
                }
            }
        }

        private void OnTouchEndVoid(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            if (Settings.Instance().Multiplayer)
            {
                ChangeOwner();
                ObjectController.photonView.RPC("OnTouchEndRpc", PhotonTargets.All);
            }
            else
            {
                foreach (ITouchEndAware touchEndAware in _touchEndList)
                {
                    touchEndAware?.OnTouchEnd();
                }
                
            }
        }
    }
}