using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ItemManager : MonoBehaviour
{
    public List<ItemSpawnpoint> AvailableItemSpawnLocations;
    public List<GameObject> ItemTemplates;

    public Dictionary<int, PickupItem> ItemDatabase = new Dictionary<int, PickupItem>();
    private int itemIndexer = 0;
	private GameStateManager m_GameStateManager;

    // Use this for initialization
    void Start()
    {
        // fill available item spawn locations from the level
        AvailableItemSpawnLocations = GameObject.FindObjectsOfType<ItemSpawnpoint>().ToList();
        Debug.Log("found available item spawn locations: " + AvailableItemSpawnLocations.Count);

        Debug.Log("item templates: " + ItemTemplates.Count);
    }

	private void OnEnable ()
	{
		m_GameStateManager = GetComponent<GameStateManager> ();
	}

	void Update()
    {
        // this handles the rendering of all the items - show the items which are up for grabs and hide the ones already picked by player
    }

    public void InitItems(List<PlayerInfo> players)
    {
        // lets create (2 x playercount) items in the level
        for (int i = 0; i < players.Count; i++)
        {
	        PlayerInfo player = players[i];

            // one item closest to the player location
            int closestIndex = GetClosestItemLocation(players[i].Position);
            InstantiateRandomItemAtLocationIndex (player, closestIndex);

            // second item farthest from the player location
            int farthestIndex = GetFarthestItemLocation(players[i].Position);
			InstantiateRandomItemAtLocationIndex(player, farthestIndex);
        }

        Dictionary<int, int> objectiveAssignments = new Dictionary<int, int>();
        players.ForEach(x => objectiveAssignments.Add(x.ID, 0));
        // lets give 2 items as objectives to each player
        foreach(PickupObject pickObject in ItemDatabase.Values)
        {
            int playerPick = GetRandomPlayer(pickObject.Owner, objectiveAssignments);
            if (playerPick == -1)
                continue;
            pickObject.ObjectiveForPlayer = playerPick;
            pickObject.ObjectiveIndex = objectiveAssignments[playerPick];
            objectiveAssignments[playerPick]++;
			Debug.Log ("Dispatching " + pickObject.gameObject.name);
			Dispatcher.SendMessage("Item", "Spawn", pickObject);
        }
    }

	private void InstantiateRandomItemAtLocationIndex (PlayerInfo player, int closestIndex)
	{
		if (closestIndex != -1)
		{
			int randomIndex = Random.Range (0, ItemTemplates.Count);
			SpawnItem(itemIndexer, player, ItemTemplates[randomIndex], AvailableItemSpawnLocations[closestIndex].transform.position);
			itemIndexer++;
			ItemTemplates.RemoveAt (randomIndex);
			AvailableItemSpawnLocations.RemoveAt (closestIndex);
		}
	}

	private GameObject SpawnItem (int itemID, PlayerInfo owner, GameObject itemPrefab, Vector3 position)
	{
		GameObject go = GameObject.Instantiate(itemPrefab, position, Quaternion.identity) as GameObject;
		go.name = itemPrefab.name;
		PickupItem item = go.GetComponent<PickupItem> ();
		item.ID = itemID;
		item.Owner = owner.ID;
		ItemDatabase.Add (itemID, item);

		return go;
	}

	private int GetRandomPlayer (int ignorePlayerID, Dictionary<int, int> playerObjectivesDict)
	{
		if (playerObjectivesDict.Count <= 1)
		{
			//Debug.Log("GetRandomPlayer: " + -1);
			return -1;
		}

		HashSet<int> eligiblePlayers = new HashSet<int> ();
		foreach (KeyValuePair<int, int> eligiblePlayer in playerObjectivesDict)
		{
			if (eligiblePlayer.Value < 2 && eligiblePlayer.Key != ignorePlayerID)
			{
				eligiblePlayers.Add (eligiblePlayer.Key);
			}
		}
		return eligiblePlayers.ElementAt (Random.Range (0, eligiblePlayers.Count));
    }

    int GetClosestItemLocation(Vector3 pos)
    {
        int index = -1;
        float distanceValue = float.MaxValue;
        for (int i = 0; i < AvailableItemSpawnLocations.Count; i++)
        {
            float distance = Vector3.Distance(AvailableItemSpawnLocations[i].transform.position, pos);
            if (distance < distanceValue)
            {
                distanceValue = distance;
                index = i;
            }
        }
        return index;
    }

    int GetFarthestItemLocation(Vector3 pos)
    {
        int index = -1;
        float distanceValue = float.MinValue;
        for (int i = 0; i < AvailableItemSpawnLocations.Count; i++)
        {
            float distance = Vector3.Distance(AvailableItemSpawnLocations[i].transform.position, pos);
            if (distance >= distanceValue)
            {
                distanceValue = distance;
                index = i;
            }
        }
        return index;
    }

	internal void RegisterItemSpawn(int itemID, string ItemName, Vector3 position, int ownerId, int seekerId, int seekIndex)
	{
		GameObject itemPrefab = ItemTemplates.Find ((x => x.name == ItemName));
		if (itemPrefab == null)
		{
			Debug.LogError ("Could not FInd Item" + ItemName);
			return;
		}
		PlayerInfo playerOwner = m_GameStateManager.GetPlayerByID (ownerId);
		var go = SpawnItem(itemID, playerOwner, itemPrefab, position);
		PickupObject pickObject = go.GetComponent<PickupItem>();
		pickObject.ObjectiveForPlayer = seekerId;
		pickObject.ObjectiveIndex = seekIndex;
	}

	private bool CheckPlayerWin(int playerID)
	{
		bool challenge1 = false;
		bool challenge2 = false;
		foreach (KeyValuePair<int, PickupItem> pair in ItemDatabase)
		{
			if (pair.Value.ObjectiveForPlayer == playerID)
				if (pair.Value.ObjectiveIndex == 0)
					challenge1 = pair.Value.IsCollected;
				else
					challenge2 = pair.Value.IsCollected;
		}

		return challenge1 && challenge2;
	}

	public PickupItem GetItemById (int itemId)
	{
		return ItemDatabase[itemId];
	}
}
