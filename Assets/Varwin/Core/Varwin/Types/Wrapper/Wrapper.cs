using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Varwin.Public;

namespace Varwin
{
    public abstract class Wrapper : DynamicObject
    {
        protected GameEntity Entity { get; set; }
        protected GameObject GameObject { get; set; }
        protected GameEntity ObjectEntity { get; set; }
        private bool _isEnabled = true;

        protected Wrapper(GameEntity entity)
        {
            Entity = entity;
        }

        public void InitEntity(GameEntity entity)
        {
            ObjectEntity = entity;
        }

        protected Wrapper(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public bool IsActive()
        {
            GameObject go = GetGameObject();
            return go.activeSelf;
        }

        public bool IsInactive()
        {
            GameObject go = GetGameObject();
            return !go.activeSelf;
        }

        public void Activate()
        {
            GameObject go = GetGameObject();
            go.SetActive(true);
        }

        public void Deactivate()
        {
            GameObject go = GetGameObject();
            go.SetActive(false);
        }

        public bool Activity
        {
            get { return GetGameObject().activeSelf; }
            set { GetGameObject().SetActive(value);}
        }

        public bool Enabled
        {
            get { return _isEnabled; }
            set
            {
                if (value)
                {
                    Enable();
                }
                else
                {
                    Disable();
                }
            }
        }

        public void Enable()
        {
            _isEnabled = true;
            if (!ObjectEntity.hasInputControls)
            {
                return;
            }

            var controls = ObjectEntity.inputControls.Values.Values;
            foreach (InputController control in controls)
            {
                control.EnableViewInput();
            }

        }

        public void Disable()
        {
            _isEnabled = false;
            if (!ObjectEntity.hasInputControls)
            {
                return;
            }

            var controls = ObjectEntity.inputControls.Values.Values;
            foreach (InputController control in controls)
            {
                control.DisableViewInput();
            }
        }

        public bool IsEnabled() => _isEnabled;

        public bool IsDisabled() => !_isEnabled;

        public GameObject GetGameObject()
        {
            if (Entity != null)
            {
                return Entity.gameObject.Value;
            }

            if (GameObject != null)
            {
                return GameObject;
            }

            return null;
        }

        public InputController GetInputController(GameObject go)
        {
            if (!ObjectEntity.hasInputControls)
            {
                return null;
            }
            
            ObjectId objectId = go.GetComponent<ObjectId>();

            if (objectId == null)
            {
                return null;
            }

            int id = objectId.Id;
            
            var controls = ObjectEntity.inputControls.Values;

            return !controls.ContainsKey(id) ? null : controls[id];
        }

        #region INPUT LOGIC

        /// <summary>
        /// Enable grab for all grabbable in object
        /// </summary>
        public void EnableGrab()
        {
            if (!ObjectEntity.hasInputControls)
            {
                return;
            }

            var controls = ObjectEntity.inputControls.Values.Values;
            foreach (InputController control in controls)
            {
                control.EnableViewGrab();
            }
        }

        /// <summary>
        /// Disable grab for all grabbable in objects
        /// </summary>
        public void DisableGrab()
        {
            if (!ObjectEntity.hasInputControls)
            {
                return;
            }

            var controls = ObjectEntity.inputControls.Values.Values;
            foreach (InputController control in controls)
            {
                control.DisableViewGrab();
            }
        }


        public int GetInstanceId() => ObjectEntity.id.Value;

        public void EnableGrabForObject(GameObject go)
        {
            InputController control = GetControlOnGameObject(go);

            control?.EnableViewGrab();
        }

        public void DisableGrabForObject(GameObject go)
        {
            InputController control = GetControlOnGameObject(go);

            control?.DisableViewGrab();
        }

        public void VibrateWithObject(GameObject go, GameObject controllerObject, float strength, float duration, float interval)
        {
            InputController control = GetControlOnGameObject(go);

            control?.Vibrate(controllerObject, strength, duration, interval);
        }

        private InputController GetControlOnGameObject(GameObject go)
        {
            if (!ObjectEntity.hasInputControls)
            {
                return null;
            }

            var controls = ObjectEntity.inputControls.Values.Values;
            foreach (InputController control in controls)
            {
                if (control.IsConnectedToGameObject(go))
                {
                    return control;
                }
            }

            return null;
        }
        
        #endregion

        #region Dynamic Methods

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = binder.Name;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Type thisType = GetType();
            FieldInfo fieldInfo = thisType.GetField(binder.Name);

            if (fieldInfo != null)
            {
                if (value.GetType() != fieldInfo.FieldType)
                {
                    CastValue(fieldInfo, value);
                }
            }
            else
            {
                PropertyInfo propertyInfo = thisType.GetProperty(binder.Name);
                
                if (value.GetType() != propertyInfo?.PropertyType)
                {
                    CastValue(propertyInfo, value);
                }
            }

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Type thisType = GetType();
            MethodInfo methodInfo = thisType.GetMethod(binder.Name);

            if (methodInfo == null)
            {
                result = false;
                //Debug.Log("Method " + binder.Name + " not found!");
                return true;
            }

            var parametres = methodInfo.GetParameters();

            if (args.Length == parametres.Length)
            {
                bool error = false;
                for (int p = 0; p < args.Length; p++)
                {
                    var callArg = args[p];
                    var parametrInfo = parametres[p];

                    if (callArg.GetType() != parametrInfo.ParameterType)
                    {
                        if (!CastValue(parametrInfo.ParameterType, callArg, out callArg))
                        {
                            error = true;
                            break;
                        }

                        args[p] = callArg;
                    }

                }

                if (!error)
                {
                    methodInfo.Invoke(this, args);
                }

                else
                {
                    //Debug.Log("Can not invoke method " + binder.Name);
                }
            }

            else
            {
                //Debug.Log("Diferent arguments count on method " + binder.Name);
            }

            result = true;
            return true;
        }

