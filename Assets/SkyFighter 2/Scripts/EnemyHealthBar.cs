/// <summary>
/// /// 12/24/2012
/// FireBallGameStudio.com
/// 
/// Enemy health bar.
/// 
/// This script is desiged so that when your ship is facing an target it will popup with the data on that item
/// 
/// This goes on EnemyTargeting on your ship, this is an cube with the rendere turned off.
/// </summary>
using UnityEngine;
using System.Collections;

public class EnemyHealthBar : MonoBehaviour {

private int Range= 70;
public new string name = "";
public bool targetWasHit =  false; //public only because you had curTarget as public
private GameObject varGameObject;
private bool colliderHit;
	void  Update ()
	{
		RayShoot();
	}
	void  RayShoot ()
	{
		RaycastHit hit;
		Vector3 facingDirection= transform.TransformDirection(Vector3.forward);
	    Debug.DrawRay(transform.position, facingDirection * Range, Color.blue);
		colliderHit = Physics.Raycast(transform.position, facingDirection,out hit, Range);
		
		if (colliderHit && !targetWasHit)
		{
			targetWasHit = true;
			name = (hit.collider.gameObject.name);
         	varGameObject = GameObject.Find(name);
         	varGameObject.GetComponent<EnemyHealth>().canShow = true;
				
        }
		
		if(!colliderHit && targetWasHit && varGameObject == null)
		{
			targetWasHit = false;	
			name = "";
		}


        else if(!colliderHit && targetWasHit && varGameObject != null)
        {
			varGameObject.GetComponent<EnemyHealth>().canShow = false;
            targetWasHit = false;

        }
	}
}
