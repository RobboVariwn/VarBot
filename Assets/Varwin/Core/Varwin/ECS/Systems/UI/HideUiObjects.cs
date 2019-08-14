using Entitas;

namespace Varwin.ECS.Systems.UI
{
    public sealed class HideUiObjects : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entitiesUi;
        private readonly IGroup<GameEntity> _entitiesLines;


        public HideUiObjects(Contexts contexts)
        {
            _entitiesUi = contexts.game.GetGroup(GameMatcher.AnyOf(GameMatcher.UIID, GameMatcher.UIObject));
            _entitiesLines = contexts.game.GetGroup(GameMatcher.LineRenderer);
        }

        public void Execute()
        {
            if (!ProjectData.ObjectsAreLoaded)
            {
                return;
            }
            
            foreach (var entity in _entitiesUi)
            {
                entity.uIID.Value.SetActive(false);
                entity.uIObject.Value.SetActive(false);
            }

            foreach (var entity in _entitiesLines.GetEntities())
            {
                entity.lineRenderer.Value.enabled = false;
            }
        }
    }
}


