using UnityEngine;
using System.Collections;

public class PersonalSpace : MonoBehaviour {

	private void OnTriggerEnter2D(Collider2D hit)
	{
		// Inform both players that their personal space was invaded
		Dispatcher.SendMessage(hit.transform.parent.name, "PersonalSpaceInvaded");
		Dispatcher.SendMessage(transform.parent.name, "PersonalSpaceInvaded");
	}

	private void OnTriggerExit2D(Collider2D hit)
	{
		// Inform both players that their personal space is not invaded anymore
		Dispatcher.SendMessage(hit.transform.parent.name, "PersonalSpaceUninvaded");
		Dispatcher.SendMessage(transform.parent.name, "PersonalSpaceUninvaded");
	} 
}
