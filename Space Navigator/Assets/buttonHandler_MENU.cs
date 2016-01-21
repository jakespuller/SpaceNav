using UnityEngine;
using System.Collections;

public class buttonHandler_MENU : MonoBehaviour {
	public GameObject settingsPanel;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnClick ()
	{
       settingsPanel.SendMessage("clickedButton");
	}
	

	
}
