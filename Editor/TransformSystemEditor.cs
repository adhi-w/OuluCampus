using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TransformSystem))]
public class TransformSystemEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        TransformSystem script = (TransformSystem)target;
        if(GUILayout.Button("Send static transforms"))
            script.sendStaticTransforms();
    }
}