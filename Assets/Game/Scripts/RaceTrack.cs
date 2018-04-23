using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTrack : MonoBehaviour
{
	public int num_laps = 5;
	
	private void Start()
	{
		// TODO: calculate paths for AI
		for (int i = 0; i < transform.childCount; i++)
			AddCheckpoints(i);
	}
	
	private void AddCheckpoints(int id)
	{
		Transform child = transform.GetChild(id);
		Collider child_collider = child.GetComponent<Collider>();
		if (!child_collider)
			return;
		
		Vector3 min = child_collider.bounds.min;
		Vector3 max = child_collider.bounds.max;
		float cell_size = HexGridManager.instance.cell_size;
		
		for(float x = min.x; x < max.x; x += cell_size)
		{
			for (float z = min.z; z < max.z; z += cell_size)
			{
				Ray ray = new Ray(new Vector3(x, 100.0f, z), Vector3.down);
				RaycastHit hit;
				if (child_collider.Raycast(ray, out hit, Mathf.Infinity))
					Game.instance.AddCheckpoint(HexGrid.CartesianToCubeRounded(ray.origin, cell_size), id);
			}
		}
		
		child.gameObject.GetComponent<Renderer>().enabled = false;
	}
	
	public int GetNumCheckpoints()
	{
		return transform.childCount;
	}
	
	private void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			Transform next_child = transform.GetChild((i + 1) % transform.childCount);
			
			Gizmos.DrawLine(child.position, next_child.position);
		}
	}
}
