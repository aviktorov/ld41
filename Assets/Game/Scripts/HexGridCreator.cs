using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridCreator : MonoBehaviour
{
	public GameObject cell_prefab = null;
	public float cell_size = 0.5f;
	public int grid_size = 10;
	
	private void Start()
	{
		if (!cell_prefab)
			return;
		
		Vector2 axial = new Vector2();
		int half_grid_size = grid_size / 2;
		for (int x = -half_grid_size; x < half_grid_size; x++)
		{
			axial.x = x;
			for (int y = -half_grid_size; y < half_grid_size; y++)
			{
				axial.y = y;
				Vector3 cartesian = HexGrid.AxialToCartesian(axial, cell_size);
				
				GameObject.Instantiate(cell_prefab, cartesian, Quaternion.identity, transform);
			}
		}
	}
}
