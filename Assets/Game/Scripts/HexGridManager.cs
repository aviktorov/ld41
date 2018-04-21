using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridManager : MonoSingleton<HexGridManager>
{
	private const int grid_mask = 1 << 8;
	
	public GameObject cell_prefab = null;
	public Material default_material = null;
	public float cell_size = 0.5f;
	public int grid_size = 10;
	
	private List<MeshRenderer> highlighted_cells = new List<MeshRenderer>();
	
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
	
	private void Update()
	{
		ClearHighlights();
	}
	
	public void HighlightCellAxial(Vector2 axial, Color color)
	{
		Vector3 cartesian = HexGrid.AxialToCartesian(axial, cell_size);
		cartesian.y = 100.0f;
		Ray ray = new Ray(cartesian, Vector3.down);
		RaycastHit hit;
		
		if (!Physics.Raycast(ray, out hit, Mathf.Infinity, grid_mask))
			return;
		
		if (!hit.collider)
			return;
		
		GameObject gameObject = hit.collider.gameObject;
		if (gameObject.tag != "GridCell")
			return;
		
		MeshRenderer mesh_renderer = gameObject.GetComponentInChildren<MeshRenderer>();
		if (!mesh_renderer)
			return;
		
		if (!highlighted_cells.Contains(mesh_renderer))
			highlighted_cells.Add(mesh_renderer);
		
		mesh_renderer.material.color = color;
	}
	
	public void HighlightCellCube(Vector3 cube, Color color)
	{
		Vector2 axial = HexGrid.CubeToAxial(cube);
		HighlightCellAxial(axial, color);
	}
	
	public void ClearHighlights()
	{
		foreach (MeshRenderer renderer in highlighted_cells)
		{
			DestroyImmediate(renderer.material);
			renderer.sharedMaterial = default_material;
		}
		
		highlighted_cells.Clear();
	}
}
