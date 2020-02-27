using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actionables;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
namespace Inner_Maps.Location_Structures {
    [System.Serializable]
    public class LocationStructure : IPlayerActionTarget, ISelectable {
        public int id { get; private set; }
        public string name { get; private set; }
        public virtual string nameplateName => name;
        public STRUCTURE_TYPE structureType { get; private set; }
        public List<Character> charactersHere { get; private set; }
        public ILocation location { get; private set; }
        public Settlement settlementLocation { get; private set; }
        // public List<SpecialToken> itemsInStructure { get; private set; }
        public HashSet<IPointOfInterest> pointsOfInterest { get; private set; }
        public Dictionary<TILE_OBJECT_TYPE, TileObjectsAndCount> groupedTileObjects { get; private set; }
        public POI_STATE state { get; private set; }
        public LocationStructureObject structureObj {get; private set;}
        public BuildSpotTileObject occupiedBuildSpot { get; private set; }
        //Inner Map
        public List<LocationGridTile> tiles { get; private set; }
        public LinkedList<LocationGridTile> unoccupiedTiles { get; private set; }
        public bool isInterior { get; private set; }
        private bool _hasBeenDestroyed;

        #region getters
        public virtual bool isDwelling => false;
        public Vector3 worldPosition { get; protected set; }
        public virtual Vector2 selectableSize => structureObj.size;
        #endregion

