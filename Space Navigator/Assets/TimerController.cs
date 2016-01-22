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
		private string systemStartTime;
		private bool written;
		private bool scoreWritten;
		//Initialize the game time left counter
		void Start ()
		{
			scoreWritten = false;
			startTime = Time.time;
			systemStartTime =  System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond;
			timeLength = timeLeft;
			//The following chunk of code uses input values from the XML file to set the time for the specified game.
			GameObject data = GameObject.FindGameObjectWithTag ("GameData");
			gameData gd = (gameData)data.GetComponent ("gameData");
			sc = (ScoreController)GameObject.FindGameObjectWithTag ("ScoreSystem").GetComponent ("ScoreController");
			try
			{
			if (gd.pram_list.practiceSession != 1)
			{
				if (gd.gameLevel <= gd.pram_list.numOfTrainingSessions) {
					float temp = 0.0f;
					gd.pram_list.trainingSessions [gd.gameLevel - 1].TryGetValue ("length", out temp);
					temp = temp * 60;
					timeLength = (float)temp;
					gd.pram_list.numOfShips = 0f;
				} else if (gd.gameLevel <= (gd.pram_list.numOfExperimentSessions + gd.pram_list.numOfTrainingSessions)) {
					float temp = 0.0f;
					int experiment_level = gd.gameLevel - gd.pram_list.numOfTrainingSessions;
					gd.pram_list.experimentSessions[experiment_level - 1].TryGetValue ("length", out temp);
					temp = temp * 60;
					timeLength = (float)temp;
					gd.pram_list.numOfShips = 0f;
				}
			} else {
				timeLength = 60;
			}
			} catch (System.NullReferenceException){}
			written = false;
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
				int warmup = 5;
				try {
					warmup = gd.pram_list.numOfTrainingSessions;
				} catch (System.NullReferenceException) { warmup = 5; }
				//Check to see if we are out of time
				if (timeLeft <= 0.0f) {
						//If we're out of time let them know, and pause so they can check score before loading the next screen
						guiText.text = "You ran out of time";
						GameObject control = GameObject.FindGameObjectWithTag ("GameController");
						spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
						if (ss.eyeTrackerOn > 0) {
							ss.TCPconnection.closeStream();
						}
						//System Time Represented in Seconds
						if (!written)
						{
							string systemEndTime = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond;
							string gameTimeInfo = gd.gameLevel+","+systemStartTime+","+systemEndTime+","+sc.score+",";
							File.AppendAllText (gd.filePath+"gameLevelInfo.txt", gameTimeInfo);
							written = true;
							gd.writtenYet = true;
						}
						//Load the next level depending on if it is a scripted instance or regular game
						if (popup == false) {
										//Go back to the scripted menu
										if (gd.pram_list.practiceSession == 1)
										{
											gd.pram_list.numPracticeGames += 1;
											StartCoroutine (pauseAndLoadNewLevel(3, "GameStart"));
										} else {
											StartCoroutine (pauseAndLoadNewLevel (3, "QuestionScene"));
										}
						}
						popup = true;

				} else {
						if (gd.gameLevel <= warmup) {
								//if we haven't run out of time yet, update the time on the screen
								if (timeLeft % 60 < 10) {
										guiText.text = "High Score: " + gd.highScore + "\nInstance: " + gd.instanceNum + "\nTime: " + Math.Floor (timeLeft / 60) + ":0" + (int)timeLeft % 60;
								} else {
										guiText.text = "High Score: " + gd.highScore + "\nInstance: " + gd.instanceNum + "\nTime: " + Math.Floor (timeLeft / 60) + ":" + (int)timeLeft % 60;
								}
						} else {

								string level = "";
								switch (gd.automation_level) {
								case 0:
										level = "OFF";
										break;
								case 1:
										level = "ON";
										break;
								case 2:
										level = "ON";
										break;
								}
				
								//if we haven't run out of time yet, update the time on the screen
								if (timeLeft % 60 < 10) {
										guiText.text = "High Score: " + gd.highScore + "\nAutomation: " + level + "\nTime: " + Math.Floor (timeLeft / 60) + ":0" + (int)timeLeft % 60;
								} else {
										guiText.text = "High Score: " + gd.highScore + "\nAutomation: " + level + "\nTime: " + Math.Floor (timeLeft / 60) + ":" + (int)timeLeft % 60;
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
		public IEnumerator pauseAndLoadNewLevel (float numSecs, string level)
		{
				Time.timeScale = 0.0f;
				float pauseEndTime = Time.realtimeSinceStartup + numSecs;
				//Gather objects needed to check on high scores
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				gameData gd = (gameData)data.GetComponent ("gameData");
				if (!scoreWritten) {
					using (StreamWriter streamer = new StreamWriter("HighScore.txt", gd.writtenYet)) {
					streamer.WriteLine (sc.score);
					streamer.Close ();
					scoreWritten = true;
				}
		}

		while (Time.realtimeSinceStartup < pauseEndTime) {
						yield return 0;
				}
				Time.timeScale = 1;
				StartCoroutine (loadLevel (level));
		}
	
		//Load the game to the indicated level
		IEnumerator loadLevel (string level)
		{
				yield return 0;
				//Send a round end string to the server so we know to save the player model, etc.
				GameObject control = GameObject.FindGameObjectWithTag ("GameController");
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				gameData gd = (gameData)data.GetComponent ("gameData");
				spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
				if (gd.pram_list.eyeTrackerOn > 0) {
					ss.TCPconnection.closeStream();
				}
				gd.gameLevel++;
				Application.LoadLevel (level);
		}
}
