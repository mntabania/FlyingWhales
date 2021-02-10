using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Locations;
using Locations.Settlements;
using Locations.Tile_Features;
using Inner_Maps;
using Inner_Maps.Location_Structures;


public class Area: IPlayerActionTarget, IPartyTargetDestination, ILocation {
    public AreaData areaData { get; private set; }

    public Region region { get; private set; }
    public BaseSettlement settlementOnArea { get; private set; }
    public RegionDivision regionDivision { get; protected set; }

    /// <summary>
    /// Number of blueprint LocationGridTiles on this.
    /// NOTE: This is not saved because this is filled out by <see cref="LocationGridTile.SetHasBlueprint"/>
    /// </summary>
    private int _blueprintsOnTile;
    public int freezingTraps { get; private set; }

    //Components
    public TileFeatureComponent featureComponent { get; private set; }
    public LocationAwareness locationAwareness { get; private set; }
    public LocationCharacterTracker locationCharacterTracker { get; private set; }
    public AreaSpellsComponent spellsComponent { get; private set; }
    public AreaBiomeEffectTrigger biomeEffectTrigger { get; private set; }
    public AreaGridTileComponent gridTileComponent { get; private set; }
    public AreaNeighbourComponent neighbourComponent { get; private set; }
    public AreaTileObjectComponent tileObjectComponent { get; private set; }
    public AreaBiomeComponent biomeComponent { get; private set; }

    #region getters
    public string name => locationName;
    public string locationName => $"Area {areaData.xCoordinate}, {areaData.yCoordinate}";
    public string persistentID => areaData.persistentID;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Area;
    public Type serializedData => typeof(SaveDataHextile);
    public int id => areaData.id;
    public float elevationNoise => areaData.elevationNoise;
    public float moistureNoise => areaData.moistureNoise;
    public float temperature => areaData.temperature;
    public BIOMES biomeType => areaData.biomeType;
    public ELEVATION elevationType => areaData.elevationType;
    public bool hasBeenDestroyed => false;
    public PARTY_TARGET_DESTINATION_TYPE partyTargetDestinationType => PARTY_TARGET_DESTINATION_TYPE.Area;
    #endregion

    public Area (int id, int x, int y) {
        areaData.persistentID = System.Guid.NewGuid().ToString();
        areaData.id = id;
        areaData.xCoordinate = x;
        areaData.yCoordinate = y;

        //Components
        locationCharacterTracker = new LocationCharacterTracker();
        locationAwareness = new LocationAwareness();
        featureComponent = new TileFeatureComponent();
        spellsComponent = new AreaSpellsComponent(); spellsComponent.SetOwner(this);
        biomeEffectTrigger = new AreaBiomeEffectTrigger(); biomeEffectTrigger.SetOwner(this);
        gridTileComponent = new AreaGridTileComponent(); gridTileComponent.SetOwner(this);
        neighbourComponent = new AreaNeighbourComponent(); neighbourComponent.SetOwner(this);
        tileObjectComponent = new AreaTileObjectComponent(); tileObjectComponent.SetOwner(this);
        biomeComponent = new AreaBiomeComponent(); biomeComponent.SetOwner(this);
    }

    public Area (SaveDataArea data) {
        areaData = data.areaData;

        spellsComponent = data.spellsComponent.Load(); spellsComponent.SetOwner(this);
        biomeEffectTrigger = data.biomeEffectTrigger.Load(); biomeEffectTrigger.SetOwner(this);
        gridTileComponent = new AreaGridTileComponent(); gridTileComponent.SetOwner(this);
        neighbourComponent = new AreaNeighbourComponent(); neighbourComponent.SetOwner(this);
        tileObjectComponent = new AreaTileObjectComponent(); tileObjectComponent.SetOwner(this);
        biomeComponent = new AreaBiomeComponent(); biomeComponent.SetOwner(this);
        locationCharacterTracker = new LocationCharacterTracker();
        locationAwareness = new LocationAwareness();
        featureComponent = new TileFeatureComponent();
    }

