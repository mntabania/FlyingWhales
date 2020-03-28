using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
namespace Inner_Maps {
    public abstract class InnerTileMap : MonoBehaviour {
        
        public static int WestEdge = 0;
        public static int NorthEdge = 0;
        public static int SouthEdge = 0;
        public static int EastEdge = 0;
        
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
        
        [FormerlySerializedAs("buildSpotPrefab")]
        [Header("Structures")]
        [SerializeField] protected GameObject tileCollectionPrefab;
        
        [Header("Perlin Noise")]
        [SerializeField] protected float offsetX;
        [SerializeField] protected float offsetY;
        
        [Header("For Testing")]
        [SerializeField] protected LineRenderer pathLineRenderer;
        [SerializeField] protected BoundDrawer _boundDrawer;
        //properties
        public int width { get; set; }
        public int height { get; set; }
        public LocationGridTile[,] map { get; private set; }
        protected List<LocationGridTile> allTiles { get; private set; }
        public List<LocationGridTile> allEdgeTiles { get; private set; }
        public Region region { get; private set; }
        public GridGraph pathfindingGraph { get; set; }
        public Vector3 worldPos { get; private set; }
        public GameObject centerGo { get; private set; }
        public List<BurningSource> activeBurningSources { get; private set; }
        public LocationGridTileCollection[,] locationGridTileCollections { get; protected set; }
        public bool isShowing => InnerMapManager.Instance.currentlyShowingMap == this;

