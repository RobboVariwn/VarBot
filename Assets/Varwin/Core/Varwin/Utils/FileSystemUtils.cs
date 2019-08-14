using System;
using System.IO;
using UnityEngine;

namespace Varwin
{
    public static class FileSystemUtils
    {
        /// <summary>
        /// Safe method to create directory
        /// </summary>
        /// <param name="path">Directory path</param>
        /// <returns>Directory info</returns>
        public static DirectoryInfo CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            
            string[] folders = path.Split(new[] {'/', '\\'}, StringSplitOptions.RemoveEmptyEntries);
            string prevPath = string.Empty;
            
            foreach (string folder in folders)
            {
                string currPath = prevPath + folder;

                if (!Directory.Exists(currPath))
                {
                    Directory.CreateDirectory(currPath);
                }

                prevPath = currPath + '/';
            }

            return new DirectoryInfo(prevPath);
        }

        public static string GetFilesPath(bool isMobile, string destination)
        {
                       
            
#if UNITY_EDITOR || !UNITY_ANDROID

            return Application.persistentDataPath + "/" + destination;        
#endif            
            string basicMobilePath =  "/storage/emulated/0/Varwin/" + destination;

            if (!Directory.Exists(basicMobilePath))
            {
                CreateDirectory(basicMobilePath);
            }

            return basicMobilePath;
        }
        
    }
}
