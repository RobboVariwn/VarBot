using Entitas;
using UnityEngine;

namespace Varwin.ECS.Components
{
    public sealed class ChildrenParentTransformComponent : IComponent
    {
        public Transform Children;
        public Transform Parent;
    }
}
