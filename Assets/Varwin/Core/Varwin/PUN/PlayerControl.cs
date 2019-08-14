using System.Collections.Generic;
using System.Linq;
using Photon;
using UnityEngine;

namespace Varwin.PUN
{
    public class PlayerControl: PunBehaviour, IPunObservable
    {
        public GameObject PlayerName;
        public GameObject LeftHand;
        public GameObject RightHand;
        public GameObject Head;
        public GameObject Body;

        //public Vector3 BodyOffset = new Vector3(0, 0, 0);

        public void InitRpc()
        {
            photonView.RPC("Init", PhotonTargets.All);
        }

        [PunRPC]
        public void Init()
        {
            PhotonView[] photonViews = FindObjectsOfType<PhotonView>();

            foreach (PhotonView view in photonViews)
            {
                if (view.ownerId != photonView.ownerId) return;
                if (view.gameObject.name.Contains("Head")) Head = view.gameObject;
                if (view.gameObject.name.Contains("RightHand")) RightHand = view.gameObject;
                if (view.gameObject.name.Contains("LeftHand")) LeftHand = view.gameObject;
                if (view.gameObject.name.Contains("Body")) Body = view.gameObject;
            }

            if (photonView.isMine) RenderGameObject(Body, false);
        }

        void RenderGameObject(GameObject go, bool render)
        {
            List<Transform> childs = go.GetComponentsInChildren<Transform>().ToList();
            foreach (var child in childs)
            {
                var r = child.GetComponent<Renderer>();
                if (r != null) r.enabled = render;
            }
        }


        //[PunRPC]
        public void Remove()
        {
            if (Head == null)
            {
                InitRpc();
            }
            PhotonNetwork.Destroy(LeftHand);
            PhotonNetwork.Destroy(RightHand);
            PhotonNetwork.Destroy(Head);
            PhotonNetwork.Destroy(Body);
        }

        private void Start()
        {
            PlayerName.GetComponentInChildren<TextMesh>().text = photonView.owner.NickName;
        }

        private void Update()
        {
            try
            {
                PlayerName.transform.LookAt(GameObjects.Instance.Head.position);
                //PlayerName.transform.eulerAngles = new Vector3(PlayerName.transform.eulerAngles.x,
                //    PlayerName.transform.eulerAngles.y - 180, PlayerName.transform.eulerAngles.z);
                Body.transform.position = Head.transform.position;
                Body.transform.eulerAngles = new Vector3(0, Head.transform.eulerAngles.y, 0);
                Body.transform.position = Head.transform.position;
            }
            catch 
            {
                Debug.Log("Some error hapends with player sync... Try to reinit");
                InitRpc();
            }

        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(Head);
                stream.SendNext(LeftHand);
                stream.SendNext(RightHand);
            }
            else
            {
                Head = (GameObject) stream.ReceiveNext();
                LeftHand = (GameObject) stream.ReceiveNext();
                RightHand = (GameObject) stream.ReceiveNext();
            }
        }
    }
}
