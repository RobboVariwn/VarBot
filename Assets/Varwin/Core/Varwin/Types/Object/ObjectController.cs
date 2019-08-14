using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DesperateDevs.Utils;
using Entitas.VisualDebugging.Unity;
using Newtonsoft.Json;
using NLog;
using UnityEngine;
using Varwin.Data;
using Varwin.Data.ServerData;
using Varwin.Models.Data;
using Varwin.Public;
using Varwin.UI.ObjectManager;
using Varwin.VRInput;
using Varwin.WWW;
using Object = UnityEngine.Object;

#pragma warning disable 618

namespace Varwin
{
    /// <summary>
    /// Base class for all objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ObjectController
    {
        #region PRIVATE VARS

        private Contexts _context;
        private int _parentId;
        private ObjectController _parent;
        private Config _config;
        private readonly Dictionary<int, InputController> _inputControllers = new Dictionary<int, InputController>();
        private readonly Dictionary<Transform, bool> _rigidBodyKinematicsDefaults = new Dictionary<Transform, bool>();

        private readonly List<GameModeSwitchController> _gameModeSwitchControllers =
            new List<GameModeSwitchController>();

        private readonly List<ColliderController> _colliderControllers = new List<ColliderController>();
        private readonly List<ObjectTransform> _objectTransforms = new List<ObjectTransform>();

        #endregion

        #region PUBLIC VARS

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Link to gameObject with IWraperAware
        /// </summary>
        public GameObject gameObject { get; private set; }

        /// <summary>
        /// Link to root gameObject
        /// </summary>
        public GameObject RootGameObject { get; private set; }

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Link to photonView
        /// </summary>
        public PhotonView photonView { get; private set; }

        /// <summary>
        /// ECS Entity link
        /// </summary>
        public GameEntity Entity;

        /// <summary>
        /// ECS Entity draw line to parent
        /// </summary>
        public GameEntity EntityParentLine;

        public Action OnDestroy = delegate { };

        /// <summary>
        /// Object Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Id inside group
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Location Id 
        /// </summary>
        public int IdLocation { get; private set; }

        /// <summary>
        /// Id object inside server
        /// </summary>
        public int IdServer { get; set; }

        /// <summary>
        /// Object Type Id. Used to save.
        /// </summary>
        public int IdObject { get; private set; }

        public WrappersCollection WrappersCollection { get; private set; }

        /// <summary>
        /// Parent BaseType (GameObject)
        /// </summary>
        public ObjectController Parent
        {
            get { return _parent; }
            set { SetParent(value); }
        }

        private void SetParent(ObjectController value)
        {
            _parent = value;
            Vector3 scale = RootGameObject.transform.localScale;

            if (value != null)
            {
                _parentId = value.Id;
                Entity.ReplaceIdParent(_parentId);

                if (EntityParentLine == null)
                {
                    EntityParentLine = _context.game.CreateEntity();
                    GameObject go = new GameObject("LineToParent");
                    EntityParentLine.AddGameObject(go);
                    LineRenderer lineRenderer = Helper.CreateLineRenderer(go);

                    if (lineRenderer != null)
                    {
                        EntityParentLine.AddLineRenderer(lineRenderer);

                        EntityParentLine.AddChildrenParentTransform(RootGameObject.transform,
                            value.RootGameObject.transform);
                    }
                }
                else
                {
                    EntityParentLine.ReplaceChildrenParentTransform(RootGameObject.transform,
                        value.RootGameObject.transform);
                }

                RootGameObject.transform.SetParent(value.RootGameObject.transform, true);

                LogManager.GetCurrentClassLogger()
                    .Info($"New parent {value.RootGameObject.name} was set to {RootGameObject.name}");
            }
            else
            {
                _parentId = 0;
                RootGameObject.transform.SetParent(null, true);

                if (Entity.hasIdParent)
                {
                    Entity.RemoveIdParent();
                }

                if (EntityParentLine != null)
                {
                    if (EntityParentLine.hasGameObject && EntityParentLine.gameObject.Value != null)
                    {
                        Object.Destroy(EntityParentLine.gameObject.Value);
                    }

                    EntityParentLine.Destroy();
                    EntityParentLine = null;
                }

                LogManager.GetCurrentClassLogger().Info($"Parent was remove to {RootGameObject.name}");
            }

            RootGameObject.transform.ExtSetGlobalScale(scale);
        }

        /// <summary>
        /// UI show object Id 
        /// </summary>
        public UIID Uiid;

        /// <summary>
        /// UI show object managment
        /// </summary>
        public UIObject UiObject;

        private JointBehaviour _jointBehaviour;
        
        #endregion

        #region ABSTRACT METHODS 

        #endregion

        #region PUBLIC METHODS

        public ObjectController(InitObjectParams initObjectParams)
        {
            _context = Contexts.sharedInstance;
            Entity = _context.game.CreateEntity();
            _config = initObjectParams.Config;

            RootGameObject = initObjectParams.RootGameObject;
            gameObject = initObjectParams.Asset;
            photonView = initObjectParams.Photonview;
            AddPhotonTransformView();
            Id = initObjectParams.Id;
            IdObject = initObjectParams.IdObject;
            IdLocation = initObjectParams.IdLocation;
            IdServer = initObjectParams.IdServer;
            Name = initObjectParams.Name;
            WrappersCollection = initObjectParams.WrappersCollection;

            int instanceId = Id;
            this.RegisterMeInLocation(ref instanceId);
            Id = instanceId;
            SetName(Name);

            if (initObjectParams.Parent != null)
            {
                SetParent(initObjectParams.Parent);
            }

            RootGameObject.AddComponent<ObjectBehaviourWrapper>().OwdObjectController = this;

            if (Settings.Instance().Multiplayer)
            {
                Entity.AddPhotonView(photonView);
            }

            if (!initObjectParams.Embedded)
            {
                AddBehaviours();
            }
            
            AddWrapper();
            SaveKinematics();

            Entity.AddId(Id);
            Entity.AddIdServer(IdServer);
            Entity.AddIdObject(IdObject);
            Entity.AddRootGameObject(RootGameObject);
            Entity.AddGameObject(gameObject);

            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

            if (rigidbody != null)
            {
                Entity.AddRigidbody(rigidbody);
            }

            Collider collider = gameObject.GetComponentInChildren<Collider>();

            if (collider)
            {
                Entity.AddCollider(collider);
            }
           
            ApplyGameMode(ProjectData.GameMode, ProjectData.GameMode);
            RequestManager.Instance.StartCoroutine(ExecuteSwitchGameModeDelayedCoroutine());

            if (!initObjectParams.Embedded)
            {
                InitUiId();
            }

            Create();
            
        }

        public IEnumerator ExecuteSwitchGameModeDelayedCoroutine()
        {
            yield return null;         
            ExecuteSwitchGameModeOnObject(ProjectData.GameMode, ProjectData.GameMode);       
        }

        private void Create()
        {
            var monobehaviours = RootGameObject.GetComponentsInChildren<MonoBehaviour>();

            foreach (MonoBehaviour monoBehaviour in monobehaviours)
            {
                if (monoBehaviour == null)
                {
                    continue;
                }

                MethodInfo method = monoBehaviour.GetType()
                    .GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (method != null && method.GetParameters().Length == 0)
                {
                    method.Invoke(monoBehaviour, null);
                }
            }
        }

        private void SaveKinematics()
        {
            var rigidBodies = RootGameObject.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rigidbody in rigidBodies)
            {
                _rigidBodyKinematicsDefaults.Add(rigidbody.transform, rigidbody.isKinematic);
            }
        }

