using System.Collections.Generic;
using Valve.VR.InteractionSystem;

namespace Varwin.VRInput.SteamVR
{
    public class SteamVRPointerControllerComponent : PointerControllerComponent
    {
        private UiPointer _menuPointer;
        public bool IsMenuOpened;
        public ControllerInteraction.ControllerHand Hand { get; protected set; }        
        
        public bool ShowUIPointer
        {
            set => _menuPointer.IsPriority = value;
            get => _menuPointer.IsPriority;
        }

        private void Awake()
        {
            TeleportPointer teleport = gameObject.AddComponent<TeleportPointer>();
            _menuPointer = gameObject.AddComponent<UiPointer>();
            _pointers = new List<PointerController.IBasePointer> {_menuPointer, teleport};

            foreach (PointerController.IBasePointer pointer in _pointers)
            {
                pointer.Init();
            }

            Hand = (ControllerInteraction.ControllerHand)GetComponent<Hand>().handType;
        }

        private void Update()
        {
            UpdatePointers();
        }

        protected override void UpdatePointers()
        {
            if (IsMenuOpened)
            {
                if (_menuPointer.IsActive())
                {
                    _menuPointer.Toggle(false);
                }

                return;
            }
            
            base.UpdatePointers();
        }


        protected override List<PointerController.IBasePointer> _pointers { get; set; }
    }
}
