using System;
using System.Collections.Generic;
using Entitas;
using NLog;
using Varwin.Data;
using Varwin.WWW;

namespace Varwin.ECS.Systems.Compiler
{
    public sealed class LoadAssemblySystem : ReactiveSystem<GameEntity>
    {
        public LoadAssemblySystem(Contexts context) : base(context.game)
        {

        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context) => context.CreateCollector(GameMatcher.AssemblyBytes);

        protected override bool Filter(GameEntity entity) => entity.hasAssemblyBytes & entity.hasType;

        protected override void Execute(List<GameEntity> entities)
        {

            foreach (var entity in entities)
            {
                var request = new RequestLoadAssembly(entity.assemblyBytes.Value);
                LogManager.GetCurrentClassLogger().Info($"Load assembly started... Entity: {entity}");
                request.OnFinish += response =>
                {
                    ResponseLoadAssembly responseLoadAssembly = (ResponseLoadAssembly) response;
                    Type compiledType = responseLoadAssembly.CompiledType;
                    entity.ReplaceType(compiledType);
                    LogManager.GetCurrentClassLogger()
                        .Info(
                            $"<Color=Green><b>Loading assembly successful! Time = {responseLoadAssembly.Milliseconds} ms.</b></Color>");
                };
                request.OnError += s =>
                {
                    //entity.ReplaceType(null);
                };
            }
        }
    }
}
