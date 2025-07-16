using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIColliderGenerator))]
public class UIColliderGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UIColliderGenerator uiColliderGenerator = (UIColliderGenerator)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Collider"))
        {
            uiColliderGenerator.Generate();
        }
        
    }
}
