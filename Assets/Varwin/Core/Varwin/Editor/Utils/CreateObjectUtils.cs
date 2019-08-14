using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Varwin.Data;
using Varwin.Public;
using Object = UnityEngine.Object;

namespace Varwin.Editor
{
    public static class CreateObjectUtils
    {
        public static void AddComponent(GameObject gameObject, string prefabPath, string assemblyName,
            string componentName)
        {
            Type objectType = Type.GetType(assemblyName);

            if (objectType == null)
            {
                Assembly assembly = Assembly.Load(assemblyName);
                objectType = assembly.GetType($"Varwin.Types.{assemblyName}.{componentName}");
            }

            UnityEngine.Object instanceRoot = PrefabUtility.InstantiatePrefab(gameObject);
            if (instanceRoot != null)
            {
                AddComponentToPrefab(gameObject, prefabPath, objectType);
                UnityEngine.Object.DestroyImmediate(instanceRoot);
            }
            else
            {
                gameObject.AddComponent(objectType);
            }
        }

        public static void AddComponentToPrefab<T>(GameObject prefabObject, string prefabPath) where T : Component
        {
            AddComponentToPrefab(prefabObject, prefabPath, typeof(T));
        }

        public static void AddComponentToPrefab(GameObject prefabObject, string prefabPath, Type objectType)
        {
            if (prefabObject.GetComponent(objectType) == null)
            {
                GameObject t = PrefabUtility.LoadPrefabContents(prefabPath);
                t.AddComponent(objectType);
                PrefabUtility.SaveAsPrefabAsset(t, prefabPath);
                PrefabUtility.UnloadPrefabContents(t);
            }
        }

        public static void AddObjectId(GameObject go)
        {
            var objectIdComponent = go.GetComponent<ObjectId>();
            if (objectIdComponent == null)
            {
                objectIdComponent = go.AddComponent<ObjectId>();
                objectIdComponent.Id = go.GetInstanceID();
            }
        }

        public static void SetupObjectIds(GameObject go)
        {
            var components = go.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var c in components)
            {
                AddObjectId(c.gameObject);
            }
        }
        
        public static GameObject GetPrefab(GameObject go)
        {
            return PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);
        }

        public static void ApplyPrefabInstanceChanges(GameObject go)
        {
            var objectFromSource = PrefabUtility.GetCorrespondingObjectFromSource(go);
            GameObject prefabInstanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);

            if (prefabInstanceRoot != null && objectFromSource != null)
            {
                SetupObjectIds(go);
                PrefabUtility.SaveAsPrefabAssetAndConnect(prefabInstanceRoot, prefabPath, InteractionMode.AutomatedAction);
            }
            else
            {
                var vod = go.GetComponent<VarwinObjectDescriptor>();
                if (vod != null)
                {
                    prefabPath = vod.Prefab;
                    SetupObjectIds(go);
                    PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.AutomatedAction);
                }
                else
                {
                    throw new Exception("VarwinObjectDescriptor is not found in the prefab");
                }
            }
        }

        public static void RevertPrefabInstanceChanges(GameObject go)
        {
            PrefabType prefabType = PrefabUtility.GetPrefabType(go);

            bool flag = prefabType == PrefabType.DisconnectedModelPrefabInstance ||
                        prefabType == PrefabType.DisconnectedPrefabInstance;
            GameObject parentPrefab = GetPrefab(go);

            if (parentPrefab != null)
            {
                if (flag)
                {
                    PrefabUtility.ReconnectToLastPrefab(parentPrefab);
                }

                PrefabUtility.RevertPrefabInstance(parentPrefab);
            }
        }

        public static List<ObjectId> GetObjectIdDuplicates(GameObject go)
        {
            var objectIds = go.GetComponentsInChildren<ObjectId>(true);
            var duplicates = new List<ObjectId>();
            foreach (var objectId in objectIds)
            {
                if (objectIds.GroupBy(x => x.Id).Any(g => g.Count() > 1 && g.Key == objectId.Id))
                {
                    duplicates.Add(objectId);
                }
            }
            return duplicates;
        }
        
        public static bool ContainsObjectIdDuplicates(GameObject go)
        {
            var objectIds = go.GetComponentsInChildren<ObjectId>(true);
            return objectIds.GroupBy(x => x.Id).Any(g => g.Count() > 1);
        }

        public static void SafeDestroy(Object obj)
        {
            if (Application.isEditor)
            {
                Object.DestroyImmediate(obj);
            }
            else
            {
                Object.Destroy(obj);
            }
        }

        public static bool ValidateAssemblies(ref List<string> invalidTypes, GameObject gameObject, string objectName)
        {
            string json = File.ReadAllText($"Assets/Objects/{objectName}/{objectName}.asmdef");
            var asmdef = json.JsonDeserialize<AssemblyDefinitionData>();

            var monoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();

            foreach (MonoBehaviour monoBehaviour in monoBehaviours)
            {
                if (monoBehaviour == null)
                {
                    continue;
                }
                
                Type type = monoBehaviour.GetType();
                string assemblyName = type.Assembly.GetName().Name;

                if (assemblyName != asmdef.name && !asmdef.references.Contains(assemblyName) && !assemblyName.StartsWith("UnityEngine"))
                {
                    invalidTypes.Add(type.Name);
                }
            }

            return invalidTypes.Count == 0;
        }
    }

    class AssemblyDefinitionData : IJsonSerializable
    {
        public string name;
        public string[] references;
        public string[] includePlatforms;
        public string[] excludePlatforms;
    }
    
}
