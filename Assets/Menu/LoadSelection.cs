using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LoadSelection : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Button b = gameObject.GetComponent<Button>();
		Text buttonText = b.GetComponentInChildren<Text>();
		string[] nameId = buttonText.text.Split(' ');
		//Adds a listener to each player button
		b.onClick.AddListener(delegate() { buttonClicked(System.Convert.ToInt32(nameId[1])); } );
	}
	
	public void buttonClicked(int id) {
		GameObject data = GameObject.FindGameObjectWithTag ("GameData");
		if (data != null) {
			gameData gd = (gameData)data.GetComponent ("gameData");
			gd.unique_id = id.ToString();
			gd.filePath += "Player" + gd.unique_id +"/";
			if (!Directory.Exists (gd.filePath))
			{
				Directory.CreateDirectory(gd.filePath);
			}
			gd.currentLevelFile = gd.filePath + "current_level.txt";
			StreamWriter streamer = new StreamWriter(gd.currentLevelFile, false);
			gd.playerName = "Player" + gd.unique_id;
			streamer.WriteLine("Player ID:" + gd.unique_id);
			streamer.WriteLine ("Current Level:" + getLevel(gd.filePath, gd));
			gd.pram_list.numPracticeGames = getLevel (gd.filePath+"Practice", gd);
			streamer.WriteLine ("Practice Level:" + gd.pram_list.numPracticeGames);
			streamer.Close();
			gd.newExperimentLoaded = true;
			Application.LoadLevel("GameStart");
		} else {
			Debug.LogError ("No game data object exists");
			Application.Quit ();
		}
	}
	
	private int getLevel(string levelPath, gameData gd) {
		string fileName = levelPath + "Level_1_Data.txt";
		int level = 1;
		while (File.Exists(fileName)) {
			level += 1;
			fileName = levelPath + "Level_"+level+"_Data.txt";
		}
		return level;
	}
}
