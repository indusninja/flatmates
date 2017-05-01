using System;
[Serializable]
public struct MessageData
{
	public Type type;
	public object var;
	public MessageData(object value)
	{
		this.type = value.GetType();
		this.var = value;
	}
	public MessageData(string value, Type type)
	{
		this.type = type;
		this.var = "";
		this.Write(value, type);
	}
	public MessageData(MessageData messageData)
	{
		this.type = messageData.var.GetType();
		this.var = messageData.var;
	}
	public T Read<T>()
	{
		return (T)((object)this.var);
	}
	public void Write(object value)
	{
		this.type = value.GetType();
		this.var = value;
	}
	public void Write(string value, Type type)
	{
		this.type = type;
		this.var = value;
		string name;
		if ((name = type.Name) != null)
		{
			if (name == "Int32")
			{
				this.var = int.Parse(value);
				return;
			}
			if (name == "Single")
			{
				this.var = float.Parse(value);
				return;
			}
			if (name == "Vector2")
			{
				this.var = Parse.Vector2(value);
				return;
			}
			if (name == "Vector3")
			{
				this.var = Parse.Vector3(value);
				return;
			}
			if (name == "Vector4")
			{
				this.var = Parse.Vector4(value);
				return;
			}
			if (name == "Quaternion")
			{
				this.var = Parse.Quaternion(value);
				return;
			}
		}
	}
	public new Type GetType()
	{
		return this.type;
	}
	public override string ToString()
	{
		return this.var.ToString();
	}
}
