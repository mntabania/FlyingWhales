using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UtilityScripts;
namespace Inner_Maps {
    public class InnerMapManager : MonoBehaviour {

        public static InnerMapManager Instance;
        
        public static readonly Vector2Int BuildingSpotSize = new Vector2Int(7, 7);
        public static readonly int BuildingSpotBorderSize = 1; //how many tiles, per side of the build spot, should not be occupied by the structure.

        public static readonly string VisibleAllTag = "Visible_All";
        public static readonly string InvisibleToCharacterTag = "Invisible_Character";
        
        public const int DefaultCharacterSortingOrder = 82;
        public const int GroundTilemapSortingOrder = 10;
        public const int DetailsTilemapSortingOrder = 40;
   
        private Vector3 _nextMapPos = Vector3.zero;
        public GameObject characterCollisionTriggerPrefab;

        [Header("Pathfinding")]
        [SerializeField] private AstarPath pathfinder;

        [Header("Tile Object")]
        [SerializeField] private TileObjectSlotDictionary tileObjectSlotSettings;
        public GameObject tileObjectSlotsParentPrefab;
        public GameObject tileObjectSlotPrefab;
    
        [Header("Lighting")]
        [SerializeField] private Light areaMapLight;

        [Header("Structures")]
        [SerializeField] private LocationStructurePrefabDictionary structurePrefabs;

        [Header("Tilemap Assets")] 
        public InnerMapAssetManager assetManager;
        [SerializeField] private WallResourceAssetDictionary wallResourceAssets; //wall assets categorized by resource.

        //Settlement Map Objects
        [FormerlySerializedAs("areaMapObjectFactory")] public MapVisualFactory mapObjectFactory;

        //structure templates
        private string templatePath;
        
        //this specifies what light intensity is to be used while inside the specific range in ticks
        private readonly Dictionary<int, float> lightSettings = new Dictionary<int, float>() {
            { 228, 0.3f }, { 61, 0.8f }
        };
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        public Dictionary<TILE_OBJECT_TYPE, List<TileObject>> allTileObjects { get; private set; }
        public InnerTileMap currentlyShowingMap { get; private set; }
        public ILocation currentlyShowingLocation { get; private set; }
        public List<InnerTileMap> innerMaps { get; private set; }
        public bool isAnInnerMapShowing => currentlyShowingMap != null;

        public IPointOfInterest currentlyHoveredPoi { get; private set; }
        public List<LocationGridTile> currentlyHighlightedTiles { get; private set; }
        private LocationGridTile lastClickedTile;
        private LocationGridTile lastHoveredTile;

        #region Monobehaviours
        private void Awake() {
            Instance = this;
            templatePath = $"{Application.dataPath}/StreamingAssets/Structure Templates/";
        }
        public void LateUpdate() {
            if (GameManager.showAllTilesTooltip) {
                if (UIManager.Instance.IsMouseOnUI() || currentlyShowingMap == null) {
                    return;
                }
                LocationGridTile hoveredTile = GetTileFromMousePosition();
                if (hoveredTile != null && hoveredTile.objHere == null) {
                    ShowTileData(hoveredTile);
                }
                lastHoveredTile = hoveredTile;
            }
        }
        private void OnClickMapObject(KeyCode keyCode) {
            if (keyCode == KeyCode.Mouse0) {
                //TODO: Create system that disables normal clicks.
                if (UIManager.Instance.IsMouseOnUI() == false 
                    && ReferenceEquals(currentlyShowingMap, null) == false 
                    && PlayerManager.Instance.player.currentActivePlayerSpell == null
                    && PlayerManager.Instance.player.seizeComponent.hasSeizedPOI == false) {
                    LocationGridTile clickedTile = GetTileFromMousePosition();
                    if (clickedTile != null && TryGetSelectablesOnTile(clickedTile, out var selectables)) {
                        if (selectables.Count > 0) {
                            if (lastClickedTile != clickedTile) {
                                //if last tile that was clicked is not the tile that has been clicked, then instead of 
                                //looping through the selectables, just select the first one.
                                selectables[0].LeftSelectAction();  
                            } else {
                                ISelectable objToSelect = null;
                                for (int i = 0; i < selectables.Count; i++) {
                                    ISelectable currentSelectable = selectables[i];
                                    if (currentSelectable.IsCurrentlySelected()) {
                                        //set next selectable in list to be selected.
                                        objToSelect = CollectionUtilities.GetNextElementCyclic(selectables, i);
                                        break;
                                    }
                                }
                                if (objToSelect == null) {
                                    objToSelect = selectables[0];
                                }
                                objToSelect.LeftSelectAction();    
                            }
                        }
                        lastClickedTile = clickedTile;    
                    }
                }
            } else if (keyCode == KeyCode.Mouse1) {
                if (UIManager.Instance.IsMouseOnUI() == false && ReferenceEquals(currentlyShowingMap, null) == false) {
                    LocationGridTile clickedTile = GetTileFromMousePosition();
                    ISelectable selectable = GetFirstSelectableOnTile(clickedTile);
                    selectable?.RightSelectAction();
                }
            }
        }
        
