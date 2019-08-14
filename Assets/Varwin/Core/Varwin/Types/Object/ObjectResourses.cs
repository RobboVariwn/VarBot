using System.Collections.Generic;
using UnityEngine;

namespace Varwin
{
    public static class ObjectResourses
    {
        private static readonly Dictionary<string, UnityEngine.Object> Resourses = new Dictionary<string, UnityEngine.Object>();

        public static TEntity GetResourse<TEntity>(string resourseName) where TEntity : UnityEngine.Object
        {
            if (!Resourses.ContainsKey(resourseName))
                return null;

            return (TEntity) Resourses[resourseName];
        }

        public static void ClearResourses()
        {
            foreach (var resourse in Resourses.Values)
            {
                Object.Destroy(resourse);
            }
            Resourses.Clear();
        }

        public static void AddResourse(UnityEngine.Object resourse)
        {
            if (resourse == null)
            {
                Debug.LogError($"Resourse is null");
                return;
            }

            if (Resourses.ContainsKey(resourse.name))
            {
                Debug.Log($"Resourses alredy have {resourse.name}");
                return;
            }

            Resourses.Add(resourse.name, resourse);
        }
    }
}
