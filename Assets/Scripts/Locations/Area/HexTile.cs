// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using System;
// using PathFind;
// using Inner_Maps;
// using Inner_Maps.Location_Structures;
// using JetBrains.Annotations;
// using Locations.Settlements;
// using Locations.Area_Features;
// using Ruinarch;
// using SpriteGlow;
// using UnityEngine.Assertions;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
// using UnityEngine.Tilemaps;
// using UtilityScripts;
// using Locations;
//
// public class HexTile : BaseMonoBehaviour, IHasNeighbours<HexTile>, IPlayerActionTarget, ISelectable, IPartyTargetDestination, ILocation {
//
//     public HexTileData data;
//     private NPCSettlement _npcSettlementOfTile;
//     public SpriteRenderer spriteRenderer;
//     private bool _isCorrupted = false;
//
//     [Space(10)]
//     [Header("Tile Visuals")]
//     [SerializeField] private GameObject _centerPiece;
//     [SerializeField] private GameObject _highlightGO;
//     [SerializeField] private GameObject _hoverHighlightGO;
//     [SerializeField] private Animator baseTileAnimator;
//     [SerializeField] private SpriteRenderer emptyBuildingSpotGO;
//     [SerializeField] private SpriteRenderer currentlyBuildingSpotGO;
//
//     [Space(10)]
//     [Header("Tile Borders")]
//     [SerializeField] private SpriteRenderer topLeftBorder;
//     [SerializeField] private SpriteRenderer leftBorder;
//     [SerializeField] private SpriteRenderer botLeftBorder;
//     [SerializeField] private SpriteRenderer botRightBorder;
//     [SerializeField] private SpriteRenderer rightBorder;
//     [SerializeField] private SpriteRenderer topRightBorder;
//     [SerializeField] private SpriteGlowEffect topLeftBorderGlow;
//     [SerializeField] private SpriteGlowEffect leftBorderGlow;
//     [SerializeField] private SpriteGlowEffect botLeftBorderGlow;
//     [SerializeField] private SpriteGlowEffect botRightBorderGlow;
//     [SerializeField] private SpriteGlowEffect rightBorderGlow;
//     [SerializeField] private SpriteGlowEffect topRightBorderGlow;
//
//     [Space(10)]
//     [Header("Structure Objects")]
//     [SerializeField] private GameObject structureParentGO;
//     [SerializeField] private SpriteRenderer mainStructure;
//     [SerializeField] private SpriteRenderer structureTint;
//     [SerializeField] private Animator structureAnimation;
//
//     [Space(10)]
//     [Header("Beaches")]
//     [SerializeField] private SpriteRenderer topLeftBeach;
//     [SerializeField] private SpriteRenderer leftBeach;
//     [SerializeField] private SpriteRenderer botLeftBeach;
//     [SerializeField] private SpriteRenderer botRightBeach;
//     [SerializeField] private SpriteRenderer rightBeach;
//     [SerializeField] private SpriteRenderer topRightBeach;
//     
//     [Space(10)]
//     [Header("Colliders")]
//     [SerializeField] private Collider2D[] colliders;
//
//     public LocationAwareness locationAwareness { get; private set; }
//
//     //properties
//     public BaseLandmark landmarkOnTile { get; private set; }
//     public Region region { get; private set; }
//     public AreaFeatureComponent featureComponent { get; private set; }
//     public BaseSettlement settlementOnTile { get; private set; }
//     public List<HexTile> AllNeighbours { get; set; }
//     public List<HexTile> ValidTiles { get { return AllNeighbours.Where(o => o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN).ToList(); } }
//     public List<HexTile> ValidTilesWithinRegion { get { return AllNeighbours.Where(o => o.region == region && o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN).ToList(); } }
//     public List<HexTile> ValidTilesNoSettlementWithinRegion { get { return AllNeighbours.Where(o => o.settlementOnTile == null && o.region == region && o.elevationType != ELEVATION.WATER && o.elevationType != ELEVATION.MOUNTAIN).ToList(); } }
//     public bool isCurrentlyBeingCorrupted { get; private set; }
//     public LocationGridTile[] locationGridTiles => locationGridTileArray.tiles;
//     public List<LocationGridTile> borderTiles { get; private set; }
//     public Sprite baseSprite { get; private set; }
//     public Vector2 selectableSize { get; private set; }
//     public AreaItem areaItem { get; private set; }
//     public List<TileObject> itemsInHex { get; protected set; }
//     public RegionDivision regionDivision { get; protected set; }
//     public LocationCharacterTracker locationCharacterTracker { get; private set; }
//     public LocationGridTileArray locationGridTileArray { get; private set; }
//
//     private List<LocationGridTile> corruptedTiles;
//     private int _uncorruptibleLandmarkNeighbors = 0; //if 0, can be corrupted, otherwise, cannot be corrupted
//     private Dictionary<HEXTILE_DIRECTION, HexTile> _neighbourDirections;
//     private AreaBiomeEffectTrigger _hexTileBiomeEffectTrigger;
//     /// <summary>
//     /// Number of blueprint LocationGridTiles on this.
//     /// NOTE: This is not saved because this is filled out by <see cref="LocationGridTile.SetHasBlueprint"/>
//     /// </summary>
//     private int _blueprintsOnTile;
//     public int freezingTraps { get; private set; }
//
//     //Components
//     public AreaSpellsComponent spellsComponent { get; private set; }
//
//     #region getters/setters
//     public string persistentID => data.persistentID;
//     public OBJECT_TYPE objectType => OBJECT_TYPE.Area;
//     public Type serializedData => typeof(SaveDataHextile);
//     public int id => data.id;
//     public int xCoordinate => data.xCoordinate;
//     public int yCoordinate => data.yCoordinate;
//     public string tileName => data.tileName;
//     public float elevationNoise => data.elevationNoise;
//     public float moistureNoise => data.moistureNoise;
//     public float temperature => data.temperature;
//     public BIOMES biomeType => data.biomeType;
//     public ELEVATION elevationType => data.elevationType;
//     public string locationName => $"({xCoordinate.ToString()}, {yCoordinate.ToString()})";
//     private GameObject centerPiece => _centerPiece;
//     private GameObject highlightGO => _highlightGO;
//     private Dictionary<HEXTILE_DIRECTION, HexTile> neighbourDirections => _neighbourDirections;
//     public bool isCorrupted => _isCorrupted;
//     public bool hasBeenDestroyed => false;
//     public PARTY_TARGET_DESTINATION_TYPE partyTargetDestinationType => PARTY_TARGET_DESTINATION_TYPE.Area;
//     public Vector3 worldPosition {
//         get {
//             Vector2 pos = areaItem.transform.position;
//             // pos.x += 3.5f;
//             // pos.y += 3.5f;
//             return pos;
//         }
//     }
//     #endregion
//
//     private void Awake() {
//         _highlightSpriteRenderer = _highlightGO.GetComponent<SpriteRenderer>();
//         _structureAnimatorSpriteRenderer = structureAnimation.gameObject.GetComponent<SpriteRenderer>();
//         _highlightGOSpriteRenderer = highlightGO.GetComponent<SpriteRenderer>();
//         _hoverHighlightSpriteRenderer = _hoverHighlightGO.GetComponent<SpriteRenderer>();
//         UnityEngine.Random.ColorHSV();
//         ConstructDefaultActions();
//
//         locationAwareness = new LocationAwareness();
//     }
//     public void Initialize(bool listenForGameLoad = true) {
//         featureComponent = new AreaFeatureComponent();
//         itemsInHex = new List<TileObject>();
//         spellsComponent = new AreaSpellsComponent(null);
//         _hexTileBiomeEffectTrigger = new AreaBiomeEffectTrigger(null);
//         selectableSize = new Vector2Int(12, 12);
//         locationCharacterTracker = new LocationCharacterTracker();
//         locationGridTileArray = new LocationGridTileArray(InnerMapManager.AreaLocationGridTileSize.x * InnerMapManager.AreaLocationGridTileSize.y);
//         SetBordersState(false, false, Color.red);
//         if (listenForGameLoad) {
//             Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);    
//         }
//     }
//     private void OnGameLoaded() {
//         Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
//         SubscribeListeners();
//         EnableColliders();
//         if (settlementOnTile != null || HasSettlementNeighbour()) { //&& landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.VILLAGE
//             CheckIfStructureVisualsAreStillValid();
//         }
//         _hexTileBiomeEffectTrigger.Initialize();
//     }
//
//     #region Elevation Functions
//     internal void SetElevation(ELEVATION elevationType) {
//         data.elevationType = elevationType;
//     }
//     #endregion
//
//     #region Biome Functions
//     internal void SetBiome(BIOMES biome) {
//         _hexTileBiomeEffectTrigger.ProcessBeforeBiomeChange();
//         data.biomeType = biome;
//         _hexTileBiomeEffectTrigger.ProcessAfterBiomeChange();
//     }
//     #endregion
//
//     #region Landmarks
//     private void SetLandmarkOnTile(BaseLandmark landmarkOnTile) {
//         this.landmarkOnTile = landmarkOnTile;
//     }
//     private void CreateLandmarkVisual() {
//         GameObject landmarkGO = Instantiate(LandmarkManager.Instance.GetLandmarkGO(), structureParentGO.transform) as GameObject;
//         landmarkGO.transform.localPosition = Vector3.zero;
//         landmarkGO.transform.localScale = Vector3.one;
//         landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkVisual>());
//         UpdateLandmarkVisuals();
//     }
//     /// <summary>
//     /// Update the structure assets of this tile based on the landmark that is on this tile.
//     /// </summary>
//     [ContextMenu("Update Landmark Visuals")]
//     public void UpdateLandmarkVisuals() {
//         RACE race = RACE.NONE;
//         if (settlementOnTile?.owner != null) {
//             race = settlementOnTile.owner.race;
//         } else if (region.structures != null){
//             LocationStructure structure = GetMostImportantStructureOnTile();
//             if (structure.structureType != STRUCTURE_TYPE.WILDERNESS && structure.settlementLocation?.owner != null) {
//                 race = structure.settlementLocation.owner.race;    
//             }
//         }
//         LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(landmarkOnTile.specificLandmarkType);
//         List<LandmarkStructureSprite> landmarkTileSprites = LandmarkManager.Instance.GetLandmarkTileSprites(this, landmarkOnTile.specificLandmarkType, race);
//         if (landmarkTileSprites == null || landmarkTileSprites.Count == 0) {
//             HideLandmarkTileSprites();
//             landmarkOnTile.landmarkVisual.SetIconState(true);
//             landmarkOnTile.SetLandmarkPortrait(landmarkData.defaultLandmarkPortrait);
//         } else {
//             LandmarkStructureSprite chosenAssets = UtilityScripts.CollectionUtilities.GetRandomElement(landmarkTileSprites);
//             SetLandmarkTileSprite(chosenAssets);
//             landmarkOnTile.landmarkVisual.SetIconState(false);
//             landmarkOnTile.SetLandmarkPortrait(chosenAssets.overrideLandmarkPortrait != null ? chosenAssets.overrideLandmarkPortrait : landmarkData.defaultLandmarkPortrait);
//         }
//         SetStructureTint(settlementOnTile?.owner?.factionColor ?? Color.white);
//     }
//     public BaseLandmark LoadLandmark(BaseLandmark landmark) {
//         //Create Landmark Game Object on tile
//         var landmarkGO = Instantiate(LandmarkManager.Instance.GetLandmarkGO(), structureParentGO.transform);
//         landmarkGO.transform.localPosition = Vector3.zero;
//         landmarkGO.transform.localScale = Vector3.one;
//         landmarkOnTile = landmark;
//         if (landmarkGO != null) {
//             landmarkOnTile.SetLandmarkObject(landmarkGO.GetComponent<LandmarkVisual>());
//         }
//         return landmarkOnTile;
//     }
//     public void RemoveLandmarkOnTile() {
//         landmarkOnTile = null;
//     }
//     public void RemoveLandmarkVisuals() {
//         HideLandmarkTileSprites();
//         Destroy(landmarkOnTile.landmarkVisual.gameObject);
//     }
//     public void SetLandmarkTileSprite(LandmarkStructureSprite sprites) {
//         mainStructure.sprite = sprites.mainSprite;
//         structureTint.sprite = sprites.tintSprite;
//         mainStructure.gameObject.SetActive(true);
//         structureTint.gameObject.SetActive(true);
//
//         if (sprites.animation == null) {
//             mainStructure.enabled = true;
//             structureAnimation.gameObject.SetActive(false);
//         } else {
//             mainStructure.enabled = landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.MONSTER_LAIR; //SPECIAL CASE FOR MONSTER LAIR
//             structureAnimation.gameObject.SetActive(true);
//             structureAnimation.runtimeAnimatorController = sprites.animation;
//         }
//     }
//     private void HideLandmarkTileSprites() {
//         Debug.Log($"Hid landmark tile sprites at {this}");
//         mainStructure.gameObject.SetActive(false);
//         structureTint.gameObject.SetActive(false);
//     }
//     public void SetStructureTint(Color color) {
//         structureTint.color = color;
//     }
//     #endregion
//
//     #region Tile Utilities
//     public List<HexTile> GetTilesInRange(int range, bool sameRegionOnly = true) {
//         List<HexTile> tilesInRange = new List<HexTile>();
//         CubeCoordinate cube = GridMap.Instance.OddRToCube(new HexCoordinate(xCoordinate, yCoordinate));
//         //Debug.Log("Center in cube coordinates: " + cube.x.ToString() + "," + cube.y.ToString() + "," + cube.z.ToString());
//         for (int dx = -range; dx <= range; dx++) {
//             for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++) {
//                 int dz = -dx - dy;
//                 HexCoordinate hex = GridMap.Instance.CubeToOddR(new CubeCoordinate(cube.x + dx, cube.y + dy, cube.z + dz));
//                 //Debug.Log("Hex neighbour: " + hex.col.ToString() + "," + hex.row.ToString());
//                 if (hex.x >= 0 && hex.y >= 0
//                     && hex.x < GridMap.Instance.width && hex.y < GridMap.Instance.height
//                     && !(hex.x == xCoordinate && hex.y == yCoordinate)) {
//                     HexTile hextile = GridMap.Instance.map[hex.x, hex.y];
//                     if(!sameRegionOnly || hextile.region == region) {
//                         tilesInRange.Add(hextile);
//                     }
//                 }
//             }
//         }
//         return tilesInRange;
//     }
//     private bool IsAtEdgeOfMap() {
//         return AllNeighbours.Count < 6; //if this tile has less than 6 neighbours, it is at the edge of the map
//     }
//     public bool HasNeighbourAtEdgeOfMap() {
//         for (int i = 0; i < AllNeighbours.Count; i++) {
//             HexTile currNeighbour = AllNeighbours[i];
//             if (currNeighbour.IsAtEdgeOfMap()) {
//                 return true;
//             }
//         }
//         return false;
//     }
//     public bool HasNeighbourFromOtherRegion() {
//         for (int i = 0; i < AllNeighbours.Count; i++) {
//             HexTile currNeighbour = AllNeighbours[i];
//             if (currNeighbour.region != region) {
//                 return true;
//             }
//         }
//         return false;
//     }
//     public bool TryGetDifferentRegionNeighbours(out List<Region> regions) {
//         regions = new List<Region>();
//         for (int i = 0; i < AllNeighbours.Count; i++) {
//             HexTile currNeighbour = AllNeighbours[i];
//             if (currNeighbour.region != region && !regions.Contains(currNeighbour.region)) {
//                 regions.Add(currNeighbour.region);
//             }
//         }
//         return regions.Count > 0;
//     }
//     public bool HasNeighbourWithElevation(ELEVATION elevation) {
//         for (int i = 0; i < AllNeighbours.Count; i++) {
//             HexTile neighbour = AllNeighbours[i];
//             if (neighbour.elevationType == elevation) {
//                 return true;
//             }
//         }
//         return false;
//     }
//     public bool HasNeighbourWithFeature(string feature) {
//         for (int i = 0; i < AllNeighbours.Count; i++) {
//             HexTile neighbour = AllNeighbours[i];
//             if (neighbour.featureComponent.HasFeature(feature)) {
//                 return true;
//             }
//         }
//         return false;
//     }
//     public bool HasOwnedSettlementNeighbour() {
//         for (int i = 0; i < AllNeighbours.Count; i++) {
//             HexTile neighbour = AllNeighbours[i];
//             if (neighbour.settlementOnTile?.owner != null) {
//                 return true;
//             }
//         }
//         return false;
//     }
//     private bool HasSettlementNeighbour() {
//         for (int i = 0; i < AllNeighbours.Count; i++) {
//             HexTile neighbour = AllNeighbours[i];
//             if (neighbour.settlementOnTile != null && neighbour.settlementOnTile.locationType == LOCATION_TYPE.VILLAGE) {
//                 return true;
//             }
//         }
//         return false;
//     }
//     public bool HasAliveVillagerResident() {
//         //Does not count if hextile is only a territory
//         return settlementOnTile != null && settlementOnTile.HasResidentThatMeetsCriteria(resident => !resident.isDead && resident.isNormalCharacter);
//     }
//     public string GetDisplayName() {
//         if (settlementOnTile != null) {
//             return settlementOnTile.name;
//         } else if (landmarkOnTile != null) {
//             return landmarkOnTile.landmarkName;
//         } else {
//             string displayName = string.Empty;
//             if (isCorrupted) {
//                 displayName = "Corrupted ";
//             }
//             displayName = $"{displayName}{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(biomeType.ToString())} {UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(elevationType.ToString())}";
//             return displayName;
//         }
//     }
//     public string GetSubName() {
//         if (landmarkOnTile != null) {
//             return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(landmarkOnTile.specificLandmarkType.ToString());
//         }
//         return string.Empty;
//     }
//     public LocationGridTile GetRandomTile() {
//         return locationGridTiles[UnityEngine.Random.Range(0, locationGridTiles.Length)];
//     }
//     public LocationGridTile GetRandomTileThatMeetCriteria(Func<LocationGridTile, bool> checker) {
//         List<LocationGridTile> tiles = null;
//         for (int i = 0; i < locationGridTiles.Length; i++) {
//             LocationGridTile tile = locationGridTiles[i];
//             if (checker.Invoke(tile)) {
//                 if (tiles == null) { tiles = new List<LocationGridTile>(); }
//                 tiles.Add(tile);
//             }
//         }
//         if (tiles != null && tiles.Count > 0) {
//             return UtilityScripts.CollectionUtilities.GetRandomElement(tiles);
//         }
//         return null;
//     }
//     public List<LocationGridTile> GetUnoccupiedTiles() {
//         List<LocationGridTile> tiles = null;
//         for (int i = 0; i < locationGridTiles.Length; i++) {
//             LocationGridTile tile = locationGridTiles[i];
//             if (tile.tileObjectComponent.objHere == null) {
//                 if (tiles == null) { tiles = new List<LocationGridTile>(); }
//                 tiles.Add(tile);
//             }
//         }
//         return tiles;
//     }
//     public bool IsNextToOrPartOfVillage() {
//         return IsPartOfVillage() || IsNextToVillage();
//     }
//     public bool IsPartOfVillage() {
//         return settlementOnTile != null && settlementOnTile.locationType == LOCATION_TYPE.VILLAGE;
//     }
//     public bool IsPartOfVillage(out BaseSettlement settlement) {
//         settlement = settlementOnTile;
//         return settlementOnTile != null && settlementOnTile.locationType == LOCATION_TYPE.VILLAGE;
//     }
//     public bool IsNextToVillage() {
//         for (int i = 0; i < AllNeighbours.Count; i++) {
//             HexTile neighbour = AllNeighbours[i];
//             if(neighbour.region == region && neighbour.IsPartOfVillage()) {
//                 return true;
//             }
//         }
//         return false;
//     }
//     public HexTile GetRandomAdjacentHextileWithinRegion(bool includeSelf = false) {
//         if (includeSelf) {
//             if(UnityEngine.Random.Range(0, 100) < 15) {
//                 return this;
//             } else {
//                 List<HexTile> neighbours = ValidTilesWithinRegion;
//                 if(neighbours != null && neighbours.Count > 0) {
//                     return neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
//                 }
//             }
//         } else {
//             List<HexTile> neighbours = ValidTilesWithinRegion;
//             if (neighbours != null && neighbours.Count > 0) {
//                 return neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
//             }
//         }
//         return null;
//     }
//     public HexTile GetRandomAdjacentNoSettlementHextileWithinRegion(bool includeSelf = false) {
//         if (includeSelf) {
//             if (UnityEngine.Random.Range(0, 100) < 15) {
//                 return this;
//             } else {
//                 List<HexTile> neighbours = ValidTilesNoSettlementWithinRegion;
//                 if (neighbours != null && neighbours.Count > 0) {
//                     return neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
//                 }
//             }
//         } else {
//             List<HexTile> neighbours = ValidTilesNoSettlementWithinRegion;
//             if (neighbours != null && neighbours.Count > 0) {
//                 return neighbours[UnityEngine.Random.Range(0, neighbours.Count)];
//             }
//         }
//         return null;
//     }
//     public HexTile GetNearestPlainHexTileWithNoResident() {
//         if(elevationType != ELEVATION.WATER && elevationType != ELEVATION.MOUNTAIN) {
//             if (!HasAliveVillagerResident()) {
//                 return this;
//             }
//         }
//         for (int i = 0; i < AllNeighbours.Count; i++) {
//             HexTile neighbour = AllNeighbours[i];
//             if (neighbour.elevationType != ELEVATION.WATER && neighbour.elevationType != ELEVATION.MOUNTAIN) {
//                 if (!neighbour.HasAliveVillagerResident()) {
//                     return neighbour;
//                 }
//             }
//         }
//         for (int i = 0; i < AllNeighbours.Count; i++) {
//             HexTile neighbour = AllNeighbours[i];
//             HexTile nearestHex = neighbour.GetNearestPlainHexTileWithNoResident();
//             if (nearestHex != null) {
//                 return nearestHex;
//             }
//         }
//         return null;
//     }
//     #endregion
//
//     #region Pathfinding
//     public void FindNeighbours(HexTile[,] gameBoard) {
//         _neighbourDirections = new Dictionary<HEXTILE_DIRECTION, HexTile>();
//         var neighbours = new List<HexTile>();
//
//         List<Point> possibleExits;
//
//         if ((yCoordinate % 2) == 0) {
//             possibleExits = UtilityScripts.Utilities.EvenNeighbours;
//         } else {
//             possibleExits = UtilityScripts.Utilities.OddNeighbours;
//         }
//
//         for (int i = 0; i < possibleExits.Count; i++) {
//             int neighbourCoordinateX = xCoordinate + possibleExits[i].X;
//             int neighbourCoordinateY = yCoordinate + possibleExits[i].Y;
//             if (neighbourCoordinateX >= 0 && neighbourCoordinateX < gameBoard.GetLength(0) && neighbourCoordinateY >= 0 && neighbourCoordinateY < gameBoard.GetLength(1)) {
//                 HexTile currNeighbour = gameBoard[neighbourCoordinateX, neighbourCoordinateY];
//                 if (currNeighbour != null) {
//                     neighbours.Add(currNeighbour);
//                 }
//             }
//
//         }
//         AllNeighbours = neighbours;
//
//         for (int i = 0; i < neighbours.Count; i++) {
//             HexTile currNeighbour = neighbours[i];
//             if (currNeighbour == null) {
//                 continue;
//             }
//             HEXTILE_DIRECTION dir = GetNeighbourDirection(currNeighbour);
//             if (dir != HEXTILE_DIRECTION.NONE) {
//                 _neighbourDirections.Add(dir, currNeighbour);
//             }
//         }
//     }
//     public void FindNeighboursForBorders() {
//         _neighbourDirections = new Dictionary<HEXTILE_DIRECTION, HexTile>();
//         var neighbours = new List<HexTile>();
//
//         List<Point> possibleExits;
//
//         if ((yCoordinate % 2) == 0) {
//             possibleExits = UtilityScripts.Utilities.EvenNeighbours;
//         } else {
//             possibleExits = UtilityScripts.Utilities.OddNeighbours;
//         }
//
//         for (int i = 0; i < possibleExits.Count; i++) {
//             int neighbourCoordinateX = xCoordinate + possibleExits[i].X;
//             int neighbourCoordinateY = yCoordinate + possibleExits[i].Y;
//             HexTile neighbour = GridMap.Instance.GetTileFromCoordinates(neighbourCoordinateX, neighbourCoordinateY);
//             if (neighbour != null) {
//                 neighbours.Add(neighbour);
//             }
//
//         }
//         AllNeighbours = neighbours;
//
//         for (int i = 0; i < neighbours.Count; i++) {
//             HexTile currNeighbour = neighbours[i];
//             if (currNeighbour == null) {
//                 continue;
//             }
//             HEXTILE_DIRECTION dir = GetNeighbourDirection(currNeighbour);
//             if (dir != HEXTILE_DIRECTION.NONE) {
//                 _neighbourDirections.Add(dir, currNeighbour);
//             }
//         }
//     }
//     private HEXTILE_DIRECTION GetNeighbourDirection(HexTile neighbour) {
//         if (neighbour == null) {
//             return HEXTILE_DIRECTION.NONE;
//         }
//         if (!AllNeighbours.Contains(neighbour)) {
//             throw new System.Exception($"{neighbour.name} is not a neighbour of {name}");
//         }
//         int thisXCoordinate = xCoordinate;
//         int thisYCoordinate = yCoordinate;
//         Point difference = new Point((neighbour.xCoordinate - thisXCoordinate),
//                     (neighbour.yCoordinate - thisYCoordinate));
//         if (thisYCoordinate % 2 == 0) { //even
//             if (difference.X == -1 && difference.Y == 1) {
//                 //top left
//                 return HEXTILE_DIRECTION.NORTH_WEST;
//             } else if (difference.X == 0 && difference.Y == 1) {
//                 //top right
//                 return HEXTILE_DIRECTION.NORTH_EAST;
//             } else if (difference.X == 1 && difference.Y == 0) {
//                 //right
//                 return HEXTILE_DIRECTION.EAST;
//             } else if (difference.X == 0 && difference.Y == -1) {
//                 //bottom right
//                 return HEXTILE_DIRECTION.SOUTH_EAST;
//             } else if (difference.X == -1 && difference.Y == -1) {
//                 //bottom left
//                 return HEXTILE_DIRECTION.SOUTH_WEST;
//             } else if (difference.X == -1 && difference.Y == 0) {
//                 //left
//                 return HEXTILE_DIRECTION.WEST;
//             }
//         } else { //odd
//             if (difference.X == 0 && difference.Y == 1) {
//                 //top left
//                 return HEXTILE_DIRECTION.NORTH_WEST;
//             } else if (difference.X == 1 && difference.Y == 1) {
//                 //top right
//                 return HEXTILE_DIRECTION.NORTH_EAST;
//             } else if (difference.X == 1 && difference.Y == 0) {
//                 //right
//                 return HEXTILE_DIRECTION.EAST;
//             } else if (difference.X == 1 && difference.Y == -1) {
//                 //bottom right
//                 return HEXTILE_DIRECTION.SOUTH_EAST;
//             } else if (difference.X == 0 && difference.Y == -1) {
//                 //bottom left
//                 return HEXTILE_DIRECTION.SOUTH_WEST;
//             } else if (difference.X == -1 && difference.Y == 0) {
//                 //left
//                 return HEXTILE_DIRECTION.WEST;
//             }
//         }
//         return HEXTILE_DIRECTION.NONE;
//     }
//     public HexTile GetNeighbour(HEXTILE_DIRECTION direction) {
//         if (neighbourDirections.ContainsKey(direction)) {
//             return neighbourDirections[direction];
//         }
//         return null;
//     }
//     [ContextMenu("Update Pathfinding Graph")]
//     public void UpdatePathfindingGraph() {
//         areaItem.UpdatePathfindingGraph();
//     }
//     public void UpdatePathfindingGraphCoroutine() {
//         StartCoroutine(UpdatePathfinding());
//     }
//     private IEnumerator UpdatePathfinding() {
//         yield return null;
//         areaItem.UpdatePathfindingGraph();
//     }
//     #endregion
//
//     #region Tile Visuals
//     internal void SetSortingOrder(int sortingOrder, string sortingLayerName = "Default") {
//         spriteRenderer.sortingOrder = sortingOrder;
//         spriteRenderer.sortingLayerName = sortingLayerName;
//         UpdateSortingOrder();
//     }
//     private void UpdateSortingOrder() {
//         int sortingOrder = spriteRenderer.sortingOrder;
//         _hoverHighlightSpriteRenderer.sortingOrder = sortingOrder + 1;
//         _highlightGOSpriteRenderer.sortingOrder = sortingOrder + 1;
//
//         topLeftBeach.sortingOrder = sortingOrder + 1;
//         leftBeach.sortingOrder = sortingOrder + 1;
//         botLeftBeach.sortingOrder = sortingOrder + 1;
//         botRightBeach.sortingOrder = sortingOrder + 1;
//         rightBeach.sortingOrder = sortingOrder + 1;
//         topRightBeach.sortingOrder = sortingOrder + 1;
//
//         topLeftBorder.sortingOrder = sortingOrder + 2;
//         topRightBorder.sortingOrder = sortingOrder + 2;
//         leftBorder.sortingOrder = sortingOrder + 2;
//         botLeftBorder.sortingOrder = sortingOrder + 2;
//         botRightBorder.sortingOrder = sortingOrder + 2;
//         rightBorder.sortingOrder = sortingOrder + 2;
//
//         emptyBuildingSpotGO.sortingOrder = sortingOrder + 1;
//
//         mainStructure.sortingOrder = sortingOrder + 5;
//         structureTint.sortingOrder = sortingOrder + 6;
//         _structureAnimatorSpriteRenderer.sortingOrder = sortingOrder + 7;
//     }
//     public SpriteRenderer GetBorder(HEXTILE_DIRECTION direction) {
//         SpriteRenderer border = null;
//         switch (direction) {
//             case HEXTILE_DIRECTION.NORTH_WEST:
//                 border = topLeftBorder;
//                 break;
//             case HEXTILE_DIRECTION.NORTH_EAST:
//                 border = topRightBorder;
//                 break;
//             case HEXTILE_DIRECTION.EAST:
//                 border = rightBorder;
//                 break;
//             case HEXTILE_DIRECTION.SOUTH_EAST:
//                 border = botRightBorder;
//                 break;
//             case HEXTILE_DIRECTION.SOUTH_WEST:
//                 border = botLeftBorder;
//                 break;
//             case HEXTILE_DIRECTION.WEST:
//                 border = leftBorder;
//                 break;
//             default:
//                 break;
//         }
//         return border;
//     }
//     internal void DeactivateCenterPiece() {
//         centerPiece.SetActive(false);
//     }
//     internal void SetBaseSprite(Sprite baseSprite) {
//         this.baseSprite = baseSprite;
//         spriteRenderer.sprite = baseSprite;
//         RuntimeAnimatorController _animation;
//         if (Biomes.Instance.TryGetTileSpriteAnimation(baseSprite, out _animation)) {
//             baseTileAnimator.runtimeAnimatorController = _animation;
//             baseTileAnimator.enabled = true;
//         } else {
//             baseTileAnimator.enabled = false;
//         }
//     }
//     public void SetBordersState(bool state, bool glowState, Color color) {
//         topLeftBorder.gameObject.SetActive(state);
//         botLeftBorder.gameObject.SetActive(state);
//         topRightBorder.gameObject.SetActive(state);
//         botRightBorder.gameObject.SetActive(state);
//         leftBorder.gameObject.SetActive(state);
//         rightBorder.gameObject.SetActive(state);
//         
//         topLeftBorderGlow.enabled = glowState;
//         botLeftBorderGlow.enabled = glowState;
//         topRightBorderGlow.enabled = glowState;
//         botRightBorderGlow.enabled = glowState;
//         leftBorderGlow.enabled = glowState;
//         rightBorderGlow.enabled = glowState;
//         
//         SetBorderColor(color);
//     }
//     private void SetBorderColor(Color color) {
//         topLeftBorder.color = color;
//         botLeftBorder.color = color;
//         topRightBorder.color = color;
//         botRightBorder.color = color;
//         leftBorder.color = color;
//         rightBorder.color = color;
//         
//         topLeftBorderGlow.GlowColor = color;
//         botLeftBorderGlow.GlowColor = color;
//         topRightBorderGlow.GlowColor = color;
//         botRightBorderGlow.GlowColor = color;
//         leftBorderGlow.GlowColor = color;
//         rightBorderGlow.GlowColor = color;
//     }
//     // public void UpdateBuildSprites() {
//     //     emptyBuildingSpotGO.gameObject.SetActive(true);
//     //     currentlyBuildingSpotGO.gameObject.SetActive(false);
//     // }
//     #endregion
//
//     #region Tile Functions
//     public void DisableColliders() {
//         for (int i = 0; i < colliders.Length; i++) {
//             colliders[i].enabled = false;
//         }
//     }
//     private void EnableColliders() {
//         for (int i = 0; i < colliders.Length; i++) {
//             colliders[i].enabled = true;
//         }
//     }
//     private InfoUIBase GetMenuToShowWhenTileIsClicked() {
//         if (region != null) {
//             //if region info ui is showing, show tile info ui
//             if (UIManager.Instance.regionInfoUI.isShowing) {
//                 if (UIManager.Instance.regionInfoUI.activeRegion == region) {
//                     return UIManager.Instance.hexTileInfoUI;    
//                 } else {
//                     return UIManager.Instance.regionInfoUI;
//                 }
//             } else if (UIManager.Instance.hexTileInfoUI.isShowing) {
//                 if (UIManager.Instance.hexTileInfoUI.activeHex.region == region) {
//                     if (UIManager.Instance.hexTileInfoUI.activeHex == this) {
//                         return UIManager.Instance.regionInfoUI;
//                     } else {
//                         return UIManager.Instance.hexTileInfoUI;
//                     }
//                 } else {
//                     return UIManager.Instance.regionInfoUI;    
//                 }
//             } else {
//                 return UIManager.Instance.regionInfoUI;
//             }
//         }
//         return null;
//     }
//     #endregion
//
//     #region Monobehaviour Functions
//     private void LeftClick() {
//         if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing() || 
//             WorldMapCameraMove.Instance.isDragging) { // || GameManager.Instance.gameHasStarted == false
//             return;
//         }
//         if (!UIManager.Instance.initialWorldSetupMenu.isPickingPortal) {
//             // InfoUIBase baseToShow = GetMenuToShowWhenTileIsClicked();
//             // if (baseToShow != null) {
//             //     if (baseToShow is RegionInfoUI) {
//             //         Messenger.Broadcast(Signals.REGION_SELECTED, region);
//             //         UIManager.Instance.ShowRegionInfo(region);
//             //     } else if (baseToShow is HextileInfoUI) {
//             //         UIManager.Instance.ShowHexTileInfo(this);
//             //     }
//             // }
//             // if (GameManager.Instance.gameHasStarted) {
//             //     UIManager.Instance.ShowHexTileInfo(this);    
//             // }
//             if (region != null) {
//                 InnerMapManager.Instance.TryShowLocationMap(region);
//                 InnerMapCameraMove.Instance.CenterCameraOnTile(this);
//             }
//         }
//         MouseOver();
//         Messenger.Broadcast(AreaSignals.AREA_LEFT_CLICKED, this);
//     }
//     private void RightClick() {
//         if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing() ||
//             GameManager.Instance.gameHasStarted == false) {
//             return;
//         }
//         Messenger.Broadcast(AreaSignals.AREA_RIGHT_CLICKED, this);
//     }
//     private void MouseOver() {
//         if (UIManager.Instance.initialWorldSetupMenu.isPickingPortal) {
//             if (CanBuildDemonicStructureHere(STRUCTURE_TYPE.THE_PORTAL)) {
//                 SetBordersState(true, true, Color.green);
//             } else {
//                 SetBordersState(true, true, Color.red);    
//             }
//         } else {
//             // InfoUIBase baseToOpen = GetMenuToShowWhenTileIsClicked();
//             // if (baseToOpen is RegionInfoUI) {
//             //     region.ShowBorders(Color.red);
//             // } else if (baseToOpen is HextileInfoUI) {
//             //     SetBordersState(true, false, Color.red);
//             // }    
//             region.ShowBorders(Color.red, true);
//             SetBordersState(true, true, Color.green);
//         }
//         if (GameManager.showAllTilesTooltip) {
//             ShowTileInfo();    
//         }
//         Messenger.Broadcast(AreaSignals.AREA_HOVERED_OVER, this);
//     }
//     private void MouseExit() {
//         if (UIManager.Instance.initialWorldSetupMenu.isPickingPortal) {
//             SetBordersState(false, true, Color.red);
//         } else {
//             // InfoUIBase baseToOpen = GetMenuToShowWhenTileIsClicked();
//             // if (baseToOpen is RegionInfoUI) {
//             //     region.HideBorders();
//             // } else if (baseToOpen is HextileInfoUI) {
//             //     SetBordersState(false, false, Color.red);
//             // }
//             region.HideBorders();
//             SetBordersState(false, true, Color.red);
//         }
//         if (GameManager.showAllTilesTooltip) {
//             UIManager.Instance.HideSmallInfo();    
//         }
//         Messenger.Broadcast(AreaSignals.AREA_HOVERED_OUT, this);
//     }
//     private void DoubleLeftClick() {
//         if (UIManager.Instance.IsMouseOnUI() || UIManager.Instance.IsConsoleShowing() || 
//             GameManager.Instance.gameHasStarted == false) {
//             return;
//         }
//         // if (region != null) {
//         //     InnerMapManager.Instance.TryShowLocationMap(region);
//         //     InnerMapCameraMove.Instance.CenterCameraOnTile(this);
//         // }
//         Messenger.Broadcast(AreaSignals.AREA_DOUBLE_CLICKED, this);
//     }
//     public void PointerClick(BaseEventData bed) {
//         PointerEventData ped = bed as PointerEventData;
//         if (ped.clickCount >= 2) {
//             if (ped.button == PointerEventData.InputButton.Left) {
//                 DoubleLeftClick();
//             }
//         } else if (ped.clickCount == 1) {
//             if (ped.button == PointerEventData.InputButton.Left) {
//                 LeftClick();
//             } else if (ped.button == PointerEventData.InputButton.Right) {
//                 RightClick();
//             }
//         } 
//     }
//     public void OnPointerEnter(BaseEventData bed) {
//         PointerEventData ped = bed as PointerEventData;
//         if (ped.pointerCurrentRaycast.gameObject.CompareTag("Avatar")) {
//             OnPointerExit(bed);
//             return;
//         }
//         MouseOver();
//     }
//     public void OnPointerExit(BaseEventData bed) {
//         MouseExit();
//     }
//     public void CenterCameraHere() {
//         if (InnerMapManager.Instance.isAnInnerMapShowing) {
//             InnerMapManager.Instance.HideAreaMap();
//             UIManager.Instance.OnCameraOutOfFocus();
//         }
//         WorldMapCameraMove.Instance.CenterCameraOn(gameObject);
//     }
//     #endregion
//
//     #region For Testing
//     [Space(10)]
//     [Header("For Testing")]
//     [SerializeField] private int range = 0;
//     List<HexTile> tiles = new List<HexTile>();
//     private SpriteRenderer _hoverHighlightSpriteRenderer;
//     private SpriteRenderer _highlightGOSpriteRenderer;
//     private SpriteRenderer _structureAnimatorSpriteRenderer;
//     private SpriteRenderer _highlightSpriteRenderer;
//     [ContextMenu("Show Tiles In Range")]
//     public void ShowTilesInRange() {
//         for (int i = 0; i < tiles.Count; i++) {
//             tiles[i].spriteRenderer.color = Color.white;
//         }
//         tiles.Clear();
//         tiles.AddRange(GetTilesInRange(range));
//         for (int i = 0; i < tiles.Count; i++) {
//             tiles[i].spriteRenderer.color = Color.magenta;
//         }
//     }
//     public override string ToString() {
//         return $"{locationName} - {biomeType.ToString()} - {landmarkOnTile?.specificLandmarkType.ToString() ?? "No Landmark"} - {elevationType.ToString()} - {region?.name ?? "No Region"}";
//     }
//     private void ShowTileInfo() {
//         string summary = $"{ToString()}";
//         summary += $"\nBiome: {biomeType.ToString()}";
//         summary += $"\nElevation: {elevationType.ToString()}";
//         summary += "\nFeatures:";
//         for (int i = 0; i < featureComponent.features.Count; i++) {
//             AreaFeature feature = featureComponent.features[i];
//             summary += $"{feature.name}, ";
//         }
//         if (regionDivision != null) {
//             summary += "\nRegion Division Info:\n";
//             summary += regionDivision.GetTestingInfo();
//         }
//         summary += $"\nSettlement on Tile: {settlementOnTile?.name}";
//         summary += $"\nCharacters:\n {locationCharacterTracker.GetCharactersSummary()}";
//         UIManager.Instance.ShowSmallInfo(summary);
//         
//     }
//     #endregion
//
//     #region Corruption
//     public void SetCorruption(bool state) {
//         if(_isCorrupted != state) {
//             _isCorrupted = state;
//             Biomes.Instance.UpdateTileSprite(this, spriteRenderer.sortingOrder);
//             //for (int i = 0; i < AllNeighbours.Count; i++) {
//             //    HexTile neighbour = AllNeighbours[i];
//             //    if (neighbour.isCorrupted == false && neighbour.isCurrentlyBeingCorrupted == false) {
//             //        neighbour.CheckForCorruptAction();
//             //    }
//             //}
//         }
//     }
//     public void AdjustUncorruptibleLandmarkNeighbors(int amount) {
//         _uncorruptibleLandmarkNeighbors += amount;
//         if(_uncorruptibleLandmarkNeighbors < 0) {
//             _uncorruptibleLandmarkNeighbors = 0;
//         }
//         if(_uncorruptibleLandmarkNeighbors > 1 && landmarkOnTile != null) {
//             _uncorruptibleLandmarkNeighbors = 1;
//         }
//     }
//     private bool CanBeCorrupted() {
//         if (isCorrupted) {
//             return false; //already corrupted.
//         }
//         if (isCurrentlyBeingCorrupted) {
//             return false; //already being corrupted.
//         }
//         if (settlementOnTile != null) {
//             return false; //disabled corruption of NPC settlements for now.
//         }
//         if (PlayerManager.Instance.player.mana < EditableValuesManager.Instance.corruptTileManaCost) {
//             return false;
//         }
//         //TODO: Add checking for if this tile has any structure blueprint on it.
//         //if it has any build spots that have a blueprint on them, do not allow
//         // for (int i = 0; i < ownedBuildSpots.Length; i++) {
//         //     BuildingSpot spot = ownedBuildSpots[i];
//         //     if (spot.hasBlueprint) {
//         //         return false;
//         //     }
//         // }
//         return true;
//         //for (int i = 0; i < AllNeighbours.Count; i++) {
//         //    HexTile neighbour = AllNeighbours[i];
//         //    if (neighbour.isCorrupted) {
//         //        return true;
//         //    }
//         //}
//         //return false;
//     }
//     public void StartCorruption() {
//         //PlayerManager.Instance.player.AdjustMana(-EditableValuesManager.Instance.corruptTileManaCost);
//         InstantlyCorruptAllOwnedInnerMapTiles();
//         OnCorruptSuccess();
//     }
//     public void RemoveCorruption() {
//         PlayerManager.Instance.player.playerSettlement.RemoveAreaFromSettlement(this);
//         for (int i = 0; i < locationGridTiles.Length; i++) {
//             LocationGridTile tile = locationGridTiles[i];
//             tile.UnCorruptTile();
//         }
//     }
//     public void InstantlyCorruptAllOwnedInnerMapTiles() {
//         for (int i = 0; i < locationGridTiles.Length; i++) {
//             LocationGridTile tile = locationGridTiles[i];
//             tile.CorruptTile();
//         }
//     }
//     private HexTile GetCorruptedNeighbour() {
//         for (int i = 0; i < AllNeighbours.Count; i++) {
//             HexTile tile = AllNeighbours[i];
//             if (tile.isCorrupted) {
//                 return tile;
//             }
//         }
//         return null;
//     }
//     private void OnCorruptSuccess() {
//         PlayerManager.Instance.player.playerSettlement.AddAreaToSettlement(this);
//         // Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCorruption);
//         // isCurrentlyBeingCorrupted = false;
//         
//         //remove features
//         featureComponent.RemoveAllFeatures(this);
//         
//         //RemovePlayerAction(GetPlayerAction(PlayerDB.Corrupt_Action));
//         //if (CanBuildDemonicStructure()) {
//         //    PlayerAction buildAction = new PlayerAction(PlayerDB.Build_Demonic_Structure_Action, CanBuildDemonicStructure, null, OnClickBuild);
//         //    AddPlayerAction(buildAction);
//         //}
//     }
//     #endregion
//
//     #region Settlement
//     public void SetSettlementOnTile(BaseSettlement settlement) {
//         settlementOnTile = settlement;
//         landmarkOnTile?.nameplate.UpdateVisuals();
//         region.UpdateSettlementsInRegion();
//         if (GameManager.Instance.gameHasStarted) {
//             UpdatePathfindingGraphOnTile();
//         }
//     }
//     public void CheckIfSettlementIsStillOnTile() {
//         if (settlementOnTile != null) {
//             for (int i = 0; i < settlementOnTile.allStructures.Count; i++) {
//                 LocationStructure structure = settlementOnTile.allStructures[i];
//                 if (structure.HasTileOnArea(this)) {
//                     return; //there is still a structure on this hex tile.
//                 }
//             }
//             //if code reaches this, then there is no longer a structure from the settlement on this tile
//             settlementOnTile.RemoveAreaFromSettlement(this);
//         }
//     }
//     #endregion
//
//     #region Pathfinding
//     public TravelLine CreateTravelLine(HexTile target, int numOfTicks, Character character) {
//         TravelLineParent lineParent = BezierCurveManager.Instance.GetTravelLineParent(this, target);
//         if (lineParent == null) {
//             GameObject goParent = Instantiate(GameManager.Instance.travelLineParentPrefab);
//             lineParent = goParent.GetComponent<TravelLineParent>();
//             lineParent.SetStartAndEndPositions(this, target, numOfTicks);
//         }
//         GameObject go = Instantiate(GameManager.Instance.travelLinePrefab, lineParent.transform);
//         go.transform.SetParent(lineParent.transform);
//         TravelLine travelLine = go.GetComponent<TravelLine>();
//         travelLine.SetCharacter(character);
//         lineParent.AddChild(travelLine);
//
//         TravelLineParent targetLineParent = BezierCurveManager.Instance.GetTravelLineParent(target, this);
//         if (targetLineParent != null) {
//             targetLineParent.transform.localPosition = new Vector3(0f, 0.3f, 0f);
//         }
//         return travelLine;
//     }
//     private void UpdatePathfindingGraphOnTile() {
//         areaItem.UpdatePathfindingGraph();
//     }
//     #endregion
//
//     #region Beaches
//     public void LoadBeaches() {
//         if (_neighbourDirections == null) {
//             return;
//         }
//         if (elevationType != ELEVATION.WATER) {
//             topLeftBeach.gameObject.SetActive(false);
//             topRightBeach.gameObject.SetActive(false);
//             rightBeach.gameObject.SetActive(false);
//             botRightBeach.gameObject.SetActive(false);
//             botLeftBeach.gameObject.SetActive(false);
//             leftBeach.gameObject.SetActive(false);
//             return;
//         }
//         foreach (KeyValuePair<HEXTILE_DIRECTION, HexTile> kvp in _neighbourDirections) {
//             bool beachState;
//             if (kvp.Value != null && kvp.Value.elevationType != ELEVATION.WATER) {
//                 beachState = true;
//             } else {
//                 beachState = false;
//             }
//             switch (kvp.Key) {
//                 case HEXTILE_DIRECTION.NORTH_WEST:
//                     topLeftBeach.gameObject.SetActive(beachState);
//                     break;
//                 case HEXTILE_DIRECTION.NORTH_EAST:
//                     topRightBeach.gameObject.SetActive(beachState);
//                     break;
//                 case HEXTILE_DIRECTION.EAST:
//                     rightBeach.gameObject.SetActive(beachState);
//                     break;
//                 case HEXTILE_DIRECTION.SOUTH_EAST:
//                     botRightBeach.gameObject.SetActive(beachState);
//                     break;
//                 case HEXTILE_DIRECTION.SOUTH_WEST:
//                     botLeftBeach.gameObject.SetActive(beachState);
//                     break;
//                 case HEXTILE_DIRECTION.WEST:
//                     leftBeach.gameObject.SetActive(beachState);
//                     break;
//                 case HEXTILE_DIRECTION.NONE:
//                     break;
//                 default:
//                     break;
//             }
//         }
//     }
//     #endregion
//
//     #region Region
//     public void SetRegion(Region region) {
//         this.region = region;
//     }
//     #endregion
//
//     #region Inner Map
//     public void SetAreaItem(AreaItem p_item) {
//         areaItem = p_item;
//         PopulateBorderTiles();
//     }
//     private void PopulateBorderTiles() {
//         borderTiles = new List<LocationGridTile>();
//         InnerTileMap tileMap = region.innerMap;
//         //To populate border tiles we need to know the width and height of hextile in the inner map, which is currently InnerMapManager.BuildingSpotSize x 2
//         int hexWidth = InnerMapManager.AreaLocationGridTileSize.x;
//         int hexHeight = InnerMapManager.AreaLocationGridTileSize.y;
//
//         //Our origin point will always be the first entry in the locationGridTiles list, assuming that the first entry is always the lower left corner of the hex tile
//         int originX = locationGridTiles[0].localPlace.x;
//         int originY = locationGridTiles[0].localPlace.y;
//
//         //Let's get the actual width and height from the origin point
//         int actualHeight = originY + (hexHeight - 1);
//         int actualWidth = originX + (hexWidth - 1);
//
//         //Now, to calculate border tiles, we will simply add the origin points and the hex width and height and loop through all the tiles corresponding those points
//         //There are four sides to the borders since the hex tile in the inner map is a square, we will call it A - left side, B - up side, C - right side, and D - down side
//         
//         //To get A, we must increment from originY, while the originX is constant
//         for (int i = originY; i < actualHeight; i++) {
//             borderTiles.Add(tileMap.map[originX, i]);
//         }
//         //To get B, we must increment from originX, while actualHeight is constant
//         for (int i = originX; i < actualWidth; i++) {
//             borderTiles.Add(tileMap.map[i, actualHeight]);
//         }
//         //To get C, we must increment from originY, while actualWidth is constant
//         for (int i = originY; i <= actualHeight; i++) {
//             borderTiles.Add(tileMap.map[actualWidth, i]);
//         }
//         //To get D, we must increment from originX, while originY is constant
//         for (int i = originX + 1; i < actualWidth; i++) {
//             borderTiles.Add(tileMap.map[i, originY]);
//         }
//
//         //IMPORTANT NOTE BELOW! DO NOT DELETE COMMENT!
//         //Let's check using an example, if the origin point is (0, 0) and the actual width = 7, and the actual height = 7
//         //Then A = (0, 0) to (0, 6)
//         //B = (0, 7) to (6, 7)
//         //C = (7, 0) to (7, 7)
//         //D = (1, 0) to (6, 0)
//
//     }
//     public void PopulateTileObjectsInHexTile(List<TileObject> p_tileObjectList, TILE_OBJECT_TYPE type) {
//         for (int i = 0; i < locationGridTiles.Length; i++) {
//             LocationGridTile tile = locationGridTiles[i];
//             if (tile.tileObjectComponent.objHere != null && tile.tileObjectComponent.objHere is TileObject obj && obj.tileObjectType == type) {
//                 p_tileObjectList.Add(obj);
//             }
//         }
//     }
//     public int GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE type) {
//         int count = 0;
//         for (int i = 0; i < locationGridTiles.Length; i++) {
//             LocationGridTile tile = locationGridTiles[i];
//             if (tile.tileObjectComponent.objHere != null && tile.tileObjectComponent.objHere is TileObject obj && obj.tileObjectType == type) {
//                 count++;
//             }
//         }
//         return count;
//     }
//     public List<T> GetTileObjectsInHexTile<T>() where T : TileObject {
//         List<T> tileObjects = null;
//         for (int i = 0; i < locationGridTiles.Length; i++) {
//             LocationGridTile tile = locationGridTiles[i];
//             if (tile.tileObjectComponent.objHere is T obj) {
//                 if (tileObjects == null) {
//                     tileObjects = new List<T>();
//                 }
//                 tileObjects.Add(obj);
//             }
//         }
//         return tileObjects;
//     }
//     #endregion
//
//     #region Listeners
//     private void SubscribeListeners() {    
//         Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
//         Messenger.AddListener<LocationStructure, HexTile>(StructureSignals.STRUCTURE_OBJECT_REMOVED, OnStructureRemoved);
//     }
//     private void OnStructurePlaced(LocationStructure structure) {
//         if (this == structure.occupiedArea) {
//             CheckIfStructureVisualsAreStillValid();
//         }
//     }
//     private void OnStructureRemoved(LocationStructure structure, HexTile removedFrom) {
//         if (this == removedFrom) {
//             CheckIfStructureVisualsAreStillValid();
//         }
//     }
//     public LocationStructure GetMostImportantStructureOnTile() {
//         LocationStructure mostImportant = region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
//         foreach (KeyValuePair<STRUCTURE_TYPE,List<LocationStructure>> pair in region.structures) {
//             for (int i = 0; i < pair.Value.Count; i++) {
//                 if (pair.Key == STRUCTURE_TYPE.WILDERNESS) {
//                     continue;
//                 }
//                 LocationStructure structure = pair.Value[i];
//                 if (structure.HasTileOnArea(this)) {
//                     int value = pair.Key.StructurePriority(); 
//                     if (value > mostImportant.structureType.StructurePriority()) {
//                         mostImportant = structure;
//                     }
//                 }
//                 
//                 // if (structure is Cave cave) {
//                 //     if (cave.occupiedHexTile != null && cave.caveHexTiles.Contains(innerMapHexTile)) {
//                 //         int value = pair.Key.StructurePriority(); 
//                 //         if (value > mostImportant.structureType.StructurePriority()) {
//                 //             mostImportant = structure;
//                 //         }    
//                 //     }
//                 // } else {
//                 //     if (structure.occupiedHexTile != null && structure.occupiedHexTile == innerMapHexTile) {
//                 //         int value = pair.Key.StructurePriority(); 
//                 //         if (value > mostImportant.structureType.StructurePriority()) {
//                 //             mostImportant = structure;
//                 //         }    
//                 //     }
//                 // }
//             }
//         }
//         
//         return mostImportant;
//     }
//     private void CheckIfStructureVisualsAreStillValid() {
//         string log = $"Checking {ToString()} to check if landmark on it is still valid";
//         STRUCTURE_TYPE mostImportantStructure = GetMostImportantStructureOnTile().structureType;
//         log += $"\nMost important structure is {mostImportantStructure.ToString()}";
//         if (mostImportantStructure == STRUCTURE_TYPE.WILDERNESS || mostImportantStructure == STRUCTURE_TYPE.CAVE || mostImportantStructure == STRUCTURE_TYPE.OCEAN) {
//             log += $"\nWill destroy existing landmark {landmarkOnTile?.ToString() ?? "Null"}";
//             LandmarkManager.Instance.DestroyLandmarkOnTile(this);
//         } else {
//             LANDMARK_TYPE landmarkType = mostImportantStructure.GetLandmarkType();
//             log += $"\nLandmark to create is {landmarkType.ToString()}";
//             if (landmarkOnTile == null) {
//                 LandmarkManager.Instance.CreateNewLandmarkOnTile(this, landmarkType);
//             } else {
//                 if (landmarkOnTile.specificLandmarkType != landmarkType) {
//                     landmarkOnTile.ChangeLandmarkType(landmarkType);    
//                 }
//             }    
//         }
//         
//         
//         Debug.Log(log);
//     }
//     #endregion
//     
//     #region Player Action Target
//     public List<PLAYER_SKILL_TYPE> actions { get; private set; }
//     public void ConstructDefaultActions() {
//         actions = new List<PLAYER_SKILL_TYPE>();
//         //PlayerAction harassAction = new PlayerAction(PlayerDB.Harass_Action, CanDoHarass, IsHarassRaidInvadeValid, () => PlayerUI.Instance.OnClickHarassRaidInvade(this, "harass"));
//         //PlayerAction raidAction = new PlayerAction(PlayerDB.Raid_Action, CanDoRaid, IsHarassRaidInvadeValid, () => PlayerUI.Instance.OnClickHarassRaidInvade(this, "raid"));
//         //PlayerAction invadeAction = new PlayerAction(PlayerDB.Invade_Action, CanDoInvade, IsHarassRaidInvadeValid, () => PlayerUI.Instance.OnClickHarassRaidInvade(this, "invade"));
//         //PlayerAction buildAction = new PlayerAction(PlayerDB.Build_Demonic_Structure_Action, () => true, CanBuildDemonicStructure, OnClickBuild);
//
//         // AddPlayerAction(SPELL_TYPE.HARASS);
//         //AddPlayerAction(SPELL_TYPE.DEFEND);
//         // AddPlayerAction(SPELL_TYPE.INVADE);
//         // AddPlayerAction(SPELL_TYPE.BUILD_DEMONIC_STRUCTURE);
//     }
//     public void AddPlayerAction(PLAYER_SKILL_TYPE action) {
//         if (actions.Contains(action) == false) {
//             actions.Add(action);
//             Messenger.Broadcast(SpellSignals.PLAYER_ACTION_ADDED_TO_TARGET, action, this as IPlayerActionTarget);    
//         }
//     }
//     public void RemovePlayerAction(PLAYER_SKILL_TYPE action) {
//         if (actions.Remove(action)) {
//             Messenger.Broadcast(SpellSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
//         }
//     }
//     public void ClearPlayerActions() {
//         actions.Clear();
//     }
//     #endregion
//
//     #region Demonic Structure Building
//     private AutoDestroyParticle _buildParticles;
//     public bool CanBuildDemonicStructureHere(STRUCTURE_TYPE structureType) {
//         if (InnerMapManager.Instance.currentlyShowingLocation == null && structureType != STRUCTURE_TYPE.THE_PORTAL) {
//             //allow portal to be built while no inner map is shown, because portal is build on the overworld
//             return false;
//         }
//         if (structureType == STRUCTURE_TYPE.EYE) {
//             return CanBuildDemonicStructureHere() && InnerMapManager.Instance.currentlyShowingLocation != null && !InnerMapManager.Instance.currentlyShowingLocation.HasStructure(STRUCTURE_TYPE.EYE); //only 1 eye per region.
//         }
//         if (structureType == STRUCTURE_TYPE.MEDDLER) {
//             return CanBuildDemonicStructureHere() && !PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.MEDDLER); //Only 1 meddler should exist in the world
//         }
//         if (structureType == STRUCTURE_TYPE.BIOLAB) {
//             return CanBuildDemonicStructureHere() && !PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.BIOLAB); //Only 1 biolab should exist in the world
//         }
//         return CanBuildDemonicStructureHere();
//     }
//     private bool CanBuildDemonicStructureHere() {
//         if (HasBlueprintOnTile()) {
//             return false;
//         }
//         if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.isCurrentlyBuildingDemonicStructure) {
//             return false;
//         }
//         if (settlementOnTile != null || landmarkOnTile != null) {
//             return false;
//         }
//         if (_buildParticles != null) {
//             return false;
//         }
//         if(elevationType == ELEVATION.WATER || elevationType == ELEVATION.MOUNTAIN) {
//             return false;
//         }
//         return true;
//         // //Cannot build on settlements and hex tiles with blueprints right now
//         // if(settlementOnTile == null && landmarkOnTile == null && elevationType != ELEVATION.WATER && elevationType != ELEVATION.MOUNTAIN && _buildParticles == null) {
//         //     return true;
//         // }
//         // return false;
//     }
//     public void StartBuild(PLAYER_SKILL_TYPE structureType) {
//         _buildParticles = GameManager.Instance.CreateParticleEffectAt(GetCenterLocationGridTile(), PARTICLE_EFFECT.Build_Demonic_Structure).GetComponent<AutoDestroyParticle>();
//         DemonicStructurePlayerSkill demonicStructureSkill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(structureType);
//         demonicStructureSkill.OnExecutePlayerSkill();
//         StartCoroutine(BuildCoroutine(structureType));
//         PlayerManager.Instance.player.SetIsCurrentlyBuildingDemonicStructure(true);
//     }
//     private IEnumerator BuildCoroutine(PLAYER_SKILL_TYPE structureType) {
//         yield return new WaitForSeconds(3f);
//         _buildParticles.StopEmission();
//         DemonicStructurePlayerSkill demonicStructureSkill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(structureType);
//         demonicStructureSkill.BuildDemonicStructureAt(this);
//         _buildParticles = null;
//         PlayerManager.Instance.player.SetIsCurrentlyBuildingDemonicStructure(false);
//     }
//     #endregion
//
//     #region Border Tester
//     [Header("Border Tester")]
//     [SerializeField] private LineRenderer borderLine;
//     [SerializeField] private Transform[] vertices;
//     public Transform[] GetVertices(HEXTILE_DIRECTION direction) {
//         Transform[] _vertices = new Transform[2];
//         switch (direction) {
//             case HEXTILE_DIRECTION.NORTH_WEST:
//                 _vertices[0] = vertices[1];
//                 _vertices[1] = vertices[0];
//                 break;
//             case HEXTILE_DIRECTION.NORTH_EAST:
//                 _vertices[0] = vertices[5];
//                 _vertices[1] = vertices[0];
//                 break;
//             case HEXTILE_DIRECTION.EAST:
//                 _vertices[0] = vertices[5];
//                 _vertices[1] = vertices[4];
//                 break;
//             case HEXTILE_DIRECTION.SOUTH_EAST:
//                 _vertices[0] = vertices[4];
//                 _vertices[1] = vertices[3];
//                 break;
//             case HEXTILE_DIRECTION.SOUTH_WEST:
//                 _vertices[0] = vertices[3];
//                 _vertices[1] = vertices[2];
//                 break;
//             case HEXTILE_DIRECTION.WEST:
//                 _vertices[0] = vertices[2];
//                 _vertices[1] = vertices[1];
//                 break;
//         }
//         return _vertices;
//     }
//     #endregion
//     
//     #region Selectable
//     public bool IsCurrentlySelected() {
//         return UIManager.Instance.hexTileInfoUI.isShowing &&
//                UIManager.Instance.hexTileInfoUI.activeHex == this;
//     }
//     public void LeftSelectAction() {
//         UIManager.Instance.ShowHexTileInfo(this);
//     }
//     public void RightSelectAction() { }
//     public void MiddleSelectAction() { }
//     public bool CanBeSelected() {
//         return true;
//     }
//     #endregion
//     
//     #region POI
//     public void OnPlacePOIInHex(IPointOfInterest poi) {
//         spellsComponent.OnPlacePOIInHex(poi);
//         if(poi is TileObject item && item.tileObjectType.IsTileObjectAnItem()) {
//             AddItemInHex(item);
//         }
//     }
//     public void OnRemovePOIInHex(IPointOfInterest poi) {
//         spellsComponent.OnRemovePOIInHex(poi);
//         if (poi is TileObject item && item.tileObjectType.IsTileObjectAnItem()) {
//             RemoveItemInHex(item);
//         }
//     }
//     public void AddItemInHex(TileObject item) {
//         if (!itemsInHex.Contains(item)) {
//             itemsInHex.Add(item);
//         }
//     }
//     public bool RemoveItemInHex(TileObject item) {
//         return itemsInHex.Remove(item);
//     }
//     #endregion
//
//     #region Characters
//     public List<T> GetAllCharactersInsideHex<T>() where T : Character {
//         return locationCharacterTracker.GetAllCharactersInsideHex<T>();
//     }
//     public void PopulateCharacterListInsideHexThatMeetCriteria(List<Character> p_characterList, System.Func<Character, bool> validityChecker) {
//         locationCharacterTracker.PopulateCharacterListInsideHexThatMeetCriteria(p_characterList, validityChecker);
//     }
//     public List<T> GetAllDeadAndAliveCharactersInsideHex<T>() where T : Character {
//         List<T> characters = null;
//         for (int i = 0; i < locationGridTiles.Length; i++) {
//             LocationGridTile tile = locationGridTiles[i];
//             if (tile.charactersHere.Count > 0) {
//                 for (int j = 0; j < tile.charactersHere.Count; j++) {
//                     Character character = tile.charactersHere[j];
//                     if (character is T validCharacter) {
//                         if (characters == null) { characters = new List<T>(); }
//                         characters.Add(validCharacter);
//                     }
//                 }
//             }
//         }
//         return characters;
//     }
//     public T GetFirstCharacterInsideHexThatMeetCriteria<T>(System.Func<Character, bool> validityChecker) where T : Character {
//         return locationCharacterTracker.GetFirstCharacterInsideHexThatMeetCriteria<T>(validityChecker);
//     }
//     public T GetRandomCharacterInsideHexThatMeetCriteria<T>(System.Func<Character, bool> validityChecker) where T : Character {
//         return locationCharacterTracker.GetRandomCharacterInsideHexThatMeetCriteria<T>(validityChecker);
//     }
//     public int GetNumOfCharactersInsideHexThatMeetCriteria(System.Func<Character, bool> criteria) {
//         return locationCharacterTracker.GetNumOfCharactersInsideHexThatMeetCriteria(criteria);
//     }
//     public LocationGridTile GetCenterLocationGridTile() {
//         LocationGridTile lowerLeftCornerTile = locationGridTileArray.tiles[0];
//         int xMin = lowerLeftCornerTile.localPlace.x;
//         int yMin = lowerLeftCornerTile.localPlace.y;
//         int xMax = xMin + (InnerMapManager.AreaLocationGridTileSize.x / 2);
//         int yMax = yMin + (InnerMapManager.AreaLocationGridTileSize.y / 2);
//         return region.innerMap.map[xMax, yMax];
//     }
//     #endregion
//
//     #region Biome
//     public void ChangeBiomeType(BIOMES biomeType) {
//         SetBiome(biomeType);
//         Biomes.Instance.UpdateTileVisuals(this);
//         ChangeGridTilesBiome();
//     }
//     private void ChangeGridTilesBiome() {
//         for (int i = 0; i < locationGridTiles.Length; i++) {
//             LocationGridTile currTile = locationGridTiles[i];
//             Vector3Int position = currTile.localPlace;
//             TileBase groundTile = InnerTileMap.GetGroundAssetPerlin(currTile.floorSample, biomeType);
//             if (currTile.structure.isInterior || currTile.isCorrupted) {
//                 //set the previous tile to the new biome, so that when the structure is destroyed
//                 //it will revert to the right asset
//             } else {
//                 currTile.parentMap.groundTilemap.SetTile(position, groundTile);
//                 currTile.UpdateGroundTypeBasedOnAsset();
//                 if (currTile.tileObjectComponent.objHere != null && currTile.tileObjectComponent.objHere.mapObjectVisual && currTile.tileObjectComponent.objHere is TileObject tileObject) {
//                     tileObject.mapVisual.UpdateTileObjectVisual(tileObject);
//                 }
//                 currTile.CreateSeamlessEdgesForSelfAndNeighbours();    
//             }
//         }
//     }
//     public void GradualChangeBiomeType(BIOMES biomeType, System.Action onFinishChangeAction) {
//         SetBiome(biomeType);
//         Biomes.Instance.UpdateTileVisuals(this);
//         StartCoroutine(ChangeGridTilesBiomeCoroutine(onFinishChangeAction));
//     }
//     private IEnumerator ChangeGridTilesBiomeCoroutine(System.Action onFinishChangeAction) {
//         // List<LocationGridTile> gridTiles = new List<LocationGridTile>(locationGridTiles);
//         // gridTiles = UtilityScripts.CollectionUtilities.Shuffle(gridTiles);
//         for (int i = 0; i < locationGridTiles.Length; i++) {
//             LocationGridTile currTile = locationGridTiles[i];
//             Vector3Int position = currTile.localPlace;
//             TileBase groundTile = InnerTileMap.GetGroundAssetPerlin(currTile.floorSample, biomeType);
//             if (currTile.structure.isInterior || currTile.isCorrupted) {
//                 //do not change tiles of interior or corrupted structures.
//                 continue;
//             }
//             
//             currTile.parentMap.groundTilemap.SetTile(position, groundTile);
//             currTile.UpdateGroundTypeBasedOnAsset();
//             if (currTile.tileObjectComponent.objHere != null && currTile.tileObjectComponent.objHere.mapObjectVisual && currTile.tileObjectComponent.objHere is TileObject tileObject) {
//                 tileObject.mapVisual.UpdateTileObjectVisual(tileObject);
//             }
//             currTile.CreateSeamlessEdgesForSelfAndNeighbours();
//             yield return null;
//         }
//         onFinishChangeAction.Invoke();
//     } 
//     #endregion
//
//     #region Defend
//     public void IncreaseIsBeingDefendedCount() {
//     }
//     public void DecreaseIsBeingDefendedCount() {
//     }
//     #endregion
//
//     #region IPartyTargetDestination
//     public LocationGridTile GetRandomPassableTile() {
//         LocationGridTile chosenTile = null;
//         List<LocationGridTile> passableTiles = ObjectPoolManager.Instance.CreateNewGridTileList();
//         for (int i = 0; i < locationGridTiles.Length; i++) {
//             LocationGridTile tile = locationGridTiles[i];
//             if (tile.IsPassable()) {
//                 passableTiles.Add(tile);
//             }
//         }
//         if (passableTiles != null && passableTiles.Count > 0) {
//             chosenTile = CollectionUtilities.GetRandomElement(passableTiles);
//         }
//         ObjectPoolManager.Instance.ReturnGridTileListToPool(passableTiles);
//         return chosenTile;
//     }
//     public bool IsAtTargetDestination(Character character) {
//         return character.gridTileLocation != null && character.gridTileLocation.area == this;
//     }
//     #endregion
//
//     #region Blueprints
//     public void AddBlueprint() {
//         _blueprintsOnTile++;
//     }
//     public void RemoveBlueprint() {
//         _blueprintsOnTile++;
//     }
//     public bool HasBlueprintOnTile() {
//         return _blueprintsOnTile > 0;
//     }
//     #endregion
//
//     #region Tile Object
//     public bool HasTileObjectInsideHexThatMeetCriteria(Func<TileObject, bool> criteria) {
//         for (int i = 0; i < locationGridTiles.Length; i++) {
//             LocationGridTile tile = locationGridTiles[i];
//             if (tile.tileObjectComponent.objHere != null) {
//                 TileObject obj = tile.tileObjectComponent.objHere as TileObject;
//                 if (obj != null && criteria.Invoke(obj)) {
//                     return true;
//                 }
//             }
//         }
//         return false;
//     }
//     #endregion
//
//     #region Region Division
//     public void SetRegionDivision(RegionDivision p_regionDivision) {
//         regionDivision = p_regionDivision;
//     }
//     #endregion
// }