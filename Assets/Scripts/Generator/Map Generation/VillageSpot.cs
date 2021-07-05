using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Grid_Tile_Features;
using Inner_Maps.Location_Structures;
using Locations.Area_Features;
using UnityEngine;
using UtilityScripts;
using Locations.Settlements;

public class VillageSpot {
    public Area mainSpot { get; }
    /// <summary>
    /// All reserved ares of village spot.
    /// NOTE: This includes the mainSpot.
    /// </summary>
    public List<Area> reservedAreas { get; }
    public int lumberyardSpots { get; }
    public int miningSpots { get; }
    public List<string> linkedBeastDens { get; private set; }
    public Area migrationSpawningArea { get; private set; }

    #region getters
    public int loggerCapacity => lumberyardSpots;
    public int minerCapacity => miningSpots;
    #endregion

    public VillageSpot(Area p_spot, List<Area> p_areas, int p_lumberyardSpots, int p_miningSpots) {
        mainSpot = p_spot;
        reservedAreas = new List<Area>(p_areas);
        lumberyardSpots = p_lumberyardSpots;
        miningSpots = p_miningSpots;
        linkedBeastDens = new List<string>();
    }
    public VillageSpot(Area p_spot, int p_lumberyardSpots, int p_miningSpots) {
        mainSpot = p_spot;
        reservedAreas = new List<Area> {p_spot};
        lumberyardSpots = p_lumberyardSpots;
        miningSpots = p_miningSpots;
        linkedBeastDens = new List<string>();
    }
    public VillageSpot(SaveDataVillageSpot p_data) {
        mainSpot = GameUtilities.GetHexTileGivenCoordinates(p_data.mainArea, GridMap.Instance.map);
        reservedAreas = GameUtilities.GetHexTilesGivenCoordinates(p_data.reservedAreas, GridMap.Instance.map);
        lumberyardSpots = p_data.lumberyardSpots;
        miningSpots = p_data.miningSpots;
        linkedBeastDens = p_data.linkedBeastDens;
        if (linkedBeastDens == null) {
            linkedBeastDens = new List<string>();
        }
        if (!reservedAreas.Contains(mainSpot)) {
            reservedAreas.Add(mainSpot);
        }
        migrationSpawningArea = GameUtilities.GetHexTileGivenCoordinates(p_data.migrationSpawningArea, GridMap.Instance.map);
    }
    public override string ToString() {
        return mainSpot.ToString();
    }
    public void ColorVillageSpots(Color p_color) {
        p_color.a = 0.8f;
        for (int i = 0; i < reservedAreas.Count; i++) {
            Area area = reservedAreas[i];
            ColorArea(area, p_color);
        }
        Color color = Color.black;
        color.a = 0.8f;
        ColorArea(mainSpot, color);

        // p_color.a = 0.5f;
        // ColorArea(migrationSpawningArea, p_color);
    }
    public void ColorArea(Area p_area, Color p_color) {
        for (int i = 0; i < p_area.gridTileComponent.gridTiles.Count; i++) {
            LocationGridTile tile = p_area.gridTileComponent.gridTiles[i];
            tile.parentMap.perlinTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.assetManager.grassTile);
            tile.parentMap.perlinTilemap.SetColor(tile.localPlace, p_color);
        }
    }
    public void AddWaterAreas(List<Area> p_areas) {
        reservedAreas.AddRange(p_areas);
        // color.a = 0.8f;
        // for (int i = 0; i < p_areas.Count; i++) {
        //     Area area = p_areas[i];
        //     ColorArea(area, color);
        // }
    }
    public void AddCaveAreas(List<Area> p_areas) {
        reservedAreas.AddRange(p_areas);
        // color.a = 0.8f;
        // for (int i = 0; i < p_areas.Count; i++) {
        //     Area area = p_areas[i];
        //     ColorArea(area, color);
        // }
    }
    public bool CanAccommodateFaction(FACTION_TYPE p_factionType) {
        switch (p_factionType) {
            case FACTION_TYPE.Elven_Kingdom:
                return lumberyardSpots > 0;
            case FACTION_TYPE.Human_Empire:
                return miningSpots > 0;
            case FACTION_TYPE.Vampire_Clan:
                return lumberyardSpots > 0 || miningSpots > 0;
            case FACTION_TYPE.Lycan_Clan:
                return lumberyardSpots > 0 || miningSpots > 0;
            case FACTION_TYPE.Demon_Cult:
                return lumberyardSpots > 0 || miningSpots > 0;
            default:
                return true;
        }
    }

    #region Resources
    public bool HasUnusedFishingSpot() {
        for (int i = 0; i < reservedAreas.Count; i++) {
            Area area = reservedAreas[i];
            if (area.elevationComponent.HasElevation(ELEVATION.WATER)) {
                for (int j = 0; j < area.tileObjectComponent.itemsInArea.Count; j++) {
                    TileObject item = area.tileObjectComponent.itemsInArea[j];
                    if (item is FishingSpot fishingSpot && fishingSpot.structureConnector != null && fishingSpot.structureConnector.isOpen) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public bool HasAccessToSkinnerAnimals() {
        for (int i = 0; i < reservedAreas.Count; i++) {
            Area area = reservedAreas[i];
            if (area.structureComponent.HasStructureInArea(GameUtilities.skinnerStructures)) {
                return true;
            }
        }
        return false;
    }
    public bool HasAccessToButcherAnimals() {
        for (int i = 0; i < reservedAreas.Count; i++) {
            Area area = reservedAreas[i];
            if (area.featureComponent.HasFeature(AreaFeatureDB.Game_Feature) || 
                area.structureComponent.HasStructureInArea(STRUCTURE_TYPE.RABBIT_HOLE)) {
                return true;
            }
        }
        return false;
    }
    public bool HasUnusedMiningSpots() {
        for (int i = 0; i < reservedAreas.Count; i++) {
            Area area = reservedAreas[i];
            if (area.elevationComponent.HasElevation(ELEVATION.MOUNTAIN)) {
                for (int j = 0; j < area.structureComponent.structureConnectors.Count; j++) {
                    StructureConnector structureConnector = area.structureComponent.structureConnectors[j];
                    //NOTE: Did not add null checking for structureConnector.tileLocation since I expect that all structure connectors
                    //in an area should have a tile location. Also added checking for isPartOfLocationStructureObject so that structure connectors
                    //that are part of settlement structures will not be counted as mining spots, even though they are inside a cave.
                    if (structureConnector.tileLocation.structure is Cave && !structureConnector.isPartOfLocationStructureObject) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public bool HasUnusedMiningSpotsThatSettlementHasNotYetConnectedTo(NPCSettlement p_settlement) {
        for (int i = 0; i < reservedAreas.Count; i++) {
            Area area = reservedAreas[i];
            if (area.elevationComponent.HasElevation(ELEVATION.MOUNTAIN)) {
                for (int j = 0; j < area.structureComponent.structureConnectors.Count; j++) {
                    StructureConnector structureConnector = area.structureComponent.structureConnectors[j];
                    //NOTE: Did not add null checking for structureConnector.tileLocation since I expect that all structure connectors
                    //in an area should have a tile location. Also added checking for isPartOfLocationStructureObject so that structure connectors
                    //that are part of settlement structures will not be counted as mining spots, even though they are inside a cave.
                    if (structureConnector.tileLocation.structure is Cave cave && !structureConnector.isPartOfLocationStructureObject && !cave.IsConnectedToSettlement(p_settlement)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public bool HasUnusedLumberyardSpots() {
        BigTreeSpotFeature bigTreeSpotFeature = GridMap.Instance.mainRegion.gridTileFeatureComponent.GetFeature<BigTreeSpotFeature>();
        SmallTreeSpotFeature smallTreeSpotFeature = GridMap.Instance.mainRegion.gridTileFeatureComponent.GetFeature<SmallTreeSpotFeature>();
        
        for (int i = 0; i < reservedAreas.Count; i++) {
            Area area = reservedAreas[i];
            List<LocationGridTile> bigTreeTiles = bigTreeSpotFeature.GetFeatureTilesInArea(area);
            List<LocationGridTile> smallTreeTiles = smallTreeSpotFeature.GetFeatureTilesInArea(area);
            if (bigTreeTiles != null) {
                for (int j = 0; j < bigTreeTiles.Count; j++) {
                    LocationGridTile tile = bigTreeTiles[j];
                    if (tile.tileObjectComponent.objHere is TreeObject treeObject && treeObject.structureConnector != null && treeObject.structureConnector.isOpen) {
                        return true;
                    }
                }
            }
            if (smallTreeTiles != null) {
                for (int j = 0; j < smallTreeTiles.Count; j++) {
                    LocationGridTile tile = smallTreeTiles[j];
                    if (tile.tileObjectComponent.objHere is TreeObject treeObject && treeObject.structureConnector != null && treeObject.structureConnector.isOpen) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    #region Linked Beast Dens
    public void AddLinkedBeastDen(LocationStructure p_structure) {
        if (!linkedBeastDens.Contains(p_structure.persistentID)) {
            linkedBeastDens.Add(p_structure.persistentID);
        }
    }
    public LocationStructure GetRandomLinkedAliveBeastDen() {
        LocationStructure chosenStructure = null;
        List<LocationStructure> pool = RuinarchListPool<LocationStructure>.Claim();
        for (int i = 0; i < linkedBeastDens.Count; i++) {
            LocationStructure s = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(linkedBeastDens[i]);
            if (s != null) {
                if (!s.hasBeenDestroyed) {
                    LocationGridTile firstTile = s.GetFirstTileWithObject();
                    AnimalBurrow burrow = firstTile.tileObjectComponent.objHere as AnimalBurrow;
                    if (burrow != null && burrow.HasAliveSpawnedMonster()) {
                        pool.Add(s);
                    }
                } else {
                    //If already destroyed remove from list
                    linkedBeastDens.RemoveAt(i);
                    i--;
                }
            }
        }
        if (pool.Count > 0) {
            chosenStructure = pool[GameUtilities.RandomBetweenTwoNumbers(0, pool.Count - 1)];
        }
        RuinarchListPool<LocationStructure>.Release(pool);
        return chosenStructure;
    }
    public string GetLinkedBeastDensSummary() {
        string log = string.Empty;
        for (int i = 0; i < linkedBeastDens.Count; i++) {
            if (i > 0) {
                log += ",";
            }
            log += DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(linkedBeastDens[i])?.name;
        }
        return log;
    }
    #endregion

    #region Migration
    public void DetermineMigrationSpawningArea() {
        Area closestValidArea = null;
        float nearestArea = float.MaxValue;
        for (int i = 0; i < GridMap.Instance.edgeAreas.Count; i++) {
            Area edgeArea = GridMap.Instance.edgeAreas[i];
            if (edgeArea.elevationComponent.elevationType == ELEVATION.PLAIN && PathfindingManager.Instance.HasPath(edgeArea.gridTileComponent.centerGridTile, mainSpot.gridTileComponent.centerGridTile)) {
                float distance = Vector2.Distance(edgeArea.gridTileComponent.centerGridTile.centeredLocalLocation, mainSpot.gridTileComponent.centerGridTile.centeredLocalLocation);
                if (distance < nearestArea) {
                    closestValidArea = edgeArea;
                    nearestArea = distance;
                }
            }
        }
        if (closestValidArea != null) {
            migrationSpawningArea = closestValidArea;
        } else {
            migrationSpawningArea = mainSpot;
        }
    }
    #endregion
}

public class SaveDataVillageSpot : SaveData<VillageSpot> {
    public Point mainArea;
    public Point[] reservedAreas;
    public int lumberyardSpots;
    public int miningSpots;
    public List<string> linkedBeastDens;
    public Point migrationSpawningArea;

    public override void Save(VillageSpot data) {
        base.Save(data);
        mainArea = new Point(data.mainSpot.areaData.xCoordinate, data.mainSpot.areaData.yCoordinate);
        reservedAreas = new Point[data.reservedAreas.Count];
        for (int i = 0; i < data.reservedAreas.Count; i++) {
            Area area = data.reservedAreas[i];
            reservedAreas[i] = new Point(area.areaData.xCoordinate, area.areaData.yCoordinate);
        }
        linkedBeastDens = new List<string>(data.linkedBeastDens);
        lumberyardSpots = data.lumberyardSpots;
        miningSpots = data.miningSpots;
        migrationSpawningArea = new Point(data.migrationSpawningArea.areaData.xCoordinate, data.migrationSpawningArea.areaData.yCoordinate);
    }
    public override VillageSpot Load() {
        return new VillageSpot(this);
    }
}