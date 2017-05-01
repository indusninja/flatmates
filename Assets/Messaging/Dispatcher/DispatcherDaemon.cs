using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class DispatcherDaemon : MonoBehaviour
{
    private static DispatcherDaemon _instance = DispatcherDaemon.instance;
    public PlayBack playBack;
    private List<Publisher> playBackPublishers;

    private Thread dispatcherThread;

    private static DispatcherDaemon instance
    {
        get
        {
            if (DispatcherDaemon._instance == null)
            {
                DispatcherDaemon._instance = new GameObject(typeof(DispatcherDaemon).ToString()).AddComponent<DispatcherDaemon>();
                UnityEngine.Object.DontDestroyOnLoad(DispatcherDaemon._instance.gameObject);
            }
            return DispatcherDaemon._instance;
        }
    }

    public static Transform Initialize()
    {
        return DispatcherDaemon.instance.transform;
    }

    private void OnApplicationQuit()
    {
        MessageSaver.DeInitialize();
        DispatcherDaemon._instance = null;
    }

    private void Awake()
    {
        UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
    }

    private void Start()
    {
    }

    private bool isThreading = false;

    private void Update()
    {
        BlackBoard.GameStarted();
        
        if (this.playBack != PlayBack.None)
        {
            this.DispatchFrom(this.playBackPublishers);
            Dispatcher.deltaTime = Time.deltaTime;
        }

        if(!isThreading)
            if(Dispatcher.instance.threadedPublishers != null && Dispatcher.instance.threadedPublishers.Count > 0)
            {
                isThreading = true;
                dispatcherThread = new Thread(new ThreadStart(Dispatcher.DispatchThreaded));
                dispatcherThread.Start();
                while (!dispatcherThread.IsAlive) ;
                Thread.Sleep(1);
            }

        if (this.playBack == PlayBack.None)
        {
            Dispatcher.deltaTime = Time.deltaTime;
            BlackBoard.Commit();

            Dispatcher.DispatchThreaded();

            foreach (Publisher publisher in Dispatcher.Dispatch())
            {
            }
        }
    }

    private void DispatchFrom(List<Publisher> publishers)
    {
        float num = Time.realtimeSinceStartup - Dispatcher.startTime;
        
        for (int i = 0; i < publishers.Count; i++)
        {
            if (num < publishers[i].time)
            {
                return;
            }
        
            Dispatcher.deltaTime = publishers[i].deltaTime;
            Dispatcher.Dispatch(publishers[i]);
            publishers.Remove(publishers[i]);
        }
    }
}
