using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleTask : Task {

    protected override void Run (string code) {
        base.Run (code);
        // Any required first-time initialization
    }

    protected override void StartNextTestIteration () {
        base.StartNextTestIteration ();
        // Run test
    }

    void Update () {
        if (running) {
            // Check for pass/fail conditions:

            //TestFailed();
            //TestPassed();
        }
    }

    protected override void Clear () {
        base.Clear ();
    }
}