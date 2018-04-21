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
	
	public float smoothness = 5.0f;
	
	[Range(0, 360.0f)]
	public float heading = 0.0f;
	
	[Range(0, 4)]
	public int gear = 0;
	
	public CarGear[] gears;
	
	private int health = 0;
	private int ap = 0;
	
	private Vector3 cube_coordinates;
	private Vector3 desired_cube_coordinates;
	private float desired_heading;
	private float target_heading;
	private Vector3 car_position;
	private Vector3 target_car_position;
	private Vector3 car_direction;
	
	private void Awake()
	{
		Game.instance.RegisterCar(this);
		health = max_health;
		ap = max_ap;
	}
	
	private void Destroy()
	{
		Game.instance.UnregisterCar(this);
	}
	
	private void Start()
	{
		float cell_size = HexGridManager.instance.cell_size;
		
		cube_coordinates = HexGrid.CartesianToCubeRounded(transform.position, cell_size);
		target_heading = heading;
		
		SetDesiredPosition(cube_coordinates);
		
		target_car_position = HexGrid.CubeToCartesian(desired_cube_coordinates, cell_size);
	}
	
	private void Update()
	{
		heading = Mathf.Lerp(heading, target_heading, smoothness * Time.deltaTime);
		
		car_position = Vector3.Lerp(car_position, target_car_position, smoothness * Time.deltaTime);
		car_direction = new Vector3(Mathf.Cos(heading * Mathf.Deg2Rad), 0.0f, Mathf.Sin(heading * Mathf.Deg2Rad));
		
		transform.position = car_position;
		transform.LookAt(car_position + car_direction);
		
		// TODO: all tweens here
	}
	
	public void Turn()
	{
		float cell_size = HexGridManager.instance.cell_size;
		
		car_position = HexGrid.CubeToCartesian(cube_coordinates, cell_size);
		
		target_car_position = HexGrid.CubeToCartesian(desired_cube_coordinates, cell_size);
		target_heading = desired_heading;
		
		cube_coordinates = desired_cube_coordinates;
		ap = max_ap;
		
		// Update desired position
	}
	
	public void SetDesiredPosition(Vector3 position)
	{
		float cell_size = HexGridManager.instance.cell_size;
		
		Vector3 car_position = HexGrid.CubeToCartesian(cube_coordinates, cell_size);
		Vector3 new_car_position = HexGrid.CubeToCartesian(position, cell_size);
		Vector3 new_car_direction = (new_car_position - car_position).normalized;
		
		desired_cube_coordinates = position;
		desired_heading = Mathf.Atan2(new_car_direction.z, new_car_direction.x) * Mathf.Rad2Deg;
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
		
		Vector3 car_position = HexGrid.CubeToCartesian(cube_coordinates, cell_size);
		Vector3 car_direction = new Vector3(Mathf.Cos(heading * Mathf.Deg2Rad), 0.0f, Mathf.Sin(heading * Mathf.Deg2Rad));
		
		Vector3 ring_cube_coordinates = HexGrid.GetOffsetCube(cube_coordinates, 4, speed);
		
		// get hex ring with 'speed' radius
		for (int side = 0; side < 6; side++)
		{
			for (int i = 0; i < speed; i++)
			{
				// filter positions by steering angle
				Vector3 cartesian = HexGrid.CubeToCartesian(ring_cube_coordinates, cell_size);
				Vector3 direction = (cartesian - car_position).normalized;
				
				if (Vector3.Dot(direction, car_direction) > cos_half_steering_arc)
					positions.Add(ring_cube_coordinates);
				
				ring_cube_coordinates = HexGrid.GetNeighborCube(ring_cube_coordinates, side);
			}
		}
	}
	
	public void GearUp()
	{
		if (gears == null)
			return;
		
		gear = Mathf.Min(gear + 1, gears.Length);
	}
	
	public void GearDown()
	{
		if (gears == null)
			return;
		
		gear = Mathf.Max(gear - 1, 0);
	}
}
