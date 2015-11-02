/// <summary>
/// 12/24/2012
/// FireBallGameStudio.com
/// 
/// Enemy health.
/// 
/// This script goes on all your enemys.
/// 
/// This script contains all the information on the health of the item and its destroys and GUIs.
/// 
/// The EnemyHealthMax is the max health of the enemy or alli you can addjust this as you would like to.
/// The CurEnemyHealth is the state at which the health has reached up to this point
/// The name is where you can type in the name of the item you put this on so it is desplayed on the gui
/// The EnemyHud is the main hud for the enemy desplayed on the GUI
/// The Icon is the icon of the item you are targeting this can be set differently for each item in the scene
/// The Background is the background texture for the health bar
/// The foreground is the forground texture for the health bar
/// The explosion is the explosion item for this item
/// The enemy should be what you put this on
/// The points is the points awareded for destorying this.
/// 
/// </summary>
using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour 
{
	public int EnemyHealthMax = 100; //Enemys Max Health
	public float curEnemyHealth = 100; //Enemys Current Health
	public new string name = ""; //The Name of the items
	public Texture2D EnemyHud; //Texture for main hud
	public Texture2D icon; //The Icon for the ship this is on
	public Texture2D background; //Background of health bar
	public Texture2D foreground; //Front image of health bar
	public GameObject explosion; //The explosino for when it is destroyed
	public GameObject Enemy; //This object	
	public int points = 10;	//The points that destroying this item gives.
	public bool canShow = false; //Decides if you can show this
	bool once = true; //Stops it from showing up multiple times
	public AudioClip explode; //Audio for when the explosion happens
	Rect box = new Rect(19, 70, 150, 17);  //The main addjustment for the GUI
	
	void Update ()//Update the code
	{
		if(curEnemyHealth == 0 && once == true) // Checks if curEnemyHeatlh is 0 and if it is make sure only once
		{
			dead(); //Start the dead function.
		}
		
	}
	public void ApplyDamage(float Damage)  //Function for Receveing and checking Health
	{
			curEnemyHealth -= Damage; // Remove the damge the has bene receved from the curEnemyHealth
		if(curEnemyHealth < 0)//Checks if the players health has gone below 0.
		{
			curEnemyHealth = 0;// If players health has gone below 0 set it to 0.
		}
		
		if(curEnemyHealth> EnemyHealthMax)//Checks if player health is higher then maxHealth.
			curEnemyHealth = EnemyHealthMax;//If players health is higher then maxHealth set it = to maxHeatlh
		
		if(EnemyHealthMax <1)//Checks if maxHealth is set to less then 1.
			EnemyHealthMax = 1;//If maxHealth is set below 1, this sets it to 1.	
	}

	
	void OnGUI()//The GUI
    {
		if(canShow == true) //checks if canShow is true
		{
			GUI.DrawTexture(new Rect (645,0,321,164),EnemyHud); //This desplays the hud for this item
			GUI.DrawTexture(new Rect (815,11,121,141),icon);//This desplays the icon of what you are targeting
			GUI.DrawTexture(new Rect(649, 58, box.width, box.height), background, ScaleMode.StretchToFill); //Draws the background image
            GUI.DrawTexture(new Rect(649, 58, box.width*curEnemyHealth/EnemyHealthMax, box.height), foreground, ScaleMode.StretchToFill); //Draws the health that gets subtracked from
			GUI.Label(new Rect(702,59,148,40), ("HP  ")+curEnemyHealth.ToString());//Displays the shields health in text.
			GUI.Label(new Rect(677,39,148,40), (name));//Displays the shields health in text.

		}
	
    }
	
	void Score()//Takes care our score
	{
			GameObject ScoreGameObject = GameObject.Find("SkyFighter 2"); // Finds the gameObject SkyFighter1
			Score score = ScoreGameObject.GetComponent<Score>(); //Locates the script Score
			score.score += points; //Tells Score to add points
			audio.PlayOneShot(explode); // Plays audio for Explode if we get destoryed
	}
	void dead() //The Dead function
	{
		Enemy.renderer.enabled = false;  //Turns our rendere of so you can see us
		explosion.GetComponentInChildren<ParticleSystem>().Play(); //Turns on the particle system for the explosion
		Destroy(Enemy, 1);//Destroys item when it is time
		once = false;// Sets once to false
		Score(); // Calls the score function
	}
}