        public LocationStructure(STRUCTURE_TYPE structureType, ILocation location) {
            id = UtilityScripts.Utilities.SetID(this);
            this.structureType = structureType;
            name = $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString())} {id.ToString()}";
            this.location = location;
            charactersHere = new List<Character>();
            // itemsInStructure = new List<SpecialToken>();
            pointsOfInterest = new HashSet<IPointOfInterest>();
            groupedTileObjects = new Dictionary<TILE_OBJECT_TYPE, TileObjectsAndCount>();
            tiles = new List<LocationGridTile>();
            unoccupiedTiles = new LinkedList<LocationGridTile>();
            SetInteriorState(structureType.IsInterior());
        }
        public LocationStructure(ILocation location, SaveDataLocationStructure data) {
            this.location = location;
            id = UtilityScripts.Utilities.SetID(this, data.id);
            structureType = data.structureType;
            name = data.name;
            charactersHere = new List<Character>();
            // itemsInStructure = new List<SpecialToken>();
            pointsOfInterest = new HashSet<IPointOfInterest>();
            groupedTileObjects = new Dictionary<TILE_OBJECT_TYPE, TileObjectsAndCount>();
            tiles = new List<LocationGridTile>();
            SetInteriorState(structureType.IsInterior());
        }

        #region Initialization
        public virtual void Initialize() {
            SubscribeListeners();
            ConstructDefaultActions();
        }
        #endregion

        #region Listeners
        protected virtual void SubscribeListeners() {
            if (structureType.HasWalls()) {
                Messenger.AddListener<StructureWallObject>(Signals.WALL_DAMAGED, OnWallDamaged);
                Messenger.AddListener<StructureWallObject>(Signals.WALL_DESTROYED, OnWallDestroyed);
                Messenger.AddListener<StructureWallObject>(Signals.WALL_REPAIRED, OnWallRepaired);
            }
        }
        protected virtual void UnsubscribeListeners() {
            if (structureType.HasWalls()) {
                Messenger.RemoveListener<StructureWallObject>(Signals.WALL_DAMAGED, OnWallDamaged);
                Messenger.RemoveListener<StructureWallObject>(Signals.WALL_DESTROYED, OnWallDestroyed);
                Messenger.RemoveListener<StructureWallObject>(Signals.WALL_REPAIRED, OnWallRepaired);
            }
        }
        #endregion

        #region Residents
        public virtual bool IsOccupied() {
            return false; //will only ever use this in dwellings, to prevent need for casting
        }
        #endregion

        #region Characters
        public void AddCharacterAtLocation(Character character, LocationGridTile tile = null) {
            if (!charactersHere.Contains(character)) {
                charactersHere.Add(character);
                //location.AddCharacterToLocation(character);
                AddPOI(character, tile);
            }
            character.SetCurrentStructureLocation(this);
        }
        public void RemoveCharacterAtLocation(Character character) {
            if (charactersHere.Remove(character)) {
                character.SetCurrentStructureLocation(null);
                RemovePOI(character);
            }
        }
        #endregion

        #region Points Of Interest
        public virtual bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null, bool placeObject = true) {
            if (!pointsOfInterest.Contains(poi)) {
                pointsOfInterest.Add(poi);
                if (placeObject) {
                    if (poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER) {
                        if (!PlaceAreaObjectAtAppropriateTile(poi, tileLocation)) {
                            pointsOfInterest.Remove(poi);
                            return false;
                        }
                    }
                }
                if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    TileObject tileObject = poi as TileObject;
                    if (groupedTileObjects.ContainsKey(tileObject.tileObjectType)) {
                        groupedTileObjects[tileObject.tileObjectType].AddTileObject(tileObject);
                    } else {
                        TileObjectsAndCount toac = new TileObjectsAndCount();
                        toac.AddTileObject(tileObject);
                        groupedTileObjects.Add(tileObject.tileObjectType, toac);
                    }
                    if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.buildSpotOwner.isPartOfParentRegionMap) {
                        // tileObject.SetFactionOwner(tileObject.gridTileLocation.buildSpotOwner.hexTileOwner.settlementOnTile.owner);
                        tileObject.gridTileLocation.buildSpotOwner.hexTileOwner.settlementOnTile?.OnItemAddedToLocation(tileObject, this);
                    }
                }
                return true;
            }
            return false;
        }
        public virtual bool RemovePOI(IPointOfInterest poi, Character removedBy = null) {
            if (pointsOfInterest.Remove(poi)) {
                if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    TileObject tileObject = poi as TileObject;
                    groupedTileObjects[tileObject.tileObjectType].RemoveTileObject(tileObject);
                    
                    if (poi.gridTileLocation.buildSpotOwner.isPartOfParentRegionMap) {
                        poi.gridTileLocation.buildSpotOwner.hexTileOwner.settlementOnTile?.OnItemRemovedFromLocation(tileObject, this);    
                    }
                }
                if (poi.gridTileLocation != null) {
                    // Debug.Log("Removed " + poi.ToString() + " from " + poi.gridTileLocation.ToString() + " at " + this.ToString());
                    if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                        //location.areaMap.RemoveCharacter(poi.gridTileLocation, poi as Character);
                    } else {
                        location.innerMap.RemoveObject(poi.gridTileLocation, removedBy);
                    }
                    //throw new System.Exception("Provided tile of " + poi.ToString() + " is null!");
                }
                return true;
            }
            return false;
        }
        public virtual bool RemovePOIWithoutDestroying(IPointOfInterest poi) {
            if (pointsOfInterest.Remove(poi)) {
                if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    TileObject tileObject = poi as TileObject;
                    groupedTileObjects[tileObject.tileObjectType].RemoveTileObject(tileObject);
                }
                if (poi.gridTileLocation != null) {
                    if (poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER) {
                        location.innerMap.RemoveObjectWithoutDestroying(poi.gridTileLocation);
                    }
                }
                return true;
            }
            return false;
        }
        public virtual bool RemovePOIDestroyVisualOnly(IPointOfInterest poi, Character remover = null) {
            if (pointsOfInterest.Remove(poi)) {
                if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    TileObject tileObject = poi as TileObject;
                    groupedTileObjects[tileObject.tileObjectType].RemoveTileObject(tileObject);
                }
                if (poi.gridTileLocation != null) {
                    if (poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER) {
                        location.innerMap.RemoveObjectDestroyVisualOnly(poi.gridTileLocation, remover);
                    }
                }
                return true;
            }
            return false;
        }
        public List<IPointOfInterest> GetPOIsOfType(POINT_OF_INTEREST_TYPE type) {
            List<IPointOfInterest> pois = new List<IPointOfInterest>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi.poiType == type) {
                    pois.Add(poi);
                }
            }
            return pois;
        }
        public List<TileObject> GetTileObjectsOfType(TILE_OBJECT_TYPE type) {
            List<TileObject> objs = new List<TileObject>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi is TileObject obj) {
                    if (obj.tileObjectType == type) {
                        objs.Add(obj);
                    }
                }
            }
            return objs;
        }
        public bool HasTileObjectOfType(TILE_OBJECT_TYPE type) {
            List<TileObject> objs = new List<TileObject>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi is TileObject obj) {
                    if (obj.tileObjectType == type) {
                        return true;
                    }
                }
            }
            return false;
        }
        public List<T> GetTileObjectsOfType<T>(TILE_OBJECT_TYPE type) where T : TileObject {
            List<T> objs = new List<T>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi is TileObject) {
                    TileObject obj = poi as TileObject;
                    if (obj.tileObjectType == type) {
                        objs.Add(obj as T);
                    }
                }
            }
            return objs;
        }
        public List<T> GetBuiltTileObjectsOfType<T>(TILE_OBJECT_TYPE type) where T : TileObject {
            List<T> objs = new List<T>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi is TileObject) {
                    TileObject obj = poi as TileObject;
                    if (obj.tileObjectType == type && obj.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                        objs.Add(obj as T);
                    }
                }
            }
            return objs;
        }
        public List<T> GetTileObjectsOfType<T>() where T : TileObject {
            List<T> objs = new List<T>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi is T) {
                    T obj = poi as T;
                    objs.Add(obj);
                }
            }
            return objs;
        }
        public T GetTileObjectOfType<T>(TILE_OBJECT_TYPE type) where T : TileObject{
            List<TileObject> objs = new List<TileObject>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi is TileObject) {
                    TileObject obj = poi as TileObject;
                    if (obj.tileObjectType == type) {
                        return obj as T;
                    }
                }
            }
            return null;
        }
        public int GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE type) {
            int count = 0;
            if (groupedTileObjects.ContainsKey(type)) {
                count = groupedTileObjects[type].count;
            }
            return count;
        }
        public ResourcePile GetResourcePileObjectWithLowestCount(TILE_OBJECT_TYPE type, bool excludeMaximum = true) {
            ResourcePile chosenPile = null;
            int lowestCount = 0;
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi is ResourcePile) {
                    ResourcePile obj = poi as ResourcePile;
                    if (excludeMaximum && obj.IsAtMaxResource(obj.providedResource)) {
                        continue; //skip
                    }
                    if (obj.tileObjectType == type) {
                        if(chosenPile == null || obj.resourceInPile <= lowestCount) {
                            chosenPile = obj;
                            lowestCount = obj.resourceInPile;
                        }
                    }
                }
            }
            return chosenPile;
        }
        private bool PlaceAreaObjectAtAppropriateTile(IPointOfInterest poi, LocationGridTile tile) {
            if (tile != null) {
                location.innerMap.PlaceObject(poi, tile);
                return true;
            } else {
                List<LocationGridTile> tilesToUse;
                if (location.locationType == LOCATION_TYPE.DEMONIC_INTRUSION) { //player settlement
                    tilesToUse = tiles;
                } else {
                    tilesToUse = GetValidTilesToPlace(poi);
                }
                if (tilesToUse.Count > 0) {
                    LocationGridTile chosenTile = tilesToUse[Random.Range(0, tilesToUse.Count)];
                    location.innerMap.PlaceObject(poi, chosenTile);
                    return true;
                } 
                // else {
                //     Debug.LogWarning("There are no tiles at " + structureType.ToString() + " at " + location.name + " for " + poi.ToString());
                // }
            }
            return false;
        }
        private List<LocationGridTile> GetValidTilesToPlace(IPointOfInterest poi) {
            switch (poi.poiType) {
                case POINT_OF_INTEREST_TYPE.TILE_OBJECT:
                    if (poi is MagicCircle) {
                        return unoccupiedTiles.Where(x => !x.HasOccupiedNeighbour()
                                                          && x.groundType != LocationGridTile.Ground_Type.Cave 
                                                          && x.groundType != LocationGridTile.Ground_Type.Water
                                                          && x.buildSpotOwner.hexTileOwner 
                                                          && x.buildSpotOwner.hexTileOwner.elevationType == ELEVATION.PLAIN
                                                          && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Wall) 
                                                          && !x.HasNeighbourOfType(LocationGridTile.Ground_Type.Cave)
                                                          && !x.HasNeighbourOfType(LocationGridTile.Ground_Type.Water)
                                                          && !x.HasNeighbourOfElevation(ELEVATION.MOUNTAIN)
                                                          && !x.HasNeighbourOfElevation(ELEVATION.WATER)
                        ).ToList();
                    } else if (poi is WaterWell) {
                        return unoccupiedTiles.Where(x => !x.HasOccupiedNeighbour() && !x.GetTilesInRadius(3).Any(y => y.objHere is WaterWell) && !x.HasNeighbouringWalledStructure()).ToList();
                    } else if (poi is GoddessStatue) {
                        return unoccupiedTiles.Where(x => !x.HasOccupiedNeighbour() && !x.GetTilesInRadius(3).Any(y => y.objHere is GoddessStatue) && !x.HasNeighbouringWalledStructure()).ToList();
                    } else if (poi is Guitar || poi is Bed || poi is Table) {
                        return GetOuterTiles().Where(x => unoccupiedTiles.Contains(x) && x.tileType != LocationGridTile.Tile_Type.Structure_Entrance).ToList();
                    } else {
                        return unoccupiedTiles.Where(x => x.tileType != LocationGridTile.Tile_Type.Structure_Entrance).ToList(); ;
                    }
                case POINT_OF_INTEREST_TYPE.CHARACTER:
                    return unoccupiedTiles.ToList();
                default:
                    return unoccupiedTiles.Where(x => !x.IsAdjacentTo(typeof(MagicCircle)) && x.tileType != LocationGridTile.Tile_Type.Structure_Entrance).ToList();
            }
        }
        // public void OwnTileObjectsInLocation(Faction owner) {
        //     for (int i = 0; i < pointsOfInterest.Count; i++) {
        //         if (pointsOfInterest[i].poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
        //             (pointsOfInterest[i] as TileObject).SetFactionOwner(owner);
        //         }
        //     }
        // }
        #endregion   
    
        #region Tiles
        public void AddTile(LocationGridTile tile) {
            if (!tiles.Contains(tile)) {
                tiles.Add(tile);
                if(tile.tileState == LocationGridTile.Tile_State.Empty) {
                    AddUnoccupiedTile(tile);
                } else {
                    RemoveUnoccupiedTile(tile);
                }
                if (structureType != STRUCTURE_TYPE.WILDERNESS && tile.IsPartOfSettlement(out var settlement)) {
                    SetSettlementLocation(settlement);
                }
            }
        }
        public void RemoveTile(LocationGridTile tile) {
            tiles.Remove(tile);
            RemoveUnoccupiedTile(tile);
        }
        public void AddUnoccupiedTile(LocationGridTile tile) {
            unoccupiedTiles.AddLast(tile);
        }
        public void RemoveUnoccupiedTile(LocationGridTile tile) {
            unoccupiedTiles.Remove(tile);
        }
        public LocationGridTile GetRandomTile() {
            if (tiles.Count <= 0) {
                return null;
            }
            return tiles[Random.Range(0, tiles.Count)];
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Get the structure's name based on specified rules.
        /// Rules are at - https://trello.com/c/mRzzH9BE/1432-location-naming-convention
        /// </summary>
        /// <param name="character">The character requesting the name</param>
        public virtual string GetNameRelativeTo(Character character) {
            switch (structureType) {
                case STRUCTURE_TYPE.INN:
                    return "the inn";
                case STRUCTURE_TYPE.WAREHOUSE:
                    return $"the {location.name} warehouse";
                case STRUCTURE_TYPE.PRISON:
                    return $"the {location.name} prison";
                case STRUCTURE_TYPE.WILDERNESS:
                    return $"the outskirts of {location.name}";
                case STRUCTURE_TYPE.CEMETERY:
                    return $"the cemetery of {location.name}";
                case STRUCTURE_TYPE.DUNGEON:
                case STRUCTURE_TYPE.WORK_AREA:
                case STRUCTURE_TYPE.EXPLORE_AREA:
                case STRUCTURE_TYPE.POND:
                    return location.name;
                case STRUCTURE_TYPE.CITY_CENTER:
                    return $"the {location.name} city center";
                default:
                    return
                        $"the {UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString())}";
            }
        }
        public List<LocationGridTile> GetOuterTiles() {
            List<LocationGridTile> outerTiles = new List<LocationGridTile>();
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                if (currTile.HasDifferentDwellingOrOutsideNeighbour()) {
                    outerTiles.Add(currTile);
                }
            }
            return outerTiles;
        }
        public void DoCleanup() {
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i);
                if (poi is TileObject) {
                    (poi as TileObject).DoCleanup();
                }
            }
        }
        public void SetSettlementLocation(Settlement settlement) {
            settlementLocation = settlement;
        }
        public void SetInteriorState(bool _isInterior) {
            isInterior = _isInterior;
        }
        #endregion

        #region Tile Objects
        protected List<TileObject> GetTileObjects() {
            List<TileObject> objs = new List<TileObject>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest currPOI = pointsOfInterest.ElementAt(i);
                if (currPOI is TileObject poi) {
                    objs.Add(poi);
                }
            }
            return objs;
        }
        public List<TileObject> GetTileObjectsThatAdvertise(params INTERACTION_TYPE[] types) {
            List<TileObject> objs = new List<TileObject>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest currPOI = pointsOfInterest.ElementAt(i);
                if (currPOI is TileObject) {
                    TileObject obj = currPOI as TileObject;
                    if (obj.IsAvailable() && obj.AdvertisesAll(types)) {
                        objs.Add(obj);
                    }
                }
            }
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                if (currTile.genericTileObject.IsAvailable() && currTile.genericTileObject.AdvertisesAll(types)) {
                    objs.Add(currTile.genericTileObject);
                }
            }
            return objs;
        }
        public TileObject GetUnoccupiedTileObject(params TILE_OBJECT_TYPE[] type) {
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi.IsAvailable() && poi is TileObject) {
                    TileObject tileObj = poi as TileObject;
                    if (type.Contains(tileObj.tileObjectType) && tileObj.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                        return tileObj;
                    }
                }
            }
            return null;
        }
        #endregion

        #region Structure Objects
        public virtual void SetStructureObject(LocationStructureObject structureObj) {
            this.structureObj = structureObj;
            Vector3 position = structureObj.transform.position;
            position.x -= 0.5f;
            position.y -= 0.5f;
            worldPosition = position;
        }
        public void SetOccupiedBuildSpot(BuildSpotTileObject buildSpotTileObject) {
            occupiedBuildSpot = buildSpotTileObject;
        }
        private void OnClickStructure() {
            Selector.Instance.Select(this);
        }
        #endregion

        #region Destroy
        protected virtual void DestroyStructure() {
            if (_hasBeenDestroyed) {
                return;
            }
            Debug.Log($"{GameManager.Instance.TodayLogString()}{ToString()} was destroyed!");
        
            if (settlementLocation != null) {
                Settlement settlement = settlementLocation;
                JobQueueItem existingRepairJob = settlement.GetJob(JOB_TYPE.REPAIR, occupiedBuildSpot);
                if (existingRepairJob != null) {
                    settlement.RemoveFromAvailableJobs(existingRepairJob);
                }    
            }
        
            //transfer tiles to either the wilderness or work settlement
            List<LocationGridTile> tilesInStructure = new List<LocationGridTile>(tiles);
            LocationStructure wilderness = location.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            for (int i = 0; i < tilesInStructure.Count; i++) {
                LocationGridTile tile = tilesInStructure[i];
                LocationStructure transferTo = wilderness;
            
                tile.ClearWallObjects();
                IPointOfInterest obj = tile.objHere;
                if (obj != null) {
                    obj.AdjustHP(-tile.objHere.maxHP, ELEMENTAL_TYPE.Normal);
                    obj.gridTileLocation?.structure.RemovePOI(obj); //because sometimes adjusting the hp of the object to 0 does not remove it?
                }
                
                tile.SetStructure(transferTo);
                tile.RevertToPreviousGroundVisual();
                tile.CreateSeamlessEdgesForTile(location.innerMap);
                tile.SetPreviousGroundVisual(null); //so that the tile will never revert to the structure tile, unless a new structure is put on it.
                tile.genericTileObject.AdjustHP(tile.genericTileObject.maxHP, ELEMENTAL_TYPE.Normal);
            }
        
            occupiedBuildSpot.RemoveOccupyingStructure(this);
            ObjectPoolManager.Instance.DestroyObject(structureObj);
            location.RemoveStructure(this);
            settlementLocation.RemoveStructure(this);
            Messenger.Broadcast(Signals.STRUCTURE_OBJECT_REMOVED, this, occupiedBuildSpot.spot);
            SetOccupiedBuildSpot(null);
            _hasBeenDestroyed = true;
        }
        private bool CheckIfStructureDestroyed() {
            //To check if a structure is destroyed, check if 50% of its walls have been destroyed.
            int neededWallsToBeConsideredValid = Mathf.FloorToInt(structureObj.walls.Length * 0.5f);
            int intactWalls = structureObj.walls.Count(wall => wall.currentHP > 0);
            if (intactWalls < neededWallsToBeConsideredValid) {
                //consider structure as destroyed
                DestroyStructure();
                return true;
            }
            return false;
        }
        #endregion

        #region Walls
        public void OnWallDestroyed(StructureWallObject structureWall) {
            //check if structure destroyed
            if (structureObj.walls.Contains(structureWall)) {
                structureWall.gridTileLocation.SetTileType(LocationGridTile.Tile_Type.Empty);
                structureObj.RescanPathfindingGridOfStructure();
                CheckInteriorState();
                CheckIfStructureDestroyed();
            }
        }
        public void OnWallRepaired(StructureWallObject structureWall) {
            if (structureObj.walls.Contains(structureWall)) {
                structureWall.gridTileLocation.SetTileType(LocationGridTile.Tile_Type.Wall);
                structureObj.RescanPathfindingGridOfStructure();
                CheckInteriorState();
            }
        }
        public void OnWallDamaged(StructureWallObject structureWall) {
            Assert.IsNotNull(structureObj, $"Wall of {this.ToString()} was damaged, but it has no structure object");
            if (structureObj.walls.Contains(structureWall)) {
                //create repair job
                OnStructureDamaged();
            }
        }
        public void OnTileDamaged(LocationGridTile tile) {
            OnStructureDamaged();
        }
        public void OnTileRepaired(LocationGridTile tile) {
            // ReSharper disable once Unity.NoNullPropagation
            structureObj?.ApplyGroundTileAssetForTile(tile);
        }
        public void OnTileDestroyed(LocationGridTile tile) {
            if (structureType.IsOpenSpace()) {
                return; //do not check for destruction if structure is open space (Wilderness, Work Settlement, Cemetery, etc.)
            }
            // CheckIfStructureDestroyed();
        }
        private void OnStructureDamaged() {
            if (structureType.IsOpenSpace() || structureType.IsSettlementStructure() == false) {
                return; //do not check for damage if structure is open space (Wilderness, Work Settlement, Cemetery, etc.)
            }
            if (occupiedBuildSpot.advertisedActions.Contains(INTERACTION_TYPE.REPAIR_STRUCTURE) == false) {
                occupiedBuildSpot.AddAdvertisedAction(INTERACTION_TYPE.REPAIR_STRUCTURE);
            }
            CheckInteriorState();
            if (settlementLocation == null) return;
            if (settlementLocation.HasJob(JOB_TYPE.REPAIR, occupiedBuildSpot) == false) {
                CreateRepairJob();
            }
        }
        private bool StillHasObjectsToRepair() {
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile tile = tiles[i];
                if (tile.genericTileObject.currentHP < tile.genericTileObject.maxHP) {
                    return true;
                }
                if (tile.walls.Any(wall => wall.currentHP < wall.maxHP)) {
                    return true;
                }
            }
            return false;
        }
        private void CheckInteriorState() {
            //if structure object only has 70% or less of walls intact, set it as exterior
            //else, set it as interior
            int neededWallsToBeConsideredExterior = Mathf.FloorToInt(structureObj.walls.Length * 0.7f);
            int intactWalls = structureObj.walls.Count(wall => wall.currentHP > 0);
            SetInteriorState(intactWalls > neededWallsToBeConsideredExterior);
        }
        #endregion

        #region Repair
        private void CreateRepairJob() {
            if (settlementLocation != null) {
                GoapPlanJob repairJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPAIR, INTERACTION_TYPE.REPAIR_STRUCTURE, occupiedBuildSpot, settlementLocation);
                repairJob.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRepairStructureJob);
                settlementLocation.AddToAvailableJobs(repairJob);    
            }
        }
        #endregion

        #region Resource
        public void ChangeResourceMadeOf(RESOURCE resource) {
            structureObj.ChangeResourceMadeOf(resource);
        }
        #endregion

        public override string ToString() {
            return $"{structureType} {id} at {location.name}";
        }

        #region Player Action Target
        public List<PlayerAction> actions { get; private set; }
        public virtual void ConstructDefaultActions() {
            actions = new List<PlayerAction>();
        }
        public void AddPlayerAction(PlayerAction action) {
            if (actions.Contains(action) == false) {
                actions.Add(action);
                Messenger.Broadcast(Signals.PLAYER_ACTION_ADDED_TO_TARGET, action, this as IPlayerActionTarget);    
            }
        }
        public void RemovePlayerAction(PlayerAction action) {
            if (actions.Remove(action)) {
                Messenger.Broadcast(Signals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
            }
        }
        public void RemovePlayerAction(string actionName) {
            for (int i = 0; i < actions.Count; i++) {
                PlayerAction action = actions[i];
                if (action.actionName == actionName) {
                    actions.RemoveAt(i);
                    Messenger.Broadcast(Signals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
                }
            }
        }
        public PlayerAction GetPlayerAction(string actionName) {
            for (int i = 0; i < actions.Count; i++) {
                PlayerAction playerAction = actions[i];
                if (playerAction.actionName == actionName) {
                    return playerAction;
                }
            }
            return null;
        }
        public void ClearPlayerActions() {
            actions.Clear();
        }
        #endregion
        
        #region Selectable
        public bool IsCurrentlySelected() {
            return UIManager.Instance.structureInfoUI.isShowing 
                   && UIManager.Instance.structureInfoUI.activeStructure == this;
        }
        public void LeftSelectAction() {
            UIManager.Instance.ShowStructureInfo(this);
        }
        public void RightSelectAction() {
            //Nothing happens
        }
        #endregion
    }
}

