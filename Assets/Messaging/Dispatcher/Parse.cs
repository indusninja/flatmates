using System;
using UnityEngine;
public static class Parse
{
	public static Vector2 Vector2(string str)
	{
		str = str.Replace('(', ' ');
		str = str.Replace(')', ' ');
		string[] array = str.Split(new char[]
		{
			','
		});
		return new Vector2(float.Parse(array[0]), float.Parse(array[1]));
	}
	public static Vector3 Vector3(string str)
	{
		str = str.Replace('(', ' ');
		str = str.Replace(')', ' ');
		string[] array = str.Split(new char[]
		{
			','
		});
		return new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
	}
	public static Vector4 Vector4(string str)
	{
		str = str.Replace('(', ' ');
		str = str.Replace(')', ' ');
		string[] array = str.Split(new char[]
		{
			','
		});
		return new Vector4(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
	}
	public static Quaternion Quaternion(string str)
	{
		Vector4 vector = Parse.Vector4(str);
		return new Quaternion(vector.x, vector.y, vector.z, vector.w);
	}
}
