using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
	[Range(0, 90)]
	public float vertical_angle = 80.0f;
	
	[Range(0, 360)]
	public float horizontal_angle = 0.0f;
	
	public float radius = 15.0f;
	public float smoothness = 5.0f;
	
	public Transform target = null;
	
	private Vector3 target_position;
	
	private void Start()
	{
		if (!target)
			return;
		
		target_position = target.position;
	}
	private void LateUpdate()
	{
		if (!target)
			return;
		
		float sin_horizontal = Mathf.Sin(horizontal_angle * Mathf.Deg2Rad);
		float cos_horizontal = Mathf.Cos(horizontal_angle * Mathf.Deg2Rad);
		
		float sin_vertical = Mathf.Sin(vertical_angle * Mathf.Deg2Rad);
		float cos_vertical = Mathf.Cos(vertical_angle * Mathf.Deg2Rad);
		
		Vector3 desired_position = target.position;
		desired_position += new Vector3(cos_horizontal * cos_vertical, sin_vertical, sin_horizontal * cos_vertical) * radius;
		
		target_position = Vector3.Lerp(target_position, target.position, smoothness * Time.deltaTime);
		transform.position = Vector3.Lerp(transform.position, desired_position, smoothness * Time.deltaTime);
		
		transform.LookAt(target_position);
	}
}
