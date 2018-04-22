using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HighlightType
{
	Default,
	Selection,
	Steer,
	ActionableSteer,
	Collision,
	DamageArea,
	Obstacle,
	Checkpoint,
}

public enum IconType
{
	MovePoint,
	TargetPoint,
	HitDamage,
	CollisionDamage,
}

public class HexGridManager : MonoSingleton<HexGridManager>
{
	private const int grid_mask = 1 << 8;
	
	public Material default_material = null;
	public Material selection_material = null;
	public Material steer_material = null;
	public Material actionable_steer_material = null;
	public Material collision_material = null;
	public Material damage_area_material = null;
	public Material obstacle_material = null;
	public Material checkpoint_material = null;
	
	public GameObject move_point_prefab = null;
	public GameObject target_point_prefab = null;
	public GameObject hit_damage_prefab = null;
	public GameObject collision_damage_prefab = null;
	
	public GameObject cell_prefab = null;
	
	public float cell_size = 0.5f;
	public int grid_size = 10;
	
	private Dictionary<int, MeshRenderer> highlighted_cells = new Dictionary<int, MeshRenderer>();
	private List<GameObject> icons = new List<GameObject>();
	
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
		ClearIcons();
		ClearHighlights();
	}
	
	public void HighlightCellCube(Vector3 cube, HighlightType type)
	{
		Vector3 cartesian = HexGrid.CubeToCartesian(cube, cell_size);
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
		
		if (!highlighted_cells.ContainsKey(gameObject.GetInstanceID()))
			highlighted_cells.Add(gameObject.GetInstanceID(), mesh_renderer);
		
		switch(type)
		{
			case HighlightType.Default: mesh_renderer.sharedMaterial = default_material; break;
			case HighlightType.Selection: mesh_renderer.sharedMaterial = selection_material; break;
			case HighlightType.Steer: mesh_renderer.sharedMaterial = steer_material; break;
			case HighlightType.ActionableSteer: mesh_renderer.sharedMaterial = actionable_steer_material; break;
			case HighlightType.Collision: mesh_renderer.sharedMaterial = collision_material; break;
			case HighlightType.Obstacle: mesh_renderer.sharedMaterial = obstacle_material; break;
			case HighlightType.Checkpoint: mesh_renderer.sharedMaterial = checkpoint_material; break;
		}
	}
	
	public void AddCellIconCube(Vector3 cube, IconType type)
	{
		Vector3 cartesian = HexGrid.CubeToCartesian(cube, cell_size);
		
		GameObject prefab = null;
		
		switch(type)
		{
			case IconType.MovePoint: prefab = move_point_prefab; break;
			case IconType.TargetPoint: prefab = target_point_prefab; break;
			case IconType.HitDamage: prefab = hit_damage_prefab; break;
			case IconType.CollisionDamage: prefab = collision_damage_prefab; break;
		}
		
		if (!prefab)
			return;
		
		icons.Add(GameObject.Instantiate(prefab, cartesian, Quaternion.identity, transform) as GameObject);
	}
	
	public void ClearIcons()
	{
		foreach(GameObject icon in icons)
			Destroy(icon);
		
		icons.Clear();
	}
	
	public void ClearHighlights()
	{
		foreach (MeshRenderer renderer in highlighted_cells.Values)
			renderer.sharedMaterial = default_material;
		
		highlighted_cells.Clear();
	}
}