        private ISelectable GetFirstSelectableOnTile(LocationGridTile tile) {
            PointerEventData pointer = new PointerEventData(EventSystem.current) {position = Input.mousePosition};

            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0) { 
                foreach (var go in raycastResults) {
                    if (go.gameObject.CompareTag("Character Marker") || go.gameObject.CompareTag("Map Object")) {
                        BaseMapObjectVisual visual = go.gameObject.GetComponent<BaseMapObjectVisual>();
                        if (visual.IsInvisible()) {
                            continue; //skip
                        }
                        //assume that all objects that have the specified tags have the BaseMapObjectVisual class
                        if (visual.selectable != null) {
                            return visual.selectable;
                        }
                    }
                }
            }
            
            if (tile.structure != null && ReferenceEquals(tile.structure.structureObj, null) == false) {
                return tile.structure;
            }
            // if (tile.IsPartOfSettlement(out var settlement)) {
            //     selectables.Add(settlement);
            // }
            if (tile.buildSpotOwner.isPartOfParentRegionMap) {
                return tile.buildSpotOwner.hexTileOwner;
            }
            return null;
        }
        private bool TryGetSelectablesOnTile(LocationGridTile tile, out List<ISelectable> selectables) {
            selectables = new List<ISelectable>();
            
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;

            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0) { 
                foreach (var go in raycastResults) {
                    if (go.gameObject.CompareTag("Character Marker") || go.gameObject.CompareTag("Map Object")) {
                        BaseMapObjectVisual visual = go.gameObject.GetComponent<BaseMapObjectVisual>();
                        if (visual.IsInvisible()) {
                            continue; //skip
                        }
                        //assume that all objects that have the specified tags have the BaseMapObjectVisual class
                        if (visual.selectable != null) {
                            selectables.Add(visual.selectable);    
                        }
                    } else if (go.gameObject.CompareTag("Map_Click_Blocker")) {
                        return false; //click was blocked
                    }
                }
            }
            
