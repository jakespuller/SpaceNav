using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//This class keeps track of the game time and acts on different time entities
public class TimerController : MonoBehaviour
{
	
		//Initialize variables for later
		public float timeLeft;
		private bool popup = false;
		private ScoreController sc;
		private float startTime;
		private float timeLength;

		//Initialize the game time left counter
		void Start ()
		{
				startTime = Time.time;
				timeLength = timeLeft;
		}
	
		// Update is called once per frame
		void Update ()
		{
				//Count the time down
				timeLeft = timeLength - (Time.time - startTime);
		
				//Gather objects needed to check on high scores
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				gameData gd = (gameData)data.GetComponent ("gameData");
				sc = (ScoreController)GameObject.FindGameObjectWithTag ("ScoreSystem").GetComponent ("ScoreController");
		
				//Check to see if we are out of time
				if (timeLeft <= 0.0f) {
						

					
						//If we're out of time let them know, and pause so they can check score before loading the next screen
						GetComponent<GUIText>().text = "You ran out of time";
						//if (sc.score > gd.highScore) {
//								gd.highScore = sc.score;
//								using (StreamWriter streamer = new StreamWriter("HighScore.txt", gd.writtenYet)) {
//										streamer.WriteLine (gd.highScore);
//										streamer.Close ();
//								}
						//}	

						//Load the next level depending on if it is a scripted instance or regular game
						if (popup == false) {
								if (!gd.scriptedInstance) {
//										//Create a holder file to know we finished this level
//										string file_name = "L" + gd.gameLevel + ".txt";
//										using (StreamWriter streamer = new StreamWriter(file_name, gd.writtenYet)) {
//												streamer.WriteLine ("Done");
//												streamer.Close ();
//										}

										//Go back to the main menu
										if (gd.gameLevel <= 5) {
												StartCoroutine (pauseAndLoadNewLevel (3, 0));
										} else {
												StartCoroutine (pauseAndLoadNewLevel (3, 2));

										}
								} else {
										//Go back to the scripted menu
										StartCoroutine (pauseAndLoadNewLevel (3, 2));
								}
						}
						popup = true;

				} else {
						if (gd.gameLevel <= 5) {
								//if we haven't run out of time yet, update the time on the screen
								if (timeLeft % 60 < 10) {
										GetComponent<GUIText>().text = "High Score: " + gd.highScore + "\nInstance: " + gd.instanceNum + "\nTime: " + Math.Floor (timeLeft / 60) + ":0" + (int)timeLeft % 60;
								} else {
										GetComponent<GUIText>().text = "High Score: " + gd.highScore + "\nInstance: " + gd.instanceNum + "\nTime: " + Math.Floor (timeLeft / 60) + ":" + (int)timeLeft % 60;
								}
						} else {

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
				
								//if we haven't run out of time yet, update the time on the screen
								if (timeLeft % 60 < 10) {
										GetComponent<GUIText>().text = "High Score: " + gd.highScore + "\nAutomation Setting: " + level + "\nTime: " + Math.Floor (timeLeft / 60) + ":0" + (int)timeLeft % 60;
								} else {
										GetComponent<GUIText>().text = "High Score: " + gd.highScore + "\nAutomation Setting: " + level + "\nTime: " + Math.Floor (timeLeft / 60) + ":" + (int)timeLeft % 60;
								}
						}
				}
		}
	
		void OnGUI ()
		{

				//Gather objects needed to check on high scores
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				gameData gd = (gameData)data.GetComponent ("gameData");

				//Style for left side slider value indicator
				GUIStyle savingStyle = new GUIStyle (GUI.skin.GetStyle ("Label"));
				savingStyle.fontSize = Screen.height / 10;
				savingStyle.alignment = TextAnchor.MiddleCenter;

				//Check to see if the person acted on the correct ship, if so move on.  If not, we send them back to the same level again.
				int heightUnit = Screen.height / 4;
				int widthUnit = Screen.width / 4;
				if (popup) {
						if (gd.scriptedInstance)
								GUI.Label (new Rect (widthUnit, heightUnit, 2 * widthUnit, 2 * heightUnit), "No response given.  Please draw a trajectory for the blinking ship", savingStyle);
						else
								GUI.Label (new Rect (widthUnit, heightUnit, 2 * widthUnit, 2 * heightUnit), "Saving Game Data\nScore = " + sc.score, savingStyle);
				}

		}
	
		//Use a yield/corouting method to wait a few seconds before loading the nex level
		public IEnumerator pauseAndLoadNewLevel (float numSecs, int level)
		{
				Time.timeScale = 0.0f;
				float pauseEndTime = Time.realtimeSinceStartup + numSecs;

			//Gather objects needed to check on high scores
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				gameData gd = (gameData)data.GetComponent ("gameData");
				using (StreamWriter streamer = new StreamWriter("HighScore.txt", gd.writtenYet)) {
			streamer.WriteLine (sc.score);
			streamer.Close ();
		}

		while (Time.realtimeSinceStartup < pauseEndTime) {
						yield return 0;
				}
				Time.timeScale = 1;
				StartCoroutine (loadLevel (level));
		}
	
		//Load the game to the indicated level
		IEnumerator loadLevel (int level)
		{
				yield return 0;

				//Send a round end string to the server so we know to save the player model, etc.
				GameObject control = GameObject.FindGameObjectWithTag ("GameController");
				spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
				ss.UDPConnection.sendString ("end");

				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				gameData gd = (gameData)data.GetComponent ("gameData");
				if (gd.scriptedInstance) {
						if (gd.correctShip) {
								gd.instanceNum++;
						} else {
								gd.correctShip = true;
						}
			
				} else {
						gd.gameLevel++;
				}
				Application.LoadLevel (level);
		}
}
