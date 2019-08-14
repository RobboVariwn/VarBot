using System;
using Varwin.Errors;
using NLogger;
using Varwin;
using Varwin.Data;
using Varwin.ECS.Systems.Loader;
using UnityEngine;
using Varwin.Public;
using Varwin.VRInput;

public class DebugLauncher : MonoBehaviour
{
    public Transform PlayerSpawnPoint;
    public GameMode GameMode;

    private void Update()
    {
        if (ProjectData.GameMode != GameMode)
            ProjectData.GameMode = GameMode;
    }

    private void Awake()
    {
        if (PlayerSpawnPoint == null)
        {
            Debug.LogError("PlayerSpawnPoint is null");

            return;
        }

        NLoggerSettings.Init();
        Application.logMessageReceived += ErrorHelper.ErrorHandler;
        Settings.CreateDebugSettings("");

        var playerRig = Instantiate(InputAdapter.Instance.PlayerController.RigInitializer.InitializeRig());
        InputAdapter.Instance.PlayerController.Init(playerRig);
        playerRig.transform.position = PlayerSpawnPoint.position;
        
    }

    private void Start()
    {
        ProjectData.GameMode = GameMode;
       
        InitObjectsOnScene();
    }

    private void InitObjectsOnScene()
    {
        var owdObjects = FindObjectsOfType<VarwinObjectDescriptor>();

        foreach (var owdObject in owdObjects)
        {
            SpawnInitParams spawn = new SpawnInitParams
            {
                Name = owdObject.Name,
                IdLocation = 1,
                IdInstance = 0,
                IdObject = 0,
                IdServer = 0
            };

            Helper.InitObject(0,
                spawn,
                owdObject.gameObject,
                null);
        }
    }
}
