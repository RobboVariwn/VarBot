using System;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    [Serializable]
    public class SdkFeature
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public bool Enabled;
        
        public string Key => $"SDKFeature.{Name}";

        public SdkFeature(string name)
        {
            Name = name;
            Enabled = Load();
        }
        
        public bool Load()
        {
            if (!EditorPrefs.HasKey(Key))
            {
                EditorPrefs.SetBool(Key, false);
            }
            return EditorPrefs.GetBool(Key, false);
        }

        public void Save()
        {
            EditorPrefs.SetBool(Key, Enabled);
        }
    }
}