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
    [SerializeField] protected Vector2Int _size;
    [SerializeField] protected Vector3Int _center;

    [Header("Objects")]
    [FormerlySerializedAs("_objectsParent")] public Transform objectsParent;

    [Header("Furniture Spots")]
    [SerializeField] protected Transform _furnitureSpotsParent;

    [Header("Walls")] 
    [Tooltip("This is only relevant if blockWallsTilemap is not null.")]
    [SerializeField] private WALL_TYPE _wallType;
    [Tooltip("This is only relevant if structure uses thin walls.")]
    [SerializeField] private RESOURCE _wallResource;
    
    [Header("Helpers")]
    [Tooltip("If this has elements, then only the provided coordinates will be set as part of the actual structure. Otherwise all the tiles inside the ground tilemap will be considered as part of the structure.")]
    [SerializeField] private List<Vector3Int> predeterminedOccupiedCoordinates;

    private StructureTemplate _parentTemplate;

    #region Properties
    private Tilemap[] allTilemaps;
    private WallVisual[] wallVisuals;
    public LocationGridTile[] tiles { get; private set; }
    public StructureWallObject[] walls { get; private set; }
    #endregion

    #region Getters
    public Vector2Int size => _size;
    public Vector3Int center => _center;
    #endregion

    #region Monobehaviours
    void Awake() {
        allTilemaps = transform.GetComponentsInChildren<Tilemap>();
        wallVisuals = transform.GetComponentsInChildren<WallVisual>();
        _parentTemplate = GetComponentInParent<StructureTemplate>();
        // _groundTileMap.CompressBounds();
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
    #endregion

    #region Tile Objects
    public void RegisterPreplacedObjects(LocationStructure structure, InnerTileMap innerMap) {
        StructureTemplateObjectData[] preplacedObjs = GetPreplacedObjects();
        for (int i = 0; i < preplacedObjs.Length; i++) {
            StructureTemplateObjectData preplacedObj = preplacedObjs[i];
            Vector3Int tileCoords = innerMap.groundTilemap.WorldToCell(preplacedObj.transform.position);
            LocationGridTile tile = innerMap.map[tileCoords.x, tileCoords.y];
            tile.SetReservedType(preplacedObj.tileObjectType);

            TileObject newTileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(preplacedObj.tileObjectType);
            newTileObject.SetIsPreplaced(true);
            structure.AddPOI(newTileObject, tile);
            newTileObject.mapVisual.SetVisual(preplacedObj.spriteRenderer.sprite);
            newTileObject.mapVisual.SetRotation(preplacedObj.transform.localEulerAngles.z);
            newTileObject.RevalidateTileObjectSlots();
        }
        SetPreplacedObjectsState(false);
    }
    public void PlacePreplacedObjectsAsBlueprints(LocationStructure structure, InnerTileMap areaMap, NPCSettlement npcSettlement) {
        StructureTemplateObjectData[] preplacedObjs = GetPreplacedObjects();
        for (int i = 0; i < preplacedObjs.Length; i++) {
            StructureTemplateObjectData preplacedObj = preplacedObjs[i];
            Vector3Int tileCoords = areaMap.groundTilemap.WorldToCell(preplacedObj.transform.position);
            LocationGridTile tile = areaMap.map[tileCoords.x, tileCoords.y];
            tile.SetReservedType(preplacedObj.tileObjectType);

            TileObject newTileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(preplacedObj.tileObjectType);
            structure.AddPOI(newTileObject, tile);
            newTileObject.mapVisual.SetVisual(preplacedObj.spriteRenderer.sprite);
            newTileObject.mapVisual.SetRotation(preplacedObj.transform.localEulerAngles.z);
            newTileObject.RevalidateTileObjectSlots();
            
            if (newTileObject.tileObjectType.IsPreBuilt() == false) { //non-prebuilt items should create a craft job targeting themselves
                newTileObject.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, newTileObject, npcSettlement);
                job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TileObjectDB.GetTileObjectData(newTileObject.tileObjectType).constructionCost });
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoCraftFurnitureJob);
                npcSettlement.AddToAvailableJobs(job);    
            }
            
        }
    }
    private StructureTemplateObjectData[] GetPreplacedObjects() {
        if (objectsParent != null) {
            return UtilityScripts.GameUtilities.GetComponentsInDirectChildren<StructureTemplateObjectData>(objectsParent.gameObject);    
        }
        return null;
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
        for (int i = 0; i < tiles.Length; i++) {
            LocationGridTile tile = tiles[i];
            if (tile.objHere != null && (tile.objHere is StructureTileObject) == false) { //TODO: Remove tight coupling with Build Spot Tile object
                tile.structure.RemovePOI(tile.objHere);
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
                    diffTile.structure.RemovePOI(diffTile.objHere);
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

    #region Furniture Spots
    private void RegisterFurnitureSpots(InnerTileMap areaMap) {
        if (_furnitureSpotsParent == null) {
            return;
        }
        FurnitureSpotMono[] spots = GetFurnitureSpots();
        for (int i = 0; i < spots.Length; i++) {
            FurnitureSpotMono spot = spots[i];
            Vector3Int tileCoords = areaMap.groundTilemap.WorldToCell(spot.transform.position);
            LocationGridTile tile = areaMap.map[tileCoords.x, tileCoords.y];
            tile.SetFurnitureSpot(spot.GetFurnitureSpot());
        }
    }
    private FurnitureSpotMono[] GetFurnitureSpots() {
        return UtilityScripts.GameUtilities.GetComponentsInDirectChildren<FurnitureSpotMono>(_furnitureSpotsParent.gameObject);
    }
    #endregion

    #region Events
    /// <summary>
    /// Actions to do when a BUILT structure object has been placed.
    /// </summary>
    /// <param name="innerMap">The map where the structure was placed.</param>
    /// <param name="structure">The structure that was placed.</param>
    public void OnBuiltStructureObjectPlaced(InnerTileMap innerMap, LocationStructure structure) {
        for (int i = 0; i < tiles.Length; i++) {
            LocationGridTile tile = tiles[i];
            //check if the template has details at this tiles location
            tile.hasDetail = _detailTileMap.GetTile(_detailTileMap.WorldToCell(tile.worldLocation)) != null;
            if (tile.hasDetail) { //if it does then set that tile as occupied
                tile.SetTileState(LocationGridTile.Tile_State.Occupied);
            }
            //set the ground asset of the parent npcSettlement map to what this objects ground map uses, then clear this objects ground map
            ApplyGroundTileAssetForTile(tile);
            tile.CreateSeamlessEdgesForTile(innerMap);
            tile.parentMap.detailsTilemap.SetTile(tile.localPlace, null);
        }
        RegisterWalls(innerMap, structure);
        _groundTileMap.gameObject.SetActive(false);
        RegisterFurnitureSpots(innerMap);
        RegisterPreplacedObjects(structure, innerMap);
        RescanPathfindingGridOfStructure();
        if (structure.settlementLocation is NPCSettlement npcSettlement) {
            npcSettlement.OnLocationStructureObjectPlaced(structure);
        } else {
            innerMap.region.OnLocationStructureObjectPlaced(structure);    
        }
        UpdateSortingOrders();
        Messenger.Broadcast(Signals.STRUCTURE_OBJECT_PLACED, structure);
    }
    public void OnOwnerStructureDestroyed() {
        gameObject.SetActive(false); //disable this object.
        _parentTemplate.CheckForDestroy(); //check if whole structure template has been destroyed.
    }
    #endregion

    #region Inquiry
    public List<LocationGridTile> GetTilesOccupiedByStructure(InnerTileMap map) {
        List<LocationGridTile> occupiedTiles = new List<LocationGridTile>();
        BoundsInt bounds = _groundTileMap.cellBounds;

        List<Vector3Int> occupiedCoordinates = new List<Vector3Int>();
        if (predeterminedOccupiedCoordinates != null && predeterminedOccupiedCoordinates.Count > 0) {
            occupiedCoordinates = predeterminedOccupiedCoordinates;
        } else {
            for (int x = bounds.xMin; x < bounds.xMax; x++) {
                for (int y = bounds.yMin; y < bounds.yMax; y++) {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tb = _groundTileMap.GetTile(pos);
                    if (tb != null) {
                        occupiedCoordinates.Add(pos);
                    }
                }
            }    
        }

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
    public bool IsBiggerThanBuildSpot() {
        return _size.x > InnerMapManager.BuildingSpotSize.x || _size.y > InnerMapManager.BuildingSpotSize.y;
    }
    public bool IsHorizontallyBig() {
        return _size.x > InnerMapManager.BuildingSpotSize.x;
    }
    public bool IsVerticallyBig() {
        return _size.y > InnerMapManager.BuildingSpotSize.y;
    }
    #endregion

    #region Visuals
    public void SetVisualMode(Structure_Visual_Mode mode) {
        Color color = Color.white;
        switch (mode) {
            case Structure_Visual_Mode.Blueprint:
                color.a = 128f / 255f;
                SetStructureColor(color);
                SetPreplacedObjectsState(false);
                break;
            default:
                color = Color.white;
                SetStructureColor(color);
                RescanPathfindingGridOfStructure();
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
        _groundTileMap.gameObject.SetActive(true);
        _blockWallsTilemap?.gameObject.SetActive(true);
        tiles = null;
    }
    #endregion

    #region Walls
    /// <summary>
    /// Create necessary objects for this structure objects walls.
    /// This takes into account what type of wall this structure object has (Block walls or Thin structure walls)
    /// </summary>
    /// <param name="map">The inner tile map this structure is part of.</param>
    /// <param name="structure">The structure instance this object is connected to.</param>
    private void RegisterWalls(InnerTileMap map, LocationStructure structure) {
        if (_blockWallsTilemap != null) {
            for (int i = 0; i < tiles.Length; i++) {
                LocationGridTile tile = tiles[i];
                //block walls
                TileBase blockWallAsset = 
                    _blockWallsTilemap.GetTile(_blockWallsTilemap.WorldToCell(tile.worldLocation));
                if (blockWallAsset != null) {
                    if (blockWallAsset.name.Contains("Wall")) {
                        BlockWall blockWall =
                            InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
                        blockWall.SetWallType(_wallType);
                        structure.AddPOI(blockWall, tile);
                    }
                    else {
                        map.structureTilemap.SetTile(tile.localPlace, blockWallAsset);
                    }
                }
            }
            //disable block walls tilemap
            _blockWallsTilemap.gameObject.SetActive(false);
        } else if (wallVisuals != null && wallVisuals.Length > 0) {
            //structure walls
            walls = new StructureWallObject[wallVisuals.Length];
            for (int i = 0; i < wallVisuals.Length; i++) {
                WallVisual wallVisual = wallVisuals[i];
                StructureWallObject structureWallObject = new StructureWallObject(structure, wallVisual, _wallResource);
                Vector3Int tileLocation = map.groundTilemap.WorldToCell(wallVisual.transform.position);
                LocationGridTile tile = map.map[tileLocation.x, tileLocation.y];
                tile.SetTileType(LocationGridTile.Tile_Type.Wall);
                structureWallObject.SetGridTileLocation(tile);
                tile.AddWallObject(structureWallObject);
                walls[i] = structureWallObject;
            }    
        }
    }
    internal void ChangeResourceMadeOf(RESOURCE resource) {
        for (int i = 0; i < walls.Length; i++) {
            StructureWallObject structureWallObject = walls[i];
            structureWallObject.ChangeResourceMadeOf(resource);
        }
        for (int i = 0; i < tiles.Length; i++) {
            LocationGridTile tile = tiles[i];
            switch (resource) {
                case RESOURCE.WOOD:
                    tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.woodFloorTile);
                    break;
                case RESOURCE.STONE:
                    tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.stoneFloorTile);
                    break;
                case RESOURCE.METAL:
                    tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.stoneFloorTile);
                    break;
            }
        }
    }
    /// <summary>
    /// Get what this structures walls are made of.
    /// </summary>
    /// <returns>A resource type. NOTE: this defaults to wood if no walls are present.</returns>
    public RESOURCE WallsMadeOf() {
        if (walls.Length > 0) {
            StructureWallObject structureWallObject = walls[0];
            return structureWallObject.madeOf;
        }
        return RESOURCE.WOOD;
    }
    #endregion

    #region Pathfinding
    [ContextMenu("Rescan Pathfinding Grid Of Structure")]
    public void RescanPathfindingGridOfStructure() {
        PathfindingManager.Instance.UpdatePathfindingGraphPartialCoroutine(_groundTileMapRenderer.bounds);
    }
    #endregion

    #region Interaction
    public void OnPointerClick(BaseEventData data) {
        Debug.Log($"Player clicked {name}");
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
                        wallVisual = InstantiateWall(leftWall, centeredPos, wallTileMap.transform, _wallResource != RESOURCE.WOOD);
                    } 
                    if (tile.name.Contains("Right")) {
                        wallVisual = InstantiateWall(rightWall, centeredPos, wallTileMap.transform, _wallResource != RESOURCE.WOOD);
                    }
                    if (tile.name.Contains("Bot")) {
                        wallVisual = InstantiateWall(bottomWall, centeredPos, wallTileMap.transform, _wallResource != RESOURCE.WOOD);
                    }
                    if (tile.name.Contains("Top")) {
                        wallVisual = InstantiateWall(topWall, centeredPos, wallTileMap.transform, _wallResource != RESOURCE.WOOD);
                    }

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
                    if (_wallResource != RESOURCE.WOOD) {
                        //only update asset if wall resource is not wood.
                        wallVisual.UpdateWallAssets(_wallResource);    
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
            wallVisual.UpdateWallAssets(_wallResource);    
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

        predeterminedOccupiedCoordinates = new List<Vector3Int>();
        for (int x = bounds.xMin; x < bounds.xMax; x++) {
            for (int y = bounds.yMin; y < bounds.yMax; y++) {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tb = _groundTileMap.GetTile(pos);
                if (tb != null && assetsToConsiderAsStructure.Contains(tb)) {
                    predeterminedOccupiedCoordinates.Add(pos);
                    // _groundTileMap.SetColor(pos, Color.red);
                }
            }
        }    
        
    }
    #endregion

}
