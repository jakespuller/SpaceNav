using UnityEngine;
using System.Collections;
using System.IO;

public class Menu : MonoBehaviour
{
	
		private int maxComplete;
		public int numWarmupGames;
		public int numGamesToPlay;
		public GameObject gameData;
		
		// Use this for initialization
		void Start ()
		{
				//Create a new game data object in order to capture data across Unity levels
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				if (data == null) {
						data = (GameObject)Instantiate (gameData, Vector3.zero, Quaternion.identity);
						gameData gd = (gameData)data.GetComponent ("gameData");
						gd.gameLevel = 1;
				} 
				gameData g = (gameData)data.GetComponent ("gameData");
				g.scriptedInstance = false;

				//See what the instance is that we've already completed.  
				maxComplete = 0;
				for (int i = 1; i <= numGamesToPlay; i++) {
						if (File.Exists ("Level_" + i + "_Data.txt")) {
								if (i > numWarmupGames) {
										if (File.Exists ("ISA_feedback_" + i + ".txt") && File.Exists ("NASATLX_feedback_" + i + ".txt")) {
												maxComplete = i;
										} 
								} else {
									
										if (File.Exists ("HighScore.txt")) {
												string[] test = File.ReadAllLines ("HighScore.txt");	
												if (test.Length < i) {
														maxComplete = test.Length;
												} else {
														maxComplete = i;
												}
										} else {
												maxComplete = 0;
										}
								}
						}
				}
				g.gameLevel = maxComplete + 1;
				g.instanceNum = maxComplete + 1;
		}
		
		// Update is called once per frame
		void Update ()
		{
		}
	
		void OnGUI ()
		{
				//Style for save button, set up so that it scales with screen size	
				GUIStyle inputButton = new GUIStyle (GUI.skin.GetStyle ("Button"));
				inputButton.fontSize = Screen.width / 20;
				inputButton.alignment = TextAnchor.MiddleCenter;
		
				//Create length units that are dynamic depending on screen size
				int heightUnit = Screen.height / 16;
				int widthUnit = Screen.width / 16;
		
				//Title of the page
				GUI.Box (new Rect (3 * widthUnit, 1 * heightUnit, 10 * widthUnit, 14 * heightUnit), "Experiment Menu");
				GUI.skin.box.fontSize = Screen.width / 30;
		
				//Button to quit the game
				if (GUI.Button (new Rect (4 * widthUnit, 4 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Quit Game", inputButton)) {
						Application.Quit ();
				}
		
				//Button to load the next level
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				if (data != null) {
						gameData gd = (gameData)data.GetComponent ("gameData");
	
						if (numWarmupGames >= gd.gameLevel) {
								gd.automation_level = pathMarker.NONE; //***CHANGE THIS ONE TOO***
								if (GUI.Button (new Rect (4 * widthUnit, 10 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Start Warmup\nGame " + gd.gameLevel, inputButton)) {
										Application.LoadLevel (1);
								}
						} else {
								if (numGamesToPlay >= gd.gameLevel) {
										gd.automation_level = (gd.gameLevel + 2) % 4;
																				
										string level = "";
										switch (gd.automation_level) {
										case 0:
												level = "A";
												break;
										case 1:
												level = "B";
												break;
										case 2:
												level = "C";
												break;
										case 3:
												level = "D";
												break;
										}


										if (GUI.Button (new Rect (4 * widthUnit, 10 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Automation Setting " + level + " \nInstance " + Mathf.Floor ((gd.gameLevel - 2) / 4), inputButton)) {
												Application.LoadLevel (1);
										}
								} 
						}
						//
						//			else {
						//								if (GUI.Button (new Rect (4 * widthUnit, 10 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Go to Scripted\nInstances", inputButton)) {
//										Application.LoadLevel (2);
//								}
//						}
				} else {
						Debug.LogError ("No game data object exists");
						Application.Quit ();	
				}
		
		}
}
