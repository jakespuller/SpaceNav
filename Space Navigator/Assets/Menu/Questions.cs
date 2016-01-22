using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;


public class Questions : MonoBehaviour {
	
	public class XmlQuestions
	{
		public int numberOfQuestions;
		public Dictionary<string, int> questionTypes;
	}
	
	public XmlQuestions questionsOnFile;
	public XmlDocument doc;
	public GameObject gameData;
	
	private int selGridInt = 0;
	private bool toggleTxt = false;
	private string uniqueID;
	public string[,] trialQuestions;
	public Vector2 scrollposition = Vector2.zero;
	private int j;
	private string innerText = "start text";
	float[] answers;
	float answer = 0.0f;
	void Start()
	{
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		gameData gd = (gameData)data.GetComponent ("gameData");
		if (File.Exists ("questions.xml"))
		{
			//Load XML
			doc = new XmlDocument();
			doc.Load("questions.xml");
			
			//Create new Xmlquestion class
			questionsOnFile = new XmlQuestions();
			//Write number of different questions from questions.xml to class
			questionsOnFile.numberOfQuestions = System.Convert.ToInt32 (doc.DocumentElement.SelectSingleNode ("/questions/numQuestions").InnerText);
			if (questionsOnFile.numberOfQuestions > 0)
			{
				//Parses the questions from the xml file to the dictionary
				XmlNode questionX = doc.DocumentElement.SelectSingleNode ("/questions");
				questionX = questionX.FirstChild.NextSibling;
				questionsOnFile.questionTypes = new Dictionary<string, int>();
				//Adds the question to the question dictionary, and the question type to the dictionary.
				for (int i = 0; i < questionsOnFile.numberOfQuestions; i++)
				{
					string type_s = questionX.SelectSingleNode("questionType").InnerText;
					int type = System.Convert.ToInt32 (type_s);
					string name = questionX.Name;
					questionsOnFile.questionTypes.Add(name, type);
					if (questionX.NextSibling != null)
					{
						questionX = questionX.NextSibling;
					}
				}
				
				//Dictionary key print for testing.
				/*
				Dictionary<string, int>.KeyCollection keys = questionsOnFile.questionTypes.Keys;
				string[] arr = new string[questionsOnFile.numberOfQuestions];
				keys.CopyTo(arr, 0);
				for (int i = 0; i < questionsOnFile.numberOfQuestions; i++)
				{
					print(arr[i]);
				}
				*/
				//Determines the questions wanted to be asked in the experiment_settings, adds them to a list
				
				j = 0;
				try
				{
					//access pram_list at current gamelevel
					IEnumerator enumerator;
					float numPrams = 0;
					int qType = 0;
					if ((gd.gameLevel-1) <= gd.pram_list.numOfTrainingSessions)
					{
						enumerator = gd.pram_list.trainingSessions[gd.gameLevel - 2].GetEnumerator ();
						gd.pram_list.trainingSessions[gd.gameLevel - 2].TryGetValue("numPrams", out numPrams);
						trialQuestions = new string[2, (int) numPrams];
					} else {
						enumerator = gd.pram_list.experimentSessions[gd.gameLevel - gd.pram_list.numOfTrainingSessions - 2].GetEnumerator ();
						gd.pram_list.experimentSessions[gd.gameLevel - gd.pram_list.numOfTrainingSessions - 2].TryGetValue("numPrams", out numPrams);
						trialQuestions = new string[2, (int) numPrams];
					}
					while(enumerator.MoveNext())
					{
						KeyValuePair<string, float> pair = (KeyValuePair<string, float>) enumerator.Current;
						//check if key is in questions
						if (questionsOnFile.questionTypes.ContainsKey(pair.Key))
						{
							//get question type
							questionsOnFile.questionTypes.TryGetValue(pair.Key, out qType);
							trialQuestions[0, j] = pair.Key;
							trialQuestions[1, j] = System.Convert.ToString(qType);
							j++;
						}
	
					}
				} catch(System.NullReferenceException){}
			}
			answers = new float[j];
			for (int i = 0; i < j; i++)
			{
				answers[i] = 0f;
			}
			uniqueID = "Enter Answer.";
		}
	}
	
