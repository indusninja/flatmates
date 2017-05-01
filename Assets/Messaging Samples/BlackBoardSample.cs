using UnityEngine;
using System.Collections;

public class BlackBoardSample : MonoBehaviour {

	void Start () {

        // This will be committed to the database in the end of the frame
        BlackBoard.Write("Player1", "Position", transform.position);
	}

    void Update()
    {
        // We can read the data from any object. If the data is not set 'default' value will be returned.
        Vector3 position = BlackBoard.Read<Vector3>("Player1", "Position");

        // BB can save value type fields from boards to the disc. Could be broken atm ;)
        BlackBoard.Write("SaveData", "PlayerName", "Foo");
        BlackBoard.Write("SaveData", "PlayerScore", 9001);
        BlackBoard.Write("SaveData", "IsPersistent", true);
        BlackBoard.Save();

        // There is some pretty handy functionalities in BB
    }
}
