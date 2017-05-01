using System;
using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{


	public GameStateManager gameStateManager;

	private PhotonView photonView;

	private bool needToCreateOwnRoom = false;

	private void Start ()
	{
		photonView = PhotonView.Get(this);

		//Setup phase
		Dispatcher.Subscribe ("Player",  "Ready",         OnLocalPlayerReady);
		Dispatcher.Subscribe ("Player",  "SetSpawnPoint", OnSpawnPointSet);
		Dispatcher.Subscribe ("Item",    "Spawn",         OnItemSpawn);
		Dispatcher.Subscribe ("World",   "DataReady",     OnWorldDataReady);
		Dispatcher.Subscribe ("Player",  "WorldReady",    OnWorldReady);
		Dispatcher.Subscribe ("Match",   "Start",         OnMatchStart);

		//Interactive phase
		Dispatcher.Subscribe("Player", "Moved", OnPlayerMove);
		Dispatcher.Subscribe("PickupObject", "PlayerDidPickupItem", OnPlayerPickupItem);

	}

	void OnJoinedRoom()
	{
		bool isMaster = PhotonNetwork.room.playerCount == 1;
		Debug.Log ("Is master: " + isMaster);
		PlayerInfo myself = gameStateManager.CreateMySelf (PhotonNetwork.player.ID, isMaster);
		photonView.RPC("SetPlayerInfo", PhotonTargets.Others, myself.Name, myself.Position,myself.Score, myself.currentState);
    }

	void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		
		ClientPlayerInfo currentplayer = gameStateManager.currentPlayerInfo;

		if (currentplayer.currentState == PlayerInfo.PlayerState.WorldReady || currentplayer.currentState == PlayerInfo.PlayerState.Playing)
		{
			if (currentplayer.isMaster)
				photonView.RPC("RoomBusy", PhotonPlayer.Find(newPlayer.ID));
			return;
		}

		gameStateManager.RegisterNewPlayer(newPlayer.ID, "Player " + newPlayer.ID);

		photonView.RPC("SetPlayerInfo", PhotonPlayer.Find(newPlayer.ID), currentplayer.Name, currentplayer.Position, currentplayer.Score, currentplayer.currentState);
	}

	[RPC]
	private void SetPlayerInfo(string playerName, Vector3 position, int score, int playerState, PhotonMessageInfo messageInfo)
	{
		Debug.Log("GotPlayerInfo:" + playerName);
		PlayerInfo.PlayerState state =(PlayerInfo.PlayerState) playerState;
		gameStateManager.SetPlayerInfo(messageInfo.sender.ID, playerName, position, Color.green, score, state);
	}

	[RPC]
	private void RoomBusy (PhotonMessageInfo messageInfo)
	{
		RoomBusy ();
	}

	private void RoomBusy ()
	{
		Debug.Log("Room was busy");
		needToCreateOwnRoom = true;
		PhotonNetwork.LeaveRoom ();
	}

	private void OnLeftRoom ()
	{
		if (needToCreateOwnRoom)
		{
			needToCreateOwnRoom = false;
			PhotonNetwork.CreateRoom(null, true, true, 4);
		}
	}

	private void OnLocalPlayerReady(Subscription subscription)
	{
		int playerId = subscription.Read<int> (0);
		photonView.RPC ("SetPlayerReady", PhotonTargets.Others);
	}

	[RPC]
	void SetPlayerReady (PhotonMessageInfo messageInfo)
	{
		int playerId = messageInfo.sender.ID;
		Debug.Log (playerId + " is ready!");
		gameStateManager.SetPlayerReady (playerId);
	}

	private void OnSpawnPointSet(Subscription subscription)
	{
		PlayerInfo player = subscription.Read<PlayerInfo>(0);
		photonView.RPC("SetPlayerSpawnPoint", PhotonTargets.Others, player.ID, player.Position);
	}

	[RPC]
	void SetPlayerSpawnPoint(int playerId, Vector3 position, PhotonMessageInfo messageInfo)
	{
		Debug.Log(playerId + " wll spawn at "+ position);
		PlayerInfo player = gameStateManager.GetPlayerByID (playerId);
		player.Position = position;
	}

	private void OnItemSpawn(Subscription subscription)
	{
		PickupItem item = subscription.Read<PickupItem> (0);
		photonView.RPC("SpawnItem", PhotonTargets.Others, item.ID, item.gameObject.name, item.gameObject.transform.position, item.Owner, item.ObjectiveForPlayer, item.ObjectiveIndex );
	}

	[RPC]
	void SpawnItem(int itemID, string ItemName, Vector3 position, int ownerId, int seekerId, int seekIndex, PhotonMessageInfo messageInfo)
	{
		gameStateManager.itemManager.RegisterItemSpawn (itemID, ItemName, position, ownerId, seekerId, seekIndex);
	}

	private void OnWorldDataReady(Subscription subscription)
	{
		photonView.RPC("SetWorldDataReady", PhotonTargets.Others);
	}

	[RPC]
	void SetWorldDataReady(PhotonMessageInfo messageInfo)
	{
		Debug.Log(messageInfo.sender.ID + " says the world data is ready ");
		gameStateManager.SetWorldDataReady();
	}

	private void OnWorldReady(Subscription subscription)
	{
		photonView.RPC("SetWorldReady", PhotonTargets.Others);
	}

	[RPC]
	void SetWorldReady(PhotonMessageInfo messageInfo)
	{
		Debug.Log(messageInfo.sender.ID + " says the world is ready ");
		gameStateManager.SetWorldReady(messageInfo.sender.ID);
	}

	private void OnMatchStart(Subscription subscription)
	{
		photonView.RPC("StartMatch", PhotonTargets.All);
	}

	[RPC]
	void StartMatch(PhotonMessageInfo messageInfo)
	{
		Debug.Log(messageInfo.sender.ID + " says the match has started ");
		gameStateManager.StartMatch();
	}


	private bool IsPlayerReady()
	{
		if (gameStateManager.currentPlayerInfo.currentState != PlayerInfo.PlayerState.Playing)
		{
			RoomBusy();
			return false;
		}
		return true;
	}

	private void OnPlayerMove(Subscription subscription)
	{
		//Debug.Log ("Send Player Move");
		//int playerId = subscription.Read<int>(0);
		Vector3 position = subscription.Read<Vector3> (1);
		photonView.RPC("MovePlayer", PhotonTargets.Others, position);
	}

	[RPC]
	void MovePlayer(Vector3 position, PhotonMessageInfo messageInfo)
	{
		if (!IsPlayerReady())
			return;
		
		//Debug.Log ("Received a Player Move");
		gameStateManager.MoveRemotePlayer(messageInfo.sender.ID, position);
	}

	private void OnPlayerPickupItem(Subscription subscription)
	{
		int playerId = subscription.Read<int> (0);
		int itemId = subscription.Read<int>(1);
		photonView.RPC("PlayerPickUpItem", PhotonTargets.Others, playerId, itemId);
	}

	[RPC]
	void PlayerPickUpItem(int playerId, int itemId, PhotonMessageInfo messageInfo)
	{
		if (!IsPlayerReady())
			return;

		gameStateManager.PlayerPickItem(playerId, itemId);
	}
}
