using System;
using System.Collections;
using System.Collections.Generic;

public class Dispatcher
{
    #region Classes

    private class Domain
    {
        #region Fields
        public string name;
		public Queue<Publisher>[] publishers = new Queue<Publisher>[2];
		public Hashtable subscribers = new Hashtable();
        #endregion

        #region Base
        
        public Domain(string name)
		{
			this.name = name;
			this.publishers[0] = new Queue<Publisher>();
			this.publishers[1] = new Queue<Publisher>();
		}

        #endregion

        #region Public

        public void AddSubscription(Subscription subscription)
		{
			if (this.subscribers.Contains(subscription.message))
			{
				((List<Subscription>)this.subscribers[subscription.message]).Add(subscription);
				return;
			}
			List<Subscription> list = new List<Subscription>();
			list.Add(subscription);
			this.subscribers.Add(subscription.message, list);
		}
		public void RemoveSubscription(Subscription subscription)
		{
			if (this.subscribers == null)
			{
				return;
			}
			if (this.subscribers.Contains(subscription.message))
			{
				((List<Subscription>)this.subscribers[subscription.message]).Remove(subscription);
			}
		}
        
        #endregion
	}

    #endregion

    #region Fields
    public static Dispatcher _instance = new Dispatcher();
	private float _startTime;
	private float _deltaTime;
    private bool skipPublisher;
	private Hashtable domains = new Hashtable();
	private int currentQueue;
	private Queue<Publisher> publishers;
	private Hashtable subscribers;
	private Hashtable blackList = new Hashtable();
	private Hashtable whiteList = new Hashtable();
	private string[] filters = new string[]
	{
		"System2"
	};
	private List<string> domainNames = new List<string>();
	private List<string> newMessages = new List<string>();

    private Subscription threadedSubscription;
    public List<Publisher> threadedPublishers = new List<Publisher>();
    #endregion

    #region Properties
    public static Dispatcher instance
	{
		get
		{
			if (Dispatcher._instance == null)
			{
				Dispatcher._instance = new Dispatcher();
			}
			return Dispatcher._instance;
		}
	}
	public static float startTime
	{
		get
		{
			return Dispatcher.instance._startTime;
		}
		set
		{
			Dispatcher.instance._startTime = value;
		}
	}
	public static float deltaTime
	{
		get
		{
			return Dispatcher.instance._deltaTime;
		}
		set
		{
			Dispatcher.instance._deltaTime = value;
		}
	}
    #endregion

    #region Base
    private Dispatcher()
	{
		DispatcherDaemon.Initialize();
	}
    #endregion

    #region Black / White listing
    public static void AddToBlackList(string domain)
	{
		Dispatcher.AddToFilters(Dispatcher.instance.blackList, domain);
	}
	public static void RemoveFromBlackList(string domain)
	{
		Dispatcher.RemoveFromFilters(Dispatcher.instance.blackList, domain);
	}
	public static void AddToWhiteList(string domain)
	{
		Dispatcher.AddToFilters(Dispatcher.instance.whiteList, domain);
	}
	public static void RemoveFromWhiteList(string domain)
	{
		Dispatcher.RemoveFromFilters(Dispatcher.instance.whiteList, domain);
	}
	private static void AddToFilters(Hashtable hashtable, string domain)
	{
		if (!hashtable.Contains(domain))
		{
			hashtable.Add(domain, true);
		}
	}
	private static void RemoveFromFilters(Hashtable hashtable, string domain)
	{
		if (hashtable.Contains(domain))
		{
			hashtable.Remove(domain);
		}
	}
	private static bool IsFiltered(string domain)
	{
		return Dispatcher.instance.blackList.Contains(domain) && !Dispatcher.instance.whiteList.Contains(domain);
	}
    #endregion

