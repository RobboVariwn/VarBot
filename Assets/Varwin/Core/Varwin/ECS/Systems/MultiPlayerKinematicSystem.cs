using Entitas;

namespace Varwin.ECS.Systems
{
    public sealed class MultiPlayerKinematicSystem: IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entities;

        public MultiPlayerKinematicSystem(Contexts contexts){

            _entities = contexts.game.GetGroup(GameMatcher
                .AllOf(GameMatcher.PhotonView, GameMatcher.Rigidbody));

        }
        public void Execute()
        {
            foreach (GameEntity entity in _entities)
            {
                if (entity.rigidbody.Value.useGravity)
                    entity.rigidbody.Value.isKinematic = !entity.photonView.Value.isMine;
            }
        }
    }
}
