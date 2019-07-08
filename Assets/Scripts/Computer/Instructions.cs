using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class Instructions : VirtualProgram {
    public SyntaxTheme syntaxTheme;
    public TMP_Text textUI;

    [TextArea(20,100)]
    public string instructions;

    void Start() {
        Run();
    }

    void Update () {
        if (!Application.isPlaying) {
            Run();
        }
    }

    void Run() {
        textUI.text = SyntaxHighlighter.HighlightCode (instructions, syntaxTheme);
    }
}