using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

namespace Inner_Maps {
    public class RegionInnerTileMap : InnerTileMap {
        private Region region { get; set; }
        public Dictionary<Region, Transform> otherRegionObjects { get; private set; } //dictionary of objects to show which direction other regions are from this one.

        [SerializeField] private GameObject regionDirectionPrefab;
        
        public override void Initialize(ILocation location) {
            base.Initialize(location);
            this.region = location as Region;
        }
        public IEnumerator GenerateMap(MapGenerationComponent mapGenerationComponent) {
            this.name = $"{region.name}'s Inner Map";
            region.SetRegionInnerMap(this);
            ClearAllTilemaps();
            Vector2Int buildSpotGridSize = CreateBuildSpotGrid(mapGenerationComponent);
            int tileMapWidth = buildSpotGridSize.x * InnerMapManager.BuildingSpotSize.x;
            int tileMapHeight = buildSpotGridSize.y * InnerMapManager.BuildingSpotSize.y;
            yield return StartCoroutine(GenerateGrid(tileMapWidth, tileMapHeight, mapGenerationComponent));
            InitializeBuildingSpots(mapGenerationComponent);
            ConnectHexTilesToBuildSpots(mapGenerationComponent);
            yield return StartCoroutine(GenerateDetails(mapGenerationComponent));
        }

        #region Build Spots
        private Vector2Int CreateBuildSpotGrid(MapGenerationComponent mapGenerationComponent) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            int buildSpotGridWidth;
            
            int maxX = region.tiles.Max(t => t.data.xCoordinate);
            int minX = region.tiles.Min(t => t.data.xCoordinate);

            int difference = ((maxX - minX) + 1) * 2;

            if (region.AreLeftAndRightMostTilesInSameRowType()) {
                buildSpotGridWidth = difference;
            } else {
                buildSpotGridWidth = difference + 1;
            }
            
            int maxY = region.tiles.Max(t => t.data.yCoordinate);
            int minY = region.tiles.Min(t => t.data.yCoordinate);
            int buildSpotGridHeight = ((maxY - minY) + 1) * 2;
            
