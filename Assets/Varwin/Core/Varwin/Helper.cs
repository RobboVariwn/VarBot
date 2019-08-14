using System;
using System.Collections.Generic;
using Varwin.Errors;
using Varwin.Data;
using DesperateDevs.Utils;
using NLog;
using UnityEngine;
using Varwin.Commands;
using Varwin.Data.ServerData;
using Varwin.ECS.Systems.Saver;
using Varwin.ECS.Systems.UI;
using Varwin.Models.Data;
using Varwin.ObjectsInteractions;
using Varwin.Public;
using Varwin.Types;
using Varwin.UI;
using Varwin.UI.VRErrorManager;
using Varwin.UI.VRMessageManager;
using Varwin.VRInput;
using Varwin.WWW;
using Object = UnityEngine.Object;

namespace Varwin
{
    /// <summary>
    ///     Класс помошник для разных методов. По мере наполнения, надо будет группировать методы по логиге и разбивать на
    ///     классы
    /// </summary>
    public static class Helper
    {
        private static GameObject _spawnView;
        private static readonly SaveObjectsSystem SaveObjectsSystem = new SaveObjectsSystem(Contexts.sharedInstance);
        private static readonly ShowUiObjects ShowUiObjectsSystem = new ShowUiObjects(Contexts.sharedInstance);
        private static readonly HideUiObjects HideUiObjectsSystem = new HideUiObjects(Contexts.sharedInstance);
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        
        public static event Action<ObjectController> ObjectControllerCreated;

        public static PhotonView AddPhoton(GameObject go, int id)
        {
            if (!Settings.Instance().Multiplayer)
            {
                return null;
            }

            PhotonView photonV = go.AddComponent<PhotonView>();
            photonV.viewID = id;
            photonV.synchronization = ViewSynchronization.UnreliableOnChange;
            photonV.ObservedComponents = new List<Component>();

            return photonV;
        }

        public static bool IsResponseGood(ResponseApi requestResponse)
        {
            if (requestResponse.Status != "error")
            {
                return true;
            }

            _logger.Error($"Response has error! {requestResponse.Message}");
            VRErrorManager.Instance.Show(requestResponse.Message);

            return false;

        }


        public static void SpawnSceneObjects(int newSceneId, int oldSceneId)
        {
            if (newSceneId == oldSceneId)
            {
                _logger.Info($"Objects for scene {newSceneId} already loaded");
                return;
            }
            
            _logger.Info($"Loading objects for scene {newSceneId}");
            ParentManager.Instance.ParentCommand = ParentCommand.None;

            if (oldSceneId != 0 && LoaderAdapter.LoaderType == typeof(ApiLoader))
            {
                AMQPClient.UnSubscribeLogicChange(oldSceneId);
                AMQPClient.UnSubscribeCompilationError(oldSceneId);
                AMQPClient.UnSubscribeObjectChange(oldSceneId);
            }

            GameStateData.ClearObjects();
            ProjectData.ObjectsAreChanged = false;
            Data.ServerData.Scene location = ProjectData.ProjectStructure.Scenes.GetProjectScene(newSceneId);
            LogicInstance logicInstance = new LogicInstance(newSceneId);
            GameStateData.RefreshLogic(logicInstance, location.AssemblyBytes);
            CreateSpawnEntities(location.SceneObjects, newSceneId);

            if (LoaderAdapter.LoaderType == typeof(ApiLoader))
            {
                AMQPClient.SubscribeLogicChange(newSceneId);
                AMQPClient.SubscribeCompilationError(newSceneId);
                AMQPClient.SubscribeObjectChange(ProjectData.ProjectId, newSceneId);
            }
        }

        public static void ReloadSceneObjects()
        {
            _logger.Info($"Reloading objects on scene {ProjectData.SceneId}");
            ParentManager.Instance.ParentCommand = ParentCommand.None;
            GameStateData.ClearObjects();
            ProjectData.ObjectsAreChanged = false;
            Data.ServerData.Scene location = ProjectData.ProjectStructure.Scenes.GetProjectScene(ProjectData.SceneId);
            LogicInstance logicInstance = new LogicInstance(ProjectData.SceneId);
            GameStateData.RefreshLogic(logicInstance, location.AssemblyBytes);
            CreateSpawnEntities(location.SceneObjects, ProjectData.SceneId);
        }

        private static void CreateSpawnEntities(List<ObjectDto> locationObjects, int groupId)
        {
            foreach (ObjectDto o in locationObjects)
            {
                CreateSpawnEntity(o, groupId);
            }
        }

