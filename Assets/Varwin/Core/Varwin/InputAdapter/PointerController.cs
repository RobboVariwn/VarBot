using System.Collections.Generic;
using UnityEngine;

namespace Varwin.VRInput
{
    public abstract class PointerController
    {
        public IMonoComponent<PointerManager> Managers;
        public PointerManager Left { get; protected set; }
        public PointerManager Right { get; protected set; }

        public void SetLeftManager(PointerManager manager)
        {
            Left = manager;
        }
        
        public void SetRightManager(PointerManager manager)
        {
            Right = manager;
        }

        /*public static InputPointer RightPointer { get; private set; }

       // public IMonoComponent<InputPointer> Pointer;

        public static void SetRightPointer(InputPointer go)
        {
            RightPointer = go;
        }
        */

        public interface IBasePointer
        {
            /// <summary>
            /// Check if pointer can be clicked
            /// </summary>
            bool CanClick();

            /// <summary>
            /// Check if pointer can be shown
            /// </summary>
            bool CanToggle();

            /// <summary>
            /// Turn rendering on/off
            /// </summary>
            void Toggle(bool value);

            /// <summary>
            /// Turn rendering on/off
            /// </summary>
            void Toggle();

            /// <summary>
            /// Button pressed
            /// </summary>
            void Click();

            /// <summary>
            /// Pointer initialization
            /// </summary>
            void Init();

            /// <summary>
            /// Update pointer's state for the current frame
            /// </summary>
            void UpdateState();

            /// <summary>
            /// Returns whether pointer is active or not
            /// </summary>
            /// <returns></returns>
            bool IsActive();
        }

        public abstract bool IsMenuOpened { set; }
        
        public abstract class PointerManager
        {
            public abstract bool IsMenuOpened { get; set; }
            
            public abstract bool ShowUIPointer { get; set; }
        }

        

        /* public abstract class InputPointer
        {
            public Transform transform;
          
            public abstract Transform GetOrigin();

            public abstract void Toggle(bool b);

            
        }*/
    }
    
    public abstract class PointerControllerComponent : MonoBehaviour
    {
        protected abstract List<PointerController.IBasePointer> _pointers { get; set; }
        public PointerController.IBasePointer CurrentPointer;

        protected virtual void UpdatePointers()
        {
            foreach (PointerController.IBasePointer pointer in _pointers)
            {
                pointer.UpdateState();

                if (!pointer.CanClick())
                {
                    if (!pointer.CanToggle())
                    {
                        continue;
                    }

                    TryChangePointer(pointer);

                    return;
                }

                pointer.Click();

                return;
            }

            TryChangePointer(null);
        }

        protected virtual bool TryChangePointer(PointerController.IBasePointer pointer)
        {
            if (CurrentPointer == pointer)
            {
                return false;
            }
                
            CurrentPointer?.Toggle();
            CurrentPointer = pointer;
            CurrentPointer?.Toggle();

            return true;
        }
    }
}
