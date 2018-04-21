using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCamera : MonoBehaviour
{
	
	public Transform hex_cursor = null;
	
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
		
		float size = HexGridManager.instance.cell_size;
		
		Vector3 cube_rounded = HexGrid.CartesianToCubeRounded(hit.point, size);
		Vector3 cartesian_rounded = HexGrid.CubeToCartesian(cube_rounded, size);
		
		hex_cursor.transform.position = cartesian_rounded;
	}
}
