using System;
using System.Collections.Generic;
using System.Linq;
using Cellular_Automata;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using Locations.Area_Features;
using Pathfinding;
using Ruinarch;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UtilityScripts;
namespace Inner_Maps {
    [ExecuteInEditMode]
    public class InnerMapManager : BaseMonoBehaviour {

        public static InnerMapManager Instance;
        
        public static readonly Vector2Int BuildingSpotSize = new Vector2Int(7, 7);
        public static readonly Vector2Int AreaLocationGridTileSize = new Vector2Int(14, 14);
        public const int DefaultCharacterSortingOrder = 82;
        public const int GroundTilemapSortingOrder = 10;
        public const int DetailsTilemapSortingOrder = 40;
        public const int SelectedSortingOrder = 900;

        /// <summary>
        /// At what tag index should randomly generated stuff start. i.e. tags per faction.
        /// </summary>
        public const int Starting_Tag_Index = 12;
        public uint currentTagIndex = Starting_Tag_Index;
   
        //tags
        public const int All_Tags = -1;
        public const int Ground_Tag = 1;
        public const int Obstacle_Tag = 2;
        public const int Demonic_Faction = 3;
        public const int Demonic_Faction_Doors = 4;
        public const int Undead_Faction = 5;
        public const int Undead_Faction_Doors = 6;
        public const int Ratmen_Faction = 7;
        public const int Ratmen_Faction_Doors = 8;
        public const int Roads = 9;
        public const int Caves = 10;
        public const int Special_Structures = 11;
        
        private Vector3 _nextMapPos = Vector3.zero;
        public GameObject characterCollisionTriggerPrefab;
        public GameObject dragonCollisionTriggerPrefab;

        [Header("Location Grid Tile")]
        public GameObject gridTileMouseEventsPrefab;

        [Header("Pathfinding")]
        [SerializeField] private AstarPath pathfinder;

        [Header("Tile Object")]
        [SerializeField] private TileObjectSlotDictionary tileObjectSlotSettings;
        public GameObject tileObjectSlotsParentPrefab;
        public GameObject tileObjectSlotPrefab;

        [Header("Tilemap Assets")] 
        public InnerMapAssetManager assetManager;
        [SerializeField] private WallResourceAssetDictionary wallResourceAssets; //wall assets categorized by resource.

        [Header("Effects")] 
        [SerializeField] private GameObject pfAreaMapTextPopup;

        //NPCSettlement Map Objects
        [FormerlySerializedAs("areaMapObjectFactory")] public MapVisualFactory mapObjectFactory;
        
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        public InnerTileMap currentlyShowingMap { get; private set; }
        public Region currentlyShowingLocation { get; private set; }
        public List<InnerTileMap> innerMaps { get; private set; }
        public IPointOfInterest currentlyHoveredPoi { get; private set; }
        public List<LocationGridTile> currentlyHighlightedTiles { get; private set; }
        public List<LocationStructure> worldKnownDemonicStructures { get; private set; }
        public GraphMask mainGraphMask { get; private set; }
        public List<PathfindingTagPair> unusedPathfindingTags { get; private set; }
        
        private LocationGridTile lastClickedTile;
        private Dictionary<TILE_OBJECT_TYPE, TileObjectScriptableObject> _tileObjectScriptableObjects;
        private float[] monsterLairSeeds = new[] {4f, 22f, 69f, 96f };
        
        #region getters
        public bool isAnInnerMapShowing => currentlyShowingMap != null;
        #endregion
        
        #region Monobehaviours
        private void Awake() {
            Instance = this;
            mainGraphMask = 0;
            _tileObjectScriptableObjects = new Dictionary<TILE_OBJECT_TYPE, TileObjectScriptableObject>();
        }
        public void LateUpdate() {
            if (GameManager.showAllTilesTooltip) {
                if (UIManager.Instance.IsMouseOnUI() || currentlyShowingMap == null) {
                    return;
                }
                LocationGridTile hoveredTile = GetTileFromMousePosition();
                if (hoveredTile != null && hoveredTile.tileObjectComponent.objHere == null) {
                    ShowTileData(hoveredTile);
                }
            }
        }
        private void OnKeyDown(KeyCode keyCode) {
            if (keyCode == KeyCode.Mouse0) {
                OnClickMapObject();
            } else if (keyCode == KeyCode.Mouse1) {
                OnRightClick();
            } else if (keyCode == KeyCode.Mouse2) {
                OnMiddleClick();
            }
        }
        private void OnRightClick() {
            if (!UIManager.Instance.IsMouseOnUI() && !ReferenceEquals(currentlyShowingMap, null) && GameManager.Instance.gameHasStarted) {
                LocationGridTile clickedTile = GetTileFromMousePosition();
                List<ISelectable> selectables = null;
                if (clickedTile != null && TryGetSelectablesOnTile(clickedTile, out selectables)) {
                    IPointOfInterest currentlySelectedPOI = UIManager.Instance.GetCurrentlySelectedPOI();
                    if (currentlySelectedPOI != null && selectables.Contains(currentlySelectedPOI)) {
                        currentlySelectedPOI.RightSelectAction();
                    } else {
                        ISelectable selectable = selectables.FirstOrDefault();
                        selectable?.RightSelectAction();
                    }
                }
                if (selectables != null) {
                    RuinarchListPool<ISelectable>.Release(selectables);    
                }
            }
        }
        private void OnMiddleClick() {
            if (UIManager.Instance.IsMouseOnUI() == false && ReferenceEquals(currentlyShowingMap, null) == false) {
                LocationGridTile clickedTile = GetTileFromMousePosition();
                ISelectable selectable = GetFirstSelectableOnTile(clickedTile);
                selectable?.MiddleSelectAction();
            }
        }
        private void OnClickMapObject() {
            if (!UIManager.Instance.IsMouseOnUI() && !ReferenceEquals(currentlyShowingMap, null) && GameManager.Instance.gameHasStarted && PlayerManager.Instance.player != null && !PlayerManager.Instance.player.IsPerformingPlayerAction()) { //(!GameManager.Instance.gameHasStarted || !PlayerManager.Instance.player.IsPerformingPlayerAction())
                LocationGridTile clickedTile = GetTileFromMousePosition();
                List<ISelectable> selectables = null;
                if (clickedTile != null && TryGetSelectablesOnTile(clickedTile, out selectables)) {
                    if (selectables.Count > 0) {
                        ISelectable objToSelect = null;
                        if (lastClickedTile != clickedTile) {
                            //if last tile that was clicked is not the tile that has been clicked, then instead of 
                            //looping through the selectables, just select the first one.
                            objToSelect = selectables[0];
                        }
                        else {
                            for (int i = 0; i < selectables.Count; i++) {
                                ISelectable currentSelectable = selectables[i];
                                if (currentSelectable.IsCurrentlySelected()) {
                                    //set next selectable in list to be selected.
                                    objToSelect = CollectionUtilities.GetNextElementCyclic(selectables, i);
                                    break;
                                }
                            }
                        }
                        if (objToSelect == null) {
                            objToSelect = selectables[0];
                        }
                        InputManager.Instance.Select(objToSelect);
                    }
                    lastClickedTile = clickedTile;
                }
                if (selectables != null) {
                    RuinarchListPool<ISelectable>.Release(selectables);    
                }
            }
        }
        private ISelectable GetFirstSelectableOnTile(LocationGridTile tile) {
            if(tile == null) {
                return null;
            }
            PointerEventData pointer = new PointerEventData(EventSystem.current) {position = Input.mousePosition};

            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0) { 
                foreach (var go in raycastResults) {
                    if (go.gameObject.CompareTag("Character Marker") || go.gameObject.CompareTag("Map Object")) {
                        BaseMapObjectVisual visual = go.gameObject.GetComponent<BaseMapObjectVisual>();
                        if (visual.IsInvisibleToPlayer()) {
                            continue; //skip
                        }
                        //assume that all objects that have the specified tags have the BaseMapObjectVisual class
                        if (visual.selectable != null && visual.selectable.CanBeSelected()) {
                            //this is meant for unbuilt structures
                            return visual.selectable;
                        }
                    }
                }
            }

