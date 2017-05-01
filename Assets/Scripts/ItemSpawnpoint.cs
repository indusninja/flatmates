using UnityEngine;
using System.Collections;

public class ItemSpawnpoint : MonoBehaviour {

	public bool Available = true;

	void SetOccupied()
	{
		Available = false;
	}
}
