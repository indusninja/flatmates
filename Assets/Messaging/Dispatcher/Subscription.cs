using System;
[Serializable]
public class Subscription
{
	public Callback callback;
	public SubscriptionCallback subscriptionCallback;
	public string domain;
	public string message;
	public string sender;
	public MessageData[] data;
	public bool autoUnSubscribe;
	public bool deliveryEnabled;
	public Subscription(string domain, string message, Callback callback) : this(domain, message, callback, null, true)
	{
	}
	public Subscription(string domain, string message, Callback callback, bool autoUnSubscribe) : this(domain, message, callback, null, autoUnSubscribe)
	{
	}
	public Subscription(string domain, string message, SubscriptionCallback subscriptionCallback) : this(domain, message, null, subscriptionCallback, true)
	{
	}
	public Subscription(string domain, string message, SubscriptionCallback subscriptionCallback, bool autoUnSubscribe) : this(domain, message, null, subscriptionCallback, autoUnSubscribe)
	{
	}
	public Subscription(string domain, string message, Callback callback, SubscriptionCallback subscriptionCallback, bool autoUnSubscribe)
	{
		this.domain = domain;
		this.message = message;
		this.callback = callback;
		this.subscriptionCallback = subscriptionCallback;
		this.autoUnSubscribe = autoUnSubscribe;
		this.deliveryEnabled = true;
	}
	public void UnSubscribe()
	{
		Dispatcher.UnSubscribe(this);
	}
	public T Read<T>(int index)
	{
		return this.data[index].Read<T>();
	}
}
