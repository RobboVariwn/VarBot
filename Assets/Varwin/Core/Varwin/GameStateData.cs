using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using UnityEngine;
using Varwin.Data;
using Varwin.Types;
using Object = UnityEngine.Object;

namespace Varwin
{
    public static class GameStateData
    {
        private static readonly Dictionary<ObjectController, int> ObjectsIds = new Dictionary<ObjectController, int>();
        private static readonly Dictionary<int, GameObject> Prefabs = new Dictionary<int, GameObject>();
        private static readonly Dictionary<int, PrefabObject> PrefabsData = new Dictionary<int, PrefabObject>();
        private static Dictionary<string, string> _objectTypesList = new Dictionary<string, string>();
        private static readonly Dictionary<int, GameEntity> ObjectTypeEntities = new Dictionary<int, GameEntity>();
        private static readonly Dictionary<int, Sprite> ObjectIcons = new Dictionary<int, Sprite>();
        private static readonly List<int> EmbeddedObjects = new List<int>();
        private static readonly WrappersCollection WrappersCollection = new WrappersCollection();
        private static LogicInstance _logicInstance = null;
        private static GameEntity _logicEntity;

        public static event Action ObjectsCollectionChanged;
        
        public static void Dispose(this ObjectController self)
        {
            if (ObjectsIds.ContainsKey(self))
            {
                ObjectsIds.Remove(self);
                ObjectsCollectionChanged?.Invoke();
            }

            if (ObjectTypeEntities.ContainsKey(self.Id))
            {
                ObjectTypeEntities.Remove(self.Id);
            }

            GC.Collect();
        }

        public static void RegisterMeInLocation(this ObjectController self, ref int instanceId)
        {
            if (instanceId == 0)
            {
                int newId;

                if (ObjectsIds.Count == 0)
                {
                    newId = 1;
                }
                else
                {
                    newId = ObjectsIds.Values.ToList().Max() + 1;
                }

                instanceId = newId;
                self.SetName(instanceId.ToString());
            }

            ObjectsIds.Add(self, instanceId);
            ObjectTypeEntities.Add(instanceId, self.Entity);
            ObjectsCollectionChanged?.Invoke();
        }

        public static ObjectController GetObjectInLocation(int idObject)
        {
            foreach (ObjectController value in ObjectsIds.Keys)
            {
                if (value.Id == idObject)
                {
                    return value;
                }
            }

            return null;
        }

        public static WrappersCollection GetWrapperCollection() => WrappersCollection;

        public static void ClearObjects()
        {
            var objectControllers = GetObjectsInLocation();

            var withOutParent = new List<ObjectController>();

            foreach (ObjectController objectController in objectControllers)
            {
                if (objectController.Parent != null)
                {
                    continue;
                }

                withOutParent.Add(objectController);
            }

            foreach (ObjectController o in withOutParent)
            {
                o.Delete();
            }

            ObjectsIds.Clear();
            ObjectsCollectionChanged?.Invoke();
            LogManager.GetCurrentClassLogger().Info($"Location objects was deleted");
        }

        public static int GetNextObjectIdInLocation()
        {
            if (ObjectsIds.Count == 0)
            {
                return 1;
            }

            int newId = ObjectsIds.Values.ToList().Max() + 1;

            return newId;
        }

        public static List<ObjectController> GetObjectsInLocation() => ObjectsIds.Keys.ToList();

        public static void AddPrefabGameObject(int objectId, Object asset, PrefabObject data)
        {
            if (!Prefabs.ContainsKey(objectId))
            {
                Prefabs.Add(objectId, (GameObject) asset);
            }
            
            if (!PrefabsData.ContainsKey(objectId))
            {
                PrefabsData.Add(objectId, data);
            }
        }
        
        public static void AddObjectIcon(int objectId, Sprite sprite)
        {
            if (!ObjectIcons.ContainsKey(objectId))
            {
                ObjectIcons.Add(objectId, sprite);
            }
        }

        public static void AddToEmbeddedList(int objectId)
        {
            if (!EmbeddedObjects.Contains(objectId))
            {
                EmbeddedObjects.Add(objectId);
            }
        }

        public static GameObject GetPrefabGameObject(int objectId) => Prefabs.ContainsKey(objectId) ? Prefabs[objectId] : null;
        public static PrefabObject GetPrefabData(int objectId) => PrefabsData.ContainsKey(objectId) ? PrefabsData[objectId] : null;
        public static List<PrefabObject> GetPrefabsData() => PrefabsData.Values.ToList();
        public static Sprite GetObjectIcon(int objectId) => ObjectIcons.ContainsKey(objectId) ? ObjectIcons[objectId] : null;
        public static bool IsEmbedded(int objectId) => EmbeddedObjects.Contains(objectId);

        public static void RefreshLogic(LogicInstance logicInstance, byte[] assemblyBytes)
        {
            DestroyLogic();
            WrappersCollection.Clear();
            GameEntity logicEntity = Contexts.sharedInstance.game.CreateEntity();
            logicEntity.AddType(null);
            logicEntity.AddLogic(logicInstance);
            logicEntity.AddAssemblyBytes(assemblyBytes);
            _logicEntity = logicEntity;
            LogManager.GetCurrentClassLogger().Info("Logic was refreshed");
        }

        private static void DestroyLogic()
        {
            if (_logicEntity != null)
            {
                _logicEntity.logic.Value = null;
                _logicEntity.Destroy();
            }
        }

        public static GameEntity GetLocationEntity() => _logicEntity;

        public static List<GameEntity> GetEntitiesInLocation(int groupId)
        {
            var result = new List<GameEntity>();

            foreach (var value in ObjectsIds)
            {
                result.Add(value.Key.Entity);
            }

            return result;
        }

        public static GameEntity GetEntity(int id) => ObjectTypeEntities.ContainsKey(id) ? ObjectTypeEntities[id] : null;

        public static void GameModeChanged(GameMode newMode, GameMode oldMode)
        {
            var objects = GetAllObjects();

            foreach (ObjectController o in objects)
            {
                o.ApplyGameMode(newMode, oldMode);
                o.ExecuteSwitchGameModeOnObject(newMode, oldMode);
            }

        }

        private static List<ObjectController> GetAllObjects() => ObjectsIds.Keys.ToList();

        public static void AddAssembly(string assemblyKey, string assemblyPath)
        {
            if (_objectTypesList.ContainsKey(assemblyKey))
            {
                return;
            }

            _objectTypesList.Add(assemblyKey, assemblyPath);
        }


        public static List<string> GetAssembliesPathes()
        {
            List<string> assembliesPathes = new List<string>();

            foreach (string value in _objectTypesList.Values)
            {
                assembliesPathes.Add(value);
            }

            return assembliesPathes;
        }

        public static LogicInstance GetLogic() => _logicInstance;

        public static void ClearLogic()
        {
            _logicInstance?.Clear();
        }

        public static void SetLogic(LogicInstance logicInstance)
        {
            _logicInstance = logicInstance;
        }
    }
}