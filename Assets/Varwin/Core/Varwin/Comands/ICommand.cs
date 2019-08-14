using System.Collections.Generic;
using Varwin.Data;

namespace Varwin.Commands
{
    public interface ICommand
    {
        void Execute();

        void Undo();
    }

    public abstract class Command : ICommand
    {
        private int _saveId;
        private int _saveIdGroup;
        protected List<SpawnInitParams> SpawnInitParams;

        public bool Executed { get; private set; }
        public bool Undid { get; private set; }
        protected abstract void Execute();

        protected abstract void Undo();

        void ICommand.Execute()
        {
            Execute();
            Executed = true;
            Undid = false;
        }

        void ICommand.Undo()
        {
            Undo();
            Executed = false;
            Undid = true;
        }

        public void SaveObject(ObjectController o)
        {
            _saveId = o.Id;
            _saveIdGroup = o.IdLocation;
            SpawnInitParams = new List<SpawnInitParams>();

            var baseTypes = o.RootGameObject.GetComponentsInChildren<ObjectBehaviourWrapper>();
            foreach (var wrapper in baseTypes)
            {
                ObjectController objectController = wrapper.OwdObjectController;
                SpawnInitParams.Add(objectController.GetSpawnInitParams());
            }
        }

        public void SaveObjectTree(ObjectController o)
        {
            _saveId = o.Id;
            _saveIdGroup = o.IdLocation;
        }

        public void SaveObject(SpawnInitParams param)
        {
            int id = 0;
            if (param.IdLocation != 0)
            {
                id = GameStateData.GetNextObjectIdInLocation();
            }

            _saveId = id;
            _saveIdGroup = param.IdLocation;
        }

        

        public ObjectController GetObject()
        {
            if (_saveIdGroup != 0)
            {
                return GameStateData.GetObjectInLocation(_saveId);
            }

            return null;
        }
    }
}
