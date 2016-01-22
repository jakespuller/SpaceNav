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
	private Vector3 botLeftPoint;
	private Vector3 topRightPoint;
	private float minX;
	private float minY;
	private float maxX;
	private float maxY;
	private string fileName;
	// Use this for initialization
	void Start() {
		scoreSystem = GameObject.Find("ScoreSystem");
		creationTime = Time.timeSinceLevelLoad;
		
		GameObject data = GameObject.FindGameObjectWithTag("GameData");
		gameData gd = (gameData) data.GetComponent("gameData");
		
		//Give ship an ID and capture spawning event
		shipID = gd.nextShipIDNum;

		fileName = gd.fileName;//gd.unique_id + "Level_" + gd.gameLevel + "_Data.txt";
		using(StreamWriter streamer = new StreamWriter(fileName, gd.writtenYet)) {
			string time = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond;
			streamer.WriteLine("Spawn,"+shipID + "," + time);
			streamer.Close();
		}
		gd.writtenYet = true;
		gd.nextShipIDNum++;
		myState = ShipState.flying;
		botLeftPoint  = Camera.main.ScreenToWorldPoint(new Vector3(0,0,10));
		topRightPoint  = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
		maxX = topRightPoint.x;
		maxY = topRightPoint.y;
	}

	//Handles the collision of a ship object or ship objects
	IEnumerator OnTriggerEnter(Collider col) {
		GameObject data = GameObject.FindGameObjectWithTag("GameData");
		gameData gd2 = (gameData)data.GetComponent("gameData");
		if (col.tag == "planet" || (col.tag == "ship" && myState == ShipState.flying))
		{
			if(col.tag=="planet") {
				string assignedPlanet=getMyPlanetName();
				GameObject myplanet = GameObject.Find (assignedPlanet);
				if(myplanet.name == col.gameObject.name && myState == ShipState.flying) {
					myState = ShipState.landed;
					planetCollisionHandler(col);			
					yield return new WaitForSeconds (.4f);
					Destroy (gameObject);
					try
					{
						gd2.pram_list.numOfShips -= 1.0f;
					} catch (System.NullReferenceException) {}
				}
			}
			if(col.tag=="ship" && myState == ShipState.flying)	{
				myState = ShipState.exploded;
				shipCollisionHandler(col);
		  		yield return new WaitForSeconds (.4f);
				Destroy (gameObject);
				try
				{
					gd2.pram_list.numOfShips -= 1.0f;
				} catch (System.NullReferenceException) {}
			}
			//string fileName = gd2.unique_id + "Level_" + gd2.gameLevel + "_Data.txt";
			using(StreamWriter streamer = new StreamWriter(fileName, gd2.writtenYet)) {
				string time = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond;
				streamer.WriteLine("NumOfShips,"+gd2.pram_list.numOfShips+","+time);
				streamer.Close();
			}
			gd2.writtenYet = true;
		}
	}
	
	void shipCollisionHandler(Collider col) {
		shipsCollidednAnimation(col.transform.position);
		
		//Capture Relevant data for a ship collision (ship id and timestamp)
		collisionHandler ch = (collisionHandler) gameObject.GetComponent("collisionHandler");
		GameObject data = GameObject.FindGameObjectWithTag("GameData");
		gameData gd = (gameData) data.GetComponent("gameData");
		//string fileName = gd.fileName;//gd.unique_id + "Level_" + gd.gameLevel + "_Data.txt";
		//Capture relevant data about the present game space
		using(StreamWriter streamer = new StreamWriter(fileName, gd.writtenYet)) {
			string time = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond;
			streamer.WriteLine("Collision,"+ch.shipID + "," + time + "," + (ch.transform.position.x/(maxX*2f) + .5f)*gd.screen_width+","+(ch.transform.position.y/(maxY*2f) + .5f)*gd.screen_height);
			streamer.Close();
		}
		gd.writtenYet = true;
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
		
		//string fileName = gd.unique_id + "Level_" + gd.gameLevel + "_Data.txt";
		//Capture relevant data about the present game space
		using(StreamWriter streamer = new StreamWriter(fileName, gd.writtenYet)) {
			string time = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond;
			streamer.WriteLine("Destination,"+ch.shipID + "," + time);
			streamer.Close();
		}
		gd.writtenYet = true;
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