        public void InitUiId()
        {
            if (ProjectData.GameMode == GameMode.View)
            {
                return;
            }

            try
            {
                GameObject uiid = Object.Instantiate(GameObjects.Instance.UIID);
                uiid.GetComponent<UIID>().Init(RootGameObject.transform, this);
                Entity.AddUIID(uiid);

                GameObject uiObject = Object.Instantiate(GameObjects.Instance.UIObject);
                uiObject.GetComponent<UIObject>().Init(RootGameObject.transform, this);
                Entity.AddUIObject(uiObject);


            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        private void AddBehaviours()
        {
            var behaviours = RootGameObject.GetComponentsInChildren<MonoBehaviour>(true);
            bool haveRootInputControl = false;
            bool haveJoints = false;

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }

                bool haveInputControlInObject;
                bool haveInputControlInRoot;
                AddInputControl(behaviour, out haveInputControlInObject, out haveInputControlInRoot);

                if (behaviour is JointPoint)
                {
                    haveJoints = true;
                }

                if (haveInputControlInObject)
                {
                    haveRootInputControl = true;
                }

                AddGameModeSwitchControls(behaviour);
                AddCollidersAware(behaviour);
                AddObjectTransforms(behaviour);
            }

            if (haveJoints)
            {
                AddJointBehaviour();
            }

            if (!haveRootInputControl)
            {
                var inputController = new InputController(this, RootGameObject, photonView,
                    true);
                _inputControllers.Add(-1, inputController);
                LogManager.GetCurrentClassLogger().Info("Added input controller on " + RootGameObject.name);
            }

            Entity.AddInputControls(_inputControllers);
        }

