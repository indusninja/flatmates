using System;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class TimerDaemon : MonoBehaviour
{
	public static TimerDaemon _timerDaemon = new GameObject("TimerDaemon").AddComponent<TimerDaemon>();
	public static TimeLayer TimeLayer = new TimeLayer();
	public List<Timer> timers = new List<Timer>();
	private float time;
	public static TimerDaemon timerDaemon
	{
		get
		{
			if (TimerDaemon._timerDaemon != null)
			{
				return TimerDaemon._timerDaemon;
			}
			TimerDaemon._timerDaemon = new GameObject("TimerDaemon").AddComponent<TimerDaemon>();
			return TimerDaemon._timerDaemon;
		}
	}
	private void OnApplicationQuit()
	{
		if (base.gameObject)
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
		TimerDaemon._timerDaemon = null;
	}
	private void Update()
	{
		if (Application.isEditor && !Application.isPlaying)
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
		this.time = Time.realtimeSinceStartup;
		TimerDaemon.TimeLayer.Update(Time.deltaTime);
		for (int i = 0; i < this.timers.Count; i++)
		{
			if (this.timers[i] != null)
			{
				this.timers[i].Update(this.time);
			}
		}
	}
}
