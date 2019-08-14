using System;
using UnityEngine;

namespace Varwin.Public
{
    public class VarwinObjectDescriptor : MonoBehaviour
    {
        [TextArea] public string ConfigBlockly;

        [TextArea] public string ConfigAssetBundle;

        public Texture2D Icon;

        public bool DeveloperMode;

        public bool ShowDebug;

        public string Name;

        public string Prefab;

        public string Guid;
        public string RootGuid;

        public bool Embedded;

        public bool MobileReady;

        public string AuthorName;
        public string AuthorEmail;
        public string AuthorUrl;

        public string LicenseCode;
        public string LicenseVersion;
        
        public string BuiltAt;

        public bool CurrentVersionWasBuilt;

        private void Reset()
        {
            Validate();
        }

        private void OnValidate()
        {
            Validate();
        }
        
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Guid))
            {
                return;
            }
            
            if (string.IsNullOrWhiteSpace(RootGuid))
            {
                if (string.IsNullOrWhiteSpace(Guid))
                {
                    Debug.LogError($"Object {gameObject.name} has empty guid. New guid was generated.", gameObject);
                    Guid = System.Guid.NewGuid().ToString();
                }
                
                RootGuid = Guid;
            }

            if (string.IsNullOrWhiteSpace(AuthorName))
            {
                AuthorName = "Anonymous";
            }

            if (string.IsNullOrWhiteSpace(LicenseCode))
            {
                LicenseCode = "cc-by";
            }

            if (string.IsNullOrWhiteSpace(LicenseVersion))
            {
                LicenseVersion = "4.0";
            }
        }

        public void PreBuild()
        {
            CurrentVersionWasBuilt = true;
            BuiltAt = DateTimeOffset.Now.ToString();
            
            Validate();
        }
    }

}