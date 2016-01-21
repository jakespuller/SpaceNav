// =====================================================================
// Copyright 2012-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/* This script demonstrates the usage of MIP timers
 * 
 * You use timers to measure a time span and show the result in the data readout. Unlike MIP's profiling methods timers can be used to measure time over an 
 * unlimited range of frames.
 */

public class TestTimer : MonoBehaviour {

    bool running=false;

    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width/2-80,110,160,20),"Start/Stop Timer")) {
            if (running) {
                running = false;
                // End the timer
                MIP.EndTimer(name + ".Simple Timer");
            }
            else {
                running = true;
                // Start the timer
                MIP.BeginTimer(name + ".Simple Timer");
            }
        }
    }
}
