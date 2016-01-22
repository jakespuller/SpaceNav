using UnityEngine;
using System.Collections;
using System.IO;

public class Menu : MonoBehaviour
{
	
		private int maxComplete;
		private int practiceLevels;
		public int numWarmupGames;
		public int numGamesToPlay;
		public GameObject gameData;
		private string uniqueID;
		private int gameLevel;
		public float screen_height, screen_width;
		private bool init = false, not_pressed = true;
		
		public EyeTrackerTCP eyetracker;
		// Use this for initialization
		void Start ()
		{
				//Create a new game data object in order to capture data across Unity levels
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				gameData gd;
				//Instantiate the game object data if the game is just starting
				if (data == null) {
						data = (GameObject)Instantiate (gameData, Vector3.zero, Quaternion.identity);
				} 
				gd = (gameData)data.GetComponent ("gameData");
				gd.scriptedInstance = false;
				//See what the instance is that we've already completed.  
				maxComplete = 0;
				for (int i = 1; i <= numGamesToPlay; i++) {
						if (File.Exists ("Level_" + i + ".txt")) {
							maxComplete = i;
							}
				}
				Debug.Log(gd.pram_list.practiceSession); 
				gd.gameLevel = maxComplete + 1;
				gameLevel = gd.gameLevel;
				gd.instanceNum = maxComplete + 1;
				uniqueID = generateUniqueID();
		}
		//Turns on the GUI
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
				GUI.Box (new Rect (3 * widthUnit, 1 * heightUnit, 10 * widthUnit, 14 * heightUnit), "Main Menu");
				GUI.skin.box.fontSize = Screen.width / 30;
				//Button to load the next level
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				if (data != null) {
						gameData gd = (gameData)data.GetComponent ("gameData");
						//Look for training session parameter in xml file
						try
						{
							numWarmupGames = gd.pram_list.numOfTrainingSessions;
						} catch (System.NullReferenceException){}
						if (numWarmupGames >= gd.gameLevel) 
						{
							if (gd.gameLevel == 1)
							{
								gd.automation_level = pathMarker.NONE;
								uniqueID = generateUniqueID();
								bool button = GUI.Button(new Rect (4 * widthUnit, 5 * heightUnit, 10 * widthUnit, 4 * heightUnit), "Start Game " + gd.gameLevel + "\n NO AUTOMATION", inputButton);
								if (uniqueID != "Enter Name." && button)
								{
									gd.unique_id = uniqueID;
									LoadLevel (1);
								}
							} else
							{
								if (GUI.Button(new Rect (4 * widthUnit, 4 * heightUnit, 10 * widthUnit, 4 * heightUnit), "Start Game " + gd.gameLevel + "\n NO AUTOMATION", inputButton)) 
								{
									gd.unique_id = read_id ();
									LoadLevel (1);
								}
							}
						} 
						else 
						{
							//Look for experiment and training session numbers in xml file
							try
							{
								numGamesToPlay = (int) gd.pram_list.numOfTrainingSessions + (int) gd.pram_list.numOfExperimentSessions;
							}
							catch (System.NullReferenceException){}
							//This is false when the game is on completion
							if (numGamesToPlay >= gd.gameLevel) 
							{
									float exp_auto = 6.0f;
									int experiment_level = 0;
									try
									{
										experiment_level = gd.gameLevel - gd.pram_list.numOfTrainingSessions;
										gd.pram_list.experimentSessions[experiment_level-1].TryGetValue ("automation", out exp_auto);
										gd.automation_level = (int) exp_auto;
									}
									catch (System.NullReferenceException){}
									if (exp_auto >= 6)
									{
										gd.automation_level = (gd.gameLevel + 2) % 4;
									}
									string name = "";
									switch (gd.automation_level) 
									{
										case 0:
											//No automation
											name = "AUTOMATION OFF";
											break;
										case 1:
											//Straight-Line
											name = "AUTOMATION ON";
											break;
										case 2:
											//Eye-tracking
											name = "AUTOMATION ON";
											break;
										}
										if (gd.gameLevel == 1)
									{
										gd.automation_level = pathMarker.NONE;
										GUI.Box (new Rect ((float)5.5 * widthUnit, 3 * heightUnit, (float) 5 * widthUnit, (float) 1.5 * heightUnit), "Difficulty will vary");
										bool button = GUI.Button(new Rect (4 * widthUnit, 5 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Start Game " + gd.gameLevel + "\n" + name, inputButton);
										if (button)
										{
											gd.unique_id = uniqueID;
											gd.screen_width = screen_width;
											gd.screen_height = screen_height;
											LoadLevel (1);
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
					else
					{
						if (GUI.Button (new Rect (4 * widthUnit, 4 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Start Game " + gd.gameLevel + "\n" + name, inputButton)) 
										{
											gd.unique_id = read_id();
											LoadLevel (1);
										}
						bool button2 = GUI.Button(new Rect (4 * widthUnit, 10 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Quit Game", inputButton);
						if (button2)
						{
							Application.Quit ();
						}
					}
				} 
				else
							{
								//Change to reset levels
								if (GUI.Button (new Rect (4 * widthUnit, 4 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Reset & Quit\n", inputButton)) 
								{
									quitAndRestart();
								}
								bool button2 = GUI.Button(new Rect (4 * widthUnit, 10 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Baseline", inputButton);
								if (button2)
								{
									gd.unique_id = uniqueID;
									Application.LoadLevel(3);
								}
							}
						}
				} else {
						Debug.LogError ("No game data object exists");
						Application.Quit ();	
				}
		
		}
		
		void LoadLevel(int level)
		{	
			//Write game level to game file
			string fileName = "Level_" + gameLevel + ".txt";
			StreamWriter streamer = new StreamWriter(fileName, true);
			streamer.WriteLine ("gameLevel: " + gameLevel + "\n");
			if (gameLevel == 1)
			{
				streamer.WriteLine ("uniqueID:" + uniqueID + "\n");
			}
			streamer.Close ();
			Application.LoadLevel(level);
		}
		
		void quitAndRestart()
		{
			for (int i = gameLevel; i > 0; i--)
			{
				string filename = "Level_"+i+".txt";
				File.Delete (filename);
			}
			Application.Quit ();
		}
		
		string read_id()
		{
			string fileName = "Level_1.txt";
			//Debug.Log ("Loading: " + fileName);
			string line;
			System.IO.StreamReader file = new System.IO.StreamReader (fileName);
			//Reads past game level
			line = file.ReadLine();
			//Reads past new line
			line = file.ReadLine();
			//Reads unique_id
			line = file.ReadLine();
			string[] chunk = line.Split (new char[]{':'});
			string id = chunk[1];
			return id;
		}
		string generateUniqueID()
		{
		int id = 1;
		if (File.Exists (id.ToString() + "Level_1_Data.txt"))
		{
			id = 2;
			while (File.Exists (id.ToString() + "Level_1_Data.txt"))
			{
				id++;
			}
		}
		return id.ToString ();
		}
}