            if (tile.structure != null) {
                if (tile.structure.IsTilePartOfARoom(tile, out var room) && room.CanBeSelected()) {
                    return room;
                }
                if (tile.structure.structureType.IsPlayerStructure()) {
                    if (tile.tileState == LocationGridTile.Tile_State.Occupied) {
                        return tile.structure;    
                    }
                } else {
                    if ((tile.structure is ManMadeStructure manMadeStructure && !ReferenceEquals(manMadeStructure.structureObj, null)) ||
                               (tile.structure is DemonicStructure demonicStructure && !ReferenceEquals(demonicStructure.structureObj, null)) && 
                               tile.structure is CityCenter == false) {
                        return tile.structure;    
                    } else if (tile.structure is Cave || tile.structure is MonsterLair) {
                        return tile.structure;    
                    }
                }
            }

            return null;
        }
        private bool TryGetSelectablesOnTile(LocationGridTile tile, out List<ISelectable> selectables) {
            selectables = RuinarchListPool<ISelectable>.Claim();

            PointerEventData pointer = new PointerEventData(EventSystem.current) {position = Input.mousePosition};

            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0) { 
                foreach (var go in raycastResults) {
                    if (go.gameObject.CompareTag("Character Marker") || go.gameObject.CompareTag("Map Object")) {
                        BaseMapObjectVisual visual = go.gameObject.GetComponent<BaseMapObjectVisual>();
                        if (visual.IsInvisibleToPlayer()) {
                            continue; //skip
                        }
                        //assume that all objects that have the specified tags have the BaseMapObjectVisual class
                        if (visual.selectable != null && visual.selectable.CanBeSelected()) {
                            selectables.Add(visual.selectable);    
                        }
                    } else if (go.gameObject.CompareTag("Location Structure Object")) {
                        LocationStructureObjectClickCollider structureObjectClickCollider = go.gameObject.GetComponent<LocationStructureObjectClickCollider>();
                        if (structureObjectClickCollider.structureObject != null && structureObjectClickCollider.structureObject.CanBeSelected()) {
                            //this is meant for unbuilt structures
                            selectables.Add(structureObjectClickCollider.structureObject);    
                        }
                    } else if (go.gameObject.CompareTag("Map_Click_Blocker")) {
                        return false; //click was blocked
                    }
                }
            }
            if (tile.structure != null) {
                if (tile.structure.IsTilePartOfARoom(tile, out var room) && room.CanBeSelected() && !selectables.Contains(tile.structure)) {
                    selectables.Add(room);
                }
                if (tile.structure.structureType.IsPlayerStructure()) {
                    if (tile.tileState == LocationGridTile.Tile_State.Occupied && !selectables.Contains(tile.structure)) {
                        selectables.Add(tile.structure);        
                    }
                } else {
                    if ((tile.structure is ManMadeStructure manMadeStructure && !ReferenceEquals(manMadeStructure.structureObj, null)) || //if man made structure check if structure object has not yet been destroyed 
                        (tile.structure is DemonicStructure demonicStructure && !ReferenceEquals(demonicStructure.structureObj, null)) && //if demonic structure structure check if structure object has not yet been destroyed
                        tile.structure is CityCenter == false) {
                        if (!selectables.Contains(tile.structure)) {
                            selectables.Add(tile.structure);    
                        }
                    } else if (tile.structure is Cave || tile.structure is MonsterLair) {
                        if (!selectables.Contains(tile.structure)) {
                            selectables.Add(tile.structure);    
                        }    
                    }
                }    
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
            innerMaps = new List<InnerTileMap>();
            worldKnownDemonicStructures = new List<LocationStructure>();
            mapObjectFactory = new MapVisualFactory();
            InnerMapCameraMove.Instance.Initialize();
            ConstructInitialUnusedPathfindingTags();
            Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnKeyDown);
        }
        /// <summary>
        /// Try and show the npcSettlement map of an npcSettlement. If it does not have one, this will generate one instead.
        /// </summary>
        /// <param name="location"></param>
        public void TryShowLocationMap(Region location) {
            Assert.IsNotNull(location.innerMap, $"{location.name} does not have a generated inner map");
            ShowInnerMap(location);
        }
        public void ShowInnerMap(Region location, bool centerCameraOnMapCenter = true, bool instantCenter = true) {
            // if (GameManager.Instance.gameHasStarted == false) {
            //     return;
            // }
            location.innerMap.Open();
            currentlyShowingMap = location.innerMap;
            currentlyShowingLocation = location;
            Messenger.Broadcast(RegionSignals.REGION_MAP_OPENED, location);

            if (centerCameraOnMapCenter) {
                InnerMapCameraMove.Instance.JustCenterCamera(instantCenter);
            }
        }
        public Region HideAreaMap() {
            if (currentlyShowingMap == null) {
                return null;
            }
            currentlyShowingMap.Close();
            Region closedLocation = currentlyShowingLocation;
            InnerMapCameraMove.Instance.CenterCameraOn(null);
            currentlyShowingMap = null;
            currentlyShowingLocation = null;
            // PlayerManager.Instance.player.SetCurrentlyActivePlayerJobAction(null);
            Messenger.Broadcast(RegionSignals.REGION_MAP_CLOSED, closedLocation);
            return closedLocation;
        }
        public void OnCreateInnerMap(InnerTileMap newMap) {
            innerMaps.Add(newMap);
            //newMap.transform.localPosition = nextMapPos;
            //set the next map position based on the new maps height
            newMap.transform.localPosition = _nextMapPos;
            newMap.UpdateTilesWorldPosition();
            PathfindingManager.Instance.CreatePathfindingGraphForLocation(newMap);
            mainGraphMask = mainGraphMask | GraphMask.FromGraph(newMap.pathfindingGraph);
            _nextMapPos = new Vector3(_nextMapPos.x, _nextMapPos.y + newMap.height + 50, _nextMapPos.z);
            newMap.OnMapGenerationFinished();
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
        public bool IsShowingInnerMap(Region location) {
            return location != null && isAnInnerMapShowing && location.innerMap == currentlyShowingMap;
        }
        public List<Vector3> GetTrimmedPath(Character character) {
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
            if (points.Count > 0) {
                for (int i = 0; i <= indexAt; i++) {
                    points.RemoveAt(0);
                }
            }
            return points;
        }
        public TileObjectScriptableObject GetTileObjectScriptableObject(TILE_OBJECT_TYPE p_tileObjectType) {
            if (!_tileObjectScriptableObjects.ContainsKey(p_tileObjectType)) {
                TileObjectScriptableObject loadedData = Resources.Load<TileObjectScriptableObject>($"Tile Object Data/{p_tileObjectType.ToString()}");
                if (loadedData == null) {
                    throw new Exception($"{p_tileObjectType} has no scriptable object!");
                }
                _tileObjectScriptableObjects.Add(p_tileObjectType, loadedData);
            }
            return _tileObjectScriptableObjects[p_tileObjectType];
        }
        #endregion

        #region For Testing
        public void ShowTileData(LocationGridTile tile, Character character = null) {
            if (!ConsoleBase.showPOIHoverData) {
                return;
            }
            if (tile == null) {
                return;
            }
            if (UIManager.Instance.poiTestingUI.gameObject.activeSelf && 
                (UIManager.Instance.poiTestingUI.gridTile == tile || UIManager.Instance.poiTestingUI.poi == tile.tileObjectComponent.objHere 
                || UIManager.Instance.poiTestingUI.poi == character)) {
                return; //do not show tooltip if right click menu is currently targeting the hovered object
            }

            //|| DEVELOPMENT_BUILD
#if UNITY_EDITOR
            Area area = tile.area;
            string summary = tile.localPlace.ToString();
            summary = $"{summary}\n{tile.centeredWorldLocation}";
            summary = $"{summary}\n<b>Area:</b>{(area.name ?? "None")}";
            summary = $"{summary}<b>Area Biome:</b>{area.biomeType.ToString()}";
            summary = $"{summary}<b>Area Elevation:</b>{(area.elevationType.ToString() ?? "None")}";
            summary = $"{summary}<b>Area Passable Tiles:</b>{area.gridTileComponent.passableTiles.Count.ToString()}";
            summary = $"{summary}<b>Settlement on Area:</b>{(area.settlementOnArea?.name ?? "None")}";
            summary = $"{summary}<b>Freezing Traps in Area:</b>{(area.freezingTraps.ToString())}";
            summary = $"{summary}<b>Structure Connectors on Area</b>({area.structureComponent.structureConnectors.Count.ToString()}): {area.structureComponent.structureConnectors.ComafyList()}";
            summary = $"{summary}\n<b>Area Features:</b>{area.featureComponent.features.ComafyList()}";
            summary = $"{summary}\n<b>Feature Details:</b>";
            for (int i = 0; i < area.featureComponent.features.Count; i++) {
                AreaFeature areaFeature = area.featureComponent.features[i];
                string testingData = areaFeature.GetTestingData();
                if (!string.IsNullOrEmpty(testingData)) {
                    summary = $"{summary}\n{testingData}";        
                }
            }
            
            summary = $"{summary}\n<b>Tile Biome:</b>{tile.mainBiomeType.ToString()}";
            summary = $"{summary}<b>Ground Map Asset Name:</b>{tile.groundTileMapAssetName}";
            summary = $"{summary}<b>Tile Elevation:</b>{tile.elevationType.ToString()}";
            summary = $"{summary}<b>Ground Type:</b>{tile.groundType.ToString()}";
            summary = $"{summary}<b>Is Occupied:</b>{tile.isOccupied.ToString()}";
            summary = $"{summary}<b>Tile Type:</b>{tile.tileType.ToString()}";
            summary = $"{summary}<b>Tile State:</b>{tile.tileState.ToString()}";
            summary = $"{summary}<b>Connectors on Tile:</b>{tile.connectorsOnTile.ToString()}";
            summary = $"{summary}<b>Current Tile Asset:</b>{(tile.parentTileMap.GetSprite(tile.localPlace)?.name ?? "Null")}";
            summary = $"{summary}<b>Has Mouse Events:</b>{tile.mouseEventsComponent.hasMouseEvents.ToString()}";

            summary = $"{summary}\nWalls: ";
            if (tile.tileObjectComponent.walls != null && tile.tileObjectComponent.walls.Count > 0) {
                for (int i = 0; i < tile.tileObjectComponent.walls.Count; i++) {
                    ThinWall thinWall = tile.tileObjectComponent.walls[i];
                    summary = $"{summary}\nWall {i.ToString()} - {thinWall.traitContainer.allTraitsAndStatuses.Keys.ToList().ComafyList()}";
                }
            } else {
                summary = $"{summary}None";
            }
            summary = $"{summary}\nCharacters Here: ";
            if (tile.charactersHere.Count > 0) {
                for (int i = 0; i < tile.charactersHere.Count; i++) {
                    Character c = tile.charactersHere[i];
                    summary = $"{summary}{c.name},";
                }
            } else {
                summary = $"{summary}None";
            }


            IPointOfInterest poi = tile.tileObjectComponent.objHere; //?? tile.tileObjectComponent.genericTileObject
            summary = $"{summary}\nContent: {poi}";
            if (poi != null) {
                summary = $"{summary}\nPUID: {poi.persistentID}";
                summary = $"{summary}\nHP: {poi.currentHP.ToString()}/{poi.maxHP.ToString()}";
                summary = $"{summary}<b>Object State:</b> {poi.state.ToString()}";
                summary = $"{summary}<b>Is Available:</b> {poi.IsAvailable().ToString()}";
                if (poi is TileObject tileObject) {
                    summary = $"{summary}<b>Character Owner:</b> {tileObject.characterOwner?.name}" ?? "None";
                    summary = $"{summary}<b>Faction Owner:</b> {tileObject.factionOwner?.name}" ?? "None";
                }
                if (poi is BaseMapObject baseMapObject) {
                    summary = $"{summary}{baseMapObject.GetAdditionalTestingData()}";
                }
                summary = $"{summary}\n\tAdvertised Actions: ";
                summary = poi.advertisedActions != null && poi.advertisedActions.Count > 0 ? poi.advertisedActions.Aggregate(summary, (current, t) => $"{current}|{t.ToString()}|") : $"{summary}None";
                
                summary = $"{summary}\n\tObject Traits: ";
                summary = poi.traitContainer.traits.Count > 0 ? poi.traitContainer.traits.Aggregate(summary, (current, t) => $"{current}\n\t\t- {t.name} - {t.GetTestingData(poi)}") : $"{summary}None";

                summary = $"{summary}\n\tObject Statuses: ";
                summary = poi.traitContainer.statuses.Count > 0 ? poi.traitContainer.statuses.Aggregate(summary, (current, t) => $"{current}\n\t\t- {t.name} - {t.GetTestingData(poi)}") : $"{summary}None";

                summary = $"{summary}\n\tJobs Targeting this: ";
                summary = poi.allJobsTargetingThis.Count > 0 ? poi.allJobsTargetingThis.Aggregate(summary, (current, t) => $"{current}\n\t\t- {t}") : $"{summary}None";
            }
            summary = tile.IsPartOfSettlement(out var settlement) ? $"{summary}\nSettlement: {settlement.name}" : $"{summary}\nSettlement: None";
            if (tile.structure != null) {
                summary = $"{summary}\nStructure: {tile.structure},Is Interior: {tile.structure.isInterior.ToString()}";
                // summary = $"{summary}\nOccupied Hex Tiles: {tile.structure.occupiedHexTiles.Count.ToString()}";
                summary = $"{summary}\nCharacters at {tile.structure}: ";
                if (tile.structure.charactersHere.Count > 0) {
                    for (int i = 0; i < tile.structure.charactersHere.Count; i++) {
                        Character currCharacter = tile.structure.charactersHere[i];
                        if (character == currCharacter) {
                            summary = $"{summary}\n{GetCharacterHoverData(currCharacter)}\n";
                        } else {
                            summary = $"{summary}{currCharacter.name},";
                        }
                    }
                } else {
                    summary = $"{summary}None";
                }
            } else {
                summary = $"{summary}\nStructure: None";
            }
            UIManager.Instance.ShowSmallInfo(summary, autoReplaceText: false);
#else
         //For build only
        TileObject tileObject = tile.tileObjectComponent.objHere;
        if (character == null && tileObject != null) {
            string tooltip = tileObject.name;
            if (tileObject.users != null && tileObject.users.Length > 0) {
                tooltip += " used by:";
                for (int i = 0; i < tileObject.users.Length; i++) {
                    Character user = tileObject.users[i];
                    if (user != null) {
                        tooltip += $"\n\t- {user.name}";
                    }
                }
            }
            UIManager.Instance.ShowSmallInfo(tooltip, autoReplaceText: false);
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
            //summary = $"{summary} <b>Supply:</b>{character.supply.ToString()}";
            summary = $"{summary} <b>Can Move:</b>{character.limiterComponent.canMove.ToString()}";
            summary = $"{summary} <b>Can Witness:</b>{character.limiterComponent.canWitness.ToString()}";
            summary = $"{summary} <b>Can Be Attacked:</b>{character.limiterComponent.canBeAttacked.ToString()}";
            summary = $"{summary} <b>Move Speed:</b>{character.marker.pathfindingAI.speed.ToString()}";
            summary = $"{summary} <b>Attack Range:</b>{character.characterClass.attackRange.ToString()}";
            summary = $"{summary} <b>Attack Speed:</b>{character.combatComponent.attackSpeed.ToString()}";
            summary = $"{summary} <b>Target POI:</b>{(character.marker.targetPOI?.name ?? "None")}";
            summary =
                $"{summary} <b>Base Structure:</b>{(character.trapStructure.structure != null ? character.trapStructure.structure.ToString() : "None")}";
            summary =
                $"{summary} <b>Actions Being Performed on this:</b>{character.numOfActionsBeingPerformedOnThis.ToString()}";

            summary = $"{summary}Destination Tile: ";
            summary = character.marker.destinationTile == null ? $"{summary}None" : $"{summary}{character.marker.destinationTile} at {character.marker.destinationTile.parentMap.region.name}";
            
            summary = $"{summary}\n\tCharacters that have reacted to me: ";
            summary = character.defaultCharacterTrait.charactersThatHaveReactedToThis.Count > 0 ? character.defaultCharacterTrait.charactersThatHaveReactedToThis.Aggregate(summary, (current, c) => $"{current}{c.name}, ") : $"{summary}None";
            //
            // summary = $"{summary}\n\tPersonal Job Queue: ";
            // summary = character.jobQueue.jobsInQueue.Count > 0 ? character.jobQueue.jobsInQueue.Aggregate(summary, (current, poi) => $"{current}{poi}, ") : $"{summary}None";
            
            summary = $"{summary}\n\tHostiles in Range: ";
            summary = character.combatComponent.hostilesInRange.Count > 0 ? character.combatComponent.hostilesInRange.Aggregate(summary, (current, poi) => $"{current}{poi.name}, ") : $"{summary}None";
            
            summary = $"{summary}\n\tAvoid in Range: ";
            summary = character.combatComponent.avoidInRange.Count > 0 ? character.combatComponent.avoidInRange.Aggregate(summary, (current, poi) => $"{current}{poi.name}, ") : $"{summary}None";
            
            summary = $"{summary}\n\tPOI's in Vision: ";
            summary = character.marker.inVisionPOIs.Count > 0 ? character.marker.inVisionPOIs.Aggregate(summary, (current, poi) => $"{current}{poi}, ") : $"{summary}None";
            
            summary = $"{summary}\n\tCharacters in Vision: ";
            summary = character.marker.inVisionCharacters.Count > 0 ? character.marker.inVisionCharacters.Select((t, i) => (Character) character.marker.inVisionCharacters.ElementAt(i)).Aggregate(summary, (current, poi) => $"{current}{poi.name}, ") : $"{summary}None";
            
            summary = $"{summary}\n\tPOI's in Range but different structures: ";
            summary = character.marker.inVisionPOIsButDiffStructure.Count > 0 ? character.marker.inVisionPOIsButDiffStructure.Aggregate(summary, (current, poi) => $"{current}{poi}, ") : $"{summary}None";

            return summary;
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
        public TileObject GetTileObject(TILE_OBJECT_TYPE type, int id) {
            return DatabaseManager.Instance.tileObjectDatabase.GetTileObject(type, id);
        }
        public TileObject GetTileObjectByPersistentID(string id) {
            return DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(id);
        }
        public TileObject GetFirstTileObject(TILE_OBJECT_TYPE type) {
            return DatabaseManager.Instance.tileObjectDatabase.GetFirstTileObject(type);
        }
        public TileObject GetFirstArtifact(ARTIFACT_TYPE artifactType) {
            return DatabaseManager.Instance.tileObjectDatabase.GetFirstArtifact(artifactType);
        }
        public T CreateNewTileObject<T>(TILE_OBJECT_TYPE tileObjectType) where T : TileObject {
            var typeName = $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(tileObjectType.ToString())}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            System.Type type = System.Type.GetType(typeName);
            if (type != null) {
                T obj = System.Activator.CreateInstance(type) as T;
                return obj;
            }
            throw new System.Exception($"Could not create new instance of tile object of type {tileObjectType.ToString()}");
        }
        public T LoadTileObject<T>(SaveDataTileObject saveDataTileObject) where T : TileObject {
            var typeName = $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(saveDataTileObject.tileObjectType.ToString())}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            System.Type type = System.Type.GetType(typeName);
            if (type != null) {
                T obj = System.Activator.CreateInstance(type, saveDataTileObject) as T;
                return obj;
            }
            throw new System.Exception($"Could not create new instance of tile object of type {saveDataTileObject.tileObjectType}");
        }
        public T LoadTileObject<T>(SaveDataArtifact saveDataTileObject) where T : TileObject {
            var typeName = $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(saveDataTileObject.artifactType.ToString())}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            System.Type type = System.Type.GetType(typeName);
            if (type != null) {
                T obj = System.Activator.CreateInstance(type, saveDataTileObject) as T;
                return obj;
            }
            throw new System.Exception($"Could not create new instance of tile object of type {saveDataTileObject.tileObjectType}");
        }
        public TILE_OBJECT_TYPE GetTileObjectTypeFromTileAsset(Sprite sprite) {
            int index = sprite.name.IndexOf("#", StringComparison.Ordinal);
            string tileObjectName = sprite.name;
            if (index != -1) {
                tileObjectName = sprite.name.Substring(0, index);    
            }
            tileObjectName = UtilityScripts.Utilities.NotNormalizedConversionStringToEnum(tileObjectName);
            TILE_OBJECT_TYPE tileObjectType = (TILE_OBJECT_TYPE) System.Enum.Parse(typeof(TILE_OBJECT_TYPE), tileObjectName);
            return tileObjectType;
        }
        public void LoadInitialSettlementItems(NPCSettlement npcSettlement) {
            ////Reference: https://trello.com/c/Kuqt3ZSP/2610-put-2-healing-potions-in-the-warehouse-at-start-of-the-game
            LocationStructure mainStorage = npcSettlement.mainStorage;
            for (int i = 0; i < 4; i++) {
                mainStorage.AddPOI(CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION));
            }
            for (int i = 0; i < 2; i++) {
                mainStorage.AddPOI(CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TOOL));
            }
            for (int i = 0; i < 2; i++) {
                mainStorage.AddPOI(CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ANTIDOTE));
            }
            
            //for (int i = 0; i < 2; i++) {
            //    mainStorage.AddPOI(CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.WATER_FLASK));
            //}
            for (int i = 0; i < 2; i++) {
                mainStorage.AddPOI(CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.POISON_FLASK));
            }
            //mainStorage.AddPOI(CreateNewArtifact(ARTIFACT_TYPE.Necronomicon));
            //mainStorage.AddPOI(CreateNewArtifact(ARTIFACT_TYPE.Berserk_Orb));
            //mainStorage.AddPOI(CreateNewArtifact(ARTIFACT_TYPE.Ankh_Of_Anubis));

            //for (int i = 0; i < 2; i++) {
            //    mainStorage.AddPOI(CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.EMBER));
            //}
        }
        public T CreateNewResourcePileAndTryCreateHaulJob<T>(TILE_OBJECT_TYPE tileObjectType, int resourcesInPile, 
            [NotNull]Character creator, [NotNull]LocationGridTile locationGridTile) where T : ResourcePile {
            T resourcePile = CreateNewTileObject<T>(tileObjectType);
            resourcePile.SetResourceInPile(resourcesInPile);
            locationGridTile.structure.AddPOI(resourcePile, locationGridTile);

            if (creator.homeSettlement != null) {
                creator.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(resourcePile);
                creator.marker.AddPOIAsInVisionRange(resourcePile); //automatically add pile to character's vision so he/she can take haul job immediately after
            }
            return resourcePile;
        }
        public void CreateWurmHoles(LocationGridTile point1, LocationGridTile point2) {
            WurmHole hole1 = CreateNewTileObject<WurmHole>(TILE_OBJECT_TYPE.WURM_HOLE);
            WurmHole hole2 = CreateNewTileObject<WurmHole>(TILE_OBJECT_TYPE.WURM_HOLE);

            hole1.SetWurmHoleConnection(hole2);
            hole2.SetWurmHoleConnection(hole1);

            point1.structure.AddPOI(hole1, point1);
            point2.structure.AddPOI(hole2, point2);
        }
        #endregion

        #region Structures
        public List<GameObject> GetStructurePrefabsForStructure(STRUCTURE_TYPE p_structureType, RESOURCE resource) {
            StructureSetting structureSetting = new StructureSetting(p_structureType, resource);
            return GetStructurePrefabsForStructure(structureSetting);
        }
        public List<GameObject> GetStructurePrefabsForStructure(StructureSetting structureSetting) {
            return LandmarkManager.Instance.GetStructureData(structureSetting.structureType).GetStructurePrefabs(structureSetting);
            // if (structureSetting.isCorrupted) {
            //     if (corruptedStructurePrefabs.ContainsKey(structureSetting)) {
            //         return corruptedStructurePrefabs[structureSetting];    
            //     }
            // } else {
            //     if (individualStructurePrefabs.ContainsKey(structureSetting)) {
            //         return individualStructurePrefabs[structureSetting];    
            //     }
            // }
            // throw new Exception($"No structure prefabs for {structureSetting.ToString()}");
        }
        public GameObject GetFirstStructurePrefabForStructure(StructureSetting structureSetting) {
            List<GameObject> choices = LandmarkManager.Instance.GetStructureData(structureSetting.structureType).GetStructurePrefabs(structureSetting);
            return choices.First();
        }
        public void AddWorldKnownDemonicStructure(LocationStructure structure) {
            worldKnownDemonicStructures.Add(structure);
        }
        public void RemoveWorldKnownDemonicStructure(LocationStructure structure) {
            worldKnownDemonicStructures.Remove(structure);
        }
        public bool HasWorldKnownDemonicStructure(LocationStructure structure) {
            return worldKnownDemonicStructures.Contains(structure);
        }
        public bool HasExistingWorldKnownDemonicStructure() {
            return worldKnownDemonicStructures.Count > 0;
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
                TileObjectScriptableObject tileObjectScriptableObject = GetTileObjectScriptableObject(tileObject.tileObjectType);
                TileObjectTileSetting setting = corrupted ? tileObjectScriptableObject.corruptedTileObjectAssets : tileObjectScriptableObject.tileObjectAssets;
                if (setting.biomeAssets.Count <= 0) {
                    //if in case tile object does not have a corrupted version, use normal assets instead. 
                    setting = tileObjectScriptableObject.tileObjectAssets;
                }
                BiomeTileObjectTileSetting biomeSetting = setting.biomeAssets.ContainsKey(biome) ? setting.biomeAssets[biome] : setting.biomeAssets[BIOMES.NONE];
                return CollectionUtilities.GetRandomElement(state == POI_STATE.ACTIVE ? biomeSetting.activeTile : biomeSetting.inactiveTile);
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
                TileObjectScriptableObject tileObjectScriptableObject = GetTileObjectScriptableObject(tileObject.tileObjectType);
                TileObjectTileSetting setting = corrupted ? tileObjectScriptableObject.corruptedTileObjectAssets : tileObjectScriptableObject.tileObjectAssets;
                if (setting.biomeAssets.Count <= 0) {
                    //if in case tile object does not have a corrupted version, use normal assets instead. 
                    setting = tileObjectScriptableObject.tileObjectAssets;
                }
                BiomeTileObjectTileSetting biomeSetting = setting.biomeAssets[BIOMES.NONE];
                return CollectionUtilities.GetRandomElement(state == POI_STATE.ACTIVE ? biomeSetting.activeTile : biomeSetting.inactiveTile);
                
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
        public bool IsPOIConsideredTheCurrentHoveredPOI(IPointOfInterest poi) {
            if (currentlyHoveredPoi == poi) {
                return true;
            } else if (currentlyHoveredPoi is Tombstone tombstone) {
                return tombstone.character == poi;
            }
            return false;
        }
        #endregion

        #region POI
        public void FaceTarget(IPointOfInterest actor, IPointOfInterest target) {
            if (actor != target && actor != null && target != null && actor.gridTileLocation != null && target.gridTileLocation != null) {
                BaseMapObjectVisual objectToLookAt = target.mapObjectVisual;
                if(target.isBeingCarriedBy != null) {
                    objectToLookAt = target.isBeingCarriedBy.mapObjectVisual;
                }
                if (target.isBeingCarriedBy != actor && ReferenceEquals(objectToLookAt, null) == false) {
                    actor.mapObjectVisual.LookAt(objectToLookAt.transform.position);
                }
            }
        }
        public void FaceTarget(IPointOfInterest actor, LocationGridTile target) {
            if (actor != null && target != null && actor.gridTileLocation != null) {
                if (target != actor.gridTileLocation) {
                    actor.mapObjectVisual.LookAt(target.centeredWorldLocation);
                }
            }
        }
        #endregion
        
        #region Artifacts
        public Artifact CreateNewArtifact(ARTIFACT_TYPE artifactType) {
            Artifact newArtifact = CreateNewArtifactFromType(artifactType);
            return newArtifact;
            //return new Artifact(artifactType);
        }
        //public Artifact CreateNewArtifact(SaveDataArtifact data) {
        //    Artifact newArtifact = CreateNewArtifactClassFromType(data) as Artifact;
        //    return newArtifact;
        //}
        private Artifact CreateNewArtifactFromType(ARTIFACT_TYPE artifactType) {
            var typeName = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(artifactType.ToString())}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            return System.Activator.CreateInstance(System.Type.GetType(typeName)) as Artifact;
        }
        //private object CreateNewArtifactClassFromType(SaveDataArtifact data) {
        //    var typeName = UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(data.artifactType.ToString());
        //    return System.Activator.CreateInstance(System.Type.GetType(typeName), data);
        //}
        #endregion

        #region Monster Lair
        public void MonsterLairCellAutomata(List<LocationGridTile> locationGridTiles, LocationStructure structure, Region region, LocationStructure wilderness) {
            // List<LocationGridTile> refinedTiles = ObjectPoolManager.Instance.CreateNewGridTileList();
            // for (int i = 0; i < locationGridTiles.Count; i++) {
            //     LocationGridTile gridTile = locationGridTiles[i];
            //     if (!gridTile.IsAtEdgeOfMap()) {
            //         refinedTiles.Add(gridTile);
            //     }
            // }
            List<LocationGridTile> tilesToAutomata = RuinarchListPool<LocationGridTile>.Claim();
            for (int i = 0; i < locationGridTiles.Count; i++) {
                LocationGridTile gridTile = locationGridTiles[i];
                if (!gridTile.area.gridTileComponent.borderTiles.Contains(gridTile)) {
                    tilesToAutomata.Add(gridTile);
                }
            }
            // tilesToAutomata.AddRange(locationGridTiles);
            
		    LocationGridTile[,] tileMap = CellularAutomataGenerator.ConvertListToGridMap(tilesToAutomata);
            float seed = CollectionUtilities.GetRandomElement(monsterLairSeeds);
		    int[,] cellMap = CellularAutomataGenerator.GenerateMap(tileMap, tilesToAutomata, 2, 10, seed: seed.ToString());
            
		    Assert.IsNotNull(cellMap, $"There was no cellmap generated for monster lair structure {structure.ToString()}");
		    
		    CellularAutomataGenerator.DrawMap(tileMap, cellMap, assetManager.monsterLairWallTile, 
			    null, 
			    (locationGridTile) => SetAsWall(locationGridTile, structure),
			    (locationGridTile) => SetAsGround(locationGridTile, structure));

            
            List<LocationGridTile> tilesToRefine = RuinarchListPool<LocationGridTile>.Claim();
            tilesToRefine.AddRange(tilesToAutomata);
            //refine further
            for (int i = 0; i < tilesToRefine.Count; i++) {
			    LocationGridTile tile = tilesToRefine[i];
			    if (!tile.HasNeighbourOfType(LocationGridTile.Ground_Type.Flesh)) {
				    tile.SetStructureTilemapVisual(null);
				    tile.SetTileType(LocationGridTile.Tile_Type.Empty);
				    tile.SetTileState(LocationGridTile.Tile_State.Empty);
				    tile.RevertTileToOriginalPerlin();
				    tile.SetStructure(wilderness);
                    tilesToAutomata.Remove(tile);
			    }
		    }
            RuinarchListPool<LocationGridTile>.Release(tilesToRefine);

            MonsterLairPerlin(tilesToAutomata, structure, seed, seed);

            //create entrances
		    //get tiles that are at the edge of the given tiles, but are not at the edge of its map.
            List<LocationGridTile> targetChoices = RuinarchListPool<LocationGridTile>.Claim();
            for (int i = 0; i < tilesToAutomata.Count; i++) {
                LocationGridTile t = tilesToAutomata[i];
                if (t.tileType == LocationGridTile.Tile_Type.Wall && !t.IsAtEdgeOfMap() &&
                    t.HasDifferentStructureNeighbour(true) && 
                    t.GetCountNeighboursOfType(LocationGridTile.Tile_Type.Wall, true) == 2 && 
                    t.GetCountNeighboursOfType(LocationGridTile.Tile_Type.Empty, true) == 2) {
                    targetChoices.Add(t);
                }
            }
            for (int i = 0; i < 5; i++) {
                if (targetChoices.Count > 0) {
				    LocationGridTile target = CollectionUtilities.GetRandomElement(targetChoices);
				    target.SetStructureTilemapVisual(null);
				    target.SetTileType(LocationGridTile.Tile_Type.Empty);
				    target.SetStructure(wilderness);
				    target.RevertTileToOriginalPerlin();
                    targetChoices.Remove(target);
			    } else {
				    Debug.LogWarning($"Could not find entrance for {structure}");
				    break;
			    }
		    }
		    RuinarchListPool<LocationGridTile>.Release(targetChoices);
            
		    for (int i = 0; i < tilesToAutomata.Count; i++) {
			    LocationGridTile tile = tilesToAutomata[i];
			    if (tile.tileObjectComponent.objHere == null && tile.tileType == LocationGridTile.Tile_Type.Wall) {
				    //create wall tile object for all walls
				    BlockWall blockWall = CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
				    blockWall.SetWallType(WALL_TYPE.Flesh);
				    structure.AddPOI(blockWall, tile);
			    }
		    }
            RuinarchListPool<LocationGridTile>.Release(tilesToAutomata);
	    }
	    private void SetAsWall(LocationGridTile tile, LocationStructure structure) {
            // if (GameManager.Instance.gameHasStarted) {
                if (tile.tileObjectComponent.objHere != null) {
                    tile.structure.RemovePOI(tile.tileObjectComponent.objHere);
                }
                tile.CreateSeamlessEdgesForSelfAndNeighbours();
            // }
            if (!GameManager.Instance.gameHasStarted) { tile.parentMap.detailsTilemap.SetTile(tile.localPlace, null); }
		    tile.SetGroundTilemapVisual(assetManager.monsterLairGroundTile);	
		    tile.SetTileType(LocationGridTile.Tile_Type.Wall);
		    tile.SetTileState(LocationGridTile.Tile_State.Occupied);
		    tile.SetStructure(structure);
	    }
	    private void SetAsGround(LocationGridTile tile, LocationStructure structure) {
            // if (GameManager.Instance.gameHasStarted) {
                if (tile.tileObjectComponent.objHere != null) {
                    tile.structure.RemovePOI(tile.tileObjectComponent.objHere);
                }
                tile.CreateSeamlessEdgesForSelfAndNeighbours();
            // }
            if (!GameManager.Instance.gameHasStarted) { tile.parentMap.detailsTilemap.SetTile(tile.localPlace, null); }
		    tile.SetStructure(structure);
		    tile.SetGroundTilemapVisual(assetManager.monsterLairGroundTile);
		    // tile.SetStructure(structure);
	    }
	    private void MonsterLairPerlin(List<LocationGridTile> tiles, LocationStructure structure, float seedX = -1f, float seedY = -1f) {
            float offsetX = seedX;
            float offsetY = seedY;
            
            if (offsetX <= 0f) { offsetX = UnityEngine.Random.Range(0f, 99999f); }
            if (offsetY <= 0f) { offsetY = UnityEngine.Random.Range(0f, 99999f); }

            int minX = tiles.Min(t => t.localPlace.x);
		    int maxX = tiles.Max(t => t.localPlace.x);
		    int minY = tiles.Min(t => t.localPlace.y);
		    int maxY = tiles.Max(t => t.localPlace.y);
		    int xSize = maxX - minX;
		    int ySize = maxY - minY;
		    for (int i = 0; i < tiles.Count; i++) {
			    LocationGridTile currTile = tiles[i];
			    float xCoord = (float) (currTile.localPlace.x - minX) / xSize * 5f + offsetX;
			    float yCoord = (float) (currTile.localPlace.y - minY) / ySize * 5f + offsetY;

			    float floorSample = Mathf.PerlinNoise(xCoord, yCoord);
			    if (floorSample <= 0.4f) {
				    SetAsWall(currTile, structure);
			    }
		    }
	    }
        #endregion

        #region Cycle Regions
        private void CycleRegions() {
            TryShowLocationMap(GridMap.Instance.mainRegion);
            //if (currentlyShowingLocation == null) {
            //    TryShowLocationMap(GridMap.Instance.allRegions[0]);
            //}
            //else {
            //    for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            //        Region region = GridMap.Instance.allRegions[i];
            //        if (currentlyShowingLocation == region) {
            //            Region nextRegion = CollectionUtilities.GetNextElementCyclic(GridMap.Instance.allRegions, i);
            //            ShowInnerMap(nextRegion, true, true);
            //            break;
            //        }
            //    }
               
            //}
        }
        #endregion

        #region Tags
        // public uint ClaimNextTag() {
        //     if (currentTagIndex > 31) {
        //         Debug.LogError("Max Tag limit has been reached! Could not claim new tags!");
        //         return 0; //always return 0 if ever tags run out, this should rarely happen!
        //     }
        //     uint claimedTag = currentTagIndex;
        //     currentTagIndex++;
        //     return claimedTag;
        // }
        public PathfindingTagPair ClaimNextPathfindingTagPair() {
            if (unusedPathfindingTags.Count > 0) {
                PathfindingTagPair pair = unusedPathfindingTags[0];
                unusedPathfindingTags.RemoveAt(0);
                return pair;
            }
            throw new Exception("No more pathfinding tags found!");
            // if (currentTagIndex > 31) {
            //     Debug.LogError("Max Tag limit has been reached! Could not claim new tags!");
            //     return 0; //always return 0 if ever tags run out, this should rarely happen!
            // }
            // uint claimedTag = currentTagIndex;
            // currentTagIndex++;
            // return claimedTag;
        }
        public void SetPathfindingTagPairAsClaimed(PathfindingTagPair p_pair) {
            for (int i = 0; i < unusedPathfindingTags.Count; i++) {
                PathfindingTagPair pair = unusedPathfindingTags[i];
                if (pair.Equals(p_pair)) {
                    unusedPathfindingTags.RemoveAt(i);
                    break;
                }
            }
        }
        public void ReturnPathfindingPair(Faction p_faction) {
            PathfindingTagPair pair = new PathfindingTagPair(p_faction.pathfindingTag, p_faction.pathfindingDoorTag);
            if (!unusedPathfindingTags.Contains(pair)) {
                unusedPathfindingTags.Add(pair);
#if DEBUG_LOG
                Debug.Log($"Returned pathfinding pair - {pair.ToString()} to pool. Updated pool is: {unusedPathfindingTags.ComafyList()}");
#endif
            }
        }
        private void ConstructInitialUnusedPathfindingTags() {
            unusedPathfindingTags = new List<PathfindingTagPair>();
            for (int i = Starting_Tag_Index; i < 32; i+= 2) {
                PathfindingTagPair pathfindingTagPair = new PathfindingTagPair((uint)i, (uint)i + 1);
                unusedPathfindingTags.Add(pathfindingTagPair);
            }
#if DEBUG_LOG
            Debug.Log($"Constructed initial unused pathfinding pairs {unusedPathfindingTags.ComafyList()}");
#endif
        }
        #endregion

        #region Moving Tile Objects
        public PoisonCloud SpawnPoisonCloud(LocationGridTile gridTileLocation, int stacks) {
            PoisonCloud poisonCloud = new PoisonCloud();
            poisonCloud.SetGridTileLocation(gridTileLocation);
            poisonCloud.OnPlacePOI();
            poisonCloud.SetStacks(stacks);
            return poisonCloud;
        }
        public PoisonCloud SpawnPoisonCloud(LocationGridTile gridTileLocation, int stacks, GameDate expiryDate) {
            PoisonCloud poisonCloud = SpawnPoisonCloud(gridTileLocation, stacks);
            poisonCloud.SetExpiryDate(expiryDate);
            return poisonCloud;
        }
        #endregion

        #region Effects
        public void ShowAreaMapTextPopup(string p_text, Vector3 p_worldPos, Color p_color) {
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(pfAreaMapTextPopup.name, p_worldPos, Quaternion.identity, transform, true);
            AreaMapTextPopup textPopup = go.GetComponent<AreaMapTextPopup>();
            textPopup.Show(p_text, p_worldPos, p_color);
        }
        #endregion

        #region Big Tree
        public bool CanBigTreeBePlacedOnTile(LocationGridTile tile) {
            if (tile.isOccupied) {
                return false;
            }
            if (tile.groundType == LocationGridTile.Ground_Type.Bone) {
                return false;
            }
            if (tile.structure != null && tile.structure.structureType.IsOpenSpace() == false) {
                return false;
            }
            if (tile.HasNeighbourOfType(LocationGridTile.Tile_Type.Wall)) {
                return false;
            }
            List<LocationGridTile> overlappedTiles = tile.parentMap.GetTiles(new Point(2, 2), tile);
            int invalidOverlap = overlappedTiles.Count(t => t.tileObjectComponent.objHere != null || t.tileType == LocationGridTile.Tile_Type.Wall || t.elevationType == ELEVATION.WATER);
            //|| t.partOfCollection.canBeBuiltOnByNPC == false

            return invalidOverlap <= 0;
        }
        public bool CanBigTreeBePlacedOnTileInRandomGeneration(LocationGridTile tile, MapGenerationData p_data) {
            if (tile.isOccupied) {
                return false;
            }
            if (tile.groundType == LocationGridTile.Ground_Type.Bone) {
                return false;
            }
            if (tile.structure != null && tile.structure.structureType.IsOpenSpace() == false) {
                return false;
            }
            if (tile.HasNeighbourOfType(LocationGridTile.Tile_Type.Wall)) {
                return false;
            }
            List<LocationGridTile> overlappedTiles = tile.parentMap.GetTiles(new Point(2, 2), tile);
            int invalidOverlap = 0;
            for (int i = 0; i < overlappedTiles.Count; i++) {
                LocationGridTile overlapped = overlappedTiles[i];
                if (overlapped.tileObjectComponent.objHere != null || overlapped.tileType == LocationGridTile.Tile_Type.Wall || p_data.GetGeneratedObjectOnTile(overlapped) != TILE_OBJECT_TYPE.NONE || overlapped.elevationType == ELEVATION.WATER) {
                    invalidOverlap++;
                }
            }
            return invalidOverlap <= 0;
        }
        #endregion

        protected override void OnDestroy() {
            if (Application.isPlaying) {
#if DEBUG_LOG
                Debug.Log("Cleaning up inner maps...");
#endif
                if (innerMaps != null) {
                    for (int i = 0; i < innerMaps.Count; i++) {
                        InnerTileMap innerTileMap = innerMaps[i];
                        pathfinder.data.RemoveGraph(innerTileMap.pathfindingGraph);    
                        innerTileMap?.CleanUp();
                    }
                    innerMaps?.Clear();    
                }
                Destroy(pathfinder);
                tileObjectSlotSettings?.Clear();
                wallResourceAssets?.Clear();
                _tileObjectScriptableObjects.Clear();
                _tileObjectScriptableObjects = null;
                base.OnDestroy();
                Instance = null;    
            }
        }
    }
}

[System.Serializable]
public struct PathfindingTagPair {
    public readonly uint groundTag;
    public readonly uint doorsTag;

    public PathfindingTagPair(uint p_groundTag, uint p_doorsTag) {
        groundTag = p_groundTag;
        doorsTag = p_doorsTag;
    }
    public override bool Equals(object obj) {
        if (obj is PathfindingTagPair pair) {
            return Equals(pair);
        }
        return false;
    }
    public bool Equals(PathfindingTagPair other) {
        return groundTag == other.groundTag && doorsTag == other.doorsTag;
    }
    public override int GetHashCode() {
        unchecked {
            return ((int) groundTag * 397) ^ (int) doorsTag;
        }
    }
    public override string ToString() {
        return $"Ground Tag: {groundTag.ToString()}. Door Tag: {doorsTag.ToString()}";
    }
}