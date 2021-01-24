using System.Collections;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Serialization;

public partial class LandmarkManager {
    [FormerlySerializedAs("areaMapsParent")] [SerializeField] private Transform innerMapsParent;
    [SerializeField] private GameObject regionInnerStructurePrefab;

    #region Region Maps
    public IEnumerator GenerateRegionMap(Region region, MapGenerationComponent mapGenerationComponent) {
        GameObject regionMapGo = Instantiate(regionInnerStructurePrefab, innerMapsParent);
        RegionInnerTileMap innerTileMap = regionMapGo.GetComponent<RegionInnerTileMap>();
        innerTileMap.Initialize(region, Random.Range(0f, 99999f), Random.Range(0f, 99999f), Random.Range(0f, 99999f), Random.Range(0f, 99999f));
        region.GenerateStructures();
        yield return StartCoroutine(innerTileMap.GenerateMap(mapGenerationComponent));
        InnerMapManager.Instance.OnCreateInnerMap(innerTileMap);
    }
    public IEnumerator LoadRegionMap(Region region, MapGenerationComponent mapGenerationComponent, SaveDataInnerMap saveDataInnerMap, SaveDataCurrentProgress saveData) {
        LevelLoaderManager.Instance.UpdateLoadingInfo($"Loading {region.name} map...");
        GameObject regionMapGo = Instantiate(regionInnerStructurePrefab, innerMapsParent);
        RegionInnerTileMap innerTileMap = regionMapGo.GetComponent<RegionInnerTileMap>();
        float xSeed = saveDataInnerMap.xSeed;
        float ySeed = saveDataInnerMap.ySeed;
        innerTileMap.Initialize(region, xSeed, ySeed, saveDataInnerMap.biomeTransitionXSeed, saveDataInnerMap.biomeTransitionYSeed);
        yield return StartCoroutine(innerTileMap.LoadMap(mapGenerationComponent, saveDataInnerMap, saveData));
        InnerMapManager.Instance.OnCreateInnerMap(innerTileMap);
    }
    #endregion
}