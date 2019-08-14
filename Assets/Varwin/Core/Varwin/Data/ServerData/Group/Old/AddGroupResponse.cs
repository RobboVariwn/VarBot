

// ReSharper disable once CheckNamespace
namespace Varwin.Data 
{
    public class AddGroupResponse : IJsonSerializable
    {
        public int Id { get; set; }
        public int InstanceId { get; set; }
        public string Name { get; set; }
    }
}
