using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;
namespace Inner_Maps {
    public class RegionInnerTileMap : InnerTileMap {
        private Dictionary<Region, Transform> otherRegionObjects { get; set; } //dictionary of objects to show which direction other regions are from this one.

        [SerializeField] private GameObject regionDirectionPrefab;
        private Bounds groundMapLocalBounds;
        
        public IEnumerator GenerateMap(MapGenerationComponent mapGenerationComponent, MapGenerationData data) {
            name = $"{region.name}'s Inner Map";
            region.SetRegionInnerMap(this);
            ClearAllTileMaps();
            Stopwatch stopwatch = new Stopwatch();
            Vector2Int regionDimensions = GetRegionDimensions(region);
            Vector2Int innerMapSize = GetInnerMapSizeGivenRegionDimensions(regionDimensions);
            yield return StartCoroutine(GenerateGrid(innerMapSize.x, innerMapSize.y, mapGenerationComponent, stopwatch));
            PopulateNeededAreaDataAfterGridGeneration(mapGenerationComponent, regionDimensions.x, regionDimensions.y, stopwatch);
            
            // int minX = allTiles.Min(t => t.localPlace.x);
            // int maxX = allTiles.Max(t => t.localPlace.x);
            // int minY = allTiles.Min(t => t.localPlace.y);
            // int maxY = allTiles.Max(t => t.localPlace.y);
            int xSize = width - 1;
            int ySize = height - 1;
            
            yield return StartCoroutine(GroundPerlin(allTiles, xSize, ySize, xSeed, ySeed, data));
            yield return StartCoroutine(GenerateElevationMap(mapGenerationComponent, data, stopwatch));
            // yield return StartCoroutine(GenerateDetails(mapGenerationComponent, xSize, ySize, stopwatch));
            StartCoroutine(GraduallyGenerateTileObjects(data));
            groundMapLocalBounds = groundTilemap.localBounds;
        }
        public IEnumerator LoadMap(MapGenerationComponent mapGenerationComponent, SaveDataInnerMap saveDataInnerMap, SaveDataCurrentProgress saveData) {
            name = $"{region.name}'s Inner Map";
            region.SetRegionInnerMap(this);
            ClearAllTileMaps();
            Stopwatch stopwatch = new Stopwatch();
            Vector2Int regionDimensions = GetRegionDimensions(region);
            Vector2Int innerMapSize = GetInnerMapSizeGivenRegionDimensions(regionDimensions);
            yield return StartCoroutine(LoadGrid(innerMapSize.x, innerMapSize.y, mapGenerationComponent, saveDataInnerMap, saveData));
            PopulateNeededAreaDataAfterGridGeneration(mapGenerationComponent, regionDimensions.x, regionDimensions.y, stopwatch);
            int minX = allTiles.Min(t => t.localPlace.x);
            int maxX = allTiles.Max(t => t.localPlace.x);
            int minY = allTiles.Min(t => t.localPlace.y);
            int maxY = allTiles.Max(t => t.localPlace.y);
            int xSize = maxX - minX;
            int ySize = maxY - minY;
            
            yield return StartCoroutine(GroundPerlin(allTiles, xSize, ySize, saveDataInnerMap.xSeed, saveDataInnerMap.ySeed, null));
            // yield return StartCoroutine(GenerateBiomeTransitions());
            groundMapLocalBounds = groundTilemap.localBounds;
        }

