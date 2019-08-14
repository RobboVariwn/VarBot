/*using Entitas;
using NLog;
using TMPro;
using UnityEngine.UI;


namespace Varwin.ECS.Systems.Loader
{
    public sealed class SceneLoaderSystem : IInitializeSystem
    {
        private readonly Contexts _contexts;
        private readonly int _locationId;
        private readonly TMP_Text _debugText;
        private readonly Logger _logger;

        public SceneLoaderSystem(Contexts context, TMP_Text debugText, int sceneId)
        {
            _contexts = context;
            _debugText = debugText;
            
            _logger = LogManager.GetCurrentClassLogger();
            _locationId = sceneId;
        }

        public void Initialize()
        {
            _logger.Info("Scene load started");
            LoaderAdapter.LoadLocation(_locationId, _debugText);
        }

       
    }
}*/