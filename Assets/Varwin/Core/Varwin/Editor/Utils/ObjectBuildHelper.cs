using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public static class ObjectBuildHelper
    {
        public static bool IsValidTypeName(string typeName, bool showMessage = false)
        {
            if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(typeName))
            {
                if (showMessage)
                {
                    EditorUtility.DisplayDialog("Error!",
                        $"Invalid type name \"{typeName}\"", "OK");
                }

                return false;
            }

            if (!Regex.IsMatch(typeName, @"^[a-zA-Z0-9_]+$"))
            {
                if (showMessage)
                {
                    EditorUtility.DisplayDialog("Error!",
                        $"Wrong type name {typeName}; Varwin type name can contain only English letters and numbers.", "OK");
                }

                return false;
            }

            return true;
        }
        
        public static string ConvertToNiceName(string name)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;  
            TextInfo textInfo = cultureInfo.TextInfo;

            return ObjectNames.NicifyVariableName(textInfo.ToTitleCase(name.Replace("_", " ").Replace("-", " ")));
        }

        public static string EscapeString(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);

                    var ret = writer.ToString();

                    return ret.Substring(1, ret.Length - 2);
                }
            }
        }

        public static List<string> GetExistingObjectsNames()
        {
            var list = new List<string>();
            var path = Path.Combine(Application.dataPath, "Objects");
            if (Directory.Exists(path))
            {
                var existingObjectDirectory = new DirectoryInfo(path);
                list = existingObjectDirectory.GetDirectories().Select(x => x.Name).ToList();
            }
            return list;
        }
    }
}