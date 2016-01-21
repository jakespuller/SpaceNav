using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System;
using System.Collections.Generic;

public class gameData : MonoBehaviour {
	public class Parameters
	{
		public int numOfTrainingSessions;
		public int numOfTrainingParameters;
		public Dictionary<string, float>[] experimentSessions;
		public Dictionary<string, float>[] trainingSessions;
		public int numOfExperimentSessions;
		public int numOfExperimentParameters;
		public int setup;
		public float numOfShips;
		public int eyeTrackerOn;
		public int borderControlSwitch;
		public int practiceSession;
		public int priorPlayerAccess;
		public int baselineOn;
		public int practiceGameLimit;
		public int numPracticeGames;
		public int trajectorySearchOn;
	}
	//Default variables to keep track of game data across different
	//Unity game levels
	public int gameLevel = 1;
	public string unique_id = "";
	public long highScore = 0;
	public ArrayList setups = new ArrayList();
 	public bool writtenYet = false;
	public Boolean scriptedInstance;
	public int instanceNum;
	public bool correctShip = true;
	public int nextShipIDNum = 1;
	public int automation_level = 0;
	public Parameters pram_list;
	public int maxGameLevel = System.Int32.MaxValue;
	public float screen_height, screen_width;
	public bool calibrationDone;
	public bool baselineDone;
	public bool newExperimentLoaded;
	public string fileName;
	public string currentLevelFile;
	public string filePath;
	public string playerName;
	//Screen points
	private Vector3 botLeftPoint;
	private Vector3 topRightPoint;
	public float maxX;
	public float maxY;
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this);
		botLeftPoint = Camera.main.ScreenToWorldPoint (new Vector3 (0, 0, 10));
		topRightPoint = Camera.main.ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height, 10));
		calibrationDone = false;
		baselineDone = false;
		newExperimentLoaded = false;
		playerName = "";
		maxX = topRightPoint.x;
		maxY = topRightPoint.y;
		//If file does not exist, use default values.
		if (File.Exists("experiment_Settings.xml"))
		{
			XmlDocument doc = new XmlDocument();
			doc.Load("experiment_Settings.xml");
			pram_list = new Parameters();
			//Looks for setup to see if there is data to process for the trials
			pram_list.setup = System.Convert.ToInt32(doc.DocumentElement.SelectSingleNode ("/Experiment_Settings/setup").InnerText);
			if (pram_list.setup == 1)
			{
				//Searches the XML doc for these specific parameters, which define the loop conditions.
				pram_list.numOfTrainingSessions = System.Convert.ToInt32(doc.DocumentElement.SelectSingleNode("/Experiment_Settings/trainingSessionAmount").InnerText);
				pram_list.numOfExperimentSessions = System.Convert.ToInt32 (doc.DocumentElement.SelectSingleNode("/Experiment_Settings/experimentSessionAmount").InnerText);
				pram_list.numOfTrainingParameters = System.Convert.ToInt32 (doc.DocumentElement.SelectSingleNode("/Experiment_Settings/numTrainingParameters").InnerText);
				pram_list.numOfExperimentParameters = System.Convert.ToInt32 (doc.DocumentElement.SelectSingleNode ("/Experiment_Settings/numExperimentParameters").InnerText);
				pram_list.eyeTrackerOn = System.Convert.ToInt32 (doc.DocumentElement.SelectSingleNode ("/Experiment_Settings/eyeTrackerOn").InnerText);
				pram_list.borderControlSwitch = System.Convert.ToInt32 (doc.DocumentElement.SelectSingleNode ("/Experiment_Settings/bordercontrolswitch").InnerText);
				pram_list.practiceSession = System.Convert.ToInt32 (doc.DocumentElement.SelectSingleNode ("/Experiment_Settings/practiceSession").InnerText); 
				pram_list.priorPlayerAccess = System.Convert.ToInt32 (doc.DocumentElement.SelectSingleNode ("/Experiment_Settings/loadPriorPlayers").InnerText);
				pram_list.baselineOn = System.Convert.ToInt32 (doc.DocumentElement.SelectSingleNode ("/Experiment_Settings/baselineOn").InnerText);
				pram_list.trajectorySearchOn = System.Convert.ToInt32(doc.DocumentElement.SelectSingleNode("/Experiment_Settings/trajectorySearchOn").InnerText);
				try {
					pram_list.practiceGameLimit = System.Convert.ToInt32 (doc.DocumentElement.SelectSingleNode("/Experiment_Settings/practiceGameLimit").InnerText);
				} catch (System.NullReferenceException) {
					pram_list.practiceGameLimit = System.Int32.MaxValue;
				}
				//Used for navigating the XML document within for loops of training and experiment
				XmlNode trainingX = doc.DocumentElement.SelectSingleNode("/Experiment_Settings/trainingSession");
				XmlNode experimentX = doc.DocumentElement.SelectSingleNode ("/Experiment_Settings/experimentSession");
				
				//Instantiates the array of dictionaries for the session settings 
				pram_list.trainingSessions = new Dictionary<string, float>[pram_list.numOfTrainingSessions];
				pram_list.experimentSessions = new Dictionary<string, float>[pram_list.numOfExperimentSessions];
				//Iterates through the XML for varous parameters in training node
				// O(i*j) where i = the #ofsessions and j = #ofparameters
				if (trainingX != null && trainingX.FirstChild != null)
				{
					trainingX = trainingX.FirstChild;
				}
				for ( int i = 0; i < pram_list.numOfTrainingSessions; i++)
				{
					//Creates a new dictionary for a specific trial in the array index
					pram_list.trainingSessions[i] = new Dictionary<string, float>();
					String numParams_S = trainingX.Attributes.Remove(trainingX.Attributes["parameters"]).Value;
					int numParams_i = System.Convert.ToInt32 (numParams_S);
					trainingX = trainingX.FirstChild;
					for (int j = 0; j < numParams_i ; j++)
					{
						pram_list.trainingSessions[i].Add(trainingX.Name, float.Parse(trainingX.InnerText));
						//prevents null exceptions
						if (trainingX.NextSibling != null)
						{
							trainingX = trainingX.NextSibling;
						}
					}
					trainingX = trainingX.ParentNode;
					pram_list.trainingSessions[i].Add ("numPrams",(float) numParams_i);
					//prevents null exceptions
					if (trainingX.NextSibling != null)
					{
						trainingX = trainingX.NextSibling;
					}
				}
				//Iterates through the XML for the various parameters in experiment node
				experimentX = experimentX.FirstChild;
				for (int i = 0; i < pram_list.numOfExperimentSessions; i++)
				{
					pram_list.experimentSessions[i] = new Dictionary<string, float>();
					String numParams_S = experimentX.Attributes.Remove(experimentX.Attributes["parameters"]).Value;
					int numParams_i = System.Convert.ToInt32 (numParams_S);
					experimentX = experimentX.FirstChild;
					for (int j = 0; j < numParams_i; j++)
					{
						pram_list.experimentSessions[i].Add(experimentX.Name, float.Parse(experimentX.InnerText));
						if (experimentX.NextSibling != null)
						{
							experimentX = experimentX.NextSibling;
						}
					}
					experimentX = experimentX.ParentNode;
					pram_list.experimentSessions[i].Add ("numPrams",(float) numParams_i);
					
					if (experimentX.NextSibling != null)
					{
						experimentX = experimentX.NextSibling;
					}
				}
				int experiment_level = gameLevel - pram_list.numOfTrainingSessions;
				float temp;
				pram_list.experimentSessions [experiment_level - 1].TryGetValue ("automation", out temp);
				automation_level = (int) temp;
			}
		}

		if (File.Exists ("HighScore.txt")) {
			string[] test = File.ReadAllLines ("HighScore.txt");	

			for (int j = 0; j < test.Length; j++) {
				int x = 0;
				
				int.TryParse (test [j], out x);
				if (x > highScore)
					highScore = x;
			}
		}

	}
	
	//Set the game settings for the different difficulty setting levels
	public void setGameSettings(int setup) {
		GameObject control = GameObject.FindGameObjectWithTag("GameController");
		spawnSystem ss = (spawnSystem) control.GetComponent("spawnSystem");
		try
		{
			if (pram_list.setup == 1) 
			{
				setup = 0;
			}
		} catch (System.NullReferenceException) {}
		//Based upon setup parameters, will determine the different spawn rates 
		if (setup == 0)
		{
			float temp = 0;
			if (gameLevel <= pram_list.numOfTrainingSessions) {
				pram_list.trainingSessions [gameLevel - 1].TryGetValue ("shipSpawnRate", out temp);
				ss.shipSpawnRate = (int) temp;
				pram_list.trainingSessions [gameLevel - 1].TryGetValue ("numNFZ", out temp);
				ss.numNFZs = (int) temp;
				pram_list.trainingSessions [gameLevel - 1].TryGetValue ("NFZchangefreq", out temp);
				ss.noFlyMoveRate = (int) temp;
				pram_list.trainingSessions [gameLevel - 1].TryGetValue ("bonusSpawnRate", out temp);
				ss.bonusSpawnRate = (int) temp;
			} else if (gameLevel <= (pram_list.numOfExperimentSessions + pram_list.numOfTrainingSessions)) {
				int experiment_level = gameLevel - pram_list.numOfTrainingSessions;
				pram_list.experimentSessions [experiment_level - 1].TryGetValue ("shipSpawnRate", out temp);
				ss.shipSpawnRate = (int) temp;
				pram_list.experimentSessions[experiment_level - 1].TryGetValue ("numNFZ", out temp);
				ss.numNFZs = (int) temp;
				pram_list.experimentSessions [experiment_level - 1].TryGetValue ("NFZchangefreq", out temp);
				ss.noFlyMoveRate = (int) temp;
				pram_list.experimentSessions [experiment_level - 1].TryGetValue ("bonusSpawnRate", out temp);
				ss.bonusSpawnRate = (int) temp;
				pram_list.experimentSessions [experiment_level - 1].TryGetValue ("automation", out temp);
				automation_level = (int) temp;			
			}
			maxGameLevel = pram_list.numOfExperimentSessions + pram_list.numOfTrainingSessions;
		}
	}
}
