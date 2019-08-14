using System;
using Entitas;

namespace Varwin.ECS.Components.System
{
    /// <summary>
    /// Component Type for spawn
    /// </summary>
    public sealed class TypeComponent : IComponent
    {
        public Type Value;
    }
}
