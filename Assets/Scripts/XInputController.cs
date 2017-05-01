using UnityEngine;
using XInputDotNetPure; // Required in C#

public class XInputController : MonoBehaviour
{
	public int playerNumber;

	// xinput
	public bool XInputEnabled = false;
	[HideInInspector]
	public bool controllerReady;
	bool playerIndexSet = false;
	PlayerIndex playerIndex;
	public GamePadState state;
	GamePadState prevState;

	public void Update ()
	{
		if (!XInputEnabled)
			return;

		// Find a PlayerIndex
		if (!playerIndexSet || !prevState.IsConnected)
		{
			PlayerIndex testPlayerIndex = (PlayerIndex)playerNumber - 1;
			GamePadState testState = GamePad.GetState (testPlayerIndex);
			if (testState.IsConnected)
			{
				Debug.Log (string.Format ("GamePad found {0}", testPlayerIndex));
				playerIndex = testPlayerIndex;
				playerIndexSet = true;
			}
			else
				return;
		}
		controllerReady = true;

		state = GamePad.GetState (playerIndex);

		prevState = state;
	}

	void OnDisable()
	{
		OnApplicationQuit();
	}

	void OnDestroy ()
	{
		OnApplicationQuit ();
	}

	void OnApplicationQuit ()
	{
		GamePad.SetVibration (playerIndex, 0, 0);
	}
}