        private void CastValue(FieldInfo fieldInfo, object value)
        {
            if (value is string)
            {
                value = value.ToString().Replace(".", ",");
            }

            if (fieldInfo.FieldType == typeof(float))
            {
                try
                {
                    value = Convert.ToSingle(value);
                    fieldInfo.SetValue(this, value);
                    //Debug.Log("Convert sucsesfull!");
                }
                catch
                {
                    DefaultValue(fieldInfo, value);
                }

                return;
            }

            if (fieldInfo.FieldType == typeof(int))
            {
                try
                {
                    value = Convert.ToInt32(value);
                    fieldInfo.SetValue(this, value);
                    //Debug.Log("Convert sucsesfull!");
                }
                catch
                {
                    DefaultValue(fieldInfo, value);
                }

                return;
            }

            if (fieldInfo.FieldType == typeof(decimal))
            {
                try
                {
                    value = Convert.ToDecimal(value);
                    fieldInfo.SetValue(this, value);
                    //Debug.Log("Convert sucsesfull!");
                }
                catch
                {
                    DefaultValue(fieldInfo, value);
                }

                return;
            }

            if (fieldInfo.FieldType == typeof(double))
            {
                try
                {
                    value = Convert.ToDouble(value);
                    fieldInfo.SetValue(this, value);
                    //Debug.Log("Convert sucsesfull!");
                }
                catch
                {
                    DefaultValue(fieldInfo, value);
                }

                return;
            }

            if (fieldInfo.FieldType == typeof(string))
            {
                try
                {
                    value = Convert.ToString(value);
                    fieldInfo.SetValue(this, value);
                    //Debug.Log("Convert sucsesfull!");
                }
                catch
                {
                    DefaultValue(fieldInfo, value);
                }
            }
        }

        private void CastValue(PropertyInfo propertyInfo, object value)
        {
            if (value is string)
            {
                value = value.ToString().Replace(".", ",");
            }

            if (propertyInfo.PropertyType == typeof(float))
            {
                try
                {
                    value = Convert.ToSingle(value);
                    propertyInfo.SetValue(this, value);
                    //Debug.Log("Convert sucsesfull!");
                }
                catch
                {
                    DefaultValue(propertyInfo, value);
                }

                return;
            }

            if (propertyInfo.PropertyType == typeof(int))
            {
                try
                {
                    value = Convert.ToInt32(value);
                    propertyInfo.SetValue(this, value);
                    //Debug.Log("Convert sucsesfull!");
                }
                catch
                {
                    DefaultValue(propertyInfo, value);
                }

                return;
            }

            if (propertyInfo.PropertyType == typeof(decimal))
            {
                try
                {
                    value = Convert.ToDecimal(value);
                    propertyInfo.SetValue(this, value);
                    //Debug.Log("Convert sucsesfull!");
                }
                catch
                {
                    DefaultValue(propertyInfo, value);
                }

                return;
            }

            if (propertyInfo.PropertyType == typeof(double))
            {
                try
                {
                    value = Convert.ToDouble(value);
                    propertyInfo.SetValue(this, value);
                    //Debug.Log("Convert sucsesfull!");
                }
                catch
                {
                    DefaultValue(propertyInfo, value);
                }

                return;
            }

            if (propertyInfo.PropertyType == typeof(string))
            {
                try
                {
                    value = Convert.ToString(value);
                    propertyInfo.SetValue(this, value);
                    //Debug.Log("Convert sucsesfull!");
                }
                catch
                {
                    DefaultValue(propertyInfo, value);
                }
            }
        }

