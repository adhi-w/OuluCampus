using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SystemScript))]
public class SystemScriptEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        SystemScript script = (SystemScript)target;

        bool started = script.started;
        GUI.enabled = !started;
        if(GUILayout.Button("Start"))
        {
            script.init();
        }
    }
}