using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;

public class spawnSystem : MonoBehaviour
{
	
		public ArrayList systems;
		public ArrayList shipPrefabs;
		public ArrayList moveHelperPrefabs;
	
		public int automate_level;
	
		public GameObject marker;
		public GameObject dot1;
		public GameObject dot2;
		public GameObject dot3;

		public GameObject system1;
		public GameObject system2;
		public GameObject system3;
		public GameObject system4;
	
		public GameObject ship1;
		public GameObject ship2;
		public GameObject ship3;
		public GameObject ship4;
	
		public GameObject moveHelper1;
		public GameObject moveHelper2;
		public GameObject moveHelper3;
		public GameObject moveHelper4;
	
		public GameObject bonus;


		public GameObject noFlyZone;
		public int numNFZs; 
		private GameObject[] noFlyZones;
		private GameObject noFlyZone1;
		private GameObject noFlyZone2;
		public GameObject gameData;

		//Screen points
		private Vector3 botLeftPoint;
		private Vector3 topRightPoint;
		private int minX;
		private int minY;
		private int maxX;
		private int maxY;

		//Default spawn rates
		private int nextShipSpawnTime = 0;
		public int shipSpawnRate = 5;
		private int nextNoFlySpawnTime = 0;
		public int noFlyMoveRate = 15;
		private int nextBonusSpawnTime = 0;
		public int bonusSpawnRate = 10;
		private int lastSpawnLocation = 0;
	
		//Variables used for scripted instances
		private Boolean scriptedInstance;
		//NFZ
		private int numScriptNFZs;
		private float[] nfzXVals;
		private float[] nfzYVals;
		private float[] nfzMoveTimes;
		//BONUS
		private int numBonuses;
		private float[] bonusXVals;
		private float[] bonusYVals;
		private float[] bonusLifeSpans;
		//Other Ships
		private int numOtherShips;
		private int[] otherShipDests;
		private float[] otherShipXVals;
		private float[] otherShipYVals;
		private float[] otherShipRotXs;
		private float[] otherShipRotYs;
		private float[] otherShipLifeSpans;
		private ArrayList[] otherPathsX;
		private ArrayList[] otherPathsY;
		//Selected Ship
		private int destNum;
		private float selShipXPos;
		private float selShipYPos;
		public GameObject selShip;
		public int eyeTrackerOn;

