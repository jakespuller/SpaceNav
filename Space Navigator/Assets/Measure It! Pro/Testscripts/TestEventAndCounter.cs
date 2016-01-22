// =====================================================================
// Copyright 2012-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/* This script demonstrates the usage of events and counters
 * 
 * Events can be used to visualize the execution order of your code, they're shown in the data readout as well as in the graph view.
 * 
 * Counters are a special case of events as they additionaly show the number of calls to it since the recording started.
 * 
 */

public class TestEventAndCounter : MonoBehaviour {
    public int EventsPerFrame = 1; // increase to produce more calls each click

    void Awake()
    {
        // Create an event slot and log the call of Awake()
        MIP.Log(name+".Callbacks", "Awake");
    }

    void Start()
    {
        // Log Start() to the same slot
        MIP.Log(name+".Callbacks","Start");
    }

    void OnEnable()
    {
        MIP.Log(name+".Callbacks", "OnEnable");
    }

    void OnDisable()
    {
        MIP.Log(name+".Callbacks", "OnDisable");
    }

    void OnGUI()
    {
        // Clicking the button will produce events and increase a click counter
        if (GUI.Button(new Rect(Screen.width/2-80, 150, 160, 20), name+".Button")) {
                // create some events
                for (int i = 0; i < EventsPerFrame; i++) {
                    // we log our events to the Button slot
                    MIP.Log(name + ".Button", "Event Call" + (i + 1).ToString());
                }
                // now let's increase our button click counter. If you look at the data readout you'll see an extra "Button" tree node. This is because
                // slots can't nest within other slots (profiling slots being the only exception).
                MIP.Count(name + ".Button.Click");

        }
        if (GUI.Button(new Rect(Screen.width / 2 - 80, 180, 160, 20), "Count me!"))
            MIP.Count(name + ".Counter");
    }

    
}
