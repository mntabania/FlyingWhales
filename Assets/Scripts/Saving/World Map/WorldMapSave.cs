using System.Collections.Generic;
using Databases;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;

[System.Serializable]
public class WorldMapSave {
    public WorldSettingsData.World_Type worldType;
    public WorldMapTemplate worldMapTemplate;
    public List<SaveDataHextile> hextileSaves;
    public List<SaveDataRegion> regionSaves;
    public List<SaveDataBaseSettlement> settlementSaves;
    public List<SaveDataLocationStructure> structureSaves;
    
    public void SaveWorld(WorldMapTemplate _worldMapTemplate, HexTileDatabase hexTileDatabase, RegionDatabase regionDatabase, SettlementDatabase settlementDatabase, LocationStructureDatabase structureDatabase) {
        //if saved world is tutorial, set world type as custom, this is so that tutorials will not spawn again when loading from a map from the tutorial
        worldType = WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial ? WorldSettingsData.World_Type.Custom : WorldSettings.Instance.worldSettingsData.worldType;
        worldMapTemplate = _worldMapTemplate;
        SaveHexTiles(hexTileDatabase.allHexTiles);
        SaveRegions(regionDatabase.allRegions);
        SaveSettlements(settlementDatabase.allSettlements);
        SaveStructures(structureDatabase.allStructures);
    }

    #region Hex Tiles
    public void SaveHexTiles(List<HexTile> tiles) {
        hextileSaves = new List<SaveDataHextile>();
        for (int i = 0; i < tiles.Count; i++) {
            HexTile currTile = tiles[i];
            SaveDataHextile newSaveData = new SaveDataHextile();
            newSaveData.Save(currTile);
            hextileSaves.Add(newSaveData);
        }
    }
    public SaveDataHextile[,] GetSaveDataMap() {
        SaveDataHextile[,] map = new SaveDataHextile[worldMapTemplate.worldMapWidth, worldMapTemplate.worldMapHeight];
        for (int i = 0; i < hextileSaves.Count; i++) {
            SaveDataHextile currTile = hextileSaves[i];
            map[currTile.xCoordinate, currTile.yCoordinate] = currTile;
        }
        return map;
    }
    public SaveDataHextile GetHexTileDataWithLandmark(LANDMARK_TYPE landmarkType) {
        for (int i = 0; i < hextileSaves.Count; i++) {
            SaveDataHextile saveDataHextile = hextileSaves[i];
            if (saveDataHextile.landmarkType == landmarkType) {
                return saveDataHextile;
            }
        }
        return null;
    }
    public List<SaveDataHextile> GetAllTilesWithLandmarks() {
        List<SaveDataHextile> tiles = new List<SaveDataHextile>();
        for (int i = 0; i < hextileSaves.Count; i++) {
            SaveDataHextile saveDataHextile = hextileSaves[i];
            if (saveDataHextile.landmarkType != LANDMARK_TYPE.NONE) {
                tiles.Add(saveDataHextile);
            }
        }
        return tiles;
    }
    #endregion

    #region Regions
    private void SaveRegions(Region[] regions) {
        regionSaves = new List<SaveDataRegion>();
        for (int i = 0; i < regions.Length; i++) {
            Region region = regions[i];
            SaveDataRegion saveDataRegion = new SaveDataRegion();
            saveDataRegion.Save(region);
            regionSaves.Add(saveDataRegion);
        }
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
        if (structure is DemonicStructure) {
            return new SaveDataDemonicStructure();
        } else if (structure is NaturalStructure) {
            if (structure is Cave) {
                return new SaveDataCave();
            }
            return new SaveDataNaturalStructure();
        } else {
            return new SaveDataManMadeStructure();
        }
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
    #endregion
}
