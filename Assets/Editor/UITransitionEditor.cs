using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/**
 * UITransitionEditor attaches diagnostic buttons to UITransition for navigation.
 **/
[CustomEditor(typeof(UITransition))]
public class UITransitionEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UITransition transitionScript = (UITransition)target;

        if (GUILayout.Button("Forward"))
        {
            transitionScript.Forward();
        }

        if (GUILayout.Button("Backward"))
        {
            transitionScript.Backward();
        }
    }
}