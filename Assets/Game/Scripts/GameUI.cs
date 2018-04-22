using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
	private const int width = 300;
	private const int height = 200;
	private void OnGUI()
	{
		switch (Game.instance.GetState())
		{
			case GameState.Gameplay: OnGameplayGUI(); break;
			case GameState.Win: OnEndGUI("You win!"); break;
			case GameState.Lose: OnEndGUI("You lose!"); break;
		}
	}
	
	private void OnEndGUI(string text)
	{
		Rect ui_area = new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height);
		
		GUILayout.BeginArea(ui_area);
		GUILayout.Label(text);
		if (GUILayout.Button("Restart"))
			Game.instance.Restart();
		
		GUILayout.EndArea();
	}
	
	private void OnGameplayGUI()
	{
		Rect ui_area = new Rect((Screen.width - width) / 2, Screen.height - height, width, height);
		
		GUILayout.BeginArea(ui_area);
		GUILayout.BeginHorizontal("box");
		foreach(Car car in Game.instance.GetCars())
		{
			GUILayout.BeginVertical("box");
			
			GUILayout.Label(string.Format("Gear: {0}", car.GetGear()));
			GUILayout.Label(string.Format("Lap: {0}", car.GetLap()));
			GUILayout.FlexibleSpace();
			GUILayout.Label(string.Format("Health: {0} / {1}", car.GetHealth(), car.max_health));
			GUILayout.Label(string.Format("AP: {0} / {1}", car.GetAPLeft(), car.max_ap));
			
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
		
		Car selected_car = Game.instance.GetSelectedCar();
		if (selected_car != null)
		{
			GUI.enabled = selected_car.CanGearUp();
			if (GUILayout.Button("Gear up"))
				selected_car.GearUp();
			
			GUI.enabled = selected_car.CanGearDown();
			if (GUILayout.Button("Gear down"))
				selected_car.GearDown();
			
			GUI.enabled = true;
		}
		
		GUILayout.FlexibleSpace();
		
		if (GUILayout.Button("End Turn"))
			Game.instance.Turn();
		
		GUILayout.EndArea();
		
		if (Event.current.type == EventType.Repaint)
			Game.instance.ui_in_use = ui_area.Contains(Event.current.mousePosition);
	}
}
