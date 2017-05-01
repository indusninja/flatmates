using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MessageAudioSource : GameScript {

	[System.Serializable]
	public class AudioLink
	{
		public string domain = "";
		public string message = "";
		public AudioClip audioClip = null;
		public bool playInstantly = false;
		public bool xfade = false;
	}

	public List<AudioLink> audioLinks;

	AudioSource audioSource;

	void Start()
	{
		base.Start();

		foreach (var link in audioLinks)
			if (link.domain != "" && link.message != "")
				Subscribe(link.domain, link.message, Play);

		audioSource = GetComponent<AudioSource>();

		InvokeRepeating("SendTestMessage", 1, 1);
	}

	void SendTestMessage()
	{
		SendMessage("Audio", "Test");
	}

	void Play(Subscription subsciption)
	{
		foreach (var link in audioLinks)
		{
			if (link.domain == subsciption.domain && link.message == subsciption.message)
			{
				if (!link.audioClip)
					continue;

				audioSource.PlayOneShot(link.audioClip);
			}
		}
	}
}
