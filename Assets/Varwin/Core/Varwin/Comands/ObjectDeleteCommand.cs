namespace Varwin.Commands
{
    public class DeleteCommand : Command
    {
        private ObjectController _objectController;
        

        /// <summary>
        /// Modify object command
        /// </summary>
        /// <param name="objectController">Object to modify</param>
        public DeleteCommand(ObjectController objectController)
        {
            _objectController = objectController;
            SaveObject(objectController);
            CommandsManager.AddCommand(this);
        }

        protected override void Execute()
        {
            _objectController = GetObject();
            _objectController?.Delete();
        }

        protected override void Undo()
        {
            foreach (var s in SpawnInitParams)
            {
                Spawner.Instance.SpawnAsset(s);
            }
            
        }

    }
}
