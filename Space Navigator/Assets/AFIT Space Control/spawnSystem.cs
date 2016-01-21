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
	
		private Vector3 botLeftPoint;
		private Vector3 topRightPoint;
		private int minX;
		private int minY;
		private int maxX;
		private int maxY;
	
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
				gameData gd2 = (gameData)data.GetComponent ("gameData");
				gd2.setGameSettings (4);
				gd2.writtenYet = false;
				automate_level = gd2.automation_level;
		
				scriptedInstance = gd2.scriptedInstance;
		
				if (scriptedInstance) {
						readFromFile ();
						//Stop new things from happening
						nextShipSpawnTime = 1000;
						nextNoFlySpawnTime = 1000;
						nextBonusSpawnTime = 1000;
						GameObject gotc = GameObject.FindGameObjectWithTag ("TimerController");
						TimerController tc = (TimerController)gotc.GetComponent ("TimerController");
						tc.timeLeft = 10;
						//Place NFZs according to the script
						numNFZs = numScriptNFZs;
						noFlyZones = new GameObject[numNFZs];
						for (int i = 0; i < numNFZs; i++) {
								noFlyZones [i] = (GameObject)Instantiate (noFlyZone);
								noFlyZones [i].transform.position = new Vector3 (nfzXVals [i], nfzYVals [i], 0);
						}
			
						//Place bonuses according to the script
						for (int i = 0; i < numBonuses; i++) {
								Instantiate (bonus, new Vector3 (bonusXVals [i], bonusYVals [i], 0), new Quaternion (0, 0, 0, 1));
						}
	
						//Place other ships according to the script
						for (int i = 0; i < numOtherShips; i++) {
								//Create the ship and its mover helper
								//Debug.Log ("spawning from here: " + (otherShipDests [i] - 1) + "  " + otherShipXVals [i] + "  " + otherShipYVals [i]);
								GameObject shippy = spawnShipHere ((otherShipDests [i] - 1), new Vector3 (otherShipXVals [i], otherShipYVals [i], 0));
								moveBehaviourScript mbs = (moveBehaviourScript)shippy.GetComponent ("moveBehaviourScript");
				
								//Create the initial path marker to start from
								pathMarker firstMarker = (pathMarker)mbs.MoveHelper.GetComponent ("pathMarker");
								firstMarker.assignedToObj = shippy;
								mbs.path = firstMarker;
				
								//Add all of the other markers along the path
								for (int j = 1; j < ((ArrayList) otherPathsX[i]).Count; j++) {
										GameObject newMarker = (GameObject)Instantiate (marker, new Vector3 ((float)((ArrayList)otherPathsX [i]) [j], (float)((ArrayList)otherPathsY [i]) [j], 0), new Quaternion (0, 0, 0, 1));
										firstMarker.enqueue (newMarker);
										firstMarker.setPos ();
								}
						}
				
						//Place place selected ship according to the script
						//GameObject selShip = spawnShipHere((destNum - 1), new Vector3(selShipXPos, selShipYPos, 170));
						selShip = spawnShipHere ((destNum - 1), new Vector3 (selShipXPos, selShipYPos, 0));
						InvokeRepeating ("blink", (float)0.2, (float)0.2);

						UDPConnection = new SpaceNavUDP ();
						UDPConnection.init ();
			
				} else { //NOT A SCRIPTED INSTANCE, just playing the game normally
						//We create numNFZ no-fly zones that will move around the screen
						noFlyZones = new GameObject[numNFZs];
						for (int i = 0; i < numNFZs; i++) {
								noFlyZones [i] = (GameObject)Instantiate (noFlyZone);
								noFlyZones [i].transform.position = getRandomPointOnScreen ();
						}

						UDPConnection = new SpaceNavUDP ();
						UDPConnection.init ();
					//Debug.Log ("called init");
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

				//Time stamp
				//float time = Convert.ToSingle(chunks[0]);
				//print("Time = " + time);
		
				//Bonuses
				//Debug.Log (chunks [0]);
				//Debug.Log (chunks [1]);
				string[] bonusString = chunks [1].Split (new char[]{';'});
				//Debug.Log (bonusString);
				numBonuses = Convert.ToInt32 (bonusString [0]);
				//Debug.Log ("num bonus:  " + numBonuses);
				bonusXVals = new float[numBonuses];
				bonusYVals = new float[numBonuses];
				//bonusLifeSpans = new float[numBonuses];
				for (int i = 1; i <= numBonuses; i++) {
						string[] position = bonusString [i].Split (new char[]{','});
						bonusXVals [i - 1] = Convert.ToSingle (position [0]);
						bonusYVals [i - 1] = Convert.ToSingle (position [1]);
						//Debug.Log("Test " + Convert.ToSingle (position [2]));
						//bonusLifeSpans [i - 1] = Convert.ToSingle (position [2]);
						//print("Bonus " + i + " at (" + bonusXVals[i-1] + ", " + bonusYVals[i-1] +"), Life = " + bonusLifeSpans[i-1]);
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
								//Debug.Log (position.Length);
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
								//print ("Ship " + i + " (dest = " + otherShipDests [i - 1] + ")at Pos (" + otherShipXVals [i - 1] + ", " + otherShipYVals [i - 1] + "), Rot (" + otherShipRotXs [i - 1] + ", " + otherShipRotYs [i - 1] + "), Life = " + otherShipLifeSpans [i - 1]);
						}
						//Path markers for the ship
						otherPathsX [i - 1] = new ArrayList ();
						otherPathsY [i - 1] = new ArrayList ();
						string[] otherShipPath = otherShipStrings [i * 2].Split (new char[]{';'});
						for (int k = 1; k < (otherShipPath.Length-1); k++) {
								string[] pathPosition = otherShipPath [k].Split (new char[]{','});
								((ArrayList)otherPathsX [i - 1]).Add (Convert.ToSingle (pathPosition [0]));
								((ArrayList)otherPathsY [i - 1]).Add (Convert.ToSingle (pathPosition [1]));
								//print ("Ship " + i + " Marker " + k + " at Pos (" + Convert.ToSingle(pathPosition[0]) + ", " + Convert.ToSingle(pathPosition[1]) + ")");
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
						//print("No Fly Zone " + i + " at Pos (" + nfzXVals[i-1] + ", " + nfzYVals[i-1] + "), Last moved at " + nfzMoveTimes[i-1]);
				}
		
				//Destination Planet
				string[] destString = chunks [4].Split (new char[]{';'});
				destNum = Convert.ToInt32 (destString [0]);
				//string[] destPos = destString[1].Split(new char[]{','});
				//float destXPos = Convert.ToSingle(destPos[0]);
				//float destYPos = Convert.ToSingle(destPos[1]);
				//print("Dest Planet " + destNum + "at Pos (" + destXPos + ", " + destYPos + ")");
		
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
				//float selShipRotX = Convert.ToSingle(selShipString[2]);
				//float selShipRotY = Convert.ToSingle(selShipString[3]);
				//float selShipLifeSpan = Convert.ToSingle(selShipString[4]);
				//float selShipDrawTime = Convert.ToSingle(selShipString[5]);
				//print("Selected ship at Pos (" + selShipXPos + ", " + selShipYPos + "), Rot (" + selShipRotX + ", " + selShipRotY + "), Life = " + selShipLifeSpan + ", Draw Time = " + selShipDrawTime);
		
//		//This Path Markers
//		string[] thisPathString = chunks[6].Split(new char[]{';'});
//		for(int i = 1; i <= Convert.ToInt32(thisPathString[0]); i++) {
//			string[] position = thisPathString[i].Split(new char[]{','});
//			float xPos = Convert.ToSingle(position[0]);
//			float yPos = Convert.ToSingle(position[1]);
//			//print("This path marker " + i + " at Pos (" + xPos + ", " + yPos + ")");
//		}
						
				//Game score at capture time
				//int score = Convert.ToInt32(chunks[7]);
				//print("Score = " + score);
		
				//Ship spawning rate
				shipSpawnRate = Convert.ToInt32 (chunks [8]);
				//print("Ship Spawn Rate = " + shipSpawnRate);
		
				//No fly zone move interval
				noFlyMoveRate = Convert.ToInt32 (chunks [9]);	
				//print("NFZ move rate = " + noFlyMoveRate);
		
				//Bonus spawning rate
				bonusSpawnRate = Convert.ToInt32 (chunks [10]);	
				//print("Bonus spawn rate = " + bonusSpawnRate);
		
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
				int side = UnityEngine.Random.Range (1, 5);
				while (side == lastSpawnLocation) {
						side = UnityEngine.Random.Range (1, 5);
				}
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
		
				//shipS.transform.eulerAngles = new Vector3((float)70.03, (float) 70.00, (float) 0);
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
								return new Vector3 ((float)(minX - 3.1), UnityEngine.Random.Range (minY, maxY), 0);
						return new Vector3 (minX - 3, UnityEngine.Random.Range (minY, maxY), 0);
				}
				if (side == 2) { //Bottom
						if (dest)
								return new Vector3 (UnityEngine.Random.Range (minX, maxX), (float)(minY - 3.1), 0);
						return new Vector3 (UnityEngine.Random.Range (minX, maxX), minY - 3, 0);
				}
				if (side == 3) { //Right
						if (dest)
								return new Vector3 ((float)(maxX + 3.1), UnityEngine.Random.Range (minY, maxY), 0);
						return new Vector3 (maxX + 3, UnityEngine.Random.Range (minY, maxY), 0);
				}
				if (side == 4) { //Top
						if (dest)
								return new Vector3 (UnityEngine.Random.Range (minX, maxX), (float)(maxY + 3.1), 0);
						return new Vector3 (UnityEngine.Random.Range (minX, maxX), maxY + 3, 0);
				}
				return new Vector3 (0, 0, 170);
		}
	
		void blink ()
		{
				if (selShip != null) {
						moveBehaviourScript mbs = (moveBehaviourScript)selShip.GetComponent ("moveBehaviourScript");
						mbs.MoveHelper.GetComponent<Renderer>().enabled = !mbs.MoveHelper.GetComponent<Renderer>().enabled;
				} else {
						CancelInvoke ("blink");
				}
		}
	
		// Update is called once per frame
		void Update ()
		{
				if ((int)Time.timeSinceLevelLoad == nextShipSpawnTime) {
						spawnNewShip ();
						nextShipSpawnTime = (int)Time.timeSinceLevelLoad + shipSpawnRate;
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

		Resources.UnloadUnusedAssets();
		
		}

		private void OnDisable ()
		{
				if (UDPConnection.receiveThread != null) 
						UDPConnection.receiveThread.Abort ();
				UDPConnection.client.Close ();
		}
	
		private void OnApplicationQuit ()
		{
				if (UDPConnection.receiveThread != null) 
						UDPConnection.receiveThread.Abort ();
				UDPConnection.client.Close ();
		}
	
}
