/// <summary>
/// 12/24/2012
/// FireBallGameStudio.com
/// 
/// Gun.
/// 
/// This script has to go on boxes ligned up with your guns and aimed toward the center to allow your ship to shoot.
/// 
/// </summary>
using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour {
float Range = 1000; // The range of the Raycast
float FireTime = 0; // The firetime
float PhaserCooldown = 0.8f;// This is the cooldown of your phaser preventing rapid fireing
public AudioClip PhaserAudio;// This is the audio for the firing
public Transform PhaserPrefab; // This is the prefab of your bullet
float Damage = 2.5f;// This controlls the damage your are sending to your enemys
public RaycastHit Hit;
	
	void  Update ()
	{
		if( FireTime > 0)
	{
			FireTime -= Time.deltaTime;
		}
		if( FireTime < 0)
		{
			FireTime=0;
		}
		
		if(Input.GetMouseButtonDown (0)){ // Checks if the mouse button has been hit if it has fire the gun
			if ( FireTime == 0)
			{
				PlayPhaserAudio ();
		  		RayShoot();
				FireTime = PhaserCooldown;
				Transform phasers= Instantiate(PhaserPrefab, transform.position, transform.rotation)as Transform;		
				phasers.GetComponent<Rigidbody>().AddForce(transform.forward * 1000);
			}
		}
	}
	
	void  RayShoot ()
	{
		Vector3 DirectionRay= transform.TransformDirection (Vector3.forward);
		Debug.DrawRay(transform.position , DirectionRay * Range , Color.cyan);
		
		if (Physics. Raycast (transform.position , DirectionRay ,out Hit , Range)) 
		{
			if (Hit.rigidbody) 
			{
				Hit.collider.SendMessageUpwards("ApplyDamage" , Damage, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	
	void  PlayPhaserAudio ()
	{
		GetComponent<AudioSource>().PlayOneShot (PhaserAudio);
	}
}