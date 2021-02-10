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
        
        public IEnumerator GenerateMap(MapGenerationComponent mapGenerationComponent) {
            name = $"{region.name}'s Inner Map";
            region.SetRegionInnerMap(this);
            ClearAllTileMaps();
            Vector2Int regionDimensions = GetRegionDimensions(region);
            Vector2Int innerMapSize = GetInnerMapSizeGivenRegionDimensions(regionDimensions);
            yield return StartCoroutine(GenerateGrid(innerMapSize.x, innerMapSize.y, mapGenerationComponent));
            CreateAreaItemGrid(mapGenerationComponent, regionDimensions.x, regionDimensions.y);
            yield return StartCoroutine(GenerateDetails(mapGenerationComponent));
            groundMapLocalBounds = groundTilemap.localBounds;
        }
        public IEnumerator LoadMap(MapGenerationComponent mapGenerationComponent, SaveDataInnerMap saveDataInnerMap, SaveDataCurrentProgress saveData) {
            name = $"{region.name}'s Inner Map";
            region.SetRegionInnerMap(this);
            ClearAllTileMaps();
            Vector2Int regionDimensions = GetRegionDimensions(region);
            Vector2Int innerMapSize = GetInnerMapSizeGivenRegionDimensions(regionDimensions);
            yield return StartCoroutine(LoadGrid(innerMapSize.x, innerMapSize.y, mapGenerationComponent, saveDataInnerMap, saveData));
            CreateAreaItemGrid(mapGenerationComponent, regionDimensions.x, regionDimensions.y);
            int minX = allTiles.Min(t => t.localPlace.x);
            int maxX = allTiles.Max(t => t.localPlace.x);
            int minY = allTiles.Min(t => t.localPlace.y);
            int maxY = allTiles.Max(t => t.localPlace.y);
            int xSize = maxX - minX;
            int ySize = maxY - minY;
            
            yield return StartCoroutine(GroundPerlin(allTiles, xSize, ySize, saveDataInnerMap.xSeed, saveDataInnerMap.ySeed));
            // yield return StartCoroutine(GenerateBiomeTransitions());
            groundMapLocalBounds = groundTilemap.localBounds;
        }

        #region Build Spots
        private void CreateAreaItemGrid(MapGenerationComponent mapGenerationComponent, int gridWidth, int gridHeight) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int x = 0; x < gridWidth; x++) {
                for (int y = 0; y < gridHeight; y++) {
                    GameObject collectionGO = Instantiate(areaItemPrefab, structureTilemap.transform);
                    float xPos = (x + 1) * (InnerMapManager.AreaLocationGridTileSize.x) - (InnerMapManager.AreaLocationGridTileSize.x / 2f);
                    float yPos = (y + 1) * (InnerMapManager.AreaLocationGridTileSize.y) - (InnerMapManager.AreaLocationGridTileSize.y / 2f);

                    HexTile tile = GridMap.Instance.map[x, y];
                    
                    AreaItem areaItem = collectionGO.GetComponent<AreaItem>();
                    areaItem.Initialize(this);
                    collectionGO.transform.localPosition = new Vector2(xPos, yPos);
                    
                    tile.SetAreaItem(areaItem);
                }
            }
            
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{base.region.name} CreateTileCollectionGrid took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        #endregion

        #region Overrides
        public override void OnMapGenerationFinished() {
            base.OnMapGenerationFinished();
            GenerateRegionDirectionObjects();
        }
        #endregion

        #region Other Regions
        public void GenerateRegionDirectionObjects() {
            otherRegionObjects = new Dictionary<Region, Transform>();
            for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
                Region otherRegion = GridMap.Instance.allRegions[i];
                if (otherRegion != region) {
                    Vector3 directionToRegion = (otherRegion.coreTile.transform.position - region.coreTile.transform.position).normalized * (height + width);
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
            Debug.Log($"Getting target tile to go to {targetRegion.name} from {region.name}. Result was {coordinates.ToString()}");
            int xCoordinate = Mathf.Clamp((int)coordinates.x, 0, width - 1);
            int yCoordinate = Mathf.Clamp((int)coordinates.y, 0, height - 1);
            LocationGridTile targetTile = map[xCoordinate, yCoordinate];
            return targetTile;
        }
        #endregion

        #region Utilities
        private Vector2Int GetRegionDimensions(Region p_region) {
            int maxX = p_region.tiles.Max(t => t.data.xCoordinate);
            int minX = p_region.tiles.Min(t => t.data.xCoordinate);
            
            int gridWidth = maxX - minX;
            
            int maxY = p_region.tiles.Max(t => t.data.yCoordinate);
            int minY = p_region.tiles.Min(t => t.data.yCoordinate);
            int gridHeight = maxY - minY;
            
            return new Vector2Int(gridWidth + 1, gridHeight + 1);
        }
        private Vector2Int GetInnerMapSizeGivenRegionDimensions(Vector2Int p_dimensions) {
            return new Vector2Int(p_dimensions.x * InnerMapManager.AreaLocationGridTileSize.x, p_dimensions.y * InnerMapManager.AreaLocationGridTileSize.y);
        }
        #endregion
    }
}