/// <summary>
/// 12/24/2012
/// FireBallGameStudio.com
/// 
/// Score.
/// 
/// 
/// This goes on your main ship so that you can get a score.
/// 
/// </summary>
using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour {

	public int score = 0;//Keeps track of your score
	public int points = 0;//Points sent over from enemy health
	public int maxScore = 99999999; //Sets the max of the score so it dont roll out of our gui design
	public void AddScore(int Points)// Receves the points of your points sent here
	{
		score += Points; // Adds the points on
	}
	
	
	void Update () 
	{
			AddjustScore(0); // Updates the points
		
		if(score> maxScore)//Checks if score is greater then max
		{
			score = maxScore;//If it is greater then max then set it to the max	
		}
		
		if(score < 0)// if the score is less then 0
		{
			score = 0;  //Set the score to 0
		}
		
		
	}
	
	public void AddjustScore(int adj)//Keeps the score uptodate
	{
			score += adj;//adjusts the score
	}
	
	void OnGUI()
    {	
			GUI.Label(new Rect(182,73,100,40),"Score "+score.ToString());//Desplays the score
    }
}
