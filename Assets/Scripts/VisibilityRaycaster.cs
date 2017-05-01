using UnityEngine;
using System.Collections;

public class VisibilityRaycaster : MonoBehaviour
{

	public LayerMask occluderLayer;
	public float maxDistance = 5f;

	private PlayerInfo m_Myself;

	void Update () {
		if (m_Myself == null)
		{
			m_Myself = GameStateManager.Instance.GetPlayerByGameObject(gameObject);
		}

		foreach (PlayerInfo player in GameStateManager.Instance.GetPlayersDict ().Values)
		{
			if (gameObject == player.gameObject)
				continue;

			if (player == null || player.gameObject == null)
				continue;

			Vector3 position = player.gameObject.transform.position;

			if (IsPlayerVisible(position))
			{
				player.gameObject.transform.Find("Sprite").GetComponent<SpriteRenderer>().enabled = true;
				//player.gameObject.transform.Find("2DLight").GetComponent<Light2D>().enabled = true;
				Dispatcher.SendMessage(player.gameObject.name, "PlayerIsVisible", m_Myself.ID);
				Dispatcher.SendMessage(name, "DidSawPlayer", player.gameObject.name);
			}
			else if ((m_Myself is ClientPlayerInfo) && !(player is ClientPlayerInfo))
			{
				player.gameObject.transform.Find("Sprite").GetComponent<SpriteRenderer>().enabled = false;
				player.gameObject.transform.Find("2DLight").GetComponent<Light2D>().enabled = false;
			}
		}
	}

	private bool IsPlayerVisible(Vector3 position)
	{
		Vector3 direction = position - transform.position;
		float distance = Vector3.Distance(transform.position, position);
		if (distance > maxDistance)
			return false;
		

		RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, occluderLayer);
	
		return !hit;
	}
}
