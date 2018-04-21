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
	public CarGear[] gears;
	
	private float heading = 0.0f;
	private int gear = 0;
	
	private int current_checkpoint = 0;
	private int current_lap = -1;
	
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
		
		Vector3 car_direction = transform.forward;
		heading = Mathf.Atan2(car_direction.z, car_direction.x) * Mathf.Rad2Deg;
		
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
		
		int distance = (int)HexGrid.GetCubeDistance(desired_cube_coordinates, cube_coordinates);
		Vector3 lerped_cube_coordinates = cube_coordinates;
		
		// trace car path
		for (int i = 0; i <= distance; i++)
		{
			float k = (float)i / distance;
			lerped_cube_coordinates = Vector3.Lerp(cube_coordinates, desired_cube_coordinates, k);
			lerped_cube_coordinates = HexGrid.GetCubeRounded(lerped_cube_coordinates);
			
			if (Game.instance.IsValidCheckpoint(lerped_cube_coordinates, current_checkpoint))
				Game.instance.NextCheckpoint(this);
			
			// TODO: check for obstacles
		}
		
		cube_coordinates = lerped_cube_coordinates;
		
		target_car_position = HexGrid.CubeToCartesian(cube_coordinates, cell_size);
		target_heading = desired_heading;
		
		ap = max_ap;
		
		UpdateDesiredPosition();
	}
	
	private void UpdateDesiredPosition()
	{
		desired_cube_coordinates = cube_coordinates;
		
		if (gears == null)
			return;
		
		int speed = gears[gear].speed;
		if (speed == 0)
			return;
		
		float cos_half_steering_arc = Mathf.Cos(7.5f * Mathf.Deg2Rad);
		float cell_size = HexGridManager.instance.cell_size;
		
		Vector3 car_direction = new Vector3(Mathf.Cos(target_heading * Mathf.Deg2Rad), 0.0f, Mathf.Sin(target_heading * Mathf.Deg2Rad));
		Vector3 ring_cube_coordinates = HexGrid.GetOffsetCube(cube_coordinates, 4, speed);
		
		// get hex ring with 'speed' radius
		for (int side = 0; side < 6; side++)
		{
			for (int i = 0; i < speed; i++)
			{
				// filter positions by steering angle
				Vector3 cartesian = HexGrid.CubeToCartesian(ring_cube_coordinates, cell_size);
				Vector3 direction = (cartesian - target_car_position).normalized;
				
				if (Vector3.Dot(direction, car_direction) > cos_half_steering_arc)
					desired_cube_coordinates = ring_cube_coordinates;
				
				ring_cube_coordinates = HexGrid.GetNeighborCube(ring_cube_coordinates, side);
			}
		}
		
		// TODO: check for obstacles
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
	
	public Vector3 GetDesiredPosition()
	{
		return desired_cube_coordinates;
	}
	
	public void SetCheckpoint(int num)
	{
		current_checkpoint = num;
	}
	
	public int GetCheckpoint()
	{
		return current_checkpoint;
	}
	
	public void SetLap(int lap)
	{
		current_lap = lap;
	}
	
	public int GetLap()
	{
		return current_lap;
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
		
		Vector3 car_direction = new Vector3(Mathf.Cos(target_heading * Mathf.Deg2Rad), 0.0f, Mathf.Sin(target_heading * Mathf.Deg2Rad));
		
		Vector3 ring_cube_coordinates = HexGrid.GetOffsetCube(cube_coordinates, 4, speed);
		
		// get hex ring with 'speed' radius
		for (int side = 0; side < 6; side++)
		{
			for (int i = 0; i < speed; i++)
			{
				// filter positions by steering angle
				Vector3 cartesian = HexGrid.CubeToCartesian(ring_cube_coordinates, cell_size);
				Vector3 direction = (cartesian - target_car_position).normalized;
				
				if (Vector3.Dot(direction, car_direction) > cos_half_steering_arc)
					positions.Add(ring_cube_coordinates);
				
				ring_cube_coordinates = HexGrid.GetNeighborCube(ring_cube_coordinates, side);
			}
		}
	}
	
	public int GetGear()
	{
		return gear;
	}
	
	public void GearUp()
	{
		if (gears == null)
			return;
		
		gear = Mathf.Min(gear + 1, gears.Length - 1);
		UpdateDesiredPosition();
	}
	
	public void GearDown()
	{
		if (gears == null)
			return;
		
		gear = Mathf.Max(gear - 1, 0);
		UpdateDesiredPosition();
	}
}
