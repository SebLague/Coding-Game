using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class TaskInfo : MonoBehaviour {

    public TMP_Text textUI;

    [TextArea (6, 10)]
    public string description;
    public TaskConstant[] constants;
    public string[] variables;
    public TaskFunction[] outputs;

    public Color descriptionCol;
    public Color headerCol;

    void Update () {
        if (!Application.isPlaying) {
            if (textUI != null) {
                textUI.text = FormattedInfoString ();
            }
        }
    }

    public string FormattedInfoString () {
        string info = "";

        info += SyntaxHighlighter.CreateColouredText (descriptionCol, description) + "\n";

        if (constants.Length > 0) {
            info += "\n" + SyntaxHighlighter.CreateColouredText (headerCol, "Constants:") + "\n";
            for (int i = 0; i < constants.Length; i++) {
                info += constants[i].name + " = " + constants[i].value;
                info += "\n";
            }
        }

        if (variables.Length > 0) {
            info += "\n" + SyntaxHighlighter.CreateColouredText (headerCol, "Variables:") + "\n";
            for (int i = 0; i < variables.Length; i++) {
                info += variables[i];
                info += "\n";
            }
        }

        if (outputs.Length > 0) {
            info += "\n" + SyntaxHighlighter.CreateColouredText (headerCol, "Outputs:") + "\n";
            for (int i = 0; i < outputs.Length; i++) {
                string paramName = (outputs[i].hasParam) ? outputs[i].paramLabel : "";
                info += outputs[i].name + "(" + paramName + ")";
                info += "\n";
            }
        }

        return info;
    }

}

[System.Serializable]
public struct TaskConstant {
    public string name;
    public float value;
}

[System.Serializable]
public struct TaskFunction {
    public string name;
    public bool hasParam;
    public string paramLabel;
}