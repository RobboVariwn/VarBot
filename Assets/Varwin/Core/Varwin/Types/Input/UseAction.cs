using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Public;
using Varwin.VRInput;

namespace Varwin
{
    public class UseAction : InputAction
    {
        private readonly List<IUseStartAware> _useStartList;
        private readonly List<IUseEndAware> _useEndList;
        
        public UseAction(ObjectController objectController, GameObject gameObject, ObjectInteraction.InteractObject vio) : 
            base(objectController, gameObject, vio)
        {
            _useStartList = GameObject.GetComponents<IUseStartAware>().ToList();

            if (_useStartList.Count > 0)
            {
                AddUseStartBehaviour();
            }

            _useEndList = GameObject.GetComponents<IUseEndAware>().ToList();

            if (_useEndList.Count > 0)
            {
                AddUseEndBehaviour();
            }
            
        }

        #region OVERRIDES

        public override void DisableViewInput()
        {
            Vio.isUsable = false;

            if (_useStartList.Count > 0)
            {
                Vio.InteractableObjectUsed -= OnUseStartVoid;
            }

            if (_useEndList.Count > 0)
            {
                Vio.InteractableObjectUnused -= OnUseEndVoid;
            }
        }
        
        public override void EnableViewInput()
        {
            Vio.isUsable = true;

            if (_useStartList.Count > 0)
            {
                Vio.InteractableObjectUsed += OnUseStartVoid;
            }

            if (_useEndList.Count > 0)
            {
                Vio.InteractableObjectUnused += OnUseEndVoid;
            }
        }

        protected override void EnableEditorInput()
        {
             
        }

        protected override void DisableEditorInput()
        {
             
        }

        #endregion
        

        private void AddUseStartBehaviour()
        {
            Vio.isUsable = true;
            Vio.useOverrideButton = ControllerInput.ButtonAlias.TriggerPress;
        }

        private void AddUseEndBehaviour()
        {
            Vio.isUsable = true;
            Vio.useOverrideButton = ControllerInput.ButtonAlias.TriggerPress;
        }
        
        private void OnUseStartVoid(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            if (Settings.Instance().Multiplayer)
            {
                ChangeOwner();
                ObjectController.photonView.RPC("OnUseStartRpc", PhotonTargets.All);
            }
            else
            {
                UsingContext context = new UsingContext {GameObject = Vio.GetUsingObject(), Hand = interactableObjectEventArgs.Hand};

                foreach (IUseStartAware useStartAware in _useStartList)
                {
                    useStartAware?.OnUseStart(context);
                }
                
            }
        }

        private void OnUseEndVoid(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            if (Settings.Instance().Multiplayer)
            {
                ChangeOwner();
                ObjectController.photonView.RPC("OnUseEndRpc", PhotonTargets.All);
            }
            else
            {
                foreach (IUseEndAware useEndAware in _useEndList)
                {
                    useEndAware?.OnUseEnd();
                }
                
            }
        }
    }
}