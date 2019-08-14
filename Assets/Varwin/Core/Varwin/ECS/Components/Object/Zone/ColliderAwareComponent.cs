using Entitas;
using Varwin.Public;

namespace Varwin.ECS.Components
{
    public class ColliderAwareComponent : IComponent
    {
        public IColliderAware Value;
    }
}