using Entitas;
using Varwin.Data.ServerData;

namespace Varwin.ECS.Systems.Group
{
    public sealed class LogicUpdate : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entities;
        private readonly byte[] _assemblyBytes;
        private readonly int _locationId;
        public LogicUpdate(Contexts contexts, byte[] assemblyBytes, int locationId)
        {
            _entities = contexts.game.GetGroup(GameMatcher.Logic);
            _assemblyBytes = assemblyBytes;
            _locationId = locationId;
        }

        public void Execute()
        {
            var location = ProjectData.ProjectStructure.Scenes.GetProjectScene(_locationId);
            location.AssemblyBytes = _assemblyBytes;

            foreach (GameEntity entity in _entities)
            {
                entity.ReplaceAssemblyBytes(_assemblyBytes);
            }
        }
    }
}
