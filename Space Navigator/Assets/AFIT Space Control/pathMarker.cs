using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.InteropServices;

public class pathMarker : MonoBehaviour
{

		public const int SIMILAR = 1;
		public const int DISSIMILAR = 2;
		public const int STRAIGHT = 3;
		public const int NONE = 0;

		//Create variables for use later
		public Vector3 lastPosition;
		private Vector3 posDot1;
		private Vector3 posDot2;
		private Vector3 posDot3;
		public GameObject marker;
		public GameObject dot1;
		public GameObject dot2;
		public GameObject dot3;
		public ArrayList points;
		public GameObject assignedToObj;
		public bool pathStarted;
		private GUIStyle dot1_butt_style;
		private GUIStyle dot2_butt_style;
		private GUIStyle dot3_butt_style;

		//Variables we want initialized for every instance
		private double timeToAA = 4;//8.3891;
		public bool mouseIsDown = false;
		private bool popup = false;
		private bool userChoice = false;
		private bool correctShip = true;
		private bool resume = false;
		private bool waiting = false;
		private bool display_butts = false;
		public float dis = 0;
		public float maxDistance = .1f;
		private int which_dot1 = 0;
		private int which_dot2 = 0;
		private int which_dot3 = 0;

		// Initialize a new path marker object
		void Start ()
		{
				pathStarted = false;
				if (points == null) {
						points = new ArrayList ();
				}
				setPos ();
		}
		
		//Return distance from 3-dimensional point A to point B
		float dist (Vector3 A, Vector3 B)
		{
				return Vector3.Distance (A, B);
		}

		//Create a straight-line trajectory from the current ship's position to the destination
		void genStraightTrajectory (Vector3 destPos, Vector3 currPos)
		{
				//Create a set of x points in a straight line from the ship to its destination			
				for (int i = 1; i <= 50; i++) {
						float incdist = (float)i * 0.02f;
						Vector3 tp = ((destPos - currPos) * (incdist)) + currPos;
						GameObject newMarker = (GameObject)Instantiate (marker, tp, gameObject.transform.rotation);
						enqueue (newMarker);
						setPos ();
				}
				writeToFile ("Auto:");
		}