    #region Elevation
    public void SetElevation(ELEVATION elevationType) {
        areaData.elevationType = elevationType;
    }
    #endregion

    #region Area Utilities
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
        return settlementOnArea != null && settlementOnArea.HasResidentThatMeetsCriteria(resident => !resident.isDead && resident.isNormalCharacter);
    }
    public LocationStructure GetMostImportantStructureOnTile() {
        LocationStructure mostImportant = region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> pair in region.structures) {
            for (int i = 0; i < pair.Value.Count; i++) {
                if (pair.Key == STRUCTURE_TYPE.WILDERNESS) {
                    continue;
                }
                LocationStructure structure = pair.Value[i];
                if (structure.HasTileOnHexTile(this)) {
                    int value = pair.Key.StructurePriority();
                    if (value > mostImportant.structureType.StructurePriority()) {
                        mostImportant = structure;
                    }
                }

                // if (structure is Cave cave) {
                //     if (cave.occupiedHexTile != null && cave.caveHexTiles.Contains(innerMapHexTile)) {
                //         int value = pair.Key.StructurePriority(); 
                //         if (value > mostImportant.structureType.StructurePriority()) {
                //             mostImportant = structure;
                //         }    
                //     }
                // } else {
                //     if (structure.occupiedHexTile != null && structure.occupiedHexTile == innerMapHexTile) {
                //         int value = pair.Key.StructurePriority(); 
                //         if (value > mostImportant.structureType.StructurePriority()) {
                //             mostImportant = structure;
                //         }    
                //     }
                // }
            }
        }

        return mostImportant;
    }
    #endregion

    #region Region
    public void SetRegion(Region region) {
        this.region = region;
    }
    #endregion

    #region Region Division
    public void SetRegionDivision(RegionDivision p_regionDivision) {
        regionDivision = p_regionDivision;
    }
    #endregion

    #region Settlement
    public void SetSettlementOnArea(BaseSettlement settlement) {
        settlementOnArea = settlement;
        region.UpdateSettlementsInRegion();

        //TODO:
        //if (GameManager.Instance.gameHasStarted) {
        //    UpdatePathfindingGraphOnTile();
        //}
    }
    public void CheckIfSettlementIsStillOnArea() {
        if (settlementOnArea != null) {
            for (int i = 0; i < settlementOnArea.allStructures.Count; i++) {
                LocationStructure structure = settlementOnArea.allStructures[i];
                if (structure.HasTileOnHexTile(this)) {
                    return; //there is still a structure on this hex tile.
                }
            }
            //if code reaches this, then there is no longer a structure from the settlement on this tile
            settlementOnArea.RemoveTileFromSettlement(this);
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
            Messenger.Broadcast(SpellSignals.PLAYER_ACTION_ADDED_TO_TARGET, action, this as IPlayerActionTarget);
        }
    }
    public void RemovePlayerAction(PLAYER_SKILL_TYPE action) {
        if (actions.Remove(action)) {
            Messenger.Broadcast(SpellSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
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
            tileObjectComponent.RemoveItemInArea(item);
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
        _blueprintsOnTile++;
    }
    public bool HasBlueprintOnTile() {
        return _blueprintsOnTile > 0;
    }
    #endregion
}

[System.Serializable]
public class SaveDataArea : SaveData<Area> {
    public AreaData areaData;

    //Tile Features
    public List<SaveDataTileFeature> tileFeatureSaveData;

    //Components
    public SaveDataAreaSpellsComponent spellsComponent;
    public SaveDataAreaBiomeEffectTrigger biomeEffectTrigger;

    public override void Save(Area p_data) {
        areaData = p_data.areaData;

        //tile features
        tileFeatureSaveData = new List<SaveDataTileFeature>();
        for (int i = 0; i < p_data.featureComponent.features.Count; i++) {
            TileFeature feature = p_data.featureComponent.features[i];
            SaveDataTileFeature saveDataTileFeature = SaveManager.ConvertTileFeatureToSaveData(feature);
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