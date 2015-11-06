using UnityEngine;
using System.Collections;
using System.IO;

public class collisionHandler : MonoBehaviour {
	
	public GameObject targetReachedAnimation;
	public GameObject collidedAnimation;
	private GameObject scoreSystem;
	public GUIText posScorePopUp;
	public GUIText negScorePopUp;
	public float creationTime;
	public float drawStartTime;
	public int shipID;
	private ShipState myState;
	 
	// Use this for initialization
	void Start() {
		scoreSystem = GameObject.Find("ScoreSystem");
		creationTime = Time.timeSinceLevelLoad;
		
		GameObject data = GameObject.FindGameObjectWithTag("GameData");
		gameData gd = (gameData) data.GetComponent("gameData");
		
		//Give ship an ID and capture spawning event
		shipID = gd.nextShipIDNum;
		string fileName = "";
		if(!gd.scriptedInstance) {
			fileName = "Level_" + gd.gameLevel + "_Data.txt";
			using(StreamWriter streamer = new StreamWriter(fileName, gd.writtenYet)) {
				streamer.WriteLine("Spawn:"+shipID + "," + Time.timeSinceLevelLoad.ToString("f2"));
				streamer.Close();
			}
		}
		gd.nextShipIDNum++;
		
		myState = ShipState.flying;
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	IEnumerator OnTriggerEnter(Collider col) {
		if(col.tag=="planet") {
			string assignedPlanet=getMyPlanetName();
			GameObject myplanet = GameObject.Find (assignedPlanet);
			if(myplanet.name == col.gameObject.name && myState == ShipState.flying) {
				myState = ShipState.landed;
				planetCollisionHandler(col);			
				yield return new WaitForSeconds (.4f);
				Destroy (gameObject);
			}
		}
		if(col.tag=="ship" && myState == ShipState.flying)	{
			myState = ShipState.exploded;
			shipCollisionHandler(col);
	  		yield return new WaitForSeconds (.4f);
			Destroy (gameObject);
		}
	}
	
	void shipCollisionHandler(Collider col) {
		shipsCollidednAnimation(col.transform.position);
		
		//Capture Relevant data for a ship collision (ship id and timestamp)
		collisionHandler ch = (collisionHandler) gameObject.GetComponent("collisionHandler");
		GameObject data = GameObject.FindGameObjectWithTag("GameData");
		gameData gd = (gameData) data.GetComponent("gameData");
		
		string fileName;
		bool goodToWrite = true;
		if(gd.scriptedInstance) {
			goodToWrite = false;
				fileName = "Response_" + gd.instanceNum + ".txt";
		} else {
			fileName = "Level_" + gd.gameLevel + "_Data.txt";
		}
		
		if(goodToWrite) {
			//Capture relevant data about the present game space
			using(StreamWriter streamer = new StreamWriter(fileName, gd.writtenYet)) {
				streamer.WriteLine("Collision:"+ch.shipID + "," + Time.timeSinceLevelLoad.ToString("f2"));
				streamer.Close();
			}
		}
	}
	
	void planetCollisionHandler(Collider col) {
		string assignedPlanet=getMyPlanetName();
		GameObject myplanet=GameObject.Find (assignedPlanet);
		
		if(myplanet.name == col.gameObject.name) {
	    	reachedDestinationAnimation();
		} 
		
		//Capture Relevant data for a ship reaching its destination (ship id and timestamp)
		collisionHandler ch = (collisionHandler) gameObject.GetComponent("collisionHandler");
		GameObject data = GameObject.FindGameObjectWithTag("GameData");
		gameData gd = (gameData) data.GetComponent("gameData");
		
		string fileName;
		bool goodToWrite = true;
		if(gd.scriptedInstance) {
			goodToWrite = false;
				fileName = "Response_" + gd.instanceNum + ".txt";
		} else {
			fileName = "Level_" + gd.gameLevel + "_Data.txt";
		}
		
		if(goodToWrite) {
			//Capture relevant data about the present game space
			using(StreamWriter streamer = new StreamWriter(fileName, gd.writtenYet)) {
				streamer.WriteLine("Destination:"+ch.shipID + "," + Time.timeSinceLevelLoad.ToString("f2"));
				streamer.Close();
			}
		}
	}
	
	string getMyPlanetName() {
	    string shipName = gameObject.name;
		string last=shipName.Substring(4,1);
		if(last!= null) {
			return "Planet"+last;
		}
		return "";
	}
	
	void reachedDestinationAnimation() {
		scoreSystem.SendMessage("addPoints",100);
		Instantiate(targetReachedAnimation, gameObject.transform.position, gameObject.transform.rotation);
		posScorePopUp.text = "+100";
		Vector3 screenPos = Camera.main.WorldToViewportPoint( gameObject.transform.position);
		Instantiate (posScorePopUp, screenPos, Quaternion.identity);
	}
	
   	void shipsCollidednAnimation(Vector3 collidePoint) {
		scoreSystem.SendMessage("addPoints",-100);
		Instantiate(collidedAnimation, collidePoint, gameObject.transform.rotation);
		negScorePopUp.text = "-100";
		Vector3 screenPos = Camera.main.WorldToViewportPoint( gameObject.transform.position);
		Instantiate (negScorePopUp, screenPos, Quaternion.identity);
	}
	
	private enum ShipState{flying, exploded, landed};	
	
}