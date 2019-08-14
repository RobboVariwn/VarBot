using System;
using System.Collections.Generic;
using System.Linq;
using Entitas;
using NLog;

namespace Varwin.ECS.Systems.Compiler
{
    public sealed class TypeAnalizatorSystem : ReactiveSystem<GameEntity>
    {
        public TypeAnalizatorSystem(Contexts context) : base(context.game)
        {

        }

        protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context) => context.CreateCollector(GameMatcher.Type);

        protected override bool Filter(GameEntity entity) => entity.hasType;

        protected override void Execute(List<GameEntity> entities)
        {

            foreach (var entity in entities)
            {

                Type compiledType = entity.type.Value;

                if (compiledType == null)
                {
                    return;
                }

                if (compiledType.GetInterfaces().Contains(typeof(ILogic)))
                {
                    LogManager.GetCurrentClassLogger().Info("Logic type was updated");
                    entity.ReplaceChangeGroupLogic(true);
                }

            }
        }
    }
}
