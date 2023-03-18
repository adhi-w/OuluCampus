using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace UnityPhysics
{
    [CustomEditor(typeof(Motion_Primitive_Controller))]
    public class PrimitiveTest : Editor
    {
        float targetFloat;
        public override void OnInspectorGUI()
        {
            Motion_Primitive_Controller script = (Motion_Primitive_Controller)target;
            base.OnInspectorGUI();


            if (GUILayout.Button("Run queue"))
            {
                script.primitiveList = script.InspectorConverter(script.inspectorList);
                script.nextPrimitive();
            }
            GUILayout.Space(13);
            GUILayout.Label("Testing:");
            targetFloat = EditorGUILayout.FloatField("Target value", targetFloat);
            
            
            if(GUILayout.Button("Drive straight"))
            { 
                script.currentPrimitive = new P_DriveStraight(targetFloat, script);
            }
            if (GUILayout.Button("Turn in place"))
            {
                script.currentPrimitive = new P_TurnInPlace(targetFloat, script);
            }
            
        }
    }
}

