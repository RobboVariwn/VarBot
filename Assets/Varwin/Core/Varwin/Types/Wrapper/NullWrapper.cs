using UnityEngine;

namespace Varwin
{
    public class NullWrapper : Wrapper
    {
        public NullWrapper(GameEntity entity) : base(entity)
        {
        }

        public NullWrapper(GameObject gameObject) : base(gameObject)
        {
        }
    }
}
