

namespace Varwin.ECS.Systems.Loader
{
    public sealed class LoaderSystems : Feature
    {
        public LoaderSystems(Contexts contexts)
        {
            if (!ProjectData.ObjectsAreLoaded)
            {
                Add(new LoadCounterSystem(contexts));
            }          
        }
    }
}
