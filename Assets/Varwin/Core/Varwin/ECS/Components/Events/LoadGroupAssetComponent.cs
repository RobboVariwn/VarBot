using Entitas;
using Varwin.Data;
using Varwin.Data.ServerData;

namespace Varwin.ECS.Components.Events
{
    public sealed class LoadGroupAssetComponent : IComponent
    {
        public ObjectDto Value;

        /// <summary>
        /// Photon Id
        /// </summary>
        public int IdPhoton;

    }
}
