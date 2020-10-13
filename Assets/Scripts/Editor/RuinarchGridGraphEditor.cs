using UnityEditor;
using Pathfinding;
using PathFinding;

[CustomGraphEditor(typeof(RuinarchGridGraph), "Ruinarch Graph")]
public class RuinarchGridGraphEditor : GridGraphEditor {
    // Here goes the GUI
    // public override void OnInspectorGUI (NavGraph target) {
    //     var graph = target as RuinarchGridGraph;
    //
    //     graph.circles = EditorGUILayout.IntField("Circles", graph.circles);
    //     graph.steps = EditorGUILayout.IntField("Steps", graph.steps);
    //     graph.scale = EditorGUILayout.FloatField("Scale", graph.scale);
    //     graph.center = EditorGUILayout.Vector3Field("Center", graph.center);
    // }
}