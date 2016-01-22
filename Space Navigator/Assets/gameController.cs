using UnityEngine;
using System.Collections;
using System.IO;

public class gameController : MonoBehaviour {
	
	public GameObject settingsPanel;
	bool moving = false;

	//Move the GameObject to the given point at the given speed
	void MoveThisTo(GameObject go, Vector3 pointB, float speed) {
		if(moving) {
			return;
		}
		moving = true;
		Hashtable ht = new Hashtable();
		ht.Add("position",pointB);
		ht.Add("speed",speed);
		ht.Add("easetype","linear");
		ht.Add("oncomplete","stopAni");
		ht.Add ("orienttopath",false);
        iTween.MoveTo(go,ht);
	}
		
	void stopAni() {
		//animation stopped
		moving = true;
	}
}
