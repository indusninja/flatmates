using UnityEngine;
using System.Collections;

public class PlayerRoom : MonoBehaviour 
{
	public int owner;

	void OnTriggerEnter2D(Collider2D hit)
	{
		if (hit.gameObject.GetComponent<PlayerController>() == null)
			return;

		int playerID = hit.gameObject.GetComponent<PlayerController>().playerID;

		if (playerID != owner)
			return;

		if (hit.gameObject.layer == LayerMask.NameToLayer("Player"))
			Dispatcher.SendMessage("Player" + playerID, "DidEnterRoom");
	}
}
