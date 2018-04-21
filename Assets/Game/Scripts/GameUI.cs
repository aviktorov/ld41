using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
	private const int width = 300;
	private const int height = 200;
	private void OnGUI()
	{
		Rect ui_area = new Rect((Screen.width - width) / 2, Screen.height - height, width, height);
		
		GUILayout.BeginArea(ui_area);
		GUILayout.BeginHorizontal("box");
		foreach(Car car in Game.instance.GetCars())
		{
			GUILayout.BeginVertical("box");
			
			GUILayout.Label(string.Format("Gear: {0}", car.GetGear()));
			GUILayout.Label(string.Format("Lap: {0}", car.GetLap()));
			
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
		
		Car selected_car = Game.instance.GetSelectedCar();
		if (selected_car != null)
		{
			if (GUILayout.Button("Gear up"))
				selected_car.GearUp();
			
			if (GUILayout.Button("Gear down"))
				selected_car.GearDown();
		}
		
		GUILayout.FlexibleSpace();
		
		if (GUILayout.Button("End Turn"))
			Game.instance.Turn();
		
		GUILayout.EndArea();
		
		if (Event.current.type == EventType.Repaint)
			Game.instance.ui_in_use = ui_area.Contains(Event.current.mousePosition);
	}
}
