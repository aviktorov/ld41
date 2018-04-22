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
	
	private int gear = 0;
	
	private int current_checkpoint = 0;
	private int current_lap = -1;
	
	private int health = 0;
	private int ap = 0;
	
	private Vector3 cube_coordinates;
	private Vector3 desired_cube_coordinates;
	
	private float heading;
	private float desired_heading;
	
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
		
		SetDesiredPosition(cube_coordinates);
	}
	
	private void Update()
	{
		float cell_size = HexGridManager.instance.cell_size;
		
		Vector3 target_car_direction = new Vector3(Mathf.Cos(heading * Mathf.Deg2Rad), 0.0f, Mathf.Sin(heading * Mathf.Deg2Rad));
		Vector3 target_car_position = HexGrid.CubeToCartesian(cube_coordinates, cell_size);
		
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(target_car_direction), smoothness * Time.deltaTime);
		transform.position = Vector3.Lerp(transform.position, target_car_position, smoothness * Time.deltaTime);
		
		// TODO: all tweens here
	}
	
	public void Turn()
	{
		int distance = (int)HexGrid.GetCubeDistance(desired_cube_coordinates, cube_coordinates);
		Vector3 traced_cube_coordinates = cube_coordinates;
		
		// trace car path
		if (distance > 0)
		{
			for (int i = 0; i <= distance; i++)
			{
				float k = (float)i / distance;
				traced_cube_coordinates = Vector3.Lerp(cube_coordinates, desired_cube_coordinates, k);
				traced_cube_coordinates = HexGrid.GetCubeRounded(traced_cube_coordinates);
				
				if (Game.instance.IsValidCheckpoint(traced_cube_coordinates, current_checkpoint))
					Game.instance.NextCheckpoint(this);
				
				// TODO: check for obstacles
			}
		}
		
		cube_coordinates = traced_cube_coordinates;
		heading = desired_heading;
		
		ap = max_ap;
		
		UpdateDesiredPosition();
	}
	
	private void UpdateDesiredPosition()
	{
		desired_cube_coordinates = cube_coordinates;
		desired_heading = heading;
		
		List<Vector3> positions = new List<Vector3>();
		GetSteerPositions(positions);
		
		if (positions.Count == 0)
			return;
		
		float cos_half_steering_arc = Mathf.Cos(30.0f * Mathf.Deg2Rad);
		float cell_size = HexGridManager.instance.cell_size;
		
		Vector3 car_position = HexGrid.CubeToCartesian(cube_coordinates, cell_size);
		Vector3 car_direction = new Vector3(Mathf.Cos(heading * Mathf.Deg2Rad), 0.0f, Mathf.Sin(heading * Mathf.Deg2Rad));
		
		// find optimal position
		float max_dot = -1.0f;
		
		foreach (Vector3 position in positions)
		{
			Vector3 cartesian = HexGrid.CubeToCartesian(position, cell_size);
			Vector3 direction = (cartesian - car_position).normalized;
			
			float dot = Vector3.Dot(direction, car_direction);
			if (cos_half_steering_arc > dot)
				continue;
			
			if (max_dot < dot)
			{
				max_dot = dot;
				desired_cube_coordinates = position;
				desired_heading = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
			}
		}
	}
	
	public void SetDesiredPosition(Vector3 position)
	{
		float cell_size = HexGridManager.instance.cell_size;
		
		desired_cube_coordinates = position;
		desired_heading = heading;
		
		if (position != cube_coordinates)
		{
			Vector3 car_position = HexGrid.CubeToCartesian(cube_coordinates, cell_size);
			Vector3 new_car_position = HexGrid.CubeToCartesian(position, cell_size);
			Vector3 new_car_direction = (new_car_position - car_position).normalized;
			
			desired_heading = Mathf.Atan2(new_car_direction.z, new_car_direction.x) * Mathf.Rad2Deg;
		}
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
		
		Vector3 car_direction = new Vector3(Mathf.Cos(heading * Mathf.Deg2Rad), 0.0f, Mathf.Sin(heading * Mathf.Deg2Rad));
		Vector3 car_position = HexGrid.CubeToCartesian(cube_coordinates, cell_size);
		
		Vector3 ring_cube_coordinates = HexGrid.GetOffsetCube(cube_coordinates, 4, speed);
		
		// get hex ring with 'speed' radius
		for (int side = 0; side < 6; side++)
		{
			for (int i = 0; i < speed; i++)
			{
				// TODO: trace & filter collision positions
				// filter positions by steering angle
				Vector3 cartesian = HexGrid.CubeToCartesian(ring_cube_coordinates, cell_size);
				Vector3 direction = (cartesian - car_position).normalized;
				
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
