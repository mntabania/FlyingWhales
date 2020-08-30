using System.Collections.Generic;
namespace Scenario_Maps {
    [System.Serializable]
    public class ScenarioWorldMapSave {
        public WorldMapTemplate worldMapTemplate;
        public List<SaveDataHextile> hextileSaves;
        
        public void SaveWorld(WorldMapTemplate _worldMapTemplate, List<HexTile> normalHexTiles) {
            worldMapTemplate = _worldMapTemplate;
            SaveHexTiles(normalHexTiles);
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
    
    }
}