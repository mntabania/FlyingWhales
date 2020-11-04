using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;
using ECS;

public class HexTile : MonoBehaviour, IHasNeighbours<HexTile>, ILocation {

    public HexTileData data;
    private Region _region;
    private Area _areaOfTile;
    [System.NonSerialized] public SpriteRenderer spriteRenderer;

    [Space(10)]
    [Header("Booleans")]
    public bool isRoad = false;
    public bool isOccupied = false;
    [SerializeField] private bool _isPassable = false;
    private bool _isCorrupted = false;

    [Space(10)]
    [Header("Pathfinding")]
    public GameObject unpassableGO;

    [Space(10)]
    [Header("Ledges")]
    [SerializeField] private GameObject topLeftLedge;
    [SerializeField] private GameObject topRightLedge;

    [Space(10)]
    [Header("Outlines")]
    [SerializeField] private GameObject topLeftOutline;
    [SerializeField] private GameObject topRightOutline;
    [SerializeField] private GameObject leftOutline;
    [SerializeField] private GameObject rightOutline;
    [SerializeField] private GameObject botLeftOutline;
    [SerializeField] private GameObject botRightOutline;

    [Space(10)]
    [Header("Tile Visuals")]
    [SerializeField] private GameObject _centerPiece;
    [SerializeField] private GameObject _highlightGO;
    [SerializeField] internal Transform UIParent;
    [SerializeField] private Transform resourceParent;
    [SerializeField] private GameObject biomeDetailParentGO;
    [SerializeField] private TextMesh tileTextMesh;
    [SerializeField] private GameObject _emptyCityGO;
    [SerializeField] private GameObject _hoverHighlightGO;
    [SerializeField] private GameObject _clickHighlightGO;
    [SerializeField] private GameObject _corruptionHighlightGO;


    [Space(10)]
    [Header("Tile Edges")]
    [SerializeField] private GameObject topLeftEdge;
    [SerializeField] private GameObject leftEdge;
    [SerializeField] private GameObject botLeftEdge;
    [SerializeField] private GameObject botRightEdge;
    [SerializeField] private GameObject rightEdge;
    [SerializeField] private GameObject topRightEdge;

    [Space(10)]
    [Header("Tile Borders")]
    [SerializeField] private SpriteRenderer topLeftBorder;
    [SerializeField] private SpriteRenderer leftBorder;
    [SerializeField] private SpriteRenderer botLeftBorder;
    [SerializeField] private SpriteRenderer botRightBorder;
    [SerializeField] private SpriteRenderer rightBorder;
    [SerializeField] private SpriteRenderer topRightBorder;

    [Space(10)]
    [Header("Structure Objects")]
    [SerializeField] private GameObject structureParentGO;
    private StructureObject _structureObjOnTile;

    [Space(10)]
    [Header("Minimap Objects")]
    [SerializeField] private SpriteRenderer minimapHexSprite;

    private PASSABLE_TYPE _passableType;

    //Landmark
    private BaseLandmark _landmarkOnTile = null;

    protected List<Character> _charactersAtLocation = new List<Character>(); //List of characters/party on landmark

    private Dictionary<HEXTILE_DIRECTION, HexTile> _neighbourDirections;

    public List<HexTile> allNeighbourRoads = new List<HexTile>();

