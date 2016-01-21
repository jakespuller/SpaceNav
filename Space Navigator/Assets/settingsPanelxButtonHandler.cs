using UnityEngine;
using System.Collections;

public class settingsPanelxButtonHandler : MonoBehaviour {
	
	
	public GameObject settingsPanel;
	public bool moved = false;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	void OnClick ()
	{
		
		Vector3 temp = new Vector3 (settingsPanel.transform.position.x,settingsPanel.transform.position.y,settingsPanel.transform.position.z);
		
		if(!moved)
		{
		temp.y+=2.2f;
		}else{
		temp.y-=2.2f;
		}
		MoveThisTo (settingsPanel, temp,.4f);
	}
	
		//---------------------------------------------------------------------
	void MoveThisTo (GameObject go, Vector3 pointB, float speed)
	{
		
		moved = !moved;
		Hashtable ht = new Hashtable();
		ht.Add("position",pointB);
		ht.Add("time",speed);
		ht.Add("EaseType",iTween.EaseType.easeOutCubic);
		ht.Add("oncomplete","stopAni");
		//ht.Add ("orienttopath",false);
		
		
        iTween.MoveTo(go,ht);
	}//---------------------------------------------------------------------

			
	void stopAni()
	{//animation stopped
		//moving = false;
	}
}
