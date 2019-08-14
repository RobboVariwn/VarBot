using System.Collections.Generic;
using Varwin.Data;

namespace Varwin.Editor
{
    /// <summary>
    /// File model class
    /// </summary>
    public class CreateObjectModel : IJsonSerializable
    {
        public string Path;
        public string ObjectName;
        public string LocalizedName;
        public string LocalizedNameRU;
        public bool IsGrabbable;
        public bool IsPhysicsOn;
        public bool MobileReady;

        public string Guid;
        public string ObjectFolder;
        public string ModelFolder;
        public string ModelImportPath;
        public string PrefabPath;
        public string ClassName;

        public float BiggestSideSize = 1.0f;
        public float Mass = 1.0f;
        
        public string Tags;

        public bool Skip = false;
        
        public class AssetExtras
        {
            public string Author;
            public string License;
            public string Source;
            public string Title;
        }
        
        public AssetExtras Extras;
    }

    public class CreateObjectTempModel : IJsonSerializable
    {
        public List<CreateObjectModel> Objects;
        public bool BuildNow;
        
        public static string TempFilePath = "temp_bake.txt";
    }

}
