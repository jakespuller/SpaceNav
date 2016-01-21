/// <summary>
///  Fireballgamestudio.com
/// 12/28/2012
/// 
/// Meteor rotate.
/// </summary>
using UnityEngine;
using System.Collections;

public class MeteorRotate : MonoBehaviour {
public float rotationSpeed = 5.0f;//Speed of rotation.
	
	void Update() 
	{
    	transform.Rotate(0,rotationSpeed*Time.deltaTime,0);//Causes rotation,
	}
}
