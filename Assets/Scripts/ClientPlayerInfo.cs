using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientPlayerInfo : PlayerInfo
{
	public int controllerID = -1;
	public Dictionary<int, PlayerInfo> OpponentPlayers;
	public Dictionary<int, Dictionary<int, float>> VisibilityMatrix;

	public ClientPlayerInfo(int id, string name, Vector3 position, Color color, int score, int animationIndex, bool master)
		: base(id, name, position, color, score, animationIndex, master)
	{
		OpponentPlayers = new Dictionary<int, PlayerInfo>();
		VisibilityMatrix = new Dictionary<int, Dictionary<int, float>>();
		VisibilityMatrix.Add(ID, new Dictionary<int, float>());
	}

	public void AddOpponent(PlayerInfo player)
	{
		if (!OpponentPlayers.ContainsKey(player.ID))
		{
			OpponentPlayers.Add(player.ID, player);
		}
		else
		{
			Debug.LogError("WTF! why am I receiving info about adding player " + player.ID);
		}

		// TODO: probably need to operate on this distance value to figure out the visibility
	   /* float distanceValue = GetDistance(Position, player.Position);
		if (!VisibilityMatrix.ContainsKey(ID))
		{
			VisibilityMatrix.Add(ID, new Dictionary<PlayerID, float>());
		}
		if (!VisibilityMatrix[ID].ContainsKey(player.ID))
		{
			VisibilityMatrix[ID].Add(player.ID, 0);
		}
		if (!VisibilityMatrix[player.ID].ContainsKey(ID))
		{
			VisibilityMatrix[player.ID].Add(ID, 0);
		}
		VisibilityMatrix[ID][player.ID] = distanceValue;
		VisibilityMatrix[player.ID][ID] = distanceValue;*/
	}

	public void RemoveOpponent(int id)
	{
		if (OpponentPlayers.ContainsKey(id))
		{
			// player found - remove
			OpponentPlayers.Remove(id);
		}

		if (VisibilityMatrix[ID].ContainsKey(id))
		{
			VisibilityMatrix[ID].Remove(id);
		}
		if (VisibilityMatrix[id].ContainsKey(ID))
		{
			VisibilityMatrix[id].Remove(ID);
		}
	}

	private float GetDistance(Vector3 vec1, Vector3 vec2)
	{
		return Mathf.Sqrt((vec1.x - vec2.x) * (vec1.x - vec2.x) +
							(vec1.y - vec2.y) * (vec1.y - vec2.y) +
							(vec1.z - vec2.z) * (vec1.z - vec2.z));
	}
}
