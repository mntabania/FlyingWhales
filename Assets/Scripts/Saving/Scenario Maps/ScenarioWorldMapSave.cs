using System.Collections.Generic;
namespace Scenario_Maps {
    [System.Serializable]
    public class ScenarioWorldMapSave {
        public WorldMapTemplate worldMapTemplate;
        public List<SaveDataArea> areaSaves;
        
        public void SaveWorld(WorldMapTemplate p_worldMapTemplate, List<Area> p_areas) {
            worldMapTemplate = p_worldMapTemplate;
            SaveAreas(p_areas);
        }

        #region Hex Tiles
        public void SaveAreas(List<Area> tiles) {
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
        //public List<SaveDataArea> GetAllTilesWithLandmarks() {
        //    List<SaveDataArea> tiles = new List<SaveDataArea>();
        //    for (int i = 0; i < areaSaves.Count; i++) {
        //        SaveDataArea saveDataArea = areaSaves[i];
        //        if (saveDataArea.landmarkType != LANDMARK_TYPE.NONE) {
        //            tiles.Add(saveDataArea);
        //        }
        //    }
        //    return tiles;
        //}
        #endregion
    
    }
}