        private void AddJointBehaviour()
        {
            if (RootGameObject.GetComponent<JointBehaviour>() != null)
            {
                return;
            }

            _jointBehaviour = RootGameObject.AddComponent<JointBehaviour>();
            _jointBehaviour.Init();
            LogManager.GetCurrentClassLogger().Info("Added joint behaviour on " + RootGameObject.name);
        }

        public void AddWrapper()
        {
            IWrapperAware wrapperAware = RootGameObject.GetComponentInChildren<IWrapperAware>();

            if (wrapperAware != null)
            {
                Wrapper wrapper = wrapperAware.Wrapper();
                WrappersCollection.Add(Id, wrapper);
                Entity.AddWrapper(wrapper);
                wrapper.InitEntity(Entity);
            }
        }

        private void AddObjectTransforms(MonoBehaviour child)
        {
            var idComponent = child.gameObject.GetComponent<ObjectId>();

            if (idComponent == null)
            {
                return;
            }

            if (Helper.HaveSaveTransform(child))
            {
                GameObject go = child.gameObject;
                var objectTransform = go.AddComponent<ObjectTransform>();
                _objectTransforms.Add(objectTransform);
                LogManager.GetCurrentClassLogger().Info("Added object transform saver on " + go.name);
            }
        }

        private void AddCollidersAware(MonoBehaviour child)
        {
            if (!child.GetType().ImplementsInterface<IColliderAware>())
            {
                return;
            }

            // ReSharper disable once SuspiciousTypeConversion.Global
            IColliderAware colliderAware = (IColliderAware) child;
            _colliderControllers.Add(new ColliderController(colliderAware, child.gameObject));
        }

        private void AddGameModeSwitchControls(MonoBehaviour child)
        {
            if (!child.GetType().ImplementsInterface<ISwitchModeSubscriber>())
            {
                return;
            }

            // ReSharper disable once SuspiciousTypeConversion.Global
            ISwitchModeSubscriber switchMode = (ISwitchModeSubscriber) child;
            _gameModeSwitchControllers.Add(new GameModeSwitchController(switchMode));
        }

        public void AddInputControl(MonoBehaviour behaviour, out bool haveInputControlInObject,
            out bool haveInputControlInRoot)
        {
            haveInputControlInObject = false;
            haveInputControlInRoot = false;

            if (behaviour == null)
            {
                return;
            }

            var idComponent = behaviour.gameObject.GetComponent<ObjectId>();

            if (idComponent == null)
            {
                return;
            }

            if (!Helper.HaveInputs(behaviour))
            {
                return;
            }

            int id = idComponent.Id;
            GameObject go = behaviour.gameObject;
            bool root = false;
            bool mainObject = false;

            if (go == RootGameObject)
            {
                haveInputControlInRoot = true;
                root = true;
            }

            if (go == gameObject)
            {
                haveInputControlInObject = true;
                mainObject = true;
            }

            var inputController = _inputControllers.ContainsKey(id)
                ? _inputControllers[id]
                : new InputController(this, go, photonView,
                    root);

            if (!_inputControllers.ContainsKey(id))
            {
                _inputControllers.Add(id, inputController);
            }

            LogManager.GetCurrentClassLogger().Info("Added input controller on " + go.name);
        }