        private static void CreateSpawnEntity(ObjectDto o, int locationId, int? parentId = null)
        {
            bool embedded = false;
            PrefabObject prefabObject = ProjectData.ProjectStructure.Objects.GetById(o.ObjectId);

            if (prefabObject != null)
            {
                embedded = prefabObject.Embedded;
            }
            else
            {
                Debug.LogError("Object is not contains in project structure");
            }

            SpawnInitParams param = new SpawnInitParams
            {
                IdLocation = locationId,
                IdInstance = o.InstanceId,
                IdObject = o.ObjectId,
                IdServer = o.Id,
                Name = o.Name,
                ParentId = parentId,
                Embedded = embedded
            };

            if (o.Data != null)
            {
                param.Transforms = o.Data.Transform;
                param.Joints = o.Data.JointData;
            }

            Spawner.Instance.SpawnAsset(param);

            foreach (ObjectDto dto in o.SceneObjects)
            {
                CreateSpawnEntity(dto, locationId, o.InstanceId);
            }
        }

        public static void SpawnObject(int objectId)
        {
            if (objectId == 0)
            {
                return;
            }

            ProjectData.ObjectsAreChanged = true;

            var transforms = new Dictionary<int, TransformDT>();
            GameObject gameObject = GameStateData.GetPrefabGameObject(objectId);
            int id = gameObject.GetComponent<ObjectId>().Id;
            transforms.Add(id, _spawnView.transform.ToTransformDT());

            SpawnInitParams param = new SpawnInitParams
            {
                IdObject = objectId,
                IdLocation = ProjectData.SceneId,
                Transforms = transforms
            };

            if (ParentManager.Instance != null && ParentManager.Instance.ParentCommand == ParentCommand.SetNew)
            {
                param.ParentId = ParentManager.Instance.GetSelectedParent().Id;
            }

            ICommand command = new SpawnCommand(param);
            command.Execute();
        }

        public static bool CanObjectBeSpawned()
        {
            if (_spawnView == null)
            {
                return false;
            }

            CollisionController _controller = _spawnView.GetComponent<CollisionController>();

            if (_controller != null)
            {
                return !_controller.IsBlocked();
            }

            return false;
        }

        public static void SetSpawnedObject(int objectId)
        {
            ProjectData.SelectedObjectIdToSpawn = objectId;

            if (_spawnView != null)
            {
                Object.Destroy(_spawnView);
            }

            GameObject go = GameStateData.GetPrefabGameObject(objectId);
            _spawnView = Object.Instantiate(go, GameObjects.Instance.SpawnPoint);
            SetGameObjectToSpawn(_spawnView);
        }

        private static void SetGameObjectToSpawn(GameObject go)
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.LookAt(GameObjects.Instance.Head);
            go.AddComponent<UISpawnPrefab>();
            go.AddComponent<CollisionController>().InitializeController();

            var transforms = go.GetComponentsInChildren<Transform>();

            foreach (Transform child in transforms)
            {
                Rigidbody body = child.GetComponent<Rigidbody>();

                if (body != null)
                {
                    body.isKinematic = true;
                }

                Animator animator = child.GetComponent<Animator>();

                if (animator != null)
                {
                    animator.enabled = false;
                }

                MonoBehaviour monoBehaviour = child.GetComponent<MonoBehaviour>();

                if (monoBehaviour != null)
                {
                    monoBehaviour.enabled = false;
                }
            }
        }

        public static void ResetSpawnObject()
        {
            UISpawnPrefab spawnPrefab = Object.FindObjectOfType<UISpawnPrefab>();

            if (spawnPrefab != null)
            {
                Object.Destroy(spawnPrefab.gameObject);
                spawnPrefab = null;
            }

            ProjectData.SelectedObjectIdToSpawn = 0;
        }

        public static void SaveSceneObjects()
        {
            if (ProjectData.GameMode == GameMode.Edit)
            {
                SaveObjectsSystem.Execute();
                
                var message = SmartLocalization.LanguageManager.Instance.GetTextValue("SAVED");
                NotificationWindowManager.Show(message, 1f);
            }
        }

        public static void AskUserToDo(string question, Action actionYes, Action actionNo, Action actionCancel = null)
        {
            if (VRMessageManager.Instance.Showing)
            {
                return;
            }

            HideUi();
            VRMessageManager.Instance.Show(question);

            VRMessageManager.Instance.Result = result =>
            {
                switch (result)
                {
                    case DialogResult.Cancel:
                        actionCancel?.Invoke();

                        break;
                    case DialogResult.Yes:
                        actionYes?.Invoke();

                        break;
                    case DialogResult.No:
                        actionNo?.Invoke();
                        break;
                }
            };
        }

        public static void HideUi(bool switchMenuMode = false)
        {
            HideUiObjectsSystem.Execute();       
            InputAdapter.Instance.PointerController.IsMenuOpened = switchMenuMode;
        }

        public static void ShowUi()
        {
            if (VRMessageManager.Instance.Showing)
            {
                return;
            }

            if (VRErrorManager.Instance.Showing)
            {
                return;
            }
        
            ShowUiObjectsSystem.Execute();
            
            InputAdapter.Instance.PointerController.IsMenuOpened = true;
        }


