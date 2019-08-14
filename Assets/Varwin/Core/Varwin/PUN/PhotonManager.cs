using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameObject = UnityEngine.GameObject;

namespace Varwin.PUN
{
	public class PhotonManager : Photon.PunBehaviour {

		#region Public Variables

		public static PhotonManager Instance;
	    public GameObject Spawner;

		[Header("Player prefubs")]
		public GameObject Head;
	    public GameObject LeftHand;
	    public GameObject RightHand;
	    public GameObject Body;

        [Header("Steam VR Controllers")]
        public Transform HeadController;
	    public Transform LeftHandController;
	    public Transform RightHandController;

        #endregion

        #region Private Variables

        private GameObject _instance;

		#endregion

		#region MonoBehaviour CallBacks

	    /// <summary>
	    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
	    /// </summary>
	    void Start()
	    {
	        Instance = this;

            if (!Settings.Instance().Multiplayer) return;

	        PhotonNetwork.PhotonServerSettings.ServerAddress = Settings.Instance().PhotonHost;
            //StartCoroutine(CheckOnLine());
            if (!PhotonNetwork.inRoom)
	        {
	            PhotonNetwork.autoCleanUpPlayerObjects = false;
	            Debug.Log("Auto clean up player objects turned off");
	        }
	     
	        if (!PhotonNetwork.connected)
	        {
	            SceneManager.LoadScene("Launcher");
	            return;
	        }

	        if (Head == null)
	        {
	            Debug.LogError(
	                "<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",
	                this);
	        }
	        else
	        {
	            if (VRAObjectPhotonManager.LocalPlayerInstance == null)
	            {
	                Debug.Log("We are Instantiating LocalPlayer from " + SceneManagerHelper.ActiveSceneName);

	                var head = PhotonNetwork.Instantiate(Head.name, Vector3.zero, Quaternion.identity, 0);
	                head.AddComponent<SyncPosition>().Init(HeadController);

	                var leftHand = PhotonNetwork.Instantiate(LeftHand.name, Vector3.zero, Quaternion.identity, 0);
	                leftHand.AddComponent<SyncPosition>().Init(LeftHandController);

	                var rightHand = PhotonNetwork.Instantiate(RightHand.name, Vector3.zero, Quaternion.identity, 0);
	                rightHand.AddComponent<SyncPosition>().Init(RightHandController);

	                var body = PhotonNetwork.Instantiate(Body.name, Vector3.zero, Quaternion.identity, 0);

                    var playerControl = head.GetComponent<PlayerControl>();
	                playerControl.InitRpc();
	            }
	            else
	            {
	                Debug.Log("Ignoring scene load for " + SceneManagerHelper.ActiveSceneName);
	            }
	        }
	    }

	    public void SpawnAssetBundle(string bundleName, string assetName)
        {
            //photonView.RPC("SpawnSpawnerRPC", PhotonTargets.MasterClient, new object[] 
            //{ bundleName, assetName, GameControllerObjects.Instance.SpawnPoint.position, GameControllerObjects.Instance.Head.position });    
            
            int id = PhotonNetwork.AllocateViewID();
            photonView.RPC("SpawnAssetBundleRpc", PhotonTargets.All, bundleName, assetName,
                GameObjects.Instance.SpawnPoint.position, GameObjects.Instance.Head.position, id);

        }

		void Update()
		{
            // "back" button of phone equals "Escape". quit app if that's pressed
            if (Input.GetKeyDown(KeyCode.Escape))
			{
				//QuitApplication();
			}
		}

	    void ValidateOnLine()
	    {
	        if (!Settings.Instance().Multiplayer) return;
            List<PlayerControl> players = FindObjectsOfType<PlayerControl>().ToList();
	        foreach (var o in players)
	        {
	            if (!PhotonNetwork.playerList.Contains(o.GetComponent<PhotonView>().owner))
	                o.Remove();
	        }
	    }

	    #endregion

        #region Photon Messages

        /// <summary>
        /// Called when a Photon Player got connected. We need to then load a bigger scene.
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPhotonPlayerConnected( PhotonPlayer other  )
		{
			Debug.Log( "OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting
            if ( PhotonNetwork.isMasterClient ) 
			{
				Debug.Log( "OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient ); // called before OnPhotonPlayerDisconnected
				LoadArena();
			}
		}

	    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	    {
	        ValidateOnLine();
	    }


	    //private void OnMasterServerEvent(MasterServerEvent msEvent)
	    //{
	       

     //   }

	    /// <summary>
		/// Called when a Photon Player got disconnected. We need to load a smaller scene.
		/// </summary>
		/// <param name="other">Other.</param>
		public override void OnPhotonPlayerDisconnected( PhotonPlayer other  )
		{
			Debug.Log( "OnPhotonPlayerDisconnected() " + other.NickName ); // seen when other disconnects
		    ValidateOnLine();
            if ( PhotonNetwork.isMasterClient ) 
			{
				Debug.Log( "OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient ); // called before OnPhotonPlayerDisconnected
                LoadArena();

			}

		    //PhotonNetwork.ReconnectAndRejoin();
        }

	    /// <summary>
		/// Called when the local player left the room. We need to load the launcher scene.
		/// </summary>
		public override void OnLeftRoom()
	    {
            SceneManager.LoadScene("Launcher");
        }

#endregion

#region Public Methods

		public void LeaveRoom()
		{
			PhotonNetwork.LeaveRoom();
		}

		public void QuitApplication()
		{
			Application.Quit();
		}

#endregion

#region Private Methods

		void LoadArena()
		{
			//if ( ! PhotonNetwork.isMasterClient ) 
			//{
			//	Debug.LogError( "PhotonNetwork : Trying to Load a level but we are not the master Client" );
			//}

			//Debug.Log( "PhotonNetwork : Loading Level : " + PhotonNetwork.room.PlayerCount ); 

			//PhotonNetwork.LoadLevel("TeacherStudent");
		}

#endregion

	}

}
