using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VirtualOS : MonoBehaviour {

    public bool useTestCodeForTasks;

    public Camera viewCam;
    public VirtualProgram taskSelect;
    public VirtualProgram editor;
    public VirtualProgram console;
    public VirtualProgram instructions;

    public enum Program { None, TaskSelect, Editor, Console, Instructions }
    public Program currentProgram;

    bool needsUpdate;

    static VirtualOS _instance;
    Task currentTask;

    void Awake () {
        SetProgram (currentProgram);
    }

    static VirtualOS Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<VirtualOS> ();
            }
            return _instance;
        }
    }

    public static bool Active {
        get {
            return Instance != null;
        }
    }

    public static void RegisterTask (Task task) {
        Instance.currentTask = task;
        Instance.CopyTargetCamera ();
        Instance.RunTask ();
    }

    void RunTask () {
        string code = ((VirtualScriptEditor) editor).code;
        if (useTestCodeForTasks) {
            code = currentTask.testCode.text;
            Debug.Log ("Running task with test code");
        }
        currentTask.StartTask (code);
        ((VirtualScriptEditor) editor).SetTaskInfo (currentTask.taskInfo);

        SetProgram (Program.Console);
    }

    void StopTask () {
        if (currentTask != null) {
            currentTask.StopTask ();
        }
    }

    void Update () {

        HandleInput ();

        if (needsUpdate && !Application.isPlaying) {
            needsUpdate = false;
            SetProgram (currentProgram);
        }
    }

    void HandleInput () {
        // Open code editor
        if (Input.GetKeyDown (KeyCode.E) && ControlOperatorDown ()) {
            if (currentProgram == Program.Console) {
                StopTask ();
                SetProgram (Program.Editor);
            }
        }

        // Run code
        if (Input.GetKeyDown (KeyCode.R) && ControlOperatorDown ()) {
            if (currentProgram == Program.Editor) {
                RunTask ();
            }
        }

        // Open task menu
        if (Input.GetKeyDown (KeyCode.T) && ControlOperatorDown ()) {
            if (currentProgram != Program.TaskSelect) {
                StopTask ();
                SetProgram (Program.TaskSelect);
            }
        }
    }

    public void SetProgram (Program program) {
        currentProgram = program;
        if (taskSelect != null) {
            taskSelect.SetActive (currentProgram == Program.TaskSelect);
            editor.SetActive (currentProgram == Program.Editor);
            console.SetActive (currentProgram == Program.Console);
            instructions.SetActive (currentProgram == Program.Instructions);
        }
    }

    public void CopyTargetCamera () {
        if (FindObjectOfType<CameraTarget> ()) {
            Camera targetCam = FindObjectOfType<CameraTarget> ().GetComponent<Camera> ();
            // Copy settings
            viewCam.transform.position = targetCam.transform.position;
            viewCam.transform.rotation = targetCam.transform.rotation;
            viewCam.orthographic = targetCam.orthographic;
            viewCam.orthographicSize = targetCam.orthographicSize;
            viewCam.fieldOfView = targetCam.fieldOfView;
            // Disable
            targetCam.gameObject.SetActive (false);
        }
    }

    bool ControlOperatorDown () {
        bool ctrl = Input.GetKey (KeyCode.LeftControl) || Input.GetKey (KeyCode.RightControl);
        bool cmd = Input.GetKey (KeyCode.LeftCommand) || Input.GetKey (KeyCode.RightCommand);
        return ctrl || cmd;
    }

    void OnValidate () {
        needsUpdate = true;
    }
}