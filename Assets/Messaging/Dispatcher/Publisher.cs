using System;
[Serializable]
public class Publisher
{
	public string domain;
	public string message;
	public MessageData[] data;
	public float time;
	public float deltaTime;
	public Publisher(string domain, string message, params object[] args)
	{
		this.domain = domain;
		this.message = message;
		if (args.Length > 0)
		{
			this.data = new MessageData[args.Length];
			for (int i = 0; i < args.Length; i++)
			{
				this.data[i] = new MessageData(args[i]);
			}
		}
	}
	public void SendMessage()
	{
		Dispatcher.SendMessage(this);
	}
}
