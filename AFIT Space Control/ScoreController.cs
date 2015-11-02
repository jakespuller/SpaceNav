using UnityEngine;
using System.Collections;

public class ScoreController : MonoBehaviour {
	public long score;
	public GUIText scoreLabel;
	float startTime;
	// Use this for initialization
	void Start () {
	  score=0;
	  startTime = Time.time;
	}
		
	public void addPoints(int points) {
	   score += points;	
	}

   	public void reset() {
	   score =0;	
	}

	
	// Update is called once per frame
	void Update() {
	   scoreLabel.text="SCORE\n"+score;
	}
}
