using UnityEngine;
using System.Collections;

public class buttonHandler_MENU : MonoBehaviour {
	public GameObject settingsPanel;
	
	void OnClick ()
	{
       settingsPanel.SendMessage("clickedButton");
	}	
}
