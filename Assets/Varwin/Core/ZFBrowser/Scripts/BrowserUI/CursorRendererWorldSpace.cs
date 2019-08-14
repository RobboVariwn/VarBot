using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{

	/// <summary>
	/// Renders a browser's cursor by add polygons to the scene offset from browser's face.
	/// </summary>
	public class CursorRendererWorldSpace : CursorRendererBase
	{
		[Tooltip("How far to keep the cursor from the surface of the browser. Set it as low as you can without causing z-fighting."
		         + " (Note: The default cursor material will always render on top of everything, this is more useful if you use a different material.")]
		public float zOffset = .005f;

		[Tooltip("How large should the cursor be when rendered? (meters)")]
		public float size = .1f;

		private GameObject cursorHolder, cursorImage;
		public float cursorSize = 0.1f;

		private PointerUIBase pointerUI;

		//public GameObject menu;
		private bool cursorVisible;

		public void Awake()
		{
			InitCursor();
		}

		private void InitCursor()
		{
			pointerUI = GetComponent<PointerUIBase>();
			cursorHolder = new GameObject("Cursor for " + name);
			cursorHolder.transform.localScale = new Vector3(size, size, size);
			//If we place it in the parent, the scale can get wonky in some cases.
			//so don't cursorHolder.transform.parent = transform;

			cursorImage = GameObject.CreatePrimitive(PrimitiveType.Quad);
			cursorImage.name = "Cursor Image";
			cursorImage.transform.parent = cursorHolder.transform;
			var mr = cursorImage.GetComponent<MeshRenderer>();
			mr.sharedMaterial = Resources.Load<Material>("Browser/CursorDecal");
			Destroy(cursorImage.GetComponent<Collider>());
			cursorImage.transform.SetParent(cursorHolder.transform, false);
			cursorImage.transform.localPosition = Vector3.zero;
			cursorImage.transform.localScale = new Vector3(cursorSize, cursorSize, cursorSize);
			cursorHolder.SetActive(false);
		}

		protected override void CursorChange()
		{
			if (cursorImage == null || cursor == null)
			{
				return;
			}
			
			if (cursor.HasMouse && cursor.Texture)
			{
				cursorVisible = true;

				var cursorRenderer = cursorImage.GetComponent<Renderer>();
				cursorRenderer.material.mainTexture = cursor.Texture;

				var hs = cursor.Hotspot;
				//cursorRenderer.transform.localPosition = new Vector3(
				//	.5f - hs.x / (cursor.Texture.width * cursorSize),
				//	-.5f + hs.y / (cursor.Texture.height * cursorSize),
				//	0
				//);
			}
			else
			{
				cursorVisible = false;
			}
		}

		public void LateUpdate()
		{
			if (pointerUI == null || cursorHolder == null)
			{
				InitCursor();
			}
			
			Vector3 pos;
			Quaternion rot;
			pointerUI.GetCurrentHitLocation(out pos, out rot);
			 

			if (float.IsNaN(pos.x))
			{
				cursorHolder.SetActive(false);
			}
			else
			{
				cursorHolder.SetActive(cursorVisible);

				cursorHolder.transform.position = pos + rot * new Vector3(0, 0, -zOffset);
				cursorHolder.transform.rotation = rot;
				 
			}

		}
	}

}