[System.Serializable]
public class SaveDataLocationStructure {
    public int id;
    public string name;
    public STRUCTURE_TYPE structureType;
    public bool isInside;
    public POI_STATE state;

    public Vector3Save entranceTile;
    public bool isFromTemplate;

    private LocationStructure loadedStructure;
    public void Save(LocationStructure structure) {
        id = structure.id;
        name = structure.name;
        structureType = structure.structureType;
        state = structure.state;
    }

    public LocationStructure Load(ILocation location) {
        LocationStructure createdStructure = null;
        switch (structureType) {
            case STRUCTURE_TYPE.DWELLING:
                createdStructure = new Dwelling(location, this);
                break;
            default:
                createdStructure = new LocationStructure(location, this);
                break;
        }
        loadedStructure = createdStructure;
        return createdStructure;
    }

    //This is loaded last so release loadedStructure
    public void LoadEntranceTile() {
        if(entranceTile.z != -1f) {
            for (int i = 0; i < loadedStructure.tiles.Count; i++) {
                LocationGridTile tile = loadedStructure.tiles[i];
                if(tile.localPlace.x == (int)entranceTile.x && tile.localPlace.y == (int) entranceTile.y) {
                    break;
                }
            }
        }
        loadedStructure = null;
    }
}

public class TileObjectsAndCount {
    public int count;
    public List<TileObject> tileObjects;
    
    public TileObjectsAndCount() {
        tileObjects = new List<TileObject>();
    }

    public void AddTileObject(TileObject tileObject) {
        tileObjects.Add(tileObject);
        count = tileObjects.Count;
    }
    public void RemoveTileObject(TileObject tileObject) {
        if (tileObjects.Remove(tileObject)) {
            count = tileObjects.Count;
        }
    }
}