using UnityEngine;
using Varwin.Commands;

namespace Varwin
{
    public class ParentManager : MonoBehaviour
    {
        public static ParentManager Instance;
        private ObjectController _selectedObjectController;
        private ParentCommand _parentCommand;

        public ParentCommand ParentCommand
        {
            get { return _parentCommand; }
            set { SetNewParentCommand(value); }
        }

        private void SetNewParentCommand(ParentCommand value)
        {
            _parentCommand = value;

            switch (ParentCommand)
            {
                case ParentCommand.DeleteParent:
                    SetParentCommand command = new SetParentCommand(_selectedObjectController, null);
                    ((ICommand)command).Execute();
                    break;
                case ParentCommand.SetNew:

                    break;
            }
        }

        private void Awake()
        {           
            if (Instance == null) 
            {
                Instance = this;
            } 
            else if (Instance != this) 
            {
                Destroy (gameObject);
            }
        }

        public ObjectController GetSelectedParent()
        {
            return _selectedObjectController;
        }

        public void SetSelectedBaseType(ObjectController selected)
        {
            _selectedObjectController = selected;
        }

        public void Invoke(ObjectController newParent)
        {
            switch (ParentCommand)
            {
                case ParentCommand.SetThis:
                    SetParentCommand command = new SetParentCommand(_selectedObjectController, newParent);
                    ((ICommand)command).Execute();
                    ParentCommand = ParentCommand.None;
                    break;
            }
            
        }
    }

    public enum ParentCommand
    {
        None, SetThis, SetNew, DeleteParent
    }
}
