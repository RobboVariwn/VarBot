using System;
using System.Reflection;
using Varwin.WWW;

namespace Varwin.Data
{
    public class ResponseLoadAssembly : IResponse
    {
        public Assembly LoadedAssembly;
        public Type CompiledType;
        public int Milliseconds;
    }
}
