using Varwin.Data;

namespace Varwin.Models.Data
{
    public class LogicBlockError : IJsonSerializable
    {
        public int SceneId { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class LogicBlockRuntimeList : IJsonSerializable
    {
        public int SceneId { get; set; }
        public string[] Blocks { get; set; }
    }
}