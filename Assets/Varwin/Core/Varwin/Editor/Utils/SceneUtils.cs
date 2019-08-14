using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public static class SceneUtils
    {
        public static List<GameObject> GetAllObjectsInScene(bool includeInactive = false)
        {
            List<GameObject> objectsInScene = new List<GameObject>();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (!includeInactive)
                {
                    if (IsHiddenOrNotEditable(go))
                        continue;

                    if (!EditorUtility.IsPersistent(go.transform.root.gameObject))
                        continue;
                }

                objectsInScene.Add(go);
            }

            return objectsInScene;
        }
        
        public static List<T> GetObjectsInScene<T>(bool includeInactive = false) where T : Component
        {
            List<T> objectsInScene = new List<T>();

            foreach (T obj in Resources.FindObjectsOfTypeAll(typeof(T)) as T[])
            {
                if (!includeInactive)
                {
                    if (IsHiddenOrNotEditable(obj) || IsHiddenOrNotEditable(obj.gameObject))
                        continue;

                    if (!EditorUtility.IsPersistent(obj.transform.root.gameObject))
                        continue;
                }

                objectsInScene.Add(obj);
            }

            return objectsInScene;
        }

        private static bool IsHiddenOrNotEditable(Object obj)
        {
            return obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave;
        }
        
        
        public static void MoveCameraToEditorView(Camera camera)
        {
            if (camera != null)
            {
                var sceneViewCamera = SceneView.lastActiveSceneView.camera;

                if (sceneViewCamera != null)
                {
                    camera.transform.position = sceneViewCamera.transform.position;
                    camera.transform.rotation = sceneViewCamera.transform.rotation;
                }
            }
        }
    }
}