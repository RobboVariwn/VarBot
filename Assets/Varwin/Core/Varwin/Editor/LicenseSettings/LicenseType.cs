using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class LicenseType
    {
        public string Code;

        public string Name;

        public string[] Versions;

        public string Link;
        
        public LicenseType(string code, string name, string[] versions, string link)
        {
            Code = code;
            Name = name;
            Versions = versions;
            Link = link;
        }

        public string GetLink(string version)
        {
            return string.Format(Link, version);
        }
    }
}