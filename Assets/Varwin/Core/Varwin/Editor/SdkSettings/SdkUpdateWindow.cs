using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Varwin.Data;
using Version = System.Version;

namespace Varwin.Editor
{
    [InitializeOnLoad]
    public class SdkUpdateWindow : EditorWindow
    {
        private static string RemoteVersion;
        private static bool IsUpdateAvailable;
        private static bool IsForceOpen;
        private static UnityWebRequest RemoteRequest;
        
        private const string DisableUpdateAutoCheckPrefsKey = "DisableSdkUpdateAutoCheck";
        private const string RemoteVersionJsonPath = "https://dist.varwin.com/releases/latest/info.json";
        private const string DownloadLink = "https://dist.varwin.com/releases/latest/VarwinSDK.unitypackage";

        private static string LocalVersion => VarwinVersionInfo.VersionNumber;
       
        private static bool IsDisableAutoCheck
        {
            get
            {
                if (!EditorPrefs.HasKey(DisableUpdateAutoCheckPrefsKey))
                {
                    EditorPrefs.SetBool(DisableUpdateAutoCheckPrefsKey, false);
                }
                
                return EditorPrefs.GetBool(DisableUpdateAutoCheckPrefsKey);
            }
            set
            {
                EditorPrefs.SetBool(DisableUpdateAutoCheckPrefsKey, value);
            }
        }

        private void OnGUI()
        {
            if (IsUpdateAvailable)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(GetUpdateAvailableMessage(), MessageType.Warning);                    
                EditorGUILayout.HelpBox(GetHelpMessage(), MessageType.Info);
                
                EditorGUILayout.Space();
                if (GUILayout.Button("Download SDK"))
                {
                    Application.OpenURL(DownloadLink);
                }
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(GetNotNeedUpdateMessage(), MessageType.Info);
            }
            
            EditorGUILayout.Space();
            IsDisableAutoCheck = EditorGUILayout.Toggle("Disable Auto Check", IsDisableAutoCheck);
        }

        private static void OnEditorUpdate()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.update -= OnEditorUpdate;
                return;
            }
            
            if (!IsForceOpen && IsDisableAutoCheck)
            {
                EditorApplication.update -= OnEditorUpdate;
                return;
            }
            
            RemoteVersion = LocalVersion;
            IsUpdateAvailable = false;

            if (RemoteRequest == null)
            {
                RemoteRequest = UnityWebRequest.Get(RemoteVersionJsonPath);
                RemoteRequest.SendWebRequest();
            }
           
            if (!RemoteRequest.isDone)
            {
                return;
            }
            
            if (!RemoteRequest.isNetworkError)
            {
                var versionObj = RemoteRequest.downloadHandler.text.JsonDeserialize<RemoteVersionInfo>();
                if (versionObj != null)
                {
                    if (!string.IsNullOrWhiteSpace(versionObj.version))
                    {
                        RemoteVersion = versionObj.version;
                    }
                }
            }
            
            if (Version.TryParse(LocalVersion, out Version localVersion))
            {
                if (Version.TryParse(RemoteVersion, out Version remoteVersion))
                {
                    IsUpdateAvailable = localVersion.CompareTo(remoteVersion) < 0;
                }
            }

            if (IsUpdateAvailable || IsForceOpen)
            {
                GetWindow<SdkUpdateWindow>("SDK Update", true);
            }
            
            EditorApplication.update -= OnEditorUpdate;
        }        
        
        private static string GetUpdateAvailableMessage()
        {
            return $"A new version is available!\n{GetVersionsText()}";
        }

        private static string GetNotNeedUpdateMessage()
        {
            return $"You have the latest version\n{GetVersionsText()}";
        }

        private static string GetVersionsText()
        {
            return $"Local version: {LocalVersion}\nRemote version: {RemoteVersion}";
        }
        
        private string GetHelpMessage()
        {
            return $"How to update:\n"
                   + $"1. Download the new version of Varwin SDK\n"
                   + $"2. Delete folder: Assets/Varwin\n"
                   + $"3. Import the downloaded Asset Package";
        }
        
        static SdkUpdateWindow()
        {
            IsForceOpen = false;
            EditorApplication.update += OnEditorUpdate;            
        }
        
        [MenuItem("VARWIN SDK/Check for Update")]
        private static void ForceCheck()
        {
            IsForceOpen = true;
            EditorApplication.update += OnEditorUpdate;       
        }               
        
        private class RemoteVersionInfo : IJsonSerializable
        {
            public string version;
        }        
    }
}