    #region SendMessage Methods
    public static Publisher SendMessage(string domain, string message, params object[] args)
	{
		Publisher publisher;
		if (args.Length > 0 && args != null)
		{
			publisher = new Publisher(domain, message, args);
		}
		else
		{
			publisher = new Publisher(domain, message, new object[0]);
		}
		Dispatcher.SendMessage(publisher);
		return publisher;
	}
	public static void SendMessage(Publisher publisher)
	{
		if (Dispatcher.instance.domains.Contains(publisher.domain))
		{
			((Dispatcher.Domain)Dispatcher.instance.domains[publisher.domain]).publishers[Dispatcher.instance.currentQueue].Enqueue(publisher);
		}
		else
		{
			Dispatcher.instance.domains.Add(publisher.domain, new Dispatcher.Domain(publisher.domain));
			Dispatcher.instance.domainNames.Add(publisher.domain);
		}
		Dispatcher.instance.newMessages.Add(publisher.domain);
	}
    public static void SendMessageThreaded(Publisher publisher)
    {
        Dispatcher.instance.threadedPublishers.Add(publisher);
    }

    public static Publisher SendMessageInstant(string domain, string message, params object[] args)
	{
		Publisher publisher;
		if (args.Length > 0 && args != null)
		{
			publisher = new Publisher(domain, message, args);
		}
		else
		{
			publisher = new Publisher(domain, message, new object[0]);
		}
		Dispatcher.SendMessageInstant(publisher);
		return publisher;
	}
	public static void SendMessageInstant(Publisher publisher)
	{
		Dispatcher.Dispatch(publisher);
	}
    #endregion

    #region Subscription Methods
    public static Subscription Subscribe(string domain, string message, Callback callback)
	{
		Subscription subscription = new Subscription(domain, message, callback);
		Dispatcher.Subscribe(subscription);
		return subscription;
	}
	
    public static Subscription Subscribe(string domain, string message, SubscriptionCallback subscriptionCallback)
	{
		Subscription subscription = new Subscription(domain, message, subscriptionCallback);
		Dispatcher.Subscribe(subscription);
		return subscription;
	}
	
    public static void Subscribe(Subscription subscription)
	{
		if (Dispatcher.instance.domains.Contains(subscription.domain))
		{
			Dispatcher.Domain domain = (Dispatcher.Domain)Dispatcher.instance.domains[subscription.domain];
			domain.AddSubscription(subscription);
			return;
		}
		Dispatcher.Domain domain2 = new Dispatcher.Domain(subscription.domain);
		Dispatcher.instance.domains.Add(subscription.domain, domain2);
		domain2.AddSubscription(subscription);
		Dispatcher.instance.domainNames.Add(subscription.domain);
	}

    public static void SubscribeThreaded(Subscription subscription)
    {
        Dispatcher.instance.threadedSubscription = subscription;
    }

    public static void UnSubscribe(Subscription subscription)
	{
		if (subscription == null)
		{
			return;
		}
		if (Dispatcher.instance.domains == null || Dispatcher.instance.domains.Count == 0)
		{
			return;
		}
		if (Dispatcher.instance.domains.Contains(subscription.domain))
		{
			Dispatcher.Domain domain = (Dispatcher.Domain)Dispatcher.instance.domains[subscription.domain];
			if (domain == null || domain.subscribers == null || domain.subscribers.Count == 0)
			{
				return;
			}
			domain.RemoveSubscription(subscription);
			if (!Dispatcher.instance.domains.Contains(subscription.domain))
			{
				Dispatcher.instance.domainNames.Remove(subscription.domain);
			}
		}
	}
	
