using UnityEngine;
using System.Collections;

public class ControlsMenu : MonoBehaviour 
{
	public Texture2D background; // Texture for the background of your start menu
	public GUISkin back;//Allows you to set your back button GUIskin in the Inspector
	private string buttonText = "";//Allows you to set the text your button says. Change the word private to public to make this avaible if you want to add text without and image.
	
	void OnGUI()
	{
		GUI.DrawTexture(new Rect (0,0,Screen.width,Screen.height),background); //Draws the Background
		GUI.skin = back;
		
		if (GUI.Button(new Rect(103, 34, 297, 85),buttonText))
		{
			Application.LoadLevel("StartMenu");
		}
	}
}
