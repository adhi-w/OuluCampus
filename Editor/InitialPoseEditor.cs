using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InitialPose))]
public class InitialPoseEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        InitialPose script = (InitialPose)target;
        if(GUILayout.Button("Send initial pose"))
        {
            script.sendPose();
        }
    }
}