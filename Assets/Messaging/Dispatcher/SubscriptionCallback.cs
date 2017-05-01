using System;
using System.Reflection;

public delegate void SubscriptionCallback(Subscription subscription);

public class RegisterMessage : Attribute
{
    public string domain;
    public string message;

    public int count;
    public int originalCount;

    public FieldInfo field;
    public MethodInfo callback;

    public RegisterMessage(string domain, string message)
    {
        this.domain = domain;
        this.message = message;
        this.count = 0;
    }

    public RegisterMessage (string domain, string message, int count)
    {
        this.domain = domain;
        this.message = message;
        this.count = count;
        this.originalCount = count;
    }
}