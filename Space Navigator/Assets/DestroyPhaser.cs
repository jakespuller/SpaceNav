/// <summary>
/// 12/24/2012
/// FireBallGameStudio.com
/// 
/// Destroy phaser.
/// 
/// This script allows the phaser to be destroyed after three seconds making it so that the bullet does not live forever.
/// 
/// This scirpt goes on the phaser bullet.
/// 
/// </summary>

using UnityEngine;
using System.Collections;

public class DestroyPhaser : MonoBehaviour
{
	void  Start ()//Happens at start
	{
		Destroy (gameObject, 3);//Destorys object it is on after 3 seconds.
	}
}