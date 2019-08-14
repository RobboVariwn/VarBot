using Varwin.Models.Data;
using Varwin.Types;

// ReSharper disable once CheckNamespace
namespace Varwin.Data
{
    public class UpdateObjectPost : IJsonSerializable
    {
        public string Name { get; set; }
        public int InstanceId { get; set; }
        public ObjectData Data { get; set; }
    }

    public class ObjectData
    {
        public TypeConfig TypeConfig;
        public TransformDT Transform;
    }
}
