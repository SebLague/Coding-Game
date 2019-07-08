using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VirtualOS))]
public class OSEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        var editor = (VirtualOS)target;

        if (GUILayout.Button("Set Cam")) {
            editor.CopyTargetCamera();
        }
    }
}
