using UnityEngine;
using System.Collections;

public class TimerSample : MonoBehaviour {

    // Pretty much the same thing as Invoke, but with some extra control

    Timer t;
	void Start () {

        Timer.Add (1.5f, CallMe);
        Timer.Add (1, OnceInSecond, true);
        t = Timer.Add(0, EveryFrame, true);
    }

    void CallMe()
    { }

    void OnceInSecond()
    {
    }

    void EveryFrame()
    {
        if (Time.realtimeSinceStartup > 10)
            t.Pause();
    }
}
