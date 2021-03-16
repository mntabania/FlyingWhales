using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Pathfinding;
using Perlin_Noise;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UtilityScripts;
using Random = UnityEngine.Random;
namespace Inner_Maps {
    public abstract partial class InnerTileMap : BaseMonoBehaviour {

        [Header("Tile Maps")]
        [SerializeField] private Tilemap[] _allTilemaps;
        public Tilemap groundTilemap;
        public TilemapRenderer groundTilemapRenderer;
        public Tilemap detailsTilemap;
        public TilemapRenderer detailsTilemapRenderer;
        public Tilemap structureTilemap;
        public TilemapCollider2D structureTilemapCollider;
        public Tilemap upperGroundTilemap;
        public TilemapRenderer upperGroundTilemapRenderer;
        public Tilemap perlinTilemap;
        
        [Header("Seamless Edges")]
        public Tilemap northEdgeTilemap;
        public TilemapRenderer northEdgeTilemapRenderer;
        public Tilemap southEdgeTilemap;
        public TilemapRenderer southEdgeTilemapRenderer;
        public Tilemap westEdgeTilemap;
        public TilemapRenderer westEdgeTilemapRenderer;
        public Tilemap eastEdgeTilemap;
        public TilemapRenderer eastEdgeTilemapRenderer;
        
        [Header("Parents")]
        public Transform objectsParent;
        public Transform structureParent;
        [FormerlySerializedAs("worldUICanvas")] public Canvas worldUiCanvas;
        public Grid grid;
        
        [Header("Other")]
        [FormerlySerializedAs("centerGOPrefab")] public GameObject centerGoPrefab;
        public Vector4 cameraBounds;
        
        [Header("Structures")]
        [SerializeField] protected GameObject areaItemPrefab;
        
        [Header("Perlin Noise")]
        [SerializeField] private float _xSeed;
        [SerializeField] private float _ySeed;
        [NonSerialized] public PerlinNoiseSettings biomePerlinSettings = new PerlinNoiseSettings() {
            noiseScale = 40.4f,
            octaves = 3,
            persistance = 0f,
            lacunarity = 2,
            offset = new Vector2(13.09f, 12f),
            regions = new PerlinNoiseRegion[] {
                new PerlinNoiseRegion() {
                    name = "DESERT",
                    color = Color.yellow,
                    height = 0.3f,
                },
                new PerlinNoiseRegion() {
                    name = "GRASSLAND",
                    color = Color.green,
                    height = 0.65f,
                },
                new PerlinNoiseRegion() {
                    name = "SNOW",
                    color = Color.white,
                    height = 1f,
                },
            }
        };
        [NonSerialized] public PerlinNoiseSettings elevationPerlinSettings = new PerlinNoiseSettings() {
            noiseScale = 34.15f,
            octaves = 4,
            persistance = 0.5f,
            lacunarity = 2f,
            offset = new Vector2(13.09f, 12f),
            regions = new PerlinNoiseRegion[] {
                new PerlinNoiseRegion() {
                    name = "Water",
                    color = Color.blue,
                    height = 0.25f,
                },
                new PerlinNoiseRegion() {
                    name = "Plain",
                    color = Color.green,
                    height = 0.65f,
                },
                new PerlinNoiseRegion() {
                    name = "Cave",
                    color = Color.black,
                    height = 1f,
                },
            }
        };
        
        [SerializeField] private float _biomeTransitionXSeed;
        [SerializeField] private float _biomeTransitionYSeed;

        [Header("For Testing")]
        [SerializeField] protected LineRenderer pathLineRenderer;
        [SerializeField] protected BoundDrawer _boundDrawer;
        //properties
        public int width;
        public int height;
        public LocationGridTile[,] map { get; private set; }
        protected List<LocationGridTile> allTiles { get; private set; }
        public List<LocationGridTile> allEdgeTiles { get; private set; }
        public Region region { get; private set; }
        public GridGraph pathfindingGraph { get; set; }
        public GridGraph unwalkableGraph { get; set; }
        public Vector3 worldPos { get; private set; }
        public GameObject centerGo { get; private set; }
        public NNConstraint onlyUnwalkableGraph { get; private set; }
        public NNConstraint onlyPathfindingGraph { get; private set; }

        private struct BiomeOrder {
            public BIOMES[] biomesInOrder;
            public override string ToString() {
                return biomesInOrder.ComafyList();
            }
        }
        private readonly BiomeOrder[] _biomeOrders = new[] {
            new BiomeOrder() {biomesInOrder = new[] {BIOMES.DESERT, BIOMES.GRASSLAND, BIOMES.FOREST}},
            new BiomeOrder() {biomesInOrder = new[] {BIOMES.DESERT, BIOMES.FOREST, BIOMES.SNOW}},
            new BiomeOrder() {biomesInOrder = new[] {BIOMES.DESERT, BIOMES.GRASSLAND, BIOMES.SNOW}},
            new BiomeOrder() {biomesInOrder = new[] {BIOMES.GRASSLAND, BIOMES.FOREST, BIOMES.SNOW}},
        };
        
        
        #region getters
        public bool isShowing => InnerMapManager.Instance.currentlyShowingMap == this;
        public float xSeed => _xSeed;
        public float ySeed => _ySeed;
        public float biomeTransitionXSeed => _biomeTransitionXSeed;
        public float biomeTransitionYSeed => _biomeTransitionYSeed;
        #endregion

