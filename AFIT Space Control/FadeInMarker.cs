using UnityEngine;
using System.Collections;
//Inspiration for this method brought to you by 
//http://forum.unity3d.com/threads/fade-in-and-fade-out-of-a-gameobject.4723/
public class FadeInMarker : MonoBehaviour {
	Color colorStart, colorEnd;
	public float markerDelay;
	public float duration = 4f;
	// Use this for initialization
	void Start () {
		//Initializes the gameobject to transparent
		float a = 0.0f;
		gameObject.renderer.material.color = new Color (1.0f, 1.0f, 1.0f, a);
		colorStart = gameObject.renderer.material.color;
		colorEnd = new Color (colorStart.r, colorStart.g, colorStart.b, 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
		StartCoroutine (FadeIn ());
	}
	
	public IEnumerator FadeIn()
	{
		//Fades in the item to opaque in X seconds
		for (float t = 0.0f; t < duration; t+= Time.deltaTime)
		{
			if (t+Time.deltaTime > duration)
			{
			gameObject.renderer.material.color = colorEnd;
			}
			yield return null;
		}
	}
}