	void OnGUI()
	{
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		gameData gd = (gameData)data.GetComponent ("gameData");
		
		//Create length units that are dynamic depending on screen size
		int heightUnit = Screen.height / 9;
		int widthUnit = Screen.width / 10;
		
		//Create a save button that writes survey results to a file and moves to the NASA TLX survey
		GUIStyle saveButton = new GUIStyle(GUI.skin.GetStyle("Button"));
		saveButton.fontSize = Screen.height/20;
		saveButton.alignment = TextAnchor.MiddleCenter;
		if (j > 0)
		{
			GUIStyle inputButton = new GUIStyle (GUI.skin.GetStyle ("Button"));
			inputButton.fontSize = Screen.width / 20;
			inputButton.alignment = TextAnchor.MiddleCenter;
			GUIStyle titleStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
			titleStyle.fontSize = Screen.height/20;
			titleStyle.alignment = TextAnchor.UpperCenter;
			//Change to reset levels
			
			scrollposition = GUI.BeginScrollView(new Rect(0, 0, Screen.width, Screen.height), scrollposition, new Rect(0, 0, Screen.width, Screen.height*(float) j+Screen.height));
				
				GUI.Label(new Rect((float)0.25*widthUnit, 8*heightUnit, (float)9.5*widthUnit, 7*heightUnit), "Scroll Down for More Questions!",titleStyle);
				//Generate gui scroll
				string[] result_Arr = new string[j];
				for (int i = 0; i < j; i++)
				{
					result_Arr[i] = handleQuestionType(trialQuestions[1, i], trialQuestions[0, i], i);	
				}
				GUIStyle toggleStyle = new GUIStyle(GUI.skin.GetStyle("Toggle"));
				toggleStyle.fontSize = Screen.height/30;
				toggleStyle.wordWrap = true;
				toggleStyle.alignment = TextAnchor.MiddleCenter;
				toggleStyle.imagePosition = ImagePosition.ImageAbove;
				toggleTxt = GUI.Toggle(new Rect(1*widthUnit, 8*j*heightUnit+Screen.height, 3*widthUnit, 1*heightUnit),toggleTxt,"This box must be checked before answers will save.",toggleStyle);
				//Add save all button.
				if(GUI.Button(new Rect((float)5*widthUnit, 8*j*heightUnit+Screen.height, 3*widthUnit, 1*heightUnit), "Save Answers", saveButton) && toggleTxt) {
					//write file
					writeToFile (result_Arr, trialQuestions, j);
					if (gd.pram_list.practiceSession == 1)
					{
						Application.LoadLevel ("MainMenu");
					}
					else 
					{
					Application.LoadLevel("GameStart");
					}
				}
			
			GUI.EndScrollView();
		} else {
			
			if(GUI.Button (new Rect ((float) 3.4*widthUnit, 3 * heightUnit, 3 * widthUnit, 3 * heightUnit), "No Questions \n Please Continue", saveButton)) {
				//write file
				if (gd.pram_list.practiceSession == 1)
				{
					Application.LoadLevel ("MainMenu");
				} 
				else 
				{
				Application.LoadLevel("GameStart");
				}
			} 
		}
	}

