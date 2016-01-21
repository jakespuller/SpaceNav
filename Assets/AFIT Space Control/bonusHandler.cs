using UnityEngine;
using System.Collections;
using System.IO;

public class bonusHandler : MonoBehaviour
{
	
		public GameObject scoreObject;
		public bool inRestrictedAirSpace = false;
		public int numObjectsInNoFlyZone = 0;
		public int timeInZone = 0;
		public GUIText posScorePopUp;
	
		public float creationTime;
	
		// Use this for initialization
		void Start ()
		{
				scoreObject = GameObject.Find ("ScoreSystem");
				creationTime = Time.timeSinceLevelLoad;
		}
	
		void OnTriggerEnter (Collider col)
		{	
				if (col.tag == "ship") { //Capture an object collecting a Bonus
						scoreObject.BroadcastMessage ("addPoints", 50);
						posScorePopUp.text = "+50";

						//Capture Relevant data for a ship picking up a bonus (ship id and timestamp)
						collisionHandler ch = (collisionHandler)col.GetComponent ("collisionHandler");
						GameObject data = GameObject.FindGameObjectWithTag ("GameData");
						gameData gd = (gameData)data.GetComponent ("gameData");
						string fileName = gd.fileName;//gd.unique_id + "Level_" + gd.gameLevel + "_Data.txt";
						//Capture relevant data about the present game space
						using (StreamWriter streamer = new StreamWriter(fileName, gd.writtenYet)) {
								string time = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond;
								streamer.WriteLine ("Bonus," + ch.shipID + "," + time + "\n");
								streamer.Close ();
						}
						Vector3 screenPos = Camera.main.WorldToViewportPoint (gameObject.transform.position);
						Instantiate (posScorePopUp, screenPos, Quaternion.identity);
						Destroy (gameObject);
				}
		}
	
		void OnTriggerExit (Collider col)
		{
		}
}
