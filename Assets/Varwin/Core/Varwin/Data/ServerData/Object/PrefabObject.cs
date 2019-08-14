

// ReSharper disable once CheckNamespace

using System;

namespace Varwin.Data
{

    public class PrefabObject : IJsonSerializable
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public bool Embedded { get; set; }
        public Config Config { get; set; }
        public string Resources { get; set; }

        public string BundleResource => Resources + "/bundle";
        public string AndroidBundleResource => Resources + "/android_bundle";
        public string ConfigResource => Resources + "/bundle.json";
        public string IconResource => Resources + "/bundle.png";
        
        
    }

  

}