        #region Areas
        private void PopulateNeededAreaDataAfterGridGeneration(MapGenerationComponent mapGenerationComponent, int gridWidth, int gridHeight, Stopwatch stopwatch) {
            stopwatch.Reset();
            stopwatch.Start();

            for (int x = 0; x < gridWidth; x++) {
                for (int y = 0; y < gridHeight; y++) {
                    GameObject collectionGO = Instantiate(areaItemPrefab, structureTilemap.transform);
                    float xPos = (x + 1) * (InnerMapManager.AreaLocationGridTileSize.x) - (InnerMapManager.AreaLocationGridTileSize.x / 2f);
                    float yPos = (y + 1) * (InnerMapManager.AreaLocationGridTileSize.y) - (InnerMapManager.AreaLocationGridTileSize.y / 2f);

                    Area area = GridMap.Instance.map[x, y];
                    
                    AreaItem areaItem = collectionGO.GetComponent<AreaItem>();
                    areaItem.Initialize(this);
                    collectionGO.transform.localPosition = new Vector2(xPos, yPos);
                    area.SetAreaItem(areaItem);
                    area.gridTileComponent.SetCenterGridTile(GetCenterLocationGridTile(area));
                }
            }
            
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{base.region.name} CreateTileCollectionGrid took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        private LocationGridTile GetCenterLocationGridTile(Area p_area) {
            int xMin = p_area.gridTileComponent.gridTiles.Min(t => t.localPlace.x);
            int yMin = p_area.gridTileComponent.gridTiles.Min(t => t.localPlace.y);
            int xMax = xMin + (InnerMapManager.AreaLocationGridTileSize.x / 2);
            int yMax = yMin + (InnerMapManager.AreaLocationGridTileSize.y / 2);
            return region.innerMap.map[xMax, yMax];
        }
        #endregion

        #region Overrides
        public override void OnMapGenerationFinished() {
            base.OnMapGenerationFinished();
            GenerateRegionDirectionObjects();
        }
        #endregion

        #region Other Regions
        private void GenerateRegionDirectionObjects() {
            otherRegionObjects = new Dictionary<Region, Transform>();
            for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
                Region otherRegion = GridMap.Instance.allRegions[i];
                if (otherRegion != region) {
                    Vector3 directionToRegion = (otherRegion.coreTile.areaData.position - region.coreTile.areaData.position).normalized * (height + width);
                    GameObject regionDirectionGO = Instantiate(regionDirectionPrefab, centerGo.transform, true);
                    regionDirectionGO.name = $"{otherRegion.name} direction";
                    regionDirectionGO.transform.localPosition = directionToRegion;
                    otherRegionObjects.Add(otherRegion, regionDirectionGO.transform);
                }
            }
        }
        private Vector3 GetClosestPointToRegion(Region targetRegion) {
            Bounds bounds = groundMapLocalBounds;
            bounds.center = bounds.center + groundTilemap.transform.position;
            Vector3 closestPoint = bounds.ClosestPoint(otherRegionObjects[targetRegion].position);
            return transform.InverseTransformPoint(closestPoint);
        }
        public LocationGridTile GetTileToGoToRegion([NotNull]Region targetRegion) {
            Assert.IsTrue(targetRegion != region, $"target region passed is same as owning region! {targetRegion.name}");
            Vector3 coordinates = GetClosestPointToRegion(targetRegion);
#if DEBUG_LOG
            Debug.Log($"Getting target tile to go to {targetRegion.name} from {region.name}. Result was {coordinates.ToString()}");
#endif
            int xCoordinate = Mathf.Clamp((int)coordinates.x, 0, width - 1);
            int yCoordinate = Mathf.Clamp((int)coordinates.y, 0, height - 1);
            LocationGridTile targetTile = map[xCoordinate, yCoordinate];
            return targetTile;
        }
#endregion

#region Utilities
        private Vector2Int GetRegionDimensions(Region p_region) {
            int maxX = p_region.areas.Max(t => t.areaData.xCoordinate);
            int minX = p_region.areas.Min(t => t.areaData.xCoordinate);
            
            int gridWidth = maxX - minX;
            
            int maxY = p_region.areas.Max(t => t.areaData.yCoordinate);
            int minY = p_region.areas.Min(t => t.areaData.yCoordinate);
            int gridHeight = maxY - minY;
            
            return new Vector2Int(gridWidth + 1, gridHeight + 1);
        }
        private Vector2Int GetInnerMapSizeGivenRegionDimensions(Vector2Int p_dimensions) {
            return new Vector2Int(p_dimensions.x * InnerMapManager.AreaLocationGridTileSize.x, p_dimensions.y * InnerMapManager.AreaLocationGridTileSize.y);
        }
#endregion
    }
}