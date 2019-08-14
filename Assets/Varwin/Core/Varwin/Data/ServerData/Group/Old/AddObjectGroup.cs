
// ReSharper disable once CheckNamespace
namespace Varwin.Data 
{
    /// <summary>
    /// Add object to group or spawn new object request
    /// </summary>
    public class AddObjectGroup : IJsonSerializable
    {
        public int GroupId { get; set; }
        public int ObjectId { get; set; }
        public int InstanceId { get; set; }
        public ObjectData Data { get; set; }
    }
}
