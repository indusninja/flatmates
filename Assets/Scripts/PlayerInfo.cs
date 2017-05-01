using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ViewerType
{
	None,
	Game,
	Player,
	Spectator
}



public class PlayerInfo
{
	public enum PlayerState
	{
		Connected,
		PlayerReady,
		WorldReady,
		Playing
	}

	private bool m_IsMaster = false;
	private PickupItem m_currentCarryItem = null;

	public int ID { get; set; }
	public string Name { get; set; }
	public Vector3 Position { get; set; }
	public Color Color { get; set; }
	public int Score { get; set; }
	public PlayerState currentState { get; set; }
	public bool isMaster {
		get { return m_IsMaster; }
	}
	public int AnimationIndex { get; set; }

	public GameObject gameObject { get; set; }

	public List<PickupObject> ObjectsOwned { get; set; }
	public List<PickupObject> ObjectsPicked { get; set; }

	public PlayerInfo(int id, string name, Vector3 position, Color color, int score, int animationIndex, bool master)
	{
		ID = id;
		Name = name;
		Position = position;
		Color = color;
		Score = score;
		m_IsMaster = master;
		AnimationIndex = animationIndex;
		currentState = PlayerState.Connected;

		ObjectsOwned = new List<PickupObject>();
		ObjectsPicked = new List<PickupObject>();
	}

	public void PickUpItem (PickupItem pickupItem)
	{
		m_currentCarryItem = pickupItem;
	}

	public bool IsCarryingItem ()
	{
		return m_currentCarryItem != null;
		;
	}

	public void Drop (PickupItem dropItem)
	{
		if (m_currentCarryItem != dropItem)
		{
			Debug.LogError ("Ican't drop an Item i don't have :(");
		}
		m_currentCarryItem = null;
	}
}
