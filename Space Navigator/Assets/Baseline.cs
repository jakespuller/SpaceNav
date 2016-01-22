using UnityEngine;
using System.Collections;
using System.IO;

public class Baseline : MonoBehaviour {
	string time_s, time_e;
	// Use this for initialization
	public static float timer;
	public static bool timeStarted;
	public static float minutes;
	public static float seconds;
	public static char on;
	public Texture2D crosshairTexture;
	public float crosshairScale = 1;
	void Start () {
		//record time of start
		time_s = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond;
		timeStarted = true;
		minutes = 0;
		seconds = 0;
		on = '0';
		//Pulse a message on the screen that instructs them how to perform the baseline?
		//Stare at the cross until it disappears, then close your eyes when it does.
		
	}
	void Update()
	{
		if (timeStarted == true)
		{
			timer += Time.deltaTime;
		}
	}
	
	void OnGUI()
	{
		int time_f = 0;
		//The baseline is finished
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		if (minutes >= 2 && seconds >= 31 && on == '1')
		{
			//end condition
			time_e = System.DateTime.Now.Hour*60*60 + System.DateTime.Now.Minute*60 + System.DateTime.Now.Second + "." + System.DateTime.Now.Millisecond;
			Debug.Log (time_e);
			Debug.Log (time_s);
			writeToFile(time_s, time_e);
			if (data != null)
			{
				gameData gd = (gameData)data.GetComponent ("gameData");
				gd.pram_list.baselineOn = 0;
			} 
			Application.LoadLevel ("GameStart");
		}
		//The first portion of the baseline is finished, reset 
		else if (minutes >= 2 && seconds >= 30 && on == '0')
		{
			on = '1';
			minutes = 0;
			seconds = 0;
			timer = 0;
			GameObject.Find ("Button").SetActive(false);
		} else if (on == '0' && minutes < 1)
		{
			//turn on the plus sign in the screen
			if (seconds < 3)
			{
				GUI.Label (new Rect(Screen.width/2-50, Screen.height/2 - 75,  100, 150), "Stare at the square.");
			}
		} else if (on == '1' && minutes < 1)
		{
			//turn on the plus sign in the screen
			if (seconds < 3)
			{
				GUI.Label (new Rect(Screen.width/2-50, Screen.height/2 - 75,  100, 150), "Close your eyes.");
			}
		}
		//Reassign counters on gui
		minutes = Mathf.Floor (timer/60);
		seconds = Mathf.RoundToInt (timer%60);
				
	}
	void writeToFile(string time_s, string time_e)
	{
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		if (data != null) {
			gameData gd = (gameData)data.GetComponent ("gameData");
			Debug.Log (gd.unique_id);
			string fileName = gd.unique_id + "baseline.txt";
			string append = "Baseline" + " Start,"+time_s+","+time_e;
			File.AppendAllText (fileName, append);
			on = '0';
		}
	}
}

