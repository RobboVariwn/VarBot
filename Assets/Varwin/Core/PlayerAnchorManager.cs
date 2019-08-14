using System.Collections;
using NLog;
using Varwin;
using Varwin.Onboarding;
using UnityEngine;
using Varwin.VRInput;

//This version is for usage inside OpW
public class PlayerAnchorManager : MonoBehaviour
{
    private bool _trackingInitialized;
    private Transform _headTransform;
    private GameObject _player;

    private void Awake()
    {
        if (InputAdapter.Instance.PlayerController.Nodes.Head.GameObject != null)
        {
            return;
        }

        GameObject opwPlayerRig = InputAdapter.Instance.PlayerController.RigInitializer.InitializeRig();
        //Resources.Load<GameObject>("OpW Player Rig");

        if (opwPlayerRig == null)
        {
            return;
        }

        if (Settings.Instance().Multiplayer)
        {
            PhotonNetwork.Instantiate("OpW Player Rig",
                transform.position,
                Quaternion.identity,
                0);
        }
        else
        {
            _player = Instantiate(opwPlayerRig, transform.position, Quaternion.identity);
            InputAdapter.Instance.PlayerController.Init(_player);
            _headTransform = InputAdapter.Instance.PlayerController.Nodes.Head.Transform;
            //_player.transform.Find("[VRTK_SDKManager]").Find("SDKSetups").Find("SteamVR").GetComponentInChildren<Camera>().transform;
            _trackingInitialized = false;

            StartCoroutine(MovePlayerToSpawnCoroutine());
        }
    }

    private void Start()
    {
    }

    private IEnumerator ResetPlayer()
    {
        Debug.Log("turning player off (disabled)");

        //   _player.SetActive(false);
        yield return new WaitForEndOfFrame();
        //  _player.SetActive(true);
        StartCoroutine(InvokeOnLoadLocation());

        yield return true;
    }

    IEnumerator InvokeOnLoadLocation()
    {
        while (!ProjectData.ObjectsAreLoaded)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();


        if (LicenseInfo.IsDemo && !Settings.Instance().OnboardingMode)
        {
            GameObject go = new GameObject("License Notificator");
            go.AddComponent<LicenceNotificator>();
        }

        yield return true;
    }

    private IEnumerator WaitGameObjectsInstance()
    {
        while (GameObjects.Instance == null)
        {
            yield return null;
        }

        GameObject newPlayer = GameObjects.Instance.PlayerRig.gameObject;
            //InputAdapter.Instance.PlayerController.Tracking.GetBoundaries(GameObjects.Instance.PlayerRig);
        //GameObjects.Instance.PlayerRig.GetComponentInChildren<VRTK_SDKSetup>(true).actualBoundaries;
        _player = newPlayer;
        _player.transform.position = transform.position;
        _headTransform = _player.transform.GetComponentInChildren<Camera>().transform;
        _trackingInitialized = false;
    }

    private IEnumerator MovePlayerToSpawnCoroutine()
    {
        yield return WaitGameObjectsInstance();

        while (!_trackingInitialized)
        {
            Vector3 diff = _headTransform.position - _player.transform.position;

            if (diff != Vector3.zero)
            {
                _trackingInitialized = true;

                Vector3 newRot = _player.transform.eulerAngles;

                newRot.x = 0;
                newRot.y -= _headTransform.transform.eulerAngles.y - transform.eulerAngles.y;
                newRot.z = 0;

                _player.transform.rotation = Quaternion.Euler(newRot);

                Vector3 newPos = _player.transform.position;
                diff = _headTransform.position - newPos;
                newPos.x -= diff.x;
                newPos.z -= diff.z;

                _player.transform.position = newPos;

                LogManager.GetCurrentClassLogger().Info("Player position initialized");
            }

            yield return null;
        }

        yield return true;
    }

    public void RestartPlayer()
    {
        StartCoroutine(RestartPlayerCoroutine());
    }

    private IEnumerator RestartPlayerCoroutine()
    {
        yield return WaitGameObjectsInstance();
        yield return MovePlayerToSpawnCoroutine();
        yield return ResetPlayer();

        yield return true;
    }

    public void SetPlayerPosition()
    {
        StartCoroutine(MovePlayerToSpawnCoroutine());
    }

    /// <summary>
    /// Backward compatibility (Надеюсь что мы скоро перепишим это вместе с объектом плеер)
    /// </summary>
    public void ReloadPlayer(GameObject newPlayer)
    {
        StartCoroutine(MovePlayerToSpawnCoroutine());
    }
}
