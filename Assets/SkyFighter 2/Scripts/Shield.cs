/// <summary>
/// 12/24/2012
/// FireBallGameStudio.com
/// 
/// Shield.
/// 
/// This script goes on your main ship.
/// 
/// ShieldHealthMax is the max health of your shield
/// CurShieldHealth is the curent health of your shield
/// Shield is where you place the item you are using as your shield on that way this script can turn it off and on
/// Cooldown is the cooldown of shiled preventing it from being able to be used over and over.
/// You can addjust the damage done by each tag in the inspector.
/// </summary>
using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour {

	public int shieldHealthMax = 100;//The shields Max Health.
	public int curShieldHealth = 100;//The shileds Current Health.
	public GameObject shield;//Allows you to assign a Shield in the inspector.
	private int cooldown = 0;//Shows the cooldown of the shield
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
	public Texture2D background;//This allows to add textures to the bars in the inspector.
	public Texture2D foreground;//This allows to add textures to the bars in the inspector.
	Rect box = new Rect(155, 57, 153, 15);//This controlls the possiton of the sheild bars on the screen.

	
	void Update () 
	{
		if(curShieldHealth == 0)//If the shields current health is 0.
		{
			shield.SetActive(false);//Set the shield ot not active
			cooldown = 2;//Turns on the shield cooldown.
			StartCoroutine(Shields());//Starts the cooldown process.
			cooldown = 2;//Makes sure the cooldown is set to 2.
			StartCoroutine(addHealth());//Starts the add health process.
		}
		if(Input.GetKey(KeyCode.F))//Allows activation of the shield
		{
			StartCoroutine(Shields());//Starts the cooldown process.
		}
			AddjustCurrentHealth(0);//Keeps the shields health up to date
	}
	
	public void AddjustCurrentHealth(int adj)//This is where the health will be adjusted
	{
			curShieldHealth += adj;//This is to recieve heals or dammage to the CurHealth.  The number is passed in then assigned to curHealth.
		
		if(curShieldHealth < 0)//Checks if the players health has gone below 0.
		{
			curShieldHealth = 0;// If players health has gone below 0 set it to 0.
			shield.SetActive(false);//Sets the shield to no active.
			cooldown = 2;//Starts the shields cooldown
			StartCoroutine(Shields());//Cooldown process
		}
		if(curShieldHealth> shieldHealthMax)//Checks if player health is higher then maxHealth.
			curShieldHealth = shieldHealthMax;//If players health is higher then maxHealth set it = to maxHeatlh
		
		if(shieldHealthMax <1)//Checks if maxHealth is set to less then 1.
			shieldHealthMax = 1;//If maxHealth is set below 1, this sets it to 1.
		
		
		if(curShieldHealth>40)
		{
			shield.renderer.material.shader = Shader.Find("Rim");//Finds where the shield color changer is.
			shield.renderer.material.SetColor("_RimColor",new Color(0,0,255,255));//Sets the color of the shield.
		}
		if(curShieldHealth <=50)//If the shields healht is less then 76 set shield color to this.
		{
			shield.renderer.material.shader = Shader.Find("Rim");//Finds where the shield color changer is.
			shield.renderer.material.SetColor("_RimColor",new Color(255,112,0,255));//Sets the color of the shield.
		}
		 if(curShieldHealth <=35)//If the shields healht is less then 43 set shield color to this.
		{
			shield.renderer.material.shader = Shader.Find("Rim");//Finds where the shield color changer is.
			shield.renderer.material.SetColor("_RimColor", new Color(255,0,0,255));//Sets the color of the shield.
		}
	}
	void OnCollisionEnter(Collision collision)//Checks for collision.
	{
		if(shield.activeSelf == true)//Checks if the shield is active.
		{
			if (collision.gameObject.tag == "Missile")//checks the tag of what we collided with it if is a missle.
				{
					curShieldHealth -= Missiile;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "Enemy")//Again checks what we collided with if it is a enemy
				{
					curShieldHealth -= Enemy;//It would apply this dammage
				}
	
			 if (collision.gameObject.tag == "Meteor")//Again checks what we collided with if it is a Meteor
				{
					curShieldHealth -= Metoeor;//It would apply this dammage
				}
	
	 		 if (collision.gameObject.tag == "AllianceMothership")//Again checks what we collided with if it is a Alliance mothership
				{
					curShieldHealth -= AMothership;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "Alliance")//Again checks what we collided with if it is a Allance
				{
					curShieldHealth -= Alliance;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "EnemyMothership")//Again checks what we collided with if it is a EnemyMothership
				{
					curShieldHealth -= EMothership;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "EnemySatellite")//Again checks what we collided with if it is a EnemySatellite
				{
					curShieldHealth -= ESatellite;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "AllianceSatellite")//Again checks what we collided with if it is a AllianceSatellite
				{
					curShieldHealth -= ASatellite;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "AlliancePlanet")//Again checks what we collided with if it is a AlliancePlanet
				{
					curShieldHealth -= APlanet;//It would apply this dammage
				}
			 if (collision.gameObject.tag == "EnemyPlanet")//Again checks what we collided with if it is a EnemyPlanet
				{
					curShieldHealth -= EPlanit;//It would apply this dammage
				}
		}
	}
	
	IEnumerator Shields ()//Shield cooldown proceess
	{
	if(cooldown == 0)//if the cooldown is less then or equal to 0 do this.
	{
		cooldown = 2;//Set the cooldown to 2.	
    	shield.SetActive(true);//Set shield to active
		yield return new WaitForSeconds(60);//Leave it active for 60 seconds unless destroyed.
		shield.SetActive(false);//Set shiled to non active.
		yield return new WaitForSeconds(10);
		cooldown = 0;//Set the cooldown to 2.
	
	}
	if(curShieldHealth == 0)
		{
			cooldown = 2;//Set the cooldown to 2.	
   			shield.SetActive (false);//Set shield to active
			yield return new WaitForSeconds(9);
			cooldown = 0;//Set the cooldown to 2.
		}
}
	void OnGUI()//The GUI Display
    {
        GUI.BeginGroup(box);//Starting of GUI Display group.
        {
			GUI.DrawTexture(new Rect(0, 0, box.width, box.height), background, ScaleMode.StretchToFill);//Draws the Background
            GUI.DrawTexture(new Rect(0, 0, box.width*curShieldHealth/shieldHealthMax, box.height), foreground, ScaleMode.StretchToFill);//Draws and changes the front health.
			GUI.Label(new Rect(30,0,100,40),"Shield HP "+curShieldHealth.ToString());//Displays the shields health in text.
        }
        GUI.EndGroup();//Ends the group.
    }
	
	IEnumerator addHealth ()//Add health to the ship shield
	{
		while(curShieldHealth < 100)//While the shields health is less then 100.
		{
			curShieldHealth +=10;	//add 10 health ever 1 second.
			yield return new WaitForSeconds(1);//sets the time to add health over.
			
		}

	}
}