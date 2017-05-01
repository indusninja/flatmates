using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MessageDocumentation : Attribute {

    public string condition;
    public string rationale;
    public string domain;
    public string message;
    public string[] args;

    static string separator = ",";

    public MessageDocumentation(string condition, string rationale, string domain, string message, params string[] args) {

        this.condition = condition;
        this.rationale = rationale;
        this.domain = domain;
        this.message = message;
        this.args = args;
    }

    new public string ToString() {

        string s = condition + separator +
                   rationale + separator +
                   domain + separator +
                   message;

        foreach (string arg in args)
            s += separator + arg;

        return s;
    }

    public static int Compare(MessageDocumentation x, MessageDocumentation y) {

        if (x == null || y == null)
            return 0;

        return string.Compare(x.message, y.message);
    }
}


[AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
public class SubscriptionDocumentation : Attribute {

    public string condition;
    public string rationale;
    public string domain;
    public string message;
    public string[] args;

    static string separator = ",";

    public SubscriptionDocumentation(string condition, string rationale, string domain, string message, params string[] args) {

        this.condition = condition;
        this.rationale = rationale;
        this.domain = domain;
        this.message = message;
        this.args = args;
    }

    new public string ToString() {

        string s = condition + separator +
                   rationale + separator +
                   domain + separator +
                   message;

        foreach (string arg in args)
            s += separator + arg;

        return s;
    }

    public static int Compare(SubscriptionDocumentation x, SubscriptionDocumentation y) {

        if (x == null || y == null)
            return 0;

        return string.Compare(x.message, y.message);
    }
}