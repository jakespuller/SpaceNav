using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class NASATLX : MonoBehaviour {
	
	//Default NASA TLX response values
	float mentalDemandValue = 50.0f;
	float physicalDemandValue = 50.0f;
	float temporalDemandValue = 50.0f;
	float frustrationLevelValue = 50.0f;
	float effortValue = 50.0f;
	float performanceValue = 50.0f;
	
	private bool toggleTxt = false;
	
	//Create a style for highlighting the different sixth input box
	public GUIStyle highlightBox;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void OnGUI() {
		//Styles for different aspects of the page, set up so that they scale with screen size
		//Title Style
		GUIStyle titleStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
		titleStyle.fontSize = Screen.height/30;
		titleStyle.alignment = TextAnchor.UpperCenter;
		
		//Style for slider value indicator numbers
		GUIStyle alignCenter = new GUIStyle(GUI.skin.GetStyle("Label"));
		alignCenter.fontSize = Screen.height/40;
		alignCenter.alignment = TextAnchor.UpperCenter;
		
		//Style for right side slider value indicator
		GUIStyle alignRight = new GUIStyle(GUI.skin.GetStyle("Label"));
		alignRight.fontSize = Screen.height/40;
		alignRight.alignment = TextAnchor.UpperRight;
		
		//Style for left side slider value indicator
		GUIStyle alignLeft = new GUIStyle(GUI.skin.GetStyle("Label"));
		alignLeft.fontSize = Screen.height/40;
		alignLeft.alignment = TextAnchor.UpperLeft;
		
		//Style for the actual text for each question
		GUIStyle questionStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
		questionStyle.fontSize = Screen.height/40;
		questionStyle.alignment = TextAnchor.UpperLeft;
		
		//Create length units that are dynamic depending on screen size
		int heightUnit = Screen.height / 8;
		int widthUnit = Screen.width / 25;
		
		//Title of the page
		GUI.Label(new Rect(1*widthUnit, 0*heightUnit, 23*widthUnit, 7*heightUnit), "NASA TLX Questionnaire:  Click on each scale at the point that best indicates your experience for the just-completed level", titleStyle);
		
		//Labels and horizontal slider for question 1
		GUI.Label(new Rect(2*widthUnit, 1*heightUnit, 10*widthUnit, 1*heightUnit), 
				  "1. Mental Demand:  How much mental and perceptual activity was required?  Was the task easy or demanding, simple or complex?", questionStyle);
		mentalDemandValue = (int) GUI.HorizontalSlider(new Rect(2*widthUnit, 2*heightUnit, 10*widthUnit, 1*heightUnit), mentalDemandValue, 0.0f, 100.0f);
		GUI.Label (new Rect(2*widthUnit, ((float)2.25*heightUnit), 10*widthUnit, 1*heightUnit), mentalDemandValue.ToString(), alignCenter);
		GUI.Label (new Rect(2*widthUnit, ((float)2.25*heightUnit), 10*widthUnit, 1*heightUnit), "0 - Low", alignLeft);
		GUI.Label (new Rect(2*widthUnit, ((float)2.25*heightUnit), 10*widthUnit, 1*heightUnit), "High - 100", alignRight);
		
		//Labels and horizontal slider for question 2
		GUI.Label(new Rect(2*widthUnit, 3*heightUnit, 10*widthUnit, 1*heightUnit), 
				  "2. Physical Demand:  How much physical activity was required?  Was the task easy or demanding, slack or strenuous?", questionStyle);
		physicalDemandValue = (int) GUI.HorizontalSlider(new Rect(2*widthUnit, 4*heightUnit, 10*widthUnit, 1*heightUnit), physicalDemandValue, 0.0f, 100.0f);
		GUI.Label (new Rect(2*widthUnit, ((float)4.25*heightUnit), 10*widthUnit, 1*heightUnit), "0 - Low", alignLeft);
		GUI.Label (new Rect(2*widthUnit, ((float)4.25*heightUnit), 10*widthUnit, 1*heightUnit), physicalDemandValue.ToString(), alignCenter);
		GUI.Label (new Rect(2*widthUnit, ((float)4.25*heightUnit), 10*widthUnit, 1*heightUnit), "High - 100", alignRight);
		
		//Labels and horizontal slider for question 3
		GUI.Label(new Rect(2*widthUnit, 5*heightUnit, 10*widthUnit, 1*heightUnit), 
				  "3. Temporal Demand:  How much time pressure did you feel due to the pace at which the tasks or task elements occured?  Was the pace slow or rapid?", questionStyle);
		temporalDemandValue = (int) GUI.HorizontalSlider(new Rect(2*widthUnit, 6*heightUnit, 10*widthUnit, 1*heightUnit), temporalDemandValue,0.0f, 100.0f);
		GUI.Label (new Rect(2*widthUnit, ((float)6.25*heightUnit), 10*widthUnit, 1*heightUnit), "0 - Low", alignLeft);
		GUI.Label (new Rect(2*widthUnit, ((float)6.25*heightUnit), 10*widthUnit, 1*heightUnit), temporalDemandValue.ToString(), alignCenter);
		GUI.Label (new Rect(2*widthUnit, ((float)6.25*heightUnit), 10*widthUnit, 1*heightUnit), "High - 100", alignRight);
		
		//Labels and horizontal slider for question 4
		GUI.Label(new Rect(13*widthUnit, 1*heightUnit, 10*widthUnit, 1*heightUnit), 
				  "4. Frustration Level:  How irritated, stressed, and annoyed versus content, relaxed, and complacent did you feel during the task?", questionStyle);
		frustrationLevelValue = (int) GUI.HorizontalSlider(new Rect(13*widthUnit, 2*heightUnit, 10*widthUnit, 1*heightUnit), frustrationLevelValue, 0.0f, 100.0f);
		GUI.Label (new Rect(13*widthUnit, ((float)2.25*heightUnit), 10*widthUnit, 1*heightUnit), "0 - Low", alignLeft);
		GUI.Label (new Rect(13*widthUnit, ((float)2.25*heightUnit), 10*widthUnit, 1*heightUnit), frustrationLevelValue.ToString(), alignCenter);
		GUI.Label (new Rect(13*widthUnit, ((float)2.25*heightUnit), 10*widthUnit, 1*heightUnit), "High - 100", alignRight);
		
		//Labels and horizontal slider for question 5
		GUI.Label(new Rect(13*widthUnit, 3*heightUnit, 10*widthUnit, 1*heightUnit), 
				  "5. Effort:  How hard did you have to work (mentally and physically) to accomplish your level of performance?", questionStyle);		
		effortValue = (int) GUI.HorizontalSlider(new Rect(13*widthUnit, 4*heightUnit, 10*widthUnit, 1*heightUnit), effortValue, 0.0f, 100.0f);
		GUI.Label (new Rect(13*widthUnit, ((float)4.25*heightUnit), 10*widthUnit, 1*heightUnit), "0 - Low", alignLeft);
		GUI.Label (new Rect(13*widthUnit, ((float)4.25*heightUnit), 10*widthUnit, 1*heightUnit), effortValue.ToString(), alignCenter);
		GUI.Label (new Rect(13*widthUnit, ((float)4.25*heightUnit), 10*widthUnit, 1*heightUnit), "High - 100", alignRight);
		
		//Labels and horizontal slider for question 6 (note the different format that is highlighted)
		GUI.Box (new Rect(13*widthUnit, 5*heightUnit, 10*widthUnit, 2*heightUnit),"",highlightBox);
		GUI.Label(new Rect(13*widthUnit, 5*heightUnit, 10*widthUnit, 1*heightUnit), 
				  "6. Performance:  How successful were you in performing the task?  How satisfied were you with your performance?", questionStyle);		
		performanceValue = (int) GUI.HorizontalSlider(new Rect(13*widthUnit, 6*heightUnit, 10*widthUnit, 1*heightUnit), performanceValue, 100.0f, 0.0f);
		GUI.Label (new Rect(13*widthUnit, ((float)6.25*heightUnit), 10*widthUnit, 1*heightUnit), "100 - Good", alignLeft);
		GUI.Label (new Rect(13*widthUnit, ((float)6.25*heightUnit), 10*widthUnit, 1*heightUnit), performanceValue.ToString(), alignCenter);
		GUI.Label (new Rect(13*widthUnit, ((float)6.25*heightUnit), 10*widthUnit, 1*heightUnit), "Poor - 0", alignRight);
	
		//Create a check box that verifies a save action
		GUIStyle toggleStyle = new GUIStyle(GUI.skin.GetStyle("Toggle"));
		toggleStyle.fontSize = Screen.height/30;
		toggleStyle.wordWrap = true;
		toggleStyle.alignment = TextAnchor.UpperCenter;
		toggleStyle.imagePosition = ImagePosition.ImageAbove;
		toggleTxt = GUI.Toggle(new Rect(2*widthUnit, 7*heightUnit, 7*widthUnit, 1*heightUnit),toggleTxt,"This box must be checked before answers will save.",toggleStyle);

		//Create a save button that writes survey results to a file and moves back to the main menu
		//unless we have finished all the levels, if so then quit
		GUIStyle saveButton = new GUIStyle(GUI.skin.GetStyle("Button"));
		saveButton.fontSize = Screen.height/20;
		saveButton.alignment = TextAnchor.MiddleCenter;
		if(GUI.Button(new Rect(10*widthUnit, 7*heightUnit, 5*widthUnit, 1*heightUnit), "Save Answers", saveButton)) {
			if(toggleTxt) {
				writeToFile();	
				
				//Move to main menu or quit game if we've finished all levels
				GameObject data = GameObject.FindGameObjectWithTag("GameData");
				if (data != null) {
					gameData gd = (gameData)data.GetComponent ("gameData");
					if(gd.gameLevel == 18) {
						Application.LoadLevel(4);
					} else {
						Application.LoadLevel(0);
					}
				} else {
					Debug.LogError("No game data");
					Application.Quit();
				}
			}
		}
	}
	
	//Write the content of the NASA TLX survey to a file
	void writeToFile() {
		GameObject data = GameObject.FindGameObjectWithTag("GameData");
		
		//Make sure we have a game data object
		if (data != null) {
			//Create a new save file for each level of the game
			gameData gd = (gameData) data.GetComponent("gameData");
			string fileName = "NASATLX_feedback_" + (gd.gameLevel-1) + ".txt";
			
			//Capture the responses for this level
			string outString = "Level = " + (gd.gameLevel - 1) +", Mental demand = "       + mentalDemandValue + 
							", Physical demand = "   + physicalDemandValue + 
							", Temporal demand = "   + temporalDemandValue + 
							", Frustration level = " + frustrationLevelValue + 
							", Effort = "            + effortValue + 
							", Performance = "       + performanceValue;
			using(StreamWriter streamer = new StreamWriter(fileName,false)) {
				streamer.WriteLine(outString);
				streamer.Close();
			}
		} else {
			Debug.LogError("No game data");
			Application.Quit();
		}
	}
}
