using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour
{
	public int Width;
	public int Height;
	
	public byte[] GetIconBytes()
	{
		Camera cameraMatrix = gameObject.GetComponent<Camera>();
		RenderTexture displayRenderTexture = cameraMatrix.activeTexture;
		RenderTexture screenshotRenderTexture = new RenderTexture(Width, Height, 24);
		cameraMatrix.targetTexture = screenshotRenderTexture;
		Texture2D screenshotTexture = new Texture2D(Width, Height, TextureFormat.RGB24, false);
		cameraMatrix.Render();
		RenderTexture.active = screenshotRenderTexture;
		screenshotTexture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
		cameraMatrix.targetTexture = displayRenderTexture;
		RenderTexture.active = null;  
		DestroyImmediate(screenshotRenderTexture);
		byte[] bytes = screenshotTexture.EncodeToPNG();
		return bytes;

	}
}

