using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathFind;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using Locations.Settlements;
using SpriteGlow;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class HexTile : MonoBehaviour, IHasNeighbours<HexTile>, IPlayerActionTarget, ISelectable {

    public HexTileData data;
    private NPCSettlement _npcSettlementOfTile;
    public SpriteRenderer spriteRenderer;
    private bool _isCorrupted = false;
    
    [Space(10)]
    [Header("Tile Visuals")]
    [SerializeField] private GameObject _centerPiece;
    [SerializeField] private GameObject _highlightGO;
    [SerializeField] private GameObject _hoverHighlightGO;
    [SerializeField] private Animator baseTileAnimator;
    [SerializeField] private SpriteRenderer emptyBuildingSpotGO;
    [SerializeField] private SpriteRenderer currentlyBuildingSpotGO;

    [Space(10)]
    [Header("Tile Borders")]
    [SerializeField] private SpriteRenderer topLeftBorder;
    [SerializeField] private SpriteRenderer leftBorder;
    [SerializeField] private SpriteRenderer botLeftBorder;
    [SerializeField] private SpriteRenderer botRightBorder;
    [SerializeField] private SpriteRenderer rightBorder;
    [SerializeField] private SpriteRenderer topRightBorder;
    [SerializeField] private SpriteGlowEffect topLeftBorderGlow;
    [SerializeField] private SpriteGlowEffect leftBorderGlow;
    [SerializeField] private SpriteGlowEffect botLeftBorderGlow;
    [SerializeField] private SpriteGlowEffect botRightBorderGlow;
    [SerializeField] private SpriteGlowEffect rightBorderGlow;
    [SerializeField] private SpriteGlowEffect topRightBorderGlow;

    [Space(10)]
    [Header("Structure Objects")]
    [SerializeField] private GameObject structureParentGO;
    [SerializeField] private SpriteRenderer mainStructure;
    [SerializeField] private SpriteRenderer structureTint;
    [SerializeField] private Animator structureAnimation;

    [Space(10)]
    [Header("Beaches")]
    [SerializeField] private SpriteRenderer topLeftBeach;
    [SerializeField] private SpriteRenderer leftBeach;
    [SerializeField] private SpriteRenderer botLeftBeach;
    [SerializeField] private SpriteRenderer botRightBeach;
    [SerializeField] private SpriteRenderer rightBeach;
    [SerializeField] private SpriteRenderer topRightBeach;

    //properties
    public BaseLandmark landmarkOnTile { get; private set; }
    public Region region { get; private set; }
    public TileFeatureComponent featureComponent { get; private set; }
    public BaseSettlement settlementOnTile { get; private set; }
    public List<HexTile> AllNeighbours { get; set; }
    public List<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN).ToList(); } }
    public bool isCurrentlyBeingCorrupted { get; private set; }
    public List<LocationGridTile> locationGridTiles { get; private set; }
    public List<LocationGridTile> borderTiles { get; private set; }
    public Sprite baseSprite { get; private set; }
    public Vector2 selectableSize { get; private set; }
    public InnerMapHexTile innerMapHexTile { get; private set; }

    private List<LocationGridTile> corruptedTiles;
    private int _uncorruptibleLandmarkNeighbors = 0; //if 0, can be corrupted, otherwise, cannot be corrupted
    private Dictionary<HEXTILE_DIRECTION, HexTile> _neighbourDirections;
    //private List<string> demonicLandmarksThatCanBeBuilt;
    private int _isBeingDefendedCount;

    //Components
    public HexTileSpellsComponent spellsComponent { get; private set; }

    #region getters/setters
    public int id => data.id;
    public int xCoordinate => data.xCoordinate;
    public int yCoordinate => data.yCoordinate;
    public string tileName => data.tileName;
    public string thisName => data.tileName;
    public float elevationNoise => data.elevationNoise;
    public float moistureNoise => data.moistureNoise;
    public float temperature => data.temperature;
    public BIOMES biomeType => data.biomeType;
    public ELEVATION elevationType => data.elevationType;
    private string locationName => $"({xCoordinate.ToString()}, {yCoordinate.ToString()})";
    private GameObject centerPiece => _centerPiece;
    private GameObject highlightGO => _highlightGO;
    private Dictionary<HEXTILE_DIRECTION, HexTile> neighbourDirections => _neighbourDirections;
    public bool isCorrupted => _isCorrupted;
    public bool isBeingDefended => _isBeingDefendedCount > 0;
    public Vector3 worldPosition {
        get {
            Vector2 pos = innerMapHexTile.gridTileCollections[0].locationGridTileCollectionItem.transform.position;
            pos.x += 3.5f;
            pos.y += 3.5f;
            return pos;
        }
    }
    #endregion

    private void Awake() {
        _highlightSpriteRenderer = _highlightGO.GetComponent<SpriteRenderer>();
        _structureAnimatorSpriteRenderer = structureAnimation.gameObject.GetComponent<SpriteRenderer>();
        _highlightGOSpriteRenderer = highlightGO.GetComponent<SpriteRenderer>();
        _hoverHighlightSpriteRenderer = _hoverHighlightGO.GetComponent<SpriteRenderer>();
        Random.ColorHSV();
        ConstructDefaultActions();
    }
    public void Initialize() {
        featureComponent = new TileFeatureComponent();
        spellsComponent = new HexTileSpellsComponent(this);
        //demonicLandmarksThatCanBeBuilt = new List<string>();
        selectableSize = new Vector2Int(12, 12);
        Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
    }
    private void OnGameLoaded() {
        Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
        SubscribeListeners();
        SetBordersState(false, false, Color.red);
        if (landmarkOnTile != null && landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.VILLAGE) {
            CheckIfStructureVisualsAreStillValid();    
        }
    }

    #region Elevation Functions
    internal void SetElevation(ELEVATION elevationType) {
        data.elevationType = elevationType;
    }
    #endregion

    #region Biome Functions
    internal void SetBiome(BIOMES biome) {
        data.biomeType = biome;
    }
    #endregion

    #region Landmarks
    private void SetLandmarkOnTile(BaseLandmark landmarkOnTile) {
        this.landmarkOnTile = landmarkOnTile;
    }
    public BaseLandmark CreateLandmarkOfType(LANDMARK_TYPE landmarkType) {
        SetLandmarkOnTile(LandmarkManager.Instance.CreateNewLandmarkInstance(this, landmarkType));
        //Create Landmark Game Object on tile
        CreateLandmarkVisual(landmarkType);
        SetElevation(landmarkType == LANDMARK_TYPE.CAVE ? ELEVATION.MOUNTAIN : ELEVATION.PLAIN);
        Biomes.Instance.UpdateTileVisuals(this);
        return landmarkOnTile;
    }
    public BaseLandmark CreateLandmarkOfType(SaveDataLandmark saveData) {
        SetLandmarkOnTile(LandmarkManager.Instance.CreateNewLandmarkInstance(this, saveData));
        //Create Landmark Game Object on tile
        CreateLandmarkVisual(saveData.landmarkType);
        return landmarkOnTile;
    }
    private void CreateLandmarkVisual(LANDMARK_TYPE landmarkType) {
        GameObject landmarkGO = Instantiate(LandmarkManager.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
        landmarkGO.transform.localPosition = Vector3.zero;
        landmarkGO.transform.localScale = Vector3.one;
        landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkVisual>());
        UpdateLandmarkVisuals();
    }
    /// <summary>
    /// Update the structure assets of this tile based on the landmark that is on this tile.
    /// </summary>
    public void UpdateLandmarkVisuals() {
        RACE race = RACE.NONE;
        if (settlementOnTile?.owner != null) {
            race = settlementOnTile.owner.race;
        }
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkOnTile.specificLandmarkType);
        List<LandmarkStructureSprite> landmarkTileSprites = LandmarkManager.Instance.GetLandmarkTileSprites(this, landmarkOnTile.specificLandmarkType, race);
        if (landmarkTileSprites == null || landmarkTileSprites.Count == 0) {
            HideLandmarkTileSprites();
            landmarkOnTile.landmarkVisual.SetIconState(true);
            landmarkOnTile.SetLandmarkPortrait(landmarkData.defaultLandmarkPortrait);
        } else {
            LandmarkStructureSprite chosenAssets =
                UtilityScripts.CollectionUtilities.GetRandomElement(landmarkTileSprites); 
            SetLandmarkTileSprite(chosenAssets);
            landmarkOnTile.landmarkVisual.SetIconState(false);
            landmarkOnTile.SetLandmarkPortrait(chosenAssets.overrideLandmarkPortrait != null ? chosenAssets.overrideLandmarkPortrait : landmarkData.defaultLandmarkPortrait);
        }
        SetStructureTint(settlementOnTile?.owner?.factionColor ?? Color.white);
    }
    public BaseLandmark LoadLandmark(BaseLandmark landmark) {
        //Create Landmark Game Object on tile
        var landmarkGO = Instantiate(LandmarkManager.Instance.GetLandmarkGO(), structureParentGO.transform);
        landmarkGO.transform.localPosition = Vector3.zero;
        landmarkGO.transform.localScale = Vector3.one;
        landmarkOnTile = landmark;
        if (landmarkGO != null) {
            landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkVisual>());
        }
        return landmarkOnTile;
    }
    public void RemoveLandmarkOnTile() {
        landmarkOnTile = null;
    }
    public void RemoveLandmarkVisuals() {
        HideLandmarkTileSprites();
        Destroy(landmarkOnTile.landmarkVisual.gameObject);
    }
    public void SetLandmarkTileSprite(LandmarkStructureSprite sprites) {
        mainStructure.sprite = sprites.mainSprite;
        structureTint.sprite = sprites.tintSprite;
        mainStructure.gameObject.SetActive(true);
        structureTint.gameObject.SetActive(true);
        
        if (sprites.animation == null) {
            mainStructure.enabled = true;
            structureAnimation.gameObject.SetActive(false);
        } else {
            mainStructure.enabled = landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.MONSTER_LAIR; //SPECIAL CASE FOR MONSTER LAIR
            structureAnimation.gameObject.SetActive(true);
            structureAnimation.runtimeAnimatorController = sprites.animation;
        }
    }
    private void HideLandmarkTileSprites() {
        mainStructure.gameObject.SetActive(false);
        structureTint.gameObject.SetActive(false);
    }
    public void SetStructureTint(Color color) {
        structureTint.color = color;
    }
    #endregion

    #region Tile Utilities
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
                    }
                }
                if (i == range - 1 && isOnlyOuter) {
                    return tilesToAdd;
                }
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
    public bool IsAtEdgeOfMap() {
        return AllNeighbours.Count < 6; //if this tile has less than 6 neighbours, it is at the edge of the map
    }
    public bool HasNeighbourAtEdgeOfMap() {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile currNeighbour = AllNeighbours[i];
            if (currNeighbour.IsAtEdgeOfMap()) {
                return true;
            }
        }
        return false;
    }
    public bool HasNeighbourFromOtherRegion() {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile currNeighbour = AllNeighbours[i];
            if (currNeighbour.region != region) {
                return true;
            }
        }
        return false;
    }
    public bool TryGetDifferentRegionNeighbours(out List<Region> regions) {
        regions = new List<Region>();
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile currNeighbour = AllNeighbours[i];
            if (currNeighbour.region != region) {
                regions.Add(currNeighbour.region);
            }
        }
        return regions.Count > 0;
    }
    public bool HasNeighbourWithElevation(ELEVATION elevation) {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile neighbour = AllNeighbours[i];
            if (neighbour.elevationType == elevation) {
                return true;
            }
        }
        return false;
    }
    public bool HasNeighbourWithFeature(string feature) {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile neighbour = AllNeighbours[i];
            if (neighbour.featureComponent.HasFeature(feature)) {
                return true;
            }
        }
        return false;
    }
    public bool HasOwnedSettlementNeighbour() {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile neighbour = AllNeighbours[i];
            if (neighbour.settlementOnTile?.owner != null) {
                return true;
            }
        }
        return false;
    }
    public string GetDisplayName() {
        if (settlementOnTile != null) {
            return settlementOnTile.name;
        } else if (landmarkOnTile != null) {
            return landmarkOnTile.landmarkName;
        } else {
            string displayName = string.Empty;
            if (isCorrupted) {
                displayName = "Corrupted ";
            }
            displayName +=
                $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(biomeType.ToString())} {UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(elevationType.ToString())}";
            return displayName;
        }
    }
    public string GetSubName() {
        if (landmarkOnTile != null) {
            return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(landmarkOnTile.specificLandmarkType.ToString());
        }
        return string.Empty;
    }
    #endregion

    #region Pathfinding
    public void FindNeighbours(HexTile[,] gameBoard) {
        _neighbourDirections = new Dictionary<HEXTILE_DIRECTION, HexTile>();
        var neighbours = new List<HexTile>();

        List<Point> possibleExits;

        if ((yCoordinate % 2) == 0) {
            possibleExits = UtilityScripts.Utilities.EvenNeighbours;
        } else {
            possibleExits = UtilityScripts.Utilities.OddNeighbours;
        }

        for (int i = 0; i < possibleExits.Count; i++) {
            int neighbourCoordinateX = xCoordinate + possibleExits[i].X;
            int neighbourCoordinateY = yCoordinate + possibleExits[i].Y;
            if (neighbourCoordinateX >= 0 && neighbourCoordinateX < gameBoard.GetLength(0) && neighbourCoordinateY >= 0 && neighbourCoordinateY < gameBoard.GetLength(1)) {
                HexTile currNeighbour = gameBoard[neighbourCoordinateX, neighbourCoordinateY];
                if (currNeighbour != null) {
                    neighbours.Add(currNeighbour);
                }
            }

        }
        AllNeighbours = neighbours;

        for (int i = 0; i < neighbours.Count; i++) {
            HexTile currNeighbour = neighbours[i];
            if (currNeighbour == null) {
                continue;
            }
            HEXTILE_DIRECTION dir = GetNeighbourDirection(currNeighbour);
            if (dir != HEXTILE_DIRECTION.NONE) {
                _neighbourDirections.Add(dir, currNeighbour);
            }
        }
    }
    public void FindNeighboursForBorders() {
        _neighbourDirections = new Dictionary<HEXTILE_DIRECTION, HexTile>();
        var neighbours = new List<HexTile>();

        List<Point> possibleExits;

        if ((yCoordinate % 2) == 0) {
            possibleExits = UtilityScripts.Utilities.EvenNeighbours;
        } else {
            possibleExits = UtilityScripts.Utilities.OddNeighbours;
        }

        for (int i = 0; i < possibleExits.Count; i++) {
            int neighbourCoordinateX = xCoordinate + possibleExits[i].X;
            int neighbourCoordinateY = yCoordinate + possibleExits[i].Y;
            HexTile neighbour = GridMap.Instance.GetTileFromCoordinates(neighbourCoordinateX, neighbourCoordinateY);
            if (neighbour != null) {
                neighbours.Add(neighbour);
            }

        }
        AllNeighbours = neighbours;

        for (int i = 0; i < neighbours.Count; i++) {
            HexTile currNeighbour = neighbours[i];
            if (currNeighbour == null) {
                continue;
            }
            HEXTILE_DIRECTION dir = GetNeighbourDirection(currNeighbour);
            if (dir != HEXTILE_DIRECTION.NONE) {
                _neighbourDirections.Add(dir, currNeighbour);
            }
        }
    }
    private HEXTILE_DIRECTION GetNeighbourDirection(HexTile neighbour) {
        if (neighbour == null) {
            return HEXTILE_DIRECTION.NONE;
        }
        if (!AllNeighbours.Contains(neighbour)) {
            throw new System.Exception($"{neighbour.name} is not a neighbour of {name}");
        }
        int thisXCoordinate = xCoordinate;
        int thisYCoordinate = yCoordinate;
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
    public HexTile GetNeighbour(HEXTILE_DIRECTION direction) {
        if (neighbourDirections.ContainsKey(direction)) {
            return neighbourDirections[direction];
        }
        return null;
    }
    #endregion

    #region Tile Visuals
    internal void SetSortingOrder(int sortingOrder, string sortingLayerName = "Default") {
        spriteRenderer.sortingOrder = sortingOrder;
        spriteRenderer.sortingLayerName = sortingLayerName;
        UpdateSortingOrder();
    }
    private void UpdateSortingOrder() {
        int sortingOrder = spriteRenderer.sortingOrder;
        _hoverHighlightSpriteRenderer.sortingOrder = sortingOrder + 1;
        _highlightGOSpriteRenderer.sortingOrder = sortingOrder + 1;

        topLeftBeach.sortingOrder = sortingOrder + 1;
        leftBeach.sortingOrder = sortingOrder + 1;
        botLeftBeach.sortingOrder = sortingOrder + 1;
        botRightBeach.sortingOrder = sortingOrder + 1;
        rightBeach.sortingOrder = sortingOrder + 1;
        topRightBeach.sortingOrder = sortingOrder + 1;

        topLeftBorder.sortingOrder = sortingOrder + 2;
        topRightBorder.sortingOrder = sortingOrder + 2;
        leftBorder.sortingOrder = sortingOrder + 2;
        botLeftBorder.sortingOrder = sortingOrder + 2;
        botRightBorder.sortingOrder = sortingOrder + 2;
        rightBorder.sortingOrder = sortingOrder + 2;

        emptyBuildingSpotGO.sortingOrder = sortingOrder + 1;

        mainStructure.sortingOrder = sortingOrder + 5;
        structureTint.sortingOrder = sortingOrder + 6;
        _structureAnimatorSpriteRenderer.sortingOrder = sortingOrder + 7;
    }
    public SpriteRenderer GetBorder(HEXTILE_DIRECTION direction) {
        SpriteRenderer border = null;
        switch (direction) {
            case HEXTILE_DIRECTION.NORTH_WEST:
                border = topLeftBorder;
                break;
            case HEXTILE_DIRECTION.NORTH_EAST:
                border = topRightBorder;
                break;
            case HEXTILE_DIRECTION.EAST:
                border = rightBorder;
                break;
            case HEXTILE_DIRECTION.SOUTH_EAST:
                border = botRightBorder;
                break;
            case HEXTILE_DIRECTION.SOUTH_WEST:
                border = botLeftBorder;
                break;
            case HEXTILE_DIRECTION.WEST:
                border = leftBorder;
                break;
            default:
                break;
        }
        return border;
    }
    internal void DeactivateCenterPiece() {
        centerPiece.SetActive(false);
    }
    internal void SetBaseSprite(Sprite baseSprite) {
        this.baseSprite = baseSprite;
        spriteRenderer.sprite = baseSprite;
        RuntimeAnimatorController _animation;
        if (Biomes.Instance.TryGetTileSpriteAnimation(baseSprite, out _animation)) {
            baseTileAnimator.runtimeAnimatorController = _animation;
            baseTileAnimator.enabled = true;
        } else {
            baseTileAnimator.enabled = false;
        }
    }
    public void SetBordersState(bool state, bool glowState, Color color) {
        topLeftBorder.gameObject.SetActive(state);
        botLeftBorder.gameObject.SetActive(state);
        topRightBorder.gameObject.SetActive(state);
        botRightBorder.gameObject.SetActive(state);
        leftBorder.gameObject.SetActive(state);
        rightBorder.gameObject.SetActive(state);
        
        topLeftBorderGlow.enabled = glowState;
        botLeftBorderGlow.enabled = glowState;
        topRightBorderGlow.enabled = glowState;
        botRightBorderGlow.enabled = glowState;
        leftBorderGlow.enabled = glowState;
        rightBorderGlow.enabled = glowState;
        
        SetBorderColor(color);
    }
    private void SetBorderColor(Color color) {
        topLeftBorder.color = color;
        botLeftBorder.color = color;
        topRightBorder.color = color;
        botRightBorder.color = color;
        leftBorder.color = color;
        rightBorder.color = color;
        
        topLeftBorderGlow.GlowColor = color;
        botLeftBorderGlow.GlowColor = color;
        topRightBorderGlow.GlowColor = color;
        botRightBorderGlow.GlowColor = color;
        leftBorderGlow.GlowColor = color;
        rightBorderGlow.GlowColor = color;
    }
    public void UpdateBuildSprites() {
        if (region.demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE) {
            //currently building at tile.
            currentlyBuildingSpotGO.gameObject.SetActive(true);
            emptyBuildingSpotGO.gameObject.SetActive(false);
        } else {
            emptyBuildingSpotGO.gameObject.SetActive(true);
            currentlyBuildingSpotGO.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Tile Functions
    public void DisableColliders() {
        GetComponent<Collider2D>().enabled = false;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = false;
        }
    }
    private InfoUIBase GetMenuToShowWhenTileIsClicked() {
        if (region != null) {
            //if region info ui is showing, show tile info ui
            if (UIManager.Instance.regionInfoUI.isShowing) {
                if (UIManager.Instance.regionInfoUI.activeRegion == region) {
                    return UIManager.Instance.hexTileInfoUI;    
                } else {
                    return UIManager.Instance.regionInfoUI;
                }
            } else if (UIManager.Instance.hexTileInfoUI.isShowing) {
                if (UIManager.Instance.hexTileInfoUI.currentlyShowingHexTile.region == region) {
                    if (UIManager.Instance.hexTileInfoUI.currentlyShowingHexTile == this) {
                        return UIManager.Instance.regionInfoUI;
                    } else {
                        return UIManager.Instance.hexTileInfoUI;
                    }
                } else {
                    return UIManager.Instance.regionInfoUI;    
                }
            } else {
                return UIManager.Instance.regionInfoUI;
            }
        }
        return null;
    }
    #endregion

    #region Monobehaviour Functions
    private void LeftClick() {
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing() || CameraMove.Instance.isDragging) {
            return;
        }
        Messenger.Broadcast(Signals.TILE_LEFT_CLICKED, this);
        InfoUIBase baseToShow = GetMenuToShowWhenTileIsClicked();
        if (baseToShow != null) {
            if (baseToShow is RegionInfoUI) {
                Messenger.Broadcast(Signals.REGION_SELECTED, region);
                UIManager.Instance.ShowRegionInfo(region);
            } else if (baseToShow is HextileInfoUI) {
                UIManager.Instance.ShowHexTileInfo(this);
            }
        }
        MouseOver();
    }
    private void RightClick() {
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        Messenger.Broadcast(Signals.TILE_RIGHT_CLICKED, this);
    }
    private void MouseOver() {
        InfoUIBase baseToOpen = GetMenuToShowWhenTileIsClicked();
        if (baseToOpen is RegionInfoUI) {
            region.ShowBorders(Color.red);
        } else if (baseToOpen is HextileInfoUI) {
            SetBordersState(true, false, Color.red);
        }
        Messenger.Broadcast(Signals.TILE_HOVERED_OVER, this);
    }
    private void MouseExit() {
        InfoUIBase baseToOpen = GetMenuToShowWhenTileIsClicked();
        if (baseToOpen is RegionInfoUI) {
            region.HideBorders();
        } else if (baseToOpen is HextileInfoUI) {
            SetBordersState(false, false, Color.red);
        }
        Messenger.Broadcast(Signals.TILE_HOVERED_OUT, this);
    }
    private void DoubleLeftClick() {
        if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing()) {
            return;
        }
        if (region != null) {
            InnerMapManager.Instance.TryShowLocationMap(region);
            InnerMapCameraMove.Instance.CenterCameraOnTile(this);
        }
        Messenger.Broadcast(Signals.TILE_DOUBLE_CLICKED, this);
    }
    public void PointerClick(BaseEventData bed) {
        PointerEventData ped = bed as PointerEventData;
        if (ped.clickCount >= 2) {
            if (ped.button == PointerEventData.InputButton.Left) {
                DoubleLeftClick();
            }
        } else if (ped.clickCount == 1) {
            if (ped.button == PointerEventData.InputButton.Left) {
                LeftClick();
            } else if (ped.button == PointerEventData.InputButton.Right) {
                RightClick();
            }
        } 
    }
    public void OnPointerEnter(BaseEventData bed) {
        PointerEventData ped = bed as PointerEventData;
        if (ped.pointerCurrentRaycast.gameObject.CompareTag("Avatar")) {
            OnPointerExit(bed);
            return;
        }
        MouseOver();
    }
    public void OnPointerExit(BaseEventData bed) {
        MouseExit();
    }
    public void CenterCameraHere() {
        if (InnerMapManager.Instance.isAnInnerMapShowing) {
            InnerMapManager.Instance.HideAreaMap();
            UIManager.Instance.OnCameraOutOfFocus();
        }
        CameraMove.Instance.CenterCameraOn(gameObject);
    }
    #endregion

    #region For Testing
    [Space(10)]
    [Header("For Testing")]
    [SerializeField] private int range = 0;
    List<HexTile> tiles = new List<HexTile>();
    private SpriteRenderer _hoverHighlightSpriteRenderer;
    private SpriteRenderer _highlightGOSpriteRenderer;
    private SpriteRenderer _structureAnimatorSpriteRenderer;
    private SpriteRenderer _highlightSpriteRenderer;
    [ContextMenu("Show Tiles In Range")]
    public void ShowTilesInRange() {
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].spriteRenderer.color = Color.white;
        }
        tiles.Clear();
        tiles.AddRange(GetTilesInRange(range));
        for (int i = 0; i < tiles.Count; i++) {
            tiles[i].spriteRenderer.color = Color.magenta;
        }
    }
    public override string ToString() {
        return $"{locationName} - {landmarkOnTile?.specificLandmarkType.ToString() ?? "No Landmark"} - {region?.name ?? "No Region"}";
    }
    public void ShowTileInfo() {
        string summary = $"{ToString()}";
        summary += $"\nLeft Most: {(region.GetLeftMostTile()?.ToString() ?? "Null")}";
        summary += $"\nRight Most: {(region.GetRightMostTile()?.ToString() ?? "Null")}";
        summary += "\nFeatures:";
        for (int i = 0; i < featureComponent.features.Count; i++) {
            TileFeature feature = featureComponent.features[i];
            summary += $"{feature.name}, ";
        }
        summary += "\nLeft Most Rows:";
        List<int> leftMostRows = region.GetLeftMostRows();
        for (int i = 0; i < leftMostRows.Count; i++) {
            summary += $"{leftMostRows[i].ToString()}, ";
        }
        summary += "\nRight Most Rows:";
        List<int> rightMostRows = region.GetRightMostRows();
        for (int i = 0; i < rightMostRows.Count; i++) {
            summary += $"{rightMostRows[i].ToString()}, ";
        }
        UIManager.Instance.ShowSmallInfo(summary);
        
    }
    #endregion

    #region Corruption
    public void SetCorruption(bool state) {
        if(_isCorrupted != state) {
            _isCorrupted = state;
            Biomes.Instance.UpdateTileSprite(this, spriteRenderer.sortingOrder);
            //for (int i = 0; i < AllNeighbours.Count; i++) {
            //    HexTile neighbour = AllNeighbours[i];
            //    if (neighbour.isCorrupted == false && neighbour.isCurrentlyBeingCorrupted == false) {
            //        neighbour.CheckForCorruptAction();
            //    }
            //}
        }
    }
    public void AdjustUncorruptibleLandmarkNeighbors(int amount) {
        _uncorruptibleLandmarkNeighbors += amount;
        if(_uncorruptibleLandmarkNeighbors < 0) {
            _uncorruptibleLandmarkNeighbors = 0;
        }
        if(_uncorruptibleLandmarkNeighbors > 1 && landmarkOnTile != null) {
            _uncorruptibleLandmarkNeighbors = 1;
        }
    }
    private bool CanBeCorrupted() {
        if (isCorrupted) {
            return false; //already corrupted.
        }
        if (isCurrentlyBeingCorrupted) {
            return false; //already being corrupted.
        }
        if (settlementOnTile != null) {
            return false; //disabled corruption of NPC settlements for now.
        }
        if (PlayerManager.Instance.player.mana < EditableValuesManager.Instance.corruptTileManaCost) {
            return false;
        }
        //TODO: Add checking for if this tile has any structure blueprint on it.
        //if it has any build spots that have a blueprint on them, do not allow
        // for (int i = 0; i < ownedBuildSpots.Length; i++) {
        //     BuildingSpot spot = ownedBuildSpots[i];
        //     if (spot.hasBlueprint) {
        //         return false;
        //     }
        // }
        return true;
        //for (int i = 0; i < AllNeighbours.Count; i++) {
        //    HexTile neighbour = AllNeighbours[i];
        //    if (neighbour.isCorrupted) {
        //        return true;
        //    }
        //}
        //return false;
    }
    public void StartCorruption() {
        //PlayerManager.Instance.player.AdjustMana(-EditableValuesManager.Instance.corruptTileManaCost);
        InstantlyCorruptAllOwnedInnerMapTiles();
        OnCorruptSuccess();
    }
    public void InstantlyCorruptAllOwnedInnerMapTiles() {
        for (int i = 0; i < locationGridTiles.Count; i++) {
            LocationGridTile tile = locationGridTiles[i];
            tile.CorruptTile();
        }
    }
    private HexTile GetCorruptedNeighbour() {
        for (int i = 0; i < AllNeighbours.Count; i++) {
            HexTile tile = AllNeighbours[i];
            if (tile.isCorrupted) {
                return tile;
            }
        }
        return null;
    }
    private void OnCorruptSuccess() {
        PlayerManager.Instance.player.playerSettlement.AddTileToSettlement(this);
        // Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCorruption);
        // isCurrentlyBeingCorrupted = false;
        
        //remove features
        featureComponent.RemoveAllFeatures(this);
        
        //RemovePlayerAction(GetPlayerAction(PlayerDB.Corrupt_Action));
        //if (CanBuildDemonicStructure()) {
        //    PlayerAction buildAction = new PlayerAction(PlayerDB.Build_Demonic_Structure_Action, CanBuildDemonicStructure, null, OnClickBuild);
        //    AddPlayerAction(buildAction);
        //}
    }
    #endregion

    #region NPCSettlement
    public void SetSettlementOnTile(BaseSettlement settlement) {
        settlementOnTile = settlement;
        landmarkOnTile?.nameplate.UpdateVisuals();
    }
    #endregion

    #region Pathfinding
    public TravelLine CreateTravelLine(HexTile target, int numOfTicks, Character character) {
        TravelLineParent lineParent = BezierCurveManager.Instance.GetTravelLineParent(this, target);
        if (lineParent == null) {
            GameObject goParent = Instantiate(GameManager.Instance.travelLineParentPrefab);
            lineParent = goParent.GetComponent<TravelLineParent>();
            lineParent.SetStartAndEndPositions(this, target, numOfTicks);
        }
        GameObject go = Instantiate(GameManager.Instance.travelLinePrefab, lineParent.transform);
        go.transform.SetParent(lineParent.transform);
        TravelLine travelLine = go.GetComponent<TravelLine>();
        travelLine.SetCharacter(character);
        lineParent.AddChild(travelLine);

        TravelLineParent targetLineParent = BezierCurveManager.Instance.GetTravelLineParent(target, this);
        if (targetLineParent != null) {
            targetLineParent.transform.localPosition = new Vector3(0f, 0.3f, 0f);
        }
        return travelLine;
    }
    #endregion

    #region Beaches
    public void LoadBeaches() {
        if (_neighbourDirections == null) {
            return;
        }
        if (elevationType != ELEVATION.WATER) {
            topLeftBeach.gameObject.SetActive(false);
            topRightBeach.gameObject.SetActive(false);
            rightBeach.gameObject.SetActive(false);
            botRightBeach.gameObject.SetActive(false);
            botLeftBeach.gameObject.SetActive(false);
            leftBeach.gameObject.SetActive(false);
            return;
        }
        foreach (KeyValuePair<HEXTILE_DIRECTION, HexTile> kvp in _neighbourDirections) {
            bool beachState;
            if (kvp.Value != null && kvp.Value.elevationType != ELEVATION.WATER) {
                beachState = true;
            } else {
                beachState = false;
            }
            switch (kvp.Key) {
                case HEXTILE_DIRECTION.NORTH_WEST:
                    topLeftBeach.gameObject.SetActive(beachState);
                    break;
                case HEXTILE_DIRECTION.NORTH_EAST:
                    topRightBeach.gameObject.SetActive(beachState);
                    break;
                case HEXTILE_DIRECTION.EAST:
                    rightBeach.gameObject.SetActive(beachState);
                    break;
                case HEXTILE_DIRECTION.SOUTH_EAST:
                    botRightBeach.gameObject.SetActive(beachState);
                    break;
                case HEXTILE_DIRECTION.SOUTH_WEST:
                    botLeftBeach.gameObject.SetActive(beachState);
                    break;
                case HEXTILE_DIRECTION.WEST:
                    leftBeach.gameObject.SetActive(beachState);
                    break;
                case HEXTILE_DIRECTION.NONE:
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

    #region Region
    public void SetRegion(Region region) {
        this.region = region;
    }
    #endregion

    #region Inner Map
    public void SetInnerMapHexTileData(InnerMapHexTile _innerMapHexTile) {
        innerMapHexTile = _innerMapHexTile;
        Assert.IsNotNull(innerMapHexTile.gridTileCollections, $"InnerMapHexTile data for {this} does not have location grid tile collections!");
        locationGridTiles = new List<LocationGridTile>();
        for (int i = 0; i < innerMapHexTile.gridTileCollections.Length; i++) {
            LocationGridTileCollection collection = innerMapHexTile.gridTileCollections[i];
            locationGridTiles.AddRange(collection.tilesInTerritory);
        }
        PopulateBorderTiles();
    }
    private void PopulateBorderTiles() {
        borderTiles = new List<LocationGridTile>();
        InnerTileMap tileMap = region.innerMap;
        //To populate border tiles we need to know the width and height of hextile in the inner map, which is currently InnerMapManager.BuildingSpotSize x 2
        int hexWidth = InnerMapManager.BuildingSpotSize.x * 2;
        int hexHeight = InnerMapManager.BuildingSpotSize.y * 2;

        //Our origin point will always be the first entry in the locationGridTiles list, assuming that the first entry is always the lower left corner of the hex tile
        int originX = locationGridTiles[0].localPlace.x;
        int originY = locationGridTiles[0].localPlace.y;

        //Let's get the actual width and height from the origin point
        int actualHeight = originY + (hexHeight - 1);
        int actualWidth = originX + (hexWidth - 1);

        //Now, to calculate border tiles, we will simply add the origin points and the hex width and height and loop through all the tiles corresponding those points
        //There are four sides to the borders since the hex tile in the inner map is a square, we will call it A - left side, B - up side, C - right side, and D - down side
        
        //To get A, we must increment from originY, while the originX is constant
        for (int i = originY; i < actualHeight; i++) {
            borderTiles.Add(tileMap.map[originX, i]);
        }
        //To get B, we must increment from originX, while actualHeight is constant
        for (int i = originX; i < actualWidth; i++) {
            borderTiles.Add(tileMap.map[i, actualHeight]);
        }
        //To get C, we must increment from originY, while actualWidth is constant
        for (int i = originY; i <= actualHeight; i++) {
            borderTiles.Add(tileMap.map[actualWidth, i]);
        }
        //To get D, we must increment from originX, while originY is constant
        for (int i = originX + 1; i < actualWidth; i++) {
            borderTiles.Add(tileMap.map[i, originY]);
        }

        //IMPORTANT NOTE BELOW! DO NOT DELETE COMMENT!
        //Let's check using an example, if the origin point is (0, 0) and the actual width = 7, and the actual height = 7
        //Then A = (0, 0) to (0, 6)
        //B = (0, 7) to (6, 7)
        //C = (7, 0) to (7, 7)
        //D = (1, 0) to (6, 0)

    }
    public List<TileObject> GetTileObjectsInHexTile(TILE_OBJECT_TYPE type) {
        List<TileObject> tileObjects = new List<TileObject>();
        for (int i = 0; i < locationGridTiles.Count; i++) {
            LocationGridTile tile = locationGridTiles[i];
            if (tile.objHere is TileObject && (tile.objHere as TileObject).tileObjectType == type) {
                tileObjects.Add(tile.objHere as TileObject);
            }
        }
        return tileObjects;
    }
    #endregion

    #region Listeners
    private void SubscribeListeners() {    
        Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
        Messenger.AddListener<LocationStructure, InnerMapHexTile>(Signals.STRUCTURE_OBJECT_REMOVED, OnStructureRemoved);
    }
    private void OnStructurePlaced(LocationStructure structure) {
        if (innerMapHexTile != null && innerMapHexTile == structure.occupiedHexTile) {
            CheckIfStructureVisualsAreStillValid();
        }
    }
    private void OnStructureRemoved(LocationStructure structure, InnerMapHexTile removedFrom) {
        if (innerMapHexTile != null && innerMapHexTile == removedFrom) {
            CheckIfStructureVisualsAreStillValid();
        }
    }
    public LocationStructure GetMostImportantStructureOnTile() {
        LocationStructure mostImportant = region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        foreach (KeyValuePair<STRUCTURE_TYPE,List<LocationStructure>> pair in region.structures) {
            for (int i = 0; i < pair.Value.Count; i++) {
                LocationStructure structure = pair.Value[i];
                if (structure.occupiedHexTile != null && structure.occupiedHexTile == innerMapHexTile) {
                    int value = pair.Key.StructurePriority(); 
                    if (value > mostImportant.structureType.StructurePriority()) {
                        mostImportant = structure;
                    }    
                }
            }
        }
        
        return mostImportant;
    }
    private void CheckIfStructureVisualsAreStillValid() {
        string log = $"Checking {ToString()} to check if landmark on it is still valid";
        STRUCTURE_TYPE mostImportantStructure = GetMostImportantStructureOnTile().structureType;
        log += $"\nMost important structure is {mostImportantStructure.ToString()}";
        if (mostImportantStructure == STRUCTURE_TYPE.WILDERNESS) {
            log += $"\nWill destroy existing landmark {landmarkOnTile?.ToString() ?? "Null"}";
            LandmarkManager.Instance.DestroyLandmarkOnTile(this);
        } else {
            LANDMARK_TYPE landmarkType = mostImportantStructure.GetLandmarkType();
            log += $"\nLandmark to create is {landmarkType.ToString()}";
            if (landmarkOnTile == null) {
                LandmarkManager.Instance.CreateNewLandmarkOnTile(this, landmarkType);
            } else {
                if (landmarkOnTile.specificLandmarkType != landmarkType) {
                    landmarkOnTile.ChangeLandmarkType(landmarkType);    
                }
            }    
        }
        
        
        Debug.Log(log);
    }
    #endregion
    
    #region Player Action Target
    public List<SPELL_TYPE> actions { get; private set; }
    public void ConstructDefaultActions() {
        actions = new List<SPELL_TYPE>();
        //PlayerAction harassAction = new PlayerAction(PlayerDB.Harass_Action, CanDoHarass, IsHarassRaidInvadeValid, () => PlayerUI.Instance.OnClickHarassRaidInvade(this, "harass"));
        //PlayerAction raidAction = new PlayerAction(PlayerDB.Raid_Action, CanDoRaid, IsHarassRaidInvadeValid, () => PlayerUI.Instance.OnClickHarassRaidInvade(this, "raid"));
        //PlayerAction invadeAction = new PlayerAction(PlayerDB.Invade_Action, CanDoInvade, IsHarassRaidInvadeValid, () => PlayerUI.Instance.OnClickHarassRaidInvade(this, "invade"));
        //PlayerAction buildAction = new PlayerAction(PlayerDB.Build_Demonic_Structure_Action, () => true, CanBuildDemonicStructure, OnClickBuild);

        AddPlayerAction(SPELL_TYPE.HARASS);
        //AddPlayerAction(SPELL_TYPE.DEFEND);
        AddPlayerAction(SPELL_TYPE.INVADE);
        AddPlayerAction(SPELL_TYPE.BUILD_DEMONIC_STRUCTURE);
    }
    public void AddPlayerAction(SPELL_TYPE action) {
        if (actions.Contains(action) == false) {
            actions.Add(action);
            Messenger.Broadcast(Signals.PLAYER_ACTION_ADDED_TO_TARGET, action, this as IPlayerActionTarget);    
        }
    }
    public void RemovePlayerAction(SPELL_TYPE action) {
        if (actions.Remove(action)) {
            Messenger.Broadcast(Signals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
        }
    }
    public void ClearPlayerActions() {
        actions.Clear();
    }
    #endregion

    #region Demonic Structure Building
    public bool CanBuildDemonicStructure() {
        //Cannot build on settlements and hextiles with blueprints right now
        if(/*isCorrupted && isCurrentlyBeingCorrupted == false &&*/ settlementOnTile == null && landmarkOnTile == null 
               && elevationType != ELEVATION.WATER && elevationType != ELEVATION.MOUNTAIN &&
            PlayerManager.Instance.player.mana >= EditableValuesManager.Instance.buildStructureManaCost && _buildParticles == null) {

            //TODO:
            // //if it has any build spots that have a blueprint on them, do not allow
            // for (int i = 0; i < ownedBuildSpots.Length; i++) {
            //     BuildingSpot spot = ownedBuildSpots[i];
            //     if (spot.hasBlueprint) {
            //         return false;
            //     }
            // }
            return true;
        }
        return false;
    }
    public void OnClickBuild() {
        //demonicLandmarksThatCanBeBuilt.Clear();
        //List<LANDMARK_TYPE> demonicLandmarkTypes = ;
        //for (int i = 0; i < demonicLandmarkTypes.Count; i++) {
        //    demonicLandmarksThatCanBeBuilt.Add(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(demonicLandmarkTypes[i].ToString()));
        //}
        UIManager.Instance.ShowClickableObjectPicker(PlayerManager.Instance.player.playerSkillComponent.demonicStructuresSkills
            , StartBuildConfirmation, null, CanChooseLandmark, "Choose a structure to build"
            , OnHoverStructureChoice, OnHoverExitStructureChoice, portraitGetter: GetStructurePortrait, shouldConfirmOnPick: true, asButton: true, identifier: "Demonic Structure");
    }
    private bool CanChooseLandmark(SPELL_TYPE structureType) {
        if (WorldConfigManager.Instance.isDemoWorld) {
            return WorldConfigManager.Instance.availableSpellsInDemoBuild.Contains(structureType);
        } else {
            if (structureType == SPELL_TYPE.THE_EYE && region.HasStructure(STRUCTURE_TYPE.THE_EYE)) {
                return false; //only 1 eye per region.
            }
            if (structureType == SPELL_TYPE.THE_GOADER && PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.THE_GOADER)) {
                return false; //only 1 finger at a time.
            }    
        }
        
        return true;
    }
    private void OnHoverStructureChoice(SPELL_TYPE structureType) {
        //string landmarkName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString());
        //LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkName);
        DemonicStructurePlayerSkill demonicStructureSkill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(structureType);
        //string info = demonicStructureSkill.description + "\n" + demonicStructureSkill.GetManaCostChargesCooldownStr();
        //UIManager.Instance.ShowSmallInfo(info);

        PlayerUI.Instance.OnHoverSpell(demonicStructureSkill);

        //if (info != string.Empty) {
        //    info += "\n";
        //}
        //info += $"Duration: {GameManager.Instance.GetCeilingHoursBasedOnTicks(landmarkData.buildDuration).ToString()} hours";
    }
    private void OnHoverExitStructureChoice(SPELL_TYPE structureType) {
        //UIManager.Instance.HideSmallInfo();
        PlayerUI.Instance.OnHoverOutSpell(null);
    }
    private Sprite GetStructurePortrait(string name) {
        string landmarkName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(name);
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkName);
        return landmarkData.defaultLandmarkPortrait;
    }

    private void StartBuildConfirmation(object structureObj) {
        SPELL_TYPE structureType = (SPELL_TYPE) structureObj;
        string landmarkName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString());
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkName);
        UIManager.Instance.ShowYesNoConfirmation("Build Structure Confirmation", "Are you sure you want to build " + landmarkName + "?", () => StartBuild(structureType));
    }
    private AutoDestroyParticle _buildParticles;
    private void StartBuild(SPELL_TYPE structureType) {
        _buildParticles = GameManager.Instance.CreateParticleEffectAt(GetCenterLocationGridTile(),
            PARTICLE_EFFECT.Build_Demonic_Structure).GetComponent<AutoDestroyParticle>();
        UIManager.Instance.HideObjectPicker();
        StartCoroutine(BuildCoroutine(structureType));
    }
    private IEnumerator BuildCoroutine(SPELL_TYPE structureType) {
        yield return new WaitForSeconds(3f);
        _buildParticles.StopEmission();
        DemonicStructurePlayerSkill demonicStructureSkill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(structureType);
        demonicStructureSkill.ActivateAbility(this);
        PlayerSkillManager.Instance.GetPlayerActionData(SPELL_TYPE.BUILD_DEMONIC_STRUCTURE).OnExecuteSpellActionAffliction();
        _buildParticles = null;
    }
    #endregion

    #region Border Tester
    [Header("Border Tester")]
    [SerializeField] private LineRenderer borderLine;
    [SerializeField] private Transform[] vertices;
    public Transform[] GetVertices(HEXTILE_DIRECTION direction) {
        Transform[] _vertices = new Transform[2];
        switch (direction) {
            case HEXTILE_DIRECTION.NORTH_WEST:
                _vertices[0] = vertices[1];
                _vertices[1] = vertices[0];
                break;
            case HEXTILE_DIRECTION.NORTH_EAST:
                _vertices[0] = vertices[5];
                _vertices[1] = vertices[0];
                break;
            case HEXTILE_DIRECTION.EAST:
                _vertices[0] = vertices[5];
                _vertices[1] = vertices[4];
                break;
            case HEXTILE_DIRECTION.SOUTH_EAST:
                _vertices[0] = vertices[4];
                _vertices[1] = vertices[3];
                break;
            case HEXTILE_DIRECTION.SOUTH_WEST:
                _vertices[0] = vertices[3];
                _vertices[1] = vertices[2];
                break;
            case HEXTILE_DIRECTION.WEST:
                _vertices[0] = vertices[2];
                _vertices[1] = vertices[1];
                break;
        }
        return _vertices;
    }
    #endregion
    
    #region Selectable
    public bool IsCurrentlySelected() {
        return UIManager.Instance.hexTileInfoUI.isShowing &&
               UIManager.Instance.hexTileInfoUI.currentlyShowingHexTile == this;
    }
    public void LeftSelectAction() {
        UIManager.Instance.ShowHexTileInfo(this);
    }
    public void RightSelectAction() {
        //Nothing happens
    }
    public bool CanBeSelected() {
        return true;
    }
    #endregion
    
    #region POI
    public void OnPlacePOIInHex(IPointOfInterest poi) {
        spellsComponent.OnPlacePOIInHex(poi);
    }
    public void OnRemovePOIInHex(IPointOfInterest poi) {
        spellsComponent.OnRemovePOIInHex(poi);
    }
    #endregion

    #region Characters
    public List<Character> GetAllCharactersInsideHex() {
        List<Character> characters = null;
        LocationGridTile lowerLeftCornerTile = innerMapHexTile.gridTileCollections[0].tilesInTerritory[0];
        int xMin = lowerLeftCornerTile.localPlace.x;
        int yMin = lowerLeftCornerTile.localPlace.y;
        int xMax = xMin + (InnerMapManager.BuildingSpotSize.x * 2);
        int yMax = yMin + (InnerMapManager.BuildingSpotSize.y * 2);

        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character character = region.charactersAtLocation[i];
            if (character.gridTileLocation.localPlace.x >= xMin && character.gridTileLocation.localPlace.x <= xMax
                && character.gridTileLocation.localPlace.y >= yMin && character.gridTileLocation.localPlace.y <= yMax) {
                if (characters == null) { characters = new List<Character>(); }
                characters.Add(character);
            }
        }
        return characters;
    }
    public List<T> GetAllCharactersInsideHex<T>() where T : Character {
        List<T> characters = null;
        LocationGridTile lowerLeftCornerTile = innerMapHexTile.gridTileCollections[0].tilesInTerritory[0];
        int xMin = lowerLeftCornerTile.localPlace.x;
        int yMin = lowerLeftCornerTile.localPlace.y;
        int xMax = xMin + (InnerMapManager.BuildingSpotSize.x * 2);
        int yMax = yMin + (InnerMapManager.BuildingSpotSize.y * 2);

        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character character = region.charactersAtLocation[i];
            if (character.gridTileLocation.localPlace.x >= xMin && character.gridTileLocation.localPlace.x <= xMax
                && character.gridTileLocation.localPlace.y >= yMin && character.gridTileLocation.localPlace.y <= yMax) {
                if (character is T converted) {
                    if (characters == null) { characters = new List<T>(); }
                    characters.Add(converted);    
                }
            }
        }
        return characters;
    }
    public LocationGridTile GetCenterLocationGridTile() {
        LocationGridTile lowerLeftCornerTile = innerMapHexTile.gridTileCollections[0].tilesInTerritory[0];
        int xMin = lowerLeftCornerTile.localPlace.x;
        int yMin = lowerLeftCornerTile.localPlace.y;
        int xMax = xMin + InnerMapManager.BuildingSpotSize.x;
        int yMax = yMin + InnerMapManager.BuildingSpotSize.y;
        return region.innerMap.map[xMax, yMax];
    }
    #endregion

    #region Biome
    public void ChangeBiomeType(BIOMES biomeType) {
        SetBiome(biomeType);
        Biomes.Instance.UpdateTileVisuals(this);
        ChangeGridTilesBiome();
    }
    private void ChangeGridTilesBiome() {
        for (int i = 0; i < locationGridTiles.Count; i++) {
            LocationGridTile currTile = locationGridTiles[i];
            Vector3Int position = currTile.localPlace;
            TileBase groundTile = InnerTileMap.GetGroundAssetPerlin(currTile.floorSample, biomeType);
            if (currTile.structure.isInterior || currTile.isCorrupted) {
                //set the previous tile to the new biome, so that when the structure is destroyed
                //it will revert to the right asset
                currTile.SetPreviousGroundVisual(groundTile);
            } else {
                currTile.SetPreviousGroundVisual(null);
                currTile.parentMap.groundTilemap.SetTile(position, groundTile);
                currTile.UpdateGroundTypeBasedOnAsset();
                if (currTile.objHere != null && currTile.objHere.mapObjectVisual && currTile.objHere is TileObject tileObject) {
                    tileObject.mapVisual.UpdateTileObjectVisual(tileObject);
                }
                currTile.CreateSeamlessEdgesForSelfAndNeighbours();    
            }
        }
    }
    public void GradualChangeBiomeType(BIOMES biomeType, System.Action onFinishChangeAction) {
        SetBiome(biomeType);
        Biomes.Instance.UpdateTileVisuals(this);
        StartCoroutine(ChangeGridTilesBiomeCoroutine(onFinishChangeAction));
    }
    private IEnumerator ChangeGridTilesBiomeCoroutine(System.Action onFinishChangeAction) {
        // List<LocationGridTile> gridTiles = new List<LocationGridTile>(locationGridTiles);
        // gridTiles = UtilityScripts.CollectionUtilities.Shuffle(gridTiles);
        for (int i = 0; i < locationGridTiles.Count; i++) {
            LocationGridTile currTile = locationGridTiles[i];
            Vector3Int position = currTile.localPlace;
            TileBase groundTile = InnerTileMap.GetGroundAssetPerlin(currTile.floorSample, biomeType);
            if (currTile.structure.isInterior || currTile.isCorrupted) {
                //set the previous tile to the new biome, so that when the structure is destroyed
                //it will revert to the right asset
                currTile.SetPreviousGroundVisual(groundTile);
            } else {
                currTile.SetPreviousGroundVisual(null);
                currTile.parentMap.groundTilemap.SetTile(position, groundTile);
                currTile.UpdateGroundTypeBasedOnAsset();
                if (currTile.objHere != null && currTile.objHere.mapObjectVisual && currTile.objHere is TileObject tileObject) {
                    tileObject.mapVisual.UpdateTileObjectVisual(tileObject);
                }
                currTile.CreateSeamlessEdgesForSelfAndNeighbours();
            }
            yield return null;
        }
        onFinishChangeAction.Invoke();
    } 
    #endregion

    #region Defend
    public void IncreaseIsBeingDefendedCount() {
        _isBeingDefendedCount++;
    }
    public void DecreaseIsBeingDefendedCount() {
        _isBeingDefendedCount--;
    }
    #endregion
}
