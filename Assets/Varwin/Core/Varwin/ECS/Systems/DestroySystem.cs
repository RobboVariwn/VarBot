using Entitas;

namespace Varwin.ECS.Systems
{
    public sealed class DestroySystem : IExecuteSystem
    {
        private readonly Contexts _contexts;
        private readonly IGroup<GameEntity> _entities;


        public DestroySystem(Contexts contexts)
        {
            _contexts = contexts;
            _entities = _contexts.game.GetGroup(GameMatcher.Destroy);
        }

        public void Execute()
        {
            foreach (var entity in _entities.GetEntities())
            {
                entity.Destroy();
            }
        }
    }
}


