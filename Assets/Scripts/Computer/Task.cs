using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task : MonoBehaviour {

    [Header ("Code")]
    bool inTestMode;
    public TextAsset testCode;
    string testCodeText;

    public TaskInfo taskInfo;
    public int taskIterations = 10;
    protected int currentIteration;
    string code;

    protected VirtualCompiler compiler;
    protected Queue<Function> outputQueue;
    protected bool running;

    protected virtual void Start () {
        inTestMode = !VirtualOS.Active;

        if (inTestMode) {
            testCodeText = (testCode == null) ? "" : testCode.text;
            StartTask (testCodeText);
        } else {
            VirtualOS.RegisterTask (this);
        }
    }

    public bool InTestMode {
        get {
            return inTestMode;
        }
    }

    public void StartTask (string code) {
        this.code = code;
        StartCoroutine (StartTaskRoutine ());
        VirtualConsole.Clear ();
    }

    IEnumerator StartTaskRoutine () {
        yield return new WaitForSeconds (.2f);
        VirtualConsole.Log ("Compiling code...");
        yield return new WaitForSeconds (.5f);
        Run (code);
    }

    protected virtual void Run (string code) {
        Clear ();

        this.code = code;
        outputQueue = new Queue<Function> ();
        running = true;
        StartNextTestIteration ();
    }

    public virtual void StopTask () {
        running = false;
        Clear ();
    }

    protected virtual void Clear () {
        currentIteration = 0;
    }

    protected void GenerateOutputs (float[] variableValues) {

        //VirtualConsole.Log ("Executing code...");
        string codeToRun = (inTestMode) ? testCodeText : code;

        compiler = new VirtualCompiler (codeToRun);
        for (int i = 0; i < taskInfo.outputs.Length; i++) {
            compiler.AddOutputFunction (taskInfo.outputs[i].name);
        }
        for (int i = 0; i < taskInfo.constants.Length; i++) {
            compiler.AddInput (taskInfo.constants[i].name, taskInfo.constants[i].value);
        }
        for (int i = 0; i < variableValues.Length; i++) {
            compiler.AddInput (taskInfo.variables[i], variableValues[i]);
        }

        //VirtualConsole.LogInputs (inputNames, inputValues);

        List<VirtualFunction> outputs = compiler.Run ();
        if (outputs.Count > 0) {
            float val = 0;
            if (outputs[0].values.Count > 0) {
                val = outputs[0].values[0];
            }
            var func = new Function () { name = outputs[0].name, value = val };
            outputQueue.Enqueue (func);

            VirtualConsole.LogOutput (func.name, func.value);
        }
    }

    protected virtual void StartNextTestIteration () {

    }

    protected void TestPassed () {
        if (running) {
            currentIteration++;
            VirtualConsole.Log ($"Test {currentIteration}/{taskIterations} passed");
            if (currentIteration == taskIterations) {
                TaskComplete ();
            } else {
                StartNextTestIteration ();
            }
        }
    }

    protected virtual void TestFailed () {
        running = false;
        VirtualConsole.LogTaskFailed ();
    }

    protected virtual void TaskComplete () {
        VirtualConsole.Log ("Task completed");
        running = false;

    }

    public struct Function {
        public string name;
        public float value;
    }
}