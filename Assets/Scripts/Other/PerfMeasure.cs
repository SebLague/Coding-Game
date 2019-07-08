using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfMeasure : MonoBehaviour {
    public int iterations = 10000;
    float[] values;
    float testVal;

    [ContextMenu ("Run")]
    void Run () {
        values = new float[iterations + 1];
        for (int i = 0; i <= iterations; i++) {
            values[i] = Random.Range (-1000f, 1000f);
        }
        var names = new string[] {
            "add",
            "mul",
            "div",
            "sqrt",
            "abs",
            "sign",
            "sin",
            "cos",

        };
        var ops = new System.Func<float, float, float>[] {
                (x, y) => x + y,
                (x, y) => x * y,
                (x, y) => x / y,
                (x, y) => (float)System.Math.Sqrt(x),
                (x, y) => Mathf.Abs(x),
                (x, y) => Mathf.Sign(x),
                (x, y) => Mathf.Sin(x),
                (x, y) => Mathf.Cos(x),
            };

        float baseTime = Measure ((a, b) => 0);
        float emptyTime = MeasureEmptyLoop();

        float[] times = new float[ops.Length];
        for (int i = 0; i < ops.Length; i++) {
            times[i] = Measure (ops[i]);
            print (names[i] + ": " + times[i]);
        }
        print ("base: " + baseTime);
        print ("empty: " + emptyTime);
    }

    float Measure (System.Func<float, float, float> operation) {
        var sw = System.Diagnostics.Stopwatch.StartNew ();
        for (int i = 0; i < iterations; i++) {
            testVal = operation (values[i], values[i+1]);
        }
        sw.Stop ();
        return sw.ElapsedMilliseconds;
    }

    float MeasureEmptyLoop () {
        var sw = System.Diagnostics.Stopwatch.StartNew ();
        for (int i = 0; i < iterations; i++) {
           
        }
        sw.Stop ();
        return sw.ElapsedMilliseconds;
    }
}