using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CompilerTest))]
public class CompilerTestEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        var compilerTest = (CompilerTest)target;

        if (GUILayout.Button("Run")) {
            compilerTest.Run();
        }

    }
}
