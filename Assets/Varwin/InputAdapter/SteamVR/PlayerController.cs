using System;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Object = UnityEngine.Object;

namespace Varwin.VRInput
{
    public class SteamVRPlayerController : PlayerController
    {
        public SteamVRPlayerController()
        {
            Tracking = new SteamVRPlayerTracking();
            RigInitializer = new SteamVRPlayerRigInitializer();
            Nodes = new SteamVRPlayerNodes();
            Controls = new SteamVRPlayerControls();
        }

        public override void Teleport(Vector3 position)
        {
            Transform player = InputAdapter.Instance.PlayerController.Nodes.Rig.Transform;

            if (player == null)
            {
                Debug.LogError("Can not teleport! Player not found");

                return;
            }

            Transform head = InputAdapter.Instance.PlayerController.Nodes.Head.Transform;

            if (head == null)
            {
                Debug.LogError("Can not teleport! Head not found");

                return;
            }


            Vector3 playerPosition = position;
            Vector3 headDelta = player.position - head.position;

            headDelta.y = 0;
            playerPosition = playerPosition + headDelta;
            player.position = playerPosition;

            base.Teleport(position);
        }

        public override void Init(GameObject rig)
        {
            Nodes.Init(rig);

            ((SteamVRPlayerControls) InputAdapter.Instance.PlayerController.Controls).Init(rig.transform);
        }


        public class SteamVRPlayerNodes : PlayerNodes
        {
            public SteamVRPlayerNodes()
            {
                Rig = new SteamVRNode();
                Head = new SteamVRNode();
                LeftHand = new SteamVRControllerNode();
                RightHand = new SteamVRControllerNode();
            }

            public override void Init(GameObject rig)
            {
                GameObject go = rig
                    .GetComponentInChildren<Camera>()
                    .gameObject;
                Head?.SetNode(go);
                Rig.SetNode(rig);

                LeftHand = new SteamVRControllerNode();
                RightHand = new SteamVRControllerNode();
            }

            public override ControllerNode GetControllerReference(GameObject controller)
            {
                var hand = controller.GetComponent<Hand>();

                switch (hand.handType)
                {
                    case SteamVR_Input_Sources.LeftHand: return LeftHand;
                    case SteamVR_Input_Sources.RightHand: return RightHand;
                }

                return null;
            }

            private class SteamVRNode : Node
            {
            }

            private class SteamVRControllerNode : ControllerNode
            {
                public override void SetNode(GameObject gameObject)
                {
                    Controller = InputAdapter.Instance.ControllerInteraction.Controller.GetFrom(gameObject);
                    PointerManager = InputAdapter.Instance.PointerController.Managers.GetFrom(gameObject);
                    base.SetNode(gameObject);
                }

                public override ControllerInteraction.ControllerSelf Controller { get; protected set; }
                public override PointerController.PointerManager PointerManager { get; protected set; }
            }

            public override GameObject GetModelAliasController(GameObject controllerEventsGameObject) =>
                throw new NotImplementedException();

            public override ControllerInteraction.ControllerHand GetControllerHand(
                GameObject controllerEventsGameObject) =>
                (ControllerInteraction.ControllerHand) controllerEventsGameObject.GetComponentInChildren<Hand>()
                    .handType;

            public override string GetControllerElementPath(
                ControllerInteraction.ControllerElements findElement,
                ControllerInteraction.ControllerHand controllerHand,
                bool b) => throw new NotImplementedException();
        }

        private class SteamVRPlayerRigInitializer : PlayerRigInitializer
        {
            private GameObject _rig;

            public override GameObject InitializeRig()
            {
                //_rig = Resources.Load<GameObject>("OpW Player Rig");
                _rig = Resources.Load<GameObject>("PlayerRig/STVRPlayer");

                return _rig;
            }
        }

        public class SteamVRPlayerControls : PlayerControls
        {
            private Transform _player;
            private const int TurnAngle = 45;

            public void Init(Transform playerRig)
            {
                _player = playerRig;

                ((SteamVRControllerInput) InputAdapter.Instance.ControllerInput).TurnLeftPressed +=
                    (sender, args) => { Turn(true); };

                ((SteamVRControllerInput) InputAdapter.Instance.ControllerInput).TurnRightPressed +=
                    (sender, args) => { Turn(false); };
            }

            public void Turn(bool isLeft)
            {
                if (isLeft)
                {
                    RotatePlayer(-TurnAngle);
                }
                else
                {
                    RotatePlayer(TurnAngle);
                }
            }

            private void RotatePlayer(float angle)
            {
                Vector3 headPos = InputAdapter.Instance.PlayerController.Nodes.Head.Transform.position;
                Transform player = InputAdapter.Instance.PlayerController.Nodes.Rig.Transform;

                Vector3 headDelta = player.position - headPos;
                headDelta.y = 0;
                Vector3 aHead = new Vector3(headPos.x, player.position.y, headPos.z);
                _player.position = aHead + Quaternion.AngleAxis(angle, Vector3.up) * headDelta;
                _player.Rotate(_player.up, angle);
            }
        }

        private class SteamVRPlayerTracking : PlayerTracking
        {
            public override GameObject GetBoundaries(Transform transform) => transform.gameObject;

            /* public class VRTKSDKManagerFabric : UnityComponentFabric<SDKManager, VRTKSDKManager, VRTK_SDKManager>
             {
             }

             public class VRTKSDKManager : SDKManager, IInitable<VRTK_SDKManager>
             {
                 private VRTK_SDKManager _sdkManager;

                 public override GameObject gameObject => _sdkManager.gameObject;

                 public void Init(VRTK_SDKManager monoBehaviour)
                 {
                     _sdkManager = monoBehaviour;
                 }
             }*/
        }
    }
}