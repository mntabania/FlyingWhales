using System.Collections.Generic;
namespace Scenario_Maps {
    [System.Serializable]
    public class ScenarioWorldMapSave {
        public WorldMapTemplate worldMapTemplate;
        public List<SaveDataArea> hextileSaves;
        
        public void SaveWorld(WorldMapTemplate p_worldMapTemplate, List<Area> p_areas) {
            worldMapTemplate = p_worldMapTemplate;
            SaveAreas(p_areas);
        }

        #region Hex Tiles
        public void SaveAreas(List<Area> tiles) {
            hextileSaves = new List<SaveDataArea>();
            for (int i = 0; i < tiles.Count; i++) {
                Area currTile = tiles[i];
                SaveDataArea newSaveData = new SaveDataArea();
                newSaveData.Save(currTile);
                hextileSaves.Add(newSaveData);
            }
        }
        public SaveDataArea[,] GetSaveDataMap() {
            SaveDataArea[,] map = new SaveDataArea[worldMapTemplate.worldMapWidth, worldMapTemplate.worldMapHeight];
            for (int i = 0; i < hextileSaves.Count; i++) {
                SaveDataArea currTile = hextileSaves[i];
                map[currTile.areaData.xCoordinate, currTile.areaData.yCoordinate] = currTile;
            }
            return map;
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