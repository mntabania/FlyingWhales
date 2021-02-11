using System.Collections.Generic;
using UnityEngine;
namespace Scenario_Maps {
    [System.Serializable]
    public class ScenarioWorldMapSave {
        public WorldMapTemplate worldMapTemplate;
        public List<SaveDataArea> hextileSaves;
        public List<SpecialStructureSetting> specialStructureSaves;
        
        public void SaveWorld(WorldMapTemplate p_worldMapTemplate, List<Area> p_areas) {
            worldMapTemplate = p_worldMapTemplate;
            SaveAreas(p_areas);
        }

        #region Hex Tiles
        public void SaveAreas(List<Area> tiles) {
            hextileSaves = new List<SaveDataArea>();
            specialStructureSaves = new List<SpecialStructureSetting>();
            for (int i = 0; i < tiles.Count; i++) {
                Area currTile = tiles[i];
                SaveDataArea newSaveData = new SaveDataArea();
                newSaveData.Save(currTile);
                hextileSaves.Add(newSaveData);
                if (currTile.primaryStructureInArea != null && currTile.primaryStructureInArea.structureType.IsSpecialStructure()) {
                    SpecialStructureSetting specialStructureSetting = new SpecialStructureSetting(new Vector2Int(currTile.areaData.xCoordinate, currTile.areaData.yCoordinate), currTile.primaryStructureInArea.structureType);
                    specialStructureSaves.Add(specialStructureSetting);
                }
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
        #endregion
    
    }
}