using Varwin.Data;

namespace Varwin.Commands
{
    public class SpawnCommand : Command
    {
        private readonly SpawnInitParams _spawnInitParams;

        /// <summary>
        /// Modify object command
        /// </summary>
        /// <param name="spawnInitParams"></param>
        public SpawnCommand(SpawnInitParams spawnInitParams)
        {
            _spawnInitParams = spawnInitParams;
            CommandsManager.AddCommand(this);
        }

        protected override void Execute()
        {
            Spawner.Instance.SpawnAsset(_spawnInitParams);
            SaveObject(_spawnInitParams);
            ProjectData.ObjectsAreChanged = true;
        }

        protected override void Undo()
        {
            ObjectController o = GetObject();
            o?.Delete();
        }

    }
}
