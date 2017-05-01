using System;
using UnityEngine;
public class Timer
{
	public float startTime;
	public float delay;
	public Callback callback;
	public CoRoutineCallback coRoutine;
	public Publisher publisher;
	public bool repeatable;
	public bool autodestruct;
	public bool isStarted;
	public bool isPlaying;
	public bool isPaused;
	public bool isStopped;
	public bool isFinished;
	private float pausingTime;
	public int timeLayer;
	public static TimeLayer TimeLayer
	{
		get
		{
			return TimerDaemon.TimeLayer;
		}
	}
	public static float deltaTime
	{
		get
		{
			return Timer.TimeLayer.deltaTime;
		}
	}
	public float span
	{
		get
		{
			return Mathf.Clamp(Time.realtimeSinceStartup / (this.startTime + this.delay), 0f, 1f);
		}
	}
	public Timer(float delay, Callback callback) : this(delay, callback, null, null, false, true)
	{
	}
	public Timer(float delay, Callback callback, bool repeatable) : this(delay, callback, null, null, repeatable, !repeatable)
	{
	}
	public Timer(float delay, Callback callback, bool repeatable, bool autodestruct) : this(delay, callback, null, null, repeatable, autodestruct)
	{
	}
	public Timer(float delay, Publisher publisher) : this(delay, null, null, publisher, false, true)
	{
	}
	public Timer(float delay, Publisher publisher, bool repeatable) : this(delay, null, null, publisher, repeatable, !repeatable)
	{
	}
	public Timer(float delay, Publisher publisher, bool repeatable, bool autodestruct) : this(delay, null, null, publisher, repeatable, autodestruct)
	{
	}
	public Timer(float delay, Callback callback, CoRoutineCallback coRoutine, Publisher publisher, bool repeatable, bool autodestruct)
	{
		this.delay = delay;
		this.callback = callback;
		this.coRoutine = coRoutine;
		this.publisher = publisher;
		this.repeatable = repeatable;
		this.autodestruct = autodestruct;
		this.isStarted = true;
		this.startTime = Time.realtimeSinceStartup;
		this.isPlaying = true;
		this.isPaused = false;
		this.isStopped = false;
		this.isFinished = true;
	}
	public static Timer Add(float delay, Callback callback)
	{
		return Timer.Add(delay, callback, null, null, false, true);
	}
	public static Timer Add(float delay, Callback callback, bool repeatable)
	{
		return Timer.Add(delay, callback, null, null, repeatable, !repeatable);
	}
	public static Timer Add(float delay, Callback callback, bool repeatable, bool autodestruct)
	{
		return Timer.Add(delay, callback, null, null, repeatable, autodestruct);
	}
	public static Timer CoAdd(float delay, CoRoutineCallback coRoutine)
	{
		return Timer.Add(delay, null, coRoutine, null, false, true);
	}
	public static Timer CoAdd(float delay, CoRoutineCallback coRoutine, bool repeatable)
	{
		return Timer.Add(delay, null, coRoutine, null, repeatable, !repeatable);
	}
	public static Timer CoAdd(float delay, CoRoutineCallback coRoutine, bool repeatable, bool autodestruct)
	{
		return Timer.Add(delay, null, coRoutine, null, repeatable, autodestruct);
	}
	public static Timer Add(float delay, Publisher publisher)
	{
		return Timer.Add(delay, null, null, publisher, false, true);
	}
	public static Timer Add(float delay, Publisher publisher, bool repeatable)
	{
		return Timer.Add(delay, null, null, publisher, repeatable, !repeatable);
	}
	public static Timer Add(float delay, Publisher publisher, bool repeatable, bool autodestruct)
	{
		return Timer.Add(delay, null, null, publisher, repeatable, autodestruct);
	}
	private static Timer Add(float delay, Callback callback, CoRoutineCallback coRoutine, Publisher publisher, bool repeatable, bool autodestruct)
	{
		Timer timer = new Timer(delay, callback, coRoutine, publisher, repeatable, autodestruct);
		TimerDaemon.timerDaemon.timers.Add(timer);
		return timer;
	}
	public static void Add(Timer timer)
	{
		TimerDaemon.timerDaemon.timers.Add(timer);
	}
	public void Play()
	{
		if (this.isPaused)
		{
			this.startTime += this.pausingTime - this.startTime;
		}
		if (this.isStopped)
		{
			this.startTime = Time.realtimeSinceStartup;
		}
		this.isPlaying = true;
		this.isPaused = false;
		this.isFinished = false;
		this.isStopped = false;
	}
	public void Pause()
	{
		this.isPlaying = false;
		this.isPaused = true;
		this.isFinished = false;
		this.isStopped = false;
		this.pausingTime = Time.realtimeSinceStartup;
	}
	public void Stop()
	{
		this.isPlaying = false;
		this.isPaused = false;
		this.isFinished = false;
		this.isStopped = true;
	}
	public void Remove()
	{
		TimerDaemon.timerDaemon.timers.Remove(this);
		this.callback = null;
		this.coRoutine = null;
		this.publisher = null;
	}
	public void Reset()
	{
		this.startTime = Time.realtimeSinceStartup;
	}
	public void Delay(float time)
	{
		this.delay += time;
	}
	public void Update(float currentTime)
	{
		if (!this.isPlaying || this.isPaused || this.isStopped)
		{
			return;
		}
		if (currentTime > this.startTime + this.delay / TimerDaemon.TimeLayer[this.timeLayer].timeScale)
		{
			if (this.callback != null)
			{
				this.callback();
				if (this.autodestruct)
				{
					this.callback = null;
				}
			}
			if (this.coRoutine != null)
			{
				TimerDaemon.timerDaemon.StartCoroutine(this.coRoutine());
				if (this.autodestruct)
				{
					this.coRoutine = null;
				}
			}
			if (this.publisher != null)
			{
				this.publisher.SendMessage();
				if (this.autodestruct)
				{
					this.publisher = null;
				}
			}
			if (this.autodestruct)
			{
				this.Remove();
			}
			if (!this.repeatable)
			{
				this.isStarted = false;
				this.isPlaying = false;
				this.isFinished = true;
			}
			if (this.repeatable)
			{
				this.Reset();
			}
		}
	}
}
