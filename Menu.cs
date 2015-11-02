using UnityEngine;
using System.Collections;
using System.IO;

public class Menu : MonoBehaviour
{
	
		private int maxComplete;
		public int numWarmupGames;
		public int numGamesToPlay;
		public GameObject gameData;
		private string uniqueID;
		private int gameLevel;
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
				gd.gameLevel = maxComplete + 1;
				gameLevel = gd.gameLevel;
				gd.instanceNum = maxComplete + 1;
				uniqueID = generateUniqueID();
				DirectoryInfo di = new DirectoryInfo("Data");
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
		
				//Button to quit the game
				if (GUI.Button (new Rect (4 * widthUnit, 10 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Quit Game", inputButton))
				{
						Application.Quit ();
				}
		
				//Button to load the next level
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				if (data != null) {
						gameData gd = (gameData)data.GetComponent ("gameData");
						//Look for training session parameter in xml file
						try
						{
							numWarmupGames = (int) gd.pram_list.numOfTrainingSessions;
						} catch (System.NullReferenceException ex){}
						//If it's the first game
						if (gd.gameLevel == 1)
						{	
							Debug.Log ("before method call");
							Debug.Log (isDelaySame (gd, gd.gameLevel));
							gd.automation_level = pathMarker.NONE;
							if (GUI.Button(new Rect (4 * widthUnit, 5 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Start Game " + gd.gameLevel +"\n Delay: " + isDelaySame(gd, 1), inputButton))
							{
								gd.unique_id = uniqueID;
								LoadLevel (1);
							}
						} else if (numWarmupGames >= gd.gameLevel) 
						{
							gd.automation_level = pathMarker.NONE; //***CHANGE THIS ONE TOO***
							Debug.Log ("At menu button");
							if (GUI.Button(new Rect (4 * widthUnit, 4 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Start Game " + gd.gameLevel + "\n Delay: " + isDelaySame(gd, gd.gameLevel), inputButton)) 
							{
								gd.unique_id = read_id ();
								LoadLevel (1);
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
									float exp_auto = 5.0f;
									int experiment_level = 0;
									try
									{
										experiment_level = gd.gameLevel - gd.pram_list.numOfTrainingSessions;
										gd.pram_list.experimentSessions[experiment_level-1].TryGetValue ("automation", out exp_auto);
										gd.automation_level = (int) exp_auto;
									}
									catch (System.NullReferenceException){}
									if (exp_auto >= 4)
									{
										gd.automation_level = (gd.gameLevel + 2) % 4;
									}					
									string level ="";
									switch (gd.automation_level) 
									{
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
										case 4:
												level = "E";
												break;
									}
									if (gd.gameLevel == 1)
									{
										if (GUI.Button(new Rect (4 * widthUnit, 5 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Start Game " + gd.gameLevel + "\n Automation: " + level + "\n Delay:" + isDelaySame(gd, experiment_level), inputButton))
										{
										gd.unique_id = uniqueID;
										LoadLevel (1);
										}
									}
									if (GUI.Button (new Rect (4 * widthUnit, 4 * heightUnit, 8 * widthUnit, 5 * heightUnit), "Automation Setting " + level + " \nInstance " + Mathf.Floor ((gd.gameLevel - 2) / 4) + "" +
										"\n Delay: " + delay_str, inputButton)) 
									{
										gd.unique_id = read_id();
										LoadLevel (1);
									}
							} 
							else
							{
								//Change to reset levels
								if (GUI.Button (new Rect (4 * widthUnit, 4 * heightUnit, 8 * widthUnit, 4 * heightUnit), "Reset & Quit\n", inputButton)) 
								{
									quitAndRestart();
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
			return chunk[1];
		}
		string generateUniqueID()
		{
			float id = Random.Range(0, 10000000);
			if (File.Exists (id.ToString() + "Level_1_Data.txt"))
			{
				id = Random.Range (0, 10000000);
				while (File.Exists (id.ToString() + "Level_1_Data.txt"))
				{
					id = Random.Range (0, 10000000);
				}
			}
			return "id" + id;
		}
		string isDelaySame(gameData gd, int level) {
			string delay_str = " ";
			gd.automation_level = pathMarker.NONE;
			float lowDelay = 0.0f;
			float midDelay = 0.0f;
			float highDelay = 0.0f;
			Debug.Log ("before pram search");
			gd.pram_list.experimentSessions [level - 1].TryGetValue("lowDelayAA", out lowDelay);
			gd.pram_list.experimentSessions [level - 1].TryGetValue("medDelayAA", out midDelay);
			gd.pram_list.experimentSessions [level - 1].TryGetValue("highDelayAA", out highDelay);
			if (lowDelay == midDelay && midDelay == highDelay) {
				delay_str = lowDelay.ToString();								
			} else {
				delay_str = "non-uniform";
			}
		Debug.Log ("Return " + delay_str);
		return delay_str;
		}
}
