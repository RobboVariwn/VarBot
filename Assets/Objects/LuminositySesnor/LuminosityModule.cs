using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;

public class LuminosityModule : VirtualModule.VirtualMagnetModule
{
    public Camera Sensor;

    private RenderTexture cubemap;

    private Texture2D tex;
    private bool needRender = false;
    private bool renderingDone;
    private float result;

    private const int SIZE = 32;

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
        cubemap = new RenderTexture(SIZE, SIZE, 0);
        cubemap.isCubemap = true;
        cubemap.Create();
        tex = new Texture2D(SIZE, SIZE, TextureFormat.RGB24, false);
    }

    public float scale(float oldMin, float oldMax, float newMin, float newMax, float oldValue)
    {
        float oldRange = (oldMax - oldMin);
        float newRange = (newMax - newMin);
        float newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

        return newValue;
    }

    // Update is called once per frame
    void Update()
    {
        if (needRender)
        {
            needRender = false;
            renderingDone = false;

            Sensor.RenderToCubemap(cubemap);
            RenderTexture.active = cubemap;
            tex.ReadPixels(new Rect(0, 0, SIZE, SIZE), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            result = tex.GetPixels(0, 0, SIZE, SIZE).Select(rgb => Mathf.Sqrt(rgb.r * rgb.r * 0.2126F + rgb.g * rgb.g * 0.7152F + rgb.b * rgb.b * 0.0722F)).Sum() / (SIZE * SIZE);
            result = scale(0F, 0.6F, 0F, 1F, result);

            renderingDone = true;
        }
    }

    private void OnDestroy()
    {
        cubemap.DiscardContents();
        cubemap.Release();
    }
}
