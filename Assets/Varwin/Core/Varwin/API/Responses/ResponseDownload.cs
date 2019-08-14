using System.Collections.Generic;
using Varwin.WWW;

namespace Varwin.Data
{
    public class ResponseDownLoad : IResponse
    {
        public List<string> LocalFilesPathes;
        public Dictionary<string, List<byte>> DownLoadedData;
    }
}
