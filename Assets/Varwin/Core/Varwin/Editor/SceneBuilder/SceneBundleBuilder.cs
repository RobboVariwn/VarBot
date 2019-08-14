#define VRMAKER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if VRMAKER
using Ionic.Zip;
#endif
using Varwin.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Varwin.Editor
{
    public class SceneBundleBuilder
    {
        private string _bakedDirectory;
        private string _bundleDirectory;
        private string _objectName;
        private string _prefabPath;
        private string _zipFilePath;
        private string _iconPath;
        private Object _createdObject;
        private bool _mobileReady;

        private const int SaveIter = 50;

        public string BuildAndShow(WorldDescriptor descriptor, byte[] icon)
        {
#if VRMAKER

            SaveCurrentScene();
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetActiveScene();
            Debug.Log(scene.path);

            _bakedDirectory = Application.dataPath.Replace("Assets", "BakedSceneTemplates");
            if (!Directory.Exists(_bakedDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_bakedDirectory);
                }
                catch
                {
                    Debug.LogError("Can't create directory \"" + _bakedDirectory + "\"");
                    EditorUtility.DisplayDialog("Can't create directory",
                        "Can't create directory \"" + _bakedDirectory + "\"",
                        "OK");
                    throw;
                }
            }
            
            _bundleDirectory = Application.dataPath.Replace("Assets", "AssetBundles");
            if (!Directory.Exists(_bundleDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_bundleDirectory);
                }
                catch
                {
                    Debug.LogError("Can't create directory \"" + _bundleDirectory + "\"");
                    EditorUtility.DisplayDialog("Can't create directory",
                        "Can't create directory \"" + _bundleDirectory + "\"",
                        "OK");
                    throw;
                }
            }
            
            _objectName = descriptor.Name;
            _prefabPath = scene.path;
            _zipFilePath = _bakedDirectory + $"/{_objectName}.vwst";
            _mobileReady = SdkSettings.MobileFeature.Enabled && descriptor.MobileReady;

            BuildAssetBundles();

            CreateFolders(descriptor);

            string bundleFile = _bundleDirectory + "/" + _objectName;
            string tempBundlePath = _bakedDirectory + "/bundle";
            string tempBundleManifest = tempBundlePath + ".manifest";
            CopyBundles(bundleFile, tempBundlePath, tempBundleManifest);

            var androidBundleFile = _bundleDirectory + "/android_" + _objectName;
            var tempAndroidBundlePath = _bakedDirectory + "/android_bundle";
            var tempAndroidBundleManifest = tempAndroidBundlePath + ".manifest";
            if (_mobileReady)
            {   
                CopyBundles(androidBundleFile, tempAndroidBundlePath, tempAndroidBundleManifest);
            }

            string tempInstallJsonFile = _bakedDirectory + "/install.json";
            string tempBundleJsonFile = _bakedDirectory + "/bundle.json";
            CreateConfig(descriptor, tempInstallJsonFile, tempBundleJsonFile);

            string tempBundleIconFile = _bakedDirectory + "/bundle.png";
            CreateIcon(tempBundleIconFile, icon);

            List<string> filesToZip = new string[]
            {
                tempBundlePath,
                tempBundleManifest,
                tempBundleJsonFile,
                tempInstallJsonFile,
                tempBundleIconFile
            }.ToList();

            if (_mobileReady)
            {
                filesToZip.Add(tempAndroidBundlePath);
                filesToZip.Add(tempAndroidBundleManifest);
            }

            return ZipFiles(filesToZip);
#endif
        }

        private void CreateIcon(string path, byte[] icon)
        {
            File.WriteAllBytes(path, icon);
        }

        private void SaveCurrentScene()
        {
            try
            {
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
            catch
            {
                Debug.LogError("Cannot apply changes in prefab");
            }
        }

        private Action OnCompile;

        private void BuildAssetBundles()
        {
            Debug.Log("Build asset bundles to " + _bakedDirectory);
            DateTime start = DateTime.Now;

            if (!Directory.Exists(_bakedDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_bakedDirectory);
                }
                catch
                {
                    Debug.LogError("Can't create directory \"" + _bakedDirectory + "\"");
                    EditorUtility.DisplayDialog("Can't create directory",
                        "Can't create directory \"" + _bakedDirectory + "\"",
                        "OK");
                    throw;
                }
            }

            var buildTargets = new List<BuildTarget>()
            {
                BuildTarget.StandaloneWindows64, 
                BuildTarget.Android 
            };
            BuildAssetBundles(buildTargets);

            TimeSpan time = DateTime.Now - start;
            Debug.Log("Build asset bundles to " + _bundleDirectory + " successful! Time = " + time.Seconds + "  sec.");
        }

        private void BuildAssetBundles(List<BuildTarget> buildTargets)
        {
            var currentTarget = EditorUserBuildSettings.activeBuildTarget;
            if (buildTargets.Contains(currentTarget))
            {
                var buildHandler = GetBuildBundlesHandler(currentTarget);
                buildHandler?.Invoke();
                buildTargets.Remove(currentTarget);
            }

            foreach (var target in buildTargets)
            {
                var buildHandler = GetBuildBundlesHandler(target);
                buildHandler?.Invoke();
            }
        }
        
        private Action GetBuildBundlesHandler(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows64: 
                    return BuildWindowsBundles;
                case BuildTarget.Android: 
                    return BuildAndroidBundles;
                default:
                    Debug.LogError($"Build Target \"{buildTarget}\" is not supported");
                    return null;
            }
        }
        
        private void BuildWindowsBundles()
        {
            BuildTarget activeBuildTarget = BuildTarget.StandaloneWindows64;
            AssetBundleBuild assetBundleBuild = default(AssetBundleBuild);

            assetBundleBuild.assetNames = new string[] {_prefabPath};
            assetBundleBuild.assetBundleName = _objectName;

            BuildPipeline.BuildAssetBundles(_bundleDirectory,
                new[] {assetBundleBuild},
                BuildAssetBundleOptions.UncompressedAssetBundle,
                activeBuildTarget);
        }
        
        private void BuildAndroidBundles()
        {
            if (SdkSettings.MobileFeature.Enabled && _mobileReady)
            {
                BuildTarget activeBuildTarget = BuildTarget.Android;
                AssetBundleBuild assetBundleBuild = default(AssetBundleBuild);

                assetBundleBuild.assetNames = new string[] {_prefabPath};
                assetBundleBuild.assetBundleName = "android_" + _objectName;

                BuildPipeline.BuildAssetBundles(_bundleDirectory,
                    new[] {assetBundleBuild},
                    BuildAssetBundleOptions.UncompressedAssetBundle,
                    activeBuildTarget);
            }
        }

        private void CopyBundles(string bundlePath, string bakedBundlePath, string bakedBundleManifest)
        {
            string bundleManifestPath = bundlePath + ".manifest";

            File.Copy(bundlePath, bakedBundlePath);
            File.Copy(bundleManifestPath, bakedBundleManifest);
        }

        private void CreateFolders(WorldDescriptor descriptor)
        {
            Debug.Log("Bake location " + _bakedDirectory);

            if (Directory.Exists(_bakedDirectory))
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(_bakedDirectory);
            }

            catch
            {
                Debug.LogError("Can not create location directory...");

                return;
            }
        }

        private void CreateConfig(WorldDescriptor descriptor, string installJsonPath, string configJsonPath)
        {
            Debug.Log("Core version is " + VarwinVersionInfo.VarwinVersion);
            
 
            string builtAt = $"{DateTimeOffset.UtcNow:s}Z";
            if (DateTimeOffset.TryParse(descriptor.BuiltAt, out DateTimeOffset builtAtDateTimeOffset))
            {
                builtAt = $"{builtAtDateTimeOffset.UtcDateTime:s}Z";
            }
            
            var installConfig = new CreateSceneTemplateWindow.InstallConfig
            {
                Name = descriptor.Name, 
                Guid = descriptor.Guid,
                RootGuid = descriptor.RootGuid,
                Author = new CreateSceneTemplateWindow.Author()
                {
                    Name = descriptor.AuthorName,
                    Email = descriptor.AuthorEmail,
                    Url = descriptor.AuthorUrl,
                },
                BuiltAt = builtAt,
                License = new CreateSceneTemplateWindow.License()
                {
                    Code = descriptor.LicenseCode,
                    Version = descriptor.LicenseVersion,
                },
                MobileReady = SdkSettings.MobileFeature.Enabled && descriptor.MobileReady,
                SdkVersion = VarwinVersionInfo.VersionNumber
            };

            var sceneConfig = new CreateSceneTemplateWindow.SceneConfig
            {
                name = descriptor.Name,
                description = descriptor.Description,
                image = descriptor.Image,
                assetBundleLabel = descriptor.AssetBundleLabel,
                dllNames = new string[0]
            };

            File.WriteAllText(installJsonPath, installConfig.ToJson());
            File.WriteAllText(configJsonPath, sceneConfig.ToJson());
        }

        private string ZipFiles(List<string> files)
        {
#if VRMAKER
            try
            {
                using (ZipFile loanZip = new ZipFile())
                {
                    loanZip.AddFiles(files, false, "");
                    loanZip.Save(_zipFilePath);
                }

                foreach (string file in files)
                {
                    File.Delete(file);
                }

                Debug.Log($"Zip was created!");

                return _zipFilePath;
            }
            catch (Exception e)
            {
                Debug.LogError($"Can not zip files! " + e.Message);
            }

            return default(string);

#endif
        }
    }
}
