using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JamSuite;

public class Turret : MonoBehaviour
{
	public int fire_cooldown = 2;
	public float turn_angle = 30.0f;
	public float fire_arc = 90.0f;
	public int fire_radius = 6;
	public int damage = 1;
	
	private Vector3 cube_coordinates;
	private Vector3 fire_coordinates;
	private int current_fire_cooldown = 0;
	public float heading = 0.0f;
	private bool want_to_fire = false;
	
	private Vector3 tween_position;
	private Quaternion tween_rotation;
	
	private void Awake()
	{
		Game.instance.RegisterTurret(this);
	}
	
	private void Destroy()
	{
		Game.instance.UnregisterTurret(this);
	}
	
	private void Start()
	{
		float cell_size = HexGridManager.instance.cell_size;
		
		heading = Mathf.Atan2(transform.forward.z, transform.forward.x) * Mathf.Rad2Deg;
		cube_coordinates = HexGrid.CartesianToCubeRounded(transform.position, cell_size);
		fire_coordinates = cube_coordinates;
		
		transform.position = HexGrid.CubeToCartesian(cube_coordinates, cell_size);
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
	}
	
	public Vector3 GetCurrentPosition()
	{
		return cube_coordinates;
	}
	
	public Vector3 GetFirePosition()
	{
		return fire_coordinates;
	}
	
	public bool IsReadyToFire()
	{
		return want_to_fire && (current_fire_cooldown == 0);
	}
	
	public void BeginTurn()
	{
		if (IsReadyToFire())
		{
			want_to_fire = false;
			Game.instance.OnTurretFire(this);
		}
		
		Car target = Game.instance.PredictClosestTarget(cube_coordinates, fire_radius);
		if (!target)
			return;
		
		// update turret heading
		float cell_size = HexGridManager.instance.cell_size;
		Vector3 car_position = HexGrid.CubeToCartesian(target.GetDesiredPosition(), cell_size);
		Vector3 target_direction = (car_position - transform.position).WithY(0.0f).normalized;
		
		float target_heading = Mathf.Atan2(target_direction.z, target_direction.x) * Mathf.Rad2Deg;
		float delta = Mathf.DeltaAngle(heading, target_heading);
		
		if (Mathf.Abs(delta) > turn_angle)
			delta = turn_angle * Mathf.Sign(delta);
		
		heading += delta;
		
		// update turret fire position
		Vector3 turret_direction = new Vector3(Mathf.Cos(heading * Mathf.Deg2Rad), 0.0f, Mathf.Sin(heading * Mathf.Deg2Rad));
		
		int fire_distance = (int)HexGrid.GetCubeDistance(cube_coordinates, target.GetDesiredPosition());
		fire_distance = Mathf.Min(fire_distance, fire_radius);
		fire_coordinates = Game.instance.TraceRay(transform.position, turret_direction, 1, fire_distance);
		
		// update turret fire conditions
		if (want_to_fire)
			return;
		
		float cos_half_fire_arc = Mathf.Cos(fire_arc * 0.5f * Mathf.Deg2Rad);
		
		if (Vector3.Dot(target_direction, turret_direction) > cos_half_fire_arc)
		{
			want_to_fire = true;
			current_fire_cooldown = fire_cooldown;
		}
	}
	
	public void EndTurn()
	{
		if (want_to_fire)
			current_fire_cooldown--;
		
		tween_position = transform.position;
		tween_rotation = transform.rotation;
	}
}
