using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour {

    public bool Available = true;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void SetOccupied()
    {
        Available = false;
    }
}
