using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoSingleton<Game> {
	
	public Transform cursor = null;
	public Camera game_camera = null;
	public RaceTrack track = null;
	
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
		if (selected_car)
			selected_car.GetSteerPositions(steer_positions);
		
		// cell selection
		if(selected_car && Input.GetMouseButtonDown(1) && !ui_in_use)
		{
			Vector3 cell_position = HexGrid.CartesianToCubeRounded(hit.point, size);
			foreach (Vector3 position in steer_positions)
			{
				if (position != cell_position)
					continue;
				
				selected_car.SetDesiredPosition(cell_position);
				break;
			}
		}
		
		// highlights
		Color checkpoint_highlight_color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
		
		foreach (Vector3 position in checkpoints.Keys)
			HexGridManager.instance.HighlightCellCube(position, checkpoint_highlight_color);
		
		if (selected_car == null)
			return;
		
		Color steer_highlight_color = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		Color desired_highlight_color = new Color(0.5f, 0.5f, 0.0f, 1.0f);
		
		foreach (Vector3 position in steer_positions)
			HexGridManager.instance.HighlightCellCube(position, steer_highlight_color);
		
		HexGridManager.instance.HighlightCellCube(selected_car.GetDesiredPosition(), desired_highlight_color);
	}
	
	public void NextCheckpoint(Car car)
	{
		int current_checkpoint = car.GetCheckpoint();
		int current_lap = car.GetLap();
		
		int next_checkpoint = (current_checkpoint + 1) % track.GetNumCheckpoints();
		int next_lap = current_lap + 1;
		car.SetCheckpoint(next_checkpoint);
		
		if (current_checkpoint == 0)
			car.SetLap(next_lap);
		
		Debug.LogFormat("New checkpoint: {0}, lap: {1}", car.GetCheckpoint(), car.GetLap());
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
	
	public void Turn()
	{
		foreach(Car car in cars.Values)
			car.Turn();
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
