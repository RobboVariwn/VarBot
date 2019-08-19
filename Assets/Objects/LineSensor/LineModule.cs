using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;

public class LineModule : VirtualModule.VirtualMagnetModule
{
    public Camera Sensor;

    private RenderTexture sensorTexture;

    private Texture2D tex;
    private bool needRender = false;
    private bool renderingDone;
    private float result;

    public override void OnConnected() { }

    public override void OnDisconnected() { }

    public override T Read<T>()
    {
        needRender = true;
        renderingDone = false;
        while (!renderingDone)
        {
            Thread.Sleep(100);
        }
        return (T)(object)result;
    }

    public override void Write<T>(T data)
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        sensorTexture = new RenderTexture(5, 5, 0);
        sensorTexture.Create();
        Sensor.targetTexture = sensorTexture;
        tex = new Texture2D(5, 5, TextureFormat.RGB24, false);
    }

    // Update is called once per frame
    void Update()
    {
        if(needRender)
        {
            needRender = false;
            renderingDone = false;

            RenderTexture.active = sensorTexture;
            tex.ReadPixels(new Rect(0, 0, 5, 5), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            result = tex.GetPixels(0, 0, 5, 5).Select(rgb => rgb.grayscale).Sum() / 25F;

            renderingDone = true;
        }
    }

    private void OnDestroy()
    {
        sensorTexture.DiscardContents();
        sensorTexture.Release();
    }
}
