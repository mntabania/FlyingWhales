using System.Collections;
using System.Collections.Generic;
using System;
using Databases;
using Events.World_Events;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;

[System.Serializable]
public class WorldMapSave {
    public WorldSettingsData.World_Type worldType;
    public WorldMapTemplate worldMapTemplate;
    public SaveDataRegion regionSave;
    public List<SaveDataArea> areaSaves;
    public List<SaveDataBaseSettlement> settlementSaves;
    public List<SaveDataLocationStructure> structureSaves;
    public List<SaveDataWorldEvent> worldEventSaves;
    
    // public void SaveWorld(WorldMapTemplate _worldMapTemplate, HexTileDatabase hexTileDatabase, RegionDatabase regionDatabase, SettlementDatabase settlementDatabase, LocationStructureDatabase structureDatabase) {
    //     //if saved world is tutorial, set world type as custom, this is so that tutorials will not spawn again when loading from a map from the tutorial
    //     worldType = WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial ? WorldSettingsData.World_Type.Custom : WorldSettings.Instance.worldSettingsData.worldType;
    //     worldMapTemplate = _worldMapTemplate;
    //     SaveHexTiles(hexTileDatabase.allHexTiles);
    //     SaveRegions(regionDatabase.allRegions);
    //     SaveSettlements(settlementDatabase.allSettlements);
    //     SaveStructures(structureDatabase.allStructures);
    // }
    public IEnumerator SaveWorldCoroutine(WorldMapTemplate _worldMapTemplate, AreaDatabase hexTileDatabase, RegionDatabase regionDatabase, SettlementDatabase settlementDatabase, 
        LocationStructureDatabase structureDatabase, List<WorldEvent> activeEvents) {
        UIManager.Instance.optionsMenu.UpdateSaveMessage("Saving world map...");
        //if saved world is tutorial, set world type as custom, this is so that tutorials will not spawn again when loading from a map from the tutorial
        worldType = WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial ? WorldSettingsData.World_Type.Custom : WorldSettings.Instance.worldSettingsData.worldType;
        worldMapTemplate = _worldMapTemplate;
        yield return SaveManager.Instance.StartCoroutine(SaveHexTilesCoroutine(hexTileDatabase.allAreas));
        yield return SaveManager.Instance.StartCoroutine(SaveRegionsCoroutine(regionDatabase.mainRegion));
        yield return SaveManager.Instance.StartCoroutine(SaveSettlementsCoroutine(settlementDatabase.allSettlements));
        yield return SaveManager.Instance.StartCoroutine(SaveStructuresCoroutine(structureDatabase.allStructures));
        yield return SaveManager.Instance.StartCoroutine(SaveWorldEventsCoroutine(activeEvents));
    }
    public void SaveWorld(WorldMapTemplate _worldMapTemplate, AreaDatabase hexTileDatabase, RegionDatabase regionDatabase, SettlementDatabase settlementDatabase,
    LocationStructureDatabase structureDatabase, List<WorldEvent> activeEvents) {
        //if saved world is tutorial, set world type as custom, this is so that tutorials will not spawn again when loading from a map from the tutorial
        worldType = WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial ? WorldSettingsData.World_Type.Custom : WorldSettings.Instance.worldSettingsData.worldType;
        worldMapTemplate = _worldMapTemplate;
        SaveHexTiles(hexTileDatabase.allAreas);
        SaveRegions(regionDatabase.mainRegion);
        SaveSettlements(settlementDatabase.allSettlements);
        SaveStructures(structureDatabase.allStructures);
        SaveWorldEvents(activeEvents);
        //yield return SaveManager.Instance.StartCoroutine(SaveHexTilesCoroutine(hexTileDatabase.allAreas));
        //yield return SaveManager.Instance.StartCoroutine(SaveRegionsCoroutine(regionDatabase.allRegions));
        //yield return SaveManager.Instance.StartCoroutine(SaveSettlementsCoroutine(settlementDatabase.allSettlements));
        //yield return SaveManager.Instance.StartCoroutine(SaveStructuresCoroutine(structureDatabase.allStructures));
        //yield return SaveManager.Instance.StartCoroutine(SaveWorldEventsCoroutine(activeEvents));
    }

