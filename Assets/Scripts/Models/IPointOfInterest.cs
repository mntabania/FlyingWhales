using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Traits;

public interface IPointOfInterest : ITraitable, ISelectable {
    new string name { get; }
    int id { get; } //Be careful with how you handle this since this can duplicate depending on its poiType
    string nameWithID { get; }
    GameObject visualGO { get; }
    POINT_OF_INTEREST_TYPE poiType { get; }
    POI_STATE state { get; }
    Region currentRegion { get; }
    new LocationGridTile gridTileLocation { get; }
    List<INTERACTION_TYPE> advertisedActions { get; }
    List<JobQueueItem> allJobsTargetingThis { get; }
    Dictionary<RESOURCE, int> storedResources { get; }
    Faction factionOwner { get; }
    Character characterOwner { get; }
    bool isDisabledByPlayer { get; }
    //Vector3 worldPosition { get; }
    bool isDead { get; }
    Character isBeingCarriedBy { get; }
    LogComponent logComponent { get; }

    void SetGridTileLocation(LocationGridTile tile);
    void AddJobTargetingThis(JobQueueItem job);
    bool RemoveJobTargetingThis(JobQueueItem job);
    bool HasJobTargetingThis(params JOB_TYPE[] jobType);
    void SetPOIState(POI_STATE state);
    void SetIsDisabledByPlayer(bool state);
    bool IsAvailable();
    LocationGridTile GetNearestUnoccupiedTileFromThis();
    GoapAction AdvertiseActionsToActor(Character actor, GoapEffect precondition, JobQueueItem job,
        Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost, ref string log);
    bool CanAdvertiseActionToActor(Character actor, GoapAction action, JobQueueItem job,
        Dictionary<INTERACTION_TYPE, object[]> otherData, ref int cost);
    bool IsValidCombatTarget();
    bool IsStillConsideredPartOfAwarenessByCharacter(Character character);
    void OnPlacePOI();
    void OnDestroyPOI();
    void ConstructResources();
    void SetResource(RESOURCE resourceType, int amount);
    void AdjustResource(RESOURCE resourceType, int amount);
    bool HasResourceAmount(RESOURCE resourceType, int amount);
    void OnSeizePOI();
    void OnUnseizePOI(LocationGridTile tileLocation);
}

/// <summary>
/// Helper struct to contain data of generic POI's for saving and loading.
/// Usage Example:
///  - When a class has a list of poi's that it needs to save/load use this. (List<IPointOfInterest>)
/// </summary>
[System.Serializable]
public class POIData {
    public int poiID;
    public int areaID; //npcSettlement location
    public POINT_OF_INTEREST_TYPE poiType;
    public TILE_OBJECT_TYPE tileObjectType; //The type of tile object that this is, should only be used if poi type is TILE_OBJECT
    // public SPECIAL_TOKEN specialTokenType; //The type of item that this is, should only be used if poi type is ITEM

    public Vector3 genericTileObjectPlace; //used for generic tile objects, use this instead of id. NOTE: Generic Tile objects must ALWAYS have an areaID

    public POIData() { }

    public POIData(IPointOfInterest poi) {
        poiID = poi.id;
        if (poi.gridTileLocation == null) {
            areaID = -1;
        } else {
            areaID = poi.gridTileLocation.parentMap.region.id;
        }
        poiType = poi.poiType;
        tileObjectType = TILE_OBJECT_TYPE.NONE;
        // specialTokenType = default(SPECIAL_TOKEN);
        genericTileObjectPlace = Vector3.zero;

        if (poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            tileObjectType = (poi as TileObject).tileObjectType;
            if (tileObjectType == TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                genericTileObjectPlace = poi.gridTileLocation.localPlace;
            }
        } 
        // else if (poiType == POINT_OF_INTEREST_TYPE.ITEM) {
        //     specialTokenType = (poi as SpecialToken).specialTokenType;
        // }
    }

    public override string ToString() {
        string name = $"{poiType} {poiID}.";
        if (poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            name += $" Tile Object Type: {tileObjectType}";
        }
        // else if (poiType == POINT_OF_INTEREST_TYPE.ITEM) {
        //     name += " Item Type: " + specialTokenType.ToString();
        // }
        return name;
    }
}