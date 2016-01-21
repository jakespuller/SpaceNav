using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class ScriptMenu : MonoBehaviour {
	
	private int maxComplete;
	public GameObject gameData;
	public int numScriptedInstances;
		
	// Use this for initialization
	void Start() {
		

		//Create a new game data object in order to capture data across Unity levels
		GameObject data = GameObject.FindGameObjectWithTag("GameData");
		if (data == null) {
			data = (GameObject) Instantiate(gameData, Vector3.zero, Quaternion.identity);
		}
		gameData gd = (gameData) data.GetComponent("gameData");
		//gd.loadExperimentSettings();
		gd.scriptedInstance = true;
		
		//See what the instance is that we've already completed.  
		maxComplete = 0;
		for (int i = 1; i <= numScriptedInstances; i++) {
			if(File.Exists("Response_" + i + ".txt")) {
					maxComplete = i;
			}
		}
		gd.instanceNum = maxComplete + 1;		
	}
		
	// Update is called once per frame
	void Update () {
	}
	
	void OnGUI() {
		//Style for save button, set up so that it scales with screen size	
		GUIStyle inputButton = new GUIStyle(GUI.skin.GetStyle("Button"));
		inputButton.fontSize = Screen.width/20;
		inputButton.alignment = TextAnchor.MiddleCenter;
		
		//Create length units that are dynamic depending on screen size
		int heightUnit = Screen.height / 16;
		int widthUnit = Screen.width / 16;
		
		//Title of the page
		GUI.Box(new Rect(3*widthUnit, 1*heightUnit, 10*widthUnit, 14*heightUnit), "Completing Scripted Instance");
		GUI.skin.box.fontSize = Screen.width/30;
		
		//Button to quit the game
		if(GUI.Button(new Rect(4*widthUnit, 4*heightUnit, 8*widthUnit, 4*heightUnit), "Quit Game", inputButton)) {
			Application.Quit();
		}
		
		//Button to load the next level
		GameObject data = GameObject.FindGameObjectWithTag("GameData");
		if (data != null) {
			gameData gd = (gameData) data.GetComponent("gameData");
			if(gd.instanceNum <= numScriptedInstances) {
				if(GUI.Button(new Rect(4*widthUnit, 10*heightUnit, 8*widthUnit, 4*heightUnit), "Complete Instance " + gd.instanceNum, inputButton)) {
					Application.LoadLevel(3);
				}
			} else {
				if(GUI.Button(new Rect(4*widthUnit, 10*heightUnit, 8*widthUnit, 4*heightUnit), "All Complete", inputButton)) {
						Application.Quit();
				}
			}
		} else {
			Debug.LogError("No game data object exists");
			Application.Quit();	
		}
		
	}
}