		public EyeTrackerTCP TCPconnection;
		public SpaceNavUDP UDPConnection;
		// Use this for initialization
		void Start ()
		{
				systems = new ArrayList ();
				ArrayList systemsPrefabs = new ArrayList ();
				shipPrefabs = new ArrayList ();
				moveHelperPrefabs = new ArrayList ();
		
				//store our system prefabs... not really used anymore
				systemsPrefabs.Add (system1);
				systemsPrefabs.Add (system2);
				systemsPrefabs.Add (system3);
				systemsPrefabs.Add (system4);
		
				//store the ship prefabs for later spawning
				shipPrefabs.Add (ship1);
				shipPrefabs.Add (ship2);
				shipPrefabs.Add (ship3);
				shipPrefabs.Add (ship4);
				
				//store the moveHelperPrefabs prefabs for later spanwing
				moveHelperPrefabs.Add (moveHelper1);
				moveHelperPrefabs.Add (moveHelper2);
				moveHelperPrefabs.Add (moveHelper3);
				moveHelperPrefabs.Add (moveHelper4);
				
				//this really just spawns the planet. It used to spawn the ships too but they needed to be treated differently
				//so they are spawned seperately now along with the move helpers. one move helper per ship
				//the move helpers are just spheres that the player drags to indicate where he wants the ship to go
				systems.Add ((GameObject)Instantiate (system1));
				systems.Add ((GameObject)Instantiate (system2));
				systems.Add ((GameObject)Instantiate (system3));
				systems.Add ((GameObject)Instantiate (system4));
		
				for (int i = 1; i <=4; i++) {
						GameObject o = (GameObject)systems [i - 1];
						o.name = "Planet" + i;
				}
		
				botLeftPoint = Camera.main.ScreenToWorldPoint (new Vector3 (0, 0, 10));
				topRightPoint = Camera.main.ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height, 10));
		
				minX = (int)botLeftPoint.x;
				minY = (int)botLeftPoint.y;
				maxX = (int)topRightPoint.x;
				maxY = (int)topRightPoint.y;
		
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				if (data == null) {
						data = (GameObject)Instantiate (gameData, getRandomPointOnScreen (), Quaternion.identity);
						//gameData gd = (gameData)data.GetComponent ("gameData");
						//gd.loadExperimentSettings ();
				}
				gameData gd = (gameData)data.GetComponent ("gameData");
				gd.setGameSettings (4);
				automate_level = gd.automation_level;
				//NOT A SCRIPTED INSTANCE, just playing the game normally
				//We create numNFZ no-fly zones that will move around the screen
				noFlyZones = new GameObject[numNFZs];
				for (int i = 0; i < numNFZs; i++) {
						noFlyZones [i] = (GameObject)Instantiate (noFlyZone);
						noFlyZones [i].transform.position = getRandomPointOnScreen ();
				}
				eyeTrackerOn = gd.pram_list.eyeTrackerOn;
				if (gd.pram_list.eyeTrackerOn > 0) {
					TCPconnection = new EyeTrackerTCP();
					TCPconnection.init ();
					TCPconnection.gamelevel = gd.gameLevel;
				}
				if (gd.pram_list.trajectorySearchOn > 0) {
					UDPConnection = new SpaceNavUDP ();
					UDPConnection.init ();
				}
		}
	
		Boolean readFromFile ()
		{
				Boolean successful = false;
		
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				gameData gd = (gameData)data.GetComponent ("gameData");
		
				string fileName = "instance_" + gd.instanceNum + ".txt";
				//Debug.Log ("Loading: " + fileName);
				string text = "";
				string line;
				System.IO.StreamReader file = new System.IO.StreamReader (fileName);
		
				while ((line = file.ReadLine()) != null) {
						text = line;
						successful = true;
				}
				string[] chunks = text.Split (new char[]{':'});

				string[] bonusString = chunks [1].Split (new char[]{';'});

				numBonuses = Convert.ToInt32 (bonusString [0]);

				bonusXVals = new float[numBonuses];
				bonusYVals = new float[numBonuses];

				for (int i = 1; i <= numBonuses; i++) {
						string[] position = bonusString [i].Split (new char[]{','});
						bonusXVals [i - 1] = Convert.ToSingle (position [0]);
						bonusYVals [i - 1] = Convert.ToSingle (position [1]);
				}
				
		
				//Other Ships
				string[] otherShipStrings = chunks [2].Split (new char[]{'~'});
				int numIts = Convert.ToInt32 (otherShipStrings [0]);
				numOtherShips = numIts;
				otherShipDests = new int[numOtherShips];
				otherShipXVals = new float[numOtherShips];
				otherShipYVals = new float[numOtherShips];
				otherShipRotXs = new float[numOtherShips];
				otherShipRotYs = new float[numOtherShips];
				otherShipLifeSpans = new float[numOtherShips];
				otherPathsX = new ArrayList[numOtherShips];
				otherPathsY = new ArrayList[numOtherShips];
				for (int i = 1; i <= numIts; i++) {
						//Data for the ship
						string[] otherShipData = otherShipStrings [(i * 2) - 1].Split (new char[]{';'});
						for (int j = 0; j < (otherShipData.Length-1); j++) {
								string[] position = otherShipData [j].Split (new char[]{','});

								if (position.Length == 6) {				
										otherShipDests [i - 1] = Convert.ToInt32 (position [0]);
										otherShipXVals [i - 1] = Convert.ToSingle (position [1]);
										otherShipYVals [i - 1] = Convert.ToSingle (position [2]);
										otherShipRotXs [i - 1] = Convert.ToSingle (position [3]);
										otherShipRotYs [i - 1] = Convert.ToSingle (position [4]);
										otherShipLifeSpans [i - 1] = Convert.ToSingle (position [5]);
								} else {
										otherShipDests [i - 1] = Convert.ToInt32 (position [1]);
										otherShipXVals [i - 1] = Convert.ToSingle (position [2]);
										otherShipYVals [i - 1] = Convert.ToSingle (position [3]);
										otherShipRotXs [i - 1] = Convert.ToSingle (position [4]);
										otherShipRotYs [i - 1] = Convert.ToSingle (position [5]);
										otherShipLifeSpans [i - 1] = Convert.ToSingle (position [6]);
								}
							
						}
						//Path markers for the ship
						otherPathsX [i - 1] = new ArrayList ();
						otherPathsY [i - 1] = new ArrayList ();
						string[] otherShipPath = otherShipStrings [i * 2].Split (new char[]{';'});
						for (int k = 1; k < (otherShipPath.Length-1); k++) {
								string[] pathPosition = otherShipPath [k].Split (new char[]{','});
								((ArrayList)otherPathsX [i - 1]).Add (Convert.ToSingle (pathPosition [0]));
								((ArrayList)otherPathsY [i - 1]).Add (Convert.ToSingle (pathPosition [1]));
						}
				}
		
				//No Fly Zones
				string[] nfzString = chunks [3].Split (new char[]{';'});
				numScriptNFZs = Convert.ToInt32 (nfzString [0]);
				nfzXVals = new float[numScriptNFZs];
				nfzYVals = new float[numScriptNFZs];
				nfzMoveTimes = new float[numScriptNFZs];
				for (int i = 1; i <= numScriptNFZs; i++) {
						string[] position = nfzString [i].Split (new char[]{','});
						nfzXVals [i - 1] = Convert.ToSingle (position [0]);
						nfzYVals [i - 1] = Convert.ToSingle (position [1]);
						nfzMoveTimes [i - 1] = Convert.ToSingle (position [2]);
				}
		
				//Destination Planet
				string[] destString = chunks [4].Split (new char[]{','});
				destNum = Convert.ToInt32 (destString [0]);

				//Selected ship
				string[] selShipString = chunks [5].Split (new char[]{','});
				//Debug.Log (selShipString.Length);
				if (selShipString.Length == 6) {
						selShipXPos = Convert.ToSingle (selShipString [0]);
						selShipYPos = Convert.ToSingle (selShipString [1]);
				} else {
						selShipXPos = Convert.ToSingle (selShipString [1]);
						selShipYPos = Convert.ToSingle (selShipString [2]);
				}	
		
				//Ship spawning rate
				shipSpawnRate = Convert.ToInt32 (chunks [8]);
		
				//No fly zone move interval
				noFlyMoveRate = Convert.ToInt32 (chunks [9]);	
		
				//Bonus spawning rate
				bonusSpawnRate = Convert.ToInt32 (chunks [10]);	
		
				file.Close ();
				return successful;
		}
	
		string readUntilChar (System.IO.StreamReader inFile, char delimeter)
		{
				int nextChar;
				string returnString = "";
				while ((nextChar = inFile.Read()) != -1 && nextChar != Convert.ToInt32(delimeter)) {
						returnString = returnString + Convert.ToChar (nextChar);
				}
				return returnString;
		}
	
		public void spawnNewShip ()
		{
				//Choose a random ship, place it in a random position, and give it a destination
				int rand = UnityEngine.Random.Range (1, 5);
				GameObject ship = (GameObject)shipPrefabs [rand - 1];

				//Selects the side the ship spawns on
				int side = UnityEngine.Random.Range (1, 5);
				while (side == lastSpawnLocation) {
						side = UnityEngine.Random.Range (1, 5);
				}
				//Creates the copy of the prefab
				GameObject shipS = (GameObject)Instantiate (ship, getRandomPointOnPerimeter (side, false), new Quaternion (0, 0, 0, 1));//clone prefab
				moveBehaviourScript mbs = (moveBehaviourScript)shipS.GetComponent ("moveBehaviourScript");
				if (side == 1 || side == 2) {
						mbs.dest = getRandomPointOnPerimeter (side + 2, true);
				} else {
						if (side == 3 || side == 4) {
								mbs.dest = getRandomPointOnPerimeter (side - 2, true);
						}
				}
		
				//Instantiate a new move helper from the prefab	
				GameObject mHelper = (GameObject)moveHelperPrefabs [rand - 1];
				GameObject mHelperS = (GameObject)Instantiate (mHelper, shipS.transform.position, new Quaternion (0, 0, 0, 1));//clone prefab
				mbs.MoveHelper = mHelperS;
		
				//Set initial speed
				mbs.mySpeed = 2.5f;
		
				//assign ship to marker
				pathMarker pm = (pathMarker)mHelperS.GetComponent ("pathMarker");
				pm.dot1 = dot1;
				pm.dot2 = dot2;
				pm.dot3 = dot3;
				pm.assignedToObj = shipS;
				lastSpawnLocation = side;
		}
	
		public GameObject spawnShipHere (int shipType, Vector3 startSpot)
		{
				//Choose a random ship, place it in a random position, and give it a destination
				GameObject ship = (GameObject)shipPrefabs [shipType];
				GameObject shipS = (GameObject)Instantiate (ship, startSpot, new Quaternion (0, 0, 0, 1));//clone prefab
				moveBehaviourScript mbs = (moveBehaviourScript)shipS.GetComponent ("moveBehaviourScript");
		
				//Instantiate a new move helper from the prefab	
				GameObject mHelper = (GameObject)moveHelperPrefabs [shipType];
				GameObject mHelperS = (GameObject)Instantiate (mHelper, shipS.transform.position, new Quaternion (0, 0, 0, 1));//clone prefab
				mbs.MoveHelper = mHelperS;
				mbs.dest = getRandomPointOnPerimeter (2, true);
		
				//Set initial speed
				mbs.mySpeed = 2.5f;
		
				//assign ship to marker
				pathMarker pm = (pathMarker)mHelperS.GetComponent ("pathMarker");
				pm.dot1 = dot1;
				pm.dot2 = dot2;
				pm.dot3 = dot3;
				pm.assignedToObj = shipS;
		
				return shipS;
		}

		//Get a random (X,Y) point on the screen
		public Vector3 getRandomPointOnScreen ()
		{
				int x = UnityEngine.Random.Range (minX, maxX);
				int y = UnityEngine.Random.Range (minY, maxY);
				return new Vector3 (x, y, 0);	
		}
	
		//Find a random point on the buffer region around the perimeter of the game space
		public Vector3 getRandomPointOnPerimeter (int side, bool dest)
		{
				if (side == 1) { //Left
						if (dest) 
								return new Vector3 ((float)(minX - 2.1), UnityEngine.Random.Range (minY, maxY), 0);
						return new Vector3 (minX - 2, UnityEngine.Random.Range (minY, maxY), 0);
				}
				if (side == 2) { //Bottom
						if (dest)
								return new Vector3 (UnityEngine.Random.Range (minX, maxX), (float)(minY - 2.1), 0);
						return new Vector3 (UnityEngine.Random.Range (minX, maxX), minY - 2, 0);
				}
				if (side == 3) { //Right
						if (dest)
								return new Vector3 ((float)(maxX + 2.1), UnityEngine.Random.Range (minY, maxY), 0);
						return new Vector3 (maxX + 2, UnityEngine.Random.Range (minY, maxY), 0);
				}
				if (side == 4) { //Top
						if (dest)
								return new Vector3 (UnityEngine.Random.Range (minX, maxX), (float)(maxY + 2.1), 0);
						return new Vector3 (UnityEngine.Random.Range (minX, maxX), maxY + 2, 0);
				}
				return new Vector3 (0, 0, 170);
		}
	
		void blink ()
		{
				if (selShip != null) {
						moveBehaviourScript mbs = (moveBehaviourScript)selShip.GetComponent ("moveBehaviourScript");
						mbs.MoveHelper.renderer.enabled = !mbs.MoveHelper.renderer.enabled;
				} else {
						CancelInvoke ("blink");
				}
		}
		
		// Update is called once per frame
		void Update ()
		{
				GameObject data = GameObject.FindGameObjectWithTag("GameData");
				gameData gd2 = (gameData) data.GetComponent("gameData");
				if ((int)Time.timeSinceLevelLoad == nextShipSpawnTime) {
						spawnNewShip ();
						nextShipSpawnTime = (int)Time.timeSinceLevelLoad + shipSpawnRate;
						try
						{
							gd2.pram_list.numOfShips += 1.0f;
							string fileName = gd2.fileName;//gd2.unique_id + "Level_" + gd2.gameLevel + "_Data.txt";
							using(StreamWriter streamer = new StreamWriter(fileName, gd2.writtenYet)) {
								string time = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond;
								streamer.WriteLine("NumOfShips,"+gd2.pram_list.numOfShips+","+time);
								streamer.Close();
							}
							gd2.writtenYet = true;
						} catch (System.NullReferenceException) {}
				}
				if ((int)Time.timeSinceLevelLoad == nextNoFlySpawnTime) {
						for (int i = 0; i < numNFZs; i++) {
								noFlyZoneController nfzc = (noFlyZoneController)noFlyZones [i].GetComponent ("noFlyZoneController");
								nfzc.lastMoveTime = Time.timeSinceLevelLoad;
								noFlyZones [i].transform.position = getRandomPointOnScreen ();
						}
						nextNoFlySpawnTime = (int)Time.timeSinceLevelLoad + noFlyMoveRate;
				}
				if ((int)Time.timeSinceLevelLoad == nextBonusSpawnTime) {
						Instantiate (bonus, getRandomPointOnScreen (), new Quaternion (0, 0, 0, 1));
						nextBonusSpawnTime = (int)Time.timeSinceLevelLoad + bonusSpawnRate;
				}
				if (gd2.pram_list.eyeTrackerOn > 0) {
					gd2.screen_width = TCPconnection.screen_width;
					gd2.screen_height = TCPconnection.screen_height;
				}
				Resources.UnloadUnusedAssets();
		}
	
}
