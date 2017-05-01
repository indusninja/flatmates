using UnityEngine;
using System.Collections;

public class PlayerState : GameScript
{
	public float health;
	private bool isPersonalSpaceInvaded;

	private void Start()
	{
		base.Start();

		Subscribe(name, "PersonalSpaceInvaded", PersonalSpaceInvaded);
		Subscribe(name, "PersonalSpaceUninvaded", PersonalSpaceUninvaded);
	}

	private void Update()
	{
		if (isPersonalSpaceInvaded)
		{
			health -= Time.deltaTime;
		}
	}

	private void PersonalSpaceInvaded()
	{
		isPersonalSpaceInvaded = true;
	}

	private void PersonalSpaceUninvaded()
	{
		isPersonalSpaceInvaded = false;
	}
}
