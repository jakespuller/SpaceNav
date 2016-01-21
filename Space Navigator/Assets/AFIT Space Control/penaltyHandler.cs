using UnityEngine;
using System.Collections;
using System.IO;

public class penaltyHandler : MonoBehaviour {
	
	public GameObject scoreObject;
	public bool inRestrictedAirSpace = false;
	public int numObjectsInNoFlyZone = 0;
	public int timeInZone = 0;
	public GUIText negScorePopUp;
	
	// Use this for initialization
	void Start() {
	   scoreObject = GameObject.Find("ScoreSystem");
	}
	
	void OnTriggerEnter(Collider col) {	
		if(col.tag=="noFlyZone") { //Capture an object entering a No Fly Zone
			numObjectsInNoFlyZone += 1;
			timeInZone = (int) Time.timeSinceLevelLoad;
			if(scoreObject != null) {
				scoreObject.BroadcastMessage("addPoints",-10);
			} else {
				scoreObject = GameObject.Find("ScoreSystem");
			}
			
			//Capture Relevant data for a ship entering NFZ (ship id and timestamp)
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
					streamer.WriteLine("NFZEnter:"+ch.shipID + "," + Time.timeSinceLevelLoad.ToString("f2"));
					streamer.Close();
				}
			}
		}
	}
	
	void OnTriggerExit(Collider col) {
		if(col.tag=="noFlyZone") { //Capture an object exit from No Fly Zone
			numObjectsInNoFlyZone -= 1;
			
			//Capture Relevant data for a ship exiting NFZ (ship id and timestamp)
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
					streamer.WriteLine("NFZExit:"+ch.shipID + "," + Time.timeSinceLevelLoad.ToString("f2"));
					streamer.Close();
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update() {	
		//Subtract points while a ship is in a no fly zone
		if(numObjectsInNoFlyZone > 0 && (int) Time.timeSinceLevelLoad == timeInZone + 1) {
			scoreObject.BroadcastMessage("addPoints",-10 * numObjectsInNoFlyZone);
			negScorePopUp.text = "-10";
			Vector3 screenPos = Camera.main.WorldToViewportPoint(gameObject.transform.position);
			Instantiate (negScorePopUp, screenPos, Quaternion.identity);
			timeInZone += 1;
		}	
	}
}
