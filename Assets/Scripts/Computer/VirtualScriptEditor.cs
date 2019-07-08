using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VirtualScriptEditor : VirtualProgram {

    const string indentString = "  ";
    string legalChars = "abcdefghijklmnopqrstuvwxyz 0.123456789+-/*=<>()[]{},";

    public SyntaxTheme syntaxTheme;
    public TMPro.TMP_Text codeUI;
    public TMPro.TMP_Text lineNumbersUI;
    public TMP_Text taskInfoUI;
    public Image caret;

    public string code { get; set; }
    public int lineIndex;
    public int charIndex;

    public float blinkRate = 1;
    float lastInputTime;
    float blinkTimer;

    void Start () {
        if (code == null) {
            code = "";
        }
        charIndex = code.Length;

        CustomInput.instance.RegisterKey (KeyCode.Backspace);
        CustomInput.instance.RegisterKey (KeyCode.LeftArrow);
        CustomInput.instance.RegisterKey (KeyCode.RightArrow);
        CustomInput.instance.RegisterKey (KeyCode.UpArrow);
        CustomInput.instance.RegisterKey (KeyCode.DownArrow);
    }

    void Update () {

        if (!active) {
            return;
        }

        HandleTextInput ();
        HandleSpecialInput ();

        //code = FormatIndenting ();
        //codeUI.text = code;

        SetLineNumbers ();

        if (codeUI != null && caret != null) {
            SetCaret (code);
        }

        string formattedCode = FormatIndenting ();
        codeUI.text = SyntaxHighlighter.HighlightCode (formattedCode, syntaxTheme);

    }

    string FormatIndenting () {
        string formattedCode = "";
        string[] lines = code.Split ('\n');

        int indentLevel = 0;
        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i];
            if (line.Contains ("}")) {
                indentLevel--;
            }

            //int originalLineLength = line.Length;
            //line = line.TrimStart (' ');

            for (int j = 0; j < indentLevel; j++) {
                line = indentString + line;
            }

            formattedCode += line;
            if (i < lines.Length - 1) {
                formattedCode += "\n";
            }

            if (line.Contains ("{")) {
                indentLevel++;
            }
        }
        // print ("originalCharIndex: " + originalCharIndex + "  new: " + charIndex);
        //print (code + "  " + formattedCode);
        return formattedCode;
    }

    void HandleTextInput () {
        string input = Input.inputString;
        if (!Input.GetKey (KeyCode.LeftControl) && !Input.GetKey (KeyCode.LeftCommand)) {
            foreach (char c in input) {
                if (legalChars.Contains (c.ToString ().ToLower ())) {
                    lastInputTime = Time.time;
                    if (string.IsNullOrEmpty (code) || charIndex == code.Length) {
                        code += c;
                    } else {
                        code = code.Insert (charIndex, c.ToString ());
                    }
                    charIndex++;
                }
            }
        }
    }

    void HandleSpecialInput () {
        // New line
        if (Input.GetKeyDown (KeyCode.Return)) {
            lastInputTime = Time.time;
            if (string.IsNullOrEmpty (code) || charIndex == code.Length) {
                code += "\n";

            } else {
                code = code.Insert (charIndex, "\n");
            }
            charIndex++;
            lineIndex++;
        }

        // Delete
        if (CustomInput.instance.GetKeyPress (KeyCode.Backspace)) {
            if (charIndex > 0) {
                lastInputTime = Time.time;
                char deletedChar = code[charIndex - 1];
                string start = code.Substring (0, charIndex - 1);
                string end = code.Substring (charIndex, code.Length - charIndex);
                code = start + end;

                charIndex--;
                if (deletedChar == '\n') {
                    lineIndex--;
                }
            }
        }

        if (CustomInput.instance.GetKeyPress (KeyCode.LeftArrow)) {
            lastInputTime = Time.time;
            if (code.Length > 0 && charIndex > 0) {
                if (code[charIndex - 1] == '\n') {
                    lineIndex--;
                }
            }
            charIndex = Mathf.Max (0, charIndex - 1);
        }
        if (CustomInput.instance.GetKeyPress (KeyCode.RightArrow)) {
            lastInputTime = Time.time;
            if (code.Length > charIndex) {
                if (code[charIndex] == '\n') {
                    lineIndex++;
                }
            }
            charIndex = Mathf.Min (code.Length, charIndex + 1);
        }
        if (CustomInput.instance.GetKeyPress (KeyCode.UpArrow)) {
            if (lineIndex > 0) {
                lastInputTime = Time.time;
                string[] lines = code.Split ('\n');
                int numCharsInPreviousLines = 0;
                for (int i = 0; i < lineIndex; i++) {
                    numCharsInPreviousLines += lines[i].Length + 1;
                }
                charIndex = numCharsInPreviousLines - 1;
                lineIndex--;
            }
        }

        if (CustomInput.instance.GetKeyPress (KeyCode.DownArrow)) {
            string[] lines = code.Split ('\n');

            if (lineIndex < lines.Length - 1) {
                lastInputTime = Time.time;

                int numCharsInPreviousLines = lines[0].Length;
                for (int i = 1; i <= lineIndex + 1; i++) {
                    numCharsInPreviousLines += lines[i].Length + 1;
                }
                charIndex = numCharsInPreviousLines;
                lineIndex++;
            }
        }
    }

    void SetCaret (string text) {
        // Blink caret
        blinkTimer += Time.deltaTime;
        if (Time.time - lastInputTime < blinkRate / 2) {
            caret.enabled = true;
            blinkTimer = 0;
        } else {
            caret.enabled = (blinkTimer % blinkRate < blinkRate / 2);
        }

        string stopChar = ".";

        // Get single line height, and height of code up to charIndex
        codeUI.text = stopChar;
        float stopCharWidth = codeUI.preferredWidth;
        float singleLineHeight = codeUI.preferredHeight;

        string codeUpToCharIndex = text.Substring (0, charIndex);
        codeUI.text = codeUpToCharIndex + stopChar;
        float height = codeUI.preferredHeight - singleLineHeight;

        // Get indent level
        int indentLevel = 0;
        string[] lines = text.Split ('\n');
        bool startIndentationNextLine = false;
        for (int i = 0; i <= lineIndex; i++) {
            if (startIndentationNextLine) {
                startIndentationNextLine = false;
                indentLevel++;
            }
            if (lines[i].Contains ("{")) {
                startIndentationNextLine = true;
            }
            if (lines[i].Contains ("}")) {
                if (startIndentationNextLine) {
                    startIndentationNextLine = false;
                } else {
                    indentLevel--;
                }
            }
        }

        // Get string from start of current line up to caret
        string textUpToCaretOnCurrentLine = "";
        for (int i = charIndex - 1; i >= 0; i--) {
            if (code[i] == '\n' || i == 0) {
                textUpToCaretOnCurrentLine = text.Substring (i, charIndex - i);
                break;
            }
        }
        textUpToCaretOnCurrentLine = textUpToCaretOnCurrentLine.Replace ("\n", "");
        for (int i = 0; i < indentLevel; i++) {
            textUpToCaretOnCurrentLine = indentString + textUpToCaretOnCurrentLine;
        }

        codeUI.text = textUpToCaretOnCurrentLine + stopChar;
        float width = codeUI.preferredWidth - stopCharWidth;

        caret.rectTransform.position = codeUI.rectTransform.position;
        caret.rectTransform.localPosition += Vector3.right * (width + caret.rectTransform.rect.width / 2f);
        caret.rectTransform.localPosition += Vector3.down * (caret.rectTransform.rect.height / 2 + height);
    }

    void SetLineNumbers () {
        string numbers = "";

        int numLines = code.Split ('\n').Length;
        for (int i = 0; i < numLines; i++) {
            numbers += (i + 1) + "\n";
        }

        lineNumbersUI.text = numbers;
    }

    public void SetTaskInfo (TaskInfo info) {
        taskInfoUI.text = info.FormattedInfoString ();
    }

}