            if (tile.structure != null && ReferenceEquals(tile.structure.structureObj, null) == false) {
                selectables.Add(tile.structure);
            }
            // if (tile.IsPartOfSettlement(out var settlement)) {
            //     selectables.Add(settlement);
            // }
            if (tile.buildSpotOwner.isPartOfParentRegionMap) {
                selectables.Add(tile.buildSpotOwner.hexTileOwner);
            }
            return true;
        }
        private void Update() {
            if (currentlyHoveredPoi != null && ReferenceEquals(currentlyHoveredPoi.mapObjectVisual, null) == false) {
                currentlyHoveredPoi.mapObjectVisual.ExecuteHoverEnterAction();    
            }
        }
        #endregion

        #region Main
        public void Initialize() {
            allTileObjects = new Dictionary<TILE_OBJECT_TYPE, List<TileObject>>();
            innerMaps = new List<InnerTileMap>();
            mapObjectFactory = new MapVisualFactory();
            InnerMapCameraMove.Instance.Initialize();
            Messenger.AddListener(Signals.TICK_ENDED, CheckForChangeLight);
            Messenger.AddListener<KeyCode>(Signals.KEY_DOWN, OnClickMapObject);
        }
        /// <summary>
        /// Try and show the settlement map of an settlement. If it does not have one, this will generate one instead.
        /// </summary>
        /// <param name="location"></param>
        public void TryShowLocationMap(ILocation location) {
            Assert.IsNotNull(location.innerMap, $"{location.name} does not have a generated inner map");
            ShowInnerMap(location);
        }
        public void ShowInnerMap(ILocation location, bool centerCameraOnMapCenter = true, bool instantCenter = true) {
            if (location.locationType == LOCATION_TYPE.DEMONIC_INTRUSION) {
                UIManager.Instance.portalPopup.SetActive(true);
                return;
            }
            location.innerMap.Open();
            currentlyShowingMap = location.innerMap;
            currentlyShowingLocation = location;
            Messenger.Broadcast(Signals.LOCATION_MAP_OPENED, location);

            if (centerCameraOnMapCenter) {
                InnerMapCameraMove.Instance.JustCenterCamera(instantCenter);
            }
        }
        public ILocation HideAreaMap() {
            if (currentlyShowingMap == null) {
                return null;
            }
            currentlyShowingMap.Close();
            ILocation closedLocation = currentlyShowingLocation;
            InnerMapCameraMove.Instance.CenterCameraOn(null);
            currentlyShowingMap = null;
            currentlyShowingLocation = null;
            // PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(null);
            Messenger.Broadcast(Signals.LOCATION_MAP_CLOSED, closedLocation);
            return closedLocation;
        }
        public void OnCreateInnerMap(InnerTileMap newMap) {
            innerMaps.Add(newMap);
            //newMap.transform.localPosition = nextMapPos;
            //set the next map position based on the new maps height
            newMap.transform.localPosition = _nextMapPos;
            newMap.UpdateTilesWorldPosition();
            PathfindingManager.Instance.CreatePathfindingGraphForLocation(newMap);
            _nextMapPos = new Vector3(_nextMapPos.x, _nextMapPos.y + newMap.height + 10, _nextMapPos.z);
            newMap.OnMapGenerationFinished();
        }
        public void DestroyInnerMap(ILocation location) {
            foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in location.structures) {
                for (var i = 0; i < keyValuePair.Value.Count; i++) {
                    keyValuePair.Value[i].DoCleanup();
                }
            }
            pathfinder.data.RemoveGraph(location.innerMap.pathfindingGraph);
            location.innerMap.CleanUp();
            innerMaps.Remove(location.innerMap);
            GameObject.Destroy(location.innerMap.gameObject);
            Debug.LogError($"Settlement map of {location.name} is destroyed!");
        }
        #endregion

        #region Utilities
        public LocationGridTile GetTileFromMousePosition() {
            Vector3 mouseWorldPos = (currentlyShowingMap.worldUiCanvas.worldCamera.ScreenToWorldPoint(Input.mousePosition));
            Vector3 localPos = currentlyShowingMap.grid.WorldToLocal(mouseWorldPos);
            Vector3Int coordinate = currentlyShowingMap.grid.LocalToCell(localPos);
            if (coordinate.x >= 0 && coordinate.x < currentlyShowingMap.width
                                  && coordinate.y >= 0 && coordinate.y < currentlyShowingMap.height) {
                return currentlyShowingMap.map[coordinate.x, coordinate.y];
            }
            return null;
        }
        public bool IsShowingInnerMap(ILocation location) {
            return location != null && isAnInnerMapShowing && location.innerMap == currentlyShowingMap;
        }
        #endregion

        #region UI
        public void HighlightTiles(List<LocationGridTile> tiles) {
            if (tiles != null) {
                for (int i = 0; i < tiles.Count; i++) {
                    tiles[i].HighlightTile();
                }
            }
            currentlyHighlightedTiles = tiles;
        }
        public void UnhighlightTiles() {
            if (currentlyHighlightedTiles != null) {
                for (int i = 0; i < currentlyHighlightedTiles.Count; i++) {
                    currentlyHighlightedTiles[i].UnhighlightTile();
                }
            }
            currentlyHighlightedTiles = null;
        }
        public void UnhighlightTiles(List<LocationGridTile> tiles) {
            for (int i = 0; i < tiles.Count; i++) {
                tiles[i].UnhighlightTile();
            }
        }
        #endregion

        #region For Testing
        public void ShowTileData(LocationGridTile tile, Character character = null) {
            if (tile == null) {
                return;
            }
            if (UIManager.Instance.poiTestingUI.gameObject.activeSelf && 
                (UIManager.Instance.poiTestingUI.gridTile == tile || UIManager.Instance.poiTestingUI.poi == tile.objHere 
                || UIManager.Instance.poiTestingUI.poi == character)) {
                return; //do not show tooltip if right click menu is currently targeting the hovered object
            } else if (UIManager.Instance.minionCommandsUI.gameObject.activeSelf && 
                       (UIManager.Instance.minionCommandsUI.targetPOI == tile.objHere 
                        || UIManager.Instance.minionCommandsUI.targetPOI == character)) {
                return; //do not show tooltip if right click menu is currently targeting the hovered object
            }

            Profiler.BeginSample("Show Tile Data Sample");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            HexTile hexTile = tile.buildSpotOwner.hexTileOwner;
            string summary = tile.localPlace.ToString();
            summary = $"{summary}\n<b>HexTile:</b>{(hexTile?.ToString() ?? "None")}";
            summary = $"{summary}\n<b>Local Location:</b>{tile.localLocation.ToString()}";
            summary = $"{summary} <b>World Location:</b>{tile.worldLocation.ToString()}";
            summary = $"{summary} <b>Centered World Location:</b>{tile.centeredWorldLocation.ToString()}";
            summary = $"{summary} <b>Ground Type:</b>{tile.groundType.ToString()}";
            summary = $"{summary} <b>Is Occupied:</b>{tile.isOccupied.ToString()}";
            summary = $"{summary} <b>Tile Type:</b>{tile.tileType.ToString()}";
            summary = $"{summary} <b>Tile State:</b>{tile.tileState.ToString()}";
            summary = $"{summary} <b>Reserved Tile Object Type:</b>{tile.reservedObjectType.ToString()}";
            summary = $"{summary} <b>Previous Tile Asset:</b>{(tile.previousGroundVisual?.name ?? "Null")}";
            summary =
                $"{summary} <b>Current Tile Asset:</b>{(tile.parentTileMap.GetSprite(tile.localPlace)?.name ?? "Null")}";
            if (tile.hasFurnitureSpot) {
                summary = $"{summary} <b>Furniture Spot:</b>{tile.furnitureSpot.ToString()}";
            }
            summary = $"{summary}\nTile Traits: ";
            if (tile.genericTileObject != null && tile.normalTraits.Count > 0) {
                summary = $"{summary}\n";
                summary = tile.normalTraits.Aggregate(summary, (current, t) => $"{current}|{t.name}|");
            } else {
                summary = $"{summary}None";
            }

            IPointOfInterest poi;
            poi = tile.objHere ?? tile.genericTileObject;
            summary = $"{summary}\nContent: {poi}";
            if (poi != null) {
                summary = $"{summary}\nHP: {poi.currentHP.ToString()}/{poi.maxHP.ToString()}";
                summary = $"{summary}\n\tObject State: {poi.state.ToString()}";
                summary = $"{summary}\n\tIs Available: {poi.IsAvailable().ToString()}";

                if (poi is TileObject tileObject) {
                    summary = $"{summary}\n\tCharacter Owner: {tileObject.characterOwner?.name}" ?? "None";
                    summary = $"{summary}\n\tFaction Owner: {tileObject.factionOwner?.name}" ?? "None";
                    
                    switch (tileObject) {
                        case TreeObject treeObject:
                            summary = $"{summary}\n\tYield: {treeObject.yield.ToString()}";
                            break;
                        case Ore ore:
                            summary = $"{summary}\n\tYield: {ore.yield.ToString()}";
                            break;
                        case ResourcePile pile:
                            summary = $"{summary}\n\tResource in Pile: {pile.resourceInPile.ToString()}";
                            break;
                        case Table table:
                            summary = $"{summary}\n\tFood in Table: {table.food.ToString()}";
                            break;
                    }
                }
                summary = $"{summary}\n\tAdvertised Actions: ";
                summary = poi.advertisedActions.Count > 0 ? poi.advertisedActions.Aggregate(summary, (current, t) => $"{current}|{t.ToString()}|") : $"{summary}None";
                
                summary = $"{summary}\n\tObject Traits: ";
                summary = poi.traitContainer.allTraits.Count > 0 ? poi.traitContainer.allTraits.Aggregate(summary, (current, t) => $"{current}\n\t\t- {t.name} - {t.GetTestingData(poi)}") : $"{summary}None";
                
                summary = $"{summary}\n\tJobs Targeting this: ";
                summary = poi.allJobsTargetingThis.Count > 0 ? poi.allJobsTargetingThis.Aggregate(summary, (current, t) => $"{current}\n\t\t- {t}") : $"{summary}None";
            }
            if (tile.structure != null) {
                summary =
                    $"{summary}\nStructure: {tile.structure}, Occupied Build Spot: {tile.structure?.occupiedBuildSpot} " +
                    $"Tiles: {tile.structure.tiles.Count.ToString()}, Has Owner: {tile.structure.IsOccupied().ToString()}, Is Interior: {tile.structure.isInterior.ToString()}";
                summary = $"{summary}\nCharacters at {tile.structure}: ";
                if (tile.structure.charactersHere.Count > 0) {
                    for (int i = 0; i < tile.structure.charactersHere.Count; i++) {
                        Character currCharacter = tile.structure.charactersHere[i];
                        if (character == currCharacter) {
                            summary = $"{summary}\n<b>{currCharacter.name}</b>";
                            summary = $"{summary}\n\t{GetCharacterHoverData(currCharacter)}\n";
                        } else {
                            summary = $"{summary}{currCharacter.name},";
                        }
                    }
                } else {
                    summary = $"{summary}None";
                }
                // summary = $"{summary}\nPOI's at {tile.structure}: \n";
                // if (tile.structure.pointsOfInterest.Count > 0) {
                //     for (int i = 0; i < tile.structure.pointsOfInterest.Count; i++) {
                //         IPointOfInterest currPOI = tile.structure.pointsOfInterest.ElementAt(i);
                //         summary = $"{summary}{currPOI.ToString()}, ";
                //     }
                // } else {
                //     summary = $"{summary}None";
                // }
            } else {
                summary = $"{summary}\nStructure: None";
            }
            Profiler.EndSample();
            UIManager.Instance.ShowSmallInfo(summary);
#else
         //For build only
        if (tile.objHere != null) {
            UIManager.Instance.ShowSmallInfo(tile.objHere.ToString());
        }
#endif
        }
        public void ShowCharacterData(Character character) {
            string summary = $"<b>{character.name}</b>";
            summary = $"{summary}\n\t{GetCharacterHoverData(character)}\n";
            UIManager.Instance.ShowSmallInfo(summary);
        }
        private string GetCharacterHoverData(Character character) {
            string summary = $"Character: {character.name}";
            summary = $"{summary}\n<b>Mood:</b>{character.moodComponent.moodState.ToString()}";
            summary = $"{summary} <b>Supply:</b>{character.supply.ToString()}";
            summary = $"{summary} <b>Can Move:</b>{character.canMove.ToString()}";
            summary = $"{summary} <b>Can Witness:</b>{character.canWitness.ToString()}";
            summary = $"{summary} <b>Can Be Attacked:</b>{character.canBeAtttacked.ToString()}";
            summary = $"{summary} <b>Move Speed:</b>{character.marker.pathfindingAI.speed.ToString()}";
            summary = $"{summary} <b>Attack Range:</b>{character.characterClass.attackRange.ToString()}";
            summary = $"{summary} <b>Attack Speed:</b>{character.attackSpeed.ToString()}";
            summary = $"{summary} <b>Target POI:</b>{(character.marker.targetPOI?.name ?? "None")}";
            summary =
                $"{summary} <b>Base Structure:</b>{(character.trapStructure.structure != null ? character.trapStructure.structure.ToString() : "None")}";

            summary = $"{summary}\n\tDestination Tile: ";
            summary = character.marker.destinationTile == null ? $"{summary}None" : $"{summary}{character.marker.destinationTile} at {character.marker.destinationTile.parentMap.location.name}";
            
            summary = $"{summary}\n\tPOI's in Vision: ";
            summary = character.marker.inVisionPOIs.Count > 0 ? character.marker.inVisionPOIs.Aggregate(summary, (current, poi) => $"{current}{poi}, ") : $"{summary}None";
            
            summary = $"{summary}\n\tCharacters in Vision: ";
            summary = character.marker.inVisionCharacters.Count > 0 ? character.marker.inVisionCharacters.Select((t, i) => (Character) character.marker.inVisionCharacters.ElementAt(i)).Aggregate(summary, (current, poi) => $"{current}{poi.name}, ") : $"{summary}None";
            
            summary = $"{summary}\n\tPOI's in Range but different structures: ";
            summary = character.marker.visionCollision.poisInRangeButDiffStructure.Count > 0 ? character.marker.visionCollision.poisInRangeButDiffStructure.Aggregate(summary, (current, poi) => $"{current}{poi}, ") : $"{summary}None";
            
            summary = $"{summary}\n\tHostiles in Range: ";
            summary = character.combatComponent.hostilesInRange.Count > 0 ? character.combatComponent.hostilesInRange.Aggregate(summary, (current, poi) => $"{current}{poi.name}, ") : $"{summary}None";
            
            summary = $"{summary}\n\tAvoid in Range: ";
            summary = character.combatComponent.avoidInRange.Count > 0 ? character.combatComponent.avoidInRange.Aggregate(summary, (current, poi) => $"{current}{poi.name}, ") : $"{summary}None";
            
            summary = $"{summary}\n\tPersonal Job Queue: ";
            summary = character.jobQueue.jobsInQueue.Count > 0 ? character.jobQueue.jobsInQueue.Aggregate(summary, (current, poi) => $"{current}{poi}, ") : $"{summary}None";
            return summary;
        }
        #endregion

        #region Town Map Generation
        public TownMapSettings GetTownMapSettings(Dictionary<int, Dictionary<int, LocationGridTileSettings>> allSettings) {
            int minX;
            int maxX;
            int minY;
            int maxY;
            GetBounds(allSettings, out minX, out maxX, out minY, out maxY);
            TownMapSettings s = new TownMapSettings();
            int shiftXBy = 0; //shift x position of all objects by n
            int shiftYBy = 0;//shift y position of all objects by n
            if (minX != 0) { shiftXBy = minX * -1; }
            if (minY != 0) { shiftYBy = minY * -1; }

            Dictionary<int, Dictionary<int, LocationGridTileSettings>> shiftedSettings = ShiftSettingsBy(new Vector2Int(shiftXBy, shiftYBy), allSettings);
            GetBounds(shiftedSettings, out minX, out maxX, out minY, out maxY);

            if (minX < 0 || minY < 0) {
                throw new System.Exception(
                    $"Minimum bounds of shifted settings has negative value! X: {minX}, Y: {minY}");
            }
            s.size = new Point(maxX, maxY);

            List<TileTemplateData> groundTiles = new List<TileTemplateData>();
            List<TileTemplateData> groundWallTiles = new List<TileTemplateData>();
            List<TileTemplateData> structureTiles = new List<TileTemplateData>();
            List<TileTemplateData> objectTiles = new List<TileTemplateData>();
            List<TileTemplateData> detailTiles = new List<TileTemplateData>();
            List<BuildingSpotData> buildingSpotData = new List<BuildingSpotData>();

            int count = 0;
            for (int x = 0; x < maxX; x++) {
                for (int y = 0; y < maxY; y++) {
                    if (shiftedSettings.ContainsKey(x) && shiftedSettings[x].ContainsKey(y)) {
                        LocationGridTileSettings currSetting = shiftedSettings[x][y];
                        currSetting.UpdatePositions(new Vector3(x, y, 0f));
                        groundTiles.Add(currSetting.groundTile);
                        groundWallTiles.Add(currSetting.groundWallTile);
                        structureTiles.Add(currSetting.structureWallTile);
                        objectTiles.Add(currSetting.objectTile);
                        detailTiles.Add(currSetting.detailTile);
                        if (currSetting.hasBuildingSpot) {
                            buildingSpotData.Add(currSetting.buildingSpot);
                        }
                    } else {
                        TileTemplateData emptyData = TileTemplateData.Empty;
                        emptyData.tilePosition = new Vector3(x, y, 0);
                        groundTiles.Add(emptyData);
                        structureTiles.Add(emptyData);
                        objectTiles.Add(emptyData);
                        detailTiles.Add(emptyData);
                    }
                    count++;
                }
            }

            if (count != (maxX * maxY)) {
                throw new System.Exception(
                    $"Total tiles are inconsistent with size! Count is: {count}. MaxX is: {maxX}. MaxY is: {maxY}");
            }

            foreach (KeyValuePair<int, Dictionary<int, LocationGridTileSettings>> kvp in shiftedSettings) {
                foreach (KeyValuePair<int, LocationGridTileSettings> kvp2 in kvp.Value) {
                    LocationGridTileSettings currSetting = kvp2.Value;
                    currSetting.UpdatePositions(new Vector3(kvp.Key, kvp2.Key, 0f));
                    groundTiles.Add(currSetting.groundTile);
                    groundWallTiles.Add(currSetting.groundWallTile);
                    structureTiles.Add(currSetting.structureWallTile);
                    objectTiles.Add(currSetting.objectTile);
                    detailTiles.Add(currSetting.detailTile);
                    count++;
                }
            }

            s.groundTiles = groundTiles.ToArray();
            s.groundWallTiles = groundWallTiles.ToArray();
            s.structureTiles = structureTiles.ToArray();
            s.objectTiles = objectTiles.ToArray();
            s.detailTiles = detailTiles.ToArray();
            s.buildSpots = buildingSpotData;
            return s;
        }
        private void GetBounds(Dictionary<int, Dictionary<int, LocationGridTileSettings>> allSettings, out int minX, out int maxX, out int minY, out int maxY) {
            minX = 99999;
            maxX = 0;
            minY = 99999;
            maxY = 0;
            foreach (KeyValuePair<int, Dictionary<int, LocationGridTileSettings>> kvp in allSettings) {
                if (kvp.Key < minX) { minX = kvp.Key; }
                else if (kvp.Key > maxX) { maxX = kvp.Key; }
                foreach (KeyValuePair<int, LocationGridTileSettings> kvp2 in kvp.Value) {
                    if (kvp2.Key < minY) { minY = kvp2.Key; } 
                    else if (kvp2.Key > maxY) { maxY = kvp2.Key; }
                }
            }
            maxX += 1;
            maxY += 1; //because collections start at 0, and I need the max length of the collections.
        }
        private Dictionary<int, Dictionary<int, LocationGridTileSettings>> ShiftSettingsBy(Vector2Int shiftBy, Dictionary<int, Dictionary<int, LocationGridTileSettings>> settings) {
            Dictionary<int, Dictionary<int, LocationGridTileSettings>> shifted = new Dictionary<int, Dictionary<int, LocationGridTileSettings>>();
            foreach (KeyValuePair<int, Dictionary<int, LocationGridTileSettings>> kvp in settings) {
                foreach (KeyValuePair<int, LocationGridTileSettings> kvp2 in kvp.Value) {
                    int shiftedX = kvp.Key + shiftBy.x;
                    int shiftedY = kvp2.Key + shiftBy.y;
                    LocationGridTileSettings currSetting = kvp2.Value;
                    if (!shifted.ContainsKey(shiftedX)) {
                        shifted.Add(shiftedX, new Dictionary<int, LocationGridTileSettings>());
                    }
                    shifted[shiftedX].Add(shiftedY, currSetting);
                }
            }
            return shifted;
        }
        public Dictionary<int, Dictionary<int, LocationGridTileSettings>> GenerateTownCenterTemplateForGeneration(StructureTemplate template, Vector3Int startPos) {
            Dictionary<int, Dictionary<int, LocationGridTileSettings>> generated = new Dictionary<int, Dictionary<int, LocationGridTileSettings>>();
            for (int i = 0; i < template.groundTiles.Length; i++) {
                TileTemplateData ground = template.groundTiles[i];
                Vector3 tilePos = ground.tilePosition;

                BuildingSpotData buildingSpot;
                bool hasBuildingSpot = template.TryGetBuildingSpotDataAtLocation(tilePos, out buildingSpot);

                tilePos.x += startPos.x;
                tilePos.y += startPos.y;
                TileTemplateData detail = template.detailTiles[i];
                TileTemplateData groundWall;
                if (template.groundWallTiles != null) {
                    groundWall = template.groundWallTiles[i];
                } else {
                    groundWall = TileTemplateData.Empty;
                }
            
                TileTemplateData structureWall = template.structureWallTiles[i];
                TileTemplateData obj = template.objectTiles[i];
                if (!generated.ContainsKey((int)tilePos.x)) {
                    generated.Add((int)tilePos.x, new Dictionary<int, LocationGridTileSettings>());
                }
                generated[(int)tilePos.x].Add((int)tilePos.y, new LocationGridTileSettings() {
                    groundTile = ground,
                    groundWallTile = groundWall,
                    detailTile = detail,
                    structureWallTile = structureWall,
                    objectTile = obj,
                    hasBuildingSpot = hasBuildingSpot,
                    buildingSpot = buildingSpot
                });
            }
            return generated;

        }
        #endregion

        #region Tile Object
        public bool HasSettingForTileObjectAsset(Sprite asset) {
            return tileObjectSlotSettings.ContainsKey(asset);
        }
        /// <summary>
        /// Get the slot settings for a given tile object asset.
        /// NOTE: should be used in conjunction with <see cref="HasSettingForTileObjectAsset"/> to check if any settings are available, since TileObjectSettings cannot be null.
        /// </summary>
        /// <param name="asset">The asset used by the tile object.</param>
        /// <returns>The list of slot settings</returns>
        public List<TileObjectSlotSetting> GetTileObjectSlotSettings(Sprite asset) {
            return tileObjectSlotSettings[asset];
        }
        public void AddTileObject(TileObject to) {
            if (!allTileObjects.ContainsKey(to.tileObjectType)) {
                allTileObjects.Add(to.tileObjectType, new List<TileObject>());
            }
            if (!allTileObjects[to.tileObjectType].Contains(to)) {
                allTileObjects[to.tileObjectType].Add(to);
            }
        }
        public void RemoveTileObject(TileObject to) {
            if (allTileObjects.ContainsKey(to.tileObjectType)) {
                allTileObjects[to.tileObjectType].Remove(to);
            }
        }
        public TileObject GetTileObject(TILE_OBJECT_TYPE type, int id) {
            if (allTileObjects.ContainsKey(type)) {
                for (int i = 0; i < allTileObjects[type].Count; i++) {
                    TileObject to = allTileObjects[type][i];
                    if(to.id == id) {
                        return to;
                    }
                }
            }
            return null;
        }
        public T CreateNewTileObject<T>(TILE_OBJECT_TYPE tileObjectType) where T : TileObject {
            var typeName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(tileObjectType.ToString());
            System.Type type = System.Type.GetType(typeName);
            if (type != null) {
                T obj = System.Activator.CreateInstance(type) as T;
                return obj;
            }
            throw new System.Exception($"Could not create new instance of tile object of type {tileObjectType}");
        }
        public TILE_OBJECT_TYPE GetTileObjectTypeFromTileAsset(Sprite sprite) {
            int index = sprite.name.IndexOf("#", StringComparison.Ordinal);
            string tileObjectName = sprite.name;
            if (index != -1) {
                tileObjectName = sprite.name.Substring(0, index);    
            }

            TILE_OBJECT_TYPE tileObjectType = (TILE_OBJECT_TYPE) System.Enum.Parse(typeof(TILE_OBJECT_TYPE), tileObjectName);
            return tileObjectType;
        }
        public void LoadInitialSettlementItems(Settlement settlement) {
            ////Reference: https://trello.com/c/Kuqt3ZSP/2610-put-2-healing-potions-in-the-warehouse-at-start-of-the-game
            LocationStructure mainStorage = settlement.mainStorage;
            for (int i = 0; i < 4; i++) {
                mainStorage.AddPOI(CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION));
            }
            for (int i = 0; i < 2; i++) {
                mainStorage.AddPOI(CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TOOL));
            }
        }
        #endregion

        #region Lighting
        public void UpdateLightBasedOnTime(GameDate date) {
            foreach (KeyValuePair<int, float> keyValuePair in lightSettings) {
                if (date.tick > keyValuePair.Key) {
                    areaMapLight.intensity = keyValuePair.Value;
                }
            }
        }
        private void CheckForChangeLight() {
            if (lightSettings.ContainsKey(GameManager.Instance.tick)) {
                StartCoroutine(TransitionLightTo(lightSettings[GameManager.Instance.tick]));
            }
        }
        private IEnumerator TransitionLightTo(float intensity) {
            while (true) {
                if (GameManager.Instance.isPaused) {
                    yield return null;
                }
                if (intensity > areaMapLight.intensity) {
                    areaMapLight.intensity += 0.05f;
                } else if (intensity < areaMapLight.intensity) {
                    areaMapLight.intensity -= 0.05f;
                }
                if (Mathf.Approximately(areaMapLight.intensity, intensity)) {
                    break;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        #endregion

        #region Structures
        public List<GameObject> GetStructurePrefabsForStructure(STRUCTURE_TYPE type) {
            return structurePrefabs[type];
        }
        #endregion

        #region Assets
        public Sprite GetTileObjectAsset(TileObject tileObject, POI_STATE state, BIOMES biome, bool corrupted = false) {
            if (tileObject.tileObjectType == TILE_OBJECT_TYPE.ARTIFACT) {
                Artifact artifact = tileObject as Artifact;
                if (ScriptableObjectsManager.Instance.artifactDataDictionary.ContainsKey(artifact.type)) {
                    return ScriptableObjectsManager.Instance.artifactDataDictionary[artifact.type].sprite;
                }
            } else {
                var assetDictionary = corrupted ? assetManager.corruptedTileObjectAssets : assetManager.tileObjectTiles;
                if (assetDictionary.ContainsKey(tileObject.tileObjectType) == false) {
                    //if the provided asset dictionary does not have assets for the tile object, then try and use the default asset dictionary.
                    assetDictionary = assetManager.tileObjectTiles;
                }
                if (assetDictionary.ContainsKey(tileObject.tileObjectType)) {
                    TileObjectTileSetting setting = assetDictionary[tileObject.tileObjectType];
                    BiomeTileObjectTileSetting biomeSetting = setting.biomeAssets.ContainsKey(biome) ? setting.biomeAssets[biome] 
                        : setting.biomeAssets[BIOMES.NONE];
                    return CollectionUtilities.GetRandomElement(state == POI_STATE.ACTIVE ? biomeSetting.activeTile : biomeSetting.inactiveTile);
                }
            }
            return null;
        }
        public Sprite GetTileObjectAsset(TileObject tileObject, POI_STATE state, bool corrupted = false) {
            if (tileObject.tileObjectType == TILE_OBJECT_TYPE.ARTIFACT) {
                Artifact artifact = tileObject as Artifact;
                if (ScriptableObjectsManager.Instance.artifactDataDictionary.ContainsKey(artifact.type)) {
                    return ScriptableObjectsManager.Instance.artifactDataDictionary[artifact.type].sprite;
                }
            } else {
                var assetDictionary = corrupted ? assetManager.corruptedTileObjectAssets : assetManager.tileObjectTiles;
                if (assetDictionary.ContainsKey(tileObject.tileObjectType) == false) {
                    //if the provided asset dictionary does not have assets for the tile object, then try and use the default asset dictionary.
                    assetDictionary = assetManager.tileObjectTiles;
                }
                if (assetDictionary.ContainsKey(tileObject.tileObjectType)) {
                    TileObjectTileSetting setting = assetDictionary[tileObject.tileObjectType];
                    BiomeTileObjectTileSetting biomeSetting = setting.biomeAssets[BIOMES.NONE];
                    return CollectionUtilities.GetRandomElement(state == POI_STATE.ACTIVE ? biomeSetting.activeTile : biomeSetting.inactiveTile);
                }
            }
            return null;
        }
        public WallAsset GetWallAsset(RESOURCE wallResource, string assetName) {
            return wallResourceAssets[wallResource].GetWallAsset(assetName);
        }
        #endregion

        #region Data Setting
        public void SetCurrentlyHoveredPOI(IPointOfInterest poi) {
            currentlyHoveredPoi = poi;
        }
        #endregion

        #region POI
        public void FaceTarget(IPointOfInterest actor, IPointOfInterest target) {
            if (actor != target && actor.gridTileLocation != null && target.gridTileLocation != null) {
                BaseMapObjectVisual objectToLookAt = target.mapObjectVisual;
                if(target.isBeingCarriedBy != null) {
                    objectToLookAt = target.isBeingCarriedBy.mapObjectVisual;
                }
                if (target.isBeingCarriedBy != actor && objectToLookAt != null) {
                    actor.mapObjectVisual.LookAt(objectToLookAt.transform.position);
                }
            }
        }
        public void FaceTarget(IPointOfInterest actor, LocationGridTile target) {
            if (actor != target && actor.gridTileLocation != null && target != null) {
                if (target != actor.gridTileLocation) {
                    actor.mapObjectVisual.LookAt(target.centeredWorldLocation);
                }
            }
        }
        #endregion
    }
}