    #region Hex Tiles
    public IEnumerator SaveHexTilesCoroutine(List<Area> tiles) {
        int batchCount = 0;
        areaSaves = new List<SaveDataArea>();
        for (int i = 0; i < tiles.Count; i++) {
            Area currTile = tiles[i];
            SaveDataArea newSaveData = new SaveDataArea();
            newSaveData.Save(currTile);
            areaSaves.Add(newSaveData);
            batchCount++;
            if (batchCount >= SaveManager.HexTile_Save_Batches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    public void SaveHexTiles(List<Area> tiles) {
        areaSaves = new List<SaveDataArea>();
        for (int i = 0; i < tiles.Count; i++) {
            Area currTile = tiles[i];
            SaveDataArea newSaveData = new SaveDataArea();
            newSaveData.Save(currTile);
            areaSaves.Add(newSaveData);
        }
    }
    public SaveDataArea[,] GetSaveDataMap() {
        SaveDataArea[,] map = new SaveDataArea[worldMapTemplate.worldMapWidth, worldMapTemplate.worldMapHeight];
        for (int i = 0; i < areaSaves.Count; i++) {
            SaveDataArea currTile = areaSaves[i];
            map[currTile.areaData.xCoordinate, currTile.areaData.yCoordinate] = currTile;
        }
        return map;
    }
    // public SaveDataHextile GetHexTileDataWithLandmark(LANDMARK_TYPE landmarkType) {
    //     for (int i = 0; i < areaSaves.Count; i++) {
    //         SaveDataHextile saveDataHextile = areaSaves[i];
    //         if (saveDataHextile.landmarkType == landmarkType) {
    //             return saveDataHextile;
    //         }
    //     }
    //     return null;
    // }
    // public List<SaveDataHextile> GetAllTilesWithLandmarks() {
    //     List<SaveDataHextile> tiles = new List<SaveDataHextile>();
    //     for (int i = 0; i < areaSaves.Count; i++) {
    //         SaveDataHextile saveDataHextile = areaSaves[i];
    //         if (saveDataHextile.landmarkType != LANDMARK_TYPE.NONE) {
    //             tiles.Add(saveDataHextile);
    //         }
    //     }
    //     return tiles;
    // }
    #endregion

    #region Regions
    private void SaveRegions(Region region) {
        SaveDataRegion saveDataRegion = new SaveDataRegion();
        saveDataRegion.Save(region);
        regionSave = saveDataRegion;
        //regionSaves = new List<SaveDataRegion>();
        //for (int i = 0; i < regions.Length; i++) {
        //    Region region = regions[i];
        //    SaveDataRegion saveDataRegion = new SaveDataRegion();
        //    saveDataRegion.Save(region);
        //    regionSaves.Add(saveDataRegion);
        //}
    }
    private IEnumerator SaveRegionsCoroutine(Region region) {
        SaveDataRegion saveDataRegion = new SaveDataRegion();
        saveDataRegion.Save(region);
        regionSave = saveDataRegion;
        yield return null;
        //int batchCount = 0;
        //regionSaves = new List<SaveDataRegion>();
        //for (int i = 0; i < regions.Length; i++) {
        //    Region region = regions[i];
        //    SaveDataRegion saveDataRegion = new SaveDataRegion();
        //    saveDataRegion.Save(region);
        //    regionSaves.Add(saveDataRegion);
        //    batchCount++;
        //    if (batchCount >= SaveManager.Region_Save_Batches) {
        //        batchCount = 0;
        //        yield return null;    
        //    }
        //}
    }
    #endregion

    #region Settlements
    public void SaveSettlements(List<BaseSettlement> allSettlements) {
        settlementSaves = new List<SaveDataBaseSettlement>();
        for (int i = 0; i < allSettlements.Count; i++) {
            BaseSettlement settlement = allSettlements[i];
            SaveDataBaseSettlement saveDataBaseSettlement = CreateNewSettlementSaveData(settlement);
            saveDataBaseSettlement.Save(settlement);
            settlementSaves.Add(saveDataBaseSettlement);
        }
    }
    public IEnumerator SaveSettlementsCoroutine(List<BaseSettlement> allSettlements) {
        int batchCount = 0;
        settlementSaves = new List<SaveDataBaseSettlement>();
        for (int i = 0; i < allSettlements.Count; i++) {
            BaseSettlement settlement = allSettlements[i];
            SaveDataBaseSettlement saveDataBaseSettlement = CreateNewSettlementSaveData(settlement);
            saveDataBaseSettlement.Save(settlement);
            settlementSaves.Add(saveDataBaseSettlement);
            batchCount++;
            if (batchCount >= SaveManager.Settlement_Save_Batches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    private SaveDataBaseSettlement CreateNewSettlementSaveData(BaseSettlement settlement) {
        if (settlement is PlayerSettlement) {
            return new SaveDataPlayerSettlement();
        } else {
            return new SaveDataNPCSettlement();    
        }
    }
    #endregion

    #region Structures
    private SaveDataLocationStructure CreateNewSaveDataFor(LocationStructure structure) {
        return Activator.CreateInstance(structure.serializedData) as SaveDataLocationStructure;
        //if (structure is DemonicStructure) {
        //    return new SaveDataDemonicStructure();
        //} else if (structure is NaturalStructure) {
        //    if (structure is Cave) {
        //        return new SaveDataCave();
        //    }
        //    return new SaveDataNaturalStructure();
        //} else {
        //    return new SaveDataManMadeStructure();
        //}
    }
    private void SaveStructures(List<LocationStructure> structures) {
        //structures
        structureSaves = new List<SaveDataLocationStructure>();
        for (int i = 0; i < structures.Count; i++) {
            LocationStructure structure = structures[i];
            SaveDataLocationStructure saveDataLocationStructure = CreateNewSaveDataFor(structure);
            saveDataLocationStructure.Save(structure);
            structureSaves.Add(saveDataLocationStructure);
        }
    }
    private IEnumerator SaveStructuresCoroutine(List<LocationStructure> structures) {
        int batchCount = 0;
        //structures
        structureSaves = new List<SaveDataLocationStructure>();
        for (int i = 0; i < structures.Count; i++) {
            LocationStructure structure = structures[i];
            SaveDataLocationStructure saveDataLocationStructure = CreateNewSaveDataFor(structure);
            saveDataLocationStructure.Save(structure);
            structureSaves.Add(saveDataLocationStructure);
            batchCount++;
            if (batchCount >= SaveManager.Structure_Save_Batches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    #endregion

    #region World Events
    private IEnumerator SaveWorldEventsCoroutine(List<WorldEvent> worldEvents) {
        worldEventSaves = new List<SaveDataWorldEvent>();
        for (int i = 0; i < worldEvents.Count; i++) {
            WorldEvent worldEvent = worldEvents[i];
            worldEventSaves.Add(worldEvent.Save());
        }
        yield return null;
    }
    private void SaveWorldEvents(List<WorldEvent> worldEvents) {
        worldEventSaves = new List<SaveDataWorldEvent>();
        for (int i = 0; i < worldEvents.Count; i++) {
            WorldEvent worldEvent = worldEvents[i];
            worldEventSaves.Add(worldEvent.Save());
        }
    }
    #endregion
}
