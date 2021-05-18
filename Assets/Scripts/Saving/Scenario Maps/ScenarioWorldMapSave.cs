﻿using System.Collections.Generic;
using Perlin_Noise;
using UnityEngine;
namespace Scenario_Maps {
    [System.Serializable]
    public class ScenarioWorldMapSave {
        public WorldMapTemplate worldMapTemplate;
        public List<SaveDataArea> areaSaves;
        public PerlinNoiseSettings elevationPerlinNoiseSettings;
        public int xSeed;
        public int ySeed;
        public float warpWeight;
        public float temperatureSeed;
        public List<SpecialStructureSetting> specialStructureSaves;
        
        public void SaveWorld(WorldMapTemplate p_worldMapTemplate, List<Area> p_areas, PerlinNoiseSettings p_elevationSettings, float p_warpWeight, float p_temperatureSeed) {
            worldMapTemplate = p_worldMapTemplate;
            elevationPerlinNoiseSettings = p_elevationSettings;
            warpWeight = p_warpWeight;
            temperatureSeed = p_temperatureSeed;
            SaveAreas(p_areas);
        }

        #region Hex Tiles
        public void SaveAreas(List<Area> tiles) {
            areaSaves = new List<SaveDataArea>();
            specialStructureSaves = new List<SpecialStructureSetting>();
            for (int i = 0; i < tiles.Count; i++) {
                Area currTile = tiles[i];
                SaveDataArea newSaveData = new SaveDataArea();
                newSaveData.Save(currTile);
                if (currTile.primaryStructureInArea != null && currTile.primaryStructureInArea.structureType.IsSpecialStructure() && currTile.primaryStructureInArea.structureType != STRUCTURE_TYPE.CAVE) {
                    SpecialStructureSetting specialStructureSetting = new SpecialStructureSetting(new Point(currTile.areaData.xCoordinate, currTile.areaData.yCoordinate), currTile.primaryStructureInArea.structureType);
                    specialStructureSaves.Add(specialStructureSetting);
                }
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
        // public List<SaveDataArea> GetAllTilesWithLandmarks() {
        //     List<SaveDataArea> tiles = new List<SaveDataArea>();
        //     for (int i = 0; i < areaSaves.Count; i++) {
        //         SaveDataArea saveDataArea = areaSaves[i];
        //         if (saveDataArea.landmarkType != LANDMARK_TYPE.NONE) {
        //             tiles.Add(saveDataArea);
        //         }
        //     }
        //     return tiles;
        // }
        #endregion
    
    }
}