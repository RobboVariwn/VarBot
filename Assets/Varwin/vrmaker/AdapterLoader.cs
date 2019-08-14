using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Varwin.VRInput;

public class AdapterLoader : MonoBehaviour
{    
    void Awake()
    {
        InputAdapter.ChangeCurrentAdapter(new SteamVRAdapter());
    }
}
