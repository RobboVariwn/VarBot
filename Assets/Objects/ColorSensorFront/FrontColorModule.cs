using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Linq;

public class FrontColorModule : VirtualModule.VirtualMagnetModule
{
    public Camera Sensor;
    public new Light light;

    private RenderTexture sensorTexture;

    private Texture2D tex;
    private bool needRender = false;
    private bool renderingDone;
    private Vector3 result;

    public override void OnConnected()
    {
        light.enabled = true;
    }

    public override void OnDisconnected()
    {
        light.enabled = false;
    }

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
        if (needRender)
        {
            needRender = false;
            renderingDone = false;

            RenderTexture.active = sensorTexture;
            tex.ReadPixels(new Rect(0, 0, 5, 5), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            var colors = tex.GetPixels(0, 0, 5, 5).Select(rgb => new Vector3(rgb.r*255F, rgb.g*255F, rgb.b*255F));
            var average_color = new Vector3(0, 0, 0);
            foreach(var color in colors)
            {
                average_color += color;
            }
            result = average_color / 25F;

            renderingDone = true;
        }
    }

    private void OnDestroy()
    {
        sensorTexture.DiscardContents();
        sensorTexture.Release();
    }
}
