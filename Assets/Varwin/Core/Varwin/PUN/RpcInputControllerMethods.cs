using Photon;
using UnityEngine;

namespace Varwin
{
    public class RpcInputControllerMethods : PunBehaviour
    {
        private InputController _inputController;

        public void Init(InputController baseType)
        {
            _inputController = baseType;
        }

        [PunRPC]
        private void OnClickRpc()
        {
            Debug.Log($"OnClick event. Sender: {gameObject.name}. Owner: {photonView.owner.NickName}");
            //_inputController.OnUseStart();
        }

        [PunRPC]
        private void OnUngrabbedRpc()
        {
            Debug.Log($"OnUngrabbed event. Sender: {gameObject.name}. Owner: {photonView.owner.NickName}");
            //_inputController.OnEditorGrabEnd();
        }

        [PunRPC]
        private void OnGrabbedRpc()
        {
            Debug.Log($"OnGrabbed event. Sender: {gameObject.name}. Owner: {photonView.owner.NickName}");
            //_inputController.OnEditorGrabStart();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
             
        }
    }
}
