using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JamSuite;

public class ObstacleManager : MonoBehaviour
{
	public GameObject[] obstacle_prefabs = null;
	
	private void Start()
	{
		for (int i = 0; i < transform.childCount; i++)
			AddObstacles(i);
	}
	
	private void AddObstacles(int id)
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
				{
					Vector3 cube = HexGrid.CartesianToCubeRounded(ray.origin, cell_size);
					Vector3 corrected_cartesian = HexGrid.CubeToCartesian(cube, cell_size);
					
					Game.instance.AddObstacle(cube);
					
					GameObject prefab = null;
					if (obstacle_prefabs != null && obstacle_prefabs.Length > 0)
						prefab = obstacle_prefabs[Random.Range(0, obstacle_prefabs.Length)];
					
					if (prefab)
					{
						GameObject obstacle = GameObject.Instantiate(prefab, corrected_cartesian, Quaternion.identity) as GameObject;
						obstacle.transform.localScale = obstacle.transform.localScale.WithY(Random.Range(1.0f, 1.2f));
					}
				}
			}
		}
		
		// child.gameObject.GetComponent<Renderer>().enabled = false;
	}
	
	public int GetNumObstacles()
	{
		return transform.childCount;
	}

}
