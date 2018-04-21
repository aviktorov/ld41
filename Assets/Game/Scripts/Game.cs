using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoSingleton<Game> {
	
	public Transform cursor = null;
	public Camera game_camera = null;
	
	private Car selected_car = null;
	private Dictionary<int, Car> cars = new Dictionary<int, Car>();
	
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
		HexGridManager.instance.ClearHighlights();
		
		if (selected_car == null)
			return;
		
		List<Vector3> positions = new List<Vector3>();
		selected_car.GetSteerPositions(positions);
		foreach (Vector3 position in positions)
			HexGridManager.instance.HighlightCellCube(position);
	}
	
	public void RegisterCar(Car car)
	{
		cars.Add(car.gameObject.GetInstanceID(), car);
	}
	
	public void UnregisterCar(Car car)
	{
		cars.Remove(car.gameObject.GetInstanceID());
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
