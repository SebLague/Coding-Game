using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VirtualConsole : VirtualProgram {

    public TMP_Text textUI;
    int maxLines;
    List<string> messages = new List<string> ();
    float writeSpeed = 60;
    int writingLineIndex;
    float currentCharIndexFloat;

    static VirtualConsole _instance;

    void Awake () {
        maxLines = textUI.text.Split ('\n').Length;
        textUI.text = "";
    }

    void Update () {
        if (writingLineIndex < messages.Count) {
            currentCharIndexFloat += writeSpeed * Time.deltaTime;
            if (currentCharIndexFloat > messages[writingLineIndex].Length) {
                writingLineIndex++;
                currentCharIndexFloat = 0;
            }
        }

        string consoleText = "";
        int lastWrittenIndex = Mathf.Min (writingLineIndex, messages.Count - 1);
        int startIndex = Mathf.Max (0, lastWrittenIndex - maxLines + 1);
        for (int i = startIndex; i <= lastWrittenIndex; i++) {
            if (i == writingLineIndex) {
                int charIndex = Mathf.Min (messages[i].Length, (int) currentCharIndexFloat);
                consoleText += messages[i].Substring (0, charIndex);
            } else {
                consoleText += messages[i] + "\n";
            }

        }

        textUI.text = consoleText;

    }

    static VirtualConsole Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<VirtualConsole> ();
            }
            return _instance;
        }
    }

    public static void Log (string message) {
        if (Instance != null) {
            Instance.messages.Add (message);
        }
    }

    public static void LogTaskFailed () {
        Log ("Task Failed");
    }

    public static void Clear () {
        if (Instance != null) {
            Instance.writingLineIndex = 0;
            Instance.currentCharIndexFloat = 0;
            Instance.messages.Clear ();
            Instance.textUI.text = "";
        }
    }

    public static void LogInputs (string[] names, float[] values) {
        for (int i = 0; i < names.Length; i++) {
            Log (names[i] + " = " + RoundValueForDisplay (values[i]));
        }
    }

    public static void LogOutput (string name, float value, bool showValue = true) {
        string valueString = (showValue) ? RoundValueForDisplay (value).ToString () : "";
        Log (name + "(" + valueString + ")");
    }

    public static float RoundValueForDisplay (float value) {
        int decimalPlaces = 2;
        int n = (int) Mathf.Pow (10, decimalPlaces);
        return Mathf.RoundToInt (value * n) / (float) n;
    }

}