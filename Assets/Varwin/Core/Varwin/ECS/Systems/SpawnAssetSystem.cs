using System;
using System.Collections.Generic;
using Entitas;
using Varwin.Models.Data;
using UnityEngine;
using Varwin.Data;
using Varwin.Data.ServerData;

namespace Varwin.ECS.Systems
{
    public sealed class SpawnAssetSystem : IExecuteSystem, ICleanupSystem
    {
        private readonly IGroup<GameEntity> _prefabEntities;
        private readonly IGroup<GameEntity> _spawnEntities;
        private bool _haveJoints;
        private static Dictionary<int, JointData> _joints;


        public SpawnAssetSystem(Contexts contexts)
        {
            _spawnEntities = contexts.game.GetGroup(GameMatcher.SpawnAsset);
            _prefabEntities = contexts.game.GetGroup(GameMatcher
                .AllOf(GameMatcher.ServerObject)
            );
        }

        public void Execute()
        {
            if (!ProjectData.ObjectsAreLoaded)
            {
                return;
            }

            _joints = new Dictionary<int, JointData>();
            foreach (GameEntity spawnEntity in _spawnEntities.GetEntities())
            {
                foreach (var prefabEntity in _prefabEntities.GetEntities())
                {
                    if (!spawnEntity.hasSpawnAsset)
                    {
                        continue;
                    }

                    if (!prefabEntity.hasServerObject)
                    {
                        continue;
                    }

                    if (prefabEntity.serverObject.Value.Id != spawnEntity.spawnAsset.SpawnInitParams.IdObject)
                    {
                        continue;
                    }

                    try
                    {
                        SpawnObject(prefabEntity, spawnEntity);
                        Debug.Log($"Object {spawnEntity.spawnAsset.SpawnInitParams.Name} was spawned");
                    }
                    catch (Exception e)
                    {
                        spawnEntity.Destroy();
                        Debug.LogError($"Spawn error! {e.Message}");
                        Debug.LogError($"{e.StackTrace}");

                    }
                }
            }

            if (_haveJoints)
            {
                _haveJoints = false;
                ProjectData.Joints = _joints;
                ProjectDataListener.Instance.RestoreJoints(_joints);
            }
            
        }

        //private void RestoreJoints()
        //{
            
        //}

        public void Cleanup()
        {
            if (!ProjectData.ObjectsAreLoaded)
            {
                return;
            }

            foreach (var entity in _spawnEntities.GetEntities())
            {
                entity.Destroy();
            }
        }

        #region METHODS

        private void SpawnObject(GameEntity prefabEntity, GameEntity spawnEntity)
        {
            GameObject spawnedGameObject = UnityEngine.Object.Instantiate(prefabEntity.gameObject.Value);
            spawnedGameObject.name = spawnedGameObject.name.Replace("(Clone)", "");

            var transforms = spawnEntity.spawnAsset.SpawnInitParams.Transforms;
            var joint = spawnEntity.spawnAsset.SpawnInitParams.Joints;
            var gameObjects = spawnedGameObject.GetComponentsInChildren<ObjectId>();

            if (transforms!= null)
            {
                foreach (ObjectId objectId in gameObjects)
                {
                    if (transforms.ContainsKey(objectId.Id))
                    {
                        transforms[objectId.Id].ToTransformUnity(objectId.gameObject.transform);
                    }
                }
            }
            else
            {
                spawnedGameObject.transform.position = Vector3.zero;
                spawnedGameObject.transform.rotation = Quaternion.identity;
            }
            
            Helper.InitObject(prefabEntity.serverObject.Value.Id, spawnEntity.spawnAsset.SpawnInitParams, spawnedGameObject, prefabEntity.serverObject.Value.Config);

            int instanceId = spawnEntity.spawnAsset.SpawnInitParams.IdInstance;

            if (instanceId == 0 || joint == null)
            {
                return;
            }

            _haveJoints = true;
            _joints.Add(instanceId, joint);
        }

        #endregion
    }
}


