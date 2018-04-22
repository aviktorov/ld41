﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
	Intro,
	Gameplay,
	EndTurn,
	Win,
	Lose,
}

public class Game : MonoSingleton<Game> {
	
	public Transform cursor = null;
	public Camera game_camera = null;
	public RaceTrack track = null;
	
	public int player_team = 0;
	
	public float turn_tween_duration = 1.0f;
	public bool ui_in_use = false;
	
	private Car selected_car = null;
	private float current_turn_tween_time = 0.0f;
	
	private Dictionary<int, Car> cars = new Dictionary<int, Car>();
	private Dictionary<int, Turret> turrets = new Dictionary<int, Turret>();
	private Dictionary<Vector3, int> checkpoints = new Dictionary<Vector3, int>();
	private Dictionary<Vector3, int> obstacles = new Dictionary<Vector3, int>();
	
	private GameState state = GameState.Intro;
	
	private void ProcessIntro()
	{
		// TODO: intro
		state = GameState.Gameplay;
	}
	
	private void ProcessGameplay()
	{
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
			
			foreach(Car car in cars.Values)
			{
				if (car.GetCurrentPosition() != cube_rounded)
					continue;
				
				intersected_car = car;
				break;
			}
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
			
			bool collision = false;
			foreach (Vector3 position in steer_positions)
			{
				Vector3 traced_position = TracePath(selected_car, current_position, position, out collision);
				if (traced_position != cell_position)
					continue;
				
				selected_car.SetDesiredPosition(position);
				break;
			}
		}
		
		// selected car
		if (selected_car != null)
		{
			Vector3 current_position = selected_car.GetCurrentPosition();
			Vector3 desired_position = selected_car.GetDesiredPosition();
			
			HexGridManager.instance.HighlightCellCube(current_position, HighlightType.Selection);
			
			bool collision = false;
			foreach (Vector3 position in all_steer_positions)
			{
				Vector3 traced_position = TracePath(selected_car, current_position, position, out collision);
				bool is_available = steer_positions.Contains(position);
				
				HighlightType type = (is_available) ? HighlightType.ActionableSteer : HighlightType.Steer;
				
				bool is_desired = (position == desired_position);
				if (is_desired)
					HexGridManager.instance.AddCellIconCube(traced_position, IconType.MovePoint);
				
				if (collision)
					HexGridManager.instance.AddCellIconCube(traced_position, IconType.CollisionDamage);
				
				HexGridManager.instance.HighlightCellCube(traced_position, type);
			}
		}
		
		foreach(Turret turret in turrets.Values)
		{
			if (!turret.IsReadyToFire())
				continue;
			
			Vector3 fire_position = turret.GetFirePosition();
			
			HexGridManager.instance.HighlightCellCube(fire_position, HighlightType.Collision);
			HexGridManager.instance.AddCellIconCube(fire_position, IconType.HitDamage);
		}
		
		// check win / lose conditions
		bool all_enemies_dead = true;
		bool all_players_dead = true;
		
		bool player_reached_finish = false;
		bool enemy_reached_fiinsh = false;
		
		foreach(Car car in cars.Values)
		{
			if (car.GetLap() >= track.num_laps)
			{
				if (car.team == player_team)
					player_reached_finish = true;
				else
					enemy_reached_fiinsh = true;
			}
			
			if (car.GetHealth() != 0)
			{
				if (car.team == player_team)
					all_players_dead = false;
				else
					all_enemies_dead = false;
			}
		}
		
