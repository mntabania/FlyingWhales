using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class BoundsTester : MonoBehaviour {
    public GameObject go;
    public Tilemap tilemap;
    private void Update() {
        if (tilemap == null || go == null) {
            return;
        }

        Bounds bounds = tilemap.localBounds;
        bounds.center = tilemap.localBounds.center + tilemap.transform.position;

        Vector3 closestPoint = bounds.ClosestPoint(go.transform.position);
        Debug.DrawLine(bounds.center, closestPoint, Color.green);
        Debug.Log($"World Pos: {closestPoint.ToString()}. Local Pos {tilemap.transform.InverseTransformPoint(closestPoint).ToString()}");
    }
}
