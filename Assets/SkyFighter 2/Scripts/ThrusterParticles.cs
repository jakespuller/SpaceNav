/// <summary>
/// /// 12/24/2012
/// FireBallGameStudio.com
/// 
/// Thruster particles.
/// 
/// THis script is designed to turn off and on the thruster particle effect
/// 
/// You set the engions on each engion and the engions for going backwards go on the bottom 4
/// </summary>

using UnityEngine;
using System.Collections;

public class ThrusterParticles : MonoBehaviour 
{
	public GameObject engine;//Allows you to put your engion on in the inspector
	public GameObject engine2;//Allows you to put your engion on in the inspector
	public GameObject engine3;//Allows you to put your engion on in the inspector
	public GameObject engine4;//Allows you to put your engion on in the inspector
	public GameObject engine5;//Allows you to put your engion on in the inspector
	public GameObject engine6;//Allows you to put your engion on in the inspector
	public GameObject engine7;//Allows you to put your engion on in the inspector
	public GameObject engine8;//Allows you to put your engion on in the inspector


void  Update (){

 	if (Input.GetKey ("space"))//If you press the space key turn on the thruster to go foward
 	{
			engine.SetActive (true); //Turns on the particle system
			engine2.SetActive (true);//Turns on the particle system
			engine3.SetActive (true);//Turns on the particle system
			engine4.SetActive (true);//Turns on the particle system
		}
		else
		{
			audio.Play();
			engine.SetActive (false);//Turns off the particle system
			engine2.SetActive (false);//Turns off the particle system
			engine3.SetActive (false);//Turns off the particle system
			engine4.SetActive (false);//Turns off the particle system
		}
		if (Input.GetKey ("x")) 
		{
			engine5.SetActive (true);//Turns on the particle system
			engine6.SetActive (true);//Turns on the particle system
			engine7.SetActive (true);;//Turns on the particle system
			engine8.SetActive (true);//Turns on the particle system
		}
		else
		{
			engine5.SetActive (false);//Turns off the particle system
			engine6.SetActive (false);//Turns off the particle system
			engine7.SetActive (false);//Turns off the particle system
			engine8.SetActive (false);//Turns off the particle system
		}
	}
}
