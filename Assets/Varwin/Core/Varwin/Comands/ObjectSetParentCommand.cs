namespace Varwin.Commands
{
    public class SetParentCommand : Command
    {
        private ObjectController _objectController;
        private readonly ObjectController _newParent;
        private readonly ObjectController _oldParent;

        /// <summary>
        /// Set object parent command
        /// </summary>
        /// <param name="objectController">Object to modify</param>
        /// <param name="newParent">New parent object</param>
        public SetParentCommand(ObjectController objectController, ObjectController newParent)
        {
            _objectController = objectController;
            _newParent = newParent;
            _oldParent = objectController.Parent;
            SaveObject(objectController);
            CommandsManager.AddCommand(this);
        }

        protected override void Execute()
        {
            _objectController = GetObject();
            _objectController.Parent = _newParent;
        }

        protected override void Undo()
        {
            ObjectController o = GetObject();
            o.Parent = _oldParent;
        }

    }
}
