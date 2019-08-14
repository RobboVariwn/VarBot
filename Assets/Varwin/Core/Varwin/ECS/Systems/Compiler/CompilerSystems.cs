namespace Varwin.ECS.Systems.Compiler
{
    public sealed class CompilerSystems : Feature
    {
        public CompilerSystems(Contexts contexts)
        {
//            Add(new RuntimeCompilerSystem(contexts));
            Add(new LoadAssemblySystem(contexts));
            Add(new TypeAnalizatorSystem(contexts));
            Add(new AddLogicSystem(contexts));
        }
    }
}
