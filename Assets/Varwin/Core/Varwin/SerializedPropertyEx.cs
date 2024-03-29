#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Varwin
{
    public static class SerializedPropertyEx
    {
        public static bool SetValue<T>(this SerializedProperty property, T value)
        {

            object obj = GetSerializedPropertyRootComponent(property);
            //Iterate to parent object of the value, necessary if it is a nested object
            string[] fieldStructure = property.propertyPath.Split('.');

            for (int i = 0; i < fieldStructure.Length - 1; i++)
            {
                obj = GetFieldOrPropertyValue<object>(fieldStructure[i], obj);
            }

            string fieldName = fieldStructure.Last();

            return SetFieldOrPropertyValue(fieldName, obj, value);

        }

        public static Component GetSerializedPropertyRootComponent(SerializedProperty property)
        {
            return (Component) property.serializedObject.targetObject;
        }

        public static T GetFieldOrPropertyValue<T>(string fieldName, object obj, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);

            if (field != null)
            {
                return (T) field.GetValue(obj);
            }

            PropertyInfo property = obj.GetType().GetProperty(fieldName, bindings);

            if (property != null)
            {
                return (T) property.GetValue(obj, null);
            }

            if (includeAllBases)
            {
                foreach (Type type in GetBaseClassesAndInterfaces(obj.GetType()))
                {
                    field = type.GetField(fieldName, bindings);

                    if (field != null)
                    {
                        return (T) field.GetValue(obj);
                    }

                    property = type.GetProperty(fieldName, bindings);

                    if (property != null)
                    {
                        return (T) property.GetValue(obj, null);
                    }
                }
            }

            return default(T);
        }

        public static bool SetFieldOrPropertyValue(string fieldName, object obj, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, bindings);

            if (field != null)
            {
                field.SetValue(obj, value);

                return true;
            }

            PropertyInfo property = obj.GetType().GetProperty(fieldName, bindings);

            if (property != null)
            {
                property.SetValue(obj, value, null);

                return true;
            }

            if (includeAllBases)
            {
                foreach (Type type in GetBaseClassesAndInterfaces(obj.GetType()))
                {
                    field = type.GetField(fieldName, bindings);

                    if (field != null)
                    {
                        field.SetValue(obj, value);

                        return true;
                    }

                    property = type.GetProperty(fieldName, bindings);

                    if (property != null)
                    {
                        property.SetValue(obj, value, null);

                        return true;
                    }
                }
            }

            return false;
        }

        public static IEnumerable<Type> GetBaseClassesAndInterfaces(this Type type, bool includeSelf = false)
        {
            List<Type> allTypes = new List<Type>();

            if (includeSelf)
            {
                allTypes.Add(type);
            }

            if (type.BaseType == typeof(object))
            {
                allTypes.AddRange(type.GetInterfaces());
            }
            else
            {
                allTypes.AddRange(
                    Enumerable
                        .Repeat(type.BaseType, 1)
                        .Concat(type.GetInterfaces())
                        .Concat(type.BaseType.GetBaseClassesAndInterfaces())
                        .Distinct());
            }

            return allTypes;
        }


    }
}
#endif