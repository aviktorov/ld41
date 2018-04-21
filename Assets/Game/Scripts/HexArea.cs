using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexAreaType
{
	Start,
	Checkpoint,
	Obstacle,
};

public class HexArea : MonoBehaviour
{
	public HexAreaType type = HexAreaType.Checkpoint;
	public Color highlight_color = Color.red;
	
	private Collider cached_collider;
	
	private void Start()
	{

	}
}
