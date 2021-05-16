﻿using System.Collections;
using Inner_Maps;
using Scenario_Maps;
using UnityEngine;
using UnityEngine.Serialization;

public partial class LandmarkManager {
    [FormerlySerializedAs("areaMapsParent")] [SerializeField] private Transform innerMapsParent;
    [SerializeField] private GameObject regionInnerStructurePrefab;

    #region Region Maps
    public IEnumerator GenerateRegionMap(Region region, MapGenerationComponent mapGenerationComponent, MapGenerationData data) {
        GameObject regionMapGo = Instantiate(regionInnerStructurePrefab, innerMapsParent);
        RegionInnerTileMap innerTileMap = regionMapGo.GetComponent<RegionInnerTileMap>();

        float xSeed = 1f;//Random.Range(0f, 99999f);
        float ySeed = 1f;//Random.Range(0f, 99999f);
        int biomeSeed = 1;//Random.Range(0, 99999);
        int elevationSeed = 1;//Random.Range(0, 99999);
        innerTileMap.Initialize(region, xSeed, ySeed, biomeSeed, elevationSeed);
        region.GenerateStructures();
        yield return StartCoroutine(innerTileMap.GenerateMap(mapGenerationComponent, data));
        InnerMapManager.Instance.OnCreateInnerMap(innerTileMap);
    }
    public IEnumerator GenerateScenarioMap(Region region, MapGenerationComponent mapGenerationComponent, MapGenerationData data, ScenarioMapData scenarioMapData) {
        GameObject regionMapGo = Instantiate(regionInnerStructurePrefab, innerMapsParent);
        RegionInnerTileMap innerTileMap = regionMapGo.GetComponent<RegionInnerTileMap>();
        innerTileMap.Initialize(region, scenarioMapData.worldMapSave.xSeed, scenarioMapData.worldMapSave.ySeed, scenarioMapData.worldMapSave.elevationPerlinNoiseSettings);
        region.GenerateStructures();
        yield return StartCoroutine(innerTileMap.GenerateMap(mapGenerationComponent, data));
        InnerMapManager.Instance.OnCreateInnerMap(innerTileMap);
    }
    public IEnumerator LoadRegionMap(Region region, MapGenerationComponent mapGenerationComponent, SaveDataInnerMap saveDataInnerMap, SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo($"Loading {region.name} Map...");
        GameObject regionMapGo = Instantiate(regionInnerStructurePrefab, innerMapsParent);
        RegionInnerTileMap innerTileMap = regionMapGo.GetComponent<RegionInnerTileMap>();
        float xSeed = saveDataInnerMap.xSeed;
        float ySeed = saveDataInnerMap.ySeed;
        innerTileMap.Initialize(region, xSeed, ySeed, saveDataInnerMap.elevationPerlinNoiseSettings);
        yield return StartCoroutine(innerTileMap.LoadMap(mapGenerationComponent, saveDataInnerMap, saveData));
        InnerMapManager.Instance.OnCreateInnerMap(innerTileMap);
    }
    #endregion
}