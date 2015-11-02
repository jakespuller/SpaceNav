/// <summary>
/// 12/24/2012
/// FireBallGameStudio.com
/// 
/// Ship health.
/// 
/// This is the main ships health.
/// 
/// shipHealthMax is the main health of thisp
/// curShipHealth is the ships current health
/// Shield is where you can place in your shield
/// Ship is the main ship
/// Remains is the remains of the ship
/// Player transform is our transform
/// Location 1 is where we want teleported to when we die
/// 
/// background texture is the background texture for the health bar
/// foreground texture is the forground texture for the health bar
/// Hud is your players hud
/// SpaceMan is the man shoing inside your playsr hud
/// 
/// lifes is the number of lifes you currently have
/// 
/// </summary>

using UnityEngine;
using System.Collections;

public class ShipHealth : MonoBehaviour {

	private int shipHealthMax = 100;// The ships Max health
	public int curShipHealth = 100; // The ships current health
	public GameObject shield; //The shield you are using
	public GameObject ship; // The ship itself
	public GameObject remains; // The remains
	public Transform player; // Ships transform
	public Transform location1; // The teleport after death location
	Rect box = new Rect(10, 50, 160, 17);// Starte Cordonates
    public Texture2D background; // Texture for the background of your health bar
    public Texture2D foreground; // Texture for the forground of your health bar
	public Texture2D Hud; // The hud of your health bar
	public Texture2D SpaceMan;  // The spaceman shown in the hud
	public int lifes = 3; // The amount of lifes you have
	public int Missiile = 0;//Allows you to set the damage from this in the insepctor.
	public int Enemy = 0;//Allows you to set the damage from this in the insepctor.
	public int Metoeor = 0;//Allows you to set the damage from this in the insepctor.
	public int AMothership = 0;//Allows you to set the damage from this in the insepctor.
	public int EMothership = 0;//Allows you to set the damage from this in the insepctor.
	public int Alliance = 0;//Allows you to set the damage from this in the insepctor.
	public int ESatellite = 0;//Allows you to set the damage from this in the insepctor.
	public int ASatellite = 0;//Allows you to set the damage from this in the insepctor.
	public int APlanet = 0;//Allows you to set the damage from this in the insepctor.
	public int EPlanit = 0;//Allows you to set the damage from this in the insepctor.

	

	
	void Start()
	{
 		 StartCoroutine(addHealth()); // Starts the health recovery system
	}
	
	
	void Update () 
		{
			AddjustCurrentlife(0); //keeps the current life uptodate
			AddjustCurrentHealth(0); //keeps the current health uptodate
		
		if(curShipHealth == 0)//checks if the curship health is 0
		{
			StartCoroutine(dead());//Starts the death process.
		}
		

		
		}
	public void AddjustCurrentlife(int adj) //Adjusts the currentlife
		{
			lifes+= adj; //add or removes lifes	
		}
		
	public void AddjustCurrentHealth(int adj) //adjusts the current health
		{
		
			curShipHealth += adj;//This is to recieve heals or dammage to the CurHealth.  The number is passed in then assigned to curHealth.
		
		if(curShipHealth < 0)//Checks if the players health has gone below 0.
		{
			curShipHealth = 0;// If players health has gone below 0 set it to 0.
			lifes -=1;

		}
		
		if(lifes <0)
		{
			lifes = 0;
		}

		
		if(curShipHealth> shipHealthMax)//Checks if player health is higher then maxHealth.
			curShipHealth = shipHealthMax;//If players health is higher then maxHealth set it = to maxHeatlh
		
		if(shipHealthMax <1)//Checks if maxHealth is set to less then 1.
			shipHealthMax = 1;//If maxHealth is set below 1, this sets it to 1.	
		}
	

	void OnCollisionEnter(Collision collision) //When collided
	{
		if(shield.activeSelf == false)// Checks to make sure the shield is not active
		{
			if (collision.gameObject.tag == "Missile")//checks the tag of what we collided with it if is a missle.
				{
					curShipHealth -= Missiile;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "Enemy")//Again checks what we collided with if it is a enemy
				{
					curShipHealth -= Enemy;//It would apply this dammage
				}
	
			 if (collision.gameObject.tag == "Meteor")//Again checks what we collided with if it is a Meteor
				{
					curShipHealth -= Metoeor;//It would apply this dammage
				}
	
	 		 if (collision.gameObject.tag == "AllianceMothership")//Again checks what we collided with if it is a Alliance mothership
				{
					curShipHealth -= AMothership;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "Alliance")//Again checks what we collided with if it is a Allance
				{
					curShipHealth -= Alliance;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "EnemyMothership")//Again checks what we collided with if it is a EnemyMothership
				{
					curShipHealth -= EMothership;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "EnemySatellite")//Again checks what we collided with if it is a EnemySatellite
				{
					curShipHealth -= ESatellite;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "AllianceSatellite")//Again checks what we collided with if it is a AllianceSatellite
				{
					curShipHealth -= ASatellite;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "AlliancePlanet")//Again checks what we collided with if it is a AlliancePlanet
				{
					curShipHealth -= APlanet;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "EnemyPlanet")//Again checks what we collided with if it is a EnemyPlanet
				{
					curShipHealth -= EPlanit;//It would apply this dammage
				}

		}
		
	}
	
	void OnGUI()
   	 {
		
       		GUI.DrawTexture(new Rect (0,0,321,164),Hud); //Draws the Hud
			GUI.DrawTexture(new Rect (20,12,121,141),SpaceMan); // Draws the Spaceman
			
            GUI.DrawTexture(new Rect(148, 38, box.width, box.height), background, ScaleMode.StretchToFill); //Draws the background health bar
            GUI.DrawTexture(new Rect(148, 38, box.width*curShipHealth/shipHealthMax, box.height), foreground, ScaleMode.StretchToFill); // Shows the current health
			
			GUI.Label(new Rect(180,37,100,40),"Ship HP "+curShipHealth.ToString());//Displays the shields health in text.
			GUI.contentColor = Color.red;//Sets the text color red

			GUI.Label(new Rect(159,100,100,40),""+lifes.ToString());//Displays the shields health in text.

    }
	
	IEnumerator addHealth ()//Add health to the ship shield
	{
		while (true)
   	   	{ 
   		if (curShipHealth < 100)
		{ // if health < 100...
     		 curShipHealth += 1; // increase health and wait the specified time
     		 yield return new WaitForSeconds(5);
   		} 
		else
		{ // if health >= 100, just yield 
     		 yield return null;
   	    }
 	   }

	}
	
	IEnumerator dead ()// The dead function
	{
		GameObject varGameObject = GameObject.Find("SkyFighter 2"); // Finds the main ship
		varGameObject.GetComponent<Flight>().enabled = false; // Finds the fly script and turns it off if we die
		
		if(lifes == 0) // if lifs are 0 
		{
			ship.SetActive (false); // sets the ship invisible
			remains.SetActive (true);  ///Sets the reamins visible
			
			yield return new WaitForSeconds(4); //waits 3 seconds
			Application.LoadLevel("GameOver");//Load gameover
		}
		
			ship.SetActive(false); // sets the ship invisible
			remains.SetActive (true);  ///Sets the reamins visible
		
			yield return new WaitForSeconds(3); //waits 3 seconds
		
			player.transform.position = location1.transform.position; //changes our position and gets us our of danger
			varGameObject.GetComponent<Flight>().enabled = true;// turns flight script back on

			curShipHealth = shipHealthMax; // Sets our health back to max
			ship.SetActive (true); //sets ship seable
			remains.SetActive(false); // turns remains off
			
	}

}
	
