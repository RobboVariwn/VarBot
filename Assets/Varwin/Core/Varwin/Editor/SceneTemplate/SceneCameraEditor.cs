using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Varwin.Editor;

[CustomEditor(typeof(SceneCamera))][CanEditMultipleObjects]
public class SceneCameraEditor : Editor
{

	private GameObject _gameObject;
	private SerializedProperty _widthProperty;
	private SerializedProperty _heightProperty;
	private int _width;
	private int _height;
	private string _screenshotDirectoryPath = "";

	private void OnEnable()
	{
		SceneCamera sceneCamera = (SceneCamera) target;

		if (sceneCamera != null)
		{
			_gameObject = sceneCamera.gameObject;
		}

		_widthProperty = serializedObject.FindProperty("Width");
		_heightProperty = serializedObject.FindProperty("Height");
		
	}

	public override void OnInspectorGUI()
	{
		EditorGUILayout.PropertyField(_widthProperty);
		EditorGUILayout.PropertyField(_heightProperty);
		
		if (GUILayout.Button("MakeShot"))
		{
			MakeShot();
		}
		
		if (GUILayout.Button("Move camera to editor view"))
		{
			Camera camera = _gameObject.GetComponent<Camera>();
			if (camera)
			{
				SceneUtils.MoveCameraToEditorView(camera);
			}
		}
		
		serializedObject.ApplyModifiedProperties();
	}

	private void MakeShot()
	{
		_width = _widthProperty.intValue;
		_height = _heightProperty.intValue;
		
		Camera cameraMatrix = _gameObject.GetComponent<Camera>();
		
		RenderTexture displayRenderTexture = cameraMatrix.activeTexture;

		RenderTexture screenshotRenderTexture = new RenderTexture(_width, _height, 24);
		cameraMatrix.targetTexture = screenshotRenderTexture;

		Texture2D screenshotTexture = new Texture2D(_width, _height, TextureFormat.RGB24, false);
		cameraMatrix.Render();

		RenderTexture.active = screenshotRenderTexture;
		screenshotTexture.ReadPixels(new Rect(0, 0, _width, _height), 0, 0);

		cameraMatrix.targetTexture = displayRenderTexture;
		RenderTexture.active = null; // JC: added to avoid errors
		DestroyImmediate(screenshotRenderTexture);

		byte[] bytes = screenshotTexture.EncodeToPNG();
            
		CheckScreenshotFolderParameters(); //нужно подвызывать, т.к., не сохраняются изменения OnStart

		string screenshotfileName = DateTime.Now.ToString("yy-MM-dd_HH_mm_ss_") + DateTime.Now.Millisecond.ToString("D") + ".png";
		string fullScreenshotFilePath = Path.Combine(_screenshotDirectoryPath, screenshotfileName);

		File.WriteAllBytes(fullScreenshotFilePath, bytes);
		Debug.Log(string.Format("Took screenshot to: {0}", fullScreenshotFilePath));
	}
	
	protected void CheckScreenshotFolderParameters()
	{
		if (_screenshotDirectoryPath == "")
		{
			_screenshotDirectoryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
		}

		if (!Directory.Exists(_screenshotDirectoryPath))
		{
			try
			{
				Directory.CreateDirectory(_screenshotDirectoryPath);
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
			}
            
		}
	}
}