        public static LineRenderer CreateLineRenderer(GameObject go)
        {
            if (ProjectData.GameMode == GameMode.View)
            {
                return null;
            }

            LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
            Color c1 = new Color(0, 112, 255, 109);
            Color c2 = new Color(26, 255, 4, 82);

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.widthMultiplier = 0.005f;

            // A simple 2 color gradient with a fixed alpha of 1.0f.
            float alpha = 1.0f;
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new[] {new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f)},
                new[] {new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f)}
            );
            lineRenderer.colorGradient = gradient;

            return lineRenderer;
        }

        public static void ShowErrorLoadObject(PrefabObject o, string error)
        {
            string message = $"Load object {o.Config.i18n.en} error! {error}";
            LogManager.GetCurrentClassLogger().Fatal(message);
            LauncherErrorManager.Instance.ShowFatal(message, null);
        }

        public static void ShowErrorLoadScene()
        {
            string message = ErrorHelper.GetErrorDescByCode(Varwin.Errors.ErrorCode.LoadSceneError);
            LogManager.GetCurrentClassLogger().Fatal("Location is not loaded");
            LauncherErrorManager.Instance.Show(message, null);
        }

        public static bool HaveSaveTransform(MonoBehaviour behaviour)
        {
            Type type = behaviour.GetType();

            return type.ImplementsInterface<ISaveTransformAware>();
        }

        public static bool HaveInputs(MonoBehaviour behaviour)
        {
            Type type = behaviour.GetType();

            if (type.ImplementsInterface<IUseStartAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IUseEndAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IGrabStartAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IGrabEndAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IPointerClickAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IPointerInAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IPointerOutAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IHighlightAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<ITouchStartAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<ITouchEndAware>())
            {
                return true;
            }

            return false;
        }


        /// <summary>
        ///     Initialize object in platform
        /// </summary>
        /// <param name="idObject">Object type id. Used for save.</param>
        /// <param name="spawnInitParams">Parameters for spawn</param>
        /// <param name="spawnedGameObject">Game object for init</param>
        public static void InitObject(int idObject, SpawnInitParams spawnInitParams, GameObject spawnedGameObject, Config config)
        {
            //var photonView = AddPhoton(spawnedGameObject, spawnInitParams.spawnAsset.IdPhoton);
            PhotonView photonView = AddPhoton(spawnedGameObject, 0);
            GameObject gameObjectLink = spawnedGameObject;
            int idLocation = spawnInitParams.IdLocation;
            int idServer = spawnInitParams.IdServer;
            int idInstance = spawnInitParams.IdInstance;
            bool embedded = spawnInitParams.Embedded;
            string name = spawnInitParams.Name;
            var parentId = spawnInitParams.ParentId;
            
            ObjectController parent = null;

            if (parentId != null)
            {
                parent = GameStateData.GetObjectInLocation(parentId.Value);
            }

            WrappersCollection wrappersCollection = null;

            if (idLocation != 0)
            {
                wrappersCollection = GameStateData.GetWrapperCollection();
            }

            InitObjectParams initObjectParams = new InitObjectParams
            {
                Id = idInstance,
                IdObject = idObject,
                IdLocation = idLocation,
                IdServer = idServer,
                Asset = gameObjectLink,
                Name = name,
                Photonview = photonView,
                RootGameObject = spawnedGameObject,
                WrappersCollection = wrappersCollection,
                Parent = parent,
                Embedded = embedded,
                Config = config
            };

            var newController = new ObjectController(initObjectParams);
            
            ObjectControllerCreated?.Invoke(newController);
        }

        public static Dictionary<int, JointPoint> GetJointPoints(GameObject go)
        {
            var objectIds = go.GetComponentsInChildren<ObjectId>();
            Dictionary<int, JointPoint> jointPoints = new Dictionary<int, JointPoint>();

            foreach (ObjectId objectId in objectIds)
            {
                var jointPoint = objectId.gameObject.GetComponent<JointPoint>();

                if (jointPoint == null)
                {
                    continue;
                }

                if (!jointPoints.ContainsKey(objectId.Id))
                {
                    jointPoints.Add(objectId.Id, jointPoint);
                }
            }

            return jointPoints;
        }

        public static void ReloadJointConnections(ObjectController objectController, JointData saveJointData)
        {
            JointBehaviour jointBehaviour = objectController.RootGameObject.GetComponent<JointBehaviour>();
            var jointPoints = GetJointPoints(objectController.RootGameObject);

            foreach (var jointConnectionsData in saveJointData.JointConnetionsData)
            {
                int pointId = jointConnectionsData.Key;
                JointPoint myJointPoint = jointPoints[pointId];
                JointConnetionsData connectionData = jointConnectionsData.Value;
                ObjectController otherObjectController = GameStateData.GetObjectInLocation(connectionData.ConnectedObjectInstanceId);
                var otherJointPoints = GetJointPoints(otherObjectController.RootGameObject);
                JointPoint otherJointPoint = otherJointPoints[connectionData.ConnectedObjectJointPointId];
                jointBehaviour.ConnectToJointPoint(myJointPoint, otherJointPoint);
                myJointPoint.IsForceLocked = connectionData.ForceLocked;
                otherJointPoint.IsForceLocked = connectionData.ForceLocked;
            }
        }
    }
}
