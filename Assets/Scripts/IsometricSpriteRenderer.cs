using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class IsometricSpriteRenderer : MonoBehaviour {

	void Update ()
	{
		renderer.sortingOrder = 10000 - (int)(transform.position.y * 10);
	}
}
