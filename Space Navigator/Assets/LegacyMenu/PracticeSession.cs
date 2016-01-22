using UnityEngine;
using System.Collections;
using System.IO;

public class PracticeSession : MonoBehaviour {
	private bool not_pressed = true;
	public EyeTrackerTCP eyetracker;
	public GameObject gameData;
	private string uniqueID;
	private int maxComplete;
	private int practiceLevels;
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
		bool exists = true;
		practiceLevels = 1;
		while(exists) {
			string fileName = "PracticeLevel_" + practiceLevels + ".txt";
			if (!File.Exists(fileName)) {
				StreamWriter streamer = new StreamWriter(fileName, true);
				streamer.WriteLine ("practiceLevel: " + practiceLevels + "\n");
				if (practiceLevels == 1)
				{
					streamer.WriteLine ("uniqueID:" + uniqueID + "\n");
				}
				exists = false;
			} else {
				practiceLevels += 1;
			}
		}
		gd.gameLevel = practiceLevels;
		//gameLevel = gd.gameLevel;
		gd.instanceNum = practiceLevels;
		uniqueID = "0";
		gd.pram_list.practiceSession = 1;
	}
	
	// Update is called once per frame
	void OnGUI() {
		//Style for save button, set up so that it scales with screen size	
		GUIStyle inputButton = new GUIStyle (GUI.skin.GetStyle ("Button"));
		inputButton.fontSize = Screen.width / 20;
		inputButton.alignment = TextAnchor.MiddleCenter;
		//Create length units that are dynamic depending on screen size
		int heightUnit = Screen.height / 16;
		int widthUnit = Screen.width / 16;
		GUI.Box (new Rect (3 * widthUnit, 1 * heightUnit, 10 * widthUnit, 14 * heightUnit), "Practice Menu");
		//Title of the page
		GUI.skin.box.fontSize = Screen.width / 30;
		//Button to load the next level
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		if (data != null)
		{
			gameData gd = (gameData)data.GetComponent ("gameData");
			bool button = GUI.Button(new Rect (4 * widthUnit, 5 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Start Practice Game", inputButton);
			if (button)
			{
				gd.unique_id = uniqueID;
				Application.LoadLevel (1);
			}
			if (not_pressed)
			{
				bool button1 = GUI.Button(new Rect (4 * widthUnit, 10 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Calibration", inputButton);
				if (button1)
				{
					eyetracker = new EyeTrackerTCP();
					eyetracker.calibrate();
					not_pressed = false;
				}
			}
			if (!not_pressed)
			{
				bool button2 = GUI.Button(new Rect (4 * widthUnit, 10 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Baseline", inputButton);
				if (button2)
				{
					gd.unique_id = uniqueID;
					Application.LoadLevel(3);
				}
			}
		}
	}
}
