using System.Collections.Generic;
using UnityEngine;
using Photon;
using Varwin.Data;
using Varwin.Models.Data;
using Varwin.Commands;

namespace Varwin 
{
    class Spawner : PunBehaviour
    {
        public static Spawner Instance;

        private void Awake()
        {           
            if (Instance == null) 
            {
                Instance = this;
            } 
            else if (Instance != this) 
            {
                Destroy (gameObject);
            }
        }
 
        public void SpawnAsset(SpawnInitParams param)
        {
            if (Settings.Instance().Multiplayer)
            {
                int idPhoton = PhotonNetwork.AllocateViewID();
                photonView.RPC("SpawnAssetRpc", PhotonTargets.All, param.ToJson(), idPhoton);
            }
            else
            {
                SpawnAsset(param, 0);
            }
        }
        
        /// <summary>
        /// Spawn object with full defined transform
        /// </summary>
        /// <param name="objectId">Object Id</param>
        /// <param name="targetTransform">Transform which define object's position, rotation and scale</param>
        public void SpawnObject(int objectId, Transform targetTransform)
        {
            if (objectId == 0)
            {
                return;
            }
            
            ProjectData.ObjectsAreChanged = true;
            
            var transforms = new Dictionary<int, TransformDT>();
            var prefabGo = GameStateData.GetPrefabGameObject(objectId);
            var rootId = prefabGo.GetComponent<ObjectId>().Id;
            var rootTransform = targetTransform.ToTransformDT();
            transforms.Add(rootId, rootTransform);
            
            var spawnParams = new SpawnInitParams()
            {
                IdObject = objectId,
                IdLocation = ProjectData.SceneId,
                Transforms = transforms
            };
            
            if (ParentManager.Instance != null && ParentManager.Instance.ParentCommand == ParentCommand.SetNew)
            {
                spawnParams.ParentId = ParentManager.Instance.GetSelectedParent().Id;
            }

            ICommand command = new SpawnCommand(spawnParams);
            command.Execute();
        }
        
        /// <summary>
        /// Spawn object in position
        /// </summary>
        /// <param name="objectId">Object Id</param>
        /// <param name="position">Object's spawn position</param>
        public void SpawnObject(int objectId, Vector3 position)
        {
            if (objectId == 0)
            {
                return;
            }
            
            ProjectData.ObjectsAreChanged = true;
            
            var transforms = new Dictionary<int, TransformDT>();
            var prefabGo = GameStateData.GetPrefabGameObject(objectId);
            var rootId = prefabGo.GetComponent<ObjectId>().Id;
            var rootTransform = new TransformDT() {
                PositionDT = new Vector3DT(position),
                RotationDT = new QuaternionDT(Quaternion.identity),
                ScaleDT = new Vector3DT(Vector3.one)
            };
            transforms.Add(rootId, rootTransform);
            
            var spawnParams = new SpawnInitParams()
            {
                IdObject = objectId,
                IdLocation = ProjectData.SceneId,
                Transforms = transforms
            };
            
            if (ParentManager.Instance != null && ParentManager.Instance.ParentCommand == ParentCommand.SetNew)
            {
                spawnParams.ParentId = ParentManager.Instance.GetSelectedParent().Id;
            }

            ICommand command = new SpawnCommand(spawnParams);
            command.Execute();
        }

        public void SpawnAsset(SpawnInitParams paramObject, int idPhoton)
        {
            if (GameStateData.GetWrapperCollection().Exist(paramObject.IdInstance))
            {
                Debug.Log($"Object with id {paramObject.IdInstance} already exist!");
                return;
            }
            
            Contexts contexts = Contexts.sharedInstance;
            var entity = contexts.game.CreateEntity();
            entity.AddSpawnAsset(paramObject, idPhoton);
        }

        //Parent will be lost!!!
        [PunRPC]
        public void SpawnAssetRpc(string paramObject, int idPhoton)
        {
            SpawnInitParams param = paramObject.JsonDeserialize<SpawnInitParams>(); 
            Contexts contexts = Contexts.sharedInstance;
            var entity = contexts.game.CreateEntity();
            entity.AddSpawnAsset(param, idPhoton);
        }
    }   
}