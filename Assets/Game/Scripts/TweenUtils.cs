using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TweenUtils
{
	public static float Smoothstep(float t)
	{
		float tt = t * t;
		return tt * (3 - 2 * t);
	}
	
	public static float Smootherstep(float t)
	{
		float ttt = t * t * t;
		return ttt * (t * (t * 6 - 15) + 10);
	}
}
