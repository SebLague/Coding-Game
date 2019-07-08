using System.Collections.Generic;
using UnityEngine;

public class CompilerTest : MonoBehaviour {

    public bool useFromFile;
    public TextAsset codeFile;

    void Update () {
        if (Input.GetKeyDown (KeyCode.Tab)) {
            Run ();
        }
    }

    public void Run () {
        Debug.Log ("Running...");
        if (useFromFile) {
            RunCode (codeFile.text);
        } else {
            var editor = FindObjectOfType<VirtualScriptEditor> ();
            RunCode (editor.code);
        }
    }

    void RunCode (string code) {
        var compiler = new VirtualCompiler (code);
        compiler.Run ();
    }

    [ContextMenu ("Test")]
    public void Test () {
        var variables = new Dictionary<string, float> ();
        variables.Add ("x", 3);
        variables.Add ("y", -5);

        string code = "-x+7 * 2 - y *-(1+3*(7+x*2)) + 2";
        var c = new ValueString (code, variables);
        var e = new NumericalExpression (c);
        float r = e.Evaluate ();
        print (r);
        //print(c.markedString);
        for (int i = 0; i < c.values.Count; i++) {
            //print(c.values[i]);
        }
    }

}