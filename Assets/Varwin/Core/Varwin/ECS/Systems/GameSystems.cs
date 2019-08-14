using Varwin.ECS.Systems.Loader;
using Varwin.ECS.Systems.Compiler;
using Varwin.ECS.Systems.UI;

namespace Varwin.ECS.Systems
{
    public sealed class GameSystems : Feature
    {
        public GameSystems(Contexts contexts)
        {
            Add(new SpawnAssetSystem(contexts));
            Add(new ZoneControlSystem(contexts));
            Add(new CompilerSystems(contexts));
            Add(new ShowParentLinesSystem(contexts));
            Add(new LogicSystems(contexts));

            /*
            if (Settings.Instance().Multiplayer)
            {
                Add(new MultiPlayerKinematicSystem(contexts));
            }
            */
        }
    }
}
