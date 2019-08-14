using System;
using System.Collections.Generic;
using UnityEngine;
using Varwin.VRInput.SteamVR;

namespace Varwin.VRInput
{
    public class SteamVRPointerController : PointerController
    {
        public SteamVRPointerController()
        {
            Managers =
                new ComponentWrapFactory<PointerManager, SteamVRPointerManager, SteamVRPointerControllerComponent>();
            //Pointer = new ComponentWrapFactory<InputPointer, SteamVRInputPointer, SteamVRInputPointerComponent>();

            ProjectData.SceneLoaded += ProjectDataOnSceneLoaded;
        }

        private void ProjectDataOnSceneLoaded()
        {
            /* InputAdapter.Instance.PlayerController.Nodes.RightHand.GameObject
                 .AddComponent<SteamVRPointerControllerComponent>();
 
             InputAdapter.Instance.PlayerController.Nodes.LeftHand.GameObject
                 .AddComponent<SteamVRPointerControllerComponent>();*/

            // RightManager = Pointers.GetFrom(InputAdapter.Instance.PlayerController.Nodes.RightHand.GameObject);
        }

        public override bool IsMenuOpened
        {
            set
            {
                if (Left == null || Right == null)
                {
                    return;
                }
                
                Left.IsMenuOpened = value;
                Right.ShowUIPointer = value;
            }
        }

        public class SteamVRPointerManager : PointerManager, IInitializable<SteamVRPointerControllerComponent>
        {
            private SteamVRPointerControllerComponent _controllerComponent;

            public override bool IsMenuOpened
            {
                get => _controllerComponent.IsMenuOpened;
                set => _controllerComponent.IsMenuOpened = value;
            }

            public override bool ShowUIPointer
            {
                get => _controllerComponent.ShowUIPointer;
                set => _controllerComponent.ShowUIPointer = value;
            }

            public void Init(SteamVRPointerControllerComponent monoBehaviour)
            {
                _controllerComponent = monoBehaviour;

                switch (_controllerComponent.Hand)
                {
                    case ControllerInteraction.ControllerHand.Left:
                        InputAdapter.Instance.PointerController.SetLeftManager(this);

                        break;
                    case ControllerInteraction.ControllerHand.Right:
                        InputAdapter.Instance.PointerController.SetRightManager(this);

                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}