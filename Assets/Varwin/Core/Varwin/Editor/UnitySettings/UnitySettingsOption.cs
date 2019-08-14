using System;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class UnitySettingsOption
    {
        private const int MaxSymbolsInName = 30;
        
        public string Name;
        public Func<dynamic> Getter;
        public Action<dynamic> Setter;
        public dynamic RecommendedValue;

        public UnitySettingsOption(string name, Func<dynamic> getter, Action<dynamic> setter, dynamic recommendedValue)
        {
            Name = name;
            Getter = getter;
            Setter = setter;
            RecommendedValue = recommendedValue;
        }

        public bool IsEquals => RecommendedValue.Equals(Getter());
        
        public bool IsNeedToDraw => !EditorPrefs.HasKey(VarwinUnitySettings.Ignore + Name) && !IsEquals;

        public void Draw()
        {
            if (IsNeedToDraw)
            {
                string currentValueStr = Getter().ToString();
                if (currentValueStr.Length > MaxSymbolsInName)
                {
                    currentValueStr = currentValueStr.Substring(0, MaxSymbolsInName);
                }
                
                GUILayout.Label(Name + string.Format(VarwinUnitySettings.CurrentValue, currentValueStr));

                GUILayout.BeginHorizontal();

                string recommendedValueStr = RecommendedValue.ToString();
                if (recommendedValueStr.Length > MaxSymbolsInName)
                {
                    recommendedValueStr = recommendedValueStr.Substring(0, MaxSymbolsInName);
                }
                if (GUILayout.Button(string.Format(VarwinUnitySettings.UseRecommended, recommendedValueStr)))
                {
                    UseRecommended();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Ignore"))
                {
                    Ignore();
                }

                GUILayout.EndHorizontal();
            }
        }

        public void Ignore(bool force = false)
        {
            if (force || !IsEquals)
            {
                EditorPrefs.SetBool(VarwinUnitySettings.Ignore + Name, true);
            }
        }

        public void ClearIgnore()
        {
            EditorPrefs.DeleteKey(VarwinUnitySettings.Ignore + Name);
        }

        public void UseRecommended(bool force = false)
        {
            if (force || !EditorPrefs.HasKey(VarwinUnitySettings.Ignore + Name))
            {
                Setter(RecommendedValue);
            }
        }
    }
}