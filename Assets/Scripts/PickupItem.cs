using UnityEngine;
using System.Collections;

public class PickupItem : PickupObject
{
	private Vector3 m_PlayerOffset;

	void Start()
	{
		m_PlayerOffset = new Vector3(.6f,1.8f,0);
		base.Start();
	}

	void OnTriggerEnter2D (Collider2D hit)
	{
		if (hit.gameObject.layer == LayerMask.NameToLayer("ItemCollector"))
            OnPickup(hit.transform.parent.gameObject);
	}

	void OnTriggerStay2D(Collider2D hit)
	{
		if (hit.gameObject.layer == LayerMask.NameToLayer("ItemCollector"))
			OnPickup(hit.transform.parent.gameObject);
	}

	private void OnPickup (GameObject playerObject)
	{
        int playerID = playerObject.GetComponent<PlayerController>().playerID;
        if (IsCollected || playerID == Owner)
			return;

		if(PickUp (playerID))
			SendMessage("PickupObject", "PlayerDidPickupItem", playerID, ID);
	}

	public bool PickUp (int playerID)
	{
		PlayerInfo player = GameStateManager.Instance.GetPlayerByID (playerID);
		if (player.IsCarryingItem ())
			return false;

		PlayerInfo itemOwner = GameStateManager.Instance.GetPlayerByID (Owner);
		VisibilityRaycaster visibilityRaycaster = itemOwner.gameObject.GetComponent<VisibilityRaycaster>();
		Vector3 direction = itemOwner.gameObject.transform.position - transform.position;
		float distance = Vector3.Distance(transform.position, itemOwner.gameObject.transform.position);

		if (distance < visibilityRaycaster.maxDistance)
		{
			RaycastHit2D ownerItemVisiblityHit = Physics2D.Raycast(transform.position, direction, distance, visibilityRaycaster.occluderLayer);
			if (!ownerItemVisiblityHit)
			{
				return false;
			}
		}

		player.PickUpItem (this);
		Holder = playerID;
		IsCollected = true;

		Subscribe("Player" + Holder, "PlayerIsVisible", Drop);
		Subscribe("Player" + Holder, "DidEnterRoom", DidEnterRoom);
		return true;
	}

	private void Drop (Subscription subscription)
	{
		int opponentID = subscription.Read<int>(0);

        if (opponentID != Owner)
			return;

		subscription.UnSubscribe();

		PlayerInfo player = GameStateManager.Instance.GetPlayerByID (Holder);
		transform.position = transform.position - m_PlayerOffset;
		player.Drop (this);

		IsCollected = false;
		Holder = 0;		
	}

	void DidEnterRoom(Subscription subscription)
	{
		subscription.UnSubscribe();

		if (!IsCollected)
			return;

		IsCollected = false;

		gameObject.SetActive (false);

		IsStolen = true;
		Debug.Log(Holder + " entered own room with " + name);

		if (GameStateManager.Instance.GetPlayerByID(Holder) != null)
			GameStateManager.Instance.GetPlayerByID(Holder).Drop(this);
	}

	private void Update()
	{
		if (!IsCollected)
			return;

		if (Holder == 0)
			return;

		PlayerInfo player = GameStateManager.Instance.GetPlayerByID (Holder);

		transform.position = player.gameObject.transform.position + m_PlayerOffset;
	}
}
