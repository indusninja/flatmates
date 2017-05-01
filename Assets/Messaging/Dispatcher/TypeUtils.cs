using System;
using UnityEngine;
public static class TypeUtils
{
	public static byte TypeToByte(Type type)
	{
		return (byte)TypeUtils.GetDataType(type);
	}
	public static Type ByteToType(byte b)
	{
		return Type.GetType(((DataType)b).ToString());
	}
	public static DataType GetDataType(Type type)
	{
		if (type == typeof(int))
		{
			return DataType.Int32;
		}
		if (type == typeof(float))
		{
			return DataType.Single;
		}
		if (type == typeof(string))
		{
			return DataType.String;
		}
		if (type == typeof(Vector2))
		{
			return DataType.Vector2;
		}
		if (type == typeof(Vector3))
		{
			return DataType.Vector3;
		}
		if (type == typeof(Vector4))
		{
			return DataType.Vector4;
		}
		if (type == typeof(Quaternion))
		{
			return DataType.Quaternion;
		}
		return DataType.Int32;
	}
	public static DataType ByteToDataType(byte b)
	{
		return (DataType)b;
	}
}
