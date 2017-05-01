using System;
using System.Collections.Generic;
public class TimeLayer
{
	private Dictionary<int, TimeLayer> layers = new Dictionary<int, TimeLayer>();
	public float deltaTime = 0.016f;
	public float time;
	public float timeScale = 1f;
	public TimeLayer this[int index]
	{
		get
		{
			if (index == 0)
			{
				return this;
			}
			if (this.layers.ContainsKey(index))
			{
				return this.layers[index];
			}
			TimeLayer timeLayer = new TimeLayer();
			this.layers.Add(index, timeLayer);
			return timeLayer;
		}
	}
	public void Update(float deltaTime)
	{
		deltaTime *= this.timeScale;
		this.time += deltaTime;
		foreach (TimeLayer current in this.layers.Values)
		{
			current.deltaTime = deltaTime * current.timeScale;
			current.time += current.deltaTime;
		}
	}
}