		//Depending on the automation level, create a number of trajectories from the current position to the destination
		void genSimilarTrajectory (Vector3 destPos, Vector3 currPos)
		{
				//Get the extreme points on the camera view of the world so we can stay within the bounds
				Vector3 min = Camera.main.ScreenToWorldPoint (new Vector3 (0, 0));
				Vector3 max = Camera.main.ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height));

				//Get the game controller in order to act on sub-objects
				GameObject control = GameObject.FindGameObjectWithTag ("GameController");
				spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");

				//Send the current state to the trajectory server and wait for a reply
				int curNum = ss.UDPConnection.numReceived;
				ss.UDPConnection.sendString (stateRepresentation ());
				WaitForReply (curNum);
		
				//Once we receive the two response trajectories, parse them into a set of float arrays
				string[] test = ss.UDPConnection.lastReceivedUDPPacket.Split (',');
				float[] x_vals1 = new float[50];
				float[] y_vals1 = new float[50];
				for (int i = 0; i < 50; i++) {
						x_vals1 [i] = float.Parse (test [i]);
						y_vals1 [i] = float.Parse (test [i + 50]);
				}
		
				//We need to transform the trajectories received back into the Unity state space.  Determine the transformation variables
				//How much to rotate each point around the origin
				double tan_val = (destPos.y - currPos.y) / (destPos.x - currPos.x);
				double theta = 0; // theta is angle in radians up from X axis (+ = Q1, - = Q2)
				if (destPos.x - currPos.x > 0) {	// + in the X direction
						theta = - Math.Atan (tan_val) * (180 / Math.PI); 
				} else {  							// - in the X direction
						theta = - (180 + Math.Atan (tan_val) * (180 / Math.PI));
				}
				double rotmatx1 = (Math.Cos (-theta * Math.PI / 180));
				double rotmatx2 = (- Math.Sin (-theta * Math.PI / 180));
				double rotmaty1 = (Math.Sin (-theta * Math.PI / 180));
				double rotmaty2 = (Math.Cos (-theta * Math.PI / 180));
		
				//How much to scale the overall trajectory from (0,0)->(1,0) to current position to destination
				double scaling_factor = Math.Sqrt (Math.Pow (destPos.x - currPos.x, 2) + Math.Pow (destPos.y - currPos.y, 2));
		
				//How far to translate from (0,0) to current ship's position
				double translate_x = currPos.x;
				double translate_y = currPos.y;

				//Apply the un-transformations to each point in the trajectory
				for (int i = 0; i < 50; i++) {
						//Rotation transformation
						float new_dest_x = (float)rotmatx1 * x_vals1 [i] + (float)rotmatx2 * y_vals1 [i];
						float new_dest_y = (float)rotmaty1 * x_vals1 [i] + (float)rotmaty2 * y_vals1 [i];
						x_vals1 [i] = new_dest_x;
						y_vals1 [i] = new_dest_y;
			
						//Scaling transformation
						x_vals1 [i] = x_vals1 [i] * (float)scaling_factor;
						y_vals1 [i] = y_vals1 [i] * (float)scaling_factor;
			
						//Translation transformation
						x_vals1 [i] = x_vals1 [i] + (float)translate_x;
						y_vals1 [i] = y_vals1 [i] + (float)translate_y;
				} 

				//Place all of the un-transformed points into the scene
				for (int i = 0; i < 50; i++) {
						Vector3 tp = currPos;
						tp.x = x_vals1 [i];
						tp.y = y_vals1 [i];

						GameObject newMarker = (GameObject)Instantiate (marker, tp, gameObject.transform.rotation);

						//Uncomment the next two lines to actually place the path marker into the current ship's queue
						enqueue (newMarker);
						setPos ();
				}

				//Write the first trajectory to the log file
				writeTrajToFile (x_vals1, y_vals1, which_dot1);
				writeToFile ("Auto:");
		}

		//Mouse button is pressed on this object 
		void OnMouseDown ()
		{
		
				//Get the game controller in order to act on sub-objects
				GameObject control = GameObject.FindGameObjectWithTag ("GameController");
				spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
				collisionHandler ch = (collisionHandler)assignedToObj.GetComponent ("collisionHandler");
		
				//Make sure we let the moveBehaviourScript know to start following path
				pathStarted = true;
		
				//Get rid of any old points
				removeAllPoints ();
		
				//Keep track of the fact we started drawing
				mouseIsDown = true;
		
				//Record when we started drawing
				ch.drawStartTime = Time.timeSinceLevelLoad;
		
//				//Check the automation level to determine what action to take next
//				if (ss.automate_level == DISSIMILAR) {
//						//Find the locations of the selected ship and its destination planet
//						Vector3 currPos = assignedToObj.transform.position;
//						string planetName = ("Planet" + assignedToObj.name.Substring (4, 1));	
//						GameObject assignedPlanet = GameObject.Find (planetName);
//						Vector3 destPos = assignedPlanet.transform.position;
//			
//						//Generate a trajectory to the destination planet from the current position
//						genSimilarTrajectory (destPos, currPos);
//				}
		}

		//Mouse button is released
		void OnMouseUp ()
		{
				//Get needed components 
				GameObject control = GameObject.FindGameObjectWithTag ("GameController");
				spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
				moveBehaviourScript mbs = (moveBehaviourScript)assignedToObj.GetComponent ("moveBehaviourScript");

				//Set the mouse as up
				mouseIsDown = false;

				//Make sure that there is more than one point in the trajectory
				if (points.Count > 2) {	
						//Decide if we need to perform a write to file action
						//if (ss.automate_level == SIMILAR) {

						//Send the string to the server
						String stater = stateRepresentation ();
						String sender = String.Concat (stater.Replace ("\n", String.Empty), ",", trajectoryRepresentation ());
						//Debug.Log (sender);
						ss.UDPConnection.sendString (sender);

						//Write the trajectory the user drew to the file
						writeToFile ("Manual:");
			
//								//Get more needed components 
//								GameObject data = GameObject.FindGameObjectWithTag ("GameData");
//								gameData gd = (gameData)data.GetComponent ("gameData");
//
//								//If it's a scripted instance, get the user's choice for most similar trajectory
//								if (gd.scriptedInstance) {
//										GameObject gotc = GameObject.FindGameObjectWithTag ("TimerController");
//										TimerController tc = (TimerController)gotc.GetComponent ("TimerController");
//										StartCoroutine (getUserChoice ());
//								}
						//}
			
						//reset the destination point to continue the current trajectory
						int point2 = points.Count - 1;
						int point1 = points.Count - 2;
						GameObject temp = (GameObject)points [point1];
						float x1 = temp.transform.position.x;
						float y1 = temp.transform.position.y;
						temp = (GameObject)points [point2];
						float x2 = temp.transform.position.x;
						float y2 = temp.transform.position.y;
						float m = (y2 - y1) / (x2 - x1);
						float b = y2 - m * x2;
						float x3 = 0;
						if (x2 - x1 > 0) {
								x3 = 100;
				
						} else {
								x3 = -100;
						}
						float y3 = m * x3 + b;
						mbs.dest = new Vector3 (x3, y3, 0);

						mbs.no_autodraw = true;
				}
		}

		//Remove all points from the path marker
		public void removeAllPoints ()
		{
				if (points != null) {
						for (int i = points.Count - 1; i >= 0; i--) {
								GameObject o = (GameObject)points [i];
								o.SetActive (false);
								points.Remove (o);
								GameObject.Destroy (o);
						}
				}
		}

		//Set lastPosition to the object's current position
		public void setPos ()
		{
				lastPosition = new Vector3 (gameObject.transform.position.x,
								    gameObject.transform.position.y,
								    gameObject.transform.position.z);
		}

		//Return a string representation of the current state
		public String stateRepresentation ()
		{
				//Get the selected ship and destination planet information
				string shipName = assignedToObj.name;
				string destNum = shipName.Substring (4, 1);
				string planetName = ("Planet" + destNum);	
				GameObject assignedPlanet = GameObject.Find (planetName);
		
				//Get start and destination of selected ship
				double start_x = assignedToObj.transform.position.x;
				double start_y = assignedToObj.transform.position.y;
				double dest_x = assignedPlanet.transform.position.x;
				double dest_y = assignedPlanet.transform.position.y;

				double straight_line_traj_dist = Math.Sqrt (Math.Pow (start_x - dest_x, 2) + Math.Pow (start_y - dest_y, 2));

				//Translate state to selected ship at origin
				double trans_x = start_x;
				double trans_y = start_y;
				start_x = start_x - trans_x;
				start_y = start_y - trans_y;
				dest_x = dest_x - trans_x;
				dest_y = dest_y - trans_y;



				//Rotate straight-line trajectory to the X axis
				double tan_val = dest_y / dest_x;
				double theta = 0; // theta is angle in radians up from X axis (+ = Q1, - = Q2)
				if (dest_x > 0) {	// + in the X direction
						theta = Math.Atan (tan_val) * (180 / Math.PI); 
				} else {  			// - in the X direction
						theta = 180 + (Math.Atan (tan_val) * (180 / Math.PI));
				}
				double rotmatx1 = (Math.Cos (-theta * Math.PI / 180));
				double rotmatx2 = (- Math.Sin (-theta * Math.PI / 180));
				double rotmaty1 = (Math.Sin (-theta * Math.PI / 180));
				double rotmaty2 = (Math.Cos (-theta * Math.PI / 180));
				double new_dest_x = rotmatx1 * dest_x + rotmatx2 * dest_y;
				double new_dest_y = rotmaty1 * dest_x + rotmaty2 * dest_y;
				dest_x = new_dest_x;
				dest_y = new_dest_y;
		
				//Create variables for use later
				double min_val = 0.05;
				double[] state_rep = new double[19];
		
				//Log all other ships
				double oship_x, oship_y, temp_x, temp_y;
				foreach (GameObject ship in GameObject.FindGameObjectsWithTag("ship")) {
						if (ship != assignedToObj) {
								oship_x = ship.transform.position.x - trans_x;
								oship_y = ship.transform.position.y - trans_y;
								temp_x = rotmatx1 * oship_x + rotmatx2 * oship_y;
								temp_y = rotmaty1 * oship_x + rotmaty2 * oship_y;
								oship_x = temp_x / dest_x;
								oship_y = temp_y / dest_x;
								double oship_dist = 1 - Math.Sqrt (Math.Pow (oship_x, 2) + Math.Pow (oship_y, 2));
								if (oship_x <= 0) {
										if (oship_y <= 0) {
												state_rep [0] = state_rep [0] + Math.Max (oship_dist, min_val);
										} else {
												state_rep [1] = state_rep [1] + Math.Max (oship_dist, min_val);
										}
								} else if (oship_x >= 1) {
										if (oship_y <= 0) {
												state_rep [2] = state_rep [2] + Math.Max (oship_dist, min_val);
										} else {
												state_rep [3] = state_rep [3] + Math.Max (oship_dist, min_val);
										}
								} else {
										if (oship_y <= 0) {
												state_rep [4] = state_rep [4] + Math.Max (oship_dist, min_val);
										} else {
												state_rep [5] = state_rep [5] + Math.Max (oship_dist, min_val);
										}
								}
						}
				}
		
				//Log all bonuses
				double bonus_x, bonus_y;
				foreach (GameObject bonus in GameObject.FindGameObjectsWithTag("Bonus")) {
						bonus_x = bonus.transform.position.x - trans_x;
						bonus_y = bonus.transform.position.y - trans_y;
						temp_x = rotmatx1 * bonus_x + rotmatx2 * bonus_y;
						temp_y = rotmaty1 * bonus_x + rotmaty2 * bonus_y;
						bonus_x = temp_x / dest_x;
						bonus_y = temp_y / dest_x;
						double bonus_dist = 1 - Math.Sqrt (Math.Pow (bonus_x, 2) + Math.Pow (bonus_y, 2));
						if (bonus_x <= 0) {
								if (bonus_y <= 0) {
										state_rep [6] = state_rep [6] + Math.Max (bonus_dist, min_val);
								} else {
										state_rep [7] = state_rep [7] + Math.Max (bonus_dist, min_val);
								}
						} else if (bonus_x >= 1) {
								if (bonus_y <= 0) {
										state_rep [8] = state_rep [8] + Math.Max (bonus_dist, min_val);
								} else {
										state_rep [9] = state_rep [9] + Math.Max (bonus_dist, min_val);
								}
						} else {
								if (bonus_y <= 0) {
										state_rep [10] = state_rep [10] + Math.Max (bonus_dist, min_val);
								} else {
										state_rep [11] = state_rep [11] + Math.Max (bonus_dist, min_val);
								}
						}
				}
		
				//Log all no-fly zones
				double nfz_x, nfz_y;
				foreach (GameObject nfz in GameObject.FindGameObjectsWithTag("noFlyZone")) {
						nfz_x = nfz.transform.position.x - trans_x;
						nfz_y = nfz.transform.position.y - trans_y;
						temp_x = rotmatx1 * nfz_x + rotmatx2 * nfz_y;
						temp_y = rotmaty1 * nfz_x + rotmaty2 * nfz_y;
						nfz_x = temp_x / dest_x;
						nfz_y = temp_y / dest_x;
						double nfz_dist = 1 - Math.Sqrt (Math.Pow (nfz_x, 2) + Math.Pow (nfz_y, 2));
						if (nfz_x <= 0) {
								if (nfz_y <= 0) {
										state_rep [12] = state_rep [12] + Math.Max (nfz_dist, min_val);
								} else {
										state_rep [13] = state_rep [13] + Math.Max (nfz_dist, min_val);
								}
						} else if (nfz_x >= 1) {
								if (nfz_y <= 0) {
										state_rep [14] = state_rep [14] + Math.Max (nfz_dist, min_val);
								} else {
										state_rep [15] = state_rep [15] + Math.Max (nfz_dist, min_val);
								}
						} else {
								if (nfz_y <= 0) {
										state_rep [16] = state_rep [16] + Math.Max (nfz_dist, min_val);
								} else {
										state_rep [17] = state_rep [17] + Math.Max (nfz_dist, min_val);
								}
						}
				}

				state_rep [18] = straight_line_traj_dist;
				
				//Normalize state representations
				state_rep [0] = (state_rep [0] - 0) / (3.0252 - 0);
				state_rep [1] = (state_rep [1] - 0) / (3.0559 - 0);
				state_rep [2] = (state_rep [2] - 0) / (3.9624 - 0);
				state_rep [3] = (state_rep [3] - 0) / (3.8802 - 0);
				state_rep [4] = (state_rep [4] - 0) / (6.4490 - 0);
				state_rep [5] = (state_rep [5] - 0) / (6.3338 - 0);
				state_rep [6] = (state_rep [6] - 0) / (3.0680 - 0);
				state_rep [7] = (state_rep [7] - 0) / (3.4675 - 0);
				state_rep [8] = (state_rep [8] - 0) / (4.0411 - 0);
				state_rep [9] = (state_rep [9] - 0) / (5.0006 - 0);
				state_rep [10] = (state_rep [10] - 0) / (7.9309 - 0);
				state_rep [11] = (state_rep [11] - 0) / (8.2551 - 0);
				state_rep [12] = (state_rep [12] - 0) / (2.5055 - 0);
				state_rep [13] = (state_rep [13] - 0) / (2.4696 - 0);
				state_rep [14] = (state_rep [14] - 0) / (2.6675 - 0);
				state_rep [15] = (state_rep [15] - 0) / (2.5860 - 0);
				state_rep [16] = (state_rep [16] - 0) / (3.6890 - 0);
				state_rep [17] = (state_rep [17] - 0) / (3.7872 - 0);
				state_rep [18] = (state_rep [18] - 2.5315) / (60.9104 - 2.5315);

				//Compile the state_rep array into a string to return
				string returner = "";
				for (int i = 0; i < state_rep.Length; i++) {
						returner = returner + state_rep [i];
						if (i < state_rep.Length - 1) {
								returner = returner + ",";
						} else {
								returner = returner + "\n";
						}
				}
		
				return(returner);
		}

		//Return a string representation of the current trajectory contained in this path marker
		public String trajectoryRepresentation ()
		{
				//Capture location of all selected ship's path markers
				int numPtsHere = points.Count;
				double[] x_pts = new double[numPtsHere];
				double[] y_pts = new double[numPtsHere];
				for (int i = 0; i < numPtsHere; i++) {
						GameObject temp = (GameObject)points [i];
						x_pts [i] = temp.transform.position.x;
						y_pts [i] = temp.transform.position.y;
				}
		
				//Set up variables to hold the interpolated trajectories
				int avgNumTrajPts = 50;
				double[] interp_x = new double[avgNumTrajPts];
				double[] interp_y = new double[avgNumTrajPts];
		
				//Set the first and last points to be the same
				interp_x [0] = x_pts [0];
				interp_y [0] = y_pts [0];
				interp_x [avgNumTrajPts - 1] = x_pts [x_pts.Length - 1];
				interp_y [avgNumTrajPts - 1] = y_pts [y_pts.Length - 1];

				//Interpolate up to or down to 50 points in the trajectory as necessary
				double ratio = ((double)numPtsHere - 1) / ((double)avgNumTrajPts - 1);
				if (avgNumTrajPts != numPtsHere) {
						for (int i = 1; i <= (avgNumTrajPts-2); i++) {
								double nextPt = ratio * (double)i;
								int ptBefore = (int)Math.Floor (nextPt);
								int ptAfter = (int)Math.Ceiling (nextPt);
								double remainder = nextPt - ptBefore;
								if (remainder == 0) {
										interp_x [i] = x_pts [(int)nextPt];
										interp_y [i] = y_pts [(int)nextPt];
								} else {
										interp_x [i] = x_pts [ptBefore] + remainder * (x_pts [ptAfter] - x_pts [ptBefore]);
										interp_y [i] = y_pts [ptBefore] + remainder * (y_pts [ptAfter] - y_pts [ptBefore]);
								}
						}
						x_pts = interp_x;
						y_pts = interp_y;
				}

				//Convert the point arrays to floats and write them to a file
				float[] x_valsU = new float[50];
				float[] y_valsU = new float[50];
				for (int i = 0; i < 50; i++) {
						x_valsU [i] = (float)x_pts [i];
						y_valsU [i] = (float)y_pts [i];
				}
				writeTrajToFile (x_valsU, y_valsU, 0);
		
				//Get the selected ship and destination planet information
				string shipName = assignedToObj.name;
				string destNum = shipName.Substring (4, 1);
				string planetName = ("Planet" + destNum);	
				GameObject assignedPlanet = GameObject.Find (planetName);
		
				//Get start and destination of selected ship
				double start_x = assignedToObj.transform.position.x;
				double start_y = assignedToObj.transform.position.y;
				double dest_x = assignedPlanet.transform.position.x;
				double dest_y = assignedPlanet.transform.position.y;
		
				//Translate state to selected ship at origin
				double trans_x = start_x;
				double trans_y = start_y;
				start_x = start_x - trans_x;
				start_y = start_y - trans_y;
				dest_x = dest_x - trans_x;
				dest_y = dest_y - trans_y;
		
				//Rotate straight-line trajectory to the X axis
				double tan_val = dest_y / dest_x;
				double theta = 0; // theta is angle in radians up from X axis (+ = Q1, - = Q2)
				if (dest_x > 0) {	// + in the X direction
						theta = Math.Atan (tan_val) * (180 / Math.PI); 
				} else {  			// - in the X direction
						theta = 180 + (Math.Atan (tan_val) * (180 / Math.PI));
				}
				double rotmatx1 = (Math.Cos (-theta * Math.PI / 180));
				double rotmatx2 = (- Math.Sin (-theta * Math.PI / 180));
				double rotmaty1 = (Math.Sin (-theta * Math.PI / 180));
				double rotmaty2 = (Math.Cos (-theta * Math.PI / 180));
				double new_dest_x = rotmatx1 * dest_x + rotmatx2 * dest_y;
				double new_dest_y = rotmaty1 * dest_x + rotmaty2 * dest_y;
				dest_x = new_dest_x;
				dest_y = new_dest_y;
		
				double temp_x, temp_y;
				for (int i = 0; i < x_pts.Length; i++) {
						x_pts [i] = x_pts [i] - trans_x;
						y_pts [i] = y_pts [i] - trans_y;
						temp_x = rotmatx1 * x_pts [i] + rotmatx2 * y_pts [i];
						temp_y = rotmaty1 * x_pts [i] + rotmaty2 * y_pts [i];
						x_pts [i] = temp_x / dest_x;
						y_pts [i] = temp_y / dest_x;
				}
			
				//Compile the state_rep array into a string to return
				string returner = "";
				for (int i = 0; i < x_pts.Length; i++) {
						returner = returner + x_pts [i] + ",";
				}
				for (int i = 0; i < y_pts.Length; i++) {
						returner = returner + y_pts [i];
						if (i < y_pts.Length - 1) {
								returner = returner + ",";
						} else {
								returner = returner + "\n";
						}
				}
				return returner;
		}

		//Wait for a reply to the just-sent UDP packet
		void WaitForReply (int curNum)
		{
				GameObject control = GameObject.FindGameObjectWithTag ("GameController");
				spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
				bool waitForReply = true;
				while (waitForReply) {
						if (ss.UDPConnection.numReceived > curNum) {
								waitForReply = false;
						}
				}
		}

		void writeToFile (String type)
		{
				//Debug.Log ("trying to write");
				//Find the objects to get data involved in game settings
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				gameData gd = (gameData)data.GetComponent ("gameData");
				GameObject control = GameObject.FindGameObjectWithTag ("GameController");
				spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
		
				//Determine if this is one of the baselines or an actual experiment run
				String fileName;
				Boolean goodToWrite = true;
				if (gd.scriptedInstance) {
						if (assignedToObj != ss.selShip) {
								goodToWrite = false;
						}
						fileName = "ScriptedResponse_" + gd.instanceNum + ".txt";
				} else {
						fileName = "Level_" + gd.gameLevel + "_Data.txt";
				}
		
				if (goodToWrite) {
						//Capture relevant data about the present game space
						using (StreamWriter streamer = new StreamWriter(fileName, gd.writtenYet)) {

								moveBehaviourScript test = (moveBehaviourScript)assignedToObj.GetComponent ("moveBehaviourScript");
								//Debug.Log ("~" + test.path.points.Count + ";");

								string shipName = assignedToObj.name;
								string destNum = shipName.Substring (4, 1);
								string planetName = ("Planet" + destNum);	
								GameObject assignedPlanet = GameObject.Find (planetName);
				
								String outString = type;
				
								//Capture a time stamp (seconds since the level started)
								outString = outString + Time.timeSinceLevelLoad.ToString ("f2") + ":";
				
								//Capture (X,Y) location and LifeSpan (in seconds) of all bonuses
								outString = outString + GameObject.FindGameObjectsWithTag ("Bonus").Length + ";";
								foreach (GameObject bonus in GameObject.FindGameObjectsWithTag("Bonus")) {
										outString = outString + bonus.transform.position.x + "," + bonus.transform.position.y + ",";
										bonusHandler bh = (bonusHandler)bonus.GetComponent ("bonusHandler");
										outString = outString + (Time.timeSinceLevelLoad - bh.creationTime).ToString ("f2") + ";";
								}
				
								//Capture Ship ID, Dest. planet, (X,Y) location, rotation angle, and LifeSpan (in seconds) of all other ships
								outString = outString + ":" + (GameObject.FindGameObjectsWithTag ("ship").Length - 1) + "~";
								foreach (GameObject ship in GameObject.FindGameObjectsWithTag("ship")) {
										if (ship != assignedToObj) {
												//Capture info for the ship itself
												collisionHandler ch = (collisionHandler)ship.GetComponent ("collisionHandler");
												outString = outString + ch.shipID + ",";
												outString = outString + ship.name.Substring (4, 1) + ",";
												outString = outString + ship.transform.position.x.ToString ("f2") + "," + ship.transform.position.y.ToString ("f2") + ",";
												outString = outString + ship.transform.eulerAngles.x.ToString ("f2") + "," + ship.transform.eulerAngles.y.ToString ("f2") + ",";
												outString = outString + (Time.timeSinceLevelLoad - ch.creationTime).ToString ("f2") + ";";
						
												//Capture info for the ship's path
												moveBehaviourScript mbs = (moveBehaviourScript)ship.GetComponent ("moveBehaviourScript");
												if(mbs.path != null) { 
													outString = outString + "~" + mbs.path.points.Count + ";";
													string tempString = "";
													for (int i = 0; i < mbs.path.points.Count; i++) {
															GameObject temp = (GameObject)mbs.path.points [i];
														tempString = tempString + temp.transform.position.x.ToString ("f2") + "," + temp.transform.position.y.ToString ("f2") + ";";
													}
													outString = outString + tempString + "~";
												} else {
													outString = outString + "~0;~";
												}
										}
								}
				
								//Capture (X,Y) location and time since last move (in seconds) of No Fly Zones
								outString = outString + ":" + GameObject.FindGameObjectsWithTag ("noFlyZone").Length + ";";
								foreach (GameObject nfz in GameObject.FindGameObjectsWithTag("noFlyZone")) {
										outString = outString + nfz.transform.position.x + "," + nfz.transform.position.y + ",";
										noFlyZoneController nfzc = (noFlyZoneController)nfz.GetComponent ("noFlyZoneController");
										outString = outString + (Time.timeSinceLevelLoad - nfzc.lastMoveTime).ToString ("f2") + ";";
								}
				
								//Capture the number and (X,Y) location of destination planet		
								outString = outString + ":" + destNum + ";" + assignedPlanet.transform.position.x.ToString ("f2") + "," + assignedPlanet.transform.position.y.ToString ("f2") + ":";
				
								//Capture ID, location, rotation, and LifeSpan of selected ship
								collisionHandler assCH = (collisionHandler)assignedToObj.GetComponent ("collisionHandler");
								outString = outString + assCH.shipID + ","
										+ assignedToObj.transform.position.x.ToString ("f2") + "," 
										+ assignedToObj.transform.position.y.ToString ("f2") + "," 
										+ assignedToObj.transform.eulerAngles.x.ToString ("f2") + "," 
										+ assignedToObj.transform.eulerAngles.y.ToString ("f2") + "," 
										+ (Time.timeSinceLevelLoad - assCH.creationTime).ToString ("f2") + ","
										+ (Time.timeSinceLevelLoad - assCH.drawStartTime).ToString ("f2") + ":";
				
								//Capture location of all selected ship's path markers
								int numThisMarkers = points.Count;
								string tempString2 = "";
								for (int i = 0; i < numThisMarkers; i++) {
										GameObject temp = (GameObject)points [i];
										tempString2 = tempString2 + temp.transform.position.x.ToString ("f2") + "," + temp.transform.position.y.ToString ("f2") + ";";
								}
								outString = outString + numThisMarkers + ";" + tempString2 + ":";
				
								//Capture the current game score
								ScoreController sc = (ScoreController)GameObject.FindGameObjectWithTag ("ScoreSystem").GetComponent ("ScoreController");
								outString = outString + sc.score + ":";				
								//Capture the ship spawning rate
								outString = outString + ss.shipSpawnRate + ":";
								//Capture the no fly zone move interval
								outString = outString + ss.noFlyMoveRate + ":";			
								//Capture the bonus spawning rate
								outString = outString + ss.bonusSpawnRate + ":";			
				
								streamer.WriteLine (outString);
								streamer.Close ();
						}
				} else {
						gd.correctShip = false;
						correctShip = false;
				}
				gd.writtenYet = true;
		}

		void writeStringToFile (string write_string)
		{
				//Find the objects to get data involved in game settings
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				gameData gd = (gameData)data.GetComponent ("gameData");
				GameObject control = GameObject.FindGameObjectWithTag ("GameController");
				spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
		
				//Determine if this is one of the baselines or an actual experiment run
				String fileName;
				Boolean goodToWrite = true;
				if (gd.scriptedInstance) {
						if (assignedToObj != ss.selShip) {
								goodToWrite = false;
						}
						fileName = "Response_" + gd.instanceNum + ".txt";
				} else {
						goodToWrite = false;
						fileName = "Level_" + gd.gameLevel + "_Data.txt";
				}
		
				if (goodToWrite) {
						//Capture relevant data about the present game space
						using (StreamWriter streamer = new StreamWriter(fileName, gd.writtenYet)) {
								string outString = write_string;

								streamer.WriteLine (outString);
								streamer.Close ();
						}
				} else {
						gd.correctShip = false;
						correctShip = false;
				}  
		}
	
		void writeTrajToFile (float[] x_vals, float[] y_vals, int which_dot)
		{
				//Find the objects to get data involved in game settings
				GameObject data = GameObject.FindGameObjectWithTag ("GameData");
				gameData gd = (gameData)data.GetComponent ("gameData");
				GameObject control = GameObject.FindGameObjectWithTag ("GameController");
				spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
		
				//Determine if this is one of the baselines or an actual experiment run
				String fileName;
				Boolean goodToWrite = true;
				if (gd.scriptedInstance) {
						if (assignedToObj != ss.selShip) {
								goodToWrite = false;
						}
						fileName = "Response_" + gd.instanceNum + ".txt";
				} else {
						goodToWrite = false;
						fileName = "Level_" + gd.gameLevel + "_Data.txt";
				}
		
				if (goodToWrite) {
						//Capture relevant data about the present game space
						using (StreamWriter streamer = new StreamWriter(fileName, gd.writtenYet)) {
								string outString = "";
								if (which_dot == 0) //The user provided string
										outString = outString + "U=";
								if (which_dot == 1)
										outString = outString + "A=";
								if (which_dot == 2)
										outString = outString + "B=";
								if (which_dot == 3)
										outString = outString + "C=";

								//Capture location of all selected ship's path markers
								string tempString2 = "";
								for (int i = 0; i < x_vals.Length; i++) {
										outString = outString + x_vals [i] + ",";
								}
								for (int i = 0; i < y_vals.Length; i++) {
										outString = outString + y_vals [i];
										if (i == y_vals.Length - 1) 
												outString = outString + ";";
										else
												outString = outString + ",";
								}
								streamer.WriteLine (outString);
								streamer.Close ();
						}
				} else {
						gd.correctShip = false;
						correctShip = false;
				}
				//gd.writtenYet = true;
		}
	
		void OnGUI ()
		{
				//Style for left side slider value indicator
				GUIStyle savingStyle = new GUIStyle (GUI.skin.GetStyle ("Label"));
				savingStyle.fontSize = Screen.height / 10;
				savingStyle.alignment = TextAnchor.MiddleCenter;
		
				dot1_butt_style = new GUIStyle (GUI.skin.GetStyle ("Button"));
				dot1_butt_style.alignment = TextAnchor.MiddleCenter;
				dot1_butt_style.normal.textColor = Color.black;
				dot1_butt_style.normal.background = MakeTex (2, 2, new Color (0f, 1f, 1f, 0.5f));
				
				dot2_butt_style = new GUIStyle (GUI.skin.GetStyle ("Button"));
				dot2_butt_style.alignment = TextAnchor.MiddleCenter;
				dot2_butt_style.normal.textColor = Color.black;
				dot2_butt_style.normal.background = MakeTex (2, 2, new Color (1f, 0f, 1f, 0.5f));
				
				dot3_butt_style = new GUIStyle (GUI.skin.GetStyle ("Button"));
				dot3_butt_style.alignment = TextAnchor.MiddleCenter;
				dot3_butt_style.normal.textColor = Color.black;
				dot3_butt_style.normal.background = MakeTex (2, 2, new Color (0f, 1f, 0f, 0.5f));
		
				int heightUnit = Screen.height / 4;
				int widthUnit = Screen.width / 4;
		
				if (popup) {
						if (correctShip) {
								GUI.Label (new Rect (widthUnit, heightUnit, 2 * widthUnit, 2 * heightUnit), "Saving\nResponse", savingStyle);
						} else {
								GUI.Label (new Rect (widthUnit, heightUnit, 2 * widthUnit, 2 * heightUnit), "Incorrect Ship\n(Must act on\nblinking ship)", savingStyle);
						}
				}

				if (userChoice) {
						GUI.Label (new Rect (widthUnit, heightUnit, 2 * widthUnit, 2 * heightUnit), "Select the most\nsimilar trajectory to\nwhat you just drew", savingStyle);
				} 

				if (display_butts) {
						float h = Screen.height / 25;
						float w = Screen.width / 25;	

						float w2 = w / 2;
						float h2 = h / 2;



						//Button to quit the game
						Vector3 pos1 = Camera.main.WorldToScreenPoint (posDot1);
						if (GUI.Button (new Rect (pos1.x - w2, Screen.height - pos1.y - h2, w, h), "A", dot1_butt_style)) {
								//Debug.Log ("A was clicked");
								writeStringToFile ("A");
								if (waiting) {
										resume = true;
								}
						}
						Vector3 pos2 = Camera.main.WorldToScreenPoint (posDot2);

						if (doButtonsOverlap (pos1, pos2, w2, h2)) {
								//Debug.Log ("1+2 Yes");
								pos2.y = pos1.y - h;
						}
						if (GUI.Button (new Rect (pos2.x - w2, Screen.height - pos2.y - h2, w, h), "B", dot2_butt_style)) {
								//Debug.Log ("B was clicked");
								writeStringToFile ("B");
								if (waiting) {
										resume = true;
								}
						}
						Vector3 pos3 = Camera.main.WorldToScreenPoint (posDot3);
						if (doButtonsOverlap (pos1, pos3, w2, h2)) {
								//Debug.Log ("1+3 Yes");
								pos3.y = pos1.y + h;
								if (doButtonsOverlap (pos2, pos3, w2, h2)) {
										pos3.y = pos2.y + h;
										//Debug.Log ("2+3 Yes");
								}
						} else {
								if (doButtonsOverlap (pos2, pos3, w2, h2)) {
										pos3.y = pos2.y + h;
										//Debug.Log ("2+3 Yes");
								}
						}

						if (GUI.Button (new Rect (pos3.x - w2, Screen.height - pos3.y - h2, w, h), "C", dot3_butt_style)) {
								//Debug.Log ("C was clicked");
								writeStringToFile ("C");
								if (waiting)
										resume = true;
						}
				}
		}

		private bool doButtonsOverlap (Vector3 pos1, Vector3 pos2, float w2, float h2)
		{
				if (((((pos1.x - w2) < (pos2.x + w2)) && ((pos1.x + w2) > (pos2.x + w2))) || (((pos2.x - w2) < (pos1.x + w2)) && ((pos2.x + w2) > (pos1.x + w2)))) && (((pos1.y - h2) < (pos2.y + h2)) && ((pos1.y + h2) > (pos2.y + h2))) || (((pos2.y - h2) < (pos1.y + h2)) && ((pos2.y + h2) > (pos1.y + h2))))
						return true;
				else
						return false;
		}
	
		private Texture2D MakeTex (int width, int height, Color col)
		{
				Color[] pix = new Color[width * height];
				for (int i = 0; i < pix.Length; i++) {
						pix [i] = col;
				}
				Texture2D result = new Texture2D (width, height);
				result.SetPixels (pix);
				result.Apply ();
				return result;
		}

		void moveToShip ()
		{
				Vector3 p = new Vector3 (assignedToObj.transform.position.x, 
								assignedToObj.transform.position.y, 
								0);
				transform.position = p;
		}
		
		public void enqueue (GameObject o)
		{
				if (points == null) {
						points = new ArrayList ();
				}
				points.Add (o);
		}
	
		public Vector3 dequeue ()
		{

				if (!hasPath ()) {
						return new Vector3 (0, 0, 0);
				} 
				GameObject o = (GameObject)points [0];
				points.RemoveAt (0);
				Vector3 tempLocation = new Vector3 (o.transform.position.x, o.transform.position.y, o.transform.position.z);
				Destroy (o);
				return tempLocation;
		}
	
		public bool hasPath ()
		{ 
				if (points == null) {
						points = new ArrayList ();
				}
		
				if (points.Count > 0) {	
						return true;
				}
		
				return false;
		}
	
		public Vector3 firstPoint ()
		{
				if (!hasPath ()) {
						return new Vector3 (0, 0, 0);
				}
				GameObject o = (GameObject)points [0];
				Vector3 tempLocation = new Vector3 (o.transform.position.x, o.transform.position.y, o.transform.position.z);
				return tempLocation;
		}