    public List<HexTile> AllNeighbours { get; set; }
    public List<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN).ToList(); } }
    public List<HexTile> NoWaterTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER).ToList(); } }
    public List<HexTile> LandmarkConnectionTiles { get { return AllNeighbours.Where(o => !o.isRoad).ToList(); } }
    public List<HexTile> AllNeighbourRoadTiles { get { return AllNeighbours.Where(o => o.isRoad).ToList(); } }
    public List<HexTile> PassableNeighbours { get { return AllNeighbours.Where(o => o.isPassable).ToList(); } }

    private bool _hasScheduledCombatCheck = false;

    private int _uncorruptibleLandmarkNeighbors = 0; //if 0, can be corrupted, otherwise, cannot be corrupted
    public BaseLandmark corruptedLandmark = null;

    #region getters/setters
    public int id { get { return data.id; } }
    public int xCoordinate { get { return data.xCoordinate; } }
    public int yCoordinate { get { return data.yCoordinate; } }
    public string tileName { get { return data.tileName; } }
    public float elevationNoise { get { return data.elevationNoise; } }
    public float moistureNoise { get { return data.moistureNoise; } }
    public float temperature { get { return data.temperature; } }
    public BIOMES biomeType { get { return data.biomeType; } }
    public ELEVATION elevationType { get { return data.elevationType; } }
    public Area areaOfTile { get { return _areaOfTile; } }
    public string locationName {
        get { return tileName + "(" + xCoordinate + ", " + yCoordinate + ")"; }
    }
    public string urlName {
        get { return "<link=" + '"' + this.id.ToString() + '"' + "_hextile>" + tileName + "</link>"; }
    }
    public string coordinates {
        get { return xCoordinate + ", " + yCoordinate; }
    }
    public Region region {
        get { return _region; }
    }
    public GameObject centerPiece {
        get { return this._centerPiece; }
    }
    public GameObject highlightGO {
        get { return this._highlightGO; }
    }
    public FOG_OF_WAR_STATE currFogOfWarState {
        //get { return _currFogOfWarState; }
        get { return FOG_OF_WAR_STATE.VISIBLE; }
    }
    internal Dictionary<HEXTILE_DIRECTION, HexTile> neighbourDirections {
        get { return _neighbourDirections; }
    }
    public GameObject emptyCityGO {
        get { return this._emptyCityGO; }
    }
    public BaseLandmark landmarkOnTile {
        get { return _landmarkOnTile; }
    }
    public GameObject clickHighlightGO {
        get { return _clickHighlightGO; }
    }
    public List<Character> charactersAtLocation {
        get { return _charactersAtLocation; }
    }
    public HexTile tileLocation {
        get { return this; }
    }
    public LOCATION_IDENTIFIER locIdentifier {
        get { return LOCATION_IDENTIFIER.HEXTILE; }
    }
    public bool isOuterTileOfRegion {
        get { return this.region.outerTiles.Contains(this); }
    }
    public bool hasLandmark {
        get { return _landmarkOnTile != null; }
    }
    public bool isPassable {
        get { return _isPassable; }
    }
    public StructureObject structureObjOnTile {
        get { return _structureObjOnTile; }
    }
    public bool isCorrupted {
        get { return _isCorrupted; }
    }
    public PASSABLE_TYPE passableType {
        get { return _passableType; }
    }
    public int uncorruptibleLandmarkNeighbors {
        get { return _uncorruptibleLandmarkNeighbors; }
    }
    #endregion

    public void Initialize() {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    #region Region Functions
    internal void SetRegion(Region region) {
        _region = region;
        if (region == null) {
            data.regionID = -1;
        } else {
            data.regionID = region.id;
        }
    }
    internal bool IsAdjacentWithRegion(Region region) {
        List<HexTile> neighbors = this.AllNeighbours;
        for (int i = 0; i < neighbors.Count; i++) {
            if (neighbors[i].region.id == region.id) {
                return true;
            }
        }
        return false;
    }
    internal bool IsAdjacentToOtherRegion() {
        List<HexTile> neighbors = this.AllNeighbours;
        for (int i = 0; i < neighbors.Count; i++) {
            if (neighbors[i].region.id != this.region.id) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Elevation Functions
    internal void SetElevation(ELEVATION elevationType) {
        data.elevationType = elevationType;
    }
    public void UpdateLedgesAndOutlines() {
        if (neighbourDirections == null) {
            return;
        }
        if (elevationType != ELEVATION.WATER) {
            //re enable all outlines and disable all ledges
            topLeftLedge.SetActive(false);
            topRightLedge.SetActive(false);
            SetOutlinesState(true);
        } else { //tile is water
            //check neighbours
            //if north west tile is not water, activate top left ledge
            if (neighbourDirections.ContainsKey(HEXTILE_DIRECTION.NORTH_WEST) && neighbourDirections[HEXTILE_DIRECTION.NORTH_WEST].elevationType != ELEVATION.WATER) {
                topLeftLedge.SetActive(true);
                topLeftOutline.SetActive(false);
            } else {
                //tile doesn't have a north west neighbour
            }
            //if north east tile is not water, activate top right edge
            if (neighbourDirections.ContainsKey(HEXTILE_DIRECTION.NORTH_EAST) && neighbourDirections[HEXTILE_DIRECTION.NORTH_EAST].elevationType != ELEVATION.WATER) {
                topRightLedge.SetActive(true);
                topRightOutline.SetActive(false);
            } else {
                //tile doesn't have a north east neighbour
            }

            //check outlines
            foreach (KeyValuePair<HEXTILE_DIRECTION, HexTile> kvp in neighbourDirections) {
                HexTile neighbour = kvp.Value;
                HEXTILE_DIRECTION direction = kvp.Key;
                if (neighbour.elevationType == ELEVATION.WATER) {
                    //deactivate the outline tile in that direction
                    switch (direction) {
                        case HEXTILE_DIRECTION.NORTH_WEST:
                            topLeftOutline.SetActive(false);
                            break;
                        case HEXTILE_DIRECTION.NORTH_EAST:
                            topRightOutline.SetActive(false);
                            break;
                        case HEXTILE_DIRECTION.EAST:
                            rightOutline.SetActive(false);
                            break;
                        case HEXTILE_DIRECTION.SOUTH_EAST:
                            botRightOutline.SetActive(false);
                            break;
                        case HEXTILE_DIRECTION.SOUTH_WEST:
                            botLeftOutline.SetActive(false);
                            break;
                        case HEXTILE_DIRECTION.WEST:
                            leftOutline.SetActive(false);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
    #endregion

    #region Biome Functions
    internal void SetBiome(BIOMES biome) {
        data.biomeType = biome;
        //if(elevationType == ELEVATION.WATER) {
        //    SetMinimapTileColor(new Color(64f/255f, 164f/255f, 223f/255f));
        //} else {
        //    SetMinimapTileColor(Utilities.biomeColor[biome]);
        //}
        //biomeColor = minimapHexSprite.color;

    }
    internal void AddBiomeDetailToTile(GameObject detailPrefab) {
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(biomeDetailParentGO);
        if (children != null) {
            for (int i = 0; i < children.Length; i++) {
                Transform currChild = children[i];
                GameObject.Destroy(currChild.gameObject);
            }
        }
        if (detailPrefab != null) {
            GameObject detailGO = GameObject.Instantiate(detailPrefab, biomeDetailParentGO.transform) as GameObject;
            detailGO.transform.localScale = Vector3.one;
            detailGO.transform.localPosition = Vector3.zero;
        }
    }
    internal void SetBiomeDetailState(bool state) {
        biomeDetailParentGO.SetActive(state);
    }
    #endregion

    #region Landmarks
    public BaseLandmark CreateLandmarkOfType(BASE_LANDMARK_TYPE baseLandmarkType, LANDMARK_TYPE landmarkType) {
        GameObject landmarkGO = null;
        //Create Landmark Game Object on tile
#if WORLD_CREATION_TOOL
        landmarkGO = GameObject.Instantiate(worldcreator.WorldCreatorManager.Instance.landmarkItemPrefab, structureParentGO.transform) as GameObject;
#else
        landmarkGO = CreateLandmarkObject(landmarkType);
#endif
        switch (baseLandmarkType) {
            case BASE_LANDMARK_TYPE.SETTLEMENT:
                _landmarkOnTile = new Settlement(this, landmarkType);
                break;
            case BASE_LANDMARK_TYPE.RESOURCE:
                _landmarkOnTile = new ResourceLandmark(this, landmarkType);
                break;
            case BASE_LANDMARK_TYPE.DUNGEON:
                _landmarkOnTile = new DungeonLandmark(this, landmarkType);
                break;
            case BASE_LANDMARK_TYPE.LAIR:
                _landmarkOnTile = new LairLandmark(this, landmarkType);
                break;
            default:
                _landmarkOnTile = new BaseLandmark(this, landmarkType);
                break;
        }
        if (landmarkGO != null) {
            landmarkGO.transform.localPosition = Vector3.zero;
            landmarkGO.transform.localScale = Vector3.one;
            _landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkObject>());
        }
        _region.AddLandmarkToRegion(_landmarkOnTile);
        if (_landmarkOnTile != null) {
            SetPassableState(true);
#if !WORLD_CREATION_TOOL
            _landmarkOnTile.SetObject(ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.STRUCTURE, Utilities.NormalizeStringUpperCaseFirstLetters(landmarkType.ToString())) as StructureObj);
#endif
        }
        return _landmarkOnTile;
    }
    public BaseLandmark CreateLandmarkOfType(BASE_LANDMARK_TYPE baseLandmarkType, LandmarkSaveData data) {
        GameObject landmarkGO = null;
        //Create Landmark Game Object on tile
#if WORLD_CREATION_TOOL
        landmarkGO = GameObject.Instantiate(worldcreator.WorldCreatorManager.Instance.landmarkItemPrefab, structureParentGO.transform) as GameObject;
#else
        landmarkGO = CreateLandmarkObject(data.landmarkType);
#endif

        switch (baseLandmarkType) {
            case BASE_LANDMARK_TYPE.SETTLEMENT:
                _landmarkOnTile = new Settlement(this, data);
                break;
            case BASE_LANDMARK_TYPE.RESOURCE:
                _landmarkOnTile = new ResourceLandmark(this, data);
                break;
            case BASE_LANDMARK_TYPE.DUNGEON:
                _landmarkOnTile = new DungeonLandmark(this, data);
                break;
            case BASE_LANDMARK_TYPE.LAIR:
                _landmarkOnTile = new LairLandmark(this, data);
                break;
            default:
                _landmarkOnTile = new BaseLandmark(this, data);
                break;
        }
        if (landmarkGO != null) {
            landmarkGO.transform.localPosition = Vector3.zero;
            landmarkGO.transform.localScale = Vector3.one;
            _landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkObject>());
        }
        _region.AddLandmarkToRegion(_landmarkOnTile);
        if (_landmarkOnTile != null) {
            SetPassableState(true);
#if !WORLD_CREATION_TOOL
            _landmarkOnTile.SetObject(ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.STRUCTURE, Utilities.NormalizeStringUpperCaseFirstLetters(_landmarkOnTile.specificLandmarkType.ToString())) as StructureObj);
#endif
        }
        return _landmarkOnTile;
    }
    private GameObject CreateLandmarkObject(LANDMARK_TYPE landmarkType) {
        if (this.region.owner != null && this.region.owner.race == RACE.ELVES) {
            if (landmarkType == LANDMARK_TYPE.ELVEN_SETTLEMENT || landmarkType == LANDMARK_TYPE.IRON_MINES || landmarkType == LANDMARK_TYPE.OAK_LUMBERYARD) {
                GameObject prefab = CityGenerator.Instance.GetLandmarkPrefab(landmarkType, this.region.owner.race);
                GameObject obj = GameObject.Instantiate(prefab, structureParentGO.transform);
                if (landmarkType != LANDMARK_TYPE.ELVEN_SETTLEMENT) {
                    obj.transform.localScale = new Vector2(1.5f, 1.5f);
                }
                GameObject landmarkGO = GameObject.Instantiate(CityGenerator.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
                landmarkGO.GetComponent<LandmarkObject>().SetIconState(false);
                return landmarkGO;
            } else {
                GameObject landmarkGO = GameObject.Instantiate(CityGenerator.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
                landmarkGO.GetComponent<LandmarkObject>().SetIconState(true);
                return landmarkGO;
            }
        } else {
            GameObject landmarkGO = GameObject.Instantiate(CityGenerator.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
            landmarkGO.GetComponent<LandmarkObject>().SetIconState(true);
            return landmarkGO;
        }
    }
    public BaseLandmark LoadLandmark(BaseLandmark landmark) {
        GameObject landmarkGO = null;
        //Create Landmark Game Object on tile
        landmarkGO = GameObject.Instantiate(CityGenerator.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
        landmarkGO.transform.localPosition = Vector3.zero;
        landmarkGO.transform.localScale = Vector3.one;
        _landmarkOnTile = landmark;
        if (landmarkGO != null) {
            _landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkObject>());
        }
        _region.AddLandmarkToRegion(_landmarkOnTile);
        if (_landmarkOnTile != null) {
            SetPassableState(true);
            _landmarkOnTile.SetObject(ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.STRUCTURE, Utilities.NormalizeStringUpperCaseFirstLetters(_landmarkOnTile.specificLandmarkType.ToString())) as StructureObj);
        }
        return _landmarkOnTile;
    }
    public void RemoveLandmarkOnTile() {
        _landmarkOnTile = null;
    }
    #endregion

    #region Tile Utilities
    public bool HasNeighbourThatIsLandmark() {
        return AllNeighbours.Where(x => x.hasLandmark).Any();
    }
    /*
	 * Returns all Hex tiles gameobjects within a radius
	 * */
    public List<HexTile> GetTilesInRange(int range, bool isOnlyOuter) {
        List<HexTile> tilesInRange = new List<HexTile>();
        List<HexTile> checkedTiles = new List<HexTile>();
        List<HexTile> tilesToAdd = new List<HexTile>();

        for (int i = 0; i < range; i++) {
            if (tilesInRange.Count <= 0) {
                //tilesInRange = this.AllNeighbours;
                for (int j = 0; j < AllNeighbours.Count; j++) {
                    tilesInRange.Add(AllNeighbours[j]);
                }
                checkedTiles.Add(this);
            } else {
                tilesToAdd.Clear();
                int tilesInRangeCount = tilesInRange.Count;
                for (int j = 0; j < tilesInRangeCount; j++) {
                    if (!checkedTiles.Contains(tilesInRange[j])) {
                        checkedTiles.Add(tilesInRange[j]);
                        List<HexTile> neighbors = tilesInRange[j].AllNeighbours;
                        for (int k = 0; k < neighbors.Count; k++) {
                            if (!tilesInRange.Contains(neighbors[k])) {
                                tilesToAdd.Add(neighbors[k]);
                            }
                        }
                        tilesInRange.AddRange(tilesToAdd);
                        //						tilesToAdd.AddRange (tilesInRange[j].AllNeighbours.Where(x => !tilesInRange.Contains(x)).ToList());
                    }
                }
                if (i == range - 1 && isOnlyOuter) {
                    return tilesToAdd;
                }

                //				tilesInRange = tilesInRange.Distinct ().ToList ();
            }
        }
        return tilesInRange;
    }
    public List<HexTile> GetTilesInRange(int range) {
        List<HexTile> tilesInRange = new List<HexTile>();
        CubeCoordinate cube = GridMap.Instance.OddRToCube(new HexCoordinate(xCoordinate, yCoordinate));
        //Debug.Log("Center in cube coordinates: " + cube.x.ToString() + "," + cube.y.ToString() + "," + cube.z.ToString());
        for (int dx = -range; dx <= range; dx++) {
            for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++) {
                int dz = -dx - dy;
                HexCoordinate hex = GridMap.Instance.CubeToOddR(new CubeCoordinate(cube.x + dx, cube.y + dy, cube.z + dz));
                //Debug.Log("Hex neighbour: " + hex.col.ToString() + "," + hex.row.ToString());
                if (hex.col >= 0 && hex.row >= 0
                    && hex.col < GridMap.Instance.width && hex.row < GridMap.Instance.height
                    && !(hex.col == xCoordinate && hex.row == yCoordinate)) {
                    tilesInRange.Add(GridMap.Instance.map[hex.col, hex.row]);
                }
            }
        }
        return tilesInRange;
    }
    internal float GetDistanceTo(HexTile targetHextile) {
        return Vector3.Distance(this.transform.position, targetHextile.transform.position);
    }
    public bool CanBuildLandmarkHere(LANDMARK_TYPE landmarkToBuild, LandmarkData data, Dictionary<HexTile, LANDMARK_TYPE> landmarksToBeCreated) {
        if (this.hasLandmark || !this.isPassable || landmarksToBeCreated.ContainsKey(this)) {
            return false; //this tile is not passable or already has a landmark
        }
        if (landmarkToBuild == LANDMARK_TYPE.OAK_FORTIFICATION || landmarkToBuild == LANDMARK_TYPE.IRON_FORTIFICATION) {
            if (this.PassableNeighbours.Where(x => x.hasLandmark || landmarksToBeCreated.ContainsKey(x)).Any()) {
                return false; //check if this tile has any neighbours that are not fortifications
            }
        } else {
            if (this.PassableNeighbours.Where(x => x.hasLandmark || landmarksToBeCreated.ContainsKey(x)).Any()) {
                return false; //check if this tile has any neighbours that have landmarks
            }
        }
        if (!data.possibleSpawnPoints.Contains(this.passableType)) {
            return false; //check if this tiles' passable type meets the types the landmark can spawn on
        }
        
        //if (this.region.outerTiles.Contains(this)) {
        //    return false; //exclude outer tiles of region
        //}
        //if (!this.region.IsPartOfMainIsland(this)) {
        //    return false;
        //}
        return true;
    }
    #endregion

    #region Pathfinding
    public void FindNeighbours(HexTile[,] gameBoard, bool isForOuterGrid = false) {
        _neighbourDirections = new Dictionary<HEXTILE_DIRECTION, HexTile>();
        var neighbours = new List<HexTile>();

        List<Point> possibleExits;

        if ((yCoordinate % 2) == 0) {
            possibleExits = Utilities.EvenNeighbours;
        } else {
            possibleExits = Utilities.OddNeighbours;
        }

        for (int i = 0; i < possibleExits.Count; i++) {
            int neighbourCoordinateX = xCoordinate + possibleExits[i].X;
            int neighbourCoordinateY = yCoordinate + possibleExits[i].Y;
            if (neighbourCoordinateX >= 0 && neighbourCoordinateX < gameBoard.GetLength(0) && neighbourCoordinateY >= 0 && neighbourCoordinateY < gameBoard.GetLength(1)) {
                HexTile currNeighbour = gameBoard[neighbourCoordinateX, neighbourCoordinateY];
                if (currNeighbour != null) {
                    neighbours.Add(currNeighbour);
                } else {
                    //This part is for outerGridTiles only!
                    try {
                        neighbourCoordinateX -= GridMap.Instance._borderThickness;
                        neighbourCoordinateY -= GridMap.Instance._borderThickness;
                        currNeighbour = GridMap.Instance.map[neighbourCoordinateX, neighbourCoordinateY];
                        neighbours.Add(currNeighbour);
                    } catch {
                        //No Handling
                    }
                }
            }

        }
        this.AllNeighbours = neighbours;

        for (int i = 0; i < neighbours.Count; i++) {
            HexTile currNeighbour = neighbours[i];
            if (currNeighbour == null) {
                continue;
            }
            HEXTILE_DIRECTION dir = GetNeighbourDirection(currNeighbour, isForOuterGrid);
            if (dir != HEXTILE_DIRECTION.NONE) {
                _neighbourDirections.Add(dir, currNeighbour);
            }
        }
    }
    internal HEXTILE_DIRECTION GetNeighbourDirection(HexTile neighbour, bool isForOuterGrid = false) {
        if (neighbour == null) {
            return HEXTILE_DIRECTION.NONE;
        }
        if (!AllNeighbours.Contains(neighbour)) {
            throw new System.Exception(neighbour.name + " is not a neighbour of " + this.name);
        }
        int thisXCoordinate = this.xCoordinate;
        int thisYCoordinate = this.yCoordinate;
        if (isForOuterGrid) {
#if WORLD_CREATION_TOOL
            if (!worldcreator.WorldCreatorManager.Instance.outerGridList.Contains(neighbour)) {
                thisXCoordinate -= worldcreator.WorldCreatorManager.Instance._borderThickness;
                thisYCoordinate -= worldcreator.WorldCreatorManager.Instance._borderThickness;
            }
#else
            if (!GridMap.Instance.outerGridList.Contains(neighbour)) {
                thisXCoordinate -= GridMap.Instance._borderThickness;
                thisYCoordinate -= GridMap.Instance._borderThickness;
            }
#endif

        }
            Point difference = new Point((neighbour.xCoordinate - thisXCoordinate),
                    (neighbour.yCoordinate - thisYCoordinate));
        if (thisYCoordinate % 2 == 0) { //even
            if (difference.X == -1 && difference.Y == 1) {
                //top left
                return HEXTILE_DIRECTION.NORTH_WEST;
            } else if (difference.X == 0 && difference.Y == 1) {
                //top right
                return HEXTILE_DIRECTION.NORTH_EAST;
            } else if (difference.X == 1 && difference.Y == 0) {
                //right
                return HEXTILE_DIRECTION.EAST;
            } else if (difference.X == 0 && difference.Y == -1) {
                //bottom right
                return HEXTILE_DIRECTION.SOUTH_EAST;
            } else if (difference.X == -1 && difference.Y == -1) {
                //bottom left
                return HEXTILE_DIRECTION.SOUTH_WEST;
            } else if (difference.X == -1 && difference.Y == 0) {
                //left
                return HEXTILE_DIRECTION.WEST;
            }
        } else { //odd
            if (difference.X == 0 && difference.Y == 1) {
                //top left
                return HEXTILE_DIRECTION.NORTH_WEST;
            } else if (difference.X == 1 && difference.Y == 1) {
                //top right
                return HEXTILE_DIRECTION.NORTH_EAST;
            } else if (difference.X == 1 && difference.Y == 0) {
                //right
                return HEXTILE_DIRECTION.EAST;
            } else if (difference.X == 1 && difference.Y == -1) {
                //bottom right
                return HEXTILE_DIRECTION.SOUTH_EAST;
            } else if (difference.X == 0 && difference.Y == -1) {
                //bottom left
                return HEXTILE_DIRECTION.SOUTH_WEST;
            } else if (difference.X == -1 && difference.Y == 0) {
                //left
                return HEXTILE_DIRECTION.WEST;
            }
        }
        return HEXTILE_DIRECTION.NONE;
    }
    #endregion

    #region Roads
    public void HighlightRoad(Color color) {
        //for (int i = 0; i < roadGOs.Count; i++) {
        //    GameObject currRoad = roadGOs[i];
        //    SetRoadColor(currRoad, color);
        //}
    }
    public void SetRoadColor(GameObject roadToChange, Color color) {
        //roadToChange.spriteRenderer.color = color;
        SpriteRenderer[] children = roadToChange.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < children.Length; i++) {
            children[i].color = color;
        }
    }
    public GameObject GetRoadGameObjectForDirection(HEXTILE_DIRECTION from, HEXTILE_DIRECTION to) {
        return null;
        //if (from == HEXTILE_DIRECTION.NONE && to == HEXTILE_DIRECTION.NONE) {
        //    return null;
        //}
        //if (from == HEXTILE_DIRECTION.NONE && to != HEXTILE_DIRECTION.NONE) {
        //    //Show the directionGO of to
        //    HexRoads roadToUse = roads.First(x => x.from == to);
        //    return roadToUse.directionGO;
        //} else if (from != HEXTILE_DIRECTION.NONE && to == HEXTILE_DIRECTION.NONE) {
        //    //Show the directionGO of from
        //    HexRoads roadToUse = roads.First(x => x.from == from);
        //    return roadToUse.directionGO;
        //} else {
        //    List<RoadObject> availableRoads = roads.First(x => x.from == from).destinations;
        //    return availableRoads.Where(x => x.to == to).First().roadObj;
        //}
    }
    public void SetTileAsRoad(bool isRoad, ROAD_TYPE roadType, GameObject roadGO) {
        //roadGOs.Add(roadGO);
        //if (this.hasLandmark) {//this.isHabitable
        //    if (isRoad) {
        //        if (_roadType == ROAD_TYPE.NONE) {
        //            _roadType = roadType;
        //        }
        //    } else {
        //        _roadType = ROAD_TYPE.NONE;
        //    }
        //} else {
        //    this.isRoad = isRoad;
        //    if (isRoad) {
        //        if (_roadType == ROAD_TYPE.NONE) {
        //            _roadType = roadType;
        //        }
        //        region.AddTileAsRoad(this);
        //        RoadManager.Instance.AddTileAsRoadTile(this);
        //    } else {
        //        _roadType = ROAD_TYPE.NONE;
        //        region.RemoveTileAsRoad(this);
        //        RoadManager.Instance.RemoveTileAsRoadTile(this);
        //    }
        //}
    }
    public void SetRoadState(bool state) {
        //for (int i = 0; i < roadGOs.Count; i++) {
        //    GameObject road = roadGOs[i];
        //    road.SetActive(state);
        //}
    }
    public void ResetRoadsColors() {
            //Color color = Color.white;
            //if (this.roadType == ROAD_TYPE.MINOR) {
            //    color = Color.gray;
            //}
            //for (int i = 0; i < roadGOs.Count; i++) {
            //    GameObject currRoad = roadGOs[i];
            //    SetRoadColor(currRoad, color);
            //}
        }
    #endregion

    #region Tile Visuals
    internal void SetSortingOrder(int sortingOrder) {
        spriteRenderer.sortingOrder = sortingOrder;
        UpdateSortingOrder();
    }
    internal void UpdateSortingOrder() {
        int sortingOrder = spriteRenderer.sortingOrder;
        //if (elevationType == ELEVATION.MOUNTAIN) {
        //    centerPiece.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 56;
        //} else {
        //    centerPiece.GetComponent<SpriteRenderer>().sortingOrder = 60; //sortingOrder + 52;
        //}
#if !WORLD_CREATION_TOOL
        int centerPieceSortingOrder = (int)GridMap.Instance.height - yCoordinate;
#else
        int centerPieceSortingOrder = (int)worldcreator.WorldCreatorManager.Instance.height - yCoordinate;
#endif

        //SpriteRenderer mainRenderer = centerPiece.GetComponent<SpriteRenderer>();
        //mainRenderer.sortingOrder = centerPieceSortingOrder;
        SpriteRenderer[] children = centerPiece.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < children.Length; i++) {
            SpriteRenderer currRenderer = children[i];
            //if (currRenderer != mainRenderer) {
                currRenderer.sortingOrder = centerPieceSortingOrder;
            //}
        }

        //SpriteRenderer[] resourcesSprites = resourceParent.GetComponentsInChildren<SpriteRenderer>();
        //for (int i = 0; i < resourcesSprites.Length; i++) {
        //    resourcesSprites[i].sortingOrder = sortingOrder + 57;
        //}

        //kingdomColorSprite.spriteRenderer.sortingOrder = sortingOrder + 3;
        highlightGO.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 4;

        //topLeftEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        //leftEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        //botLeftEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        //botRightEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        //rightEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
        //topRightEdge.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + 1;
    }
    internal SpriteRenderer ActivateBorder(HEXTILE_DIRECTION direction, Color color) {
        SpriteRenderer activatedBorder = null;
        switch (direction) {
            case HEXTILE_DIRECTION.NORTH_WEST:
                topLeftBorder.gameObject.SetActive(true);
                activatedBorder = topLeftBorder;
                break;
            case HEXTILE_DIRECTION.NORTH_EAST:
                topRightBorder.gameObject.SetActive(true);
                activatedBorder = topRightBorder;
                break;
            case HEXTILE_DIRECTION.EAST:
                rightBorder.gameObject.SetActive(true);
                activatedBorder = rightBorder;
                break;
            case HEXTILE_DIRECTION.SOUTH_EAST:
                botRightBorder.gameObject.SetActive(true);
                activatedBorder = botRightBorder;
                break;
            case HEXTILE_DIRECTION.SOUTH_WEST:
                botLeftBorder.gameObject.SetActive(true);
                activatedBorder = botLeftBorder;
                break;
            case HEXTILE_DIRECTION.WEST:
                leftBorder.gameObject.SetActive(true);
                activatedBorder = leftBorder;
                break;
            default:
                break;
        }
        if (activatedBorder != null) {
            activatedBorder.color = color;
        }
        return activatedBorder;
    }
    internal void DeactivateCenterPiece() {
        if (this.biomeType == BIOMES.FOREST && this.elevationType == ELEVATION.PLAIN) {
            centerPiece.SetActive(false);
        }
    }
    internal void LoadEdges() {
        int biomeLayerOfHexTile = Utilities.biomeLayering.IndexOf(this.biomeType);
        List<HexTile> neighbours = new List<HexTile>(this.AllNeighbours);
        if (this.elevationType == ELEVATION.WATER) {
            neighbours = neighbours.Where(x => x.elevationType != ELEVATION.WATER).ToList();
        }
        for (int i = 0; i < neighbours.Count; i++) {
            HexTile currentNeighbour = neighbours[i];

            int biomeLayerOfNeighbour = Utilities.biomeLayering.IndexOf(currentNeighbour.biomeType);

            if (biomeLayerOfHexTile < biomeLayerOfNeighbour || this.elevationType == ELEVATION.WATER) {
                //int neighbourX = currentNeighbour.xCoordinate;
                //int neighbourY = currentNeighbour.yCoordinate;

                Point difference = new Point((currentNeighbour.xCoordinate - this.xCoordinate),
                    (currentNeighbour.yCoordinate - this.yCoordinate));
                if ((currentNeighbour.biomeType != this.biomeType && currentNeighbour.elevationType != ELEVATION.WATER) ||
                    this.elevationType == ELEVATION.WATER) {
                    GameObject gameObjectToEdit = null;
                    Texture[] spriteMasksToChooseFrom = null;
                    if (this.yCoordinate % 2 == 0) {
                        if (difference.X == -1 && difference.Y == 1) {
                            //top left
                            gameObjectToEdit = this.topLeftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.topLeftMasks;
                        } else if (difference.X == 0 && difference.Y == 1) {
                            //top right
                            gameObjectToEdit = this.topRightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.topRightMasks;
                        } else if (difference.X == 1 && difference.Y == 0) {
                            //right
                            gameObjectToEdit = this.rightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.rightMasks;
                        } else if (difference.X == 0 && difference.Y == -1) {
                            //bottom right
                            gameObjectToEdit = this.botRightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.botRightMasks;
                        } else if (difference.X == -1 && difference.Y == -1) {
                            //bottom left
                            gameObjectToEdit = this.botLeftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.botLeftMasks;
                        } else if (difference.X == -1 && difference.Y == 0) {
                            //left
                            gameObjectToEdit = this.leftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.leftMasks;
                        }
                    } else {
                        if (difference.X == 0 && difference.Y == 1) {
                            //top left
                            gameObjectToEdit = this.topLeftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.topLeftMasks;
                        } else if (difference.X == 1 && difference.Y == 1) {
                            //top right
                            gameObjectToEdit = this.topRightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.topRightMasks;
                        } else if (difference.X == 1 && difference.Y == 0) {
                            //right
                            gameObjectToEdit = this.rightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.rightMasks;
                        } else if (difference.X == 1 && difference.Y == -1) {
                            //bottom right
                            gameObjectToEdit = this.botRightEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.botRightMasks;
                        } else if (difference.X == 0 && difference.Y == -1) {
                            //bottom left
                            gameObjectToEdit = this.botLeftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.botLeftMasks;
                        } else if (difference.X == -1 && difference.Y == 0) {
                            //left
                            gameObjectToEdit = this.leftEdge;
                            spriteMasksToChooseFrom = Biomes.Instance.leftMasks;
                        }
                    }
                    if (gameObjectToEdit != null && spriteMasksToChooseFrom != null) {
                        SpriteRenderer sr = gameObjectToEdit.GetComponent<SpriteRenderer>();
                        sr.sprite = Biomes.Instance.GetTextureForBiome(currentNeighbour.biomeType);
                        sr.sortingOrder += biomeLayerOfNeighbour;
                        //                        Material mat = new Material(Shader.Find("AlphaMask"));
                        gameObjectToEdit.GetComponent<SpriteRenderer>().material.SetTexture("_Alpha", spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)]);
                        gameObjectToEdit.SetActive(true);

                        //                        gameObjectToEdit.spriteRenderer.material = mat;
                        //gameObjectToEdit.spriteRenderer.material.SetTexture("Alpha (A)", (Texture)spriteMasksToChooseFrom[Random.Range(0, spriteMasksToChooseFrom.Length)]);
                        //					gameObjectToEdit.GetComponent<SpriteRenderer> ().material = materialForTile;
                    }

                }
            }


        }
    }
    internal void SetBaseSprite(Sprite baseSprite) {
        spriteRenderer.sprite = baseSprite;
    }
    internal void SetCenterSprite(Sprite centerSprite) {
        this.centerPiece.GetComponent<SpriteRenderer>().sprite = centerSprite;
        this.centerPiece.SetActive(true);
    }
    internal void SetMinimapTileColor(Color color) {
        color.a = 255f / 255f;
        minimapHexSprite.color = color;
    }
    public void SetTileText(string text, int fontSize, Color fontColor, string layer = "Default") {
        tileTextMesh.text = text;
        tileTextMesh.characterSize = fontSize;
        tileTextMesh.color = fontColor;
        tileTextMesh.gameObject.layer = LayerMask.NameToLayer(layer);
        tileTextMesh.transform.localPosition = Vector3.zero;
        tileTextMesh.gameObject.SetActive(true);
    }
    private void SetOutlinesState(bool state) {
        topLeftOutline.SetActive(state);
        topRightOutline.SetActive(state);
        leftOutline.SetActive(state);
        rightOutline.SetActive(state);
        botLeftOutline.SetActive(state);
        botRightOutline.SetActive(state);
    }
    public void HighlightTile(Color color, float alpha) {
        color.a = alpha;
        _highlightGO.SetActive(true);
        _highlightGO.GetComponent<SpriteRenderer>().color = color;
    }
    public void UnHighlightTile() {
            _highlightGO.SetActive(false);
        }
    #endregion

    #region Structures Functions
    internal void CreateStructureOnTile(Faction faction, STRUCTURE_TYPE structureType, STRUCTURE_STATE structureState = STRUCTURE_STATE.NORMAL) {
        GameObject[] gameObjectsToChooseFrom = CityGenerator.Instance.GetStructurePrefabsForRace(faction.race, structureType);

        string structureKey = gameObjectsToChooseFrom[Random.Range(0, gameObjectsToChooseFrom.Length)].name;
        GameObject structureGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(structureKey, Vector3.zero, Quaternion.identity, structureParentGO.transform);
        AssignStructureObjectToTile(structureGO.GetComponent<StructureObject>());
        if (structureType == STRUCTURE_TYPE.CITY) {
            structureGO.transform.localPosition = new Vector3(0f, -0.85f, 0f);
            _landmarkOnTile.landmarkVisual.SetIconState(false);
        }

        _structureObjOnTile.Initialize(structureType, faction.factionColor, structureState, this);
        this._centerPiece.SetActive(false);
    }
    /*
        * Assign a structure object to this tile.
        * NOTE: This will destroy any current structures on this tile
        * and replace it with the new assigned one.
        * */
    internal void AssignStructureObjectToTile(StructureObject structureObj) {
        if (_structureObjOnTile != null) {
            //Destroy Current Structure
            _structureObjOnTile.DestroyStructure();
        }
        _structureObjOnTile = structureObj;
        structureObj.transform.SetParent(this.structureParentGO.transform);
        structureObj.transform.localPosition = Vector3.zero;
    }
    internal GameObject CreateSpecialStructureOnTile(LAIR lairType) {
        GameObject structureGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(CityGenerator.Instance.GetStructurePrefabForSpecialStructures(lairType).name,
            Vector3.zero, Quaternion.identity, structureParentGO.transform);
        //GameObject.Instantiate(
        //CityGenerator.Instance.GetStructurePrefabForSpecialStructures(lairType), structureParentGO.transform) as GameObject;
        structureGO.transform.localPosition = Vector3.zero;
        return structureGO;
    }
    internal void HideStructures() {
        structureParentGO.SetActive(false);
    }
    internal void ShowStructures() {
        structureParentGO.SetActive(true);
    }
    public void RuinStructureOnTile(bool immediatelyDestroyStructures) {
        if (_structureObjOnTile != null) {
            Debug.Log(GameManager.Instance.month + "/" + GameManager.Instance.days + "/" + GameManager.Instance.year + " - RUIN STRUCTURE ON: " + this.name);
            if (immediatelyDestroyStructures) {
                _structureObjOnTile.DestroyStructure();
            } else {
                _structureObjOnTile.SetStructureState(STRUCTURE_STATE.RUINED);
            }
            if (landmarkOnTile != null) {
                //landmarkOnTile.AddHistory ("Landmark structure destroyed!");
            }
        }
    }
    /*
        Does this tile have a structure on it?
            */
    public bool HasStructure() {
        return _structureObjOnTile != null || (landmarkOnTile != null && landmarkOnTile.isOccupied);
    }
    #endregion

    #region Tile Functions
    public void DisableColliders() {
        this.GetComponent<Collider2D>().enabled = false;
        Collider[] colliders = this.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = false;
        }
    }
    /*
        * Reset all values for this tile.
        * NOTE: This will set the structure to ruined.
        * To force destroy structure, call DestroyStructure
        * in StructureObject instead.
        * */
    public void ResetTile() {
        this.isOccupied = false;

        RuinStructureOnTile(false);
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
        }
    }
    public void Occupy() {
        this.isOccupied = true;
    }
    public void Unoccupy(bool immediatelyDestroyStructures = false) {
        //if (!_isBorderOfCities.Select(x => x.kingdom).Contains(ownedByCity.kingdom)
        //    && !_isOuterTileOfCities.Select(x => x.kingdom).Contains(ownedByCity.kingdom)) {
        //    _visibleByKingdoms.Remove(ownedByCity.kingdom);
        //}
        isOccupied = false;
        //ownedByCity = null;
        //SetMinimapTileColor(biomeColor);
        //this._kingdomColorSprite.color = Color.white;
        //this.kingdomColorSprite.gameObject.SetActive(false);
        RuinStructureOnTile(immediatelyDestroyStructures);
        //city = null;

        //Destroy Nameplates
        //RemoveCityNamePlate();
        Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(UIParent.gameObject);
        for (int i = 0; i < children.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(children[i].gameObject);
        }
    }
    #endregion

    #region Passability
    public void SetPassableState(bool state) {
        _isPassable = state;
        _centerPiece.SetActive(!state);
//#if WORLD_CREATION_TOOL
//        unpassableGO.SetActive(false);
//#else
        unpassableGO.SetActive(!state);
//#endif
        if (!state) {
            _passableType = PASSABLE_TYPE.UNPASSABLE;
        }
        //UpdatePassableVisuals();
    }
    public void SetPassableObject(object obj) {
        SetCenterSprite(null);
        Transform[] existingChildren = Utilities.GetComponentsInDirectChildren<Transform>(centerPiece);
        if (existingChildren != null) {
            for (int i = 0; i < existingChildren.Length; i++) {
                Transform currChild = existingChildren[i];
                GameObject.Destroy(currChild.gameObject);
            }
        }

        if (obj == null) {
            //SetCenterSprite(null);
            return;
        }
        if (obj is Sprite) {
            SetCenterSprite(obj as Sprite);
        } else {
            GameObject centerObj = GameObject.Instantiate(obj as Object, centerPiece.transform) as GameObject;
            centerObj.transform.localPosition = Vector3.zero;
            centerObj.transform.localScale = Vector3.one;
            SpriteRenderer[] children = centerObj.GetComponentsInChildren<SpriteRenderer>();
            for (int i = 0; i < children.Length; i++) {
                SpriteRenderer currChild = children[i];
                currChild.sortingLayerName = "TileDetails";
            }
            SetCenterSprite(null);
        }
    }
    public void DeterminePassableType() {
        _passableType = GetPassableType();
    }
    public PASSABLE_TYPE GetPassableType() {
        PassableTileData data = GetPassableTileData();
        if (data.adjacentTiles.Count == 2 
            || (data.adjacentTiles.Count == 1 && data.adjacentTiles[0].tiles.Count <= 2 && data.unadjacentTiles.Count == 1)) {
            //minor bottleneck (connected to 2 unadjacent pairs of either 1 or 2 adjacent passable tiles)
            return PASSABLE_TYPE.MINOR_BOTTLENECK;
        } else if (data.unadjacentTiles.Count == 2) {
            //major bottleneck (connected to 2 unadjacent passable tiles)
            return PASSABLE_TYPE.MAJOR_BOTTLENECK;
        } else if (data.IsDeadEnd()) {
            if (data.HasNumberOfUnadjacentTiles(1) || data.HasNumberOfAdjacentTiles(2)) {
                return PASSABLE_TYPE.MAJOR_DEADEND;
            } else if (data.HasNumberOfAdjacentTiles(3)) {
                return PASSABLE_TYPE.MINOR_DEADEND;
            }
            throw new System.Exception("Cannot Get Dead End Type!");
        }  else if (data.unadjacentTiles.Count == 3) {
            //crossroad (connected to 3 unadjacent passable tiles)
            return PASSABLE_TYPE.CROSSROAD;
        } else if (data.TotalPassableNeighbours(this) >= 4) {
            //wide open (connected to 4o to 6 passable tiles)
            return PASSABLE_TYPE.WIDE_OPEN;
        } else {
            //open (the rest)
            return PASSABLE_TYPE.OPEN;
        }
    }
    private PassableTileData GetPassableTileData() {
        return new PassableTileData(this);
    }
    public bool IsBottleneck() {
        return _passableType == PASSABLE_TYPE.MAJOR_BOTTLENECK || _passableType == PASSABLE_TYPE.MINOR_BOTTLENECK;
    }
    public bool IsDeadEnd() {
        return _passableType == PASSABLE_TYPE.MAJOR_DEADEND || _passableType == PASSABLE_TYPE.MINOR_DEADEND;
    }
    public bool IsBorderTileOfRegion() {
            return region.outerTiles.Contains(this);
        }
    #endregion

    #region Monobehaviour Functions
    private void OnMouseOver() {
        MouseOver();
    }
    private void OnMouseExit() {
        MouseExit();
    }
    void OnMouseDown() {
        LeftClick();
    }
    public void LeftClick() {
#if !WORLD_CREATION_TOOL
        if (UIManager.Instance.IsMouseOnUI() || currFogOfWarState != FOG_OF_WAR_STATE.VISIBLE || UIManager.Instance.IsConsoleShowing()) {
            //if (UIManager.Instance.IsConsoleShowing()) {
            //    UIManager.Instance.consoleUI.AddText(this.name);
            //}
            return;
        }

        if (this.landmarkOnTile != null) {
            if (UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark != null) {
                if (UIManager.Instance.landmarkInfoUI.isWaitingForAttackTarget && !UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark.isAttackingAnotherLandmark) {
                    if (UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark.landmarkObj.CanAttack(this.landmarkOnTile)) {
                        //Attack landmark;
                        Debug.Log(UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark.landmarkName + " will attack " + this.landmarkOnTile.landmarkName);
                        UIManager.Instance.landmarkInfoUI.currentlyShowingLandmark.landmarkObj.AttackLandmark(this.landmarkOnTile);
                        UIManager.Instance.landmarkInfoUI.SetAttackButtonState(false);
                        UIManager.Instance.landmarkInfoUI.SetActiveAttackButtonGO(false);
                        return;
                    } else {
                        Debug.Log("Cannot attack " + landmarkOnTile.landmarkName + "! Same faction!");
                        return;
                    }
                }
            }
            UIManager.Instance.ShowLandmarkInfo(this.landmarkOnTile);
        }
        //      else{
        //	UIManager.Instance.ShowHexTileInfo (this);
        //}
        UIManager.Instance.HidePlayerActions();
#endif
    }
    private void RightClick() {
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing() || UIManager.Instance.characterInfoUI.activeCharacter == null || this.landmarkOnTile == null) {
            //if (UIManager.Instance.IsConsoleShowing() && this.hasLandmark) {
            //    UIManager.Instance.consoleUI.AddText(this.landmarkOnTile.landmarkName);
            //}
            return;
        }
        UIManager.Instance.ShowPlayerActions(this.landmarkOnTile);

        //		if(this.landmarkOnTile == null){
        //			UIManager.Instance.ShowPlayerActions (this);
        //		}else{
        //			UIManager.Instance.ShowPlayerActions (this.landmarkOnTile);
        //		}

    }

    public void MouseOver() {
#if WORLD_CREATION_TOOL
        //Debug.Log("IS MOUSE OVER UI " + worldcreator.WorldCreatorUI.Instance.IsMouseOnUI());
        if (!worldcreator.WorldCreatorUI.Instance.IsMouseOnUI()) {
            Messenger.Broadcast<HexTile>(Signals.TILE_HOVERED_OVER, this);
            if (Input.GetMouseButton(0)) {
                Messenger.Broadcast<HexTile>(Signals.TILE_LEFT_CLICKED, this);
            }
            if (Input.GetMouseButton(1)) {
                Messenger.Broadcast<HexTile>(Signals.TILE_RIGHT_CLICKED, this);
            }
        }
        //ShowHexTileInfo();
#else
        if (UIManager.Instance.IsMouseOnUI() || currFogOfWarState != FOG_OF_WAR_STATE.VISIBLE) {
            return;
        }
        if (this.landmarkOnTile != null) {
            _hoverHighlightGO.SetActive(true);
        }
        //ShowHexTileInfo();
        //if (Input.GetMouseButtonDown(0)) {
        //    LeftClick();
        //} else if (Input.GetMouseButtonDown(1)) {
        //    RightClick();
        //}
#endif
    }
    public void MouseExit() {
#if WORLD_CREATION_TOOL
        //if (!worldcreator.WorldCreatorUI.Instance.IsMouseOnUI()) {
            Messenger.Broadcast<HexTile>(Signals.TILE_HOVERED_OUT, this);
        //}
#else
        _hoverHighlightGO.SetActive(false);
        HideSmallInfoWindow();
        if (UIManager.Instance.IsMouseOnUI() || currFogOfWarState != FOG_OF_WAR_STATE.VISIBLE || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        //if (_landmarkOnTile != null && isHabitable) {
        //	if (_landmarkOnTile.owner != null) {
        //		this.region.HighlightRegionTiles(_landmarkOnTile.owner.factionColor, 69f / 255f);
        //	}
        //}
        
#endif
    }
    #endregion

    #region For Testing
    [Space(10)]
    [Header("For Testing")]
    [SerializeField] private int range = 0;
    List<HexTile> tiles = new List<HexTile>();
    [ContextMenu("Show Tiles In Range")]
    public void ShowTilesInRange() {
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].spriteRenderer.color = Color.white;
        }
        tiles.Clear();
        tiles.AddRange(this.GetTilesInRange(range));
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].spriteRenderer.color = Color.magenta;
        }
    }
    [ContextMenu("Show Hextile Positions")]
    public void ShowHextileBounds() {
        Debug.Log("Local Pos: " + this.transform.localPosition.ToString());
        Debug.Log("Pos: " + this.transform.position.ToString());
    }
    [ContextMenu("Force Reset Tile")]
    public void ForceResetTile() {
        ResetTile();
    }
    [ContextMenu("Load Border Lines")]
    public void LoadBorderLinesForTesting() {
        HexTile currTile = this;
        for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
            HexTile currNeighbour = currTile.AllNeighbours[j];
            if (currNeighbour.region != currTile.region) {
                //Load Border For currTile
                Debug.Log(currNeighbour.name + " - " + currTile.GetNeighbourDirection(currNeighbour, true).ToString());
                //HEXTILE_DIRECTION borderTileToActivate = currTile.GetNeighbourDirection(currNeighbour, true);
                //SpriteRenderer border = currTile.ActivateBorder(borderTileToActivate);
                //currTile.region.AddRegionBorderLineSprite(border);

                //if(currTile.xCoordinate == _borderThickness - 1 && currTile.yCoordinate > _borderThickness && currTile.yCoordinate < height) {
                //    //tile is part of left border
                //    if(borderTileToActivate == HEXTILE_DIRECTION.NORTH_WEST) {
                //        currTile.region.AddRegionBorderLineSprite(currTile.ActivateBorder(HEXTILE_DIRECTION.NORTH_EAST));
                //    }
                //}
            }
        }
    }
    [ContextMenu("Log Neighbour Directions")]
    public void LogNeighbourDirections() {
        string text = this.name + " neighbours: ";
        foreach (KeyValuePair<HEXTILE_DIRECTION, HexTile> kvp in neighbourDirections) {
            text += "\n" + kvp.Key.ToString() + " - " + kvp.Value.name;
        }
        Debug.Log(text);
    }
    [ContextMenu("Log Screen Position")]
    public void LogScreenPosition() {
        Debug.Log(CameraMove.Instance.wholeMapCamera.WorldToScreenPoint(this.transform.position));
    }
    private void ShowHexTileInfo() {
        UIManager.Instance.ShowSmallInfo(this.name + " - " + this.passableType.ToString());
    }
    private void ShowLandmarkInfo() {
        string text = string.Empty;
        text += "[b]Characters in landmark: [/b]";
        if(_landmarkOnTile.charactersAtLocation.Count > 0) {
            for (int i = 0; i < _landmarkOnTile.charactersAtLocation.Count; i++) {
                Character currObj = _landmarkOnTile.charactersAtLocation[i];
                text += "\n" + currObj.name;
                //if (currObj is Party) {
                //    text += "\n" + ((Party)currObj).name;
                //} else if (currObj is Character) {
                //    text += "\n" + ((Character)currObj).name;
                //}
            }
        } else {
            text += "NONE";
        }
        
        text += "\n[b]Characters in tile: [/b]";
        if (_charactersAtLocation.Count > 0) {
            for (int i = 0; i < _charactersAtLocation.Count; i++) {
                Character currObj = _charactersAtLocation[i];
                text += "\n" + currObj.name;
                //if (currObj is Party) {
                //    text += "\n" + ((Party)currObj).name;
                //} else if (currObj is Character) {
                //    text += "\n" + ((Character)currObj).name;
                //}
            }
        } else {
            text += "NONE";
        }

        UIManager.Instance.ShowSmallInfo(text);
    }
    private void HideSmallInfoWindow() {
        UIManager.Instance.HideSmallInfo();
    }
    public override string ToString() {
        return this.locationName;
    }
    #endregion

    #region Characters
	public void AddCharacterToLocation(Character character) {
		if (!_charactersAtLocation.Contains(character)) {
			_charactersAtLocation.Add(character);
            character.SetSpecificLocation(this);
            //if (character is Character){
            //  Character currChar = character as Character;
            //  currChar.SetSpecificLocation(this);
			//}else if(character is Party){
            //  Party currParty = character as Party;
            //  currParty.SetSpecificLocation(this);
			//}
            if (!_hasScheduledCombatCheck) {
                ScheduleCombatCheck();
            }
        }
	}
	public void RemoveCharacterFromLocation(Character character) {
		_charactersAtLocation.Remove(character);
        character.SetSpecificLocation(null);
  //      if (character is Character){
  //          Character currChar = character as Character;
  //          currChar.SetSpecificLocation(null);
  //      } else if(character is Party){
  //          Party currParty = character as Party;
  //          currParty.SetSpecificLocation(null);
		//}
        if(_charactersAtLocation.Count == 0 && _hasScheduledCombatCheck) {
            UnScheduleCombatCheck();
        }
	}
    public void ReplaceCharacterAtLocation(Character characterToReplace, Character characterToAdd) {
        if (_charactersAtLocation.Contains(characterToReplace)) {
            int indexOfCharacterToReplace = _charactersAtLocation.IndexOf(characterToReplace);
            _charactersAtLocation.Insert(indexOfCharacterToReplace, characterToAdd);
            _charactersAtLocation.Remove(characterToReplace);
            characterToAdd.SetSpecificLocation(this);
            //if (characterToAdd is Character) {
            //    Character currChar = characterToAdd as Character;
            //    currChar.SetSpecificLocation(this);
            //} else if (characterToAdd is Party) {
            //    Party currParty = characterToAdd as Party;
            //    currParty.SetSpecificLocation(this);
            //}
            if (!_hasScheduledCombatCheck) {
                ScheduleCombatCheck();
            }
        }
    }
    public Character GetCharacterAtLocationByID(int id, bool includeTraces = false){
		for (int i = 0; i < _charactersAtLocation.Count; i++) {
            if (_charactersAtLocation[i].id == id) {
                return _charactersAtLocation[i];
            }
            //if (_charactersAtLocation[i]	is Character){
			//	if(((Character)_charactersAtLocation[i]).id == id){
			//		return (Character)_charactersAtLocation [i];
			//	}
			//}else if(_charactersAtLocation[i] is Party){
			//	Party party = (Party)_charactersAtLocation [i];
			//	for (int j = 0; j < party.partyMembers.Count; j++) {
			//		if(party.partyMembers[j].id == id){
			//			return party.partyMembers [j];
			//		}
			//	}
			//}
		}
		return null;
	}
	//public Party GetPartyAtLocationByLeaderID(int id){
	//	for (int i = 0; i < _charactersAtLocation.Count; i++) {
	//		if(_charactersAtLocation[i]	is Party){
	//			if(((Party)_charactersAtLocation[i]).partyLeader.id == id){
	//				return (Party)_charactersAtLocation [i];
	//			}
	//		}
	//	}
	//	return null;
	//}
	//public int CharactersCount(bool includeHostile = false){
	//	int count = 0;
	//	for (int i = 0; i < _charactersAtLocation.Count; i++) {
	//		if(_charactersAtLocation[i]	is Party){
	//			count += ((Party)_charactersAtLocation [i]).partyMembers.Count;
	//		}else {
	//			count += 1;
	//		}
	//	}
	//	return count;
	//}
    #endregion

    #region Combat
    public void ScheduleCombatCheck() {
        //_hasScheduledCombatCheck = true;
        //Messenger.AddListener(Signals.HOUR_STARTED, CheckForCombat);
    }
    public void UnScheduleCombatCheck() {
        //_hasScheduledCombatCheck = false;
        //Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForCombat);
    }
    /*
        Check this location for encounters, start if any.
        Mechanics can be found at https://trello.com/c/PgK25YvC/837-encounter-mechanics.
            */
    public void CheckForCombat() {
        //At the start of each day:
        if (HasHostilities() && HasCombatInitializers()) {
            PairUpCombats();
        }
        ContinueDailyActions();
    }
    public void PairUpCombats() {
        List<Character> combatInitializers = GetCharactersByCombatPriority();
        if (combatInitializers != null) {
            for (int i = 0; i < combatInitializers.Count; i++) {
                Character currInitializer = combatInitializers[i];
                Debug.Log("Finding combat pair for " + currInitializer.name);
                if (currInitializer.isInCombat) {
                    continue; //this current group is already in combat, skip it
                }
                //- If there are hostile parties in combat stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
                List<Character> combatGroups = new List<Character>(GetGroupsBasedOnStance(STANCE.COMBAT, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
                if (combatGroups.Count > 0) {
                    Character chosenEnemy = combatGroups[Random.Range(0, combatGroups.Count)];
                    StartCombatBetween(currInitializer, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }

                //Otherwise, if there are hostile parties in neutral stance who are not engaged in combat, the attacking character will initiate combat with one of them at random
                List<Character> neutralGroups = new List<Character>(GetGroupsBasedOnStance(STANCE.NEUTRAL, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
                if (neutralGroups.Count > 0) {
                    Character chosenEnemy = neutralGroups[Random.Range(0, neutralGroups.Count)];
                    StartCombatBetween(currInitializer, chosenEnemy);
                    continue; //the attacking group has found an enemy! skip to the next group
                }

                //- Otherwise, if there are hostile parties in stealthy stance who are not engaged in combat, the attacking character will attempt to initiate combat with one of them at random.
                List<Character> stealthGroups = new List<Character>(GetGroupsBasedOnStance(STANCE.STEALTHY, true, currInitializer).Where(x => x.IsHostileWith(currInitializer)));
                if (stealthGroups.Count > 0) {
                    //The chance of initiating combat is 35%
                    if (Random.Range(0, 100) < 35) {
                        Character chosenEnemy = stealthGroups[Random.Range(0, stealthGroups.Count)];
                        StartCombatBetween(currInitializer, chosenEnemy);
                        continue; //the attacking group has found an enemy! skip to the next group
                    }
                }
            }
        }
    }
    public List<Character> GetCharactersByCombatPriority() {
        //if (_charactersAtLocation.Count <= 0) {
        //    return null;
        //}
        //return _charactersAtLocation.Where(x => x.currentAction.combatPriority > 0).OrderByDescending(x => x.currentAction.combatPriority).ToList();
        return null;
    }
    public bool HasCombatInitializers() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            Character currChar = _charactersAtLocation[i];
            //if (currChar.currentAction.combatPriority > 0) {
            //    return true;
            //}
        }
        return false;
    }
    public bool HasHostilities() {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            Character currItem = _charactersAtLocation[i];
            for (int j = 0; j < _charactersAtLocation.Count; j++) {
                Character otherItem = _charactersAtLocation[j];
                if(currItem != otherItem) {
                    if (currItem.IsHostileWith(otherItem)) {
                        return true; //there are characters with hostilities
                    }
                }
            }
        }
        return false;
    }
    public bool HasHostileCharactersWith(Character character) {
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            Character currItem = _charactersAtLocation[i];
            if (currItem == character) {
                continue; //skip
            }
            Faction factionOfItem = currItem.faction;
            //if (currItem is Character) {
            //    factionOfItem = (currItem as Character).faction;
            //} else if (currItem is Party) {
            //    factionOfItem = (currItem as Party).faction;
            //}
            if (factionOfItem == null || character.faction == null) {
                return true;
            } else {
                if (factionOfItem.id == character.faction.id) {
                    continue; //skip this item, since it has the same faction as the other faction
                }
                FactionRelationship rel = character.faction.GetRelationshipWith(factionOfItem);
                if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasHostilitiesWith(Faction faction, bool withFactionOnly = false) {
        if(faction == null && _charactersAtLocation.Count > 0) {
            return true; //the passed faction is null (factionless), if there are any characters on this tile
        }
        if (!withFactionOnly) { //only check characters if with faction only is false
            for (int i = 0; i < _charactersAtLocation.Count; i++) {
                Character currItem = _charactersAtLocation[i];
                Faction factionOfItem = currItem.faction;
                //if (currItem is Character) {
                //    factionOfItem = (currItem as Character).faction;
                //} else if (currItem is Party) {
                //    factionOfItem = (currItem as Party).faction;
                //}
                if (factionOfItem == null) {
                    return true;
                } else {
                    if (factionOfItem.id == faction.id) {
                        continue; //skip this item, since it has the same faction as the other faction
                    }
                    FactionRelationship rel = faction.GetRelationshipWith(factionOfItem);
                    if (rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    public List<Character> GetGroupsBasedOnStance(STANCE stance, bool notInCombatOnly, Character except = null) {
        List<Character> groups = new List<Character>();
        for (int i = 0; i < _charactersAtLocation.Count; i++) {
            Character currGroup = _charactersAtLocation[i];
            if (notInCombatOnly) {
                if (currGroup.isInCombat) {
                    continue; //skip
                }
            }
            if(currGroup.GetCurrentStance() == stance) {
                if (except != null && currGroup == except) {
                    continue; //skip
                }
                groups.Add(currGroup);
            }
        }
        return groups;
    }
    public void StartCombatBetween(Character combatant1, Character combatant2) {
        Combat combat = new Combat(this);
        combatant1.SetIsInCombat(true);
        combatant2.SetIsInCombat(true);
        combat.AddCharacter(SIDES.A, combatant1);
        combat.AddCharacter(SIDES.B, combatant2);

        //if (combatant1 is Party) {
        //    combat.AddCharacters(SIDES.A, (combatant1 as Party).partyMembers);
        //} else {
        //    combat.AddCharacter(SIDES.A, combatant1 as Character);
        //}
        //if (combatant2 is Party) {
        //    combat.AddCharacters(SIDES.B, (combatant2 as Party).partyMembers);
        //} else {
        //    combat.AddCharacter(SIDES.B, combatant2 as Character);
        //}
        //this.specificLocation.SetCurrentCombat(combat);
        MultiThreadPool.Instance.AddToThreadPool(combat);
    }
    public void ContinueDailyActions() {
        //for (int i = 0; i < _charactersAtLocation.Count; i++) {
        //    Character currItem = _charactersAtLocation[i];
        //    currItem.ContinueDailyAction();
        //}
    }
    #endregion

    #region Materials
    public void SetMaterialOnTile(MATERIAL material) {
        //_materialOnTile = material;
        //GameObject resource = GameObject.Instantiate(Biomes.Instance.ebonyPrefab, resourceParent) as GameObject;
        //resource.transform.localPosition = Vector3.zero;
        //resource.transform.localScale = Vector3.one;
        region.AddTileWithMaterial(this);
    }
    #endregion

    #region Corruption
    public void SetCorruption(bool state, BaseLandmark landmark = null) {
        if(_isCorrupted != state) {
            _isCorrupted = state;
            if (_isCorrupted) {
                corruptedLandmark = landmark;
            }
            this._corruptionHighlightGO.SetActive(_isCorrupted);
            if (landmarkOnTile != null) {
                landmarkOnTile.ToggleCorruption(_isCorrupted);
            }
        }
    }
    public bool CanThisTileBeCorrupted() {
        //if(landmarkNeighbor != null && (!landmarkNeighbor.tileLocation.isCorrupted || landmarkNeighbor.tileLocation.uncorruptibleLandmarkNeighbors > 0)) {
        //    return false;
        //}
        //if(_landmarkDirection.Count > 0) {
        //    foreach (BaseLandmark landmark in _landmarkDirection.Keys) {
        //        if (landmark.IsDirectionBlocked(_landmarkDirection[landmark])) {
        //            return false;
        //        }
        //    }
        //}
        return true;
    }
    public void SetUncorruptibleLandmarkNeighbors(int amount) {
        _uncorruptibleLandmarkNeighbors = amount;
    }
    public void AdjustUncorruptibleLandmarkNeighbors(int amount) {
            _uncorruptibleLandmarkNeighbors += amount;
            if(_uncorruptibleLandmarkNeighbors < 0) {
                _uncorruptibleLandmarkNeighbors = 0;
            }
            if(_uncorruptibleLandmarkNeighbors > 1 && _landmarkOnTile != null) {
                _uncorruptibleLandmarkNeighbors = 1;
            }
        }
    #endregion

    #region Areas
    public void SetArea(Area area) {
        _areaOfTile = area;
    }
    #endregion
}
