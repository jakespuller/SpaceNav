/// <summary>
/// /// 12/24/2012
/// FireBallGameStudio.com
/// 
/// Flight.
/// 
/// This goes on your main shio this allows the ship to fly in space to use this you must have a Rigidbody.
/// 
/// </summary>
using UnityEngine;
using System.Collections;

public class Flight : MonoBehaviour 
{
	public float rotationSpeed = 50.0f;
	public int flightSpeed = 700;
	public int reverseSpeed = -300;
	public GameObject varGameObject;



	void  FixedUpdate ()
	{
		    varGameObject.GetComponent<ParticleSystem>().startSpeed =15;
			varGameObject.GetComponent<ParticleSystem>().emissionRate =20;
		
		if (Input.GetKey ("space")) //Set the space key as your booster forward,  you can change this as you would like
		{
    		GetComponent<Rigidbody>().AddRelativeForce (Vector3.forward * flightSpeed);
			varGameObject = GameObject.Find("Stars");
            varGameObject.GetComponent<ParticleSystem>().startSpeed =50;
			varGameObject.GetComponent<ParticleSystem>().emissionRate =120;
		}
		
		if (Input.GetKey ("x")) //Sets the x key as your reverse key,  you can change this as you would like
		{
			GetComponent<Rigidbody>().AddRelativeForce (Vector3.forward * reverseSpeed);
		}

		if (Input.GetKey ("a")) //Sets the a as your turn left key,  you can change this as you would like
		{
			GetComponent<Rigidbody>().AddRelativeTorque (0, -500, 0);
		}

		if (Input.GetKey ("d")) //Sets the a as your turn right key,  you can change this as you would like
		{
			GetComponent<Rigidbody>().AddRelativeTorque (0, 500, 0);
		}
	
			float rotation = Input.GetAxis ("Horizontal") * rotationSpeed;//Tilts the ship
			rotation *= Time.deltaTime;
			transform.Rotate (0, 0, rotation);
	
		if ( Vector3.Angle( Vector3.up, transform.up ) < 360) 
		{
			transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.Euler( 0, transform.rotation.eulerAngles.y, 0 ), Time.deltaTime * 1 );
		}
	
		if (Input.GetKey ("s")) //Sets s as your key to tip the nose up, you can change this as your would like
		{
			GetComponent<Rigidbody>().AddRelativeTorque (-400, 0, 0);
		}
		
		if (Input.GetKey ("w")) //Sets w as your key to tip the nose down, you can change this as your would like
		{
			GetComponent<Rigidbody>().AddRelativeTorque (400, 0, 0);
		}
	}
}
