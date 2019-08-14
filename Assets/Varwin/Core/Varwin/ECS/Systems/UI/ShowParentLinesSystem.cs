using Entitas;
using UnityEngine;

namespace Varwin.ECS.Systems.UI
{
    public sealed class ShowParentLinesSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entitiesLines;

        public ShowParentLinesSystem(Contexts contexts)
        {
            _entitiesLines = contexts.game.GetGroup(GameMatcher.AnyOf(GameMatcher.LineRenderer, GameMatcher.ChildrenParentTransform));
        }

        public void Execute()
        {
            foreach (var entity in _entitiesLines.GetEntities())
            {
                Line(entity.childrenParentTransform.Children.position, entity.childrenParentTransform.Parent.position,
                    entity.lineRenderer.Value);
            }
        }

        void Line(Vector3 children, Vector3 parent, LineRenderer lineRenderer)
        {
            if (lineRenderer == null) return;
            if (!lineRenderer.enabled) return;

            int _seg = 2;
            Vector3[] _vP = new Vector3[2];

            _vP[0] = children;
            _vP[1] = parent;

            for (int i = 0; i < _seg; i++)
            {
                float t = i / (float)_seg;
                lineRenderer.SetVertexCount(_seg);
                lineRenderer.SetPositions(_vP);
            }

        }
    }
}


