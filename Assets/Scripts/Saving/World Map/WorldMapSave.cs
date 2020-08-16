using System.Collections.Generic;
using Locations.Settlements;

[System.Serializable]
public class WorldMapSave {
    public WorldMapTemplate worldMapTemplate;
    public List<SaveDataHextile> hextileSaves;
    public List<SaveDataRegion> regionSaves;
    
    public void SaveWorld(WorldMapTemplate _worldMapTemplate, List<HexTile> normalHexTiles, Region[] regions, bool saveInnerMaps = true) {
        worldMapTemplate = _worldMapTemplate;
        SaveHexTiles(normalHexTiles);
        SaveRegions(regions, saveInnerMaps);
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
    private void SaveRegions(Region[] regions, bool saveInnerMaps) {
        regionSaves = new List<SaveDataRegion>();
        for (int i = 0; i < regions.Length; i++) {
            Region region = regions[i];
            SaveDataRegion saveDataRegion = new SaveDataRegion();
            saveDataRegion.Save(region, saveInnerMaps);
            regionSaves.Add(saveDataRegion);
        }
    }
    #endregion
}
