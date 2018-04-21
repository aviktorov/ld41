using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexGrid
{
	private const float sqrt3 = 1.73205080757f;
	private const float sqrt3_div_2 = sqrt3 * 0.5f;
	private const float sqrt3_div_3 = sqrt3 / 3.0f;
	
	private static Vector2[] axial_directions = new Vector2[6]
	{
		new Vector2(+1, 0), new Vector2(+1, -1), new Vector2(0, -1),
		new Vector2(-1, 0), new Vector2(-1, +1), new Vector2(0, +1),
	};
	
	private static Vector3[] cube_directions = new Vector3[6]
	{
		new Vector3(+1, -1, 0), new Vector3(+1, 0, -1), new Vector3(0, +1, -1),
		new Vector3(-1, +1, 0), new Vector3(-1, 0, +1), new Vector3(0, -1, +1),
	};
	
	public static Vector2 GetNeighborAxial(Vector2 axial, int direction)
	{
		return axial + axial_directions[direction];
	}
	
	public static Vector2 GetOffsetAxial(Vector2 axial, int direction, float scale)
	{
		return axial + axial_directions[direction] * scale;
	}
	
	public static Vector3 GetNeighborCube(Vector3 cube, int direction)
	{
		return cube + cube_directions[direction];
	}
	
	public static Vector3 GetOffsetCube(Vector3 cube, int direction, float scale)
	{
		return cube + cube_directions[direction] * scale;
	}
	
	public static Vector3 AxialToCube(Vector2 axial)
	{
		return new Vector3(axial.x, -(axial.x + axial.y), axial.y);
	}
	
	public static Vector2 CubeToAxial(Vector3 cube)
	{
		return new Vector2(cube.x, cube.z);
	}
	
	public static Vector3 GetCubeRounded(Vector3 cube)
	{
		int rx = (int)Mathf.Round(cube.x);
		int ry = (int)Mathf.Round(cube.y);
		int rz = (int)Mathf.Round(cube.z);
		
		float diff_x = Mathf.Abs(cube.x - rx);
		float diff_y = Mathf.Abs(cube.y - ry);
		float diff_z = Mathf.Abs(cube.z - rz);
		
		if (diff_x > diff_y && diff_x > diff_z)
			rx = -(ry + rz);
		else if (diff_y > diff_z)
			ry = -(rx + rz);
		else
			rz = -(rx + ry);
		
		return new Vector3(rx, ry, rz);
	}
	
	public static float GetCubeDistance(Vector3 p0, Vector3 p1)
	{
		float diff_x = Mathf.Abs(p0.x - p1.x);
		float diff_y = Mathf.Abs(p0.y - p1.y);
		float diff_z = Mathf.Abs(p0.z - p1.z);
		
		return Mathf.Max(diff_x, Mathf.Max(diff_y, diff_z));
	}
	
	public static Vector2 GetAxialRounded(Vector2 axial)
	{
		Vector3 cube = AxialToCube(axial);
		Vector3 cube_rounded = GetCubeRounded(cube);
		
		return CubeToAxial(cube_rounded);
	}
	
	public static Vector3 AxialToCartesian(Vector2 axial, float size)
	{
		float x = sqrt3 * axial.x + sqrt3_div_2 * axial.y;
		float z = 3.0f / 2.0f * axial.y;
		
		return new Vector3(x, 0.0f, z) * size;
	}
	
	public static Vector3 CubeToCartesian(Vector3 cube, float size)
	{
		return AxialToCartesian(CubeToAxial(cube), size);
	}
	
	public static Vector2 CartesianToAxial(Vector3 cartesian, float size)
	{
		float x = sqrt3_div_3 * cartesian.x - 1.0f / 3.0f * cartesian.z;
		float y = 2.0f / 3.0f * cartesian.z;
		
		return new Vector2(x, y) / size;
	}
	
	public static Vector3 CartesianToCube(Vector3 cartesian, float size)
	{
		return AxialToCube(CartesianToAxial(cartesian, size));
	}
	
	public static Vector2 CartesianToAxialRounded(Vector3 cartesian, float size)
	{
		return GetAxialRounded(CartesianToAxial(cartesian, size));
	}
	
	public static Vector3 CartesianToCubeRounded(Vector3 cartesian, float size)
	{
		return GetCubeRounded(CartesianToCube(cartesian, size));
	}
}
