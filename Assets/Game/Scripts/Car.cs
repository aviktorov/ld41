using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JamSuite;

[System.Serializable]
public struct CarGear
{
	public int speed;
	public float steering_arc;
}

public class Car : MonoBehaviour
{
	public int max_health = 5;
	public int max_ap = 2;
	[Range(0, 4)]
	public int gear = 0;
	
	[Range(0, 360.0f)]
	public float heading = 0.0f;
	
	public CarGear[] gears;
	
	private int health = 0;
	private int ap = 0;
	
	private Vector3 car_cube_coordinates;
	private Vector3 car_position;
	private Vector3 car_direction;
	
	private void Awake()
	{
		health = max_health;
		ap = max_ap;
	}
	
	private void Start()
	{
		float cell_size = HexGridManager.instance.cell_size;
		
		car_cube_coordinates = HexGrid.CartesianToCubeRounded(transform.position, cell_size);
		car_position = HexGrid.CubeToCartesian(car_cube_coordinates, cell_size);
	}
	
	private void Update()
	{
		car_direction = new Vector3(Mathf.Cos(heading * Mathf.Deg2Rad), 0.0f, Mathf.Sin(heading * Mathf.Deg2Rad));
		
		transform.position = car_position;
		transform.LookAt(car_position + car_direction);
		
		List<Vector3> positions = new List<Vector3>();
		GetSteerPositions(positions);
		foreach (Vector3 position in positions)
			HexGridManager.instance.HighlightCellCube(position);
	}
	
	public void Turn()
	{
		// TODO: all logic here
	}
	
	public void GetSteerPositions(List<Vector3> positions)
	{
		positions.Clear();
		
		if (gears == null)
			return;
		
		int speed = gears[gear].speed;
		if (speed == 0)
			return;
		
		float cos_half_steering_arc = Mathf.Cos(gears[gear].steering_arc * 0.5f * Mathf.Deg2Rad);
		float cell_size = HexGridManager.instance.cell_size;
		
		Vector3 current_cube_coordinates = HexGrid.GetOffsetCube(car_cube_coordinates, 4, speed);
		
		// get hex ring with 'speed' radius
		for (int side = 0; side < 6; side++)
		{
			for (int i = 0; i < speed; i++)
			{
				// filter positions by steering angle
				Vector3 cartesian = HexGrid.CubeToCartesian(current_cube_coordinates, cell_size);
				Vector3 direction = (cartesian - car_position).normalized;
				Debug.DrawRay(car_position, car_direction, Color.red, 5.0f);
				Debug.DrawRay(car_position, direction, Color.green, 5.0f);
				
				if (Vector3.Dot(direction, car_direction) > cos_half_steering_arc)
					positions.Add(current_cube_coordinates);
				
				current_cube_coordinates = HexGrid.GetNeighborCube(current_cube_coordinates, side);
			}
		}
	}
	
	public void GearUp()
	{
		if (gears == null)
			return;
		
		gear = Mathf.Min(gear + 1, gears.Length);
	}
	
	public void DecreaseSpeed()
	{
		if (gears == null)
			return;
		
		gear = Mathf.Max(gear - 1, 0);
	}
}
