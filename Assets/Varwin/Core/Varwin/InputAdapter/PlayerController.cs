using UnityEngine;

namespace Varwin.VRInput
{
    public abstract class PlayerController
    {
        public PlayerTracking Tracking;
        public PlayerRigInitializer RigInitializer;
        public PlayerNodes Nodes;
        public PlayerControls Controls;

        public delegate void TeleportHandler(Vector3 position);
        public event TeleportHandler PlayerTeleported;

        public abstract class PlayerTracking
        {
            public abstract GameObject GetBoundaries(Transform transform);

            public delegate void TrackingEvent(PlayerNodes.Node node);

            public event TrackingEvent TrackingLost;
            public event TrackingEvent TrackingRestored;
        }

        public abstract class PlayerRigInitializer
        {
            public abstract GameObject InitializeRig();
        }

        public class ControllerReferenceArgs
        {
            public ControllerInteraction.ControllerHand hand;
        }

        public virtual void Teleport(Vector3 position)
        {
            Debug.Log($"Teleport player to {position}");
            PlayerTeleported?.Invoke(position);
        }

        public abstract class PlayerNodes
        {
            public Node Rig { get; protected set; }
            public Node Head { get; protected set; }
            public ControllerNode LeftHand { get; protected set; }
            public ControllerNode RightHand { get; protected set; }

            public abstract void Init(GameObject rig);

            public abstract ControllerNode GetControllerReference(GameObject controller);

            public virtual ControllerNode GetControllerReference(ControllerInteraction.ControllerHand hand)
            {
                switch (hand)
                {
                    case ControllerInteraction.ControllerHand.Left: return LeftHand;
                    case ControllerInteraction.ControllerHand.Right: return RightHand;
                }

                return null;
            }
            
            public abstract class Node
            {
                public virtual GameObject GameObject { get; protected set; }
                public virtual Transform Transform { get; protected set; }

                public virtual void SetNode(GameObject gameObject)
                {
                    GameObject = gameObject;
                    Transform = gameObject.transform;
                }
            }

            public abstract class ControllerNode : Node
            {
                public abstract ControllerInteraction.ControllerSelf Controller { get; protected set; }
                public abstract PointerController.PointerManager PointerManager { get; protected set; }
            }

            public abstract GameObject GetModelAliasController(GameObject controllerEventsGameObject);

            public abstract ControllerInteraction.ControllerHand
                GetControllerHand(GameObject controllerEventsGameObject);


            public abstract string GetControllerElementPath(
                ControllerInteraction.ControllerElements findElement,
                ControllerInteraction.ControllerHand controllerHand,
                bool b);
        }


        public abstract class PlayerControls
        {
        }

        public abstract void Init(GameObject rig);
    }
}
