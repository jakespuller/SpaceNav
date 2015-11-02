﻿using UnityEngine;
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
	public int maxGameLevel = 17;
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this);
		
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
				//Used for navigating the XML document within for loops of training and experiment
				XmlNode trainingX = doc.DocumentElement.SelectSingleNode("/Experiment_Settings/trainingSession");
				XmlNode experimentX = doc.DocumentElement.SelectSingleNode ("/Experiment_Settings/experimentSession");
				
				//Instantiates the array of dictionaries for the session settings 
				pram_list.trainingSessions = new Dictionary<string, float>[pram_list.numOfTrainingSessions];
				pram_list.experimentSessions = new Dictionary<string, float>[pram_list.numOfExperimentSessions];
				//Iterates through the XML for varous parameters in training node
				// O(i*j) where i = the #ofsessions and j = #ofparameters
				trainingX = trainingX.FirstChild;
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
		if(setup == 1) {
			ss.shipSpawnRate = 5;
			ss.numNFZs = 2;
			ss.noFlyMoveRate = 15;
			ss.bonusSpawnRate = 10;
		}
		if(setup == 2) {
			ss.shipSpawnRate = 2;
			ss.numNFZs = 2;
			ss.noFlyMoveRate = 15;
			ss.bonusSpawnRate = 10;
		}
		if(setup == 3) {
			ss.shipSpawnRate = 5;
			ss.numNFZs = 4;
			ss.noFlyMoveRate = 15;
			ss.bonusSpawnRate = 10;
		}
		if(setup == 4) {
			ss.shipSpawnRate = 2;
			ss.numNFZs = 4;
			ss.noFlyMoveRate = 15;
			ss.bonusSpawnRate = 10;
		}
	}
}
