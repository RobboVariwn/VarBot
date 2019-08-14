using UnityEngine;
using Varwin.ECS.Systems;

namespace Varwin.ECS
{
    public class EntitasRunSystems : MonoBehaviour
    {
        private GameSystems _systems;
        
        private void Awake()
        {
            if (_systems != null)
            {
                Destroy(this);
            }
        }
        
        private void Start()
        {
            _systems = new GameSystems(Contexts.sharedInstance);
            _systems.Initialize();
        }

        private void Update()
        {
            _systems.Execute();
            _systems.Cleanup();
        }
    }
}
