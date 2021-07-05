using System;
using System.Collections;
using System.Collections.Generic;
using Locations;
using Locations.Settlements;
using Locations.Area_Features;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class Area: IPlayerActionTarget, IPartyTargetDestination, ILocation {
    public AreaData areaData { get; private set; }

    public Region region { get; private set; }
    public BaseSettlement settlementOnArea { get; private set; }
    // public BiomeDivision biomeDivision { get; protected set; }
    public AreaItem areaItem { get; private set; }

    /// <summary>
    /// Number of blueprint LocationGridTiles on this.
    /// NOTE: This is not saved because this is filled out by <see cref="LocationGridTile.SetHasBlueprint"/>
    /// </summary>
    private int _blueprintsOnTile;
    public int freezingTraps { get; private set; }

    //Components
    public AreaFeatureComponent featureComponent { get; private set; }
    public LocationAwareness locationAwareness { get; private set; }
    public LocationCharacterTracker locationCharacterTracker { get; private set; }
    public AreaSpellsComponent spellsComponent { get; private set; }
    public AreaBiomeEffectTrigger biomeEffectTrigger { get; private set; }
    public AreaGridTileComponent gridTileComponent { get; private set; }
    public AreaNeighbourComponent neighbourComponent { get; private set; }
    public AreaTileObjectComponent tileObjectComponent { get; private set; }
    public AreaBiomeComponent biomeComponent { get; private set; }
    public AreaStructureComponent structureComponent { get; private set; }
    public AreaElevationComponent elevationComponent { get; }

    #region getters
    public string name => locationName;
    public string locationName => $"Area {areaData.xCoordinate.ToString()}, {areaData.yCoordinate.ToString()}";
    public string persistentID => areaData.persistentID;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Area;
    public Type serializedData => typeof(SaveDataArea);
    public int id => areaData.id;
    public BIOMES biomeType => biomeComponent.biomeType;
    public ELEVATION elevationType => elevationComponent.elevationType;  //areaData.elevationType;
    public bool hasBeenDestroyed => false;
    public PARTY_TARGET_DESTINATION_TYPE partyTargetDestinationType => PARTY_TARGET_DESTINATION_TYPE.Area;
    public LocationStructure primaryStructureInArea => structureComponent.GetMostImportantStructureOnTile();
    public Vector3 worldPosition {
        get {
            Vector2 pos = areaItem.transform.position;
            // pos.x += 3.5f;
            // pos.y += 3.5f;
            return pos;
        }
    }
    #endregion

    public Area (int id, int x, int y) {
        areaData = new AreaData {persistentID = System.Guid.NewGuid().ToString(), id = id, xCoordinate = x, yCoordinate = y};

        //Components
        locationCharacterTracker = new LocationCharacterTracker();
        locationAwareness = new LocationAwareness();
        featureComponent = new AreaFeatureComponent();
        spellsComponent = new AreaSpellsComponent(); spellsComponent.SetOwner(this);
        biomeEffectTrigger = new AreaBiomeEffectTrigger(); biomeEffectTrigger.SetOwner(this);
        gridTileComponent = new AreaGridTileComponent(); gridTileComponent.SetOwner(this);
        neighbourComponent = new AreaNeighbourComponent(); neighbourComponent.SetOwner(this);
        tileObjectComponent = new AreaTileObjectComponent(); tileObjectComponent.SetOwner(this);
        biomeComponent = new AreaBiomeComponent(); biomeComponent.SetOwner(this);
        structureComponent = new AreaStructureComponent(); structureComponent.SetOwner(this);
        elevationComponent = new AreaElevationComponent(); elevationComponent.SetOwner(this);
    }

    public Area (SaveDataArea data) {
        areaData = data.areaData;

        if (data.spellsComponent != null) {
            spellsComponent = data.spellsComponent.Load(); spellsComponent.SetOwner(this);    
        } else {
            spellsComponent = new AreaSpellsComponent(); spellsComponent.SetOwner(this);
        }
        if (data.biomeEffectTrigger != null) {
            biomeEffectTrigger = data.biomeEffectTrigger.Load(); biomeEffectTrigger.SetOwner(this);
        } else {
            biomeEffectTrigger = new AreaBiomeEffectTrigger(); biomeEffectTrigger.SetOwner(this);
        }
        
        gridTileComponent = new AreaGridTileComponent(); gridTileComponent.SetOwner(this);
        neighbourComponent = new AreaNeighbourComponent(); neighbourComponent.SetOwner(this);
        tileObjectComponent = new AreaTileObjectComponent(); tileObjectComponent.SetOwner(this);
        biomeComponent = new AreaBiomeComponent(); biomeComponent.SetOwner(this);
        structureComponent = new AreaStructureComponent(); structureComponent.SetOwner(this);
        elevationComponent = new AreaElevationComponent(); elevationComponent.SetOwner(this);
        locationCharacterTracker = new LocationCharacterTracker();
        locationAwareness = new LocationAwareness();
        featureComponent = new AreaFeatureComponent();
    }
    public override string ToString() {
        return $"{locationName} - {elevationType.ToString()} - {region?.name ?? "No Region"}";
    }

    #region Area Utilities
    public void SetAreaItem(AreaItem p_areaItem) {
        areaItem = p_areaItem;
        gridTileComponent.PopulateBorderTiles(this);
    }
    public bool IsNextToOrPartOfVillage() {
        return IsPartOfVillage() || neighbourComponent.IsNextToVillage();
    }
    public bool IsPartOfVillage() {
        return settlementOnArea != null && settlementOnArea.locationType == LOCATION_TYPE.VILLAGE;
    }
    public bool IsPartOfVillage(out BaseSettlement settlement) {
        settlement = settlementOnArea;
        return settlementOnArea != null && settlementOnArea.locationType == LOCATION_TYPE.VILLAGE;
    }
    public void PopulateAreasInRange(List<Area> areas, int range, bool includeCenterTile = false) {
        Area[,] areaMap = region.areaMap;
        int mapSizeX = areaMap.GetUpperBound(0);
        int mapSizeY = areaMap.GetUpperBound(1);
        int x = areaData.xCoordinate;
        int y = areaData.yCoordinate;

        if (includeCenterTile) {
            areas.Add(this);
        }

        for (int dx = x - range; dx <= x + range; dx++) {
            for (int dy = y - range; dy <= y + range; dy++) {
                if (dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                    if (dx == x && dy == y) {
                        continue;
                    }
                    Area result = areaMap[dx, dy];
                    areas.Add(result);
                }
            }
        }
    }
    public bool HasAliveVillagerResident() {
        //Does not count if hextile is only a territory
        return settlementOnArea != null && settlementOnArea.HasResidentThatIsVillagerAndNotDead();
    }
    public int GetAreaDistanceTo(Area p_targetArea) {
        LocationGridTile targetCenterTile = p_targetArea.gridTileComponent.centerGridTile;
        LocationGridTile sourceCenterTile = gridTileComponent.centerGridTile;

        float gridTileDistance = sourceCenterTile.GetDistanceTo(targetCenterTile);

        //Divide the center grid tile distance with the size of the area so we can get the area distance
        //Example: if the target area is adjacent to the right of the source area, then the distance of the 2 center tiles should be 14 since the radius is 7
        //meaning from source center tile to the right edge then left edge of target to its center tile
        //So if we divide 14 from 14 the answer is 1, which is correct.
        float areaDistance = gridTileDistance / InnerMapManager.AreaLocationGridTileSize.x;
        return (int) areaDistance;
    }
    #endregion

    #region Region
    public void SetRegion(Region region) {
        this.region = region;
    }
    #endregion

    #region Settlement
    public void SetSettlementOnArea(BaseSettlement settlement) {
        settlementOnArea = settlement;
        region.UpdateSettlementsInRegion();
        if (GameManager.Instance.gameHasStarted) {
             areaItem.UpdatePathfindingGraph();
        }
    }
    public void CheckIfSettlementIsStillOnArea() {
        if (settlementOnArea != null) {
            for (int i = 0; i < settlementOnArea.allStructures.Count; i++) {
                LocationStructure structure = settlementOnArea.allStructures[i];
                if (structure.HasTileOnArea(this)) {
                    return; //there is still a structure on this hex tile.
                }
            }
            //if code reaches this, then there is no longer a structure from the settlement on this tile
            settlementOnArea.RemoveAreaFromSettlement(this);
        }
    }
    #endregion

    #region IPlayerActionTarget
    public List<PLAYER_SKILL_TYPE> actions { get; private set; }
    public void ConstructDefaultActions() {
        actions = new List<PLAYER_SKILL_TYPE>();
        //PlayerAction harassAction = new PlayerAction(PlayerDB.Harass_Action, CanDoHarass, IsHarassRaidInvadeValid, () => PlayerUI.Instance.OnClickHarassRaidInvade(this, "harass"));
        //PlayerAction raidAction = new PlayerAction(PlayerDB.Raid_Action, CanDoRaid, IsHarassRaidInvadeValid, () => PlayerUI.Instance.OnClickHarassRaidInvade(this, "raid"));
        //PlayerAction invadeAction = new PlayerAction(PlayerDB.Invade_Action, CanDoInvade, IsHarassRaidInvadeValid, () => PlayerUI.Instance.OnClickHarassRaidInvade(this, "invade"));
        //PlayerAction buildAction = new PlayerAction(PlayerDB.Build_Demonic_Structure_Action, () => true, CanBuildDemonicStructure, OnClickBuild);

        // AddPlayerAction(SPELL_TYPE.HARASS);
        //AddPlayerAction(SPELL_TYPE.DEFEND);
        // AddPlayerAction(SPELL_TYPE.INVADE);
        // AddPlayerAction(SPELL_TYPE.BUILD_DEMONIC_STRUCTURE);
    }
    public void AddPlayerAction(PLAYER_SKILL_TYPE action) {
        if (actions.Contains(action) == false) {
            actions.Add(action);
            Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_ADDED_TO_TARGET, action, this as IPlayerActionTarget);
        }
    }
    public void RemovePlayerAction(PLAYER_SKILL_TYPE action) {
        if (actions.Remove(action)) {
            Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
        }
    }
    public void ClearPlayerActions() {
        actions.Clear();
    }
    #endregion

    #region POI
    public void OnPlacePOIInHex(IPointOfInterest poi) {
        spellsComponent.OnPlacePOIInHex(poi);
        if (poi is TileObject item) { //&& item.tileObjectType.IsTileObjectAnItem(
            tileObjectComponent.AddItemInArea(item);
        }
    }
    public void OnRemovePOIInHex(IPointOfInterest poi) {
        spellsComponent.OnRemovePOIInHex(poi);
        if (poi is TileObject item) { // && item.tileObjectType.IsTileObjectAnItem()
            if (tileObjectComponent.RemoveItemInArea(item)) {
                // Debug.Log($"Removed item {poi} in area {this.ToString()}");    
            }
        }
    }
    #endregion

    #region IPartyTargetDestination
    public LocationGridTile GetRandomPassableTile() {
        return gridTileComponent.GetRandomPassableTile();
    }
    public bool IsAtTargetDestination(Character character) {
        return character.gridTileLocation != null && character.gridTileLocation.area == this;
    }
    #endregion

    #region Freezing Trap
    public void AddFreezingTrapInArea() {
        freezingTraps++;
    }
    public void RemoveFreezingTrapInArea() {
        freezingTraps--;
    }
    #endregion

    #region Blueprints
    public void AddBlueprint() {
        _blueprintsOnTile++;
    }
    public void RemoveBlueprint() {
        _blueprintsOnTile--;
    }
    public bool HasBlueprintOnTile() {
        return _blueprintsOnTile > 0;
    }
    #endregion

    #region Village Spot
    /// <summary>
    /// Is this area reserved be a village spot that is not the provided one.
    /// </summary>
    /// <param name="p_villageSpot">The village spot to check against.</param>
    /// <returns>true or false</returns>
    public bool IsReservedByOtherVillage(VillageSpot p_villageSpot) {
        return GetOccupyingVillageSpot() != p_villageSpot;
    }
    public VillageSpot GetOccupyingVillageSpot() {
        for (int i = 0; i < GridMap.Instance.mainRegion.villageSpots.Count; i++) {
            VillageSpot spot = GridMap.Instance.mainRegion.villageSpots[i];
            if (spot.reservedAreas.Contains(this)) {
                return spot;
            }
        }
        return null;
    }
    #endregion
    
}

