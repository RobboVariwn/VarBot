
namespace Varwin.Public
{
    public interface IColliderAware
    {
        void OnObjectEnter(Wrapper[] wrappers);
        void OnObjectExit(Wrapper[] wrappers);
    }
}