        #region Generation
        public void Initialize(Region location, float xSeed, float ySeed, PerlinNoiseSettings biomeSettings, PerlinNoiseSettings elevationSettings, float biomeTransitionXSeed, float biomeTransitionYSeed) {
            region = location;
            _xSeed = xSeed;
            _ySeed = ySeed;

            biomePerlinSettings = biomeSettings;
            elevationPerlinSettings = elevationSettings;
            _biomeTransitionXSeed = biomeTransitionXSeed;
            _biomeTransitionYSeed = biomeTransitionYSeed;
            
            //set tile map sorting orders
            groundTilemapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder;
            detailsTilemapRenderer.sortingOrder = InnerMapManager.DetailsTilemapSortingOrder;
            
            northEdgeTilemapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 1;
            southEdgeTilemapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 1;
            westEdgeTilemapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 2;
            eastEdgeTilemapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 2;
            
            upperGroundTilemapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 3;
        }
        public void Initialize(Region location, float xSeed, float ySeed, int biomeSeed, int elevationSeed, float biomeTransitionXSeed, float biomeTransitionYSeed) {
            region = location;
            _xSeed = xSeed;
            _ySeed = ySeed;

            BiomeOrder chosenOrder = CollectionUtilities.GetRandomElement(_biomeOrders);
            Debug.Log($"Chosen biome order is {chosenOrder.ToString()}");
            for (int i = 0; i < chosenOrder.biomesInOrder.Length; i++) {
                BIOMES biomes = chosenOrder.biomesInOrder[i];
                PerlinNoiseRegion perlinNoiseRegion = biomePerlinSettings.regions[i];
                perlinNoiseRegion.name = biomes.ToString();
                biomePerlinSettings.regions[i] = perlinNoiseRegion;
            }

            biomePerlinSettings.seed = biomeSeed;
            elevationPerlinSettings.seed = elevationSeed;
            _biomeTransitionXSeed = biomeTransitionXSeed;
            _biomeTransitionYSeed = biomeTransitionYSeed;
            
            //set tile map sorting orders
            groundTilemapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder;
            detailsTilemapRenderer.sortingOrder = InnerMapManager.DetailsTilemapSortingOrder;
            
            northEdgeTilemapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 1;
            southEdgeTilemapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 1;
            westEdgeTilemapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 2;
            eastEdgeTilemapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 2;
            
            upperGroundTilemapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 3;
        }
        protected IEnumerator GenerateGrid(int width, int height, MapGenerationComponent mapGenerationComponent) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            this.width = width;
            this.height = height;

            map = new LocationGridTile[width, height];
            allTiles = new List<LocationGridTile>();
            allEdgeTiles = new List<LocationGridTile>();
            int batchCount = 0;
            LocationStructure wilderness = region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            Vector3Int[] positionArray = new Vector3Int[width * height];
            TileBase[] groundTilesArray = new TileBase[width * height];

