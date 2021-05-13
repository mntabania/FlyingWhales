using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Cellular_Automata;
using Inner_Maps.Location_Structures;
using Perlin_Noise;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Inner_Maps {
    public abstract partial class InnerTileMap {
        
        #region Main
        private ELEVATION GetElevationFromMap(int x, int y, float[,] noiseMap) {
            float currentHeight = noiseMap[x, y];
            PerlinNoiseRegion noiseRegion = elevationPerlinSettings.GetPerlinNoiseRegion(currentHeight);
            if (noiseRegion.name.Equals("Water", StringComparison.InvariantCultureIgnoreCase)) {
                return ELEVATION.WATER;
            }
            else if (noiseRegion.name.Equals("Cave", StringComparison.InvariantCultureIgnoreCase)) {
                return ELEVATION.MOUNTAIN;
            }
            else {
                return ELEVATION.PLAIN;
            }
        }
        protected IEnumerator GenerateElevationMap(MapGenerationComponent mapGenerationComponent, MapGenerationData data) {
            float[,] noiseMap = Noise.GenerateNoiseMap(elevationPerlinSettings, width, height);
            ElevationIsland[][] elevationMap = new ElevationIsland[width][];
            for (int index = 0; index < width; index++) {
                elevationMap[index] = new ElevationIsland[height];
            }
            List<ElevationIsland> allElevationIslands = new List<ElevationIsland>();
            int batchCount = 0;

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    ELEVATION elevation = GetElevationFromMap(x, y, noiseMap);
                    LocationGridTile tile = map[x, y];
                    if (elevation == ELEVATION.PLAIN) { continue; }
                    bool wasAddedToAdjacentElevationIsland = false;
                    for (int i = 0; i < tile.neighbourList.Count; i++) {
                        LocationGridTile neighbour = tile.neighbourList[i];
                        ELEVATION neighbourElevation = GetElevationFromMap(neighbour.localPlace.x, neighbour.localPlace.y, noiseMap);
                        if (elevation == neighbourElevation) {
                            ElevationIsland elevationIsland = elevationMap[neighbour.localPlace.x][neighbour.localPlace.y];
                            if (elevationIsland != null) {
                                elevationIsland.AddTile(tile, data);
                                elevationMap[x][y] = elevationIsland;
                                wasAddedToAdjacentElevationIsland = true;
                                break;
                            }
                        }
                    }
                    if (!wasAddedToAdjacentElevationIsland) {
                        ElevationIsland elevationIsland = new ElevationIsland(elevation);
                        elevationIsland.AddTile(tile, data);
                        allElevationIslands.Add(elevationIsland);
                        elevationMap[x][y] = elevationIsland;
                    }
                    // yield return null;
                    batchCount++;
                    if (batchCount == MapGenerationData.InnerMapTileGenerationBatches) {
                        batchCount = 0;
                        yield return null;
                    }
                }
            }
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} GenerateElevationMap part 1 took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
            stopwatch.Reset();
            
            stopwatch.Start();
            //merge same elevation islands that are next to each other
            for (int k = 0; k < 2; k++) {
                for (int i = 0; i < allElevationIslands.Count; i++) {
                    ElevationIsland elevationIsland = allElevationIslands[i];
                    if (elevationIsland.tiles.Count == 0) { continue; }
                    for (int j = 0; j < allElevationIslands.Count; j++) {
                        ElevationIsland otherIsland = allElevationIslands[j];
                        if (otherIsland.tiles.Count == 0) { continue; }
                        if (elevationIsland != otherIsland && elevationIsland.elevation == otherIsland.elevation && elevationIsland.IsAdjacentToIsland(otherIsland)) {
                            elevationIsland.MergeWithIsland(otherIsland, data);
                        }
                        // yield return null;
                        batchCount++;
                        if (batchCount == MapGenerationData.InnerMapTileGenerationBatches) {
                            batchCount = 0;
                            yield return null;
                        }
                    }
                }
            }
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} GenerateElevationMap part 2 took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
            stopwatch.Reset();
            
            stopwatch.Start();
            //check if each island meets the needed tile requirement. If it does not, then set that island to be part of an adjacent island
            for (int i = 0; i < allElevationIslands.Count; i++) {
                ElevationIsland elevationIsland = allElevationIslands[i];
                if (elevationIsland.tiles.Count < ElevationIsland.MinimumTileRequirement) {
                    ElevationIsland adjacentIsland = elevationIsland.GetFirstAdjacentIsland(allElevationIslands);
                    if (adjacentIsland != null) {
                        adjacentIsland.MergeWithIsland(elevationIsland, data);    
                    } else {
                        for (int j = 0; j < elevationIsland.tiles.Count; j++) {
                            LocationGridTile tileInIsland = elevationIsland.tiles.ElementAt(j);
                            tileInIsland.SetElevation(ELEVATION.PLAIN);
                        }
                        elevationIsland.RemoveAllTiles();
                    }
                }
            }
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} GenerateElevationMap part 3 took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
            stopwatch.Reset();

            //remove odd elevation borders. Border tiles that are not next to much tiles of the same elevation.
            for (int i = 0; i < allElevationIslands.Count; i++) {
                ElevationIsland island = allElevationIslands[i];
                if (island.elevation == ELEVATION.WATER) {
                    List<LocationGridTile> borderTiles = island.borderTiles;
                    for (int j = 0; j < borderTiles.Count; j++) {
                        LocationGridTile tile = borderTiles[j];
                        if (tile.GetDifferentElevationNeighboursCount() == 5) {
                            island.RemoveTile(tile, data);
                        }
                    }    
                }
            }
            
            //clean up all biome islands list, remove islands with no tiles.
            for (int i = 0; i < allElevationIslands.Count; i++) {
                ElevationIsland biomeIsland = allElevationIslands[i];
                if (biomeIsland.tiles.Count == 0) {
                    allElevationIslands.RemoveAt(i);
                    i--;
                }
            }
            yield return StartCoroutine(CreateAndDrawElevationStructures(allElevationIslands, mapGenerationComponent));
        }
        public IEnumerator CreateAndDrawElevationStructures(List<ElevationIsland> allElevationIslands, MapGenerationComponent mapGenerationComponent) {
            //Create structure instances
            for (int i = 0; i < allElevationIslands.Count; i++) {
                ElevationIsland elevationIsland = allElevationIslands[i];
                if (elevationIsland.elevation == ELEVATION.PLAIN) {
                    continue;
                }
                STRUCTURE_TYPE structureType = elevationIsland.elevation.GetStructureTypeForElevation();
                NPCSettlement settlement = null;
                if (structureType == STRUCTURE_TYPE.CAVE) {
                    //only create settlement for caves
                    settlement = LandmarkManager.Instance.CreateNewSettlement(region, LOCATION_TYPE.DUNGEON, elevationIsland.occupiedAreas.ToArray());
                }
                LocationStructure elevationStructure = LandmarkManager.Instance.CreateNewStructureAt(region, structureType, settlement);
                if (structureType == STRUCTURE_TYPE.CAVE) {
                    yield return StartCoroutine(DrawCave(elevationIsland, elevationStructure, mapGenerationComponent));
                } else if (structureType == STRUCTURE_TYPE.OCEAN) {
                    yield return StartCoroutine(DrawOcean(elevationIsland, elevationStructure, mapGenerationComponent));
                }
                elevationStructure.SetOccupiedArea(elevationIsland.occupiedAreas.First());
            }
        }
        #endregion

        #region Caves
        private IEnumerator DrawCave(ElevationIsland p_island, LocationStructure p_caveStructure, MapGenerationComponent mapGenerationComponent) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            int batchCount = 0;
            List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
            tiles.AddRange(p_island.tiles);
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile tile = tiles[i];
                if (p_island.borderTiles.Contains(tile)) {
                    //set as wall
                    SetAsMountainWall(tile, p_caveStructure);
                } else {
                    SetAsMountainGround(tile, p_caveStructure);
                }
                batchCount++;
                if (batchCount == MapGenerationData.InnerMapElevationBatches) {
                    batchCount = 0;
                    yield return null;
                }
            }
            RuinarchListPool<LocationGridTile>.Release(tiles);
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} Draw Cave took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
            stopwatch.Reset();
            
            stopwatch.Start();
            yield return StartCoroutine(MountainCellAutomata(p_island.tiles.ToList(), p_caveStructure));
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} Draw Cave Cell Automata took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        private void SetAsMountainWall(LocationGridTile tile, LocationStructure structure) {
            if (tile.tileObjectComponent.objHere is BlockWall) { return; }
            tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.caveGroundTile);
            tile.SetTileType(LocationGridTile.Tile_Type.Wall);
            tile.SetTileState(LocationGridTile.Tile_State.Occupied);
            if (tile.structure != structure) {
                tile.SetStructure(structure);    
            }

            //create wall tile object
            BlockWall blockWall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
            blockWall.SetWallType(WALL_TYPE.Stone);
            structure.AddPOI(blockWall, tile);
            tile.SetIsDefault(false);
        }
        private void SetAsMountainGround(LocationGridTile tile, LocationStructure structure) {
            if (tile.structure != structure) {
                tile.SetStructure(structure);    
            }
            if (tile.tileObjectComponent.objHere is BlockWall) {
                tile.structure.RemovePOI(tile.tileObjectComponent.objHere);
            }
            tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.caveGroundTile);
            tile.SetIsDefault(false);
        }
        private IEnumerator MountainCellAutomata(List<LocationGridTile> locationGridTiles, LocationStructure elevationStructure) {
            LocationGridTile[,] tileMap = CellularAutomataGenerator.ConvertListToGridMap(locationGridTiles);
		    int fillPercent = 25;
		    int smoothing = 1;
		    if (locationGridTiles.Count > 200) { 
			    fillPercent = 35;
            } else if (locationGridTiles.Count > 300) { 
                fillPercent = 55;
            } else if (locationGridTiles.Count > 500) { 
                fillPercent = 75;
            }
		    int[,] cellMap = CellularAutomataGenerator.GenerateMap(tileMap, locationGridTiles, smoothing, fillPercent);
		    
		    Assert.IsNotNull(cellMap, $"There was no cellmap generated for elevation structure {elevationStructure.ToString()}");
		    
		    yield return MapGenerator.Instance.StartCoroutine(CellularAutomataGenerator.DrawMapCoroutine(tileMap, cellMap, InnerMapManager.Instance.assetManager.caveWallTile, 
			    null, 
			    (locationGridTile) => SetAsMountainWall(locationGridTile, elevationStructure),
			    (locationGridTile) => SetAsMountainGround(locationGridTile, elevationStructure)));
            
		    List<BlockWall> validWallsForOreVeins = RuinarchListPool<BlockWall>.Claim();
            elevationStructure.PopulateTileObjectsOfTypeThatIsBlockWallValidForOreVein2(validWallsForOreVeins);
		    
		    var randomOreAmount = elevationStructure.occupiedAreas.Count == 1 ? UnityEngine.Random.Range(4, 11) : UnityEngine.Random.Range(8, 16);
		    for (int i = 0; i < randomOreAmount; i++) {
			    if (validWallsForOreVeins.Count == 0) { break; }
			    BlockWall blockWall = CollectionUtilities.GetRandomElement(validWallsForOreVeins);
			    CreateOreVein(blockWall.gridTileLocation);
			    validWallsForOreVeins.Remove(blockWall);
		    }
            RuinarchListPool<BlockWall>.Release(validWallsForOreVeins);
        }
        //private bool IsBlockWallValidForOreVein(BlockWall p_blockWall) {
        //    if (p_blockWall.gridTileLocation != null) {
        //        List<LocationGridTile> caveNeighbours = p_blockWall.gridTileLocation.FourNeighbours().Where(t => t.tileObjectComponent.objHere is BlockWall).ToList();
        //        int wildernessNeighboursCount = p_blockWall.gridTileLocation.FourNeighbours().Count(t => t.structure.structureType == STRUCTURE_TYPE.WILDERNESS);
        //        if (caveNeighbours.Count == 2 && wildernessNeighboursCount == 1) {
        //            GridNeighbourDirection[] directions = new GridNeighbourDirection[2];
        //            for (int i = 0; i < caveNeighbours.Count; i++) {
        //                LocationGridTile neighbour = caveNeighbours[i];
        //                p_blockWall.gridTileLocation.TryGetNeighbourDirection(neighbour, out GridNeighbourDirection direction);
        //                directions[i] = direction;
        //            }
        //            return (directions[0] == GridNeighbourDirection.North && directions[1] == GridNeighbourDirection.South) || (directions[0] == GridNeighbourDirection.South && directions[1] == GridNeighbourDirection.North) ||
        //                   (directions[0] == GridNeighbourDirection.East && directions[1] == GridNeighbourDirection.West) || directions[0] == GridNeighbourDirection.West && directions[1] == GridNeighbourDirection.East;
        //        } else if (caveNeighbours.Count == 3 && wildernessNeighboursCount == 1) {
        //            return p_blockWall.gridTileLocation.neighbourList.Count(t => t.tileObjectComponent.objHere is BlockWall) == 5 && 
        //                   p_blockWall.gridTileLocation.neighbourList.Count(t => t.structure.structureType == STRUCTURE_TYPE.WILDERNESS) == 3;
        //        }
        //        // if (caveNeighbours == 2 || caveNeighbours == 4) {
        //        //     return p_blockWall.gridTileLocation.FourNeighbours().Count(t => t.structure is Wilderness) >= 1;	
        //        // }
        //    }
        //    return false;
        //}
        public void CreateOreVein(LocationGridTile tile) {
            if (tile != null) {
                if (tile.tileObjectComponent.objHere != null) {
                    tile.structure.RemovePOI(tile.tileObjectComponent.objHere);
                }
                TileObject well = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ORE_VEIN);
                tile.structure.AddPOI(well, tile);
            }
        }
        #endregion

        #region Ocean
        private IEnumerator DrawOcean(ElevationIsland p_island, LocationStructure p_oceanStructure, MapGenerationComponent mapGenerationComponent) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            int batchCount = 0;
            List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
            tiles.AddRange(p_island.tiles);
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile tile = tiles.ElementAt(i);
                SetAsWater(tile, p_oceanStructure);
                batchCount++;
                if (batchCount == MapGenerationData.InnerMapElevationBatches) {
                    batchCount = 0;
                    yield return null;
                }
            }
            
            RuinarchListPool<LocationGridTile>.Release(tiles);
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} Draw Ocean took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
            stopwatch.Reset();
            
            stopwatch.Start();
            
            //create water wells
            int westMost = p_oceanStructure.tiles.Min(t => t.localPlace.x);
            int eastMost = p_oceanStructure.tiles.Max(t => t.localPlace.x);
            int southMost = p_oceanStructure.tiles.Min(t => t.localPlace.y);
            int northMost = p_oceanStructure.tiles.Max(t => t.localPlace.y);
		
            LocationGridTile northTile = CollectionUtilities.GetRandomElement(p_oceanStructure.tiles.Where(t => t.localPlace.y == northMost && t.tileObjectComponent.objHere == null));
            if (!northTile.IsAtEdgeOfMap()) {
                CreateFishingSpot(northTile);    
            }
            
            LocationGridTile southTile = CollectionUtilities.GetRandomElement(p_oceanStructure.tiles.Where(t => t.localPlace.y == southMost && t.tileObjectComponent.objHere == null));
            if (!southTile.IsAtEdgeOfMap()) {
                CreateFishingSpot(southTile);
            }

            LocationGridTile westTile = CollectionUtilities.GetRandomElement(p_oceanStructure.tiles.Where(t => t.localPlace.x == westMost && t.tileObjectComponent.objHere == null));
            if (!westTile.IsAtEdgeOfMap()) {
                CreateFishingSpot(westTile);
            }

            LocationGridTile eastTile = CollectionUtilities.GetRandomElement(p_oceanStructure.tiles.Where(t => t.localPlace.x == eastMost && t.tileObjectComponent.objHere == null));
            if (!eastTile.IsAtEdgeOfMap()) {
                CreateFishingSpot(eastTile);
            }

            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} Create Fishing Spots took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
            stopwatch.Reset();
        }
        public void CreateFishingSpot(LocationGridTile tile) {
            if (tile != null) {
                TileObject well = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.FISHING_SPOT);
                tile.structure.AddPOI(well, tile);
                // well.mapObjectVisual.SetVisual(null);	
            }
        }
        private void SetAsWater(LocationGridTile tile, LocationStructure structure) {
            tile.SetStructureTilemapVisual(InnerMapManager.Instance.assetManager.shoreTile);
            tile.SetTileState(LocationGridTile.Tile_State.Occupied);
            tile.SetStructure(structure);
            tile.tileObjectComponent.genericTileObject.traitContainer.AddTrait(tile.tileObjectComponent.genericTileObject, "Wet", overrideDuration: 0);
        }
        #endregion
    }
}