using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
public static class BinarySaver
{
	public static void ClearFile()
	{
	}
	public static void WriteBinFile(object[] data)
	{
		IFormatter formatter = new BinaryFormatter();
		Stream stream = new FileStream(Application.persistentDataPath + "/bb2.bin", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		for (int i = 0; i < data.Length; i++)
		{
			object graph = data[i];

			MemoryStream memoryStream = new MemoryStream();
			formatter.Serialize(memoryStream, graph);
			byte[] buffer = memoryStream.GetBuffer();
			byte[] bytes = BitConverter.GetBytes(buffer.Length);
			stream.Write(bytes, 0, 4);
			stream.Write(buffer, 0, buffer.Length);
			memoryStream.Close();
		}
		stream.Close();
	}
	public static object[] ReadBinFile()
	{	
		if (!File.Exists(Application.persistentDataPath + "/bb2.bin"))
		{
			return null;
		}
		List<object> list = new List<object>();
		Stream stream = new FileStream(Application.persistentDataPath + "/bb2.bin", FileMode.Open, FileAccess.Read, FileShare.None);
		IFormatter formatter = new BinaryFormatter();
		while (stream.Position < stream.Length)
		{
			byte[] array = new byte[4];
			stream.Read(array, 0, 4);
			int num = (int)BitConverter.ToUInt32(array, 0);
			array = new byte[num];
			stream.Read(array, 0, num);
			MemoryStream memoryStream = new MemoryStream(num);
			memoryStream.Write(array, 0, num);
			memoryStream.Position = 0L;
			object item = formatter.Deserialize(memoryStream);
			list.Add(item);
			memoryStream.Close();
		}
		stream.Close();
		return list.ToArray();
	}
}