            int count = 0;
            TileBase regionOutsideTile = InnerMapManager.Instance.assetManager.outsideTile;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    var position = new Vector3Int(x, y, 0);
                    positionArray[count] = position;
                    groundTilesArray[count] = regionOutsideTile;
                    Area area = DetermineAreaGivenCoordinates(x, y);
                    LocationGridTile tile = new LocationGridTile(x, y, groundTilemap, this, area);
                    area.gridTileComponent.AddGridTile(tile);
                    area.elevationComponent.OnTileAddedToArea(tile);
                    tile.tileObjectComponent.CreateGenericTileObject();
                    tile.SetStructure(wilderness);
                    allTiles.Add(tile);
                    if (tile.IsAtEdgeOfWalkableMap()) {
                        allEdgeTiles.Add(tile);
                    }
                    map[x, y] = tile;
                    count++;
                    batchCount++;
                    if (batchCount == MapGenerationData.InnerMapTileGenerationBatches) {
                        batchCount = 0;
                        yield return null;    
                    }
                }
            }
            groundTilemap.SetTiles(positionArray, groundTilesArray);
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} GenerateGrid took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
            stopwatch.Reset();
            
            stopwatch.Start();
            Parallel.ForEach(allTiles, (currentTile) => {
                currentTile.FindNeighbours(map);
            });
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} GridFindNeighbours took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        private Area DetermineAreaGivenCoordinates(int x, int y) {
            int hexTileX = Mathf.FloorToInt((float)x / InnerMapManager.AreaLocationGridTileSize.x);
            int hexTileY = Mathf.FloorToInt((float)y / InnerMapManager.AreaLocationGridTileSize.y);

            return GridMap.Instance.map[hexTileX, hexTileY];
        }
        protected IEnumerator LoadGrid(int width, int height, MapGenerationComponent mapGenerationComponent, SaveDataInnerMap saveDataInnerMap, SaveDataCurrentProgress saveData) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            this.width = width;
            this.height = height;

            map = new LocationGridTile[width, height];
            allTiles = new List<LocationGridTile>();
            allEdgeTiles = new List<LocationGridTile>();
            int batchCount = 0;
            
            Vector3Int[] positionArray = new Vector3Int[width * height];
            TileBase[] groundTilesArray = new TileBase[width * height];
            int count = 0;
            LocationStructure wilderness = region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            TileBase regionOutsideTile = InnerMapManager.Instance.assetManager.outsideTile;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    SaveDataLocationGridTile existingSaveData = saveDataInnerMap.GetSaveDataForTile(new Point(x, y));
                    var position = new Vector3Int(x, y, 0);
                    positionArray[count] = position;
                    groundTilesArray[count] = regionOutsideTile;
                    Area area = DetermineAreaGivenCoordinates(x, y);
                    LocationGridTile tile;
                    if (existingSaveData != null) {
                        //has existing save data
                        tile = existingSaveData.InitialLoad(groundTilemap, this, saveData, area);
                    } else {
                        tile = new LocationGridTile(x, y, groundTilemap, this, area);
                        tile.tileObjectComponent.CreateGenericTileObject();    
                    }
                    area.gridTileComponent.AddGridTile(tile);
                    area.elevationComponent.OnTileAddedToArea(tile);
                    tile.SetStructure(wilderness);
                    tile.tileObjectComponent.genericTileObject.SetGridTileLocation(tile); //had to do this since I could not set tile location before setting structure because awareness list depends on it.
                    allTiles.Add(tile);
                    if (tile.IsAtEdgeOfWalkableMap()) {
                        allEdgeTiles.Add(tile);
                    }
                    map[x, y] = tile;
                    
                    batchCount++;
                    if (batchCount == MapGenerationData.InnerMapTileGenerationBatches) {
                        batchCount = 0;
                        yield return null;    
                    }
                }
                
            }
            groundTilemap.SetTiles(positionArray, groundTilesArray);
            
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} Load Grid took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
            stopwatch.Reset();
            
            stopwatch.Start();
            // batchCount = 0;
            Parallel.ForEach(allTiles, (currentTile) => {
                currentTile.FindNeighbours(map);
            });
            // for (int i = 0; i < allTiles.Count; i++) {
            //     LocationGridTile tile = allTiles[i];
            //     tile.FindNeighbours(map);
            //     batchCount++;
            //     if (batchCount == MapGenerationData.InnerMapTileGenerationBatches) {
            //         batchCount = 0;
            //         yield return null;    
            //     }
            // }
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} GridFindNeighbours took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        public IEnumerator LoadTileVisuals(MapGenerationComponent mapGenerationComponent, SaveDataInnerMap saveDataInnerMap, Dictionary<string, TileBase> tileAssetDB) {
            int batchCount = 0;
            for (int i = 0; i < saveDataInnerMap.tileSaves.Values.Count; i++) {
                SaveDataLocationGridTile saveDataLocationGridTile = saveDataInnerMap.tileSaves.Values.ElementAt(i);
                LocationGridTile tile = map[(int)saveDataLocationGridTile.localPlace.x, (int)saveDataLocationGridTile.localPlace.y];
                //load tile assets
                if (!string.IsNullOrEmpty(saveDataLocationGridTile.groundTileMapAssetName)) {
                    tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.TryGetTileAsset(saveDataLocationGridTile.groundTileMapAssetName, tileAssetDB));    
                }
                if (!string.IsNullOrEmpty(saveDataLocationGridTile.wallTileMapAssetName)) {
                    tile.parentMap.structureTilemap.SetTile(tile.localPlace, InnerMapManager.Instance.assetManager.TryGetTileAsset(saveDataLocationGridTile.wallTileMapAssetName, tileAssetDB));
                    tile.UpdateGroundTypeBasedOnAsset();    
                }    
                batchCount++;
                if (batchCount == MapGenerationData.InnerMapTileGenerationBatches) {
                    batchCount = 0;
                    yield return null;    
                }
            }
            
            
        } 
        #endregion

        #region Visuals
        protected void ClearAllTileMaps() {
            for (var i = 0; i < _allTilemaps.Length; i++) {
                _allTilemaps[i].ClearAllTiles();
            }
        }
        public IEnumerator CreateSeamlessEdges() {
            int batchCount = 0;
            for (int i = 0; i < allTiles.Count; i++) {
                LocationGridTile tile = allTiles[i];
                if (tile.structure != null && !tile.structure.structureType.IsOpenSpace() 
                    && tile.structure.structureType != STRUCTURE_TYPE.MONSTER_LAIR) { continue; } //skip non open space structure tiles.
                tile.CreateSeamlessEdgesForTile(this);
                batchCount++;
                if (batchCount == MapGenerationData.InnerMapSeamlessEdgeBatches) {
                    batchCount = 0;
                    yield return null;
                }
            }
        }
        public void SetUpperGroundVisual(Vector3Int location, TileBase asset, float alpha = 1f) {
            upperGroundTilemap.SetTile(location, asset);
            Color color = upperGroundTilemap.GetColor(location);
            color.a = alpha;
            upperGroundTilemap.SetColor(location, color);
        }
        public void SetUpperGroundVisual(Vector3Int[] locations, TileBase[] assets) {
            upperGroundTilemap.SetTiles(locations, assets);
        }
        #endregion

        #region Data Getting
        public void GetUnoccupiedTilesInRadius(List<LocationGridTile> tiles, LocationGridTile centerTile, int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
            int mapSizeX = map.GetUpperBound(0);
            int mapSizeY = map.GetUpperBound(1);
            int x = centerTile.localPlace.x;
            int y = centerTile.localPlace.y;
            if (includeCenterTile) {
                tiles.Add(centerTile);
            }

            int xLimitLower = x - radiusLimit;
            int xLimitUpper = x + radiusLimit;
            int yLimitLower = y - radiusLimit;
            int yLimitUpper = y + radiusLimit;

            for (int dx = x - radius; dx <= x + radius; dx++) {
                for (int dy = y - radius; dy <= y + radius; dy++) {
                    if (dx >= 0 && dx <= mapSizeX && dy >= 0 && dy <= mapSizeY) {
                        if (dx == x && dy == y) {
                            continue;
                        }
                        if (radiusLimit > 0 && dx > xLimitLower && dx < xLimitUpper && dy > yLimitLower && dy < yLimitUpper) {
                            continue;
                        }
                        LocationGridTile result = map[dx, dy];
                        if ((!includeTilesInDifferentStructure && result.structure != centerTile.structure) || result.isOccupied || result.charactersHere.Count > 0) { continue; }
                        tiles.Add(result);
                    }
                }
            }
        }
        public LocationGridTile GetRandomUnoccupiedEdgeTile() {
            List<LocationGridTile> unoccupiedEdgeTiles = new List<LocationGridTile>();
            for (int i = 0; i < allEdgeTiles.Count; i++) {
                if (!allEdgeTiles[i].isOccupied && allEdgeTiles[i].structure != null) { // - There should not be a checker for structure, fix the generation of allEdgeTiles in AreaInnerTileMap's GenerateGrid
                    unoccupiedEdgeTiles.Add(allEdgeTiles[i]);
                }
            }
            if (unoccupiedEdgeTiles.Count > 0) {
                return unoccupiedEdgeTiles[Random.Range(0, unoccupiedEdgeTiles.Count)];
            }
            return null;
        }
        #endregion
        
        #region Points of Interest
        public void PlaceObject(TileObject obj, LocationGridTile tile, bool placeAsset = true) {
            tile.tileObjectComponent.SetObjectHere(obj);
            //switch (obj.poiType) {
            //    case POINT_OF_INTEREST_TYPE.CHARACTER:
            //        OnPlaceCharacterOnTile(obj as Character, tile);
            //        break;
            //    default:
            //        tile.tileObjectComponent.SetObjectHere(obj);
            //        break;
            //}
        }
        public void LoadObject(TileObject obj, LocationGridTile tile) {
            tile.tileObjectComponent.LoadObjectHere(obj);
            //switch (obj.poiType) {
            //    case POINT_OF_INTEREST_TYPE.CHARACTER:
            //        OnPlaceCharacterOnTile(obj as Character, tile);
            //        break;
            //    default:
            //        tile.tileObjectComponent.LoadObjectHere(obj);
            //        break;
            //}
        }
        public void RemoveObject(LocationGridTile tile, Character removedBy = null) {
            tile.tileObjectComponent.RemoveObjectHere(removedBy);
        }
        public void RemoveObjectWithoutDestroying(LocationGridTile tile) {
            tile.tileObjectComponent.RemoveObjectHereWithoutDestroying();
        }
        public void RemoveObjectDestroyVisualOnly(LocationGridTile tile, Character remover = null) {
            tile.tileObjectComponent.RemoveObjectHereDestroyVisualOnly(remover);
        }
        private void OnPlaceCharacterOnTile(Character character, LocationGridTile tile) {
            GameObject markerGO = character.marker.gameObject; 
            if (markerGO.transform.parent != objectsParent) {
                //This means that the character travelled to a different npcSettlement
                markerGO.transform.SetParent(objectsParent);
                markerGO.transform.localPosition = tile.centeredLocalLocation;
                // character.marker.UpdatePosition();
            }

            if (!character.marker.gameObject.activeSelf) {
                character.marker.gameObject.SetActive(true);
            }
        }
        public void OnCharacterMovedTo(Character character, LocationGridTile to, LocationGridTile from) {
            if (from == null) { 
                //from is null (Usually happens on start up, should not happen otherwise)
                to.AddCharacterHere(character);
                to.structure.AddCharacterAtLocation(character);
            } else {
                if (to.structure == null) {
                    return; //quick fix for when the character is pushed to a tile with no structure
                }
                if (from.structure != to.structure) {
                    from.structure?.RemoveCharacterAtLocation(character);
                    if (to.structure != null) {
                        to.structure.AddCharacterAtLocation(character);
                    } else {
                        throw new Exception($"{character.name} is going to tile {to} which does not have a structure!");
                    }
                } else if (character.currentStructure != to.structure) {
                    //Added this because there are times when a structure is built on a tile where the character is standing, when this happens, the structures of the tiles are changed (that is why it will not go through the first if statement because the from and to structure will be the same) but the current structure of the character will not
                    //So, in order for the current structure of the character to update, we added this condition
                    //Current example: When a necromancer builds a lair
                    character.currentStructure?.RemoveCharacterAtLocation(character);
                    if (to.structure != null) {
                        to.structure.AddCharacterAtLocation(character);
                    } else {
                        throw new Exception($"{character.name} is going to tile {to} which does not have a structure!");
                    }
                }
                if (from.area != to.area) {
                    from.area.OnRemovePOIInHex(character);
                    to.area.OnPlacePOIInHex(character);
                }
                from.RemoveCharacterHere(character);
                to.AddCharacterHere(character);
            }
        
        }
        #endregion

        #region Data Setting
        public void UpdateTilesWorldPosition() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    map[x, y].UpdateWorldLocation();
                }
            }
            SetWorldPosition();
        }
        public void SetWorldPosition() {
            worldPos = transform.position;
        }
        #endregion

        #region Utilities
        public void Open() { }
        public void Close() { }
        public virtual void OnMapGenerationFinished() {
            name = $"{region.name}'s Inner Map";
            groundTilemap.CompressBounds();
            _boundDrawer.ManualUpdateBounds(groundTilemap.localBounds);
            worldUiCanvas.worldCamera = InnerMapCameraMove.Instance.camera;
            var orthographicSize = InnerMapCameraMove.Instance.camera.orthographicSize;
            cameraBounds = new Vector4 {x = -185.8f}; //x - minX, y - minY, z - maxX, w - maxY 
            cameraBounds.y = orthographicSize;
            cameraBounds.z = (cameraBounds.x + width) - 28.5f;
            cameraBounds.w = height - orthographicSize;
            SpawnCenterGo();
            
            onlyUnwalkableGraph = NNConstraint.Default;
            onlyUnwalkableGraph.constrainWalkability = true;
            onlyUnwalkableGraph.walkable = true;
            onlyUnwalkableGraph.graphMask = GraphMask.FromGraph(unwalkableGraph);
            
            onlyPathfindingGraph = NNConstraint.Default;
            onlyPathfindingGraph.graphMask = GraphMask.FromGraph(pathfindingGraph);
        }
        private void SpawnCenterGo() {
            centerGo = Instantiate<GameObject>(centerGoPrefab, transform);
            Vector3 centerPosition = new Vector3(width/2f, height/2f); //new Vector3((cameraBounds.x + cameraBounds.z) * 0.5f, (cameraBounds.y + cameraBounds.w) * 0.5f);
            centerGo.transform.localPosition = centerPosition;
            //innerMapCenter = centerGo.GetComponent<InnerMapCenter>();
            //innerMapCenter.ResizeFogOfWarBasedOnTileMapSize(this);
        }
        private void ShowPath(List<Vector3> points) {
            pathLineRenderer.gameObject.SetActive(true);
            pathLineRenderer.positionCount = points.Count;
            Vector3[] positions = new Vector3[points.Count];
            for (int i = 0; i < points.Count; i++) {
                positions[i] = points[i];
            }
            pathLineRenderer.SetPositions(positions);
        }
        private void ShowPath(Character character) {
            List<Vector3> points = InnerMapManager.Instance.GetTrimmedPath(character);
            ShowPath(points);
        }
        private void HidePath() {
            pathLineRenderer.gameObject.SetActive(false);
        }
        public LocationGridTile GetTileFromWorldPos(Vector3 worldPosition) {
            Vector3Int cell = groundTilemap.WorldToCell(worldPosition);
            if (UtilityScripts.Utilities.IsInRange(cell.x, 0, width) &&
                UtilityScripts.Utilities.IsInRange(cell.y, 0, height)) {
                return map[cell.x, cell.y];    
            }
            return null;
        }
        public LocationGridTile GetTileFromMapCoordinates(int xPos, int yPos) {
            if (UtilityScripts.Utilities.IsInRange(xPos, 0, width) &&
                UtilityScripts.Utilities.IsInRange(yPos, 0, height)) {
                return map[xPos, yPos];    
            }
            return null;
        }
        #endregion

        #region Structures
        public List<LocationStructure> PlaceBuiltStructureTemplateAt(GameObject p_structurePrefab, Area p_area, BaseSettlement p_settlement) {
            GameObject structureTemplateGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(p_structurePrefab.name, p_area.gridTileComponent.centerGridTile.centeredLocalLocation, Quaternion.identity, structureParent);
            
            List<LocationStructure> createdStructures = new List<LocationStructure>();
            
            StructureTemplate structureTemplate = structureTemplateGO.GetComponent<StructureTemplate>();
            structureTemplate.transform.localScale = Vector3.one;
            for (int i = 0; i < structureTemplate.structureObjects.Length; i++) {
                LocationStructureObject structureObject = structureTemplate.structureObjects[i];
                if (structureObject == null) {
                    throw new Exception($"No LocationStructureObject for {p_structurePrefab.name}");
                }
                structureObject.RefreshAllTilemaps();
                List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(this);
                structureObject.SetTilesInStructure(occupiedTiles.ToArray());
                structureObject.ClearOutUnimportantObjectsBeforePlacement();
                LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(p_area.region, structureObject.structureType, p_settlement);
                createdStructures.Add(structure);
                for (int j = 0; j < occupiedTiles.Count; j++) {
                    LocationGridTile tile = occupiedTiles[j];
                    tile.SetStructure(structure);
                }
                
                Assert.IsTrue(structure is DemonicStructure || structure is ManMadeStructure);
                if (structure is DemonicStructure demonicStructure) {
                    demonicStructure.SetStructureObject(structureObject);    
                } else if (structure is ManMadeStructure manMadeStructure) {
                    manMadeStructure.SetStructureObject(structureObject);    
                }
                
                structure.SetOccupiedArea(p_area);
                structureObject.OnBuiltStructureObjectPlaced(this, structure, out int createdWalls, out int totalWalls);
                structure.CreateRoomsBasedOnStructureObject(structureObject);
                structure.OnBuiltNewStructure();
                
                if (createdWalls < totalWalls) {
                    int missingWalls = totalWalls - createdWalls;
                    TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.BLOCK_WALL);
                    structure.AdjustHP(-(missingWalls * tileObjectData.maxHP));
                }
            }
            return createdStructures;
        }
        /// <summary>
        /// Build a structure object at the given center tile.
        /// NOTE: This will also create a LocationStructure instance for the new structure.
        /// </summary>
        /// <param name="p_structurePrefab">The structure prefab to use.</param>
        /// <param name="centerTile">The center tile to place the prefab at.</param>
        /// <param name="p_settlement">The settlement that owns the structure that will be placed</param>
        /// <returns>The instance of the placed structure.</returns>
        public LocationStructure PlaceBuiltStructureTemplateAt(GameObject p_structurePrefab, LocationGridTile centerTile, BaseSettlement p_settlement) {
            GameObject structureTemplateGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(p_structurePrefab.name, centerTile.centeredLocalLocation, Quaternion.identity, structureParent);
        
            LocationStructureObject structureObject = structureTemplateGO.GetComponent<LocationStructureObject>();
            if (structureObject == null) {
                throw new Exception($"No LocationStructureObject for {p_structurePrefab.name}");
            }
            Area hexTile = centerTile.area;
            p_settlement.AddAreaToSettlement(hexTile);
            structureObject.RefreshAllTilemaps();
            List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(this);
            structureObject.SetTilesInStructure(occupiedTiles.ToArray());
            structureObject.ClearOutUnimportantObjectsBeforePlacement();
            LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(centerTile.parentMap.region, structureObject.structureType, p_settlement);
            for (int j = 0; j < occupiedTiles.Count; j++) {
                LocationGridTile tile = occupiedTiles[j];
                tile.SetStructure(structure);
            }
            
            Assert.IsTrue(structure is DemonicStructure || structure is ManMadeStructure);
            if (structure is DemonicStructure demonicStructure) {
                demonicStructure.SetStructureObject(structureObject);    
            } else if (structure is ManMadeStructure manMadeStructure) {
                manMadeStructure.SetStructureObject(structureObject);    
            }
            
            structure.SetOccupiedArea(centerTile.area);
            structureObject.OnBuiltStructureObjectPlaced(this, structure, out int createdWalls, out int totalWalls);
            structure.CreateRoomsBasedOnStructureObject(structureObject);
            structure.OnBuiltNewStructure();
            
            if (createdWalls < totalWalls) {
                int missingWalls = totalWalls - createdWalls;
                TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.BLOCK_WALL);
                structure.AdjustHP(-(missingWalls * tileObjectData.maxHP));
            }

            Debug.Log($"Placed {structure} at {centerTile}");
            return structure;
            // hexTile.innerMapHexTile.Occupy();
        }
        #endregion

        #region Details
        private void ConvertDetailToTileObject(LocationGridTile tile) {
            Sprite sprite = detailsTilemap.GetSprite(tile.localPlace);
            TileObject obj = InnerMapManager.Instance.CreateNewTileObject<TileObject>(InnerMapManager.Instance.GetTileObjectTypeFromTileAsset(sprite));
            tile.structure.AddPOI(obj, tile);
            obj.mapVisual.SetVisual(sprite);
            detailsTilemap.SetTile(tile.localPlace, null);
        }
        public List<LocationGridTile> GetTiles(Point size, LocationGridTile startingTile, List<LocationGridTile> mustBeIn = null) {
            List<LocationGridTile> tiles = new List<LocationGridTile>();

            // int upperBoundX = map.GetUpperBound(0);
            // int upperBoundY = map.GetUpperBound(1);
            
            for (int x = startingTile.localPlace.x; x < startingTile.localPlace.x + size.X; x++) {
                for (int y = startingTile.localPlace.y; y < startingTile.localPlace.y + size.Y; y++) {
                    if (x >= width || y >= height) {
                        continue; //skip
                    }
                    if (mustBeIn != null && !mustBeIn.Contains(map[x, y])) {
                        continue; //skip
                    }
                    tiles.Add(map[x, y]);
                }
            }
            return tiles;
        }
        protected IEnumerator GenerateDetails(MapGenerationComponent mapGenerationComponent, int xSize, int ySize) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            yield return StartCoroutine(MapPerlinDetails(allTiles, xSize, ySize, xSeed, ySeed));
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} GenerateDetails took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        protected IEnumerator GroundPerlin(List<LocationGridTile> tiles, int xSize, int ySize, float xSeed, float ySeed) {
            yield return StartCoroutine(BiomePerlin());
            Vector3Int[] positionArray = new Vector3Int[tiles.Count];
            TileBase[] groundTilesArray = new TileBase[tiles.Count];
            
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                float xCoord = (float)currTile.localPlace.x / xSize * 11f + xSeed;
                float yCoord = (float)currTile.localPlace.y / ySize * 11f + ySeed;

                float floorSample = Mathf.PerlinNoise(xCoord, yCoord);
                currTile.SetFloorSample(floorSample);
                positionArray[i] = currTile.localPlace;
                groundTilesArray[i] = GetGroundAssetPerlin(floorSample, currTile.biomeType); //currTile.biomeType
            }
            
            //Mass Update tiles
            groundTilemap.SetTiles(positionArray, groundTilesArray);
            for (int i = 0; i < positionArray.Length; i++) {
                LocationGridTile tile = map[positionArray[i].x, positionArray[i].y];
                tile.InitialUpdateGroundTypeBasedOnAsset();
            }
            yield return null;
        }
        private IEnumerator BiomePerlin() {
            float[,] noiseMap = Noise.GenerateNoiseMap(biomePerlinSettings, width, height);

            List<BiomeIsland> allBiomeIslands = new List<BiomeIsland>();
            
            BiomeIsland[][] biomeIslandMap = new BiomeIsland[width][];
            for (int index = 0; index < width; index++) {
                biomeIslandMap[index] = new BiomeIsland[height];
            }
            int batchCount = 0;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    float currentHeight = noiseMap[x, y];
                    PerlinNoiseRegion noiseRegion = biomePerlinSettings.GetPerlinNoiseRegion(currentHeight);
                    LocationGridTile tile = map[x, y];
                    string upperCase = noiseRegion.name.ToUpperInvariant();
                    BIOMES biome = (BIOMES)System.Enum.Parse(typeof(BIOMES), upperCase);
                    tile.SetIndividualBiomeType(biome);
                    bool wasAddedToAdjacentBiomeIsland = false;
                    for (int i = 0; i < tile.neighbourList.Count; i++) {
                        LocationGridTile neighbour = tile.neighbourList[i];
                        if (biome == neighbour.biomeType) {
                            BiomeIsland biomeIsland = biomeIslandMap[neighbour.localPlace.x][neighbour.localPlace.y];
                            if (biomeIsland != null) {
                                biomeIsland.AddTile(tile);
                                biomeIslandMap[x][y] = biomeIsland;
                                wasAddedToAdjacentBiomeIsland = true;
                                break;
                            }
                        }
                    }
                    if (!wasAddedToAdjacentBiomeIsland) {
                        BiomeIsland biomeIsland = new BiomeIsland(biome);
                        biomeIsland.AddTile(tile);
                        allBiomeIslands.Add(biomeIsland);
                        biomeIslandMap[x][y] = biomeIsland;
                    }
                    // yield return null;
                    batchCount++;
                    if (batchCount == MapGenerationData.InnerMapTileGenerationBatches) {
                        batchCount = 0;
                        yield return null;
                    }
                }
            }

            //merge same biome islands that are next to each other
            for (int k = 0; k < 2; k++) {
                for (int i = 0; i < allBiomeIslands.Count; i++) {
                    BiomeIsland biomeIsland = allBiomeIslands[i];
                    for (int j = 0; j < allBiomeIslands.Count; j++) {
                        BiomeIsland otherIsland = allBiomeIslands[j];
                        if (biomeIsland != otherIsland && biomeIsland.biome == otherIsland.biome && biomeIsland.IsAdjacentToIsland(otherIsland)) {
                            biomeIsland.MergeWithIsland(otherIsland);
                        }
                        batchCount++;
                        if (batchCount == MapGenerationData.InnerMapTileGenerationBatches) {
                            batchCount = 0;
                            yield return null;
                        }
                    }
                }
            }
            
            //clean up all biome islands list, remove islands with no tiles.
            for (int i = 0; i < allBiomeIslands.Count; i++) {
                BiomeIsland biomeIsland = allBiomeIslands[i];
                if (biomeIsland.tiles.Count == 0) {
                    allBiomeIslands.RemoveAt(i);
                    i--;
                }
            }
            
            //Remove Biome islands with less that or equal to the minimum amount of tiles.
            for (int i = 0; i < allBiomeIslands.Count; i++) {
                BiomeIsland biomeIsland = allBiomeIslands[i];
                if (biomeIsland.tiles.Count <= BiomeIsland.MinimumTilesInIsland) {
                    //merge with an adjacent island and set tiles to that biome
                    BiomeIsland adjacent = biomeIsland.GetFirstAdjacentIsland(allBiomeIslands);
                    adjacent?.MergeWithIsland(biomeIsland);
                }
            }
        }
        private IEnumerator MapPerlinDetails(List<LocationGridTile> tiles, int xSize, int ySize, float xSeed, float ySeed) {
            int batchCount = 0;
            //flower, rock and garbage
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                GenerateDetailOnTile(xSize, ySize, xSeed, ySeed, currTile);    
            
                batchCount++;
                if (batchCount == MapGenerationData.InnerMapDetailBatches) {
                    batchCount = 0;
                    yield return null;    
                }
            }
        }
        private void GenerateDetailOnTile(int xSize, int ySize, float xSeed, float ySeed, LocationGridTile currTile) {
            if ((currTile.elevationType == ELEVATION.MOUNTAIN
                 || currTile.elevationType == ELEVATION.WATER)) {
                return;
            }
            if (currTile.area.primaryStructureInArea != null && currTile.area.primaryStructureInArea.structureType == STRUCTURE_TYPE.MONSTER_LAIR) {
                return;
            }
            

            float xCoordDetail = (float) currTile.localPlace.x / xSize * 8f + xSeed;
            float yCoordDetail = (float) currTile.localPlace.y / ySize * 8f + ySeed;
            float sampleDetail = Mathf.PerlinNoise(xCoordDetail, yCoordDetail);

            //trees and shrubs
            if (currTile.tileObjectComponent.objHere == null && currTile.HasNeighbouringWalledStructure() == false) {
                if (sampleDetail < 0.55f) {
                    if (Random.Range(0, 100) < 50) {
                        //shrubs
                        if (currTile.groundType == LocationGridTile.Ground_Type.Grass && currTile.biomeType != BIOMES.SNOW && currTile.biomeType != BIOMES.TUNDRA && currTile.biomeType != BIOMES.DESERT) {
                            Assert.IsNotNull(currTile.structure);
                            TileBase tileBase = null;
                            //plant or herb plant

                            //35
                            tileBase = UtilityScripts.GameUtilities.RollChance(60) ? InnerMapManager.Instance.assetManager.shrubTile : InnerMapManager.Instance.assetManager.herbPlantTile;
                            if (currTile.biomeType == BIOMES.FOREST || currTile.biomeType == BIOMES.GRASSLAND) {
                                if (tileBase == InnerMapManager.Instance.assetManager.herbPlantTile) {
                                    if (UtilityScripts.GameUtilities.RollChance(10)) {
                                        TileObject obj = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.BERRY_SHRUB);
                                        currTile.structure.AddPOI(obj, currTile);
                                    }
                                    else {
                                        detailsTilemap.SetTile(currTile.localPlace, tileBase);
                                        //place tile object
                                        ConvertDetailToTileObject(currTile);
                                    }
                                }
                            }
                            return;
                        }
                    }
                }

                if (GameUtilities.RollChance(3)) {
                    detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.GetFlowerTile(currTile.biomeType));
                    Assert.IsNotNull(currTile.structure);
                    ConvertDetailToTileObject(currTile);
                    // TileObject obj = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WEREWOLF_PELT);
                    // currTile.structure.AddPOI(obj, currTile);
                } else if (GameUtilities.RollChance(4)) {
                    detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.GetRockTile(currTile.biomeType));
                    Assert.IsNotNull(currTile.structure);
                    ConvertDetailToTileObject(currTile);
                } else if (GameUtilities.RollChance(3)) {
                    detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.GetGarbTile(currTile.biomeType));
                    Assert.IsNotNull(currTile.structure);
                    ConvertDetailToTileObject(currTile);
                }
            }
        }
        public static TileBase GetGroundAssetPerlin(float floorSample, BIOMES biomeType) {
            if (biomeType == BIOMES.SNOW || biomeType == BIOMES.TUNDRA) {
                if (floorSample < 0.5f) {
                    return InnerMapManager.Instance.assetManager.snowTile;
                } else if (floorSample >= 0.5f && floorSample < 0.8f) {
                    return InnerMapManager.Instance.assetManager.snowDirt;
                } else {
                    return InnerMapManager.Instance.assetManager.stoneTile;
                }
            } else if (biomeType == BIOMES.DESERT) {
                if (floorSample < 0.5f) {
                    return InnerMapManager.Instance.assetManager.desertGrassTile;
                } else if (floorSample >= 0.5f && floorSample < 0.8f) {
                    return InnerMapManager.Instance.assetManager.desertSandTile;
                } else {
                    return InnerMapManager.Instance.assetManager.desertStoneGroundTile;
                }
            } else if (biomeType == BIOMES.FOREST) {
                if (floorSample < 0.8f) {
                    return InnerMapManager.Instance.assetManager.grassTile;
                } else {
                    return InnerMapManager.Instance.assetManager.stoneTile;
                }
            } else {
                if (floorSample < 0.5f) {
                    return InnerMapManager.Instance.assetManager.grassTile;
                } else if (floorSample >= 0.5f && floorSample < 0.8f) {
                    return InnerMapManager.Instance.assetManager.soilTile;
                } else {
                    return InnerMapManager.Instance.assetManager.stoneTile;
                }
            }
        }
        #endregion

        #region Monobehaviours
        public void Update() {
            Character activeCharacter = UIManager.Instance.GetCurrentlySelectedCharacter();
            if (activeCharacter != null && activeCharacter.currentRegion == region
                && !activeCharacter.isDead
                && activeCharacter.marker
                && activeCharacter.marker.pathfindingAI.hasPath
                && activeCharacter.carryComponent.IsNotBeingCarried()
                /* && (activeCharacter.stateComponent.currentState == null 
                    || (activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.PATROL 
                        && activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.STROLL
                        && activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.STROLL_OUTSIDE
                        && activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.BERSERKED))*/
                
                ) {
                if (activeCharacter.marker.pathfindingAI.currentPath != null && activeCharacter.marker.isMoving) {
                    ShowPath(activeCharacter);
                } else {
                    HidePath();
                }
            } else {
                HidePath();
            }
        }
        #endregion

        #region Cleanup
        public void CleanUp() {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    LocationGridTile locationGridTile = map[x, y];
                    locationGridTile?.CleanUp();
                }    
            }
            map = null;
            allTiles?.Clear();
            allTiles = null;
            allEdgeTiles?.Clear();
            allEdgeTiles = null;
            pathfindingGraph = null;
            Destroy(centerGo);
            centerGo = null;
        }
        #endregion
        
    }
}