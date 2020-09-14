using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;

public class BlockerTraversalProvider : ITraversalProvider {

    private CharacterMarker _marker;
    public BlockerTraversalProvider(CharacterMarker marker) {
        _marker = marker;
    }

	public bool CanTraverse(Path path, GraphNode node) {
        // Make sure that the node is walkable and that the 'enabledTags' bitmask
        // includes the node's tag.
        //return node.Walkable && (path.enabledTags >> (int) node.Tag & 0x1) != 0;
        // alternatively:
        if(_marker.useCanTraverse){
            return DefaultITraversalProvider.CanTraverse(path, node) && _marker.pathfindingAI.IsNodeWalkable((Vector3) node.position);
        }
        return DefaultITraversalProvider.CanTraverse(path, node);
    }

    public uint GetTraversalCost(Path path, GraphNode node) {
        uint additionalPenalty = 0;
        //uint additionalPenalty = _marker.pathfindingAI.GetNodePenaltyForSettlements(path, (Vector3) node.position);

        //if (!_marker.pathfindingAI.IsNodeWalkable((Vector3) node.position)) {
        //    additionalPenalty = 5000000;
        //}

        //if (!_marker.useCanTraverse) {
        //    //    _marker.pathfindingAI.GetNodePenalty((Vector3) node.position) 
        //    //+_marker.pathfindingAI.GetNodePenaltyForStructures(path, (Vector3) node.position)
        //    //+
        //    additionalPenalty = _marker.pathfindingAI.GetNodePenaltyForSettlements(path, (Vector3) node.position);
        //}

        // The traversal cost is the sum of the penalty of the node's tag and the node's penalty
        //return path.GetTagPenalty((int) node.Tag) + node.Penalty;
        // alternatively:
        return DefaultITraversalProvider.GetTraversalCost(path, node) + additionalPenalty;
    }
    public void CleanUp() {
        _marker = null;
    }



    //private bool IsNodeWalkable(GraphNode node) {
    //    if (_marker.terrifyingCharacters.Count > 0) {
    //        for (int i = 0; i < _marker.terrifyingCharacters.Count; i++) {
    //            Character terrifyingCharacter = _marker.terrifyingCharacters.ElementAtOrDefault(i);
    //            if (terrifyingCharacter == null || (terrifyingCharacter.currentParty.icon.isTravelling && terrifyingCharacter.currentParty.icon.travelLine != null && _marker.character.currentStructure != terrifyingCharacter.currentStructure)) {
    //                continue;
    //            }
    //            if (!terrifyingCharacter.isDead) {
    //                Vector3 nodePos = (Vector3) node.position;
    //                if (Vector3.Distance(nodePos, terrifyingCharacter.marker.worldPos) <= _marker.penaltyRadius) {
    //                    return false;
    //                }
    //            }
    //        }
    //    }

    //    //if (_marker.terrifyingCharacters.Count > 0) {
    //    //    Vector3 nodePos = (Vector3) node.position;
    //    //    nodePos = new Vector3(Mathf.Floor(nodePos.x), Mathf.Floor(nodePos.y), Mathf.Floor(nodePos.z));
    //    //    for (int i = 0; i < _marker.terrifyingCharacters.Count; i++) {
    //    //        Character terrifyingCharacter = _marker.terrifyingCharacters[i];
    //    //        if (terrifyingCharacter.currentParty.icon.isTravelling && terrifyingCharacter.currentParty.icon.travelLine != null) {
    //    //            continue;
    //    //        }
    //    //        if (!terrifyingCharacter.isDead) {
    //    //            //if(terrifyingCharacter.gridTileLocation.worldLocation == nodePos) {
    //    //            //    return false;
    //    //            //}
    //    //            List<LocationGridTile> tilesInRadius = terrifyingCharacter.GetTilesInRadius(1, 0, true);
    //    //            for (int j = 0; j < tilesInRadius.Count; j++) {
    //    //                LocationGridTile currentTileToCheck = tilesInRadius[j];
    //    //                if (currentTileToCheck.worldLocation == nodePos) {
    //    //                    return false;
    //    //                }
    //    //            }
    //    //        }
    //    //    }
    //    //}
    //    return true;
    //}
}
