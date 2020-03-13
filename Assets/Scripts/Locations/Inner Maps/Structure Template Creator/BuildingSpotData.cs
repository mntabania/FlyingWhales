using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the building spot used for initial map generation and structure
/// template creation. This will be converted to a Building Spot in the actual game.
/// </summary>
[System.Serializable]
public struct BuildingSpotData {

    public int id;
    public Vector3Int location; //where in the template grid is this connector placed
    public bool isOpen; //is this connection still open?
    public int[] adjacentSpots;
    public Vector2Int buildingSpotGridPos; //where in the building spot grid is this spot?

    public Vector3Int Difference(BuildingSpotData otherConnector) {
        return new Vector3Int(location.x - otherConnector.location.x, location.y - otherConnector.location.y, 0);
    }
    public void SetIsOpen(bool state) {
        isOpen = state;
    }
    //public void SetAdjacentSpotsAsOpen(StructureTemplate structureTemplate, List<BuildingSpotData> except) {
    //    if (adjacentSpots != null) {
    //        for (int i = 0; i < adjacentSpots.Length; i++) {
    //            int currID = adjacentSpots[i];
    //            BuildingSpotData spot = structureTemplate.GetBuildingSpotWithID(currID);
    //            if (except.Contains(spot) == false) {
    //                spot.SetIsOpen(true);
    //            }
    //        }
    //    }
    //}
}
