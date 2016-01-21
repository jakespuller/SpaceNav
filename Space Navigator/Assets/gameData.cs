using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class gameData : MonoBehaviour {
	
	//Default variables to keep track of game data across different
	//Unity game levels
	public int gameLevel = 1;
	public long highScore = 0;
	public ArrayList setups = new ArrayList();
 	public bool writtenYet = false;
	public Boolean scriptedInstance;
	public int instanceNum;
	public bool correctShip = true;
	public int nextShipIDNum = 1;
	public int automation_level = 0;
	
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this);
		
//		string fileName = "HighScore.txt";
//		if(File.Exists(fileName)) {
//			System.IO.StreamReader file = new System.IO.StreamReader(fileName);
//			highScore = Convert.ToInt32(file.ReadLine());
//		}
//		int highscore = 0;
		
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
	
	// Update is called once per frame
	void Update () {
	}
	
	//Set the game settings for the different difficulty setting levels
	public void setGameSettings(int setup) {
		GameObject control = GameObject.FindGameObjectWithTag("GameController");
		spawnSystem ss = (spawnSystem) control.GetComponent("spawnSystem");
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
