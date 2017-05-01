using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class GameScript : MonoBehaviour {

    #region Subscriptions

    private List<Subscription> subscriptions;

    // Unsubscribe
    public void Unsubscribe( Subscription sub ) {
        Dispatcher.UnSubscribe( sub );

        if( subscriptions.Contains( sub ) )
            subscriptions.Remove( sub );
    }

    // Subscribe
    public Subscription Subscribe( string domain, string message, Callback callback ) {

        return this.Subscribe(new Subscription(domain, message, callback) );
    }

    public Subscription Subscribe( string domain, string message, SubscriptionCallback callback ) {

        return this.Subscribe(new Subscription(domain, message, callback) );
    }

    public Subscription SubscribePrivate(string message, Callback callback) {

        return this.Subscribe(new Subscription(name + GetInstanceID(), message, callback));
    }

    public Subscription SubscribePrivate(string message, SubscriptionCallback callback) {

        return this.Subscribe(new Subscription(name + GetInstanceID(), message, callback));
    }

    public Subscription Subscribe(Subscription subscription) {
        Dispatcher.Subscribe( subscription );

        subscriptions.Add(subscription);
        subscription.deliveryEnabled = enabled;

        return subscription;
    }

    #endregion

	#region Timer
	
	public Timer AddTimer(float delay, Callback callback) {
		Timer timer = Timer.Add(delay, callback, false);
		timers.Add(timer);
		
		return timer;
	}

    public Timer AddTimer(float delay, Callback callback, bool isRepeatable) {

        Timer timer = Timer.Add(delay, callback, isRepeatable);
        timers.Add(timer);
        return timer;
    }

    private List<Timer> timers = new List<Timer>();

    #endregion

    #region Message Registering

    List<RegisterMessage> attributeCache;

    void FindAttributes()
    {
        List<string> messages = new List<string>();

        BindingFlags flags = BindingFlags.Public
                            | BindingFlags.Instance
                            | BindingFlags.DeclaredOnly
                            | BindingFlags.NonPublic;                            

        // Get fields
        foreach (FieldInfo field in GetType().GetFields(flags))
        {
            foreach (RegisterMessage attribute in field.GetCustomAttributes(typeof(RegisterMessage), false))
            {
                if (attributeCache == null)
                    attributeCache = new List<RegisterMessage>();

                attribute.field = field;
                attributeCache.Add(attribute);

                string message = attribute.domain + "@" + attribute.message;

                if (!messages.Contains(message))
                    messages.Add(message);
            }
        }

        // Get methods
        foreach (MethodInfo method in GetType().GetMethods(flags))
        {
            foreach (RegisterMessage attribute in method.GetCustomAttributes(typeof(RegisterMessage), false))
            {
                if (attributeCache == null)
                    attributeCache = new List<RegisterMessage>();
                
                attribute.callback = method;
                attributeCache.Add(attribute);

                string message = attribute.domain + "@" + attribute.message;

                if (!messages.Contains(message))
                    messages.Add(message);
            }
        }

        foreach (string message in messages)
        {
            string[] s = message.Split('@');
            Subscribe(s[0], s[1], AttributeCallback);
        }
    }

    void AttributeCallback(Subscription subscription)
    {
        foreach (RegisterMessage attribute in attributeCache)
            if (attribute.domain == subscription.domain && attribute.message == subscription.message)
            {
                attribute.count = attribute.count > 0 ? attribute.count - 1 :
                                  attribute.originalCount;

                if (attribute.count == 0)
                {
                    if (attribute.field != null)
                        attribute.field.SetValue(this, true);
                    if (attribute.callback != null)
                        attribute.callback.Invoke(this, null);
                }
            }
    }


    #endregion

    public void SendMessage(Publisher publisher) {
        Dispatcher.SendMessage(publisher);
    }

    public Publisher SendMessage(string domain, string message, params object[] args) {
        return Dispatcher.SendMessage(domain, message, args);
    }

    public Publisher SendMessageInstant(string domain, string message, params object[] args) {
        return Dispatcher.SendMessageInstant(domain, message, args);
    }

    public Publisher SendMessagePrivate(string message, params object[] args) {
        return Dispatcher.SendMessage(name+GetInstanceID(), message, args);
    }

    public Publisher SendMessagePrivateInstant(string message, params object[] args) {
        return Dispatcher.SendMessageInstant(name + GetInstanceID(), message, args);
    }

    public void SetSubscriptionsActive(bool enabled) {

        if (subscriptions == null)
            return;

        for (int i = 0; i < subscriptions.Count; i++) {
            subscriptions[i].deliveryEnabled = enabled;
        }
    }

    public virtual void OnEnable() {

        SetSubscriptionsActive(true);

        SendMessage(name, "Enabled");
    }

    public virtual void OnDisable() {

        SetSubscriptionsActive(false);

        SendMessage(name, "Disabled");
    }

    public void OnDestroy() {

        // Purge blackboard
        BlackBoard.Clear(name);

        // unsub everything in the sub list
        if( subscriptions != null && subscriptions.Count > 0 ) {

            foreach( Subscription sub in subscriptions ) {
                if( sub != null )
                    Dispatcher.UnSubscribe( sub );
            }

        }

        if (timers != null && timers.Count > 0)
            foreach (Timer timer in timers)
                timer.Remove();
    }

    void OnApplicationQuit() {

        if (timers != null && timers.Count > 0)
            foreach (Timer timer in timers)
                timer.Remove();
    }

    public void Start() {
        subscriptions = new List<Subscription>();

        FindAttributes();
    }
}
