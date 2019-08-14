using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {

/// <summary>
/// A BrowserUI that tracks pointer interaction through a camera to a mesh of some sort.
/// </summary>
[RequireComponent(typeof(MeshCollider))]
public class PointerUIMesh : PointerUIBase {
	protected MeshCollider meshCollider;

	protected Dictionary<int, RaycastHit> rayHits = new Dictionary<int, RaycastHit>();

	[Tooltip("Which layers should UI rays collide with (and be able to hit)?")]
	public LayerMask layerMask = -1;

	public override void Awake() {
		base.Awake();
		meshCollider = GetComponent<MeshCollider>();
	}

    protected override Vector2 MapPointerToBrowser(Vector2 screenPosition, int pointerId)
    {
        try
        {
            var camera = viewCamera ? viewCamera : Camera.main;
            return MapRayToBrowser(camera.ScreenPointToRay(screenPosition), pointerId);
        }
        catch
        {
            return Vector2.zero;
        }

    }

    protected override Vector2 MapRayToBrowser(Ray worldRay, int pointerId) {
		RaycastHit hit;
		var rayHit = Physics.Raycast(worldRay, out hit, maxDistance, layerMask);

		//store hit data for GetCurrentHitLocation
		rayHits[pointerId] = hit;

		if (!rayHit || hit.collider.transform != meshCollider.transform) {
			//not aimed at it
			return new Vector3(float.NaN, float.NaN);
		} else {
			return hit.textureCoord;
		}
	}

	public override void GetCurrentHitLocation(out Vector3 pos, out Quaternion rot) {
		if (currentPointerId == 0 || !rayHits.ContainsKey(currentPointerId)) {
			//no pointer
			pos = new Vector3(float.NaN, float.NaN, float.NaN);
			rot = Quaternion.identity;
			return;
		}

		var hitInfo = rayHits[currentPointerId];

		if (hitInfo.collider == null)
		{
			pos = new Vector3(float.NaN, float.NaN, float.NaN);
			rot = Quaternion.identity;
			return;
		}
		 
		var up = hitInfo.collider.transform.up;

		pos = hitInfo.point;
		rot = Quaternion.LookRotation(-hitInfo.normal, up);
	}

}

}
