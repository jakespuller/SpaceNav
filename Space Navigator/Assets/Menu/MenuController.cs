using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuController : MonoBehaviour {


	public GameObject gameData;
	public GameObject playerID;
	
	public EyeTrackerTCP eyetracker;
	
	private bool eyeTrackerOn;
	private bool practiceOn;
	private bool priorPlayerAccess;
	private bool baselineOn;
	private bool calibrationDone;
	private bool baselineDone;
	private bool newExperimentLoaded;
	private bool gamesLoaded;
	private bool gameLevelDisplayed;
	private bool automationOn;
	private bool practiceLevelDisplayed;
	private bool eyeTrackerTurnt;
	private bool playerIDDisplayed;
	
	private string levelName;
	private string uniqueID;
	private string playerName;
	
	private int practiceGameLimit;
	private int numPracticeGames;
	private int maxID;
	private int maxComplete;
	private int numGamesToPlay;
	private int gameLevel;
	private int practiceLevels;

	void Start() {
		//Create a new game data object in order to capture data across Unity levels
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		//Instantiate the game object data if the game is just starting
		if (data == null) 
		{
			data = (GameObject)Instantiate (gameData, Vector3.zero, Quaternion.identity);
		}
		gameData gd = (gameData)data.GetComponent ("gameData");
		//Initialize parameters
		gd.scriptedInstance = false; //Why is this still necessary?
		
		System.Int32.TryParse(generateUniqueID(), out maxID);
		levelName = Application.loadedLevelName;
		gamesLoaded = false;
		gameLevelDisplayed = false;
		practiceLevelDisplayed = false;
		playerIDDisplayed = false;
		eyeTrackerTurnt = false;
		/* Deletes files that may be left behind from prior instances of the game
		 that could interfere with gameplay as dictated by settings files */
		if (levelName == "MainMenu")
		{
			gd.filePath = "Data/";
			/* Creates directory/checks for data directory */
			if (!Directory.Exists(gd.filePath)) 
			{
				Directory.CreateDirectory(gd.filePath);
			}
			
			int i = 1;
			string levelFile = "current_level.txt";
			if (File.Exists (levelFile))
			{
				File.Delete (levelFile);
			}
		}
	}
	
	void Update() {
		///Create game data object for transfering relevant parameters to the menu
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		
		//Safeguards the sanctity of the data
		if (data != null) 
		{
			gameData gd = (gameData)data.GetComponent ("gameData");
			setParameters();
			//Initializes the game
			//Turns on and off buttons depending on the settings
			//Modifies gameLevel to the correct point depending on the level files
			if (levelName == "GameStart") 
			{
				gameStart(gd);
			} 
			else if (levelName == "MainMenu") 
			{
				mainMenu();
			} 
			else if (!gamesLoaded && levelName == "LoadExperiment") 
			{
				loadExperiment();
			}
		}
		else 
		{
			Debug.LogError ("No game data object exists");
			Application.Quit ();	
		}
	}

	//Jumps to other menus/game locations
	public void goto_new() {
		//Create participant directory 
		
		uniqueID = generateUniqueID();
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		if (data != null) 
		{
			gameData gd = (gameData)data.GetComponent ("gameData");
			gd.unique_id = uniqueID;
			gd.playerName = "Player"+gd.unique_id;
			gd.filePath += gd.playerName +"/";
			if (!Directory.Exists (gd.filePath)) {
				Directory.CreateDirectory(gd.filePath);
			}
			gd.currentLevelFile = gd.filePath + "current_level.txt";
			StreamWriter streamer = new StreamWriter(gd.currentLevelFile, true);
			streamer.WriteLine("Player ID:" + gd.unique_id);
			streamer.WriteLine ("Current Level:" + getLevel(gd.filePath, gd));
			streamer.Close();
			
			gd.newExperimentLoaded = true;
			Application.LoadLevel("GameStart");
		} 
		else 
		{
			Debug.LogError ("No game data object exists");
			Application.Quit ();
		}
	}
	
	public void goto_load() {
		Application.LoadLevel ("LoadExperiment");
	}
	
	public void goto_exit() {
		for (int i = gameLevel; i > 0; i--)
		{
			string filename = "Level_"+i+".txt";
			File.Delete (filename);
		}
		Application.Quit ();
	}
	
	public void goto_calibrate() {
		Application.LoadLevel ("Calibration");
	}
	
	public void goto_main_menu() {
		Application.LoadLevel ("MainMenu");
	}
	
	public void goto_scores() {
		Application.LoadLevel ("Scores");
	}
	
	public void goto_baseline() {
		Application.LoadLevel ("BaselineScene");
	}
	
	//Initialize the practice game and start
	public void practice_game() {
		//Start practice game
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		if (data != null)
		{
			gameData gd = (gameData)data.GetComponent ("gameData");
			//Application.LoadLevel("Level2");
			bool exists = true;
			practiceLevels = 1;
			while(exists)
			{
				string fileName = gd.filePath + "PracticeLevel_" + practiceLevels + "_Data.txt";
				gd.fileName = gd.filePath;
				gd.fileName = fileName;
				if (!File.Exists(fileName))
				{
					StreamWriter streamer = new StreamWriter(fileName, true);
					streamer.WriteLine ("practiceLevel: " + practiceLevels + "\n");
					if (practiceLevels == 1)
					{
						streamer.WriteLine ("uniqueID:" + uniqueID + "\n");
					}
					exists = false;
				} 
				else
				{
					practiceLevels += 1;
				}
			}
			gd.gameLevel = practiceLevels;
			gd.instanceNum = practiceLevels;
			gd.pram_list.practiceSession = 1;
			Application.LoadLevel ("Level2");
		}
		else
		{
			Debug.LogError ("No game data object exists");
			Application.Quit ();	
		}
	}
	
	public void load_scores() {
		Text scoreObject = GameObject.Find("Scores").GetComponent<Text>();
		if (File.Exists("HighScore.txt")) 
		{
			string[] lines = File.ReadAllLines("HighScore.txt");
			List<int> scores = new List<int>();
			//Convert each score to an int type
			foreach (string line in lines) 
			{
				scores.Add(Convert.ToInt32 (line));
			}
			scores.Sort();
			scores.Reverse();
			int count = 1;
			//Add the string to the score GameObject on the screen
			foreach (int score in scores) 
			{
				scoreObject.text += count.ToString() + ". ";
				scoreObject.text += score;
				scoreObject.text += "\n";
				count += 1;
			}
		} 
		else
		{
			scoreObject.text = "No Scores Exist.";
		}
	}

	//Calibrates the eye tracker
	public void calibrateET() {
		eyetracker.calibrate();
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		if (data != null) 
		{
			gameData gd = (gameData)data.GetComponent ("gameData");
			gd.calibrationDone = true;
		} 
		else 
		{
			Debug.LogError ("No game data object exists");
			Application.Quit ();	
		}
	}

	//Starts the experiment
	public void start_game() {
		//Write game level to game file
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		string fileName = "Level_" + gameLevel + ".txt";
		if (data != null) 
		{
			gameData gd = (gameData)data.GetComponent ("gameData");
			StreamWriter streamWCurrentLevel = new StreamWriter("current_level.txt");
			StreamReader streamRCurrentLevel = new StreamReader(gd.currentLevelFile);
			while(!streamRCurrentLevel.EndOfStream) 
			{
				streamWCurrentLevel.WriteLine(streamRCurrentLevel.ReadLine());
			}
			streamWCurrentLevel.Close();
			streamRCurrentLevel.Close();
			gd.fileName = gd.filePath;
			gd.fileName += "Level_" + gameLevel + "_Data.txt";
			if ((numPracticeGames-1)>practiceGameLimit) {
				gd.pram_list.practiceSession = 0;
			}
		}
		Application.LoadLevel("Level2");
	}

	string generateUniqueID()
	{
		int id = 1;
		string filePath = "Data/Player" + id.ToString ();
		while (Directory.Exists (filePath))
		{
			id++;
			filePath = "Data/Player" + id.ToString () +"/";
		}
		return id.ToString ();
	}
	
	private void displayGameInformation(string gameType, int maxComplete) {
		if (gameType == "Normal") {
			GameObject gameLevelDisplay = GameObject.Find("GameLevel");
			Text gameLevelDisplayText = gameLevelDisplay.GetComponent<Text>();
			gameLevelDisplayText.text = "Game's completed: " + (maxComplete) + "/" + numGamesToPlay;
			string automationText = "Automation: ON";
			if (!automationOn) {
				automationText = "Automation: OFF";
			}
			gameLevelDisplayText.text += "\n" + automationText;
		} else if (gameType == "Practice") {
			GameObject practiceLevelDisplay = GameObject.Find("PracticeLevel");
			Text practiceLevelDisplayText = practiceLevelDisplay.GetComponent<Text>();
			practiceLevelDisplayText.text = "Practice games completed: " + maxComplete + "/" + practiceGameLimit;
			practiceLevelDisplayText.text += "\n Automation: ON";
		}
	}
	
	private int getLevel(string levelPath, gameData gd) {
		string fileName = levelPath + "Level_1_Data.txt";
		int level = 1;
		while (File.Exists(fileName)) {
			level += 1;
			fileName = levelPath + "Level_"+level+"_Data.txt";
		}
		return level;
	}
	
	private void setParameters() {
		///Create game data object for transfering relevant parameters to the menu
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		
		//Safeguards the sanctity of the data
		if (data != null) 
		{
			gameData gd = (gameData)data.GetComponent ("gameData");
			//Modify local parameters from the "global" game data parameters
			numGamesToPlay = gd.pram_list.numOfExperimentSessions;
			practiceGameLimit = gd.pram_list.practiceGameLimit;
			calibrationDone = gd.calibrationDone;
			baselineDone = gd.baselineDone;
			newExperimentLoaded = gd.newExperimentLoaded;
			playerName = gd.playerName;
			if (gd.pram_list.practiceSession > 0) 
			{
				practiceOn = true;
			} 
			else 
			{
				practiceOn = false;
			}
			Debug.Log(practiceOn);
			if (gd.pram_list.priorPlayerAccess > 0) 
			{
				priorPlayerAccess = true;
			} 
			else 
			{
				priorPlayerAccess = false;
			}
			if (gd.pram_list.baselineOn > 0) 
			{
				baselineOn = true;
			} 
			else 
			{
				baselineOn = false;
			}
			if (gd.automation_level > 0) {
				automationOn = true;
			} else {
				automationOn = false;
			}
			if (!eyeTrackerTurnt) {
				if (gd.pram_list.eyeTrackerOn > 0 && !eyeTrackerTurnt) 
				{

					eyeTrackerOn = true;
					eyetracker = new EyeTrackerTCP();
					eyeTrackerTurnt = true;
				} else {
					eyeTrackerOn = false;
				}
			}
		}
	}
	
	private void mainMenu() {
		if ((!priorPlayerAccess && !newExperimentLoaded) || maxID <= 1) 
		{
			GameObject loadExperiment = GameObject.Find ("LoadExperiment");
			loadExperiment.GetComponent<Button>().interactable = false;
		}
	}
	
	private void loadExperiment() { 
		GameObject panel = GameObject.Find("Scrollview");
		for (int i = 1; i <= maxID-1; i++)
		{
			GameObject go = Instantiate(playerID, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
			go.transform.SetParent(panel.transform);
			float x_pos = go.transform.position.x;
			float z_pos = go.transform.position.z;
			//TODO: Auto-size given screen size
			go.transform.position = new Vector3(panel.transform.position.x, i*40+200, panel.transform.position.z);
			Text buttonText = go.GetComponentInChildren<Text>();
			buttonText.text = "";
			buttonText.text = "Player " + i.ToString();
			if (File.Exists (i+"Level_"+numGamesToPlay+"_Data.txt")) 
			{
				go.GetComponent<Button>().interactable = false;
			}
		}
		gamesLoaded = true;
	}
	
	private void gameStart(gameData gd) {
		GameObject practice = GameObject.Find ("Practice");
		GameObject startGame = GameObject.Find ("StartGame");
		GameObject baseline = GameObject.Find ("Baseline");
		GameObject calibration = GameObject.Find ("Calibrate");

		if (!calibrationDone && eyeTrackerOn) 
		{
			startGame.GetComponent<Button>().interactable = false;
			practice.GetComponent<Button>().interactable = false;
			baseline.GetComponent<Button>().interactable = false;
		} 
		else if (calibrationDone || !eyeTrackerOn) 
		{
			calibration.GetComponent<Button>().interactable = false;
		}
		
		if (baselineOn && !baselineDone && File.Exists ((maxID-1).ToString() + "Baseline.txt")) 
		{
			startGame.GetComponent<Button>().interactable = false;
			practice.GetComponent<Button>().interactable = false;
		} 
		else if (baselineDone || !baselineOn) 
		{
			baseline.GetComponent<Button>().interactable = false;
		}

		//TODO: Change to maxPracticeCompleted
		if (!practiceLevelDisplayed) 
		{
			int maxPracticeComplete = 0;
			string maxFileName = "";
			//Replace with read from current_level.txt
			for (int i = 1; i <= practiceGameLimit; i++) 
			{
				maxFileName = gd.filePath + "PracticeLevel_"+ i + "_Data.txt";
				if (File.Exists(maxFileName)) 
				{
					maxPracticeComplete = i;
				}
			}
			numPracticeGames = maxPracticeComplete;
			displayGameInformation("Practice", maxPracticeComplete);
			practiceLevelDisplayed = true;
		}
		Debug.Log(practiceOn);
		Debug.Log(numPracticeGames);
		Debug.Log(practiceGameLimit);
		if (!practiceOn || ((numPracticeGames) > practiceGameLimit)) 
		{
			practice.GetComponent<Button>().interactable = false;
		} 

		
		//DETERMINES AND DISPLAYS the current gameLevel
		if (!gameLevelDisplayed)
		{
			//See the instance that has already completed.  
			maxComplete = 0;
			string maxFileName = "";
			//Replace with read from current_level.txt
			for (int i = 1; i <= numGamesToPlay; i++) 
			{
				maxFileName = gd.filePath + "Level_"+i+"_Data.txt";
				if (File.Exists (maxFileName)) 
				{
					maxComplete = i;
				}
			}
			gd.gameLevel = maxComplete + 1;
			gameLevel = gd.gameLevel;
			gd.instanceNum = maxComplete + 1;
			//Turns the ability to start the game off if the player is beyond their set experimental capacity
			if (gd.gameLevel > numGamesToPlay) 
			{
				startGame.GetComponent<Button>().interactable = false;
			}
			displayGameInformation("Normal", maxComplete);
			gameLevelDisplayed = true;
		}

		if (!playerIDDisplayed) {
			GameObject playerIDDisplay = GameObject.Find("PlayerID");
			Text playerIDDisplayText = playerIDDisplay.GetComponent<Text>();
			playerIDDisplayText.text = playerName;
			playerIDDisplayed = true;
		}
	}
}