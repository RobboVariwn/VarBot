using System;
using UnityEngine;

public class WorldDescriptor : MonoBehaviour
{
    public Transform PlayerSpawnPoint;
    public string Name;
    public string Guid;
    public string RootGuid;
    [TextArea(4, 12)]
    public string Description;
    public string Image;
    public string AssetBundleLabel;
    public string[] DllNames;
    
    public string AuthorName;
    public string AuthorEmail;
    public string AuthorUrl;

    public string LicenseCode;
    public string LicenseVersion;

    public string BuiltAt;
    public bool MobileReady;

    public bool CurrentVersionWasBuilt;
    
    private void Reset()
    {
        Validate();
    }

    private void OnValidate()
    {
        Validate();
    }
        
    public void Validate()
    {
        LockTrasform();
        
        if (string.IsNullOrWhiteSpace(Guid))
        {
            return;
        }
            
        if (string.IsNullOrWhiteSpace(RootGuid))
        {
            if (string.IsNullOrWhiteSpace(Guid))
            {
                Debug.LogError($"Scene Template {gameObject.name} has empty guid. New guid was generated.", gameObject);
                Guid = System.Guid.NewGuid().ToString();
            }
                
            RootGuid = Guid;
        }

        if (string.IsNullOrWhiteSpace(AuthorName))
        {
            AuthorName = "Anonymous";
        }

        if (string.IsNullOrWhiteSpace(LicenseCode))
        {
            LicenseCode = "cc-by";
        }

        if (string.IsNullOrWhiteSpace(LicenseVersion))
        {
            LicenseVersion = "4.0";
        }
    }

    private void OnDrawGizmos()
    {
        LockTrasform();
    }

    private void LockTrasform()
    {
        if (Application.isPlaying)
        {
            return;
        }
        
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}
