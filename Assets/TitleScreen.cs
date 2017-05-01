using UnityEngine;
using System.Collections;

public class TitleScreen : MonoBehaviour {

	public GameObject title;
	public GameObject game;

	// Use this for initialization
	IEnumerator Start () {

		yield return null;

		while (!Input.anyKey)
			yield return null;

		game.SetActiveRecursively(true);
		title.SetActiveRecursively(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
