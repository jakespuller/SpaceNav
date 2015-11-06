using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class ISAMenu : MonoBehaviour {
	
	//Default ISA menu button selected
	private int selGridInt = 0;
	
	//Button values for the ISA selection menu
	private string[] selStrings = new string[] {"1. Under-Utilized:  Nothing to do.  Rather boring."
												,"2. Relaxed:  More than enough time for all tasks. Active on the task less than 50% of the time."
												,"3. Comfortably Busy Pace:  All tasks well in hand.  Busy but stimulating pace.\nCould keep going continuously at this level."
												,"4. High:  Non-essential tasks suffering.  Could not work at this level very long."
												,"5. Excessive:  Behind on tasks.  Losing track of the full picture."};

	
	private bool toggleTxt = false;
	//private bool toggleImg = false;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void OnGUI() {
		
		//Create length units that are dynamic depending on screen size
		int heightUnit = Screen.height / 9;
		int widthUnit = Screen.width / 10;
		
		//Title of the page
		GUIStyle titleStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
		titleStyle.fontSize = Screen.height/20;
		titleStyle.alignment = TextAnchor.UpperCenter;
		GUI.Label(new Rect((float)0.25*widthUnit, 0*heightUnit, (float)9.5*widthUnit, 7*heightUnit), "ISA Rating:\nSelect ONE rating that best indicates your workload for the just-completed level",titleStyle);
		
		//Create the selection grid in a button style
		GUILayout.BeginArea(new Rect(1*widthUnit, 2*heightUnit, 8*widthUnit, 5*heightUnit));
		selGridInt = GUILayout.SelectionGrid(selGridInt, selStrings, 1, "Button"); 
		GUI.skin.button.fontSize = Screen.height/30;
		GUI.skin.button.alignment = TextAnchor.MiddleLeft;
		GUILayout.EndArea();
		
		//Create a check box that verifies a save action
		GUIStyle toggleStyle = new GUIStyle(GUI.skin.GetStyle("Toggle"));
		toggleStyle.fontSize = Screen.height/30;
		toggleStyle.wordWrap = true;
		toggleStyle.alignment = TextAnchor.MiddleCenter;
		toggleStyle.imagePosition = ImagePosition.ImageAbove;
		toggleTxt = GUI.Toggle(new Rect(1*widthUnit, 7*heightUnit, 2*widthUnit, 1*heightUnit),toggleTxt,"This box must be checked before answers will save.",toggleStyle);
		
		//Create a save button that writes survey results to a file and moves to the NASA TLX survey
		GUIStyle saveButton = new GUIStyle(GUI.skin.GetStyle("Button"));
		saveButton.fontSize = Screen.height/20;
		saveButton.alignment = TextAnchor.MiddleCenter;
		if(GUI.Button(new Rect(4*widthUnit, 7*heightUnit, 2*widthUnit, 1*heightUnit), "Save Answers", saveButton)) {
			if(toggleTxt) {
				writeToFile();
				Application.LoadLevel (3);
			} 
		}
	}
	
	//Write the content of the ISA survey to a file
	void writeToFile() {
		GameObject data = GameObject.FindGameObjectWithTag("GameData");
		
		//Make sure we have a game data object
		if (data != null) {
			//Create a new save file for each level of the game
			gameData gd = (gameData) data.GetComponent("gameData");
			string fileName = "ISA_feedback_" + (gd.gameLevel-1) + ".txt";
			
			//Capture the response for this level
			string outString = "Level = " + (gd.gameLevel - 1) + ", Selected State = " + selStrings[selGridInt];
			using(StreamWriter streamer = new StreamWriter(fileName,false)) {
				streamer.WriteLine(outString);
				streamer.Close();
			}

		} else {
			Debug.LogError("No game data");
			Application.Quit();
		}

	}
}