            buildingSpots = new BuildingSpot[buildSpotGridWidth, buildSpotGridHeight];
            for (int x = 0; x < buildSpotGridWidth; x++) {
                for (int y = 0; y < buildSpotGridHeight; y++) {
                    GameObject buildSpotGo = GameObject.Instantiate(buildSpotPrefab, this.structureTilemap.transform);
                    float xPos = (x + 1) * (InnerMapManager.BuildingSpotSize.x) - (InnerMapManager.BuildingSpotSize.x / 2f);
                    float yPos = (y + 1) * (InnerMapManager.BuildingSpotSize.y) - (InnerMapManager.BuildingSpotSize.y / 2f);
                    buildSpotGo.transform.localPosition = new Vector2(xPos, yPos);
                    BuildingSpot newSpot = new BuildingSpot(
                        new Vector3Int((int)xPos, (int)yPos, 0), new Vector2Int(x, y));
                    buildingSpots[x, y] = newSpot;
                    
                    BuildingSpotItem spotItem = buildSpotGo.GetComponent<BuildingSpotItem>();
                    spotItem.SetBuildingSpot(newSpot);
                    newSpot.SetBuildSpotItem(spotItem);
                }
            }
            
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{location.name} CreateBuildSpotGrid took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
            return new Vector2Int(buildSpotGridWidth, buildSpotGridHeight);
        }
        private void InitializeBuildingSpots(MapGenerationComponent mapGenerationComponent) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            int upperBoundX = buildingSpots.GetUpperBound(0);
            int upperBoundY = buildingSpots.GetUpperBound(1);
            for (int x = 0; x <= upperBoundX; x++) {
                for (int y = 0; y <= upperBoundY; y++) {
                    BuildingSpot spot = buildingSpots[x, y];
                    spot.Initialize(this);
                    spot.FindNeighbours(this);
                }
            }
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{location.name} initialize building spots took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        private void ConnectHexTilesToBuildSpots(MapGenerationComponent mapGenerationComponent) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            
            HexTile leftMostTile = region.GetLeftMostTile();
            List<int> leftMostRows = region.GetLeftMostRows();
            int xBound = region.hexTileMap.GetUpperBound(0);
            int yBound = region.hexTileMap.GetUpperBound(1);
            for (int localX = 0; localX <= xBound; localX++) {
                for (int localY = 0; localY <= yBound; localY++) {
                    HexTile firstTileInRow = region.hexTileMap[0, localY];
                    HexTile tile = region.hexTileMap[localX, localY];
                    if (tile.region == this.region) {
                        //the row will be indented if its row type (odd/even) is not the same as the row type of the left most tile.
                        //and the first tile in it's row is not null.
                        bool isIndented = UtilityScripts.Utilities.IsEven(tile.yCoordinate) !=
                                          UtilityScripts.Utilities.IsEven(leftMostTile.yCoordinate);

                        int buildSpotColumn1 = localX * 2;
                        int buildSpotColumn2 = buildSpotColumn1 + 1;
                        
                        if (isIndented) {
                            buildSpotColumn1 += 1;
                            buildSpotColumn2 += 1;
                            if (firstTileInRow.region != this.region) {
                                buildSpotColumn1 -= 2;
                                buildSpotColumn2 -= 2;
                            }
                        }


                        int buildSpotRow1 = localY * 2;
                        int buildSpotRow2 = buildSpotRow1 + 1;
                        AssignBuildSpotsToHexTile(tile, buildSpotColumn1, buildSpotColumn2,
                            buildSpotRow1, buildSpotRow2);    
                    }
                }
            }
            
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{location.name} ConnectHexTilesToBuildSpots took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        private void AssignBuildSpotsToHexTile(HexTile tile, int column1, int column2, int row1, int row2) {
            int w = (column2 - column1) + 1;
            int h = (row2 - row1) + 1;
            BuildingSpot[] spots = new BuildingSpot[w * h];
            int index = 0;
            for (int column = column1; column <= column2; column++) {
                for (int row = row1; row <= row2; row++) {
                    BuildingSpot spot = buildingSpots[column, row];
                    spot.SetHexTileOwner(tile);
                    spots[index] = spot;
                    index++;
                }
            }
            tile.SetOwnedBuildSpot(spots);
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
                    Vector3 directionToRegion = (otherRegion.coreTile.transform.position - region.coreTile.transform.position).normalized * 60f;
                    GameObject regionDirectionGO = Instantiate(regionDirectionPrefab, centerGo.transform, true);
                    regionDirectionGO.name = $"{otherRegion.name} direction";
                    regionDirectionGO.transform.localPosition = directionToRegion;
                    otherRegionObjects.Add(otherRegion, regionDirectionGO.transform);
                }
            }
        }
        private Vector3 GetClosestPointToRegion(Region targetRegion) {
            Bounds bounds = groundTilemap.localBounds;
            bounds.center = groundTilemap.localBounds.center + groundTilemap.transform.position;
            Vector3 closestPoint = bounds.ClosestPoint(otherRegionObjects[targetRegion].position);
            return transform.InverseTransformPoint(closestPoint);
        }
        public LocationGridTile GetTileToGoToRegion([NotNull]Region targetRegion) {
            Assert.IsTrue(targetRegion != this.region, $"target region passed is same as owning region! {targetRegion.name}");
            Vector3 coordinates = GetClosestPointToRegion(targetRegion);
            Debug.Log($"Getting target tile to go to {targetRegion.name} from {this.region.name}. Result was {coordinates.ToString()}");
            int xCoordinate = Mathf.Clamp((int)coordinates.x, 0, width - 1);
            int yCoordinate = Mathf.Clamp((int)coordinates.y, 0, height - 1);
            LocationGridTile targetTile = map[xCoordinate, yCoordinate];
            return targetTile;
        }
        #endregion
    }
}