		if (all_enemies_dead || player_reached_finish)
			state = GameState.Win;
		else if (all_players_dead || enemy_reached_fiinsh)
			state = GameState.Lose;
	}
	
	private void ProcessEndTurn()
	{
		current_turn_tween_time += Time.deltaTime;
		if (current_turn_tween_time > turn_tween_duration)
			state = GameState.Gameplay;
	}
	
	private void ProcessWin()
	{
		// TODO: UI tween
	}
	
	private void ProcessLose()
	{
		// TODO: UI tween
	}
	
	private void Update()
	{
		// visualize track
		foreach (Vector3 position in checkpoints.Keys)
			HexGridManager.instance.HighlightCellCube(position, HighlightType.Checkpoint);
		
		foreach (Vector3 position in obstacles.Keys)
			HexGridManager.instance.HighlightCellCube(position, HighlightType.Obstacle);
		
		// process game logic
		switch(state)
		{
			case GameState.Intro: ProcessIntro(); break;
			case GameState.Gameplay: ProcessGameplay(); break;
			case GameState.EndTurn: ProcessEndTurn(); break;
			case GameState.Win: ProcessWin(); break;
			case GameState.Lose: ProcessLose(); break;
		}
	}
	
	public void Restart()
	{
		SceneManager.LoadScene("Main");
	}
	
	public GameState GetState()
	{
		return state;
	}
	
	public float GetTurnTweenTime()
	{
		return current_turn_tween_time;
	}
	
	public float GetTurnTweenTimeNormalized()
	{
		return Mathf.Clamp01(current_turn_tween_time / turn_tween_duration);
	}
	
	public Vector3 TraceCarPath(Car car)
	{
		Vector3 p0 = car.GetCurrentPosition();
		Vector3 p1 = car.GetDesiredPosition();
		int current_checkpoint = car.GetCheckpoint();
		
		int distance = (int)HexGrid.GetCubeDistance(p0, p1);
		Vector3 traced_cube_coordinates = p0;
		Vector3 prev_traced_cube_coordinates = p0;
		
		if (distance == 0)
			return traced_cube_coordinates;
		
		for (int i = 0; i <= distance; i++)
		{
			prev_traced_cube_coordinates = traced_cube_coordinates;
			
			float k = Mathf.Clamp01((float)i / distance);
			traced_cube_coordinates = Vector3.Lerp(p0, p1, k);
			traced_cube_coordinates = HexGrid.GetCubeRounded(traced_cube_coordinates);
			
			// checkpoints
			if (IsValidCheckpoint(traced_cube_coordinates, current_checkpoint))
				OnCheckpointReached(car);
			
			// static obstacles
			if (IsObstacle(traced_cube_coordinates))
			{
				traced_cube_coordinates = prev_traced_cube_coordinates;
				OnObstacleCollision(car);
				break;
			}
			
			// TODO: laser walls
		}
		
		// cars
		Car other_car = GetIntersectedCar(car, traced_cube_coordinates);
		if (other_car)
		{
			traced_cube_coordinates = prev_traced_cube_coordinates;
			OnCarCollision(car, other_car);
		}
		
		return traced_cube_coordinates;
	}
	
	public Car GetIntersectedCar(Car car, Vector3 p)
	{
		foreach(Car other_car in cars.Values)
		{
			if(other_car == car)
				continue;
			
			if (other_car.GetDesiredPosition() == p)
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
	
	public Vector3 TraceRay(Vector3 cartesian_position, Vector3 cartesian_direction, int start_cube_distance, int max_cube_distance)
	{
		float cell_size = HexGridManager.instance.cell_size;
		
		Vector3 p0 = HexGrid.CartesianToCube(cartesian_position, cell_size);
		Vector3 p1 = HexGrid.CartesianToCube(cartesian_position + cartesian_direction, cell_size);
		
		float distance = HexGrid.GetCubeDistance(p0, p1);
		Vector3 traced_cube_coordinates = Vector3.zero;
		
		for (int i = start_cube_distance; i <= max_cube_distance; i++)
		{
			float k = (float)i / distance;
			traced_cube_coordinates = p0 + (p1 - p0) * k;
			traced_cube_coordinates = HexGrid.GetCubeRounded(traced_cube_coordinates);
			
			// static obstacles
			if (IsObstacle(traced_cube_coordinates))
				break;
			
			// dynamic obstacles (cars, projectiles, beams, etc.)
			Car other_car = GetIntersectedCar(null, traced_cube_coordinates);
			if (other_car != null)
				break;
		}
		
		return traced_cube_coordinates;
	}
	
	public Vector3 TracePath(Car car, Vector3 p0, Vector3 p1, out bool collision)
	{
		collision = false;
		int distance = (int)HexGrid.GetCubeDistance(p0, p1);
		Vector3 traced_cube_coordinates = p0;
		
		if (distance == 0)
			return traced_cube_coordinates;
		
		for (int i = 0; i <= distance; i++)
		{
			float k = Mathf.Clamp01((float)i / distance);
			traced_cube_coordinates = Vector3.Lerp(p0, p1, k);
			traced_cube_coordinates = HexGrid.GetCubeRounded(traced_cube_coordinates);
			
			// static obstacles
			if (IsObstacle(traced_cube_coordinates))
			{
				collision = true;
				break;
			}
			
			// TODO: laser walls
		}
		
		// cars
		Car other_car = GetIntersectedCar(car, traced_cube_coordinates);
		if (other_car)
			collision = true;
		
		return traced_cube_coordinates;
	}
	
	public Car PredictClosestTarget(Vector3 cube_coordinates, int radius)
	{
		Car target = null;
		int target_distance = radius + 1;
		
		foreach(Car car in cars.Values)
		{
			int distance = (int)HexGrid.GetCubeDistance(car.GetDesiredPosition(), cube_coordinates);
			if (distance < target_distance)
			{
				target = car;
				target_distance = distance;
			}
		}
		return target;
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
	
	public void OnCarHit(Car car, Turret turret)
	{
		car.OnCarHit(turret);
	}
	
	public void OnTurretFire(Turret turret)
	{
		foreach(Car car in cars.Values)
			if (car.GetCurrentPosition() == turret.GetFirePosition())
				OnCarHit(car, turret);
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
		if (obstacles.ContainsKey(cube_coordinates))
			return true;
		
		foreach(Turret turret in turrets.Values)
			if (turret.GetCurrentPosition() == cube_coordinates)
				return true;
		
		return false;
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
	
	public void RegisterTurret(Turret turret)
	{
		turrets.Add(turret.gameObject.GetInstanceID(), turret);
	}
	
	public void UnregisterTurret(Turret turret)
	{
		turrets.Remove(turret.gameObject.GetInstanceID());
	}
	
	public void Turn()
	{
		foreach(Car car in cars.Values)
			car.BeginTurn();
		
		foreach(Car car in cars.Values)
			car.EndTurn();
		
		foreach(Turret turret in turrets.Values)
			turret.BeginTurn();

		foreach(Turret turret in turrets.Values)
			turret.EndTurn();
		
		state = GameState.EndTurn;
		current_turn_tween_time = 0.0f;
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
