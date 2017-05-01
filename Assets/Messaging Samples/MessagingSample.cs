using UnityEngine;

public class MessagingSample : GameScript {

	void Start () {

        // Initialize bunch of shit
        base.Start();
        

        // Register methods to a message
        Subscribe("Domain", "Message", Foo);
        Subscribe("Domain", "Message", Bar);

        // Send message. It will take one frame of time to deliver
        SendMessage ("Domain", "Message");

        // We can also send any data we want
        SendMessage ("Domain2", "Message", gameObject, "DerpaDerpa", 100, this);
	}

    // Callback example 1
    void Foo ()
    {}

    // Callback example 2
    void Bar(Subscription subscription)
    {
        // Read sent data from object 1 (0 is the gameObject, 1 is "DerpaDerpa", 2 is '100' ...
        string s = subscription.Read<string> (1);

        // We don't want these messages anymore :<
        subscription.UnSubscribe();
    }

    // Register method with attribute
    [RegisterMessage ("Domain", "Message")]
    void Herp ()
    {}

    // This is called when 3 messages are received
    [RegisterMessage ("Domain", "Message", 3)]
    void Derp ()
    {}
}
