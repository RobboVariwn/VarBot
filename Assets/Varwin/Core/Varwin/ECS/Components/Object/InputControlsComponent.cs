using System.Collections.Generic;
using Entitas;

namespace Varwin.ECS.Components
{
    public sealed class InputControlsComponent : IComponent
    {
        public Dictionary<int, InputController> Values;
    }
}