        #region Generation
        public virtual void Initialize(Region location) {
            this.region = location;
            activeBurningSources = new List<BurningSource>();
            
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
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), InnerMapManager.Instance.assetManager.GetOutsideFloorTile(region));
                    LocationGridTile tile = new LocationGridTile(x, y, groundTilemap, this);
                    tile.CreateGenericTileObject();
                    tile.SetStructure(wilderness);
                    allTiles.Add(tile);
                    if (tile.IsAtEdgeOfWalkableMap()) {
                        allEdgeTiles.Add(tile);
                    }
                    map[x, y] = tile;
                }
                batchCount++;
                if (batchCount == MapGenerationData.InnerMapTileGenerationBatches) {
                    batchCount = 0;
                    yield return null;    
                }
            }
            allTiles.ForEach(x => x.FindNeighbours(map));
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} GenerateGrid took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }

        #endregion

        #region Visuals
        public void ClearAllTilemaps() {
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
        public List<LocationGridTile> GetUnoccupiedTilesInRadius(LocationGridTile centerTile, int radius, int radiusLimit = 0, bool includeCenterTile = false, bool includeTilesInDifferentStructure = false) {
            List<LocationGridTile> tiles = new List<LocationGridTile>();
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
            return tiles;
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
        public void PlaceObject(IPointOfInterest obj, LocationGridTile tile, bool placeAsset = true) {
            switch (obj.poiType) {
                case POINT_OF_INTEREST_TYPE.CHARACTER:
                    OnPlaceCharacterOnTile(obj as Character, tile);
                    break;
                default:
                    tile.SetObjectHere(obj);
                    break;
            }
        }
        public void RemoveObject(LocationGridTile tile, Character removedBy = null) {
            tile.RemoveObjectHere(removedBy);
        }
        public void RemoveObjectWithoutDestroying(LocationGridTile tile) {
            tile.RemoveObjectHereWithoutDestroying();
        }
        public void RemoveObjectDestroyVisualOnly(LocationGridTile tile, Character remover = null) {
            tile.RemoveObjectHereDestroyVisualOnly(remover);
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
                from.RemoveCharacterHere(character);
                to.AddCharacterHere(character);
                if (from.structure != to.structure) {
                    from.structure?.RemoveCharacterAtLocation(character);
                    if (to.structure != null) {
                        to.structure.AddCharacterAtLocation(character);
                    } else {
                        throw new Exception($"{character.name} is going to tile {to} which does not have a structure!");
                    }
                }
                if (from.collectionOwner.partOfHextile != to.collectionOwner.partOfHextile) {
                    if (from.collectionOwner.isPartOfParentRegionMap) {
                        from.collectionOwner.partOfHextile.hexTileOwner.OnRemovePOIInHex(character);
                    }
                    if (to.collectionOwner.isPartOfParentRegionMap) {
                        to.collectionOwner.partOfHextile.hexTileOwner.OnPlacePOIInHex(character);
                    }
                }
                Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.REMOVE_STATUS, character as IPointOfInterest);
                Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.APPREHEND, character as IPointOfInterest);
                Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.KNOCKOUT, character as IPointOfInterest);
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

        #region Burning Source
        public void AddActiveBurningSource(BurningSource bs) {
            if (!activeBurningSources.Contains(bs)) {
                activeBurningSources.Add(bs);
                Log log = new Log(GameManager.Instance.Today(), "General", "Location", "Fire");
                log.AddToFillers(region, region.name, LOG_IDENTIFIER.LANDMARK_1);
                PlayerManager.Instance.player.ShowNotificationFrom(region, log);
            }
        }
        public void RemoveActiveBurningSources(BurningSource bs) {
            activeBurningSources.Remove(bs);
        }
        #endregion

        #region Utilities
        public void CleanUp() {
            UtilityScripts.Utilities.DestroyChildren(objectsParent);
        }
        public void Open() { }
        public void Close() { }
        public virtual void OnMapGenerationFinished() {
            name = $"{region.name}'s Inner Map";
            groundTilemap.CompressBounds();
            _boundDrawer.ManualUpdateBounds(groundTilemap.localBounds);
            worldUiCanvas.worldCamera = InnerMapCameraMove.Instance.innerMapsCamera;
            var orthographicSize = InnerMapCameraMove.Instance.innerMapsCamera.orthographicSize;
            cameraBounds = new Vector4 {x = -185.8f}; //x - minX, y - minY, z - maxX, w - maxY 
            cameraBounds.y = orthographicSize;
            cameraBounds.z = (cameraBounds.x + width) - 28.5f;
            cameraBounds.w = height - orthographicSize;
            SpawnCenterGo();
        }
        private void SpawnCenterGo() {
            centerGo = Instantiate<GameObject>(centerGoPrefab, transform);
            Vector3 centerPosition = new Vector3(width/2f, height/2f); //new Vector3((cameraBounds.x + cameraBounds.z) * 0.5f, (cameraBounds.y + cameraBounds.w) * 0.5f);
            centerGo.transform.localPosition = centerPosition;
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
            List<Vector3> points = new List<Vector3>(character.marker.pathfindingAI.currentPath.vectorPath);
            int indexAt = 0; //the index that the character is at.
            float nearestDistance = 9999f;
            //refine the current path to remove points that the character has passed.
            //to do that, get the point in the list that the character is nearest to, then remove all other points before that point
            for (int i = 0; i < points.Count; i++) {
                Vector3 currPoint = points[i];
                float distance = Vector3.Distance(character.marker.transform.position, currPoint);
                if (distance < nearestDistance) {
                    indexAt = i;
                    nearestDistance = distance;
                }
            }
            //Debug.Log(character.name + " is at index " + indexAt.ToString() + ". current path length is " + points.Count);
            if (points.Count > 0) {
                for (int i = 0; i <= indexAt; i++) {
                    points.RemoveAt(0);
                }
            }
            //points.Insert(0, character.marker.transform.position);
            //Debug.Log(character.name + " new path length is " + points.Count);
            ShowPath(points);
        }
        private void HidePath() {
            pathLineRenderer.gameObject.SetActive(false);
        }
        #endregion

        #region Structures
        public void PlaceBuiltStructureTemplateAt(GameObject structurePrefab, HexTile hexTile, BaseSettlement settlement) {
            GameObject structureTemplateGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(
                structurePrefab.name, hexTile.GetCenterLocationGridTile().centeredLocalLocation,
                Quaternion.identity, structureParent);
        
            StructureTemplate structureTemplate = structureTemplateGO.GetComponent<StructureTemplate>();

            for (int i = 0; i < structureTemplate.structureObjects.Length; i++) {
                LocationStructureObject structureObject = structureTemplate.structureObjects[i];
                structureObject.RefreshAllTilemaps();
                List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(this);
                structureObject.SetTilesInStructure(occupiedTiles.ToArray());
                structureObject.ClearOutUnimportantObjectsBeforePlacement();
                LocationStructure structure =
                    LandmarkManager.Instance.CreateNewStructureAt(hexTile.region, structureObject.structureType,
                        settlement);
                for (int j = 0; j < occupiedTiles.Count; j++) {
                    LocationGridTile tile = occupiedTiles[j];
                    tile.SetStructure(structure);
                }
                structure.SetStructureObject(structureObject);
                structure.SetOccupiedHexTile(hexTile.innerMapHexTile);
                structureObject.OnBuiltStructureObjectPlaced(this, structure);
            }
            
            hexTile.innerMapHexTile.Occupy();
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
        private IEnumerator MapPerlinDetails(List<LocationGridTile> tiles) {
            offsetX = Random.Range(0f, 99999f);
            offsetY = Random.Range(0f, 99999f);
            int minX = tiles.Min(t => t.localPlace.x);
            int maxX = tiles.Max(t => t.localPlace.x);
            int minY = tiles.Min(t => t.localPlace.y);
            int maxY = tiles.Max(t => t.localPlace.y);

            int xSize = maxX - minX;
            int ySize = maxY - minY;

            int batchCount = 0;
            Vector3Int[] positionArray = new Vector3Int[tiles.Count];
            TileBase[] groundTilesArray = new TileBase[tiles.Count];
            
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                HexTile hex = region.coreTile;
                if(currTile.collectionOwner != null && currTile.collectionOwner.partOfHextile != null) {
                    hex = currTile.collectionOwner.partOfHextile.hexTileOwner;
                }
                float xCoord = (float)currTile.localPlace.x / xSize * 11f + offsetX;
                float yCoord = (float)currTile.localPlace.y / ySize * 11f + offsetY;

                float floorSample = Mathf.PerlinNoise(xCoord, yCoord);
                positionArray[i] = currTile.localPlace;
                currTile.SetFloorSample(floorSample);
                //ground
                groundTilesArray[i] = GetGroundAssetPerlin(floorSample, hex.biomeType);
                currTile.SetPreviousGroundVisual(null);
                
                // batchCount++;
                // if (batchCount == MapGenerationData.InnerMapDetailBatches) {
                //     batchCount = 0;
                //     yield return null;    
                // }
            }

            MassSetGroundTileMapVisuals(positionArray, groundTilesArray);
            
            batchCount = 0;
            //flower, rock and garbage
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                
                if (ReferenceEquals(currTile.collectionOwner.partOfHextile, null) == false) {
                    if ((currTile.collectionOwner.partOfHextile.hexTileOwner.elevationType == ELEVATION.MOUNTAIN 
                         || currTile.collectionOwner.partOfHextile.hexTileOwner.elevationType == ELEVATION.WATER)) {
                        continue; //skip other details generation for tiles belonging to mountain or water tiles, since they will be overwritten after ElevationStructureGeneration anyway.    
                    }
                    if (currTile.collectionOwner.partOfHextile.hexTileOwner.landmarkOnTile != null 
                        && currTile.collectionOwner.partOfHextile.hexTileOwner.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.MONSTER_LAIR) {
                        continue; //skip other details generation for tiles belonging to monster lair, since they will be overwritten anyway.    
                    }
                }
                
                float xCoordDetail = (float)currTile.localPlace.x / xSize * 8f + offsetX;
                float yCoordDetail = (float)currTile.localPlace.y / ySize * 8f + offsetY;
                float sampleDetail = Mathf.PerlinNoise(xCoordDetail, yCoordDetail);
                
                //trees and shrubs
                if (!currTile.hasDetail && currTile.HasNeighbouringWalledStructure() == false) {
                    if (sampleDetail < 0.5f) {
                        if (currTile.groundType == LocationGridTile.Ground_Type.Grass || currTile.groundType == LocationGridTile.Ground_Type.Snow) {
                            if (Random.Range(0, 100) < 50) {
                                //shrubs
                                if (region.coreTile.biomeType != BIOMES.SNOW && region.coreTile.biomeType != BIOMES.TUNDRA) {
                                    currTile.hasDetail = true;
                                    TileBase tileBase = null;
                                    //plant or herb plant
                                    if(UnityEngine.Random.Range(0, 2) == 0) {
                                        tileBase = InnerMapManager.Instance.assetManager.shrubTile;
                                    } else {
                                        tileBase = InnerMapManager.Instance.assetManager.herbPlantTile;
                                    }
                                    detailsTilemap.SetTile(currTile.localPlace, tileBase);
                                    if (currTile.structure != null) {
                                        //place tile object
                                        ConvertDetailToTileObject(currTile);
                                    } else {
                                        //place detail instead
                                        currTile.SetTileState(LocationGridTile.Tile_State.Empty);
                                        Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), Vector3.one);
                                        detailsTilemap.RemoveTileFlags(currTile.localPlace, TileFlags.LockTransform);
                                        detailsTilemap.SetTransformMatrix(currTile.localPlace, m);
                                    }
                                    continue; //go to next tile.
                                }
                            }
                        }
                    } 
                    
                    if (Random.Range(0, 100) < 3) {
                        currTile.hasDetail = true;
                        detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.GetFlowerTile(region));
                        if (currTile.structure != null) {
                            ConvertDetailToTileObject(currTile);
                        } else {
                            currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                        }
                        
                    } else if (Random.Range(0, 100) < 4) {
                        currTile.hasDetail = true;
                        detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.GetRockTile(region));
                        if (currTile.structure != null) {
                            ConvertDetailToTileObject(currTile);
                        } else {
                            currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                        }
                    } else if (Random.Range(0, 100) < 3) {
                        currTile.hasDetail = true;
                        detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.GetGarbTile(region));
                        if (currTile.structure != null) {
                            ConvertDetailToTileObject(currTile);
                        } else {
                            currTile.SetTileState(LocationGridTile.Tile_State.Occupied);
                        }
                    }
                }
                
                batchCount++;
                if (batchCount == MapGenerationData.InnerMapDetailBatches) {
                    batchCount = 0;
                    yield return null;    
                }
            }
        }
        protected IEnumerator GenerateDetails(MapGenerationComponent mapGenerationComponent) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            //Generate details for the outside map
            List<LocationGridTile> tilesToPerlin = allTiles.Where(x =>
                x.objHere == null
                && (x.structure == null || x.structure.structureType == STRUCTURE_TYPE.WILDERNESS ||
                    x.structure.structureType == STRUCTURE_TYPE.WORK_AREA)
                && x.tileType != LocationGridTile.Tile_Type.Wall
                && !x.IsAdjacentTo(typeof(MagicCircle))
            ).ToList();
            yield return StartCoroutine(MapPerlinDetails(tilesToPerlin));
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} GenerateDetails took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        public void MassSetGroundTileMapVisuals(Vector3Int[] tilePositions, TileBase[] groundAssets) {
            groundTilemap.SetTiles(tilePositions, groundAssets);
            for (int i = 0; i < tilePositions.Length; i++) {
                LocationGridTile tile = map[tilePositions[i].x, tilePositions[i].y];
                tile.UpdateGroundTypeBasedOnAsset();
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
            if (UIManager.Instance.characterInfoUI.isShowing 
                && UIManager.Instance.characterInfoUI.activeCharacter.currentRegion == region.coreTile.region
                && !UIManager.Instance.characterInfoUI.activeCharacter.isDead
                //&& UIManager.Instance.characterInfoUI.activeCharacter.isWaitingForInteraction <= 0
                && UIManager.Instance.characterInfoUI.activeCharacter.marker
                && UIManager.Instance.characterInfoUI.activeCharacter.marker.pathfindingAI.hasPath
                && (UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState == null 
                    || (UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.PATROL 
                        && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.STROLL
                        && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.STROLL_OUTSIDE
                        && UIManager.Instance.characterInfoUI.activeCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.BERSERKED))) {

                if (UIManager.Instance.characterInfoUI.activeCharacter.marker.pathfindingAI.currentPath != null
                    && UIManager.Instance.characterInfoUI.activeCharacter.currentParty.icon.isTravelling) {
                    //ShowPath(UIManager.Instance.characterInfoUI.activeCharacter.marker.currentPath);
                    ShowPath(UIManager.Instance.characterInfoUI.activeCharacter);
                    //UIManager.Instance.characterInfoUI.activeCharacter.marker.HighlightHostilesInRange();
                } else {
                    HidePath();
                }
            } else {
                HidePath();
            }
        }
        #endregion
        
    }
}
