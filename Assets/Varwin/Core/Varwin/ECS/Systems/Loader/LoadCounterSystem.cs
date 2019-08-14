using Entitas;

namespace Varwin.ECS.Systems.Loader
{
    /// <summary>
    /// Calculate when objects loaded
    /// </summary>
    public sealed class LoadCounterSystem : IExecuteSystem 
    {
        private readonly IGroup<GameEntity> _loadCounter;

        public LoadCounterSystem(Contexts contexts)
        {
            _loadCounter = contexts.game.GetGroup(GameMatcher.LoadObjectsCounter);
        }

        public void Execute()
        {
            foreach (GameEntity entity in _loadCounter)
            {
                if (entity.loadObjectsCounter.PrefabsCount != entity.loadObjectsCounter.PrefabsLoaded || ProjectData.ObjectsAreLoaded)
                {
                    continue;
                }

                ProjectData.ObjectsAreLoaded = true;
                entity.loadObjectsCounter.LoadComplete = true;
                ProjectData.ObjectsWasLoaded();
            }
        }

       
    }
}
