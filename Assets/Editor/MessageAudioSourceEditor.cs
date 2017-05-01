using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MessageAudioSource))]
class MessageAudioSourceEditor : Editor
{
	const int prefixWidth = 60;
	const int fieldWidth = 150;

	public override void OnInspectorGUI()
	{
		MessageAudioSource manager = target as MessageAudioSource;

		foreach (var link in manager.audioLinks)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical ();
			EditorGUILayout.BeginHorizontal();

			GUILayout.Label("Domain", GUILayout.Width(prefixWidth));
			link.domain = GUILayout.TextField(link.domain, GUILayout.Width(fieldWidth));

			GUILayout.Label("Message", GUILayout.Width(prefixWidth));
			link.message = GUILayout.TextField(link.message, GUILayout.Width(fieldWidth));

			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();

			GUILayout.Label("Clip", GUILayout.Width(prefixWidth));
			link.audioClip = EditorGUILayout.ObjectField(link.audioClip, typeof(AudioClip), false, GUILayout.Width(fieldWidth + 18)) as AudioClip;

			//GUILayout.Space(10);
			//GUILayout.Label("Override", GUILayout.Width(prefixWidth));
			//link.playInstantly = GUILayout.Toggle(link.playInstantly, "", GUILayout.Width(10));

			//GUILayout.Space(30);
			//GUILayout.Label("XFade", GUILayout.Width(prefixWidth));
			//link.xfade = GUILayout.Toggle(link.xfade, "", GUILayout.Width(10));

			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical();

			if (GUILayout.Button("-", GUILayout.Width(20)))
			{
				manager.audioLinks.Remove(link);
				return;
			}

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);
		}

		if (GUILayout.Button ("Add"))
			manager.audioLinks.Add(new MessageAudioSource.AudioLink());
	}
}
