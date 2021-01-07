using System.Collections.Generic;
using System.Collections;
using Locations;
using Inner_Maps;
using System;
public static class LocationAwarenessUtility
{
    public static List<ILocationAwareness> allLocationsToBeUpdated = new List<ILocationAwareness>();

    /*
     * this function will add the awareness to pending awareness list
     * */
    public static void AddToAwarenessList(IPointOfInterest targetAwareness, LocationGridTile gridTileLocation) {
        if (gridTileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS && gridTileLocation.structure.structureType != STRUCTURE_TYPE.OCEAN) {
            gridTileLocation.structure.locationAwareness.AddPendingAwarenessToPendingAddList(targetAwareness);
            gridTileLocation.structure.locationAwareness.RemovePendingAwarenessFromPendingRemoveList(targetAwareness);
            allLocationsToBeUpdated.Add(gridTileLocation.structure.locationAwareness);
        } else if (gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
            gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.LocationAwareness.AddPendingAwarenessToPendingAddList(targetAwareness);
            gridTileLocation.structure.locationAwareness.RemovePendingAwarenessFromPendingRemoveList(targetAwareness);
            allLocationsToBeUpdated.Add(gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.LocationAwareness);
        }
    }

    public static void RemoveFromAwarenessList(IPointOfInterest targetAwareness, LocationGridTile gridTileLocation) {
        if (gridTileLocation.structure.structureType != STRUCTURE_TYPE.WILDERNESS && gridTileLocation.structure.structureType != STRUCTURE_TYPE.OCEAN) {
            gridTileLocation.structure.locationAwareness.AddPendingAwarenessToPendingRemoveList(targetAwareness);
            gridTileLocation.structure.locationAwareness.RemovePendingAwarenessFromPendingAddList(targetAwareness);
            allLocationsToBeUpdated.Add(gridTileLocation.structure.locationAwareness);
        } else if (gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
            gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.LocationAwareness.AddPendingAwarenessToPendingRemoveList(targetAwareness);
            gridTileLocation.structure.locationAwareness.RemovePendingAwarenessFromPendingAddList(targetAwareness);
            allLocationsToBeUpdated.Add(gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.LocationAwareness);
        }
    }

    public static void UpdateAllPendingAwareness() {
        for (int i = 0; i < allLocationsToBeUpdated.Count; i++) {
            allLocationsToBeUpdated[i].UpdateAwareness();
        }
        allLocationsToBeUpdated.Clear();
    }

    public static IEnumerator UpdateAllPendingAwarenessThread() {
        for (int i = 0; i < allLocationsToBeUpdated.Count; i++) {
            allLocationsToBeUpdated[i].UpdateAwareness();
            yield return 0;
        }
        allLocationsToBeUpdated.Clear();
    }
}