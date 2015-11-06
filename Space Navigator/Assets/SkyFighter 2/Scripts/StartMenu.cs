using UnityEngine;
using System.Collections;

public class StartMenu : MonoBehaviour 
{
	public Texture2D background; // Texture for the background of your start menu
	public GUISkin menubackground;//Allows you to set your background GUIskin in the Inspector
	public GUISkin start;//Allows you to set your start button GUIskin in the Inspector
	public GUISkin exit;//Allows you to set your exit button GUIskin in the Inspector
	public GUISkin controls;//Allows you to set your controls button GUIskin in the Inspector
	private string backgroundText = "";//Allows you to set text for the background.
	private string button2Text = "";//Allows you to set the text your button says. Change the word private to public to make this avaible if you want to add text without and image.
	private string button3Text = "";//Allows you to set the text your button says. Change the word private to public to make this avaible if you want to add text without and image.
	private string button4Text = "";//Allows you to set the text your button says. Change the word private to public to make this avaible if you want to add text without and image.
	
	void OnGUI()
    {
		GUI.DrawTexture(new Rect (0,0,Screen.width,Screen.height),background); //Draws the Background
		GUI.skin = menubackground;
		GUI.Box(new Rect(102,21,292, 318),backgroundText);
		GUI.skin = start;
		
		if (GUI.Button(new Rect(103, 34, 297, 85),button2Text))
		{
			Application.LoadLevel("SkyFighter 2 Space Starter Kit");
		}
		GUI.skin = controls;
		
		if (GUI.Button(new Rect(141, 111, 221, 100),button3Text))
		{
			Application.LoadLevel("Controls");
		}
		GUI.skin = exit;
		
		if (GUI.Button(new Rect(68, 193, 358, 88),button4Text))
		{
			Application.Quit();
		}
	}
}
