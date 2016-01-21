/// <summary>
/// Fireballgamestuio.com
/// 12/28/2012
/// 
/// Orbit.
/// 
/// This script allows you to set an item to rotate around another.  Currently it is used to allow the meteors to rotate around saturn.  You place this script on the object
/// you wish to rotate around somthing then set the object you want it to rotate around as the target.
/// </summary>
using UnityEngine;
using System.Collections;

public class Orbit : MonoBehaviour 
{
	public float degrees= 10;// The degress that it roates in set time.
	public Transform target;// The object it rotates around
	
	void  Update ()
	{
		transform.RotateAround (target.position, Vector3.up, degrees * Time.deltaTime);//Causes the rotation.
	}
}
