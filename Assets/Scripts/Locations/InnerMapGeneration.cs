using System.Collections;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Serialization;

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
    #endregion

    public void MakeAllRegionsAwareOfEachOther() {
        for (var i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            var currRegion = GridMap.Instance.allRegions[i];
            for (var j = 0; j < GridMap.Instance.allRegions.Length; j++) {
                var otherRegion = GridMap.Instance.allRegions[j];
                if (currRegion != otherRegion && otherRegion.regionTileObject != null) {
                    currRegion.AddAwareness(otherRegion.regionTileObject);
                }
            }
        }
    }
}