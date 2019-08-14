using System;
using System.IO;
using TMPro;
using UnityEngine;

public class VarwinVersionInfo : MonoBehaviour
{
    public static string VarwinVersion
    {
        get
        {
            TryReadVersionInfo();
            return string.Format(_versionString, _versionNumber);
        }
    }

    public static string VersionNumber
    {
        get
        {
            TryReadVersionInfo();
            return _versionNumber;
        }
    }

    private static bool _versionIsLoaded = false;
    private static string _versionNumber = "0.4.0";
    private static string _versionString = "Version {0} beta";

    public TMP_Text VersionTextObject;

    private void Start()
    {
        if (VersionTextObject != null)
        {
            VersionTextObject.text = VarwinVersion;
        }
    }

    private static void TryReadVersionInfo()
    {
        if (_versionIsLoaded)
        {
            return;
        }
        
        try
        {
            
#if UNITY_ANDROID && !UNITY_EDITOR
            string path = Application.streamingAssetsPath + "/version.txt";
            var fileRequest = new WWW(path);
            while (!fileRequest.isDone) { }
            if (string.IsNullOrEmpty(fileRequest.error))
            {
                string[] lines = fileRequest.text.Split(new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                _versionNumber = lines[0];
                _versionString = lines[1];
            }
#else
            string path = Application.dataPath + "/StreamingAssets/version.txt";
            if (File.Exists(path))
            {
                var file = File.ReadAllLines(path);
                _versionNumber = file[0];
                _versionString = file[1];
            }
#endif
            _versionIsLoaded = true;
        }
        catch 
        {
            Debug.Log("File version not found!");
        }
    }
}
