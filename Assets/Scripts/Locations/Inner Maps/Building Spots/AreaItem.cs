using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Pathfinding;
using TMPro;
using UnityEngine;

public class AreaItem : BaseMonoBehaviour {

    [SerializeField] private Collider2D boundsCollider;

    private InnerTileMap _innerTileMap;

    public void Initialize(InnerTileMap innerTileMap) {
        _innerTileMap = innerTileMap;
    }
    
    void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Vector3 position = transform.position;
        Gizmos.DrawWireCube(position, new Vector3(InnerMapManager.AreaLocationGridTileSize.x - 0.5f, InnerMapManager.AreaLocationGridTileSize.y - 0.5f, 0));    
    }


    [ContextMenu("Update Pathfinding Graphs")]
    public void UpdatePathfindingGraph() {
        GraphUpdateObject graphUpdateObject = new TagGraphUpdateObject(boundsCollider.bounds) {nnConstraint = _innerTileMap.onlyPathfindingGraph, updatePhysics = true, modifyWalkability = false};
        PathfindingManager.Instance.UpdatePathfindingGraphPartialCoroutine(graphUpdateObject);
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        _innerTileMap = null;
    }
}
