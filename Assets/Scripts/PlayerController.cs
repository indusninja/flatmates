using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
internal class PlayerController : MonoBehaviour
{
	public float force;
	public float speed;

	public enum ControllerType { None, Xbox, Keyboard }
	public ControllerType controller;

	public int controllerID = 0;
	public int playerID = 0;
	public AnimationClip myAnimation;
	private bool AnimationSet = false;

	private Transform myTransform;
	private Transform sprite;

	private GameStateManager gameController;

	public Texture2D ScoreTexture;
	public Texture2D WantTexture;

	public Texture2D WinTexture;
	public Texture2D LoseTexture;

	public GUIStyle WinMessageFont;

	private PickupItem Item1;
	private PickupItem Item2;

	Dictionary<int, bool> PlayerItem1Status = new Dictionary<int, bool>();
	Dictionary<int, bool> PlayerItem2Status = new Dictionary<int, bool>();

	void OnEnable()
	{
		myTransform = GetComponent<Transform>();
		sprite = transform.Find("Sprite");

		gameController = GameStateManager.Instance;
		foreach (PickupItem item in GameStateManager.Instance.itemManager.ItemDatabase.Values)
		{
			if (item.ObjectiveForPlayer == playerID)
			{
				if (item.ObjectiveIndex == 0)
					Item1 = item;
				else
					Item2 = item;
			}
		}

		// applying a proper animation

	}


	public void Update()
	{
		//base.Update();

		//if (controller == ControllerType.Xbox && !controllerReady)
		//    return;

		float horizontal = 0;
		float vertical = 0;

		switch (controller)
		{
			case ControllerType.Keyboard:
				horizontal = (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
				vertical = (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
				break;

			case ControllerType.Xbox:
				//horizontal = state.ThumbSticks.Left.X;
				//vertical = state.ThumbSticks.Left.Y;
				horizontal = Input.GetAxis("P" + controllerID + "Horizontal");
				vertical = Input.GetAxis("P" + controllerID + "Vertical");
				break;
		}

		rigidbody2D.AddForce(new Vector2(horizontal * force, vertical * force));
		rigidbody2D.velocity = new Vector2(Mathf.Clamp(rigidbody2D.velocity.x, -speed, speed), Mathf.Clamp(rigidbody2D.velocity.y, -speed, speed));

		// flip the player sprite when going left/right
		if (horizontal < 0)
		{
			sprite.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
		}
		else if (horizontal > 0)
		{
			sprite.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
		}

		if (myAnimation != null && !AnimationSet)
		{
			Animator myAnimator = GetComponent<Animator>();
			myAnimator.Play(myAnimation.name);
			AnimationSet = true;
		}

		foreach (PickupItem item in GameStateManager.Instance.itemManager.ItemDatabase.Values)
		{
			if (item.ObjectiveIndex == 0)
			{
				if (!PlayerItem1Status.ContainsKey(item.ObjectiveForPlayer))
				{
					PlayerItem1Status[item.ObjectiveForPlayer] = false;
				}
				PlayerItem1Status[item.ObjectiveForPlayer] = item.IsStolen;
			}
			else
			{
				if (!PlayerItem2Status.ContainsKey(item.ObjectiveForPlayer))
				{
					PlayerItem2Status[item.ObjectiveForPlayer] = false;
				}
				PlayerItem2Status[item.ObjectiveForPlayer] = item.IsStolen;
			}
		}
	}

	void OnGUI()
	{
		GUI.DrawTexture(new Rect(0, Screen.height - 40, 111, 29), ScoreTexture);

		Rect wantarea = new Rect(Screen.width - 100, Screen.height - 50, 94, 41);
		GUI.DrawTexture(wantarea, WantTexture);

		Sprite itemSprite = Item1.IsStolen ? Item2.GetComponent<SpriteRenderer>().sprite : Item1.GetComponent<SpriteRenderer>().sprite;
		Texture2D wantItem = itemSprite.texture;
		Rect ItemRect = new Rect(wantarea.x + 65, wantarea.y + 13, 17, 14);
		Rect textCoords = new Rect(itemSprite.rect.x / wantItem.width, itemSprite.rect.y / wantItem.height,
			itemSprite.rect.width / wantItem.width, itemSprite.rect.height / wantItem.height);
		GUI.DrawTextureWithTexCoords(ItemRect, wantItem, textCoords, true);

		foreach (int playerid in gameController.GetPlayersDict().Keys)
		{
			if (PlayerItem1Status.ContainsKey(playerid) && PlayerItem2Status.ContainsKey(playerid))
			{
				if (PlayerItem1Status[playerid] && PlayerItem2Status[playerid])
				{
					if (playerid == playerID)
					{
						GUI.DrawTexture(new Rect((Screen.width - 320) / 2, (Screen.height - 180) / 2, 320, 180), WinTexture);
						//GUI.Label(new Rect(0, (Screen.height / 2) - 10, Screen.width, 20), "All their stuff IS yours!", WinMessageFont);
						//Time.timeScale = 0;
						return;
					}
					else
					{
						GUI.DrawTexture(new Rect((Screen.width - 320) / 2, (Screen.height - 180) / 2, 320, 180), LoseTexture);
						//GUI.Label(new Rect(0, (Screen.height / 2) - 10, Screen.width, 20), "Its probably time to move out!", WinMessageFont);
						//Time.timeScale = 0;
						return;
					}
				}
			}
		}
		/*if (Item1.IsStolen && Item2.IsStolen)
		{
			
		}
		else
		{

		}*/
	}

	public void FixedUpdate()
	{
		Dispatcher.SendMessage("Player", "Moved", playerID, myTransform.position);
	}
}
