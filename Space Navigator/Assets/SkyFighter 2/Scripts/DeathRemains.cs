/// <summary>
/// 12/24/2012
/// FireBallGameStudio.com
/// 
/// Death remains.
/// 
/// This script is designed to allow us on death to active the remains and play some explosions and set the ship on fire.  For the the options below you must set the
/// ship fire as the the shipfire1,  the ships explosinog for shipExplode1, the ships remains in the ship opetion and the audio clips for both fire and explode in there
/// options.
/// 
/// This script belongs on the remains object and the remains object must be turned off.
/// 
/// </summary>
using UnityEngine;
using System.Collections;

public class DeathRemains : MonoBehaviour {


	public GameObject shipfire1; //This is the fire that is on this ship when it is destroyed
	public GameObject shipExplode1;//The explosion for the ship
	public GameObject ship; //The ship
	
	public AudioClip fireAudio;
	public AudioClip explodeAudio;



	void Start()
	{
		
		
		if( ship.activeSelf== true) // checks if what is in ship is active
		{
			
			shipExplode1.GetComponentInChildren<ParticleSystem>().Play();
			PlayexplodeAudio ();
		}
		
	}


	void PlayexplodeAudio ()
	{
		GetComponent<AudioSource>().clip = explodeAudio;
		GetComponent<AudioSource>().Play();
	
	}

	
		
	

	
	void Update()
	{
		if( ship.activeSelf == true) //Checks if the ship is active
		{
			shipfire1.GetComponentInChildren<ParticleSystem>().Play(); //turns on the fire
		}
	}

}
