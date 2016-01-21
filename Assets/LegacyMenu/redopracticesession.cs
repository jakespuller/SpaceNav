using UnityEngine;
using System.Collections;
using System.IO;


public class redopracticesession : MonoBehaviour {
	public GameObject gameData;
	// Use this for initialization
	void Start () {
		//Create a new game data object in order to capture data across Unity levels
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		gameData gd;
		//Instantiate the game object data if the game is just starting
		if (data == null) {
			data = (GameObject)Instantiate (gameData, Vector3.zero, Quaternion.identity);
		} 
		gd = (gameData)data.GetComponent ("gameData");
		gd.scriptedInstance = false;
	}
	
	void OnGUI()
	{
		//Style for save button, set up so that it scales with screen size	
		GUIStyle inputButton = new GUIStyle (GUI.skin.GetStyle ("Button"));
		inputButton.fontSize = Screen.width / 20;
		inputButton.alignment = TextAnchor.MiddleCenter;
		//Create length units that are dynamic depending on screen size
		int heightUnit = Screen.height / 16;
		int widthUnit = Screen.width / 16;
		GUI.Box (new Rect ((float)2.5 * widthUnit, 1 * heightUnit, 12 * widthUnit, 14 * heightUnit), "Practice Menu");
		//Title of the page
		GUI.skin.box.fontSize = Screen.width / 30;
		//Button to load the next level
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		if (data != null)
		{
			gameData gd = (gameData)data.GetComponent ("gameData");
			bool button = GUI.Button(new Rect (3 * widthUnit, 5 * heightUnit, 11 * widthUnit, 4 * heightUnit), "Play Another Practice Game", inputButton);
			if (button)
			{
				Application.LoadLevel (0);
			}
			bool button1 = GUI.Button(new Rect (3 * widthUnit, 10 * heightUnit, 11 * widthUnit, 4 * heightUnit), "Continue with Experiment", inputButton);
			if (button1)
			{
				for (int i = gd.gameLevel; i > 0; i--)
				{
					string filename = "PracticeLevel_"+i+".txt";
					File.Delete (filename);
				}
				gd.pram_list.practiceSession = 0;
				Application.LoadLevel (5);
			}
		}
	}
}
