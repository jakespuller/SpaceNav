﻿using UnityEngine;
using System.Collections;

public class locationManager : MonoBehaviour {

	bool moved = false;



	// Use this for initialization
	void Start () {
	
	}
	
	
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void clickedButton()
	{
		Vector3 temp = new Vector3 (transform.position.x,transform.position.y,transform.position.z);
		
		if(!moved)
		{
			temp.y-=2.2f;
		}else
		{
			temp.y+=2.2f;
		}
		
		
		MoveThisTo (gameObject, temp,.4f, "elastic");
	}
	
	//---------------------------------------------------------------------
	void MoveThisTo (GameObject go, Vector3 pointB, float speed, string easetype)
	{
		//if(moving)return;
		moved = !moved;
		Hashtable ht = new Hashtable();
		ht.Add("position",pointB);
		ht.Add("time",speed);
		ht.Add("EaseType",iTween.EaseType.easeOutBounce);
		ht.Add("oncomplete","stopAni");
		//ht.Add ("orienttopath",false);
		
		
        iTween.MoveTo(go,ht);
	}//---------------------------------------------------------------------

			
	void stopAni()
	{//animation stopped
		//moving = false;
	}
	
	
}
