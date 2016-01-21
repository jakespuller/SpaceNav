using UnityEngine;
using System.Collections;

public class locationManager : MonoBehaviour {

	bool moved = false;
	
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
	
	void MoveThisTo (GameObject go, Vector3 pointB, float speed, string easetype)
	{
		moved = !moved;
		Hashtable ht = new Hashtable();
		ht.Add("position",pointB);
		ht.Add("time",speed);
		ht.Add("EaseType",iTween.EaseType.easeOutBounce);
		ht.Add("oncomplete","stopAni");
        iTween.MoveTo(go,ht);
	}
		
	void stopAni()
	{
	}
}
