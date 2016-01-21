using UnityEngine;
using System.Collections;

public class GameOver : MonoBehaviour {
	public Texture2D background; // Texture for the background of your start menu
	public GUISkin exit;//Allows you to set your exit button GUIskin in the Inspector
	public GUISkin start;//Allows you to set your start button GUIskin in the Inspector
	public GUISkin menubackground;//Allows you to set your background GUIskin in the Inspector
	private string backgroundText = "";//Allows you to set text for the background.
	private string buttonText = "";//Allows you to set text for the background.
	private string button1Text = "";//Allows you to set the text your button says. Change the word private to public to make this avaible if you want to add text without and image.
	
	void OnGUI()
	{
		GUI.DrawTexture(new Rect (0,0,Screen.width,Screen.height),background); //Draws the Background
		GUI.skin = menubackground;
		GUI.Box(new Rect(18,13,262, 304),backgroundText);
		GUI.skin = start;
		
		if (GUI.Button(new Rect(48, 28, 212, 100),buttonText))
		{
			Application.LoadLevel("StartMenu");
		}
		GUI.skin = exit;
		
		if (GUI.Button(new Rect(-74, 140, 455, 82),button1Text))
		{
			Application.Quit();
		}
	}
}	
