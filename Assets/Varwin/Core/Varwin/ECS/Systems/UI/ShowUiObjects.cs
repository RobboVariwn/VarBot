using Entitas;
using UnityEngine;

namespace Varwin.ECS.Systems.UI
{
    public sealed class ShowUiObjects : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entitiesUiMenu;
        private readonly IGroup<GameEntity> _entitiesLines;


        public ShowUiObjects(Contexts contexts)
        {
            _entitiesUiMenu = contexts.game.GetGroup(GameMatcher.AnyOf(GameMatcher.UIID, GameMatcher.UIObject));
            _entitiesLines = contexts.game.GetGroup(GameMatcher.LineRenderer);
        }

        public void Execute()
        {
            foreach (var entity in _entitiesUiMenu.GetEntities())
            {
                if (entity.hasUIID && ProjectData.GameMode != GameMode.View)
                {
                    entity.uIID.Value.SetActive(true);
                }

                if (entity.hasUIObject && ProjectData.GameMode != GameMode.View && ProjectData.GameMode != GameMode.Preview)
                {
                    entity.uIObject.Value.SetActive(true);
                }
            }

            foreach (var entity in _entitiesLines.GetEntities())
            {
                if (entity.hasLineRenderer && ProjectData.GameMode != GameMode.View)
                    entity.lineRenderer.Value.enabled = true;
            }
        }

        
    }
}


