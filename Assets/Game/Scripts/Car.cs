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
	public int team = 0;
	public Color team_color = Color.white;
	
	public int max_health = 5;
	public int max_ap = 2;
	
	public float smoothness = 5.0f;
	public CarGear[] gears;
	
	private int current_checkpoint = 0;
	private int current_lap = 0;
	
	private int health = 0;
	
	private int gear = 0;
	private int desired_gear = 0;
	private int turn_gear = 0;
	
	private Vector3 cube_coordinates;
	private Vector3 desired_cube_coordinates;
	private Vector3 desired_forward_cube_coordinates;
	private Vector3 turn_cube_coordinates;
	
	private float heading;
	private float desired_heading;
	private float turn_heading;
	
	private Vector3 tween_position;
	private Quaternion tween_rotation;
	
	private void Awake()
	{
		Game.instance.RegisterCar(this);
		health = max_health;
	}
	
	private void Destroy()
	{
		Game.instance.UnregisterCar(this);
	}
	
	private void Start()
	{
		float cell_size = HexGridManager.instance.cell_size;
		
		heading = Mathf.Atan2(transform.forward.z, transform.forward.x) * Mathf.Rad2Deg;
		cube_coordinates = HexGrid.CartesianToCubeRounded(transform.position, cell_size);
		transform.position = HexGrid.CubeToCartesian(cube_coordinates, cell_size);
		
		desired_forward_cube_coordinates = cube_coordinates;
		desired_cube_coordinates = cube_coordinates;
		desired_heading = heading;
	}
	
	private void Update()
	{
		if (Game.instance.GetState() != GameState.EndTurn)
			return;
		
		float tween_time = Game.instance.GetTurnTweenTimeNormalized();
		tween_time = TweenUtils.Smootherstep(tween_time);
		
		float cell_size = HexGridManager.instance.cell_size;
		
		Vector3 target_direction = new Vector3(Mathf.Cos(heading * Mathf.Deg2Rad), 0.0f, Mathf.Sin(heading * Mathf.Deg2Rad));
		Vector3 target_position = HexGrid.CubeToCartesian(cube_coordinates, cell_size);
		
		transform.rotation = Quaternion.Lerp(tween_rotation, Quaternion.LookRotation(target_direction), tween_time);
		transform.position = Vector3.Lerp(tween_position, target_position, tween_time);
		
		// TODO: all tweens here
	}
	
	public void BeginTurn()
	{
		turn_gear = desired_gear;
		turn_heading = desired_heading;
		turn_cube_coordinates = Game.instance.TraceCarPath(this);
	}
	
	public void EndTurn()
	{
		cube_coordinates = turn_cube_coordinates;
		heading = turn_heading;
		gear = turn_gear;
		desired_gear = turn_gear;
		
		tween_position = transform.position;
		tween_rotation = transform.rotation;
		
		UpdateDesiredPosition();
	}
	
	public void OnObstacleCollision()
	{
		Damage(desired_gear);
		turn_gear = 0;
	}
	
	public void OnCarCollision(Car car)
	{
		// TODO: better damage
		int damage = (car.GetDesiredGear() + desired_gear) / 2;
		Damage(damage);
		
		// TODO: better gear
		turn_gear = 0;
		
		// TODO: modify heading?
	}
	
	public void OnCarHit(Turret turret)
	{
		Damage(turret.damage);
		
		// TODO: modify gear?
	}
	
	public void Damage(int damage)
	{
		if (health == 0)
			return;
		
		health = Mathf.Max(health - damage, 0);
	}
	
	public void UpdateDesiredPosition()
	{
		desired_cube_coordinates = cube_coordinates;
		desired_forward_cube_coordinates = cube_coordinates;
		desired_heading = heading;
		
		List<Vector3> positions = new List<Vector3>();
		GetAllSteerPositions(positions);
		
		if (positions.Count == 0)
			return;
		
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
			if (max_dot < dot)
			{
				max_dot = dot;
				desired_cube_coordinates = position;
				desired_forward_cube_coordinates = position;
				desired_heading = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
			}
		}
	}
	
	private int GetSteerDistance(Vector3 position)
	{
		int steer_distance = (int)HexGrid.GetCubeDistance(desired_forward_cube_coordinates, position);
		return Mathf.Min(steer_distance, 2);
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
	
	public Vector3 GetCurrentPosition()
	{
		return cube_coordinates;
	}
	
	public Vector3 GetDesiredPosition()
	{
		return desired_cube_coordinates;
	}
	
	public int GetHealth()
	{
		return health;
	}
	
	public int GetAPUsed()
	{
		return Mathf.Abs(desired_gear - gear) + GetSteerDistance(desired_cube_coordinates);
	}
	
	public int GetAPLeft()
	{
		return max_ap - GetAPUsed();
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
	
	public void GetAvailableSteerPositions(List<Vector3> positions)
	{
		List<Vector3> steer_positions = new List<Vector3>();
		GetAllSteerPositions(steer_positions);
		
		positions.Clear();
		
		Vector3 temp = desired_cube_coordinates;
		desired_cube_coordinates = desired_forward_cube_coordinates;
		
		int ap_left = GetAPLeft();
		
		desired_cube_coordinates = temp;
		
		if (ap_left <= 0)
		{
			positions.Add(desired_forward_cube_coordinates);
			return;
		}
		
		foreach(Vector3 position in steer_positions)
		{
			int steer_distance = GetSteerDistance(position);
			
			if (steer_distance > ap_left)
				continue;
			
			positions.Add(position);
		}
	}
	
	public void GetAllSteerPositions(List<Vector3> positions)
	{
		positions.Clear();
		
		if (gears == null)
			return;
		
		CarGear car_gear = gears[desired_gear];
		int speed = car_gear.speed;
		if (speed == 0)
			return;
		
		float cos_half_steering_arc = Mathf.Cos(car_gear.steering_arc * 0.5f * Mathf.Deg2Rad);
		float cell_size = HexGridManager.instance.cell_size;
		
		Vector3 car_direction = new Vector3(Mathf.Cos(heading * Mathf.Deg2Rad), 0.0f, Mathf.Sin(heading * Mathf.Deg2Rad));
		Vector3 car_position = HexGrid.CubeToCartesian(cube_coordinates, cell_size);
		
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
	
	public void SetGear(int g)
	{
		gear = g;
	}
	
	public int GetGear()
	{
		return gear;
	}
	
	public int GetDesiredGear()
	{
		return desired_gear;
	}
	
	public bool CanGearUp()
	{
		if (gears == null)
			return false;
		
		if (desired_gear >= gears.Length - 1)
			return false;
		
		desired_gear++;
		int delta = (max_ap - GetAPUsed());
		desired_gear--;
		
		return delta >= 0;
	}
	
	public bool CanGearDown()
	{
		if (gears == null)
			return false;
		
		if (desired_gear <= 0)
			return false;
		
		desired_gear--;
		int delta = (max_ap - GetAPUsed());
		desired_gear++;
		
		return delta >= 0;
	}
	
	public void GearUp()
	{
		if (gears == null)
			return;
		
		desired_gear = Mathf.Min(desired_gear + 1, gears.Length - 1);
		UpdateDesiredPosition();
	}
	
	public void GearDown()
	{
		if (gears == null)
			return;
		
		desired_gear = Mathf.Max(desired_gear - 1, 0);
		UpdateDesiredPosition();
	}
}
