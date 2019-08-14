using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public static class LicenseSettings
    {
        public const string CreativeCommonsLink = @"https://creativecommons.org/licenses/";
        
        public static readonly List<LicenseType> Licenses = new List<LicenseType>
        {
            new CreativeCommonsLicenseType("cc-by",       "Attribution"),
            new CreativeCommonsLicenseType("cc-by-sa",    "Attribution Share Alike"),
            new CreativeCommonsLicenseType("cc-by-nd",    "Attribution No Derivatives"),
            new CreativeCommonsLicenseType("cc-by-nc",    "Attribution Non-Commercial"),
            new CreativeCommonsLicenseType("cc-by-nc-sa", "Attribution Non-Commercial Share Alike"),
            new CreativeCommonsLicenseType("cc-by-nc-nd", "Attribution Non-Commercial No Derivatives"),
        };
    }
}