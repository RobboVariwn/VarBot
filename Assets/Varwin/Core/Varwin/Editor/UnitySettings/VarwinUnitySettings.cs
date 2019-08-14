using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Varwin.Editor
{
    public static class VarwinUnitySettings
    {
        public const string Ignore = "Varwin.ignore.";
        public const string UseRecommended = "Use recommended ({0})";
        public const string CurrentValue = " (current = {0})";

        public readonly static List<UnitySettingsOption> Options = new List<UnitySettingsOption>()
        {
            // Android 
            
            new UnitySettingsOption(
                "Android Texture Compression",
                () => EditorUserBuildSettings.androidBuildSubtarget,
                x => EditorUserBuildSettings.androidBuildSubtarget = x,
                MobileTextureSubtarget.ASTC),
            
            // Rendering Settings
            
            new UnitySettingsOption(
                "Color Space",
                () => PlayerSettings.colorSpace,
                x => PlayerSettings.colorSpace = x,
                ColorSpace.Linear),

            // Other Settings
            
            new UnitySettingsOption(
                "API Compatibility Level",
                () => PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone),
                x => PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, x),
                ApiCompatibilityLevel.NET_4_6),

            new UnitySettingsOption(
                "Scripting Runtime Version",
                () => PlayerSettings.scriptingRuntimeVersion,
                x => PlayerSettings.scriptingRuntimeVersion = x,
                ScriptingRuntimeVersion.Latest),

            
            // XR Settings
            
            new UnitySettingsOption(
                "Virtual Reality Supported",
                () => PlayerSettings.virtualRealitySupported,
                x => PlayerSettings.virtualRealitySupported = x,
                true),

            new UnitySettingsOption(
                "Stereo Rendering Mode",
                () => PlayerSettings.stereoRenderingPath,
                x => PlayerSettings.stereoRenderingPath = x,
                StereoRenderingPath.SinglePass),

            new UnitySettingsOption(
                "360 Stereo Capture",
                () => PlayerSettings.enable360StereoCapture,
                x => PlayerSettings.enable360StereoCapture = x,
                false),

            //Define Symbols
            
            new UnitySettingsOption(
                "Scripting Define Symbols",
                () => PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone),
                x => PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, x),
                "R_NEWER;VRMAKER")
            
        };

        public static readonly Dictionary<int, string> Tags = new Dictionary<int, string>()
        {
            {0, "TeleportArea"},
            {1, "NotTeleport"},
            {2, "Beam"},
        };

        public static readonly Dictionary<int, string> Layers = new Dictionary<int, string>()
        {
            {8, "Rotators"},
            {9, "VRControllers"},
            {10, "Location"},
            {11, "LiquidContainer"},
            {12, "LiquidEraser"},
            {13, "Zones"},
        };
        
        public static readonly List<string> SteamVrBindingsFiles = new List<string>()
        {
            "actions.json", 
            "binding_holographic_hmd.json", 
            "binding_rift.json", 
            "binding_vive.json", 
            "binding_vive_pro.json", 
            "binding_vive_tracker_camera.json",
            "bindings_holographic_controller.json", 
            "bindings_knuckles.json", 
            "bindings_oculus_touch.json", 
            "bindings_vive_controller.json"
        };
        
        public static readonly string[] AdditionalExtensionsToInclude = {"txt", "xml", "fnt", "cd", "asmdef", "rsp"};

        private static SerializedObject _playerSettings;
        private static SerializedObject _physicsManager;
        
        private static SerializedObject _tagManager;
        private static SerializedProperty _tags;
        private static SerializedProperty _layers;

        static VarwinUnitySettings()
        {
            _playerSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            _physicsManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset")[0]);
            
            _tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            _tags = _tagManager.FindProperty("tags");
            _layers = _tagManager.FindProperty("layers");
        }

        public static bool TagsAreValid()
        {
            foreach (var tag in Tags)
            {
                if (tag.Key < _tags.arraySize)
                {
                    SerializedProperty t = _tags.GetArrayElementAtIndex(tag.Key);
                    if (!t.stringValue.Equals(tag.Value))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        
        public static void SetupTags()
        {
            if (TagsAreValid()) return;

            foreach (var tag in Tags)
            {
                while (tag.Key >= _tags.arraySize)
                {
                    _tags.InsertArrayElementAtIndex(tag.Key);
                }
                SerializedProperty n = _tags.GetArrayElementAtIndex(tag.Key);
                n.stringValue = tag.Value;
            }
        }

        public static string GetLayer(int layerIndex)
        {
            SerializedProperty sp = _layers.GetArrayElementAtIndex(layerIndex);
            return sp?.stringValue;
        }

        public static void SetLayer(int layerIndex, string layerName)
        {
            SerializedProperty sp = _layers.GetArrayElementAtIndex(layerIndex);
            if (sp != null) sp.stringValue = layerName;
        }

        public static void Save()
        {
            _tagManager.ApplyModifiedProperties();
            _playerSettings.ApplyModifiedProperties();
            _physicsManager.ApplyModifiedProperties();
        }

        public static bool CheckSteamVRBindings()
        {
            string root = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));

            foreach (string bindingsFile in SteamVrBindingsFiles)
            {
                string path = Path.Combine(root, bindingsFile);

                if (!File.Exists(path))
                {
                    return false;
                }
            }

            return true;
        }
    }
}