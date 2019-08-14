using Varwin.Data;
using Varwin.Models.Data;
using Varwin.Types;

// ReSharper disable once CheckNamespace
namespace Varwin.Data 
{
    public class UpdateGroupObjectParams
    {
        public int IdServer;
        public int IdInstance;
        public TypeConfig TypeConfig;
        public string Name;
        public TransformDT Transform;
    }
}
