using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using UnityEngine;
public class MessageSaver
{
	private static MessageSaver instance = new MessageSaver();
	private FileStream txtDoc;
	private XmlDocument xmlDoc;
	private Stream binStream;
	private MessageSaver()
	{
	}
	public static void Initialize()
	{
		Debug.Log("Open file for message saving");
		MessageSaver.instance.binStream = new FileStream(Application.persistentDataPath + "/Data.bin", FileMode.Create, FileAccess.Write, FileShare.None);
	}
	public static void DeInitialize()
	{
		if (MessageSaver.instance.txtDoc != null)
		{
			MessageSaver.instance.txtDoc.Close();
		}
		if (MessageSaver.instance.binStream != null)
		{
			MessageSaver.instance.binStream.Close();
		}
	}
	public static void WriteTextFile(Publisher publisher)
	{
		string text = "";
		text += publisher.domain;
		text = text + " " + publisher.message;
		MessageData[] data = publisher.data;
		for (int i = 0; i < data.Length; i++)
		{
			MessageData messageData = data[i];
			string[] array = messageData.GetType().ToString().Split(new char[]
			{
				'.'
			});
			object obj = text;
			text = string.Concat(new object[]
			{
				obj,
				" ",
				messageData,
				":",
				array[array.Length - 1]
			});
		}
		text += Environment.NewLine;
		byte[] array2 = new byte[text.Length];
		Encoding.ASCII.GetBytes(text.ToCharArray(), 0, text.Length, array2, 0);
		MessageSaver.instance.txtDoc.Write(array2, 0, text.Length);
	}
	public static List<Publisher> ReadTextFile()
	{
		List<Publisher> list = new List<Publisher>();
		string[] array = File.ReadAllLines("Data.txt");
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text = array2[i];
			string[] array3 = text.Split(new char[]
			{
				' '
			});
			if (array3.Length >= 3 && !string.IsNullOrEmpty(text))
			{
				MessageData[] array4 = new MessageData[array3.Length - 2];
				string domain = array3[0];
				string message = array3[1];
				int num = 0;
				for (int j = 2; j < array3.Length; j++)
				{
					string[] array5 = array3[j].Split(new char[]
					{
						':'
					});
					array4[num++].Write(array5[0], Type.GetType(array5[1]));
				}
				list.Add(new Publisher(domain, message, new object[]
				{
					1
				})
				{
					data = array4
				});
			}
		}
		return list;
	}
	public static void WriteBinFile(Publisher publisher)
	{
		IFormatter formatter = new BinaryFormatter();
		Stream stream = new FileStream("MyFile.bin", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		MemoryStream memoryStream = new MemoryStream();
		formatter.Serialize(memoryStream, publisher);
		byte[] buffer = memoryStream.GetBuffer();
		byte[] bytes = BitConverter.GetBytes(buffer.Length);
		stream.Write(bytes, 0, 4);
		stream.Write(buffer, 0, buffer.Length);
		memoryStream.Close();
		stream.Close();
	}
	public static List<Publisher> ReadBinFile()
	{
		List<Publisher> list = new List<Publisher>();
		Stream stream = new FileStream("MyFile.bin", FileMode.Open, FileAccess.Read, FileShare.None);
		IFormatter formatter = new BinaryFormatter();
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		int num = (int)BitConverter.ToUInt32(array, 0);
		array = new byte[num];
		stream.Read(array, 0, num);
		MemoryStream memoryStream = new MemoryStream(num);
		memoryStream.Write(array, 0, num);
		memoryStream.Position = 0L;
		Publisher item = (Publisher)formatter.Deserialize(memoryStream);
		list.Add(item);
		memoryStream.Close();
		stream.Close();
		return list;
	}
	public static void SaveBinary(Publisher publisher)
	{
		MemoryStream memoryStream = new MemoryStream();
		float value = Time.realtimeSinceStartup - Dispatcher.startTime;
		memoryStream.Write(BitConverter.GetBytes(value), 0, 4);
		value = Dispatcher.deltaTime;
		memoryStream.Write(BitConverter.GetBytes(Time.deltaTime), 0, 4);
		MessageSaver.WriteStringToBin(publisher.domain, memoryStream);
		MessageSaver.WriteStringToBin(publisher.message, memoryStream);
		if (publisher.data != null)
		{
			MessageData[] data = publisher.data;
			for (int i = 0; i < data.Length; i++)
			{
				MessageData messageData = data[i];
				MessageSaver.WriteDataToBin(messageData.var, memoryStream);
			}
		}
		byte[] buffer = BitConverter.GetBytes((int)memoryStream.Length);
		MessageSaver.instance.binStream.Write(buffer, 0, 4);
		buffer = memoryStream.GetBuffer();
		MessageSaver.instance.binStream.Write(buffer, 0, (int)memoryStream.Position);
		memoryStream.Close();
		MessageSaver.instance.binStream.Flush();
	}
	private static void WriteStringToBin(string s, MemoryStream stream)
	{
		stream.WriteByte((byte)s.Length);
		byte[] array = new byte[s.Length];
		Encoding.ASCII.GetBytes(s, 0, s.Length, array, 0);
		stream.Write(array, 0, array.Length);
	}
	private static void WriteVector2ToBin(Vector2 v, MemoryStream stream)
	{
		stream.Write(BitConverter.GetBytes(v.x), 0, 4);
		stream.Write(BitConverter.GetBytes(v.y), 0, 4);
	}
	private static void WriteVector3ToBin(Vector3 v, MemoryStream stream)
	{
		stream.Write(BitConverter.GetBytes(v.x), 0, 4);
		stream.Write(BitConverter.GetBytes(v.y), 0, 4);
		stream.Write(BitConverter.GetBytes(v.z), 0, 4);
	}
	private static void WriteVector4ToBin(Vector4 v, MemoryStream stream)
	{
		stream.Write(BitConverter.GetBytes(v.x), 0, 4);
		stream.Write(BitConverter.GetBytes(v.y), 0, 4);
		stream.Write(BitConverter.GetBytes(v.z), 0, 4);
		stream.Write(BitConverter.GetBytes(v.w), 0, 4);
	}
	private static void WriteQuaternionToBin(Quaternion q, MemoryStream stream)
	{
		stream.Write(BitConverter.GetBytes(q.x), 0, 4);
		stream.Write(BitConverter.GetBytes(q.y), 0, 4);
		stream.Write(BitConverter.GetBytes(q.z), 0, 4);
		stream.Write(BitConverter.GetBytes(q.w), 0, 4);
	}
	private static void WriteDataToBin(object value, MemoryStream stream)
	{
		stream.WriteByte(TypeUtils.TypeToByte(value.GetType()));
		switch (TypeUtils.GetDataType(value.GetType()))
		{
		case DataType.Int32:
			stream.Write(BitConverter.GetBytes((int)value), 0, 4);
			return;
		case DataType.Single:
			stream.Write(BitConverter.GetBytes((float)value), 0, 4);
			return;
		case DataType.String:
			MessageSaver.WriteStringToBin((string)value, stream);
			return;
		case DataType.Vector2:
			MessageSaver.WriteVector2ToBin((Vector2)value, stream);
			return;
		case DataType.Vector3:
			MessageSaver.WriteVector3ToBin((Vector3)value, stream);
			return;
		case DataType.Vector4:
			MessageSaver.WriteVector4ToBin((Vector4)value, stream);
			return;
		case DataType.Quaternion:
			MessageSaver.WriteQuaternionToBin((Quaternion)value, stream);
			return;
		default:
			return;
		}
	}
	public static List<Publisher> LoadBinary()
	{
		List<Publisher> list = new List<Publisher>();
		Stream stream = new FileStream(Application.persistentDataPath + "/Data.bin", FileMode.Open, FileAccess.Read, FileShare.None);
		Debug.Log("File size : " + stream.Length);
		while (stream.Position < stream.Length)
		{
			byte[] array = new byte[4];
			stream.Read(array, 0, 4);
			int num = BitConverter.ToInt32(array, 0);
			array = new byte[num];
			stream.Read(array, 0, num);
			MemoryStream memoryStream = new MemoryStream(array);
			float time = (float)MessageSaver.BinToSingle(memoryStream);
			float deltaTime = (float)MessageSaver.BinToSingle(memoryStream);
			string domain = MessageSaver.BinToString(memoryStream);
			string message = MessageSaver.BinToString(memoryStream);
			List<MessageData> list2 = new List<MessageData>();
			while (memoryStream.Position < memoryStream.Length)
			{
				object value = MessageSaver.ReadDataFromBin(memoryStream);
				list2.Add(new MessageData(value));
			}
			list.Add(new Publisher(domain, message, new object[]
			{
				""
			})
			{
				data = list2.ToArray(),
				time = time,
				deltaTime = deltaTime
			});
			memoryStream.Close();
		}
		stream.Close();
		return list;
	}
	private static object ReadDataFromBin(MemoryStream stream)
	{
		switch (TypeUtils.ByteToDataType((byte)stream.ReadByte()))
		{
		case DataType.Int32:
			return MessageSaver.BinToInt32(stream);
		case DataType.Single:
			return MessageSaver.BinToSingle(stream);
		case DataType.String:
			return MessageSaver.BinToString(stream);
		case DataType.Vector2:
			return MessageSaver.BinToVector2(stream);
		case DataType.Vector3:
			return MessageSaver.BinToVector3(stream);
		case DataType.Vector4:
			return MessageSaver.BinToVector4(stream);
		case DataType.Quaternion:
			return MessageSaver.BinToQuaternion(stream);
		default:
			return null;
		}
	}
	private static object BinToInt32(MemoryStream stream)
	{
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		return BitConverter.ToInt32(array, 0);
	}
	private static object BinToSingle(MemoryStream stream)
	{
		byte[] array = new byte[4];
		stream.Read(array, 0, 4);
		return BitConverter.ToSingle(array, 0);
	}
	private static string BinToString(MemoryStream stream)
	{
		int num = stream.ReadByte();
		byte[] array = new byte[num];
		stream.Read(array, 0, num);
		return new string(Encoding.ASCII.GetChars(array));
	}
	private static object BinToVector2(MemoryStream stream)
	{
		byte[] array = new byte[4];
		Vector2 vector = default(Vector2);
		stream.Read(array, 0, 4);
		vector.x = BitConverter.ToSingle(array, 0);
		stream.Read(array, 0, 4);
		vector.y = BitConverter.ToSingle(array, 0);
		return vector;
	}
	private static object BinToVector3(MemoryStream stream)
	{
		byte[] array = new byte[4];
		Vector3 vector = default(Vector3);
		stream.Read(array, 0, 4);
		vector.x = BitConverter.ToSingle(array, 0);
		stream.Read(array, 0, 4);
		vector.y = BitConverter.ToSingle(array, 0);
		stream.Read(array, 0, 4);
		vector.z = BitConverter.ToSingle(array, 0);
		return vector;
	}
	private static object BinToVector4(MemoryStream stream)
	{
		byte[] array = new byte[4];
		Vector4 vector = default(Vector4);
		stream.Read(array, 0, 4);
		vector.x = BitConverter.ToSingle(array, 0);
		stream.Read(array, 0, 4);
		vector.y = BitConverter.ToSingle(array, 0);
		stream.Read(array, 0, 4);
		vector.z = BitConverter.ToSingle(array, 0);
		stream.Read(array, 0, 4);
		vector.w = BitConverter.ToSingle(array, 0);
		return vector;
	}
	private static object BinToQuaternion(MemoryStream stream)
	{
		byte[] array = new byte[4];
		Quaternion quaternion = default(Quaternion);
		stream.Read(array, 0, 4);
		quaternion.x = BitConverter.ToSingle(array, 0);
		stream.Read(array, 0, 4);
		quaternion.y = BitConverter.ToSingle(array, 0);
		stream.Read(array, 0, 4);
		quaternion.z = BitConverter.ToSingle(array, 0);
		stream.Read(array, 0, 4);
		quaternion.w = BitConverter.ToSingle(array, 0);
		return quaternion;
	}
	private void _WriteXMLFile(Publisher publisher)
	{
		XmlElement root = this.xmlDoc.CreateElement("Publisher");
		float num = Time.realtimeSinceStartup - BlackBoard.Read<float>("time", "DispatcherStartedTime");
		MessageSaver.WriteXMLElement("Time", num, this.xmlDoc, root);
		MessageSaver.WriteXMLElement("Domain", publisher.domain, this.xmlDoc, root);
		MessageSaver.WriteXMLElement("Message", publisher.message, this.xmlDoc, root);
		for (int i = 0; i < publisher.data.Length; i++)
		{
			string elementName = publisher.data[i].GetType().ToString();
			MessageSaver.WriteXMLElement(elementName, publisher.data[i].Read<object>(), this.xmlDoc, root);
		}
		this.xmlDoc.Save("data.xml");
	}
	public static void WriteXMLFile(Publisher publisher)
	{
		MessageSaver.instance._WriteXMLFile(publisher);
	}
	public static List<Publisher> ReadXMLFile()
	{
		if (!File.Exists("data.xml"))
		{
			return null;
		}
		List<Publisher> list = new List<Publisher>();
		XmlTextReader xmlTextReader = new XmlTextReader("data.xml");
		while (xmlTextReader.Read())
		{
			XmlNodeType nodeType = xmlTextReader.NodeType;
			if (nodeType == XmlNodeType.Element && xmlTextReader.Name == "Publisher")
			{
				list.Add(MessageSaver.ReadXMLPublisher(xmlTextReader.ReadSubtree()));
			}
		}
		return list;
	}
	private static void WriteXMLElement(string elementName, object value, XmlDocument xmlDoc, XmlElement root)
	{
		XmlElement xmlElement = xmlDoc.CreateElement(elementName);
		XmlText newChild = xmlDoc.CreateTextNode(value.ToString());
		xmlElement.AppendChild(newChild);
		root.AppendChild(xmlElement);
		xmlDoc.DocumentElement.AppendChild(root);
	}
	private static Publisher ReadXMLPublisher(XmlReader reader)
	{
		float time = 0f;
		string domain = "";
		string message = "";
		List<MessageData> list = new List<MessageData>();
		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.Element)
			{
				string name;
				if ((name = reader.Name) != null)
				{
					if (name == "Time")
					{
						reader.Read();
						time = float.Parse(reader.Value);
						continue;
					}
					if (name == "Domain")
					{
						reader.Read();
						domain = reader.Value;
						continue;
					}
					if (name == "Message")
					{
						reader.Read();
						message = reader.Value;
						continue;
					}
					if (name == "Publisher")
					{
						continue;
					}
				}
				string name2 = reader.Name;
				reader.Read();
				list.Add(new MessageData(reader.Value, Type.GetType(name2)));
			}
		}
		return new Publisher(domain, message, new object[]
		{
			""
		})
		{
			data = list.ToArray(),
			time = time
		};
	}
}