        private bool CastValue(Type valueType, object value, out object result)
        {
            if (value is string)
            {
                value = value.ToString().Replace(".", ",");
            }

            if (valueType == typeof(float))
            {
                try
                {
                    result = Convert.ToSingle(value);
                    //Debug.Log("Convert sucsesfull!");
                    return true;
                }
                catch
                {
                    DefaultValue(valueType, value, out result);
                    return false;
                }
            }

            if (valueType == typeof(int))
            {
                try
                {
                    result = Convert.ToInt32(value);
                    ////Debug.Log("Convert sucsesfull!");
                    return true;
                }
                catch
                {
                    DefaultValue(valueType, value, out result);
                    return false;
                }
            }

            if (valueType == typeof(decimal))
            {
                try
                {
                    result = Convert.ToDecimal(value);
                    //Debug.Log("Convert sucsesfull!");
                    return true;
                }
                catch
                {
                    DefaultValue(valueType, value, out result);
                    return false;
                }
            }

            if (valueType == typeof(double))
            {
                try
                { 
                    result = Convert.ToDouble(value);
                    //Debug.Log("Convert sucsesfull!");
                    return true;
                }
                catch
                {
                    DefaultValue(valueType, value, out result);
                    return false;
                }
            }

            if (valueType == typeof(string))
            {
                try
                {
                    result = value.ToString();
                    //Debug.Log("Convert sucsesfull!");
                    return true;
                }
                catch
                {
                    DefaultValue(valueType, value, out result);
                    return false;
                }
            }

            result = null;

            return false;
        }


        private void DefaultValue(Type type, object value, out object result)
        {
            //Debug.Log("Cannot convert " + value + " to " + type);

            if (type == typeof(float) || type == typeof(decimal) || type == typeof(int))
            {
                result = 0;
            }

            if (type == typeof(string))
            {
                result = "";
            }

            result = null;
        }

        private void DefaultValue(FieldInfo fieldInfo, object value)
        {
            //Debug.Log("Cannot convert " + value + " to " + fieldInfo.FieldType);

            if (fieldInfo.FieldType == typeof(float) || fieldInfo.FieldType == typeof(decimal) || fieldInfo.FieldType == typeof(int))
            {
                fieldInfo.SetValue(this, 0);
            }

            if (fieldInfo.FieldType == typeof(string))
            {
                fieldInfo.SetValue(this, "");
            }
        }

        private void DefaultValue(PropertyInfo propertyInfo, object value)
        {
            //Debug.Log("Cannot convert " + value + " to " + propertyInfo.PropertyType);

            if (propertyInfo.PropertyType == typeof(float) || propertyInfo.PropertyType == typeof(decimal) || propertyInfo.PropertyType == typeof(int))
            {
                propertyInfo.SetValue(this, 0);
            }

            if (propertyInfo.PropertyType == typeof(string))
            {
                propertyInfo.SetValue(this, "");
            }
        }

        public bool HasProperty(string name)
        {
            Type thisType = GetType();
            var property = thisType.GetProperty(name);
            return property != null;
        }

        public bool HasField(string name)
        {
            Type thisType = GetType();
            var field = thisType.GetField(name);
            return field != null;
        }
        
        public bool HasMethod(string name)
        {
            Type thisType = GetType();
            var method = thisType.GetMethod(name);
            return method != null;
        }

        #endregion
    }
    
    public static class WrapperEx
    {
        public static void DisableInputUsing(this IWrapperAware self, GameObject go) => self.Wrapper().GetInputController(go)?.DisableViewUsing();
        public static void EnableInputUsing(this IWrapperAware self, GameObject go) => self.Wrapper().GetInputController(go)?.EnableViewUsing();
        public static void DisableInputGrab(this IWrapperAware self, GameObject go) => self.Wrapper().GetInputController(go)?.DisableViewGrab();
        public static void EnableInputGrab(this IWrapperAware self, GameObject go) => self.Wrapper().GetInputController(go)?.EnableViewGrab();
        public static void DisableTouch(this IWrapperAware self, GameObject go) => self.Wrapper().GetInputController(go)?.DisableViewTouch();
        public static void EnableTouch(this IWrapperAware self, GameObject go) => self.Wrapper().GetInputController(go)?.EnableViewTouch();
    }

}
