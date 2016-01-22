// =====================================================================
// Copyright 2012-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections.Generic;

/* This script demonstrates the usage of MIP.Watch()
 * 
 * Watcher slots are a special case of events. They will only create an event if the content being watched changes
 * to let you reproduce the state of a variable over time.
 * 
 * Internally the content will be converted to strings (arrays and lists will be handled to produce some nice readable
 * output). So if you want to check the state of some custom classes, override ToString() and produce a useful output.
 * 
 */

public class TestWatch : MonoBehaviour {
    public bool aBool;
    public Vector2 aVector;
    public List<Vector2> aList = new List<Vector2>();


	
	// Update is called once per frame
	void Update () {
        // let's watch aBool. If it's value changes, an event will be raised:
        MIP.Watch(name+".aBool",aBool);
        // this works with arrays and lists as well as with every content (ToString() will be called internally)
        //MIP.Watch(name + ".aList", aList);
        
	}

    void OnGUI()
    {
        // change aBool on button click
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 80, 30, 160, 60));
        if (GUILayout.Button("Change aBool"))
            aBool = !aBool;
        // change aVector two times on button click by adding vectors
        if (GUILayout.Button("Change aVector 2x")) {
            aVector = Random.insideUnitCircle * 100;
            aList.Add(aVector);
            // calling MIP.Watch() once a frame normally is enough.
            // But we want to see how MIP behaves if we watch it several times each frame...
            MIP.Watch(name + ".aVector", aVector);
            aVector = Random.insideUnitCircle * 100;
            aList.Add(aVector);
            MIP.Watch(name + ".aVector", aVector);
        }
        GUILayout.EndArea();
    
    }
}