	//Creates the various gui spaces for specified question type 
	string handleQuestionType(string type, string question, int spacing)
	{
		string questionPath, questionAsked;
		XmlNode questionA;
		//Create length units that are dynamic depending on screen size
		int heightUnit = Screen.height / 9;
		int widthUnit = Screen.width / 10;
		
		GUIStyle titleStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
		titleStyle.fontSize = Screen.height/20;
		titleStyle.alignment = TextAnchor.UpperCenter;
		string result = "";
		switch(type)
		{
			//Multiple choice question, One answer to rule them all.
			case "1":
				questionPath = "/questions/"+question;
				questionA = doc.DocumentElement.SelectSingleNode (questionPath);
				questionAsked = questionA.FirstChild.InnerText;
				//Takes questionX to the first possible answer
				questionA = questionA.FirstChild.NextSibling.NextSibling;
				List<string> questionS = new List<string>();
				bool determineNotNull = true;
				while (determineNotNull) 
				{
					questionS.Add(questionA.InnerText);
					//Grabs all the questions into an array.
					if (questionA.NextSibling != null)
					{
						questionA = questionA.NextSibling;
					} else {
						determineNotNull = false;
					}
				}
				string[] questionAnswerArr = questionS.ToArray ();
				//Title of the page
				GUI.Label(new Rect((float)0.25*widthUnit, 0*heightUnit+(float)spacing*Screen.height, (float)9.5*widthUnit, 7*heightUnit), questionAsked,titleStyle);
				
				//Create the selection grid in a button style
				GUILayout.BeginArea(new Rect(1*widthUnit, 3*heightUnit+(float)spacing*Screen.height, 8*widthUnit, 5*heightUnit));
				selGridInt = GUILayout.SelectionGrid(selGridInt, questionAnswerArr, 1, "Button"); 
				GUI.skin.button.fontSize = Screen.height/30;
				GUI.skin.button.alignment = TextAnchor.MiddleLeft;
				GUILayout.EndArea();
				result = questionAnswerArr[selGridInt].Substring (0,1);
				break;
			//Pick a number between x and y
			case "2":
				//Not a completely necessary string, could be eliminated, but it's an artifact of copy and pasting from the first case.
				questionPath = "/questions/"+question;
				//Grab question parameters
				questionAsked = doc.DocumentElement.SelectSingleNode(questionPath+"/question").InnerText;
				string minInput = doc.DocumentElement.SelectSingleNode(questionPath+"/questionMinInput").InnerText;
				string maxInput = doc.DocumentElement.SelectSingleNode(questionPath+"/questionMaxInput").InnerText;
				string minCaption = doc.DocumentElement.SelectSingleNode(questionPath+"/questionMinCaption").InnerText;
				string maxCaption = doc.DocumentElement.SelectSingleNode(questionPath+"/questionMaxCaption").InnerText;
				float minInputf = float.Parse(minInput);
				float maxInputf = float.Parse (maxInput);
				//Styles for different aspects of the page, set up so that they scale with screen size
				
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
				
				//Title of the page
				GUI.Label(new Rect((float)0.25*widthUnit, 0*heightUnit + (float)spacing*Screen.height, 9*widthUnit, 7*heightUnit), questionAsked, titleStyle);
				
				//Labels and horizontal slider for question
				answers[spacing] = GUI.HorizontalSlider(new Rect((float)2*widthUnit, 2*heightUnit+(float)spacing*Screen.height, 5*widthUnit, 1*heightUnit), answers[spacing], minInputf, maxInputf);
				
				GUI.Label (new Rect(2*widthUnit, ((float)2.25*heightUnit+(float)spacing*Screen.height), 5*widthUnit, 1*heightUnit), ((int)answers[spacing]).ToString(), alignCenter);
				GUI.Label (new Rect(2*widthUnit, ((float)2.25*heightUnit+(float)spacing*Screen.height), 5*widthUnit, 1*heightUnit), minInput + " - " + minCaption, alignLeft);
				GUI.Label (new Rect(2*widthUnit, ((float)2.25*heightUnit+(float)spacing*Screen.height), 5*widthUnit, 1*heightUnit), maxInput + " - " + maxCaption, alignRight);
				result = answers[spacing].ToString();
				break;
			//Short answer, blank box to fill in your thoughts. Please do not desicrate the box.
			case "3":
				questionPath = "/questions/"+question;
				questionA = doc.DocumentElement.SelectSingleNode (questionPath);
				questionAsked = questionA.FirstChild.InnerText;
				//Title of the page
				GUI.Label(new Rect((float)0.25*widthUnit, 0*heightUnit + (float)spacing*Screen.height, (float)9.5*widthUnit, 7*heightUnit), questionAsked,titleStyle);
				uniqueID = GUI.TextField (new Rect ((float)0.25 * widthUnit, 2 * heightUnit + (float)spacing*Screen.height, (float) 9.5 * widthUnit, (float) 3 * heightUnit), uniqueID);
				result = uniqueID;
				break;
			default:
				break;
		}
		
		return result;
	}
	//Write the content of the ISA survey to a file
	void writeToFile(string[] results, string[,] questions, int length) {
		GameObject data = GameObject.FindGameObjectWithTag("GameData");
		//Make sure we have a game data object
		if (data != null) {
			//Create a new save file for each level of the game
			bool goodToWrite = false;
			gameData gd = (gameData) data.GetComponent("gameData");
			string title ="";
			string fileName = gd.filePath + "Questions.txt";
			if (!File.Exists (fileName))
			{
				title = "GameLevel, answers, questiontype, question#, answers, questiontype, question#, etc...";
			}
			string outstring = "";
			//Capture the response for this level
			if (File.Exists (fileName))
			{
				goodToWrite = true;
			}
			using(StreamWriter streamer = new StreamWriter(fileName,goodToWrite)) {
				streamer.WriteLine(title);
				streamer.Write((gd.gameLevel-1).ToString() + ",");
				for (int i = 0; i < length; i++)
				{
					int questionLength = questions[0, 1].Length;
					int charInQuestion = 8;
					string question_number = questions[0, 1].Substring (charInQuestion, questionLength-charInQuestion);
					outstring = results[i] +","+questions[1, i]+","+question_number +",";
					streamer.Write(outstring);
				}
				streamer.Write ("\n");
				streamer.Close();
			}
			
		} else {
			Debug.LogError("No game data");
			Application.Quit();
		}
		
	}

}
