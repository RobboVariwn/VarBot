using System;
using System.Collections.Generic;
using UnityEngine;

namespace Varwin
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CheckerAttribute : Attribute
    {
        public string Name;
        
        public CheckerAttribute(string name)
        {
            Name = name;
            
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ActionAttribute : Attribute
    {
        public string Name;
       
        /// <summary>
        /// Action Name
        /// </summary>
        /// <param name="name"></param>
        public ActionAttribute(string name)
        {
            Name = name;
            
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GetterAttribute : Attribute
    {
        public string Name;
       
        public GetterAttribute(string name)
        {
            Name = name;
           
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SetterAttribute : Attribute
    {
        public string Name;
        
        public SetterAttribute(string name)
        {
            Name = name;
           
        }
    }

    [AttributeUsage(AttributeTargets.Event)]
    public class EventAttribute : Attribute
    {
        public string Name;
       
        public EventAttribute(string name)
        {
            Name = name;
           
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class LocaleAttribute : Attribute
    {
        public string[] Strings;
        public SystemLanguage Language;
        
        [Obsolete("This will be removed in the future.")]
        public I18n I18n = null;
        
        public string Code => Language.ToString().Substring(0, 2).ToLowerInvariant();
        
        public LocaleAttribute(SystemLanguage language, params string[] strings)
        {
            Strings = strings;
            Language = language;
        }

        [Obsolete("This will be removed in the future. Use [Locale(SystemLanguage, string[])] instead", true)]
        public LocaleAttribute(string en, string ru)
        {
            I18n = new I18n
            {
                en = en,
                ru = ru
            };
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ValueAttribute : Attribute
    {
        public string Name;

        public ValueAttribute(string name)
        {
            Name = name;

        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ValuesAttribute : Attribute
    {
        public string Name;

        public ValuesAttribute(string name)
        {
            Name = name;
        }
    }


}
