using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public partial class LandmarkManager {
    
    [Header("Inner Structures")]
    [FormerlySerializedAs("innerStructurePrefab")] [SerializeField] private GameObject areaInnerStructurePrefab;
    [FormerlySerializedAs("areaMapsParent")] [SerializeField] private Transform innerMapsParent;
    [SerializeField] private GameObject regionInnerStructurePrefab;

    #region Region Maps
    public IEnumerator GenerateRegionMap(Region region, MapGenerationComponent mapGenerationComponent) {
        GameObject regionMapGo = Instantiate(regionInnerStructurePrefab, innerMapsParent);
        RegionInnerTileMap innerTileMap = regionMapGo.GetComponent<RegionInnerTileMap>();
        innerTileMap.Initialize(region);
        region.GenerateStructures();
        yield return StartCoroutine(innerTileMap.GenerateMap(mapGenerationComponent));
        InnerMapManager.Instance.OnCreateInnerMap(innerTileMap);
    }
    public IEnumerator LoadRegionMap(Region region, MapGenerationComponent mapGenerationComponent, SaveDataInnerMap saveDataInnerMap, Dictionary<string, TileBase> tileAssetDB) {
        LevelLoaderManager.Instance.UpdateLoadingInfo($"Loading {region.name} map...");
        GameObject regionMapGo = Instantiate(regionInnerStructurePrefab, innerMapsParent);
        RegionInnerTileMap innerTileMap = regionMapGo.GetComponent<RegionInnerTileMap>();
        innerTileMap.Initialize(region);
        yield return StartCoroutine(innerTileMap.LoadMap(mapGenerationComponent, saveDataInnerMap, tileAssetDB));
        InnerMapManager.Instance.OnCreateInnerMap(innerTileMap);
    }
    #endregion
}