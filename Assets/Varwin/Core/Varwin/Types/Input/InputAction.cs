using UnityEngine;
using Varwin.VRInput;

namespace Varwin
{
    public class InputAction
    {
        protected readonly ObjectInteraction.InteractObject Vio;
        protected readonly ObjectController ObjectController;
        protected readonly GameObject GameObject;
        public bool IsRootGameObject => GameObject == ObjectController.RootGameObject;

        protected InputAction(ObjectController objectController, GameObject gameObject, ObjectInteraction.InteractObject vio)
        {
            ObjectController = objectController;
            GameObject = gameObject;
            Vio = vio;
        }

        public virtual void DisableViewInput()
        {
            
        }

        public virtual void EnableViewInput()
        {
            
        }

        protected virtual void DisableEditorInput()
        {
            
        }

        protected virtual void EnableEditorInput()
        {
            
        }
        
        public void GameModeChanged(GameMode newGameMode)
        {
            if (newGameMode == GameMode.Edit)
            {
                DisableViewInput();
                EnableEditorInput();
            }
            else
            {
                DisableEditorInput();
                EnableViewInput();
            }
        }

        protected void ChangeOwner()
        {
            if (!Settings.Instance().Multiplayer)
            {
                return;
            }

            if (ObjectController.photonView.ownerId == PhotonNetwork.player.ID)
            {
                return;
            }

            ObjectController.photonView.TransferOwnership(PhotonNetwork.player.ID);
        }
        
    }
}