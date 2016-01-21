// =====================================================================
// Copyright 2012-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/* This script demonstrates the profiling capabilities of Measure It! Pro
 * 
 * Profiling is done by enclosing code sections with a call to MIP.BeginProfiler() and MIP.EndProfiler()
 * (or by using the MIPProfiler class). Profiling is limited to frames, so you can't measure times from 
 * one frame to another (use MIP.BeginTimer() and MIP.EndTimer() to measure independently from frames).
 * 
 */

public class TestProfiler : MonoBehaviour {
    public int LateUpdateFrom=4;
    public int LateUpdateTo = 4;
    public bool Nested=true;
    public int NestedFrom=1;
    public int NestedTo=2;
    public bool UseTicks;

    void Update()
    {
        // We want to profile the time Update() takes, so we create a profiling slot for it:
        MIP.BeginProfiler(name + ".Update");

        // Consume some time to simulate real code execution
        if (UseTicks)
            MIP.Wait((int)(Mathf.Sin(Time.time*2)*10 +50)); // wait some ticks
        else
            MIP.WaitMillisecs((int)(Mathf.Sin(Time.time * 2) * 2 + 5)); // wait some milliseconds
        // End the profiling of this slot
        MIP.EndProfiler(name + ".Update");
    }

    void LateUpdate()
    {
        // In C# you might want to use a using-block. This work's exactly like calling BeginProfiler() 
        // and EndProfiler(), but it's shorter and more convenient:
        using (new MIPProfiler(name + ".LateUpdate")) {
            
            // Consume some time
            if (UseTicks)
                MIP.Wait(LateUpdateFrom, LateUpdateTo);
            else
                MIP.WaitMillisecs(LateUpdateFrom, LateUpdateTo);

            // You can nest profiling calls. In the data readout subsequent profiling sections are shown in the "Others" column
            if (Nested)
                NestOne();
        }

    }

    void NestOne()
    {
        // Profile another section within LateUpdate()
        using (new MIPProfiler("NestOne")) {
            // and another one...
            NestTwo();
            // consume time
            MIP.WaitMillisecs(NestedFrom, NestedTo);
            // call this two times each frame and watch the data readout
            NestTwo();
        }
    }
    void NestTwo()
    {
        // another level of nesting:
        MIP.BeginProfiler("NestTwo");
            // consume some time
            MIP.WaitMillisecs(NestedFrom, NestedTo);
            MIP.EndProfiler("NestTwo");
    }
   
}
