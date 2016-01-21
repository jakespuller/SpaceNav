using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public class pathMarker : MonoBehaviour
{
		//constant declarations for the different automation type
		public const int NONE = 0;
		public const int STRAIGHT = 1;
		public const int EYETRACK = 2;
		public const int SIMILAR = 3;
		public const int DISSIMILAR = 4;
		public const int ZONED = 5;
		//for transfer of game variables
		public GameObject gameData;
		//Create variables for use later
		public Vector3 lastPosition;
		private Vector3 posDot1;
		private Vector3 posDot2;
		private Vector3 posDot3;
		//Path markers
		public GameObject marker;
		public GameObject marker1;
		public GameObject dot1;
		public GameObject dot2;
		public GameObject dot3;
		public ArrayList points;
		public GameObject assignedToObj;
		public bool pathStarted;
		private GUIStyle dot1_butt_style;
		private GUIStyle dot2_butt_style;
		private GUIStyle dot3_butt_style;
		//For file defined automation behavior. Used later for determining the time and number of ships activate automation
		private float lowBottom = 0.0f, lowHigh = 0.0f, midBottom = 0.0f, midHigh = 0.0f, high = 0.0f, lowDelay = 0.0f, midDelay = 0.0f, highDelay = 0.0f, shipNum = 0.0f, automationTapAndDraw = 0.0f;
		//Used for de-activating specific portions of the automation control flow
		private float POWERLEVEL = 9000.0f;

		//Variables we want initialized for every instance
		//Default time to automation
		private double timeToAA = 2;//8.3891;
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
		private float clickTime = -1.0f;
		private float clickDelta = 0.35f;
		private bool doubleClick = false, click = false;
		private Vector3 botLeftPoint;
		private Vector3 topRightPoint;
		private float minX;
		private float minY;
		private float maxX;
		private float maxY;
		// Initialize a new path marker object
		private float[] x_vals1 = new float[50];
		private float[] y_vals1 = new float[50];
		public float max_markers = 50;
		public Vector3 tp;
		public float duration = 3f;
		private float screen_res_width, screen_res_height;
	void Start ()
	{
		//Initializes the points array
		pathStarted = false;
		if (points == null) {
				points = new ArrayList ();
		}
		setPos ();
		//Declares the game data object to access file parameters for automation control
		//Uses a try catch incase the file does not exist.
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		gameData gd2 = (gameData)data.GetComponent ("gameData");
		//Get the game controller in order to act on sub-objects
		GameObject control = GameObject.FindGameObjectWithTag ("GameController");
		spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
		//screen_height and width as detected by eye_tracker. Usually maximum screen resolution
		if (gd2.pram_list.eyeTrackerOn > 0) {
			screen_res_height = ss.TCPconnection.screen_height;
			screen_res_width = ss.TCPconnection.screen_width;
		}
		try {
			if (gd2.gameLevel > gd2.pram_list.numOfTrainingSessions)
			{
				int experiment_level = gd2.gameLevel - (int)gd2.pram_list.numOfTrainingSessions;
				gd2.pram_list.experimentSessions [experiment_level - 1].TryGetValue ("automationTapAndDraw", out automationTapAndDraw);
				gd2.pram_list.experimentSessions [experiment_level - 1].TryGetValue ("lowShipAA1", out lowBottom);
				gd2.pram_list.experimentSessions [experiment_level - 1].TryGetValue ("lowShipAA2", out lowHigh);
				midBottom = lowHigh + 1;
				gd2.pram_list.experimentSessions [experiment_level - 1].TryGetValue ("medShipAA", out midHigh);
				high = midHigh + 1;
				gd2.pram_list.experimentSessions [experiment_level - 1].TryGetValue ("lowDelayAA", out lowDelay);
				gd2.pram_list.experimentSessions [experiment_level - 1].TryGetValue ("medDelayAA", out midDelay);
				gd2.pram_list.experimentSessions [experiment_level - 1].TryGetValue ("highDelayAA", out highDelay);

			}
		} catch (System.NullReferenceException) {
				//If file parameter doesn't exist, then the program catches this, keeping it as the value initially declared
		}
		botLeftPoint  = Camera.main.ScreenToWorldPoint(new Vector3(0,0,10));
		topRightPoint  = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
		minX = botLeftPoint.x;
		minY = botLeftPoint.y;
		maxX = topRightPoint.x;
		maxY = topRightPoint.y;
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
			for (int i = 1; i <= max_markers; i++) {
				float incdist = (float)i * 0.02f;
				tp = ((destPos - currPos) * (incdist)) + currPos;
				float timeDelay = duration / (max_markers - i + 1);
				GameObject newMarker = (GameObject)Instantiate (marker, tp, gameObject.transform.rotation);
				FadeInMarker fadeDelay = newMarker.GetComponent <FadeInMarker>();
				fadeDelay.duration = timeDelay;
				enqueue (newMarker);
				setPos ();
			}
			string drawtime = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second +"." + System.DateTime.Now.Millisecond;
			writeToFile ("SLT,", drawtime);
	}
	//Depending on eye tracker involvement and location of eyes, draw trajector
	void eyeTrackerAgent(Vector3 destPos, Vector3 currPos)
	{
		//Get the game controller in order to act on sub-objects
		GameObject control = GameObject.FindGameObjectWithTag ("GameController");
		spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
		if (ss.eyeTrackerOn > 0) {
			//Determines if the trajectory should be drawn based on eye tracker sensor
			bool outOfRange = false;
			//Rescaling the unity ship coordinates to a percentage of the game scene
			float xPercUnityScreen = (currPos.x/(maxX*2) + .5f);
			float yPercUnityScreen = (currPos.y/(maxY*2) + .5f);
			//Assumes the game uses max resolution, which is the height and width detected by the eye tracker
			float shipscaled_x = xPercUnityScreen*screen_res_width;
			float shipscaled_y = yPercUnityScreen*screen_res_height;
			//Scales the screen using the pythagorean theorem to find hypotenuse of screen
			//Then it uses a magic number to scale the hypotenuse of the screen
			float screen_scale = Mathf.Sqrt((float)(screen_res_width*screen_res_width+screen_res_height*screen_res_height))/5f;
			//Debug.Log (ss.TCPconnection.lastReceivedThread);
			string[] coords_a = ss.TCPconnection.lastReceivedThread.Split(',');
			//distance between ship position and eye position
			int count = 0;
			int numpts = coords_a.Length;
			float percentage_acceptable = (float)numpts/3f*.95f;
			float distance, fpogx, fpogy;
			//print (numpts);
			//print (percentage_acceptable);
			for (int i = 0; (i+3) < numpts; i+=3)
			{
				//FPOGX
				fpogx = float.Parse (coords_a[i+1]);
				//FPOGY
				fpogy = float.Parse (coords_a[i+2]);
				if (fpogx == 0 && fpogy == 0) {continue;}
				distance = Mathf.Sqrt((fpogx-shipscaled_x)*(fpogx-shipscaled_x)+(fpogy-shipscaled_y)*(fpogy-shipscaled_y));
				//print (distance);
				if (screen_scale <= distance)
				{
					count++;
				} else {
					count--;
				}
				//print (count);
				if ((float) count > percentage_acceptable)
				{
					outOfRange = true;
				}
				//print (i + " Interval");
			}
			//print (count);
			if(outOfRange)
			{
				//draw trajectory
				//straight_line_trajectory	to planet from current pos.
				//Vector3 trajArr[] = new Vector3[max_markers];
				for (int i = 1; i <= max_markers; i++) {
					float incdist = (float)i * 0.02f;
					tp = ((destPos - currPos) * (incdist)) + currPos;
					float timeDelay = duration / (max_markers - i + 1);
					GameObject newMarker = (GameObject)Instantiate (marker, tp, gameObject.transform.rotation);
					FadeInMarker fadeDelay = newMarker.GetComponent <FadeInMarker>();
					fadeDelay.duration = timeDelay;
					enqueue (newMarker);
					setPos ();
				}
				string drawtime = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second +"." + System.DateTime.Now.Millisecond;
				writeToFile("ETA,", drawtime);
			}
		} else {
			genStraightTrajectory(destPos, currPos);
		}
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
			for (int i = 0; i < max_markers; i++) {
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
			for (int i = 0; i < max_markers; i++) {
					Vector3 tp = currPos;
					tp.x = x_vals1 [i];
					tp.y = y_vals1 [i];
					float timeDelay = duration / (max_markers - i + 1);
					GameObject newMarker = (GameObject)Instantiate (marker, tp, gameObject.transform.rotation);
					FadeInMarker fadeDelay = newMarker.GetComponent <FadeInMarker>();
					fadeDelay.duration = timeDelay;
					enqueue (newMarker);
					setPos ();
			}

			//Write the first trajectory to the log file
			writeTrajToFile (x_vals1, y_vals1, which_dot1);
			string drawtime = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second +"." + System.DateTime.Now.Millisecond;
			writeToFile ("TSS,", drawtime);
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
					fileName = gd.filePath + "Level_" + gd.gameLevel + "_Data.txt";
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
		//Create straight-line trajectory from the current ship position to destination if the ship is within the specified zone
	void genZonedStraightTrajectory (Vector3 destPos, Vector3 currPos)
	{
		string planetName = ("Planet" + assignedToObj.name.Substring (4, 1));	
		GameObject assignedPlanet = GameObject.Find (planetName);
		float x_diff_abs = 0.0f, y_diff_abs = 0.0f;
		float boardAdjustment = 2.0f;
		Vector3 oldDestPos = destPos;
		bool offScreen = false;
		switch(planetName)
		{
			case "Planet1":
				if (currPos.y <= 0)
				{
					y_diff_abs = Math.Abs (minY - currPos.y);
					if (currPos.x >= 0)
					{
						x_diff_abs = Math.Abs(maxX-currPos.x);
						if (x_diff_abs < y_diff_abs)
						{
							destPos.x = maxX+boardAdjustment;
							destPos.y = currPos.y;
							offScreen = true;
						} else
						{
							destPos.x = currPos.x;
							destPos.y = minY-boardAdjustment;
							offScreen = true;
						}
					} else
					{
						x_diff_abs = Math.Abs (minX - currPos.x);
						if (x_diff_abs < y_diff_abs)
						{
							destPos.x = minX-boardAdjustment;
							destPos.y = currPos.y;
							offScreen = true;
						} else
						{
							destPos.x = currPos.x;
							destPos.y = minY-boardAdjustment;
							offScreen = true;
						}
					}
				}
				break;
			case "Planet2":
				if (currPos.x >= 0)
				{
					x_diff_abs = Math.Abs (maxX - currPos.x);
					if (currPos.y >= 0)
					{
						y_diff_abs = Math.Abs(maxY-currPos.y);
						if (x_diff_abs < y_diff_abs)
						{
							destPos.x = maxX+boardAdjustment;
							destPos.y = currPos.y;
							offScreen = true;
						} else
						{
							destPos.x = currPos.x;
							destPos.y = maxY+boardAdjustment;
							offScreen = true;
						}
					} else
					{
						y_diff_abs = Math.Abs(minY-currPos.y);
						if (x_diff_abs < y_diff_abs)
						{
							destPos.x = maxX+boardAdjustment;
							destPos.y = currPos.y;
							offScreen = true;
						} else
						{
							destPos.x = currPos.x;
							destPos.y = minY-boardAdjustment;
							offScreen = true;
						}
					}
				}
				break;
			case "Planet3":
				if (currPos.x <= 0)
				{
					x_diff_abs = Math.Abs (minX - currPos.x);
					if (currPos.y >= 0)
					{
						y_diff_abs = Math.Abs(maxY-currPos.y);
						if (x_diff_abs < y_diff_abs)
						{
							destPos.x = minX-boardAdjustment;
							destPos.y = currPos.y;
							offScreen = true;
						} else
						{
							destPos.x = currPos.x;
							destPos.y = maxY+boardAdjustment;
							offScreen = true;
						}
					} else
					{
						y_diff_abs = Math.Abs(minY-currPos.y);
						if (x_diff_abs < y_diff_abs)
						{
							destPos.x = minX-boardAdjustment;
							destPos.y = currPos.y;
							offScreen = true;
						} else
						{
							destPos.x = currPos.x;
							destPos.y = minY-boardAdjustment;
							offScreen = true;
						}
					}
				}
				break;
			case "Planet4":
				if (currPos.y >= 0)
				{
					y_diff_abs = Math.Abs (maxY - currPos.y);
					if (currPos.x >= 0)
					{
						x_diff_abs = Math.Abs(maxX-currPos.x);
						if (x_diff_abs < y_diff_abs)
						{
							destPos.x = maxX+boardAdjustment;
							destPos.y = currPos.y;
							offScreen = true;
						} else
						{
							destPos.x = currPos.x;
							destPos.y = maxY+boardAdjustment;
							offScreen = true;
						}
					} else
					{
						x_diff_abs = Math.Abs (minX - currPos.x);
						if (x_diff_abs < y_diff_abs)
						{
							destPos.x = minX-boardAdjustment;
							destPos.y = currPos.y;
							offScreen = true;
						} else
						{
							destPos.x = currPos.x;
							destPos.y = maxY+boardAdjustment;
							offScreen = true;
						}
					}
				}
				break;
		}
		Vector3[] trajArr = new Vector3[50];
		//Define the straight line trajectory points in an array	
		
		for (int i = 1; i <= max_markers; i++) {
			float incdist = (float)i * 0.02f;
			trajArr[i-1] = ((destPos - currPos) * (incdist)) + currPos;
		}
		if (!offScreen)
		{
			//Collision avoidance
		}
		for (int i = 1; i <= max_markers; i++) {
			Vector3 tp = trajArr[i-1];
			float timeDelay = duration / (max_markers - i + 1);
			GameObject newMarker = (GameObject)Instantiate (marker, tp, gameObject.transform.rotation);
			FadeInMarker fadeDelay = newMarker.GetComponent <FadeInMarker>();
			fadeDelay.duration = timeDelay;
			enqueue (newMarker);
			setPos ();
		}
		string drawtime = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second +"." + System.DateTime.Now.Millisecond;
		writeToFile ("Zoned,", drawtime);
	}

	//Mouse button is pressed on this object 
	void OnMouseDown ()
	{
			//Get the game controller in order to act on sub-objects
			GameObject control = GameObject.FindGameObjectWithTag ("GameController");
			spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
			collisionHandler ch = (collisionHandler)assignedToObj.GetComponent ("collisionHandler");
			moveBehaviourScript mbs = (moveBehaviourScript)assignedToObj.GetComponent ("moveBehaviourScript");
			GameObject data = GameObject.FindGameObjectWithTag ("GameData");
			gameData gd = (gameData)data.GetComponent ("gameData");
			//Make sure we let the moveBehaviourScript know to start following path
			pathStarted = true;
	
			//Get rid of any old points
			removeAllPoints ();
	
			//Keep track of the fact we started drawing
			mouseIsDown = true;
	
			//Record when we started drawing
			ch.drawStartTime = Time.timeSinceLevelLoad;
			
			//Checks for automation tap and draw parameter set
			if (automationTapAndDraw > 0)
			{
				//Checks to see if there has been one click and if that click is within the range of the previous click
				if (click && Time.time <= (clickTime + clickDelta) && gd.automation_level != 0) {
					click = false;
					Automation(ss, ch, mbs);
				}
				else {
					click = true;
					clickTime = Time.time;
				}
			}
	}

	//Mouse button is released
	void OnMouseUp ()
	{
			//Get needed components 
			moveBehaviourScript mbs = (moveBehaviourScript)assignedToObj.GetComponent ("moveBehaviourScript");

			//Set the mouse as up
			mouseIsDown = false;

			//Make sure that there is more than one point in the trajectory
			if (points.Count > 2) {	
					//Write the trajectory the user drew to the file
					string drawtime = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second +"." + System.DateTime.Now.Millisecond;
					writeToFile ("Manual,", drawtime);
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
	
	void writeToFile (String type, String drawtime)
	{
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		gameData gd = (gameData)data.GetComponent ("gameData");
		collisionHandler assCH = (collisionHandler)assignedToObj.GetComponent ("collisionHandler");
		String fileName = gd.fileName;//gd.unique_id+"Level_"+gd.gameLevel+"_Data.txt";
		using (StreamWriter streamer = new StreamWriter(fileName, gd.writtenYet))
		{
			//Ship trajectory, ship ID, timestamp, automation type
			int numThisMarkers = points.Count;
			string tempString2 = "";
			for (int i = 0; i < numThisMarkers; i++) {
				GameObject temp = (GameObject)points [i];
				float x_scaled = (temp.transform.position.x/(maxX*2) + .5f);
				float y_scaled = (temp.transform.position.y/(maxY*2) + .5f);
				tempString2 += x_scaled + "," + y_scaled + ",";

			}
			//File.AppendAllText (fileName, (type+assCH.shipID+","+drawtime+","+tempString2));
			streamer.WriteLine(type+assCH.shipID+","+drawtime+","+tempString2);
		}
		gd.writtenYet = true;
	}

	//Used to move the marker to the ship
	void moveToShip ()
	{
			Vector3 p = new Vector3 (assignedToObj.transform.position.x, 
							assignedToObj.transform.position.y, 
							0);
			transform.position = p;
	}
	//Adds the gameObject to the points array, used for adding the marker to the ship path
	public void enqueue (GameObject o)
	{
			if (points == null) {
					points = new ArrayList ();
			}
			points.Add (o);
	}
	//Not used in this method, but removes a gameObject (a marker) from the points array
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
	//Checks if a ship has a path
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
	//returns the first point in the path array
	public Vector3 firstPoint ()
	{
			if (!hasPath ()) {
					return new Vector3 (0, 0, 0);
			}
			GameObject o = (GameObject)points [0];
			Vector3 tempLocation = new Vector3 (o.transform.position.x, o.transform.position.y, o.transform.position.z);
			return tempLocation;
	}

	//Engages the automation
	void Automation (spawnSystem ss, collisionHandler assCH, moveBehaviourScript mbs)
	{
			removeAllPoints ();
			assCH.drawStartTime = Time.timeSinceLevelLoad;
			pathStarted = true;
			//Find the locations of the selected ship and its destination planet
			Vector3 currPos = assignedToObj.transform.position;
			string planetName = ("Planet" + assignedToObj.name.Substring (4, 1));	
			GameObject assignedPlanet = GameObject.Find (planetName);
			Vector3 destPos = assignedPlanet.transform.position;
			//Generate a trajectory to the destination planet from the current position
			if (ss.automate_level == STRAIGHT) {
				genStraightTrajectory (destPos, currPos);
			} else if (ss.automate_level == EYETRACK) {
				eyeTrackerAgent(destPos, currPos);
			} else if (ss.automate_level == SIMILAR || ss.automate_level == DISSIMILAR) {
				genSimilarTrajectory(destPos, currPos);		
			} else if (ss.automate_level == ZONED) {
				genZonedStraightTrajectory(destPos, currPos);
			}
			//reset the destination point to continue the current trajectory
			if (hasPath ())
			{
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
			}
	}
	//Used in determining if the ship needs to obey the system in the file input from experiment_Settings.xml
	//Determines when automation activates by the number of ships on screen followed by the delay period
	void automationControlFlow (spawnSystem ss, collisionHandler assCH, moveBehaviourScript mbs, float shipTime, float shipNum)
	{
			//All three are active
			if (lowDelay < POWERLEVEL && midDelay < POWERLEVEL && highDelay < POWERLEVEL) {
					if (lowDelay == 0.0f && midDelay == 0.0f && highDelay == 0.0f) {
							if (lowBottom == 0.0f && lowHigh == 0.0f && midHigh == 0.0f) {
									//This is a default situation, where the file was not found and the paremeters are all 0
									Automation (ss, assCH, mbs);
									return;
							}
					}
					//Checks for number of ships and resulting delay for those ships
					if (shipNum <= lowHigh && shipNum >= lowBottom) {

							if (shipTime >= lowDelay) {
									Automation (ss, assCH, mbs);
									return;
							}
					} else if (shipNum <= midHigh && shipNum >= midBottom) {
							if (shipTime >= midDelay) {
									Automation (ss, assCH, mbs);
									return;
							}
					} else if (shipNum >= high) {
							if (shipTime >= highDelay) {
									Automation (ss, assCH, mbs);
									return;
							}
					}
			}
	//Only high range is active
	else if (lowDelay > POWERLEVEL && midDelay > POWERLEVEL && highDelay < POWERLEVEL) {
					if (shipNum >= high) {
							if (shipTime >= highDelay) {
									Automation (ss, assCH, mbs);
									return;
							}
					}
			}
	//Only mid range is active
	else if (lowDelay > POWERLEVEL && midDelay < POWERLEVEL && highDelay > POWERLEVEL) {
					if (shipNum >= midBottom && shipNum <= midHigh) {
							if (shipTime >= midDelay) {
									Automation (ss, assCH, mbs);
									return;
							}
					}
			}
	//Only low range is active
	else if (lowDelay < POWERLEVEL && midDelay > POWERLEVEL && highDelay > POWERLEVEL) {
					if (shipNum >= lowBottom && shipNum <= lowHigh) {
							if (shipTime >= lowDelay) {
									Automation (ss, assCH, mbs);
									return;
							}
					} 
			}
	//Mid and high ranges are active
	else if (lowDelay > POWERLEVEL && midDelay < POWERLEVEL && highDelay < POWERLEVEL) {
					if (shipNum >= midBottom && shipNum <= midHigh) {
							if (shipTime >= midDelay) {
									Automation (ss, assCH, mbs);
									return;
							}
					} else if (shipNum >= high) {
							if (shipTime >= highDelay) {
									Automation (ss, assCH, mbs);
									return;
							}
					}
			}
	//Low and high range are active
	else if (lowDelay < POWERLEVEL && midDelay > POWERLEVEL && highDelay < POWERLEVEL) {
					if (shipNum >= lowBottom && shipNum <= lowHigh) {
							if (shipTime >= lowDelay) {
									Automation (ss, assCH, mbs);
									return;
							}
					} else if (shipNum >= high) {
							if (shipTime >= highDelay) {
									Automation (ss, assCH, mbs);
									return;
							}
					}
			}
	//Low and mid range are active
	else if (lowDelay < POWERLEVEL && midDelay < POWERLEVEL && highDelay > POWERLEVEL) {
					if (shipNum >= lowBottom && shipNum <= lowHigh) {
							if (shipTime >= lowDelay) {
									Automation (ss, assCH, mbs);
									return;
							}
					} else if (shipNum >= midBottom && shipNum <= midHigh) {
							if (shipTime >= midDelay) {
									Automation (ss, assCH, mbs);
									return;
							}
					}
			}
	//None are active
	else if (lowDelay > POWERLEVEL && midDelay > POWERLEVEL && highDelay > POWERLEVEL) {
					return;
			} 
	}
	
	// Update is called once per frame
	void Update ()
	{
			if (assignedToObj == null) {
					removeAllPoints ();
					Destroy (gameObject);
			} else {
					//Initialization for importing information from other scripts within the game
					GameObject control = GameObject.FindGameObjectWithTag ("GameController");
					spawnSystem ss = (spawnSystem)control.GetComponent ("spawnSystem");
					collisionHandler assCH = (collisionHandler)assignedToObj.GetComponent ("collisionHandler");
					moveBehaviourScript mbs = (moveBehaviourScript)assignedToObj.GetComponent ("moveBehaviourScript");
					GameObject data = GameObject.FindGameObjectWithTag ("GameData");
					gameData gd2 = (gameData)data.GetComponent ("gameData");
					//Determines if the automation should begin.
					if (ss.automate_level != NONE && !hasPath () && !pathStarted && !mbs.no_autodraw) {
							float shipTime = Time.timeSinceLevelLoad - assCH.creationTime;
							float numShips = 0;
							try { numShips = gd2.pram_list.numOfShips; } catch (System.NullReferenceException) {};
							automationControlFlow (ss, assCH, mbs, shipTime, numShips);
					}
					//Used for human interaction with mover helper
					if (!(Input.GetMouseButton (0) && mouseIsDown)) { //mouse down tracks for just this ship
							//mouse not pressed for this ship. Marker needs to stay with ship
							moveToShip ();
					} else { //marker is being dragged
							//this is the initial click, not the dragging
							if (Input.GetMouseButtonDown (0) && mouseIsDown)
							{
								if (click && Time.time > (clickTime + clickDelta))
								{
									click = false;
								}
								return;
							}
							//Get our distance from the last object (i.e. this pathMarker's parent)
							dis = Vector3.Distance (gameObject.transform.position, lastPosition);
			
							//If we are currently dragging a trajectory and we have moved the mouse a set max distance
							//beyond the last recorded position, we instantiate another marker
							if (dis > maxDistance) {
				
									GameObject newMarker = (GameObject)Instantiate (marker1, gameObject.transform.position, gameObject.transform.rotation);
									Vector3 tp = new Vector3 (newMarker.transform.position.x, newMarker.transform.position.y, 0);
									newMarker.transform.position = tp;
				
									enqueue (newMarker);
				
									setPos ();
							}
					}
			}
	}
}