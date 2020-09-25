using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Pathfinding;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
namespace Inner_Maps {
    public abstract class InnerTileMap : BaseMonoBehaviour {
        
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
        [SerializeField] private float _xSeed;
        [SerializeField] private float _ySeed;

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
        public Vector3 worldPos { get; private set; }
        public GameObject centerGo { get; private set; }
        public LocationGridTileCollection[,] locationGridTileCollections { get; protected set; }
        
        #region getters
        public bool isShowing => InnerMapManager.Instance.currentlyShowingMap == this;
        public float xSeed => _xSeed;
        public float ySeed => _ySeed;
        #endregion

        #region Generation
        public virtual void Initialize(Region location, float xSeed, float ySeed) {
            region = location;
            _xSeed = xSeed;
            _ySeed = ySeed;
            
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
        protected IEnumerator LoadGrid(int width, int height, MapGenerationComponent mapGenerationComponent, SaveDataInnerMap saveDataInnerMap, SaveDataCurrentProgress saveData) {
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
                    SaveDataLocationGridTile existingSaveData = saveDataInnerMap.GetSaveDataForTile(new Point(x, y));
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), InnerMapManager.Instance.assetManager.GetOutsideFloorTile(region));
                    LocationGridTile tile;
                    if (existingSaveData != null) {
                        //has existing save data
                        tile = existingSaveData.InitialLoad(groundTilemap, this, saveData);
                    } else {
                        tile = new LocationGridTile(x, y, groundTilemap, this);
                        tile.CreateGenericTileObject();    
                    }
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
            mapGenerationComponent.AddLog($"{region.name} Load Grid took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
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

        // #region Burning Source
        // public void AddActiveBurningSource(BurningSource bs) {
        //     if (!activeBurningSources.Contains(bs)) {
        //         activeBurningSources.Add(bs);
        //         // Log log = new Log(GameManager.Instance.Today(), "General", "Location", "Fire");
        //         // log.AddToFillers(region, region.name, LOG_IDENTIFIER.LANDMARK_1);
        //         // PlayerManager.Instance.player.ShowNotificationFrom(region, log);
        //     }
        // }
        // public void RemoveActiveBurningSources(BurningSource bs) {
        //     activeBurningSources.Remove(bs);
        // }
        // #endregion

        #region Utilities
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
        public void PredetermineGraphNodes() {
            //predetermine graph nodes
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    LocationGridTile tile = map[x, y];
                    tile.PredetermineGraphNode();
                }    
            }
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
        public void PlaceBuiltStructureTemplateAt(GameObject structurePrefab, HexTile hexTile, BaseSettlement settlement) {
            GameObject structureTemplateGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(structurePrefab.name, hexTile.GetCenterLocationGridTile().centeredLocalLocation, 
                Quaternion.identity, structureParent);
        
            StructureTemplate structureTemplate = structureTemplateGO.GetComponent<StructureTemplate>();
            structureTemplate.transform.localScale = Vector3.one;
            for (int i = 0; i < structureTemplate.structureObjects.Length; i++) {
                LocationStructureObject structureObject = structureTemplate.structureObjects[i];
                if (structureObject == null) {
                    throw new Exception($"No LocationStructureObject for {structurePrefab.name}");
                }
                structureObject.RefreshAllTilemaps();
                List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(this);
                structureObject.SetTilesInStructure(occupiedTiles.ToArray());
                structureObject.ClearOutUnimportantObjectsBeforePlacement();
                LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(hexTile.region, structureObject.structureType, settlement);
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
                
                structure.SetOccupiedHexTile(hexTile.innerMapHexTile);
                structureObject.OnBuiltStructureObjectPlaced(this, structure);
                structure.CreateRoomsBasedOnStructureObject(structureObject);
                structure.OnBuiltNewStructure();
            }

            hexTile.innerMapHexTile.Occupy();
        }
        /// <summary>
        /// Build a structure object at the given center tile.
        /// NOTE: This will also create a LocationStructure instance for the new structure.
        /// </summary>
        /// <param name="structurePrefab">The structure prefab to use.</param>
        /// <param name="centerTile">The center tile to place the prefab at.</param>
        /// <param name="settlement">The settlement that owns the structure that will be placed</param>
        /// <returns>The instance of the placed structure.</returns>
        public LocationStructure PlaceBuiltStructureTemplateAt(GameObject structurePrefab, LocationGridTile centerTile, BaseSettlement settlement) {
            GameObject structureTemplateGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(structurePrefab.name, centerTile.centeredLocalLocation, Quaternion.identity, structureParent);
        
            LocationStructureObject structureObject = structureTemplateGO.GetComponent<LocationStructureObject>();
            if (structureObject == null) {
                throw new Exception($"No LocationStructureObject for {structurePrefab.name}");
            }
            Assert.IsTrue(centerTile.collectionOwner.isPartOfParentRegionMap, $"Structure Object {structurePrefab.name} for {settlement} is being placed on unlinked tile {centerTile}");
            HexTile hexTile = centerTile.collectionOwner.partOfHextile.hexTileOwner;
            settlement.AddTileToSettlement(hexTile);
            structureObject.RefreshAllTilemaps();
            List<LocationGridTile> occupiedTiles = structureObject.GetTilesOccupiedByStructure(this);
            structureObject.SetTilesInStructure(occupiedTiles.ToArray());
            structureObject.ClearOutUnimportantObjectsBeforePlacement();
            LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(centerTile.parentMap.region, structureObject.structureType, settlement);
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
            
            structure.SetOccupiedHexTile(centerTile.collectionOwner.partOfHextile);
            structureObject.OnBuiltStructureObjectPlaced(this, structure);
            structure.CreateRoomsBasedOnStructureObject(structureObject);
            structure.OnBuiltNewStructure();

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
        protected IEnumerator GenerateDetails(MapGenerationComponent mapGenerationComponent) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            //Generate details for the outside map
            // List<LocationGridTile> tilesToPerlin = allTiles.Where(x =>
            //     x.objHere == null
            //     && (x.structure == null || x.structure.structureType == STRUCTURE_TYPE.WILDERNESS)
            //     && x.tileType != LocationGridTile.Tile_Type.Wall
            //     && !x.IsAdjacentTo(typeof(MagicCircle))
            // ).ToList();
            int minX = allTiles.Min(t => t.localPlace.x);
            int maxX = allTiles.Max(t => t.localPlace.x);
            int minY = allTiles.Min(t => t.localPlace.y);
            int maxY = allTiles.Max(t => t.localPlace.y);
            int xSize = maxX - minX;
            int ySize = maxY - minY;
            
            yield return StartCoroutine(MapPerlinDetails(allTiles, xSize, ySize, xSeed, ySeed));
            stopwatch.Stop();
            mapGenerationComponent.AddLog($"{region.name} GenerateDetails took {stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)} seconds to complete.");
        }
        public IEnumerator GroundPerlin(List<LocationGridTile> tiles, int xSize, int ySize, float xSeed, float ySeed) {
            Vector3Int[] positionArray = new Vector3Int[tiles.Count];
            TileBase[] groundTilesArray = new TileBase[tiles.Count];
            
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                HexTile hex = region.coreTile;
                if(currTile.collectionOwner != null && currTile.collectionOwner.isPartOfParentRegionMap) {
                    hex = currTile.collectionOwner.partOfHextile.hexTileOwner;
                }
                float xCoord = (float)currTile.localPlace.x / xSize * 11f + xSeed;
                float yCoord = (float)currTile.localPlace.y / ySize * 11f + ySeed;

                float floorSample = Mathf.PerlinNoise(xCoord, yCoord);
                positionArray[i] = currTile.localPlace;
                currTile.SetFloorSample(floorSample);
                groundTilesArray[i] = GetGroundAssetPerlin(floorSample, hex.biomeType);
            }

