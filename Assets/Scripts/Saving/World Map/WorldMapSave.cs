using System.Collections.Generic;
using Databases;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;

[System.Serializable]
public class WorldMapSave {
    public WorldMapTemplate worldMapTemplate;
    public List<SaveDataHextile> hextileSaves;
    public List<SaveDataRegion> regionSaves;
    public List<SaveDataBaseSettlement> settlementSaves;
    public List<SaveDataLocationStructure> structureSaves;
    public List<SaveDataTileObject> tileObjectSaves;
    
    public void SaveWorld(WorldMapTemplate _worldMapTemplate, HexTileDatabase hexTileDatabase, RegionDatabase regionDatabase, SettlementDatabase settlementDatabase,
        LocationStructureDatabase structureDatabase, TileObjectDatabase tileObjectDatabase) {
        worldMapTemplate = _worldMapTemplate;
        SaveHexTiles(hexTileDatabase.allHexTiles);
        SaveRegions(regionDatabase.allRegions);
        SaveSettlements(settlementDatabase.allSettlements);
        SaveStructures(structureDatabase.allStructures);
        SaveTileObjects(tileObjectDatabase.allTileObjectsList);
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

    #region Tile Objects
    public void SaveTileObjects(List<TileObject> tileObjects) {
        //tile objects
        List<TileObject> finishedObjects = new List<TileObject>();
        tileObjectSaves = new List<SaveDataTileObject>();
        for (int i = 0; i < tileObjects.Count; i++) {
            TileObject tileObject = tileObjects[i];
            if (tileObject.gridTileLocation == null && tileObject.isBeingCarriedBy == null) {
                // Debug.LogWarning($"Grid tile location of {tileObject} is null! Not saving that...");
                continue; //skip tile objects without grid tile location that are not being carried.
            }
            if (finishedObjects.Contains(tileObject)) {
                // Debug.LogWarning($"{tileObject} has a duplicate value in tile object list!");
                continue; //skip    
            }
            if (tileObject is GenericTileObject) {
                continue; //do not place save data of generic tile objects here, since they are loaded alongside their respective LocationGridTiles  
            }
            if (tileObject is Artifact artifact) {
                string tileObjectTypeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(artifact.type.ToString());
                SaveDataTileObject saveDataTileObject = createNewSaveDataForArtifact(tileObjectTypeName);
                saveDataTileObject.Save(tileObject);
                tileObjectSaves.Add(saveDataTileObject);    
            } else {
                string tileObjectTypeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(tileObject.tileObjectType.ToString());
                SaveDataTileObject saveDataTileObject = CreateNewSaveDataForTileObject(tileObjectTypeName);
                saveDataTileObject.Save(tileObject);
                tileObjectSaves.Add(saveDataTileObject);    
            }
            finishedObjects.Add(tileObject);
        }
        finishedObjects.Clear();
        finishedObjects = null;
    }
    public static SaveDataTileObject CreateNewSaveDataForTileObject(string tileObjectTypeString) {
        var typeName = $"SaveData{tileObjectTypeString}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            SaveDataTileObject obj = System.Activator.CreateInstance(type) as SaveDataTileObject;
            return obj;
        }
        return new SaveDataTileObject(); //if no special save data for tile object was found, then just use the generic one
    }
    private SaveDataTileObject createNewSaveDataForArtifact(string tileObjectTypeString) {
        var typeName = $"SaveData{tileObjectTypeString}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
            SaveDataTileObject obj = System.Activator.CreateInstance(type) as SaveDataTileObject;
            return obj;
        }
        return new SaveDataArtifact(); //if no special save data for tile object was found, then just use the generic one
    }
    #endregion
}
