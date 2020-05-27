using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using TMPro;
using UnityEngine;

public class LocationGridTileCollectionItem : MonoBehaviour {

    [SerializeField] private Collider2D boundsCollider;
    
    void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Vector3 position = transform.position;
        Gizmos.DrawWireCube(position, new Vector3(InnerMapManager.BuildingSpotSize.x - 0.5f, InnerMapManager.BuildingSpotSize.y - 0.5f, 0));    
    }


    [ContextMenu("Update Pathfinding Graphs")]
    public void UpdatePathfindingGraph() {
        AstarPath.active.UpdateGraphs(boundsCollider.bounds);
    }
    
    
}
