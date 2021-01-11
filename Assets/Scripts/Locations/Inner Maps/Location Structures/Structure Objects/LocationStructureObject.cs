using EZObjectPools;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class LocationStructureObject : PooledObject {

    public enum Structure_Visual_Mode { Blueprint, Built }

    public STRUCTURE_TYPE structureType;
    
    [Header("Tilemaps")]
    [SerializeField] protected Tilemap _groundTileMap;
    [SerializeField] protected Tilemap _detailTileMap;
    [SerializeField] protected TilemapRenderer _groundTileMapRenderer;
    [SerializeField] protected TilemapRenderer _detailTileMapRenderer;
    [SerializeField] protected Tilemap _blockWallsTilemap;

    [Header("Template Data")]
    [SerializeField] private Vector2Int _size;
    [SerializeField] private Vector3Int _center;
    [SerializeField] private int _craftCost = 50;
    [SerializeField] private int _repairCost = 5;

    [Header("Objects")]
    [FormerlySerializedAs("_objectsParent")] public Transform objectsParent;
    
    [Header("Walls")] 
    [Tooltip("This is only relevant if blockWallsTilemap is not null.")]
    [FormerlySerializedAs("_wallType")][SerializeField] private WALL_TYPE _blockWallType;
    [Tooltip("This is only relevant if structure uses thin walls.")]
    [FormerlySerializedAs("_wallResource")][SerializeField] private RESOURCE _thinWallResource;
    
    [Header("Helpers")]
    [FormerlySerializedAs("predeterminedOccupiedCoordinates")][SerializeField] private List<Vector3Int> _predeterminedOccupiedCoordinates;

    [Header("Connectors")] 
    [SerializeField] private StructureConnector[] _connectors; 
    
    [FormerlySerializedAs("rooms")] [Header("Rooms")] 
    public RoomTemplate[] roomTemplates; //if this is null then it means that this structure object has no rooms.

    public bool wallsContributeToDamage = true;
    private StructureTemplate _parentTemplate;
    private StructureTemplateObjectData[] _preplacedObjs;
    private int _totalBlockWallsCount;

    #region Properties
    private Tilemap[] allTilemaps;
    private WallVisual[] wallVisuals;
    public LocationGridTile[] tiles { get; private set; }
    #endregion

    #region Getters
    public Vector2Int size => _size;
    public Vector3Int center => _center;
    public StructureConnector[] connectors => _connectors;
    public List<Vector3Int> localOccupiedCoordinates {
        get {
            if (_predeterminedOccupiedCoordinates.Count == 0) {
                DetermineOccupiedTileCoordinates();
            }
            return _predeterminedOccupiedCoordinates;
        }
    }
    public RESOURCE thinWallResource => _thinWallResource;
    public int craftCost => _craftCost;
    public int repairCost => _repairCost;
    #endregion

    #region Monobehaviours
    void Awake() {
        allTilemaps = transform.GetComponentsInChildren<Tilemap>();
        wallVisuals = transform.GetComponentsInChildren<WallVisual>();
        _parentTemplate = GetComponentInParent<StructureTemplate>();
        DetermineOccupiedTileCoordinates();
    }
    #endregion

    #region Tile Maps
    public void RefreshAllTilemaps() {
        for (int i = 0; i < allTilemaps.Length; i++) {
            allTilemaps[i].RefreshAllTiles();
        }
    }
    private void UpdateSortingOrders() {
        _groundTileMapRenderer.sortingOrder = InnerMapManager.GroundTilemapSortingOrder + 5;
        _detailTileMapRenderer.sortingOrder = InnerMapManager.DetailsTilemapSortingOrder;
        for (int i = 0; i < wallVisuals.Length; i++) {
            WallVisual wallVisual = wallVisuals[i];
            wallVisual.UpdateSortingOrders(_groundTileMapRenderer.sortingOrder + 2);
        }
    }
    private void SetStructureColor(Color color) {
        for (int i = 0; i < allTilemaps.Length; i++) {
            allTilemaps[i].color = color;
        }
        for (int i = 0; i < wallVisuals.Length; i++) {
            WallVisual wallVisual = wallVisuals[i];
            wallVisual.SetWallColor(color);
        }
    }
    #endregion

    #region Tiles
    public void SetTilesInStructure(LocationGridTile[] tiles) {
        this.tiles = tiles;
    }
    /// <summary>
    /// Determine the local points that this structure object occupies.
    /// This is computed on startup. NOTE: If predeterminedOccupiedCoordinates has preset values, then this will have no effect.
    /// </summary>
    private void DetermineOccupiedTileCoordinates() {
        if (_predeterminedOccupiedCoordinates.Count == 0) {
            BoundsInt bounds = _groundTileMap.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++) {
                for (int y = bounds.yMin; y < bounds.yMax; y++) {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tb = _groundTileMap.GetTile(pos);
                    if (tb != null) {
                        _predeterminedOccupiedCoordinates.Add(pos);
                    }
                }
            }
        }
    }
    #endregion

    #region Tile Objects
    private void RegisterPreplacedObjectsOfType(LocationStructure structure, InnerTileMap innerMap, params TILE_OBJECT_TYPE[] validTileObjectTypes) {
        StructureTemplateObjectData[] preplacedObjs = GetPreplacedObjects();
        for (int i = 0; i < preplacedObjs.Length; i++) {
            StructureTemplateObjectData preplacedObj = preplacedObjs[i];
            if (validTileObjectTypes.Contains(preplacedObj.tileObjectType)) {
                Vector3Int tileCoords = innerMap.groundTilemap.WorldToCell(preplacedObj.transform.position);
                LocationGridTile tile = innerMap.map[tileCoords.x, tileCoords.y];
                TileObject newTileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(preplacedObj.tileObjectType);
                newTileObject.SetIsPreplaced(true);
            
                PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);    
            }
        }
        SetPreplacedObjectsState(false);
    }
    private void RegisterPreplacedObjects(LocationStructure structure, InnerTileMap innerMap) {
        StructureTemplateObjectData[] preplacedObjs = GetPreplacedObjects();
        for (int i = 0; i < preplacedObjs.Length; i++) {
            StructureTemplateObjectData preplacedObj = preplacedObjs[i];
            Vector3Int tileCoords = innerMap.groundTilemap.WorldToCell(preplacedObj.transform.position);
            LocationGridTile tile = innerMap.map[tileCoords.x, tileCoords.y];
            if (tile.objHere != null) {
                if (tile.objHere.traitContainer.HasTrait("Indestructible")) {
                    //skip placement if current object there is indestructible
                    continue;
                } else {
                    tile.structure.RemovePOI(tile.objHere);    
                }
            }
            TileObject newTileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(preplacedObj.tileObjectType);
            newTileObject.SetIsPreplaced(true);
            
            PreplacedObjectProcessing(preplacedObj, tile, structure, newTileObject);
        }
        SetPreplacedObjectsState(false);
    }
    protected virtual void PreplacedObjectProcessing(StructureTemplateObjectData preplacedObj,
        LocationGridTile tile, LocationStructure structure, TileObject newTileObject) {
        tile.structure.AddPOI(newTileObject, tile);
        newTileObject.mapVisual.SetVisual(preplacedObj.spriteRenderer.sprite);
        newTileObject.mapVisual.SetRotation(preplacedObj.transform.localEulerAngles.z);
        newTileObject.RevalidateTileObjectSlots();
    }
    public void PlacePreplacedObjectsAsBlueprints(LocationStructure structure, InnerTileMap areaMap, NPCSettlement npcSettlement) {
        StructureTemplateObjectData[] preplacedObjs = GetPreplacedObjects();
        for (int i = 0; i < preplacedObjs.Length; i++) {
            StructureTemplateObjectData preplacedObj = preplacedObjs[i];
            Vector3Int tileCoords = areaMap.groundTilemap.WorldToCell(preplacedObj.transform.position);
            LocationGridTile tile = areaMap.map[tileCoords.x, tileCoords.y];

            TileObject newTileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(preplacedObj.tileObjectType);
            structure.AddPOI(newTileObject, tile);
            newTileObject.mapVisual.SetVisual(preplacedObj.spriteRenderer.sprite);
            newTileObject.mapVisual.SetRotation(preplacedObj.transform.localEulerAngles.z);
            newTileObject.RevalidateTileObjectSlots();
            newTileObject.SetIsPreplaced(true);
            
            if (!newTileObject.tileObjectType.IsPreBuilt()) { //non-prebuilt items should create a craft job targeting themselves
                newTileObject.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, newTileObject, npcSettlement);
                UtilityScripts.JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(npcSettlement, job, INTERACTION_TYPE.TAKE_RESOURCE);
                job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TileObjectDB.GetTileObjectData(newTileObject.tileObjectType).mainRecipe });
                npcSettlement.AddToAvailableJobs(job);    
            }
            
        }
    }
    private StructureTemplateObjectData GetStructureTemplateObjectData(LocationGridTile tile, InnerTileMap areaMap) {
        StructureTemplateObjectData[] preplacedObjs = GetPreplacedObjects();
        for (int i = 0; i < preplacedObjs.Length; i++) {
            StructureTemplateObjectData preplacedObj = preplacedObjs[i];
            Vector3Int tileCoords = areaMap.groundTilemap.WorldToCell(preplacedObj.transform.position);
            if (tileCoords.x == tile.localPlace.x && tileCoords.y == tile.localPlace.y) {
                return preplacedObj;
            }
        }
        return null;
    }
    private StructureTemplateObjectData[] GetPreplacedObjects() {
        if (objectsParent != null) {
            if (_preplacedObjs == null) {
                _preplacedObjs = UtilityScripts.GameUtilities.GetComponentsInDirectChildren<StructureTemplateObjectData>(objectsParent.gameObject);
            }
            return _preplacedObjs;
        }
        return null;
    }
    public bool HasPreplacedObjectOfType(TILE_OBJECT_TYPE p_tileObjectType) {
        StructureTemplateObjectData[] prePlacedObjects = GetPreplacedObjects();
        if (prePlacedObjects != null) {
            for (int i = 0; i < prePlacedObjects.Length; i++) {
                StructureTemplateObjectData prePlaced = prePlacedObjects[i];
                if (prePlaced.tileObjectType == p_tileObjectType) {
                    return true;
                }
            }
        }
        return false;
    }
    internal void ReceiveMapObject<T>(MapObjectVisual<T> mapGameObject) where T : IDamageable {
        mapGameObject.transform.SetParent(objectsParent);
    }
    private void SetPreplacedObjectsState(bool state) {
        StructureTemplateObjectData[] preplacedObjs = GetPreplacedObjects();
        if (preplacedObjs != null) {
            for (int i = 0; i < preplacedObjs.Length; i++) {
               preplacedObjs[i].gameObject.SetActive(state);
            }
        }
    }
    public void ClearOutUnimportantObjectsBeforePlacement() {
        bool isDemonicStructure = structureType.GetLandmarkType().IsPlayerLandmark();
        for (int i = 0; i < tiles.Length; i++) {
            LocationGridTile tile = tiles[i];
            StructureTemplateObjectData preplacedObj = GetStructureTemplateObjectData(tile, tile.parentMap);
            if (tile.objHere != null && tile.objHere is TileObject tileObject && (tileObject is StructureTileObject) == false) { //TODO: Remove tight coupling with Build Spot Tile object
                if (tileObject.traitContainer.HasTrait("Indestructible")) {
                    tile.structure.OnlyRemovePOIFromList(tileObject);
                } else {
                    if (isDemonicStructure && tile.objHere is Tombstone tombstone) {
                        tombstone.SetRespawnCorpseOnDestroy(false);
                    }
                    bool hasBlockWall = _blockWallsTilemap == null ? false : _blockWallsTilemap.GetTile(_blockWallsTilemap.WorldToCell(tile.worldLocation));
                    if (!tileObject.tileObjectType.IsTileObjectImportant() || preplacedObj != null || hasBlockWall || structureType == STRUCTURE_TYPE.THE_PORTAL) {
                        tile.structure.RemovePOI(tile.objHere);    
                    }    
                }
                
            }
            
            tile.parentMap.detailsTilemap.SetTile(tile.localPlace, null);
            tile.parentMap.northEdgeTilemap.SetTile(tile.localPlace, null);
            tile.parentMap.southEdgeTilemap.SetTile(tile.localPlace, null);
            tile.parentMap.eastEdgeTilemap.SetTile(tile.localPlace, null);
            tile.parentMap.westEdgeTilemap.SetTile(tile.localPlace, null);

            //clear out any details and objects on tiles adjacent to the built structure
            List<LocationGridTile> differentStructureTiles = tile.neighbourList.Where(x => !tiles.Contains(x)).ToList();
            for (int j = 0; j < differentStructureTiles.Count; j++) {
                LocationGridTile diffTile = differentStructureTiles[j];
                if (diffTile.objHere != null && (diffTile.objHere is StructureTileObject) == false) { //TODO: Remove tight coupling with Build Spot Tile object
                    if (diffTile.objHere.traitContainer.HasTrait("Indestructible")) {
                        diffTile.structure.OnlyRemovePOIFromList(diffTile.objHere);
                    } else {
                        if (isDemonicStructure && diffTile.objHere is Tombstone tombstone) {
                            tombstone.SetRespawnCorpseOnDestroy(false);
                        }
                        diffTile.structure.RemovePOI(diffTile.objHere);    
                    }
                    
                }
                diffTile.parentMap.detailsTilemap.SetTile(diffTile.localPlace, null);

                GridNeighbourDirection dir;
                if (diffTile.TryGetNeighbourDirection(tile, out dir)) {
                    switch (dir) {
                        case GridNeighbourDirection.North:
                            diffTile.parentMap.northEdgeTilemap.SetTile(diffTile.localPlace, null);
                            break;
                        case GridNeighbourDirection.South:
                            diffTile.parentMap.southEdgeTilemap.SetTile(diffTile.localPlace, null);
                            break;
                        case GridNeighbourDirection.West:
                            diffTile.parentMap.westEdgeTilemap.SetTile(diffTile.localPlace, null);
                            break;
                        case GridNeighbourDirection.East:
                            diffTile.parentMap.eastEdgeTilemap.SetTile(diffTile.localPlace, null);
                            break;
                        default:
                            break;
                    }
                }

            }
        }
    }
    #endregion

    #region Events
    /// <summary>
    /// Actions to do when a BUILT structure object has been placed.
    /// </summary>
    /// <param name="innerMap">The map where the structure was placed.</param>
    /// <param name="structure">The structure that was placed.</param>
    /// <param name="buildAllTileObjects">Should all preplaced objects be built</param>
    public void OnBuiltStructureObjectPlaced(InnerTileMap innerMap, LocationStructure structure, out int createdWalls, out int totalWalls, bool buildAllTileObjects = true) {
        // bool isDemonicStructure = structure is DemonicStructure;
        for (int i = 0; i < tiles.Length; i++) {
            LocationGridTile tile = tiles[i];
            //set the ground asset of the parent npcSettlement map to what this objects ground map uses, then clear this objects ground map
            ApplyGroundTileAssetForTile(tile);
            tile.CreateSeamlessEdgesForTile(innerMap);
            tile.parentMap.detailsTilemap.SetTile(tile.localPlace, null);
        }
        ProcessConnectors(structure);
        RegisterWalls(innerMap, structure, out createdWalls, out totalWalls);
        _groundTileMap.gameObject.SetActive(false);
        if (buildAllTileObjects) {
            RegisterPreplacedObjects(structure, innerMap);    
        } else {
            if (structure.settlementLocation is NPCSettlement npcSettlement) {
                PlacePreplacedObjectsAsBlueprints(structure, innerMap, npcSettlement);    
            }
        }
        
        RescanPathfindingGridOfStructure(innerMap);
        UpdateSortingOrders();
        Messenger.Broadcast(StructureSignals.STRUCTURE_OBJECT_PLACED, structure);
    }
    public void OnLoadStructureObjectPlaced(InnerTileMap innerMap, LocationStructure structure, SaveDataLocationStructure saveData) {
        if (structure is ManMadeStructure && structure.structureType != STRUCTURE_TYPE.RUINED_ZOO) {
            //Only register walls if structure is a man made structure, this is because demonic structures and the ruined zoo uses the loaded block wall objects. 
            RegisterWalls(innerMap, structure, out int createdWalls, out int totalWalls);
        }
        if (saveData is SaveDataManMadeStructure saveDataManMadeStructure && saveDataManMadeStructure.structureConnectors != null) {
            LoadConnectors(saveDataManMadeStructure.structureConnectors, innerMap);    
        }
        _groundTileMap.gameObject.SetActive(false);
        if (_blockWallsTilemap != null) {
            _blockWallsTilemap.gameObject.SetActive(false);    
        }
        RescanPathfindingGridOfStructure(innerMap);
        UpdateSortingOrders();
        SetPreplacedObjectsState(false);
        Messenger.Broadcast(StructureSignals.STRUCTURE_OBJECT_PLACED, structure);
    }
    public void OnOwnerStructureDestroyed(InnerTileMap innerTileMap) {
        RescanPathfindingGridOfStructure(innerTileMap, 0);
        gameObject.SetActive(false); //disable this object.
        _parentTemplate.CheckForDestroy(); //check if whole structure template has been destroyed.
    }
    public void OnRepairStructure(InnerTileMap innerMap, LocationStructure structure, out int createdWalls, out int totalWalls) {
        RepairWallsAndFloors(innerMap, structure, out createdWalls, out totalWalls);
        RegisterPreplacedObjects(structure, innerMap);

        RescanPathfindingGridOfStructure(innerMap);
        UpdateSortingOrders();
    }
    #endregion

    #region Inquiry
    public List<LocationGridTile> GetTilesOccupiedByStructure(InnerTileMap map) {
        List<LocationGridTile> occupiedTiles = new List<LocationGridTile>();
        // BoundsInt bounds = _groundTileMap.cellBounds;

        var localPosition = map.transform.InverseTransformPoint(transform.position);
        Vector3Int actualLocation = new Vector3Int(Mathf.FloorToInt(localPosition.x), Mathf.FloorToInt(localPosition.y), 0);
        for (int i = 0; i < localOccupiedCoordinates.Count; i++) {
            Vector3Int currCoordinate = localOccupiedCoordinates[i];

            Vector3Int gridTileLocation = actualLocation;

            //get difference from center
            int xDiffFromCenter = currCoordinate.x - center.x;
            int yDiffFromCenter = currCoordinate.y - center.y;
            gridTileLocation.x += xDiffFromCenter;
            gridTileLocation.y += yDiffFromCenter;

            if (UtilityScripts.Utilities.IsInRange(gridTileLocation.x, 0, map.width) 
                && UtilityScripts.Utilities.IsInRange(gridTileLocation.y, 0, map.height)) {
                LocationGridTile tile = map.map[gridTileLocation.x, gridTileLocation.y];
                occupiedTiles.Add(tile);    
            } else {
                throw new Exception($"IndexOutOfRangeException when trying to place structure object {name} at {map.region.name}");
            }
        }
        return occupiedTiles;
    }
    protected LocationGridTile ConvertLocalPointInStructureToTile(Vector3Int coordinates, InnerTileMap map) {
        var localPosition = map.transform.InverseTransformPoint(transform.position);
        Vector3Int actualLocation = new Vector3Int(Mathf.FloorToInt(localPosition.x), Mathf.FloorToInt(localPosition.y), 0);
        Vector3Int gridTileLocation = actualLocation;

        //get difference from center
        int xDiffFromCenter = coordinates.x - center.x;
        int yDiffFromCenter = coordinates.y - center.y;
        gridTileLocation.x += xDiffFromCenter;
        gridTileLocation.y += yDiffFromCenter;
        
        return map.map[gridTileLocation.x, gridTileLocation.y];
    }
    public List<LocationGridTile> GetTilesOccupiedByRoom(InnerTileMap map, RoomTemplate roomTemplate) {
        List<LocationGridTile> occupiedTiles = new List<LocationGridTile>();
        List<Vector3Int> occupiedCoordinates = new List<Vector3Int>(roomTemplate.coordinatesInRoom);

        var localPosition = map.transform.InverseTransformPoint(transform.position);
        Vector3Int actualLocation = new Vector3Int(Mathf.FloorToInt(localPosition.x), Mathf.FloorToInt(localPosition.y), 0);
        for (int i = 0; i < occupiedCoordinates.Count; i++) {
            Vector3Int currCoordinate = occupiedCoordinates[i];

            Vector3Int gridTileLocation = actualLocation;

            //get difference from center
            int xDiffFromCenter = currCoordinate.x - center.x;
            int yDiffFromCenter = currCoordinate.y - center.y;
            gridTileLocation.x += xDiffFromCenter;
            gridTileLocation.y += yDiffFromCenter;

            if (UtilityScripts.Utilities.IsInRange(gridTileLocation.x, 0, map.width) 
                && UtilityScripts.Utilities.IsInRange(gridTileLocation.y, 0, map.height)) {
                LocationGridTile tile = map.map[gridTileLocation.x, gridTileLocation.y];
                occupiedTiles.Add(tile);    
            } else {
                throw new Exception($"IndexOutOfRangeException when trying to place structure object {name} at {map.region.name}");
            }
        }
        return occupiedTiles;
    }
    #endregion

    #region Visuals
    public void SetVisualMode(Structure_Visual_Mode mode, InnerTileMap innerTileMap) {
        Color color = Color.white;
        switch (mode) {
            case Structure_Visual_Mode.Blueprint:
                color.a = 128f / 255f;
                SetStructureColor(color);
                SetPreplacedObjectsState(false);
                SetWallCollidersState(false);
                break;
            default:
                color = Color.white;
                SetStructureColor(color);
                SetWallCollidersState(true);
                RescanPathfindingGridOfStructure(innerTileMap);
                break;
        }
    }
    public void ApplyGroundTileAssetForTile(LocationGridTile tile) {
        TileBase tileBase = GetGroundTileAssetForTile(tile);
        if (tileBase != null) {
            tile.SetGroundTilemapVisual(tileBase);    
        }
    }
    private TileBase GetGroundTileAssetForTile(LocationGridTile tile) {
        return _groundTileMap.GetTile(_groundTileMap.WorldToCell(tile.worldLocation));
    }
    #endregion

    #region Object Pool
    public override void Reset() {
        base.Reset();
        SetPreplacedObjectsState(true);
        if (_groundTileMap != null) {
            _groundTileMap.gameObject.SetActive(true);    
        }
        if (_blockWallsTilemap != null) {
            _blockWallsTilemap.gameObject.SetActive(true);    
        }
        SetWallCollidersState(true);
        tiles = null;
        _preplacedObjs = null;
        for (int i = 0; i < connectors.Length; i++) {
            connectors[i].Reset();
        }
        for (int i = 0; i < wallVisuals.Length; i++) {
            WallVisual wallVisual = wallVisuals[i];
            wallVisual.ResetWallAssets(_thinWallResource);
            wallVisual.Reset();
        }    
    }
    #endregion

    #region Walls
    /// <summary>
    /// Create necessary objects for this structure objects walls.
    /// This takes into account what type of wall this structure object has (Block walls or Thin structure walls)
    /// </summary>
    /// <param name="map">The inner tile map this structure is part of.</param>
    /// <param name="structure">The structure instance this object is connected to.</param>
    private void RegisterWalls(InnerTileMap map, LocationStructure structure, out int createdWalls, out int totalWalls) {
        createdWalls = 0;
        totalWalls = 0;
        if (_blockWallsTilemap != null) {
            _blockWallsTilemap.gameObject.SetActive(true);
            for (int i = 0; i < tiles.Length; i++) {
                LocationGridTile tile = tiles[i];
                //block walls
                TileBase blockWallAsset = _blockWallsTilemap.GetTile(_blockWallsTilemap.WorldToCell(tile.worldLocation));
                if (blockWallAsset != null) {
                    if (blockWallAsset.name.Contains("Wall")) {
                        bool shouldBuildBlockWall = true;
                        if (tile.objHere != null) {
                            if (tile.objHere.traitContainer.HasTrait("Indestructible")) {
                                shouldBuildBlockWall = false;
                                tile.structure.OnlyAddPOIToList(tile.objHere);
                            } else {
                                tile.structure.RemovePOI(tile.objHere);    
                            }
                        }
                        if (shouldBuildBlockWall) {
                            createdWalls++;
                            BlockWall blockWall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
                            blockWall.SetWallType(_blockWallType);
                            structure.AddPOI(blockWall, tile);
                            if (wallsContributeToDamage) {
                                structure.AddObjectAsDamageContributor(blockWall);    
                            }
                        }
                        totalWalls++;
                    } else {
                        map.structureTilemap.SetTile(tile.localPlace, blockWallAsset);
                    }
                }
            }
            //disable block walls tilemap
            _blockWallsTilemap.gameObject.SetActive(false);
        } else if (wallVisuals != null && wallVisuals.Length > 0 && structure is ManMadeStructure manMadeStructure) {
            //structure walls
            List<StructureWallObject> wallObjects = new List<StructureWallObject>();
            for (int i = 0; i < wallVisuals.Length; i++) {
                WallVisual wallVisual = wallVisuals[i];
                StructureWallObject structureWallObject = new StructureWallObject(structure, wallVisual, _thinWallResource);
                Vector3Int tileLocation = map.groundTilemap.WorldToCell(wallVisual.transform.position);
                LocationGridTile tile = map.map[tileLocation.x, tileLocation.y];
                tile.SetTileType(LocationGridTile.Tile_Type.Wall);
                structureWallObject.SetGridTileLocation(tile);
                tile.AddWallObject(structureWallObject);
                createdWalls++;
                totalWalls++;
                if (wallsContributeToDamage) {
                    structure.AddObjectAsDamageContributor(structureWallObject);    
                }
                wallObjects.Add(structureWallObject);
            }    
            manMadeStructure.SetWallObjects(wallObjects, _thinWallResource);
        }
    }
    private void RepairWallsAndFloors(InnerTileMap map, LocationStructure structure, out int createdWalls, out int totalWalls) {
        createdWalls = 0;
        totalWalls = 0;
        if (_blockWallsTilemap != null) {
            _blockWallsTilemap.gameObject.SetActive(true);
            for (int i = 0; i < tiles.Length; i++) {
                LocationGridTile tile = tiles[i];

                ApplyGroundTileAssetForTile(tile);
                tile.CreateSeamlessEdgesForTile(map);
                tile.parentMap.detailsTilemap.SetTile(tile.localPlace, null);

                //block walls
                TileBase blockWallAsset = _blockWallsTilemap.GetTile(_blockWallsTilemap.WorldToCell(tile.worldLocation));
                if (blockWallAsset != null) {
                    if (blockWallAsset.name.Contains("Wall")) {
                        bool shouldBuildBlockWall = true;
                        if (tile.objHere != null) {
                            if (tile.objHere.traitContainer.HasTrait("Indestructible")) {
                                shouldBuildBlockWall = false;
                                tile.structure.OnlyAddPOIToList(tile.objHere);
                            } else {
                                tile.structure.RemovePOI(tile.objHere);    
                            }
                        }

                        if (shouldBuildBlockWall) {
                            createdWalls++;
                            BlockWall blockWall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
                            blockWall.SetWallType(_blockWallType);
                            structure.AddPOI(blockWall, tile);
                            if (wallsContributeToDamage) {
                                structure.AddObjectAsDamageContributor(blockWall);
                            }
                        }
                        totalWalls++;
                    } else {
                        map.structureTilemap.SetTile(tile.localPlace, blockWallAsset);
                    }
                }
            }
            //disable block walls tilemap
            _blockWallsTilemap.gameObject.SetActive(false);
        } else {
            if (wallVisuals != null && wallVisuals.Length > 0 && structure is ManMadeStructure manMadeStructure) {
                for (int i = 0; i < tiles.Length; i++) {
                    LocationGridTile tile = tiles[i];

                    ApplyGroundTileAssetForTile(tile);
                    tile.CreateSeamlessEdgesForTile(map);
                    tile.parentMap.detailsTilemap.SetTile(tile.localPlace, null);
                }
                //structure walls
                List<StructureWallObject> wallObjects = new List<StructureWallObject>();
                for (int i = 0; i < wallVisuals.Length; i++) {
                    WallVisual wallVisual = wallVisuals[i];
                    StructureWallObject structureWallObject = new StructureWallObject(structure, wallVisual, _thinWallResource);
                    Vector3Int tileLocation = map.groundTilemap.WorldToCell(wallVisual.transform.position);
                    LocationGridTile tile = map.map[tileLocation.x, tileLocation.y];
                    tile.SetTileType(LocationGridTile.Tile_Type.Wall);
                    structureWallObject.SetGridTileLocation(tile);
                    tile.AddWallObject(structureWallObject);
                    createdWalls++;
                    totalWalls++;
                    if (wallsContributeToDamage) {
                        structure.AddObjectAsDamageContributor(structureWallObject);
                    }
                    wallObjects.Add(structureWallObject);
                }
                manMadeStructure.SetWallObjects(wallObjects, _thinWallResource);
            }
        }
        
    }
    private void SetWallCollidersState(bool state) {
        for (int i = 0; i < wallVisuals.Length; i++) {
            WallVisual wallVisual = wallVisuals[i];
            wallVisual.SetUnpassableColliderState(state);
        }
    }
    #endregion

    #region Pathfinding
    [ContextMenu("Rescan Pathfinding Grid Of Structure")]
    public void RescanPathfindingGridOfStructure(InnerTileMap innerTileMap, int tag = 1) {
        GraphUpdateObject guo = new GraphUpdateObject(_groundTileMapRenderer.bounds) {nnConstraint = innerTileMap.onlyUnwalkableGraph};
        PathfindingManager.Instance.UpdatePathfindingGraphPartialCoroutine(guo);

        guo = new TagGraphUpdateObject(_groundTileMapRenderer.bounds) {nnConstraint = innerTileMap.onlyPathfindingGraph, updatePhysics = true, modifyWalkability = false};
        PathfindingManager.Instance.UpdatePathfindingGraphPartialCoroutine(guo);
    }
    #endregion

    #region Interaction
    public void OnPointerClick(BaseEventData data) {
        Debug.Log($"Player clicked {name}");
    }
    #endregion

    #region Connectors
    /// <summary>
    /// Get first valid connector given a list of choices.
    /// NOTE: This assumes that all connectors owned by this is still open. In other words, this is used to initially place a structure object.
    /// </summary>
    /// <param name="connectionChoices">A list of all OPEN connector choices.</param>
    /// <param name="innerTileMap">The inner map that this structure will be placed on.</param>
    /// <param name="usedConnectorIndex">The index of the connector that was used by this structure object.</param>
    /// <returns>The first valid connector from the list of choices.</returns>
    public StructureConnector GetFirstValidConnector(List<StructureConnector> connectionChoices, InnerTileMap innerTileMap, out int usedConnectorIndex, out LocationGridTile tileToPlaceStructure) {
        //loop through connection choices
        for (int i = 0; i < connectionChoices.Count; i++) {
            StructureConnector connectorA = connectionChoices[i];
            LocationGridTile connectorATileLocation = connectorA.GetLocationGridTileGivenCurrentPosition(innerTileMap);
            if (connectorATileLocation == null) {
                continue;
            }
            //for each choice check each connector that I own, and check if that connector can connect to the other connector
            for (int j = 0; j < connectors.Length; j++) {
                //To check if connectorA and connectorB can connect, get the center tile that this object will occupy given the location of connectorA
                //and from that, get the tiles, that this object will occupy if placed on the computed center tile.
                //If it will occupy a tile that is NOT part of the Wilderness, then connectorA is invalid.
                StructureConnector connectorB = connectors[j];
                var connectorBLocalPos = connectorB.transform.localPosition;
                Vector2Int connectorBLocalPosition = new Vector2Int(Mathf.FloorToInt(connectorBLocalPos.x), Mathf.FloorToInt(connectorBLocalPos.y));
                Vector2Int distanceFromCenter = new Vector2Int(center.x - connectorBLocalPosition.x, center.y - connectorBLocalPosition.y);
                Vector2Int computedCenterLocation = new Vector2Int(connectorATileLocation.localPlace.x + distanceFromCenter.x, connectorATileLocation.localPlace.y + distanceFromCenter.y);
                
                LocationGridTile centerTile = innerTileMap.GetTileFromMapCoordinates(computedCenterLocation.x, computedCenterLocation.y);
                if (centerTile != null && HasEnoughSpaceIfPlacedOn(centerTile)) {
                    tileToPlaceStructure = centerTile;
                    usedConnectorIndex = j;
                    return connectorA;
                }
            }
        }
        tileToPlaceStructure = null;
        usedConnectorIndex = -1;
        return null;
    }
    public bool HasEnoughSpaceIfPlacedOn(LocationGridTile centerTile) {
        if (!CanPlaceStructureOnTile(centerTile)) {
            return false;
        }
        InnerTileMap map = centerTile.parentMap;
        for (int i = 0; i < localOccupiedCoordinates.Count; i++) {
            Vector3Int currCoordinate = localOccupiedCoordinates[i];

            Vector3Int gridTileLocation = centerTile.localPlace;

            //get difference from center
            int xDiffFromCenter = currCoordinate.x - center.x;
            int yDiffFromCenter = currCoordinate.y - center.y;
            gridTileLocation.x += xDiffFromCenter;
            gridTileLocation.y += yDiffFromCenter;

            if (UtilityScripts.Utilities.IsInRange(gridTileLocation.x, 0, map.width) 
                && UtilityScripts.Utilities.IsInRange(gridTileLocation.y, 0, map.height)) {
                LocationGridTile tile = map.map[gridTileLocation.x, gridTileLocation.y];
                if (!CanPlaceStructureOnTile(tile)) {
                    return false;
                }
            } else {
                return false; //returned coordinates are out of the map
            }
        }
        return true;
    }
    private bool CanPlaceStructureOnTile(LocationGridTile tile) {
        if (tile.structure.structureType != STRUCTURE_TYPE.WILDERNESS) {
            return false; //if calculated tile that will be occupied, is not part of wilderness, then this structure object cannot be placed on given center.
        }
        if (tile.hasBlueprint) {
            return false; //This is to prevent overlapping blueprints. If any tile that will be occupied by this has a blueprint, then do not allow
        }
        if (tile.IsAtEdgeOfMap()) {
            return false;
        }
        if (tile.collectionOwner.isPartOfParentRegionMap) {
            HexTile hexTileOwner = tile.collectionOwner.partOfHextile.hexTileOwner;
            if (hexTileOwner.elevationType == ELEVATION.WATER || hexTileOwner.elevationType == ELEVATION.MOUNTAIN) {
                return false;
            }
            if (hexTileOwner.landmarkOnTile != null && hexTileOwner.landmarkOnTile.specificLandmarkType.GetStructureType().IsSpecialStructure()) {
                return false;
            }
        } else {
            return false; //prevent structure placement on tiles that aren't linked to any hextile. This is to prevent errors when trying to check if a tile IsPartOfSettlement 
        }
        //limit so that structures will not be directly adjacent with each other
        for (int j = 0; j < tile.neighbourList.Count; j++) {
            LocationGridTile neighbour = tile.neighbourList[j];
            if (neighbour.hasBlueprint) {
                return false; //if bordering tile has a blueprint, then do not allow this structure to be placed. This is to prevent structures from being directly adjacent with each other, while they are still blueprints.
            }
            //only limit adjacency if adjacent tile is not wilderness and not city center (Allow adjacency with city center since it has no walls, and looks better when structures are close to it.)
            if (neighbour.structure.structureType != STRUCTURE_TYPE.WILDERNESS && neighbour.structure.structureType != STRUCTURE_TYPE.CITY_CENTER) {
                return false;
            }
        }
        return true;
    }
    private List<LocationGridTile> GetTilesThatWillBeOccupiedGivenCenter(InnerTileMap map, LocationGridTile centerTile) {
        List<LocationGridTile> occupiedTiles = new List<LocationGridTile>();
        // BoundsInt bounds = _groundTileMap.cellBounds;

        List<Vector3Int> localOccupiedCoordinates = this.localOccupiedCoordinates;
        
        // List<Vector3Int> localOccupiedCoordinates = new List<Vector3Int>();
        // if (predeterminedOccupiedCoordinates != null && predeterminedOccupiedCoordinates.Count > 0) {
        //     localOccupiedCoordinates = predeterminedOccupiedCoordinates;
        // } else {
        //     for (int x = bounds.xMin; x < bounds.xMax; x++) {
        //         for (int y = bounds.yMin; y < bounds.yMax; y++) {
        //             Vector3Int pos = new Vector3Int(x, y, 0);
        //             TileBase tb = _groundTileMap.GetTile(pos);
        //             if (tb != null) {
        //                 localOccupiedCoordinates.Add(pos);
        //             }
        //         }
        //     }    
        // }
        for (int i = 0; i < localOccupiedCoordinates.Count; i++) {
            Vector3Int currCoordinate = localOccupiedCoordinates[i];

            Vector3Int gridTileLocation = centerTile.localPlace;

            //get difference from center
            int xDiffFromCenter = currCoordinate.x - center.x;
            int yDiffFromCenter = currCoordinate.y - center.y;
            gridTileLocation.x += xDiffFromCenter;
            gridTileLocation.y += yDiffFromCenter;

            if (UtilityScripts.Utilities.IsInRange(gridTileLocation.x, 0, map.width) 
                && UtilityScripts.Utilities.IsInRange(gridTileLocation.y, 0, map.height)) {
                LocationGridTile tile = map.map[gridTileLocation.x, gridTileLocation.y];
                occupiedTiles.Add(tile);
            } else {
                throw new Exception($"IndexOutOfRangeException when trying to place structure object {name} at {map.region.name}");
            }
        }
        return occupiedTiles;
    }
    private void ProcessConnectors(LocationStructure structure) {
        for (int i = 0; i < _connectors.Length; i++) {
            StructureConnector connector = _connectors[i];
            connector.OnPlaceConnector(structure.region.innerMap);
        }
    }
    private void LoadConnectors(SaveDataStructureConnector[] connectorSaves, InnerTileMap innerTileMap) {
        Assert.IsTrue(connectorSaves.Length == _connectors.Length, $"Inconsistent connector save data on {name}");
        for (int i = 0; i < _connectors.Length; i++) {
            StructureConnector connector = _connectors[i];
            SaveDataStructureConnector saveData = connectorSaves[i];
            connector.LoadReferences(saveData, innerTileMap);
        }
    }
    #endregion
    
    #region Helpers
    [Header("Wall Converter")]
    [SerializeField] private Tilemap wallTileMap;
    [SerializeField] private GameObject leftWall;
    [SerializeField] private GameObject rightWall;
    [SerializeField] private GameObject topWall;
    [SerializeField] private GameObject bottomWall;
    [SerializeField] private GameObject cornerPrefab;
    [ContextMenu("Convert Walls")]
    public void ConvertWalls() {
        wallTileMap.CompressBounds();
        BoundsInt bounds = wallTileMap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = wallTileMap.GetTile(pos);

                Vector3 worldPos = wallTileMap.CellToWorld(pos);

                if (tile != null) {
                    Vector2 centeredPos = new Vector2(worldPos.x + 0.5f, worldPos.y + 0.5f);
                    if (tile.name.Contains("Door")) {
                        continue; //skip
                    }
                    WallVisual wallVisual = null;
                    if (tile.name.Contains("Left")) {
                        wallVisual = InstantiateWall(leftWall, centeredPos, wallTileMap.transform, _thinWallResource != RESOURCE.WOOD);
                    } 
                    if (tile.name.Contains("Right")) {
                        wallVisual = InstantiateWall(rightWall, centeredPos, wallTileMap.transform, _thinWallResource != RESOURCE.WOOD);
                    }
                    if (tile.name.Contains("Bot")) {
                        wallVisual = InstantiateWall(bottomWall, centeredPos, wallTileMap.transform, _thinWallResource != RESOURCE.WOOD);
                    }
                    if (tile.name.Contains("Top")) {
                        wallVisual = InstantiateWall(topWall, centeredPos, wallTileMap.transform, _thinWallResource != RESOURCE.WOOD);
                    }

                    if (wallVisual != null) {
                        Vector3 cornerPos = centeredPos;
                        if (tile.name.Contains("BotLeft")) {
                            cornerPos.x -= 0.5f;
                            cornerPos.y -= 0.5f;
                            Instantiate(cornerPrefab, cornerPos, Quaternion.identity, wallVisual.transform);
                        } else if (tile.name.Contains("BotRight")) {
                            cornerPos.x += 0.5f;
                            cornerPos.y -= 0.5f;
                            Instantiate(cornerPrefab, cornerPos, Quaternion.identity, wallVisual.transform);
                        } else if (tile.name.Contains("TopLeft")) {
                            cornerPos.x -= 0.5f;
                            cornerPos.y += 0.5f;
                            Instantiate(cornerPrefab, cornerPos, Quaternion.identity, wallVisual.transform);
                        } else if (tile.name.Contains("TopRight")) {
                            cornerPos.x += 0.5f;
                            cornerPos.y += 0.5f;
                            Instantiate(cornerPrefab, cornerPos, Quaternion.identity, wallVisual.transform);
                        }
                        if (_thinWallResource != RESOURCE.WOOD) {
                            //only update asset if wall resource is not wood.
                            wallVisual.UpdateWallAssets(_thinWallResource);    
                        }    
                    }
                }
            }
        }
    }
    private WallVisual InstantiateWall(GameObject wallPrefab, Vector3 centeredPos, Transform parent, bool updateWallAsset) {
        GameObject wallGO = Instantiate(wallPrefab, parent);
        wallGO.transform.position = centeredPos;
        WallVisual wallVisual = wallGO.GetComponent<WallVisual>();
        if (updateWallAsset) {
            wallVisual.UpdateWallAssets(_thinWallResource);    
        }
        return wallVisual;
    }

    [ContextMenu("Convert Objects")]
    public void ConvertObjects() {
        UtilityScripts.Utilities.DestroyChildren(objectsParent);
        _detailTileMap.CompressBounds();
        // Material mat = Resources.Load<Material>("Fonts & Materials/2D Lighting");
        BoundsInt bounds = _detailTileMap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = _detailTileMap.GetTile(pos);
                Vector3 worldPos = _detailTileMap.CellToWorld(pos);
            
                if (tile != null) {
                    Matrix4x4 m = _detailTileMap.GetTransformMatrix(pos);
                    Vector2 centeredPos = new Vector2(worldPos.x + 0.5f, worldPos.y + 0.5f);
                    
                    GameObject newGo = new GameObject("StructureTemplateObjectData");
                    newGo.layer = LayerMask.NameToLayer("Area Maps");
                    newGo.transform.SetParent(objectsParent);
                    newGo.transform.position = centeredPos;
                    newGo.transform.localRotation = m.rotation;

                    StructureTemplateObjectData stod = newGo.AddComponent<StructureTemplateObjectData>();
                    SpriteRenderer spriteRenderer = newGo.AddComponent<SpriteRenderer>();

                    spriteRenderer.sortingLayerName = "Area Maps";
                    spriteRenderer.sortingOrder = 60;
                    // spriteRenderer.material = mat;
                    
                    int index = tile.name.IndexOf("#", StringComparison.Ordinal);
                    string tileObjectName = tile.name;
                    if (index != -1) {
                        tileObjectName = tile.name.Substring(0, index);    
                    }
                    tileObjectName = tileObjectName.ToUpper();
                    TILE_OBJECT_TYPE tileObjectType = (TILE_OBJECT_TYPE) Enum.Parse(typeof(TILE_OBJECT_TYPE), tileObjectName);
                    stod.tileObjectType = tileObjectType;
                    stod.spriteRenderer = spriteRenderer;
                    spriteRenderer.sprite = _detailTileMap.GetSprite(pos);
                }
            }
        }
        _detailTileMap.enabled = false;
        _detailTileMapRenderer.enabled = false;
    }

    [Header("Predetermine Structure Tiles")] 
    [SerializeField] private TileBase[] assetsToConsiderAsStructure;
    [ContextMenu("Predetermine Structure Tiles")]
    public void PredetermineOccupiedCoordinates() {
        BoundsInt bounds = _groundTileMap.cellBounds;

        _predeterminedOccupiedCoordinates = new List<Vector3Int>();
        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tb = _groundTileMap.GetTile(pos);
                if (tb != null && assetsToConsiderAsStructure.Contains(tb)) {
                    _predeterminedOccupiedCoordinates.Add(pos);
                    // _groundTileMap.SetColor(pos, Color.red);
                }
            }
        }    
        
    }
    #endregion

}

[System.Serializable]
public struct RoomTemplate {
    public Vector3Int[] coordinatesInRoom;
}