            //Mass Update tiles
            groundTilemap.SetTiles(positionArray, groundTilesArray);
            for (int i = 0; i < positionArray.Length; i++) {
                LocationGridTile tile = map[positionArray[i].x, positionArray[i].y];
                tile.InitialUpdateGroundTypeBasedOnAsset();
            }
            yield return null;
        }
        private IEnumerator MapPerlinDetails(List<LocationGridTile> tiles, int xSize, int ySize, float xSeed, float ySeed) {
            yield return StartCoroutine(GroundPerlin(tiles, xSize, ySize, xSeed, ySeed));
            int batchCount = 0;
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
                
                float xCoordDetail = (float)currTile.localPlace.x / xSize * 8f + xSeed;
                float yCoordDetail = (float)currTile.localPlace.y / ySize * 8f + ySeed;
                float sampleDetail = Mathf.PerlinNoise(xCoordDetail, yCoordDetail);
                
                //trees and shrubs
                if (currTile.objHere == null && currTile.HasNeighbouringWalledStructure() == false) {
                    //if (sampleDetail < 0.5f) {
                    //    if (currTile.groundType == LocationGridTile.Ground_Type.Grass || currTile.groundType == LocationGridTile.Ground_Type.Snow) {
                    //        if (Random.Range(0, 100) < 50) {
                    //            //shrubs
                    //            if (region.coreTile.biomeType != BIOMES.SNOW && region.coreTile.biomeType != BIOMES.TUNDRA) {
                    //                TileBase tileBase = null;
                    //                //plant or herb plant
                    //                tileBase = Random.Range(0, 2) == 0 ? InnerMapManager.Instance.assetManager.shrubTile : InnerMapManager.Instance.assetManager.herbPlantTile;
                    //                detailsTilemap.SetTile(currTile.localPlace, tileBase);
                    //                Assert.IsNotNull(currTile.structure);
                    //                //place tile object
                    //                ConvertDetailToTileObject(currTile);
                    //                continue; //skip next processing, since detail was already placed.
                    //            }
                    //        }
                    //    }
                    //} 

                    if (sampleDetail < 0.55f) {
                        if (Random.Range(0, 100) < 50) {
                            //shrubs
                            if (currTile.groundType == LocationGridTile.Ground_Type.Grass && region.coreTile.biomeType != BIOMES.SNOW && region.coreTile.biomeType != BIOMES.TUNDRA && region.coreTile.biomeType != BIOMES.DESERT) {
                                Assert.IsNotNull(currTile.structure);
                                TileBase tileBase = null;
                                //plant or herb plant
                                
                                tileBase = UtilityScripts.GameUtilities.RollChance(35) ? InnerMapManager.Instance.assetManager.shrubTile : InnerMapManager.Instance.assetManager.herbPlantTile;
                                if (region.coreTile.biomeType == BIOMES.FOREST || region.coreTile.biomeType == BIOMES.GRASSLAND) {
                                    if(tileBase == InnerMapManager.Instance.assetManager.herbPlantTile) {
                                        if (UtilityScripts.GameUtilities.RollChance(30)) {
                                            TileObject obj = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.BERRY_SHRUB);
                                            currTile.structure.AddPOI(obj, currTile);
                                        } else {
                                            detailsTilemap.SetTile(currTile.localPlace, tileBase);
                                            //place tile object
                                            ConvertDetailToTileObject(currTile);
                                        }
                                    }
                                }
                                continue; //skip next processing, since detail was already placed.
                            }
                        }
                    }

                    if (Random.Range(0, 100) < 3) {
                        detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.GetFlowerTile(region));
                        Assert.IsNotNull(currTile.structure);
                        ConvertDetailToTileObject(currTile);
                    } else if (Random.Range(0, 100) < 4) {
                        detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.GetRockTile(region));
                        Assert.IsNotNull(currTile.structure);
                        ConvertDetailToTileObject(currTile);
                    } else if (Random.Range(0, 100) < 3) {
                        detailsTilemap.SetTile(currTile.localPlace, InnerMapManager.Instance.assetManager.GetGarbTile(region));
                        Assert.IsNotNull(currTile.structure);
                        ConvertDetailToTileObject(currTile);
                    }
                }
                
                batchCount++;
                if (batchCount == MapGenerationData.InnerMapDetailBatches) {
                    batchCount = 0;
                    yield return null;    
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
            if (activeCharacter != null && activeCharacter.currentRegion == region.coreTile.region
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
            if (locationGridTileCollections != null) {
                for (int i = 0; i < locationGridTileCollections.GetUpperBound(0); i++) {
                    for (int j = 0; j < locationGridTileCollections.GetUpperBound(1); j++) {
                        LocationGridTileCollection collection = locationGridTileCollections[i, j];
                        collection?.CleanUp();
                    }
                }
                locationGridTileCollections = null;    
            }
            
            
            // UtilityScripts.Utilities.DestroyChildren(objectsParent);
        }
        #endregion
        
    }
}
