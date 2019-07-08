using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarsRoverTask : Task {

    public Transform rover;
    public float timeMin = 3;
    public float timeMax = 10;
    const float targetDst = 30;
    const float bufferDst = 0.2f;
    public ParticleSystem[] roverDust;

    Vector3 initialPos;

    const int drainPow = 4;
    float roverSpeed;
    float batteryRemaining;

    float baseDrainRate;
    float motorEfficiency;

    protected override void Run (string code) {
        initialPos = rover.position;
        base.Run (code);
    }

    protected override void StartNextTestIteration () {
        base.StartNextTestIteration ();

        rover.position = initialPos;

        int safety = 0;
        while (true) {
            RandomizeConditions ();
            float time = targetDst / roverSpeed;
            if (time >= timeMin && time <= timeMax) {
                print (roverSpeed + " t: " + time);
                break;
            }

            safety++;
            if (safety > 250) {
                Debug.Log ("Safety exceeded");
                break;
            }
        }

        roverSpeed = 0;

        float[] inputs = { batteryRemaining, baseDrainRate, motorEfficiency };
        GenerateOutputs (inputs);
        if (outputQueue.Count > 0) {
            var func = outputQueue.Dequeue ();
            roverSpeed = func.value;
        }
        if (roverSpeed > 0) {
            foreach (var p in roverDust) {
                p.Play (true);
            }
        }
    }

    void RandomizeConditions () {
        baseDrainRate = Random.Range (0.1f, 100f);
        motorEfficiency = Random.Range (.5f, 100);

        float optimalSpeed = Mathf.Sqrt (Mathf.Sqrt (baseDrainRate * motorEfficiency / (drainPow - 1)));
        float dst = motorEfficiency * optimalSpeed / (baseDrainRate * motorEfficiency + Mathf.Pow (optimalSpeed, drainPow));

        batteryRemaining = targetDst / dst;
        roverSpeed = optimalSpeed;
    }

    void Update () {
        if (running) {
            float drainRate = baseDrainRate + Mathf.Pow (roverSpeed, drainPow) / motorEfficiency;
            float deltaCharge = drainRate * Time.deltaTime;
            float movePercent = Mathf.Min (batteryRemaining, deltaCharge) / deltaCharge;
            batteryRemaining = Mathf.Max (0, batteryRemaining - deltaCharge);

            rover.transform.position += Vector3.right * roverSpeed * Time.deltaTime * movePercent;

            if (batteryRemaining <= 0) {
                float dst = rover.transform.position.x - initialPos.x;

                if (dst > targetDst - bufferDst) {
                    TestPassed ();
                } else {
                    float remainingDst = targetDst - dst;
                    VirtualConsole.Log ("Dst remaining: " + VirtualConsole.RoundValueForDisplay (remainingDst));
                    TestFailed ();
                }
            }
        }
    }

    protected override void Clear () {
        base.Clear ();
        rover.position = initialPos;
        foreach (var p in roverDust) {
            p.Stop (true);
            p.Clear (true);
        }
    }
}