    public static void AutoUnSubscribe()
	{
		string[] array = new string[Dispatcher.instance.domains.Count];
		Dispatcher.instance.domains.Keys.CopyTo(array, 0);
		for (int i = 0; i < array.Length; i++)
		{
			Dispatcher.Domain domain = (Dispatcher.Domain)Dispatcher.instance.domains[array[i]];
			string[] array2 = new string[domain.subscribers.Count];
			domain.subscribers.Keys.CopyTo(array2, 0);
			for (int j = 0; j < array2.Length; j++)
			{
				List<Subscription> list = (List<Subscription>)domain.subscribers[array2[j]];
				for (int k = 0; k < list.Count; k++)
				{
					Subscription subscription = list[k];
					if (subscription.autoUnSubscribe)
					{
						domain.RemoveSubscription(subscription);
					}
				}
			}
		}
	}

    public static void UnSubscribeAll()
	{
		string[] array = new string[Dispatcher.instance.domains.Count];
		Dispatcher.instance.domains.Keys.CopyTo(array, 0);
		for (int i = 0; i < array.Length; i++)
		{
			Dispatcher.Domain domain = (Dispatcher.Domain)Dispatcher.instance.domains[array[i]];
			domain.subscribers.Clear();
		}
		Dispatcher.instance.domainNames.Clear();
	}
    #endregion

    #region Dispatching
    public static IEnumerable Dispatch()
	{
		int num = Dispatcher.instance.currentQueue;
		Dispatcher.instance.currentQueue = ((num == 0) ? 1 : 0);
		string[] array = Dispatcher.instance.newMessages.ToArray();
		Dispatcher.instance.newMessages.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			Dispatcher.Domain domain = (Dispatcher.Domain)Dispatcher.instance.domains[array[i]];
			Dispatcher.instance.publishers = domain.publishers[num];
			if (Dispatcher.instance.publishers.Count != 0)
			{
				Dispatcher.instance.subscribers = domain.subscribers;
				if (Dispatcher.IsFiltered(domain.name))
				{
					Dispatcher.instance.publishers.Clear();
				}
				while (Dispatcher.instance.publishers.Count > 0)
				{
					Publisher publisher = Dispatcher.instance.publishers.Dequeue();
					yield return publisher;

				    if(!Dispatcher.instance.skipPublisher)
					    ProcessPublisher(publisher);

				    Dispatcher.instance.skipPublisher = false;
				}
			}
		}
		yield break;
	}

	private static void ProcessPublisher(Publisher publisher)
	{
		if (Dispatcher.instance.subscribers.Contains(publisher.message))
		{
			object obj = Dispatcher.instance.subscribers[publisher.message];
			List<Subscription> list = obj as List<Subscription>;

			InvokeMessage(publisher, list);
		}
	}

	private static void InvokeMessage(Publisher publisher, List<Subscription> list)
	{
		for (int j = 0; j < list.Count; j++)
		{
		    if (!list[j].deliveryEnabled)
		        continue;

            if (list[j].callback != null)
			{
                list[j].callback();
		    }
			else if (list[j].subscriptionCallback.Target != null)
			{
                list[j].data = publisher.data;
                list[j].subscriptionCallback(list[j]);
			}
	    }
	}

    // This is used when we are dispatching messages from external storage
	public static void Dispatch(Publisher publisher)
	{
		if (Dispatcher.instance.domains.Contains(publisher.domain))
		{
			Dispatcher.Domain domain = (Dispatcher.Domain)Dispatcher.instance.domains[publisher.domain];
			if (domain.subscribers.Contains(publisher.message))
			{
				object obj = domain.subscribers[publisher.message];
				List<Subscription> list = obj as List<Subscription>;
				
				InvokeMessage(publisher, list);
			}
		}
	}

    public static void DispatchThreaded()
    {
        List<Publisher> publishers = Dispatcher.instance.threadedPublishers;
        Subscription subscription = Dispatcher.instance.threadedSubscription;

        foreach (var publisher in publishers)
        {
            subscription.data = publisher.data;
            subscription.subscriptionCallback(subscription);
        }

        if (publishers != null && publishers.Count > 0)
        {
            UnityEngine.Debug.Log("Thread completed : " + publishers.Count + " messages processed.");
            publishers.Clear();
        }
    }
    #endregion
}
