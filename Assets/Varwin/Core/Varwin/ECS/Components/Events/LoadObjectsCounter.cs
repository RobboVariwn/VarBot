using System;
using Entitas;
// ReSharper disable All

namespace Varwin.ECS.Components.Events
{
    public sealed class LoadObjectsCounter : IComponent
    {
        public int PrefabsCount;
        public int PrefabsLoaded;
        public bool LoadComplete;
    }
}
