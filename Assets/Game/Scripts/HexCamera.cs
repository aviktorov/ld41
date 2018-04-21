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

	}
}
