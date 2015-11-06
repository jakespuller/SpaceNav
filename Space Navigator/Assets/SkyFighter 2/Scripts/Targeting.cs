/// <summary>
/// /// 12/24/2012
/// FireBallGameStudio.com
/// 
/// Targeting.
/// 
/// This script is desined to allow the targeting icon to change colors depending on weather the enemy you are over is good or bad or neather
/// </summary>

using UnityEngine;
using System.Collections;

public class Targeting : MonoBehaviour {
public GameObject scope; // The main score that is set infront of the ship to allow aiming of ship
private int Range= 70; // The range of the array
	
	void  Update ()
	{
		RayShoot();//Stast the rayShoot array
	}
	void  RayShoot ()
	{
		RaycastHit hit;
		
		Vector3 facingDirection= transform.TransformDirection(Vector3.forward);
		Debug.DrawRay(transform.position, facingDirection * Range, Color.blue);
		scope.GetComponent<Renderer>().material.color = Color.blue;
		
		if (Physics.Raycast(transform.position, facingDirection,out hit, Range) && hit.transform.gameObject.tag == "Enemy") //Checks the tag
		{
			scope.GetComponent<Renderer>().material.color = Color.red; //Sets the scope color red
		
   		}
		
		if (Physics.Raycast(transform.position, facingDirection,out hit, Range) && hit.transform.gameObject.tag == "Alliance") //Checks the tag
   		{
            scope.GetComponent<Renderer>().material.color = Color.green; //Sets the scope color green

	    }
	    
	    if (Physics.Raycast(transform.position, facingDirection,out hit, Range) && hit.transform.gameObject.tag == "Meteor") //Checks the tag
   		{
            scope.GetComponent<Renderer>().material.color = Color.red; //Sets the scope color red
		}

	    
	    if (Physics.Raycast(transform.position, facingDirection,out hit, Range) && hit.transform.gameObject.tag == "AllianceMothership") //Checks the tag
   		{
            scope.GetComponent<Renderer>().material.color = Color.green; //Sets the scope color green
	    }
	    
	    if (Physics.Raycast(transform.position, facingDirection,out hit, Range) && hit.transform.gameObject.tag == "EnemyMothership") //Checks the tag
   		{
            scope.GetComponent<Renderer>().material.color = Color.red;  //Sets the scope color red
	    }
	    
	   if (Physics.Raycast(transform.position, facingDirection,out hit, Range) && hit.transform.gameObject.tag == "EnemySatellite") //Checks the tag
   		{
            scope.GetComponent<Renderer>().material.color = Color.red;  //Sets the scope color red

		}
	    if (Physics.Raycast(transform.position, facingDirection,out hit, Range) && hit.transform.gameObject.tag == "AllianceSatellite") //Checks the tag
   		{
            scope.GetComponent<Renderer>().material.color = Color.green;  //Sets the scope color green
	    }
	    if (Physics.Raycast(transform.position, facingDirection,out hit, Range) && hit.transform.gameObject.tag == "AlliancePlanet") //Checks the tag
   		{
            scope.GetComponent<Renderer>().material.color = Color.green;//Sets the scope color green
	    }
	    if (Physics.Raycast(transform.position, facingDirection,out hit, Range) && hit.transform.gameObject.tag == "EnemyPlanet") //Checks the tag
   		{
            scope.GetComponent<Renderer>().material.color = Color.red; //Sets the scope color red
	    }
	}
}