        public void ApplyGameMode(GameMode newMode, GameMode oldMode)
        {
            foreach (InputController controller in _inputControllers.Values)
            {
                controller.GameModeChanged(newMode);
            }
            
            if (newMode == GameMode.Edit)
            {
                SetKinematicsOn();
            }

            if (newMode == GameMode.Preview || newMode == GameMode.View)
            {
                if (oldMode == GameMode.Edit || oldMode == GameMode.Undefined)
                {
                    SetKinematicsDefaults();
                }
            }      
        }

        public void ExecuteSwitchGameModeOnObject(GameMode newMode, GameMode oldMode)
        {
            foreach (var gameModeSwitchController in _gameModeSwitchControllers)
            {
                try
                {
                    gameModeSwitchController.SwitchGameMode(newMode, oldMode);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Can not apply game mode for {Name}. Error: {e.Message}");
                }
            }
        }

        private void SetKinematicsDefaults()
        {
            foreach (var rigidbody in _rigidBodyKinematicsDefaults)
            {
                rigidbody.Key.GetComponent<Rigidbody>().isKinematic = rigidbody.Value;
            }
        }

        private void SetKinematicsOn()
        {
            foreach (Transform transform in _rigidBodyKinematicsDefaults.Keys)
            {
                transform.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        public Dictionary<int, TransformDT> GetTransforms()
        {
            Dictionary<int, TransformDT> transforms = new Dictionary<int, TransformDT>();

            foreach (var controllersValue in _inputControllers.Values)
            {
                var transform = controllersValue.GetTransform();

                if (transform != null)
                {
                    if (!transforms.Keys.Contains(transform.Id))
                    {
                        transforms.Add(transform.Id, transform.Transform);
                    }
                }
            }

            foreach (ObjectTransform objectTransform in _objectTransforms)
            {
                var transform = objectTransform.GetTransform();

                if (transform != null)
                {
                    if (!transforms.Keys.Contains(transform.Id))
                    {
                        transforms.Add(transform.Id, transform.Transform);
                    }
                }
            }

            return transforms;
        }

        public JointData GetJointData()
        {
            JointData jointData;

            JointBehaviour jointBehaviour = RootGameObject.GetComponent<JointBehaviour>();

            if (jointBehaviour == null)
            {
                return null;
            }

            jointData = new JointData {JointConnetionsData = new Dictionary<int, JointConnetionsData>()};

            foreach (JointPoint jointPoint in jointBehaviour.JointPoints)
            {
                if (jointPoint.IsFree)
                {
                    continue;
                }

                var objectId = jointPoint.gameObject.GetComponent<ObjectId>();

                if (objectId == null)
                {
                    LogManager.GetCurrentClassLogger().Error($"Joint point {jointPoint.gameObject} have no object id");

                    continue;
                }

                var connectedJointPointObjectId = jointPoint.ConnectedJointPoint.gameObject.GetComponent<ObjectId>();

                if (connectedJointPointObjectId == null)
                {
                    LogManager.GetCurrentClassLogger()
                        .Error($"Connected joint point {jointPoint.ConnectedJointPoint.gameObject} have no object id");

                    continue;
                }

                int jointPointId = objectId.Id;
                int connectedObjectInstanceId = jointPoint.ConnectedJointPoint.JointBehaviour.Wrapper.GetInstanceId();
                int connectedObjectJointPointId = connectedJointPointObjectId.Id;

                jointData.JointConnetionsData.Add(jointPointId,
                    new JointConnetionsData
                    {
                        ConnectedObjectInstanceId = connectedObjectInstanceId,
                        ConnectedObjectJointPointId = connectedObjectJointPointId,
                        ForceLocked = jointPoint.IsForceLocked
                    });
            }

            return jointData;
        }

        public SpawnInitParams GetSpawnInitParams()
        {
            Dictionary<int, TransformDT> transforms = new Dictionary<int, TransformDT>();
            JointData jointData = GetJointData();
            var objectsIds = RootGameObject.GetComponentsInChildren<ObjectId>();

            foreach (var objectId in objectsIds)
            {
                if (!transforms.ContainsKey(objectId.Id))
                {
                    transforms.Add(objectId.Id, objectId.gameObject.transform.ToTransformDT());
                }
            }

            var spawn = new SpawnInitParams
            {
                ParentId = _parentId,
                IdLocation = ProjectData.SceneId,
                IdInstance = Id,
                IdObject = IdObject,
                IdServer = IdServer,
                Name = Name,
                Joints = jointData,
                Transforms = transforms
            };

            return spawn;
        }

        public void SetServerId(int id)
        {
            IdServer = id;
            Entity.ReplaceIdServer(id);
        }

        public void SetName(string newName)
        {
            Name = newName;
            Entity.ReplaceName(newName);
        }

        public List<ObjectController> GetChildrens()
        {
            List<ObjectController> result = new List<ObjectController>();

            List<ObjectBehaviourWrapper> childrens =
                RootGameObject.GetComponentsInChildren<ObjectBehaviourWrapper>().ToList();

            foreach (var children in childrens)
            {
                if (children.OwdObjectController.Id == Id)
                {
                    continue;
                }

                result.Add(children.OwdObjectController);
            }

            return result;
        }

        public void Delete()
        {
            List<ObjectBehaviourWrapper> childrens =
                RootGameObject.GetComponentsInChildren<ObjectBehaviourWrapper>().ToList();
            childrens.Reverse();

            JointBehaviour jointBehaviour = RootGameObject.GetComponent<JointBehaviour>();

            if (jointBehaviour != null)
            {
                jointBehaviour.UnLockAndDisconnectPoints();
            }


            foreach (var children in childrens)
            {
                ObjectController type = children.OwdObjectController;
                WrappersCollection.Remove(type.Id);

                foreach (ColliderController colliderController in type._colliderControllers)
                {
                    colliderController.Destroy();
                }

                foreach (InputController inputController in type._inputControllers.Values)
                {
                    inputController.Destroy();
                }

                type.OnDestroy?.Invoke();
                type.Entity?.Destroy();

                if (type.Uiid != null)
                {
                    Object.Destroy(type.Uiid.gameObject);
                }

                if (type.UiObject != null)
                {
                    Object.Destroy(type.UiObject.gameObject);
                }

                if (type.EntityParentLine != null)
                {
                    Object.Destroy(type.EntityParentLine.gameObject.Value);
                    type.EntityParentLine.Destroy();
                }

                type.RootGameObject.DestroyGameObject();
                type.Dispose();
            }

            ProjectData.ObjectsAreChanged = true;
        }

        public void ShowUi()
        {
            UiObject.gameObject.SetActive(true);
            Uiid.gameObject.SetActive(true);
        }

        public void HideUi()
        {
            UiObject.gameObject.SetActive(false);
            Uiid.gameObject.SetActive(false);
        }

        public string GetLocalizedName()
        {
            if (_config == null)
            {
                return "name unknown";
            }

            switch (Settings.Instance().Language)
            {
                case "ru": return _config.i18n.ru;
                case "en": return _config.i18n.en;
            }

            Debug.Log(Settings.Instance().Language);

            return _config.i18n.en;
        }

        #endregion

        private void AddPhotonTransformView()
        {
            if (!Settings.Instance().Multiplayer)
            {
                return;
            }

            PhotonTransformView photonTransformView = gameObject.AddComponent<PhotonTransformView>();
            photonTransformView.m_PositionModel.SynchronizeEnabled = true;

            photonTransformView.m_PositionModel.InterpolateOption =
                PhotonTransformViewPositionModel.InterpolateOptions.Lerp;
            photonTransformView.m_PositionModel.InterpolateLerpSpeed = 10;


            photonTransformView.m_RotationModel.SynchronizeEnabled = true;

            photonTransformView.m_RotationModel.InterpolateOption =
                PhotonTransformViewRotationModel.InterpolateOptions.Lerp;
            photonTransformView.m_RotationModel.InterpolateLerpSpeed = 10;

            photonView.ObservedComponents.Add(photonTransformView);
        }
    }
}
