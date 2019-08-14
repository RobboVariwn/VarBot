using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Varwin.Data.ServerData
{
    public class LocationObjectsDto : IJsonSerializable
    {
        public int SceneId { get; set; }
        public List<ObjectDto> SceneObjects { get; set; }
    }

    public class ObjectDto : IJsonSerializable
    {
        public int Id { get; set; }
        public int InstanceId { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int? ParentId { get; set; }
        public int ObjectId { get; set; }
        public ObjectData Data { get; set; }
        public List<ObjectDto> SceneObjects { get; set; }
    }



}