[System.Serializable]
public class SaveDataArea : SaveData<Area> {
    public AreaData areaData;

    //Tile Features
    public List<SaveDataAreaFeature> tileFeatureSaveData;

    //Components
    public SaveDataAreaSpellsComponent spellsComponent;
    public SaveDataAreaBiomeEffectTrigger biomeEffectTrigger;

    public override void Save(Area p_data) {
        areaData = p_data.areaData;

        //tile features
        tileFeatureSaveData = new List<SaveDataAreaFeature>();
        for (int i = 0; i < p_data.featureComponent.features.Count; i++) {
            AreaFeature feature = p_data.featureComponent.features[i];
            SaveDataAreaFeature saveDataTileFeature = SaveManager.ConvertAreaFeatureToSaveData(feature);
            saveDataTileFeature.Save(feature);
            tileFeatureSaveData.Add(saveDataTileFeature);
        }
        spellsComponent = new SaveDataAreaSpellsComponent(); spellsComponent.Save(p_data.spellsComponent);
        biomeEffectTrigger = new SaveDataAreaBiomeEffectTrigger(); biomeEffectTrigger.Save(p_data.biomeEffectTrigger);
    }
    public override Area Load() {
        return new Area(this);
    }
}

// [System.Serializable]
// public class SaveDataAreaNew : SaveData<Area> {
//     public AreaData areaData;
//
//     //Tile Features
//     public List<SaveDataAreaFeature> tileFeatureSaveData;
//
//     //Components
//     public SaveDataAreaSpellsComponent spellsComponent;
//     public SaveDataAreaBiomeEffectTrigger biomeEffectTrigger;
//
//     public override void Save(Area p_data) {
//         areaData = p_data.areaData;
//
//         //tile features
//         tileFeatureSaveData = new List<SaveDataAreaFeature>();
//         for (int i = 0; i < p_data.featureComponent.features.Count; i++) {
//             AreaFeature feature = p_data.featureComponent.features[i];
//             SaveDataAreaFeature saveDataTileFeature = SaveManager.ConvertAreaFeatureToSaveData(feature);
//             saveDataTileFeature.Save(feature);
//             tileFeatureSaveData.Add(saveDataTileFeature);
//         }
//         spellsComponent = new SaveDataAreaSpellsComponent(); spellsComponent.Save(p_data.spellsComponent);
//         biomeEffectTrigger = new SaveDataAreaBiomeEffectTrigger(); biomeEffectTrigger.Save(p_data.biomeEffectTrigger);
//     }
//     public override Area Load() {
//         // return new Area(this);
//         return null;
//     }
// }