//		IEnumerator getUserChoice ()
//		{
//				GameObject control = GameObject.FindGameObjectWithTag ("GameController");
//				spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
//				waiting = true;
//
//				//Pause execution for 3 seconds
//				userChoice = true;
//				//Debug.Log ("-- Pausing");
//				Time.timeScale = 0.0f;
//				float pauseEndTime = Time.realtimeSinceStartup + 3;
//				while (Time.realtimeSinceStartup < pauseEndTime) {
//						yield return 0;
//				}
//				userChoice = false;
//				//Debug.Log ("-- Resuming");
//				display_butts = true;
//				//Remove the trajectory the user drew and show the three choices
//				//removeAllPoints ();
//			
//				//Get the selected ship and its destination planet
//				Vector3 currPos = assignedToObj.transform.position;
//				string planetName = ("Planet" + assignedToObj.name.Substring (4, 1));	
//				GameObject assignedPlanet = GameObject.Find (planetName);
//				Vector3 destPos = assignedPlanet.transform.position;
//
//
//				which_dot1 = UnityEngine.Random.Range (1, 4);
//				if (which_dot1 == 1) {
//						which_dot2 = UnityEngine.Random.Range (2, 4);
//				}
//				if (which_dot1 == 2) {
//						which_dot2 = UnityEngine.Random.Range (1, 3);
//						if (which_dot2 == 2) 
//								which_dot2 = 3;
//				}
//				if (which_dot1 == 3) {
//						which_dot2 = UnityEngine.Random.Range (1, 3);
//				}
//				which_dot3 = 6 - (which_dot1 + which_dot2);
//				//Debug.Log (which_dot1 + "  " + which_dot2 + "  " + which_dot3);
//
//				//Get the closest response trajectory in the DB
//				genSimilarTrajectory (destPos, currPos);
//
//				//Draw the straight line trajectory
//				float[] x_vals3 = new float[50];
//				float[] y_vals3 = new float[50];
//				for (int i = 1; i <= 50; i++) {
//						float incdist = (float)i * 0.02f;
//						Vector3 tp = ((destPos - currPos) * (incdist)) + currPos;
//						x_vals3 [i - 1] = tp.x;
//						y_vals3 [i - 1] = tp.y;
//						if (i == 38) {
//								if (which_dot3 == 1) {
//										posDot1 = tp;
//								}
//								if (which_dot3 == 2) {
//										posDot2 = tp;
//								}
//								if (which_dot3 == 3) {
//										posDot3 = tp;
//								}
//	
//						}
//						if (which_dot3 == 1) {
//								GameObject newMarker = (GameObject)Instantiate (dot1, tp, gameObject.transform.rotation);
//						}
//						if (which_dot3 == 2) {
//								GameObject newMarker = (GameObject)Instantiate (dot2, tp, gameObject.transform.rotation);
//						}
//						if (which_dot3 == 3) {
//								GameObject newMarker = (GameObject)Instantiate (dot3, tp, gameObject.transform.rotation);
//						}
//				}
//				writeTrajToFile (x_vals3, y_vals3, which_dot3);
//				removeAllPoints ();
//		
//
//
//				//Pause execution until the user responds
//
//				while (!resume) {
//						yield return 0;
//				}
//
//				popup = true;
//				Time.timeScale = 1;
//				waiting = false;
//
//				//Load the next level
//				GameObject gotc = GameObject.FindGameObjectWithTag ("TimerController");
//				TimerController tc = (TimerController)gotc.GetComponent ("TimerController");
//		
//				StartCoroutine (tc.pauseAndLoadNewLevel (3, 2));
//		}

		// Update is called once per frame
		void Update ()
		{
				if (assignedToObj == null) {
						removeAllPoints ();
						Destroy (gameObject);
				} else {
						GameObject control = GameObject.FindGameObjectWithTag ("GameController");
						spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
						collisionHandler assCH = (collisionHandler)assignedToObj.GetComponent ("collisionHandler");
						moveBehaviourScript mbs = (moveBehaviourScript)assignedToObj.GetComponent ("moveBehaviourScript");
						if (ss.automate_level != NONE && !hasPath () && (Time.timeSinceLevelLoad - assCH.creationTime) >= timeToAA && !pathStarted && !mbs.no_autodraw) {
								
								pathStarted = true;
								removeAllPoints ();
								assCH.drawStartTime = Time.timeSinceLevelLoad;

								//Find the locations of the selected ship and its destination planet
								Vector3 currPos = assignedToObj.transform.position;
								string planetName = ("Planet" + assignedToObj.name.Substring (4, 1));	
								GameObject assignedPlanet = GameObject.Find (planetName);
								Vector3 destPos = assignedPlanet.transform.position;
							
								//Generate a trajectory to the destination planet from the current position
								if (ss.automate_level == SIMILAR) {
										genSimilarTrajectory (destPos, currPos);
								}
								if (ss.automate_level == STRAIGHT) {
										genStraightTrajectory (destPos, currPos);
								}
								if (ss.automate_level == DISSIMILAR) {
										genSimilarTrajectory (destPos, currPos);
								}
								//reset the destination point to continue the current trajectory
								int point2 = points.Count - 1;
								int point1 = points.Count - 2;
								GameObject temp = (GameObject)points [point1];
								float x1 = temp.transform.position.x;
								float y1 = temp.transform.position.y;
								temp = (GameObject)points [point2];
								float x2 = temp.transform.position.x;
								float y2 = temp.transform.position.y;
								float m = (y2 - y1) / (x2 - x1);
								float b = y2 - m * x2;
								float x3 = 0;
								if (x2 - x1 > 0) {
										x3 = 100;
								} else {
										x3 = -100;
								}
								float y3 = m * x3 + b;
								//moveBehaviourScript mbs = (moveBehaviourScript)assignedToObj.GetComponent ("moveBehaviourScript");
								mbs.dest = new Vector3 (x3, y3, 0);
						}

						if (!(Input.GetMouseButton (0) && mouseIsDown)) { //mouse down tracks for just this ship
								//mouse not pressed for this ship. Marker needs to stay with ship
								moveToShip ();
						} else { //marker is being dragged
								//this is the initial click, not the dragging
								if (Input.GetMouseButtonDown (0) && mouseIsDown) {
										return;
								}			
				
								//Get our distance from the last object (i.e. this pathMarker's parent)
								dis = Vector3.Distance (gameObject.transform.position, lastPosition);
				
								//If we are currently dragging a trajectory and we have moved the mouse a set max distance
								//beyond the last recorded position, we instantiate another marker
								if (dis > maxDistance) {

										GameObject newMarker = (GameObject)Instantiate (marker, gameObject.transform.position, gameObject.transform.rotation);
										Vector3 tp = new Vector3 (newMarker.transform.position.x, newMarker.transform.position.y, 0);
										newMarker.transform.position = tp;
					
										enqueue (newMarker);

										setPos ();
								}
						}
				}
		}
}
