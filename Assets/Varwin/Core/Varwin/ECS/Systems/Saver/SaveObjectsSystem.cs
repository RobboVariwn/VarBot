using System.Collections.Generic;
using Entitas;
using NLog;
using Varwin.Data;
using Varwin.Data.ServerData;
using Varwin.WWW;
using ObjectData = Varwin.Data.ServerData.ObjectData;

namespace Varwin.ECS.Systems.Saver
{
    public sealed class SaveObjectsSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _allEntities;

        public SaveObjectsSystem(Contexts contexts)
        {
            _allEntities = contexts.game.GetGroup(GameMatcher.AllOf(
                GameMatcher.RootGameObject,
                GameMatcher.IdServer, GameMatcher.Name, GameMatcher.Id, GameMatcher.IdObject));
        }

        public void Execute()
        {

            Dictionary<int, ObjectDto> dictionary = new Dictionary<int, ObjectDto>();
            List<ObjectDto> result = new List<ObjectDto>();
            ProjectData.Joints = new Dictionary<int, JointData>();

            foreach (GameEntity entity in _allEntities)
            {
                var objectController = GameStateData.GetObjectInLocation(entity.id.Value);
                var transforms = objectController.GetTransforms();
                var joints = objectController.GetJointData();

                ObjectDto newTreeObjectDto = new ObjectDto
                {
                    Id = entity.idServer.Value,
                    InstanceId = entity.id.Value,
                    ObjectId = entity.idObject.Value,
                    Data = new Data.ServerData.ObjectData
                    {
                        Transform = transforms,
                        JointData = joints
                    },
                    SceneObjects = new List<ObjectDto>()
                };

                if (joints != null)
                {
                    ProjectData.Joints.Add(entity.id.Value, joints);
                }

                if (entity.hasIdParent)
                {
                    newTreeObjectDto.ParentId = entity.idParent.Value;
                }
                else
                {
                    newTreeObjectDto.ParentId = null;
                }

                dictionary.Add(entity.id.Value, newTreeObjectDto);
            }

            foreach (var treeObject in dictionary.Values)
            {
                if (treeObject.ParentId != null)
                {
                    dictionary[treeObject.ParentId.Value].SceneObjects.Add(treeObject);
                }
                else
                {
                    result.Add(treeObject);
                }
            }

            LocationObjectsDto objectsData = new LocationObjectsDto
            {
                SceneId = ProjectData.SceneId,
                SceneObjects = result
            };

            var request = new RequestApi(ApiRoutes.SaveSceneObjectsRequest, RequestType.Post, objectsData.ToJson());
            request.OnFinish += response =>
            {
                ResponseApi responseApi = (ResponseApi)response;

                if (Helper.IsResponseGood(responseApi))
                {
                    string json = responseApi.Data.ToString();
                    LocationObjectsDto data = json.JsonDeserialize<LocationObjectsDto>();
                    LogManager.GetCurrentClassLogger().Info($"SceneObjects on location {ProjectData.SceneId} was saved!");
                    Data.ServerData.Scene location = ProjectData.ProjectStructure.Scenes.GetProjectScene(ProjectData.SceneId);
                    location.UpdateProjectSceneObjects(data.SceneObjects);
                    ProjectData.OnSave?.Invoke();
                }

            };
            request.OnError += response =>
            {
                //ToDo OnError
            };


        }

    }
}
