using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCamera : MonoBehaviour
{
	
	public Transform hex_cursor = null;
	public float size = 1.0f;
	
	private Camera cached_camera = null;
	
	private void Awake()
	{
		cached_camera = GetComponent<Camera>();
	}
	
	private void Update()
	{
		Ray ray = cached_camera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		if (!Physics.Raycast(ray, out hit))
			return;
		
		Vector2 axial_rounded = HexGrid.CartesianToAxialRounded(hit.point, size);
		Vector3 cartesian_rounded = HexGrid.AxialToCartesian(axial_rounded, size);
		
		// restore up position
		cartesian_rounded.y = hit.point.y;
		
		hex_cursor.transform.position = cartesian_rounded;
	}
}
