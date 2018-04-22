using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoSingleton<Game> {
	
	public Transform cursor = null;
	public Camera game_camera = null;
	public RaceTrack track = null;
	
	public Color checkpoint_highlight_color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
	public Color obstacle_highlight_color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
	public Color prohibited_steer_highlight_color = new Color(0.5f, 0.5f, 0.5f, 0.9f);
	public Color available_steer_highlight_color = new Color(0.8f, 0.8f, 0.8f, 1.0f);
	public Color desired_highlight_color = new Color(0.5f, 0.5f, 0.0f, 1.0f);
	public Color collision_highlight_color = new Color(0.7f, 0.1f, 0.1f, 1.0f);

	private Car selected_car = null;
	private Dictionary<int, Car> cars = new Dictionary<int, Car>();
	private Dictionary<Vector3, int> checkpoints = new Dictionary<Vector3, int>();
	private Dictionary<Vector3, int> obstacles = new Dictionary<Vector3, int>();
	
	public bool ui_in_use = false;
	
	private void Update () {
		// raycast
		RaycastHit hit;
		Ray ray = game_camera.ScreenPointToRay(Input.mousePosition);
		bool intersected = Physics.Raycast(ray, out hit);
		float size = HexGridManager.instance.cell_size;
		
		Car intersected_car = null;
		
		// cursor position
		if (intersected)
		{
			Vector3 cube_rounded = HexGrid.CartesianToCubeRounded(hit.point, size);
			Vector3 cartesian_rounded = HexGrid.CubeToCartesian(cube_rounded, size);
			
			cursor.position = cartesian_rounded;
			
			if (hit.collider.tag == "Car")
				intersected_car = hit.collider.gameObject.GetComponent<Car>();
		}
		
		// car selection
		if(Input.GetMouseButtonDown(0) && !ui_in_use)
			selected_car = intersected_car;
		
		List<Vector3> steer_positions = new List<Vector3>();
		List<Vector3> all_steer_positions = new List<Vector3>();
		if (selected_car)
		{
			selected_car.GetAvailableSteerPositions(steer_positions);
			selected_car.GetAllSteerPositions(all_steer_positions);
		}
		
		// cell selection
		if(selected_car && Input.GetMouseButtonDown(1) && !ui_in_use)
		{
			Vector3 cell_position = HexGrid.CartesianToCubeRounded(hit.point, size);
			Vector3 current_position = selected_car.GetCurrentPosition();
			
			foreach (Vector3 position in steer_positions)
			{
				Vector3 traced_position = TracePath(selected_car, current_position, position);
				if (traced_position != cell_position)
					continue;
				
				selected_car.SetDesiredPosition(position);
				break;
			}
		}
		
		// track
		foreach (Vector3 position in checkpoints.Keys)
			HexGridManager.instance.HighlightCellCube(position, checkpoint_highlight_color);
		
		foreach (Vector3 position in obstacles.Keys)
			HexGridManager.instance.HighlightCellCube(position, obstacle_highlight_color);
		
		// car
		if (selected_car != null)
		{
			Vector3 current_position = selected_car.GetCurrentPosition();
			Vector3 desired_position = selected_car.GetDesiredPosition();
			
			foreach (Vector3 position in all_steer_positions)
			{
				Vector3 traced_position = TracePath(selected_car, current_position, position);
				bool is_available = steer_positions.Contains(position);
				bool is_desired = position == desired_position;
				bool has_interruptions = traced_position != position;
				
				Color color = (is_available) ? available_steer_highlight_color : prohibited_steer_highlight_color;
				if (is_desired)
					color = desired_highlight_color;
				
				if (has_interruptions)
					color = collision_highlight_color;
				
				HexGridManager.instance.HighlightCellCube(traced_position, color);
			}
		}
	}
	
	public Vector3 TraceCarPath(Car car)
	{
		Vector3 p0 = car.GetCurrentPosition();
		Vector3 p1 = car.GetDesiredPosition();
		int current_checkpoint = car.GetCheckpoint();
		
		int distance = (int)HexGrid.GetCubeDistance(p0, p1);
		Vector3 traced_cube_coordinates = p0;
		
		if (distance == 0)
			return traced_cube_coordinates;
		
		for (int i = 0; i <= distance; i++)
		{
			float k = Mathf.Clamp01((float)i / distance);
			float next_k = Mathf.Clamp01((float)(i + 1) / distance);
			traced_cube_coordinates = Vector3.Lerp(p0, p1, k);
			traced_cube_coordinates = HexGrid.GetCubeRounded(traced_cube_coordinates);
			
			Vector3 next_traced_cube_coordinates = Vector3.Lerp(p0, p1, next_k);
			next_traced_cube_coordinates = HexGrid.GetCubeRounded(next_traced_cube_coordinates);
			
			// checkpoints
			if (IsValidCheckpoint(traced_cube_coordinates, current_checkpoint))
				OnCheckpointReached(car);
			
			// static obstacles
			if (IsObstacle(next_traced_cube_coordinates))
			{
				OnObstacleCollision(car);
				break;
			}
			
			// dynamic obstacles (cars, projectiles, beams, etc.)
			Car other_car = GetIntersectedCar(car, next_traced_cube_coordinates);
			if (other_car)
			{
				OnCarCollision(car, other_car);
				break;
			}
		}
		
		return traced_cube_coordinates;
	}
	
	public Car GetIntersectedCar(Car car, Vector3 p)
	{
		foreach(Car other_car in cars.Values)
		{
			if(other_car == car)
				continue;
			
			Vector3 p0 = other_car.GetCurrentPosition();
			Vector3 p1 = other_car.GetDesiredPosition();
			
			if (HasIntersection(p0, p1, p))
				return other_car;
		}
		
		return null;
	}
	
	public bool HasIntersection(Vector3 p0, Vector3 p1, Vector3 p)
	{
		int distance = (int)HexGrid.GetCubeDistance(p0, p1);
		Vector3 traced_cube_coordinates = p0;
		
		if (distance == 0)
			return (p == p0);
		
		for (int i = 0; i <= distance; i++)
		{
			float k = (float)i / distance;
			traced_cube_coordinates = Vector3.Lerp(p0, p1, k);
			traced_cube_coordinates = HexGrid.GetCubeRounded(traced_cube_coordinates);
			
			if (traced_cube_coordinates == p)
				return true;
		}
		
		return false;
	}
	
	public Vector3 TracePath(Car car, Vector3 p0, Vector3 p1)
	{
		int distance = (int)HexGrid.GetCubeDistance(p0, p1);
		Vector3 traced_cube_coordinates = p0;
		
		if (distance == 0)
			return traced_cube_coordinates;
		
		for (int i = 0; i <= distance; i++)
		{
			float k = (float)i / distance;
			traced_cube_coordinates = Vector3.Lerp(p0, p1, k);
			traced_cube_coordinates = HexGrid.GetCubeRounded(traced_cube_coordinates);
			
			// static obstacles
			if (IsObstacle(traced_cube_coordinates))
				break;
			
			// dynamic obstacles (cars, projectiles, beams, etc.)
			Car other_car = GetIntersectedCar(car, traced_cube_coordinates);
			if (other_car)
				break;
		}
		
		return traced_cube_coordinates;
	}
	
	public void OnCheckpointReached(Car car)
	{
		int current_checkpoint = car.GetCheckpoint();
		int current_lap = car.GetLap();
		
		int next_checkpoint = (current_checkpoint + 1) % track.GetNumCheckpoints();
		int next_lap = current_lap + 1;
		car.SetCheckpoint(next_checkpoint);
		
		if (current_checkpoint == 0)
			car.SetLap(next_lap);
	}
	
	public void OnObstacleCollision(Car car)
	{
		car.OnObstacleCollision();
	}
	
	public void OnCarCollision(Car car, Car other_car)
	{
		car.OnCarCollision(other_car);
		other_car.OnCarCollision(car);
	}
	
	public void AddCheckpoint(Vector3 cube_coordinates, int checkpoint)
	{
		int id = 0;
		if (!checkpoints.TryGetValue(cube_coordinates, out id))
			checkpoints.Add(cube_coordinates, checkpoint);
		
		checkpoints[cube_coordinates] = checkpoint;
	}
	
	public bool IsValidCheckpoint(Vector3 cube_coordinates, int checkpoint)
	{
		int id = 0;
		if (!checkpoints.TryGetValue(cube_coordinates, out id))
			return false;
		
		return id == checkpoint;
	}
	
	public void AddObstacle(Vector3 cube_coordinates)
	{
		int id = 0;
		if (!obstacles.TryGetValue(cube_coordinates, out id))
			obstacles.Add(cube_coordinates, 0);
		
		obstacles[cube_coordinates] = 0;
	}
	
	public bool IsObstacle(Vector3 cube_coordinates)
	{
		int id = 0;
		if (!obstacles.TryGetValue(cube_coordinates, out id))
			return false;
		
		return true;
	}
	
	public void RegisterCar(Car car)
	{
		cars.Add(car.gameObject.GetInstanceID(), car);
	}
	
	public void UnregisterCar(Car car)
	{
		cars.Remove(car.gameObject.GetInstanceID());
	}
	
	public Dictionary<int, Car>.ValueCollection GetCars()
	{
		return cars.Values;
	}
	
	public void UpdateDesiredPositions()
	{
		foreach (Car car in cars.Values)
			car.UpdateDesiredPosition();
	}
	
	public void Turn()
	{
		foreach(Car car in cars.Values)
			car.BeginTurn();
		
		foreach(Car car in cars.Values)
			car.EndTurn();
		
		UpdateDesiredPositions();
	}
	
	public Car GetSelectedCar()
	{
		return selected_car;
	}
	
	public void SelectCar(Car car)
	{
		selected_car = car;
	}
}
