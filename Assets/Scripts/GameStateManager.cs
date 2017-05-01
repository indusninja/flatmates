using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : GameScript
{
	private static GameStateManager s_Instance;

	private Time GameStartTime;

	private Dictionary<int, PlayerInfo> players;

	private ClientPlayerInfo m_CurrentPlayerInfo;
    public ItemManager itemManager;

	public GameObject PlayerPrefab;
	public bool useNetwork = false;

	private SpawnPoint[] LevelSpawns;

	//public Texture2D PlayerSprite1;
	//public Texture2D PlayerSprite2;
	//public Texture2D PlayerSprite3;
	//public Texture2D PlayerSprite4;

	public List<AnimationClip> PlayerIdleClips;
	int animationIndex = 0;

	public List<Color> PlayerColors;
	int colorIndex = 0;

	static public GameStateManager Instance 
	{
		get
		{
			return s_Instance;
		}
	}

	public ClientPlayerInfo currentPlayerInfo
	{ 
		get { return m_CurrentPlayerInfo; }
	}

	public PlayerInfo GetPlayerByID (int id)
	{
		PlayerInfo player;
		if (players.TryGetValue (id, out player))
			return players[id];

		Debug.LogError ("Could not find key "+ id);
		return null;
	}

	public PlayerInfo GetPlayerByGameObject (GameObject go)
	{
		foreach (PlayerInfo player in GameStateManager.Instance.GetPlayersDict().Values)
		{
			if (go == player.gameObject)
			{
				return player;
			}
		}
		return null;
	}

	public Dictionary<int, PlayerInfo> GetPlayersDict ()
	{
		return players;
	} 

	void Start()
	{
		base.Start();

		players = new Dictionary<int, PlayerInfo> ();
		Debug.Log("Waiting for host to join game... Press A to join");

		if (s_Instance != null)
		{
			Debug.LogError ("For some reason there are more than one GameStatemanager, DIE");
		}
		s_Instance = this;

	}

	void OnEnable ()
	{
		LevelSpawns = GameObject.FindObjectsOfType<SpawnPoint>();
		if (LevelSpawns == null || LevelSpawns.Length == 0)
		{
			Debug.LogError("No spawn points found in the level");
		}
		else
		{
			Debug.Log(LevelSpawns.Length + " spawn points found");
		}
	}

	private void JoinSelfPlayer(int playerID)
	{
		if (useNetwork)
			playerID = PhotonNetwork.player.ID;

		m_CurrentPlayerInfo = CreateMySelf(playerID, true);
		Debug.Log("host player created");

		SpawnPoint spawnLocation = GetRandomSpawn();
        if (spawnLocation == null)
            return;

		GameObject player = null;
		if (useNetwork)
			player = PhotonNetwork.Instantiate(PlayerPrefab.name, spawnLocation.transform.position, Quaternion.identity, 0);
		else
		
			player = Instantiate (PlayerPrefab, spawnLocation.transform.position, Quaternion.identity) as GameObject;

		Debug.Log("player: " + player);
		player.name = "Player" + playerID;
		spawnLocation.Available = false;

		//player.GetComponentInChildren<SpriteRenderer>().color = m_CurrentPlayerInfo.Color;
		PlayerController controller = player.GetComponentInChildren<PlayerController>();
		controller.playerID = playerID;
		controller.controller = m_CurrentPlayerInfo.ID > 4 ? PlayerController.ControllerType.Keyboard : PlayerController.ControllerType.Xbox;
		controller.controllerID = m_CurrentPlayerInfo.ID;
		controller.myAnimation = PlayerIdleClips[m_CurrentPlayerInfo.AnimationIndex];

		Debug.Log("Waiting for other players to join game... Press A to join");
	}

	void Update()
	{
		if(!useNetwork && Input.anyKeyDown)
		{
			int playerID = FindControllerID();
			if (playerID != -1)
			{
				if (m_CurrentPlayerInfo == null)
				{
					JoinSelfPlayer(playerID);
				}
				else if (m_CurrentPlayerInfo.ID != playerID && !m_CurrentPlayerInfo.OpponentPlayers.ContainsKey(playerID))
				{
					SpawnPoint spawnLocation = GetRandomSpawn();
                    if (spawnLocation == null)
                        return;

					PlayerJoined(playerID, "Player" + playerID, spawnLocation.transform.position, Color.green, 0, PlayerInfo.PlayerState.Connected);
					spawnLocation.Available = false;
					Debug.Log("player " + playerID + "created");
				}
			}
		}

		if (useNetwork && Input.anyKeyDown && m_CurrentPlayerInfo != null && m_CurrentPlayerInfo.currentState == PlayerInfo.PlayerState.Connected)
		{
			int controllerID = FindControllerID();
			if (controllerID != -1)
			{
				m_CurrentPlayerInfo.controllerID = controllerID;
				SetPlayerReady (m_CurrentPlayerInfo.ID);
				Dispatcher.SendMessage ("Player", "Ready", m_CurrentPlayerInfo.ID);
			}
		}
	}

	int FindControllerID()
	{
		int controllerID = -1;
		for (int i = 1; i < 4; i++)
		{
			string keyCode = "Joystick" + (i == 0 ? "" : i.ToString()) + "Button0";
			//Debug.Log("checking: " + keyCode + " : " + Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), keyCode)));
			if (Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), keyCode)))
				return i;
		}
		if (Input.anyKey)
			return 5;
		return controllerID;
	}

	public void PlayerJoined(int id, string name, Vector3 position, Color color, int score, PlayerInfo.PlayerState playerstate)
	{
		
		Debug.Log("Player " + id + " just joined the game");
		RegisterNewPlayer(id, name);
		PlayerInfo newPlayer = SetPlayerInfo(id, name, position, color, score, playerstate);

		SpawnPlayer (newPlayer);
	}

	SpawnPoint GetRandomSpawn()
	{
        if (!LevelSpawns.Any(x => x.Available))
        {
            Debug.LogError("No Spawn Locations found!!!");
            return null;
        }
		SpawnPoint sp = LevelSpawns[UnityEngine.Random.Range(0, LevelSpawns.Length)];
		while(!sp.Available)
		{
			sp = LevelSpawns[UnityEngine.Random.Range(0, LevelSpawns.Length)];
		}
		return sp;
	}

	public void RegisterNewPlayer(int id, string name)
	{
		var player = new PlayerInfo(id, name, Vector3.zero, Color.black, 0, animationIndex, false);
		players.Add(id, player);
		animationIndex++;
		m_CurrentPlayerInfo.AddOpponent(player);
	}

	public PlayerInfo SetPlayerInfo(int id, string playerName, Vector3 position, Color color, int score, PlayerInfo.PlayerState playerstate)
	{
		if (!players.ContainsKey (id))
		{
			RegisterNewPlayer (id, playerName);
		}
		PlayerInfo player = players[id];
		player.Name = playerName;
		player.Position = position;
		player.Color = color;
		player.Score = score;
		player.currentState = playerstate;
		return player;
	}

	public ClientPlayerInfo CreateMySelf(int playerID, bool isMaster)
	{
		m_CurrentPlayerInfo = new ClientPlayerInfo(playerID, "Player" + playerID, Vector3.zero, Color.red, 0, animationIndex, isMaster);
		animationIndex++;
		players.Add(playerID, m_CurrentPlayerInfo);
		return m_CurrentPlayerInfo;
	}

	public void SetPlayerReady (int playerId)
	{
		PlayerInfo player = players[playerId];
		player.currentState = PlayerInfo.PlayerState.PlayerReady;

		bool allPlayersReady = true;
		foreach (KeyValuePair<int, PlayerInfo> playerTuple in players)
		{
			if (playerTuple.Value.currentState != PlayerInfo.PlayerState.PlayerReady)
			{
				allPlayersReady = false;
				break;
			}
		}
		if (allPlayersReady)
			OnAllPlayersReady ();
	}

	private void OnAllPlayersReady ()
	{
		if (!useNetwork)
			throw new NotImplementedException();

		Debug.Log ("currentPlayer " + currentPlayerInfo.ID);
		Debug.Log ("IsMaster? " + currentPlayerInfo.isMaster);
		if (currentPlayerInfo.isMaster)
		{
			SetupWorld ();
		}
	}

	private void SetupWorld ()
	{
		//Setup item spawn points
        itemManager.InitItems(players.Values.ToList());

		//setup player spawn points
		SetupPlayerSpawnPoints ();

		Dispatcher.SendMessage("World", "DataReady");
		SetWorldDataReady ();
	}

	//Sets up all players initial spawn points
	private void SetupPlayerSpawnPoints ()
	{
		foreach (KeyValuePair<int, PlayerInfo> playerTuple in players)
		{
			SpawnPoint spawnLocation = GetRandomSpawn();
            if (spawnLocation == null)
                continue;
			PlayerInfo player = playerTuple.Value;
			player.Position = spawnLocation.transform.position;

			Dispatcher.SendMessage("Player", "SetSpawnPoint", player);

			spawnLocation.Available = false;
		}
	}

	public void SetWorldDataReady ()
	{
		foreach (KeyValuePair<int, PlayerInfo> playerTuple in players)
		{
			PlayerInfo player = playerTuple.Value;
			SpawnPlayer (player);
		}
		currentPlayerInfo.currentState = PlayerInfo.PlayerState.WorldReady;
		Dispatcher.SendMessage("Player", "WorldReady", currentPlayerInfo);
	}

	private void SpawnPlayer (PlayerInfo player)
	{
		Debug.Log ("Spawning Player: " + player.ID);
		GameObject playerGO = (GameObject)Instantiate(PlayerPrefab, player.Position, Quaternion.identity);
		//playerGO.GetComponentInChildren<SpriteRenderer>().color = player.Color;
		playerGO.name = "Player" + player.ID;

		player.gameObject = playerGO;

		ClientPlayerInfo localPlayer = player as ClientPlayerInfo;

		PlayerController controller = playerGO.GetComponentInChildren<PlayerController>();

		if (localPlayer != null)
		{
			controller.controller = localPlayer.controllerID > 4 ? PlayerController.ControllerType.Keyboard : PlayerController.ControllerType.Xbox;
			controller.controllerID = localPlayer.controllerID;
		}
		controller.playerID = player.ID;
		controller.myAnimation = PlayerIdleClips[player.AnimationIndex];
		
		controller.enabled = false;

		PlayerRoom[] rooms = FindObjectsOfType<PlayerRoom>();
		foreach (var room in rooms)
		{
			BoxCollider2D box = room.GetComponent<BoxCollider2D>();
			if (box.OverlapPoint(player.Position))
				room.owner = player.ID;
		}
	}

	public void MoveRemotePlayer (int id, Vector3 position)
	{
		PlayerInfo player = GetPlayerByID (id);
		player.Position = position;
		player.gameObject.transform.position = position;
	}

	public void SetWorldReady (int id)
	{
		PlayerInfo player = GetPlayerByID(id);
		player.currentState = PlayerInfo.PlayerState.WorldReady;

		bool allWorldReady = true;
		foreach (KeyValuePair<int, PlayerInfo> playerTuple in players)
		{
			if (playerTuple.Value.currentState != PlayerInfo.PlayerState.WorldReady)
			{
				allWorldReady = false;
				break;
			}
		}
		if (allWorldReady)
			OnAllWorldssReady();
	}

	private void OnAllWorldssReady ()
	{
		if (!useNetwork)
			throw new NotImplementedException();

		Debug.Log ("currentPlayer " + currentPlayerInfo.ID);
		Debug.Log ("IsMaster? " + currentPlayerInfo.isMaster);
		if (currentPlayerInfo.isMaster)
		{
			SendStartMatch ();
		}
	}

	private void SendStartMatch ()
	{
		Dispatcher.SendMessage ("Match", "Start");
		currentPlayerInfo.currentState = PlayerInfo.PlayerState.Playing;
	}

	public void StartMatch ()
	{
		foreach (KeyValuePair<int, PlayerInfo> playerTuple in players)
		{
			PlayerInfo player = playerTuple.Value;
			player.currentState = PlayerInfo.PlayerState.Playing;

			ClientPlayerInfo localPlayer = player as ClientPlayerInfo;

			
			if (localPlayer != null)
			{
				localPlayer.gameObject.GetComponent<PlayerController> ().enabled = true;
			}
		}
	}

	public void PlayerPickItem (int playerId, int itemId)
	{
		PickupItem item = itemManager.GetItemById (itemId);

		item.PickUp (playerId);
	}
}
