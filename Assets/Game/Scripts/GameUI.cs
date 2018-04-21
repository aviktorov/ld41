using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
	private void OnGUI()
	{
		GUILayout.BeginVertical("box");
		
		if (GUILayout.Button("End Turn"))
			Game.instance.Turn();
		
		Car car = Game.instance.GetSelectedCar();
		if (car != null)
		{
			if (GUILayout.Button("Gear up"))
				car.GearUp();
			
			if (GUILayout.Button("Gear down"))
				car.GearDown();
		}
		
		GUILayout.EndVertical();
		
		if (Event.current.type == EventType.Repaint)
			Game.instance.ui_in_use = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
	}
}
