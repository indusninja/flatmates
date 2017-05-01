using UnityEngine;
using System.Collections;

public class OnJoinRoom : Photon.MonoBehaviour {

	public void OnJoinedRoom()
	{
		Dispatcher.SendMessage ("Network", "OnJoinedRoom");
		Debug.Log ("JoinedRoom!!!");
    }
}
