using System;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    /// <summary>
    /// Defaul author settings class
    /// </summary>
    public static class AuthorSettings
    {
        public const string NamePref = "Name";
        public const string EmailPref = "Email";
        public const string UrlPref = "Url";
        
        public static string Name;
        public static string SavedName;
        
        public static string Email;
        public static string SavedEmail;

        public static string Url;
        public static string SavedUrl;

        
        public static void Initialize()
        {
            string defaultInitialName = "Anonymous";
            
            Name = LoadPref(NamePref, defaultInitialName);
            SavedName = Name;

            if (string.IsNullOrWhiteSpace(SavedName))
            {
                Name = defaultInitialName;
                SavedName = defaultInitialName;
                SavePref(NamePref, Name);
            }
            
            Email = LoadPref(EmailPref);
            SavedEmail = Email;
            
            Url = LoadPref(UrlPref);
            SavedUrl = Url;
        }

        public static void Clear()
        {
            EditorPrefs.DeleteKey(GetKey(NamePref));
            EditorPrefs.DeleteKey(GetKey(EmailPref));
            EditorPrefs.DeleteKey(GetKey(UrlPref));
        }
        
        public static string GetKey(string key)
        {
            return $"DefaultAuthor.{key}";
        }

        public static string LoadPref(string name, string defaultValue = "")
        {
            string key = GetKey(name);
            string value = defaultValue;
            if (EditorPrefs.HasKey(key))
            {
                value = EditorPrefs.GetString(key, value);
            }
            EditorPrefs.SetString(key, value);
            return value;
        }

        public static void SavePref(string name, string value)
        {
            EditorPrefs.SetString(GetKey(name), value);
        }

        public static void SaveAll()
        {
            SavePref(NamePref, Name);
            SavePref(EmailPref, Email);
            SavePref(UrlPref, Url);
        }
        
        public static bool IsChanged()
        {
            return !string.Equals(Name, SavedName)
                   || !string.Equals(Email, SavedEmail)
                   || !string.Equals(Url, SavedUrl);
        }
    }
}