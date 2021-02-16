using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(LandmarkManager))]
public class LandmarkManagerEditor : Editor {


    public override void OnInspectorGUI() {
        LandmarkManager landmarkManager = (LandmarkManager)target;

        //myTarget.experience = EditorGUILayout.IntField("Experience", myTarget.experience);
        //EditorGUILayout.LabelField("Level", myTarget.Level.ToString());

        // Show default inspector property editor
        DrawDefaultInspector();

        GUILayout.Space(10f);
        
        if (GUILayout.Button("Load Structure Data")) {
            landmarkManager.LoadStructureData();
        }
    }
}
#endif