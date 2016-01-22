// =====================================================================
// Copyright 2012-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections.Generic;

/* This script demonstrates the usage of MIP.Note() and MIP.Print()
 * 
 * Print and Note works much the same. While Print simply logs to the MIP console, Note stores values within the data readout
 */
public class TestNoteAndPrint : MonoBehaviour {
    string[] anArray = new string[3] { "One", "Two", "Three" };
    List<Color> aList = new List<Color>();
    Dictionary<string,Color> aDict=new Dictionary<string,Color>();

	
	void Start () {
        aList.AddRange(MIP.Properties.SlotColors);
        aDict.Add("Red", new Color(1, 0, 0));
        aDict.Add("Green", new Color(0, 1, 0));
        aDict.Add("Blue", new Color(0, 0, 1));
        aDict.Add("White", new Color(1, 1, 1));
        // If the default console is catched, this will be logged to the MIP console also:
        Debug.Log("Catch me if you can...");
	}

    void Update()
    {
        MIP.Note("Complex Types.Dictionary", aDict);
        MIP.Note("Complex Types.CustomType", MIP.CurrentTime);
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width/2-80, 220, 160, 20), "Print to console")) {
            // Print some stuff to the MIP console
            MIP.PrintWarning("Gossip","Alert! Alert! Data incoming...");
            MIP.Print(aList);
            MIP.Print(anArray); 
            MIP.Print(aDict);
            MIP.Print("Gossip","Ken sent me");
        }
    }

}
