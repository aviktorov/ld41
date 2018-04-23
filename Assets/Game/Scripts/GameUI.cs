using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
	public GameObject game_ui = null;
	public GameObject end_ui = null;
	
	public Text[] end_texts = null;
	public RectTransform hp_rect = null;
	public Button gear_up_button = null;
	public Button gear_down_button = null;
	private float start_hp_width = 0.0f;
	
	private void Start()
	{
		start_hp_width = hp_rect.rect.width;
	}
	
	private void Update()
	{
		switch(Game.instance.GetState())
		{
			case GameState.Gameplay: ProcessGameplayUI(); break;
			case GameState.Win: ProcessEndUI("You win"); break;
			case GameState.Lose: ProcessEndUI("You lose"); break;
		}
	}
	
	private void ProcessEndUI(string end_text)
	{
		game_ui.SetActive(false);
		end_ui.SetActive(true);
		
		foreach(Text text in end_texts)
			text.text = end_text;
	}
	
	private void ProcessGameplayUI()
	{
		game_ui.SetActive(true);
		end_ui.SetActive(false);
		
		Car player_car = Game.instance.GetPlayerCar();
		float new_width = Mathf.Clamp01((float)player_car.GetHealth() / player_car.max_health) * start_hp_width;
		hp_rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, new_width);
		
		gear_up_button.interactable = player_car.CanGearUp();
		gear_down_button.interactable = player_car.CanGearDown();
		
		// TODO: update buttons
		// TODO: show promts & stuff
	}
	
	public void TryGearUp()
	{
		Car player_car = Game.instance.GetPlayerCar();
		if (!player_car.CanGearUp())
			return;
		
		player_car.GearUp();
	}
	
	public void TryGearDown()
	{
		Car player_car = Game.instance.GetPlayerCar();
		if (!player_car.CanGearDown())
			return;
		
		player_car.GearDown();
	}
}
