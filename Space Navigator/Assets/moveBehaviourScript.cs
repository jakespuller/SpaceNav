using UnityEngine;
using System.Collections;
using System.IO;

public class moveBehaviourScript : MonoBehaviour {
	
	//GLOBALS//
    public Vector3 dest;
	public Vector3 startPos;
	public GameObject MoveHelper;
	public pathMarker path;
	public ArrayList collidedWithGameObjects;
	public float mySpeed = 2.5f;
	public bool moving;
	public bool no_autodraw = false;
	private Vector3 botLeftPoint;
	private Vector3 topRightPoint;
	private int minX;
	private int minY;
	private int maxX;
	private int maxY;
	
	// Use this for initialization
	void Start() {
		collidedWithGameObjects = new ArrayList();
		path = (pathMarker) MoveHelper.GetComponent("pathMarker");
	    startPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);		

		if(!path.hasPath()) {
			MoveTo(dest, mySpeed);	
		} else {
			Vector3 target = path.dequeue();
		  	MoveTo(target, mySpeed);
		
		}
		
		botLeftPoint  = Camera.main.ScreenToWorldPoint(new Vector3(0,0,10));
		topRightPoint  = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
		minX = (int) botLeftPoint.x;
		minY = (int) botLeftPoint.y;
		maxX = (int) topRightPoint.x;
		maxY = (int) topRightPoint.y;
	}
	
	// Update is called once per frame
	void Update () {
		//Check to see if the ship has left the screen
		if(transform.position.x > maxX+3 || transform.position.y > maxY+3 ||
		   transform.position.x < minX-3 || transform.position.y < minY-3) {


			//Capture Relevant data for a ship leaving the screen (ship id and timestamp)
			collisionHandler ch = (collisionHandler) gameObject.GetComponent("collisionHandler");
			GameObject data = GameObject.FindGameObjectWithTag ("GameData");
			gameData gd = (gameData)data.GetComponent ("gameData");
			
			string fileName;
			bool goodToWrite = true;
			if (gd.scriptedInstance) {
				goodToWrite = false;
				fileName = "Response_" + gd.instanceNum + ".txt";
			} else {
				fileName = "Level_" + gd.gameLevel + "_Data.txt";
			}
			
			if (goodToWrite) {
				//Capture relevant data about the present game space
				using (StreamWriter streamer = new StreamWriter(fileName, gd.writtenYet)) {
					streamer.WriteLine ("Screen:" + ch.shipID + "," + Time.timeSinceLevelLoad.ToString ("f2"));
					streamer.Close ();
				}
			}

			Destroy(gameObject);

		}
		if(!path.hasPath()) {
			return;
		}   
		if(path.pathStarted && path.hasPath() && !path.mouseIsDown) {
			path.pathStarted = false;
			Vector3 nextPoint = path.dequeue();
			MoveTo(nextPoint, mySpeed);
		}
		if(path.mouseIsDown && path.hasPath()) {
			Vector3 targettemp = (Vector3)path.firstPoint();
			MoveTo(targettemp,mySpeed);
		}
	}
	
	//Move the GameObject to pointB at the given speed
	void MoveTo (Vector3 pointB, float speed) {
		moving = true;
		Hashtable ht = new Hashtable();
		ht.Add("position",pointB);
		ht.Add("speed",speed);
		ht.Add("easetype","linear");
		ht.Add("oncomplete","stopAni");
		ht.Add ("orienttopath",true);
       	iTween.MoveTo(gameObject,ht);
	}

	//IEnumerator stopAni()
	void stopAni() {
		moving = false;
		
//		//Check to see if the ship has left the screen
//		if(transform.position.x > maxX || transform.position.y > maxY ||
//		   transform.position.x < minX || transform.position.y < minY) {
//			Destroy(gameObject);
//			print ("Destroy");
//			print (transform.position.x);
//			if(!path.hasPath ()){
//				print ("No Path");
//			}
//		}
		
		if(!path.hasPath()) {


			//Just reached the end of its path
			if(!path.mouseIsDown) {	
				MoveTo(dest, mySpeed);
			}
		} else {
			Vector3 target = path.dequeue();
		  	MoveTo(target, mySpeed);	
		}		
	}
	
}
