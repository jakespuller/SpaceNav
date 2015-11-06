using UnityEngine;
using System.Collections;
using System.IO;

public class NASATLXCompare : MonoBehaviour {
	
	private int maxComplete;
	public GameObject gameData;

	private ArrayList aVals;
	private ArrayList bVals;
	private int iterator;
	private int[] order;
	private bool swap;
	private string fileName = "NASA_TLX_Comp.txt";
	
	private string[] compare = {"Mental Demand","Physical Demand","Temporal Demand","Effort","Frustration","Performance"};
	
	// Use this for initialization
	void Start() {
		aVals = new ArrayList();
		bVals = new ArrayList();
		iterator = 0;
		for(int i = 0; i < 5; i++) {
			for(int j = i+1; j < 6; j++) {
				aVals.Add(i);
				bVals.Add(j);
			}
		}
		
		ArrayList x = new ArrayList();
		
		
		for(int i = 0; i < aVals.Count; i++) {
			x.Add(i);
		}
		order = new int[aVals.Count];
		for(int i = 0; i < aVals.Count; i++) {
			int rand = (int) UnityEngine.Random.Range(0, x.Count);
			order[i] = (int) x[rand];
			x.RemoveAt(rand);
		}
		
		swap = setSwap();

		using(StreamWriter streamer = new StreamWriter(fileName, false)) {
			streamer.Close();
		}
	}
		
	bool setSwap() {
		int rand = (int) UnityEngine.Random.Range(0, 1);
		if(rand == 0) {
			return true;
		} else { 
			return false;
		}
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void OnGUI() {
		//Style for save button, set up so that it scales with screen size	
		GUIStyle inputButton = new GUIStyle(GUI.skin.GetStyle("Button"));
		inputButton.fontSize = Screen.width/20;
		inputButton.alignment = TextAnchor.MiddleCenter;
		
		//Create length units that are dynamic depending on screen size
		int heightUnit = Screen.height / 16;
		int widthUnit = Screen.width / 16;
		
		//Title of the page
		GUI.Box(new Rect(3*widthUnit, 1*heightUnit, 10*widthUnit, 14*heightUnit), "Which scale was most relevant to\nworkload in the Space Navigator game?");
		GUI.skin.box.fontSize = Screen.width/30;

		
		//Button to quit the game
		if(!swap){
			if(iterator < aVals.Count && GUI.Button(new Rect(4*widthUnit, 4*heightUnit, 8*widthUnit, 4*heightUnit), compare[(int)aVals[order[iterator]]], inputButton)) {
				using(StreamWriter streamer = new StreamWriter(fileName, true)) {
					streamer.WriteLine(compare[(int)aVals[order[iterator]]] + " chosen over " + compare[(int)bVals[order[iterator]]]);
					streamer.Close();
				}
				iterator++;
				swap = setSwap();
				if(iterator >= aVals.Count) {
					Application.LoadLevel(0);
				}
			}
			if(iterator < aVals.Count && GUI.Button(new Rect(4*widthUnit, 10*heightUnit, 8*widthUnit, 4*heightUnit), compare[(int)bVals[order[iterator]]], inputButton)) {
				using(StreamWriter streamer = new StreamWriter(fileName, true)) {
					streamer.WriteLine(compare[(int)bVals[order[iterator]]] + " chosen over " + compare[(int)aVals[order[iterator]]]);
					streamer.Close();
				}
				iterator++;	
				swap = setSwap();
				if(iterator >= bVals.Count) {
					Application.LoadLevel(0);
				}
			}
		} else {
			if(iterator < aVals.Count && GUI.Button(new Rect(4*widthUnit, 10*heightUnit, 8*widthUnit, 4*heightUnit), compare[(int)aVals[order[iterator]]], inputButton)) {
				using(StreamWriter streamer = new StreamWriter(fileName, true)) {
					streamer.WriteLine(compare[(int)aVals[order[iterator]]] + " chosen over " + compare[(int)bVals[order[iterator]]]);
					streamer.Close();
				}
				iterator++;
				swap = setSwap();
				if(iterator >= aVals.Count) {
					Application.LoadLevel(0);
				}
			}
			if(iterator < aVals.Count && GUI.Button(new Rect(4*widthUnit, 4*heightUnit, 8*widthUnit, 4*heightUnit), compare[(int)bVals[order[iterator]]], inputButton)) {
				using(StreamWriter streamer = new StreamWriter(fileName, true)) {
					streamer.WriteLine(compare[(int)bVals[order[iterator]]] + " chosen over " + compare[(int)aVals[order[iterator]]]);
					streamer.Close();
				}
				iterator++;
				swap = setSwap();
				if(iterator >= bVals.Count) {
					Application.LoadLevel(0);
				}
			}
		}

	}

	
}
