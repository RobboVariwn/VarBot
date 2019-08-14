using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using UnityEngine;

namespace Varwin
{
    public class WrappersCollection
    {
        private readonly Dictionary<int, Wrapper> _wrappers = new Dictionary<int, Wrapper>();
        
        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

        /// <summary>
        /// Add new wrapper to collection
        /// </summary>
        /// <param name="idInstance">Object id</param>
        /// <param name="wrapper">Object wrapper</param>
        public void Add(int idInstance, Wrapper wrapper)
        {
            _wrappers.Add(idInstance, wrapper);
        }

        /// <summary>
        /// Get all wrappers from collection
        /// </summary>
        /// <returns></returns>
        public List<Wrapper> Wrappers()
        {
            List<Wrapper> result = new List<Wrapper>();
            result.AddRange(_wrappers.Values);
            return result;
        }

        /// <summary>
        /// Get all wrappers of type from collection
        /// </summary>
        /// <param name="typeName">Wrapper type name</param>
        /// <returns>List of wrappers of type</returns>
        public List<Wrapper> GetWrappersOfType(string typeName)
        {
            Type type = GetType(typeName);
            
            var result = new List<Wrapper>();
            
            foreach (Wrapper wrapper in _wrappers.Values) 
            {
                if (wrapper.GetType() == type)
                {
                    result.Add(wrapper);
                }
            }
            
            return result;
        }

        /// <summary>
        /// Get wrapper by id
        /// </summary>
        /// <param name="id">Object id</param>
        /// <returns></returns>
        public Wrapper Get(int id)
        {
            if (_wrappers.ContainsKey(id))
            {
                return _wrappers[id];
            }

            throw new Exception($"Wrapper with {id} not found!");
        }

        /// <summary>
        /// Check existing object with id
        /// </summary>
        /// <param name="id">Object instance id</param>
        /// <returns></returns>
        public bool Exist(int id) => _wrappers.ContainsKey(id);

        /// <summary>
        /// Get wrappers of children object 
        /// </summary>
        /// <param name="target">Target wrapper</param>
        /// <returns></returns>
        public List<Wrapper> GetChildren(Wrapper target)
        {
            List<Wrapper> result = new List<Wrapper>();
            GameObject gameObject = target.GetGameObject();
            int targetId = gameObject.GetComponent<ObjectBehaviourWrapper>().OwdObjectController.Id;
            var behaviours = gameObject.GetComponentsInChildren<ObjectBehaviourWrapper>();

            foreach (ObjectBehaviourWrapper behaviour in behaviours)
            {
                if (behaviour.OwdObjectController.Parent != null && behaviour.OwdObjectController.Parent.Id == targetId)
                {
                    result.Add(behaviour.OwdObjectController.Entity.wrapper.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// Get wrappers of descendants object
        /// </summary>
        /// <param name="target">Target wrapper</param>
        /// <returns></returns>
        public List<Wrapper> GetDescendants(Wrapper target)
        {
            List<Wrapper> result = new List<Wrapper>();
            GameObject gameObject = target.GetGameObject();
            int targetId = gameObject.GetComponent<ObjectBehaviourWrapper>().OwdObjectController.Id;
            var behaviours = gameObject.GetComponentsInChildren<ObjectBehaviourWrapper>();

            foreach (ObjectBehaviourWrapper behaviour in behaviours)
            {
                if (behaviour.OwdObjectController.Id == targetId)
                {
                    continue;
                }

                result.Add(behaviour.OwdObjectController.Entity.wrapper.Value);
            }

            return result;
        }

        /// <summary>
        /// Get wrapper of parent object
        /// </summary>
        /// <param name="target">Target wrapper</param>
        /// <returns></returns>
        public Wrapper GetParent(Wrapper target)
        {
            GameObject gameObject = target.GetGameObject();
            ObjectController objectController = gameObject.GetComponent<ObjectBehaviourWrapper>().OwdObjectController;
            return objectController.Parent?.Entity.wrapper.Value;
        }

        /// <summary>
        /// Get wrapper of ancestry object
        /// </summary>
        /// <param name="target">Target wrapper</param>
        /// <returns></returns>
        public List<Wrapper> GetAncestry(Wrapper target)
        {
            List<Wrapper> result = new List<Wrapper>();
            GameObject gameObject = target.GetGameObject();
            ObjectController objectController = gameObject.GetComponent<ObjectBehaviourWrapper>().OwdObjectController;
            ObjectController parent = objectController.Parent;

            while (parent != null)
            {
                result.Add(parent.Entity.wrapper.Value);
                parent = parent.Parent;
            }

            return result;
        }

        /// <summary>
        /// Clear all wrappers from collection
        /// </summary>
        public void Clear()
        {
            _wrappers.Clear();
        }

        /// <summary>
        /// Remove wrapper by id
        /// </summary>
        /// <param name="id"></param>
        public void Remove(int id)
        {
            if (_wrappers.ContainsKey(id))
            {
                _wrappers.Remove(id);
            }
            else
            {
                LogManager.GetCurrentClassLogger().Info($"Object {id} have no wrapper!");
            }
        }

        private Type GetType(string typeName)
        {
            Type type = null;
            if (_types.ContainsKey(typeName))
            {
                type = _types[typeName];
            }
            else
            {
                var wrapper = _wrappers.Values.FirstOrDefault(w => w.GetType().ToString() == typeName);
                if (wrapper != null)
                {
                    type = wrapper.GetType();
                    _types.Add(typeName, type);
                }
            }

            return type;
        }
    }

}


