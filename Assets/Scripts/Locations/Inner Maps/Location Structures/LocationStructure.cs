using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine.Assertions;
namespace Inner_Maps.Location_Structures {
    [System.Serializable]
    public abstract class LocationStructure : IPlayerActionTarget, ISelectable, IPartyTarget {
        public int id { get; private set; }
        public string name { get; protected set; }
        public string nameWithoutID { get; protected set; }
        public int maxResidentCapacity { get; protected set; }
        public virtual string nameplateName => name;
        public STRUCTURE_TYPE structureType { get; private set; }
        public List<STRUCTURE_TAG> structureTags { get; protected set; }
        public List<Character> charactersHere { get; private set; }
        public Region location { get; private set; }
        public BaseSettlement settlementLocation { get; private set; }
        public HashSet<IPointOfInterest> pointsOfInterest { get; private set; }
        public Dictionary<TILE_OBJECT_TYPE, TileObjectsAndCount> groupedTileObjects { get; private set; }
        public POI_STATE state { get; private set; }
        public LocationStructureObject structureObj {get; private set;}
        public InnerMapHexTile occupiedHexTile { get; private set; }
        //Inner Map
        public List<LocationGridTile> tiles { get; private set; }
        public List<LocationGridTile> passableTiles { get; private set; }
        public LinkedList<LocationGridTile> unoccupiedTiles { get; private set; }
        //public List<LocationGridTile> outerTiles { get; private set; }
        public bool isInterior { get; private set; }
        public bool hasBeenDestroyed { get; private set; }
        //HP
        public int maxHP { get; protected set; }
        public int currentHP { get; protected set; }
        public HashSet<IDamageable> objectsThatContributeToDamage { get; private set; }
        public List<Character> residents { get; protected set; }
        public StructureRoom[] rooms { get; protected set; }

        //protected Faction _owner;

        #region getters
        public virtual bool isDwelling => false;
        public virtual Vector3 worldPosition { get; protected set; }
        public virtual Vector2 selectableSize => structureObj.size;
        public LocationStructure currentStructure => this;
        //public Faction owner => settlementLocation != null ? settlementLocation.owner : _owner;
        #endregion

        protected LocationStructure(STRUCTURE_TYPE structureType, Region location) {
            id = UtilityScripts.Utilities.SetID(this);
            this.structureType = structureType;
            nameWithoutID = $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString())}";
            name = $"{nameWithoutID} {id.ToString()}";
            this.location = location;
            charactersHere = new List<Character>();
            pointsOfInterest = new HashSet<IPointOfInterest>();
            groupedTileObjects = new Dictionary<TILE_OBJECT_TYPE, TileObjectsAndCount>();
            tiles = new List<LocationGridTile>();
            passableTiles = new List<LocationGridTile>();
            unoccupiedTiles = new LinkedList<LocationGridTile>();
            objectsThatContributeToDamage = new HashSet<IDamageable>();
            structureTags = new List<STRUCTURE_TAG>();
            residents = new List<Character>();
            SetMaxHPAndReset(3000);
            //outerTiles = new List<LocationGridTile>();
            SetInteriorState(structureType.IsInterior());
            maxResidentCapacity = 5;
        }
        protected LocationStructure(Region location, SaveDataLocationStructure data) {
            this.location = location;
            id = UtilityScripts.Utilities.SetID(this, data.id);
            structureType = data.structureType;
            name = data.name;
            charactersHere = new List<Character>();
            pointsOfInterest = new HashSet<IPointOfInterest>();
            groupedTileObjects = new Dictionary<TILE_OBJECT_TYPE, TileObjectsAndCount>();
            structureTags = new List<STRUCTURE_TAG>();
            //outerTiles = new List<LocationGridTile>();
            tiles = new List<LocationGridTile>();
            passableTiles = new List<LocationGridTile>();
            objectsThatContributeToDamage = new HashSet<IDamageable>();
            residents = new List<Character>();
            SetMaxHPAndReset(3000);
            SetInteriorState(structureType.IsInterior());
            maxResidentCapacity = 5;
        }

        #region Virtuals
        public virtual void OnBuiltStructure() { }
        protected virtual void OnAddResident(Character newResident) { }
        protected virtual void OnRemoveResident(Character newResident) { }
        public virtual bool CanBeResidentHere(Character character) { return true; }
        #endregion

        #region Initialization
        public virtual void Initialize() {
            SubscribeListeners();
            ConstructDefaultActions();
        }
        #endregion

        #region Listeners
        protected virtual void SubscribeListeners() { }
        protected virtual void UnsubscribeListeners() { }
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
                case STRUCTURE_TYPE.POND:
                    return location.name;
                case STRUCTURE_TYPE.CITY_CENTER:
                    return $"the {location.name} city center";
                default:
                    string normalizedStructure =
                        UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString());
                    if (normalizedStructure.Contains("The")) {
                        return normalizedStructure;
                    } else {
                        return
                            $"the {normalizedStructure}";    
                    }
            }
        }
        //public void SetOuterTiles() {
        //    for (int i = 0; i < tiles.Count; i++) {
        //        LocationGridTile currTile = tiles[i];
        //        if (currTile.HasDifferentDwellingOrOutsideNeighbour()) {
        //            outerTiles.Add(currTile);
        //        }
        //    }
        //}
        //Note: Retained this because I don't know how to set the outer tiles on world creation. I only have the SetOuterTiles whenever a new structure is built. I also don't know when to set the outer tiles of wilderness. - Chy
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
        public void SetSettlementLocation(BaseSettlement npcSettlement) {
            settlementLocation = npcSettlement;
        }
        public void SetInteriorState(bool _isInterior) {
            isInterior = _isInterior;
        }
        public void CenterOnStructure() {
            if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != location.innerMap) {
                InnerMapManager.Instance.HideAreaMap();
            }
            if (location.innerMap.isShowing == false) {
                InnerMapManager.Instance.ShowInnerMap(location);
            }
            if (structureObj != null) {
                InnerMapCameraMove.Instance.CenterCameraOn(structureObj.gameObject);
            }
        }
        public void AddStructureTag(STRUCTURE_TAG tag) {
            structureTags.Add(tag);
        }
        public bool RemoveStructureTag(STRUCTURE_TAG tag) {
            return structureTags.Remove(tag);
        }
        public bool HasStructureTag(params STRUCTURE_TAG[] tag) {
            for (int i = 0; i < tag.Length; i++) {
                if (structureTags.Contains(tag[i])) {
                    return true;
                }
            }
            return false;
        }
        public bool HasStructureTags() {
            return structureTags.Count > 0;
        }
        public override string ToString() {
            return $"{structureType.ToString()} {id.ToString()} at {location.name}";
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
        public int GetNumberOfSummonsHere() {
            int count = 0;
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (character.gridTileLocation != null && character is Summon) {
                    count++;
                }
            }
            return count;
        }
        public int GetNumberOfNonPreplacedTileObjectsHere() {
            int count = 0;
            foreach (TileObjectsAndCount tileObjAndCount in groupedTileObjects.Values) {
                for (int i = 0; i < tileObjAndCount.tileObjects.Count; i++) {
                    TileObject obj = tileObjAndCount.tileObjects[i];
                    if (obj.tileObjectType != TILE_OBJECT_TYPE.BLOCK_WALL && obj.preplacedLocationStructure != this) {
                        count++;
                    }
                }
            }
            return count;
        }
        public List<Character> GetCharactersThatMeetCriteria(System.Func<Character, bool> criteria) {
            List<Character> characters = new List<Character>();
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (criteria.Invoke(character)) {
                    characters.Add(character);
                }
            }
            return characters;
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
                    if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                    && tileObject.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile is NPCSettlement npcSettlement) {
                        npcSettlement.OnItemAddedToLocation(tileObject, this);
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
                    
                    if (poi.gridTileLocation.collectionOwner.isPartOfParentRegionMap 
                        && poi.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile is NPCSettlement npcSettlement) {
                        npcSettlement.OnItemRemovedFromLocation(tileObject, this);    
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
                    if (poi.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                    && poi.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.settlementOnTile is NPCSettlement npcSettlement) {
                        npcSettlement.OnItemRemovedFromLocation(tileObject, this);    
                    }
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
        public List<T> GetTileObjectsOfType<T>(System.Func<T, bool> validityChecker = null) where T : TileObject {
            List<T> objs = new List<T>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi is T obj) {
                    if (validityChecker != null) {
                        if (validityChecker.Invoke(obj)) {
                            objs.Add(obj);    
                        }  
                    } else {
                        objs.Add(obj);    
                    }
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
        public T GetTileObjectOfType<T>() where T : TileObject{
            List<TileObject> objs = new List<TileObject>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi is T obj) {
                    return obj;
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
        public T GetResourcePileObjectWithLowestCount<T>(bool excludeMaximum = true) where T : ResourcePile {
            T chosenPile = null;
            int lowestCount = 0;
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi is T obj) {
                    if (excludeMaximum && obj.IsAtMaxResource(obj.providedResource)) {
                        continue; //skip
                    }
                    if(chosenPile == null || obj.resourceInPile <= lowestCount) {
                        chosenPile = obj;
                        lowestCount = obj.resourceInPile;
                    }
                }
            }
            return chosenPile;
        }
        public ResourcePile GetResourcePileObjectWithLowestCount(TILE_OBJECT_TYPE tileObjectType, bool excludeMaximum = true){
            ResourcePile chosenPile = null;
            int lowestCount = 0;
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i); 
                if (poi is ResourcePile obj && obj.tileObjectType == tileObjectType) {
                    if (excludeMaximum && obj.IsAtMaxResource(obj.providedResource)) {
                        continue; //skip
                    }
                    if(chosenPile == null || obj.resourceInPile <= lowestCount) {
                        chosenPile = obj;
                        lowestCount = obj.resourceInPile;
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
                if (location.locationType == LOCATION_TYPE.DEMONIC_INTRUSION) { //player npcSettlement
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
                                                          && x.collectionOwner.partOfHextile.hexTileOwner 
                                                          && x.collectionOwner.partOfHextile.hexTileOwner.elevationType == ELEVATION.PLAIN
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
                    } else if (poi is TreasureChest || poi is ElementalCrystal) {
                        return unoccupiedTiles.Where(x => x.IsPartOfSettlement() == false).ToList();
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
        protected virtual void OnTileAddedToStructure(LocationGridTile tile) { }
        protected virtual void OnTileRemovedFromStructure(LocationGridTile tile) { }
        public void AddTile(LocationGridTile tile) {
            if (!tiles.Contains(tile)) {
                tiles.Add(tile);
                if(tile.tileState == LocationGridTile.Tile_State.Empty) {
                    AddUnoccupiedTile(tile);
                } else {
                    RemoveUnoccupiedTile(tile);
                }
                if (tile.IsPassable()) {
                    AddPassableTile(tile);
                } else {
                    RemovePassableTile(tile);
                }
                if (structureType != STRUCTURE_TYPE.WILDERNESS && tile.IsPartOfSettlement(out var settlement)) {
                    SetSettlementLocation(settlement);
                }
                OnTileAddedToStructure(tile);
            }
        }
        public void RemoveTile(LocationGridTile tile) {
            if (tiles.Remove(tile)) {
                OnTileRemovedFromStructure(tile);
            }
            RemoveUnoccupiedTile(tile);
        }
        public void AddPassableTile(LocationGridTile tile) {
            passableTiles.Add(tile);
        }
        public void RemovePassableTile(LocationGridTile tile) {
            passableTiles.Remove(tile);
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
        public LocationGridTile GetRandomPassableTile() {
            if (passableTiles.Count <= 0) {
                return null;
            }
            return passableTiles[Random.Range(0, passableTiles.Count)];
        }
        public LocationGridTile GetRandomUnoccupiedTile() {
            if (unoccupiedTiles.Count <= 0) {
                return null;
            }
            return unoccupiedTiles.ElementAt(Random.Range(0, unoccupiedTiles.Count));
        }
        public virtual void OnTileDamaged(LocationGridTile tile, int amount) { }
        public virtual void OnTileRepaired(LocationGridTile tile, int amount) {
            if (hasBeenDestroyed) { return; }
            if (tile.genericTileObject.currentHP >= tile.genericTileObject.maxHP) {
                // ReSharper disable once Unity.NoNullPropagation
                structureObj?.ApplyGroundTileAssetForTile(tile);    
                tile.CreateSeamlessEdgesForSelfAndNeighbours();
            }
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
        public IDamageable GetNearestDamageableThatContributeToHP(LocationGridTile fromTile) {
            IDamageable nearest = null;
            float nearestDist = 9999f;
            for (int i = 0; i < objectsThatContributeToDamage.Count; i++) {
                IDamageable poi = objectsThatContributeToDamage.ElementAt(i);
                if (poi.gridTileLocation != null) {
                    float dist = fromTile.GetDistanceTo(poi.gridTileLocation);
                    if (nearest == null || dist < nearestDist) {
                        nearest = poi;
                        nearestDist = dist;
                    }
                }
            }
            return nearest;
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
        public void SetOccupiedHexTile(InnerMapHexTile hexTile) {
            InnerMapHexTile previousOccupiedHexTile = occupiedHexTile;
            occupiedHexTile = hexTile;
            if (previousOccupiedHexTile != null) {
                previousOccupiedHexTile.CheckIfVacated();
            }
        }
        private void OnClickStructure() {
            Selector.Instance.Select(this);
        }
        #endregion

        #region Destroy
        protected virtual void DestroyStructure() {
            if (hasBeenDestroyed) {
                return;
            }
            hasBeenDestroyed = true;
            Debug.Log($"{GameManager.Instance.TodayLogString()}{ToString()} was destroyed!");

            //transfer tiles to either the wilderness or work npcSettlement
            List<LocationGridTile> tilesInStructure = new List<LocationGridTile>(tiles);
            LocationStructure wilderness = location.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            for (int i = 0; i < tilesInStructure.Count; i++) {
                LocationGridTile tile = tilesInStructure[i];
                LocationStructure transferTo = wilderness;
            
                tile.ClearWallObjects();
                IPointOfInterest obj = tile.objHere;
                if (obj != null) {
                    // obj.AdjustHP(-tile.objHere.maxHP, ELEMENTAL_TYPE.Normal, showHPBar: true);
                    obj.gridTileLocation?.structure.RemovePOI(obj); //because sometimes adjusting the hp of the object to 0 does not remove it?
                }
                
                tile.SetStructure(transferTo);
                tile.SetTileType(LocationGridTile.Tile_Type.Empty);
                tile.RevertToPreviousGroundVisual();
                tile.SetPreviousGroundVisual(null); //so that the tile will never revert to the structure tile, unless a new structure is put on it.
                tile.genericTileObject.AdjustHP(tile.genericTileObject.maxHP, ELEMENTAL_TYPE.Normal);
            }
        
            //disable game object. Destruction of structure game object is handled by it's parent structure template.
            structureObj.OnOwnerStructureDestroyed(); 
            location.RemoveStructure(this);
            settlementLocation.RemoveStructure(this);
            Messenger.Broadcast(Signals.STRUCTURE_OBJECT_REMOVED, this, occupiedHexTile);
            SetOccupiedHexTile(null);
            UnsubscribeListeners();
            Messenger.Broadcast(Signals.STRUCTURE_DESTROYED, this);
        }
        #endregion

        #region Resource
        public void ChangeResourceMadeOf(RESOURCE resource) {
            structureObj.ChangeResourceMadeOf(resource);
        }
        #endregion

        #region Player Action Target
        public List<SPELL_TYPE> actions { get; private set; }

        public virtual void ConstructDefaultActions() {
            actions = new List<SPELL_TYPE>();
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
        
        #region Selectable
        public bool IsCurrentlySelected() {
            return UIManager.Instance.structureInfoUI.isShowing 
                   && UIManager.Instance.structureInfoUI.activeStructure == this;
        }
        public void LeftSelectAction() {
            UIManager.Instance.ShowStructureInfo(this);
        }
        public void RightSelectAction() { }
        public bool CanBeSelected() {
            return true;
        }
        #endregion

        #region HP
        public void AddObjectAsDamageContributor(IDamageable damageable) {
            objectsThatContributeToDamage.Add(damageable);
        }
        protected void OnObjectDamaged(IPointOfInterest poi, int amount) {
            if (objectsThatContributeToDamage.Contains(poi)) {
                AdjustHP(amount);
            }
        }
        protected void OnObjectRepaired(IPointOfInterest poi, int amount) {
            if (objectsThatContributeToDamage.Contains(poi)) {
                AdjustHP(amount);
            }
        }
        protected virtual void AdjustHP(int amount) {
            if (hasBeenDestroyed) { return; }
            currentHP += amount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            if (currentHP == 0) {
                DestroyStructure();
            }
        }
        public void SetMaxHP(int amount) {
            maxHP = amount;
        }
        public void SetMaxHPAndReset(int amount) {
            SetMaxHP(amount);
            currentHP = maxHP;
        }
        #endregion

        #region Residents
        public bool AddResident(Character character) {
            if (!residents.Contains(character)) { //residents.Count < maxResidentCapacity && 
                residents.Add(character);
                character.SetHomeStructure(this);
                OnAddResident(character);
                //if(settlementLocation == null) {
                //    //Only set/unset faction owner for structures that do not have a settlement, if a structure has a settlement, the settlement should be the one being owned by the faction not the specific structure
                //    if (owner == null && character.faction != null && character.faction.isMajorNonPlayer) {
                //        //If a character becomes a resident and he/she has a faction and this structure has no faction owner yet, set it as the faction owner
                //        LandmarkManager.Instance.OwnStructure(character.faction, this);
                //    }
                //}
                Messenger.Broadcast(Signals.ADDED_STRUCTURE_RESIDENT, character, this);
                return true;
            }
            return false;
        }
        public void RemoveResident(Character character) {
            if (residents.Remove(character)) {
                character.SetHomeStructure(null);
                OnRemoveResident(character);
                //if (settlementLocation == null) {
                //    //Only set/unset faction owner for structures that do not have a settlement, if a structure has a settlement, the settlement should be the one being owned by the faction not the specific structure
                //    if (residents.Count <= 0 && owner != null) {
                //        //if all residents of a settlement is removed, then remove faction owner
                //        LandmarkManager.Instance.UnownStructure(this);
                //    }
                //}
                Messenger.Broadcast(Signals.REMOVED_STRUCTURE_RESIDENT, character, this);
            }
        }
        public bool IsResident(Character character) {
            return character.homeStructure == this; //residents.Contains(character);
        }
        public bool HasPositiveRelationshipWithAnyResident(Character character) {
            if (residents.Contains(character)) {
                return true; //if the provided character is a resident of this dwelling, then yes, consider relationship as positive
            }
            for (int i = 0; i < residents.Count; i++) {
                Character currResident = residents[i];
                RELATIONSHIP_EFFECT effect = character.relationshipContainer.GetRelationshipEffectWith(currResident);
                if (effect == RELATIONSHIP_EFFECT.POSITIVE) {
                    return true;
                }
            }
            return false;
        }
        public bool HasEnemyOrNoRelationshipWithAnyResident(Character character) {
            for (int i = 0; i < residents.Count; i++) {
                Character currResident = residents[i];
                RELATIONSHIP_EFFECT effect = character.relationshipContainer.GetRelationshipEffectWith(currResident);
                if (effect == RELATIONSHIP_EFFECT.NEGATIVE || effect == RELATIONSHIP_EFFECT.NONE) {
                    return true;
                }
            }
            return false;
        }
        public bool IsOccupied() {
            return residents.Count > 0;
        }
        public int GetNumberOfResidentsExcluding(out List<Character> validResidents, params Character[] excludedCharacters) {
            validResidents = null;
            if (residents != null) {
                validResidents = new List<Character>();
                int residentCount = 0;
                for (int i = 0; i < residents.Count; i++) {
                    Character resident = residents[i];
                    if (excludedCharacters.Contains(resident) == false) {
                        residentCount++;
                        validResidents.Add(resident);
                    }
                }
                return residentCount;
            }
            return 0;
        }
        public bool HasCloseFriendOrNonEnemyRivalRelative(Character character) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if(character != resident) {
                    if (character.relationshipContainer.IsEnemiesWith(resident)) {
                        return false;
                    } else {
                        if(character.relationshipContainer.HasRelationshipWith(resident, RELATIONSHIP_TYPE.RELATIVE, RELATIONSHIP_TYPE.SIBLING, RELATIONSHIP_TYPE.CHILD, RELATIONSHIP_TYPE.PARENT)) {
                            return true;
                        }
                        if(character.relationshipContainer.GetOpinionLabel(resident) == RelationshipManager.Close_Friend) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        #region Facilities
        public bool HasUnoccupiedFurnitureSpot() {
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                if (currTile.objHere == null && currTile.hasFurnitureSpot) {
                    return true;
                }
            }
            return false;
        }
        //public bool HasFacilityDeficit() {
        //    foreach (KeyValuePair<FACILITY_TYPE, int> kvp in facilities) {
        //        if (kvp.Value <= 0) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        //public FACILITY_TYPE GetMostNeededValidFacility() {
        //    //get the facility with the lowest value, that can be provided given the unoccupied furnitureSpots
        //    int lowestValue = 99999;
        //    FACILITY_TYPE lowestFacility = FACILITY_TYPE.NONE;
        //    foreach (KeyValuePair<FACILITY_TYPE, int> keyValuePair in facilities) {
        //        if (keyValuePair.Value < lowestValue && HasUnoccupiedFurnitureSpotsThatCanProvide(keyValuePair.Key)) {
        //            lowestValue = keyValuePair.Value;
        //            lowestFacility = keyValuePair.Key;
        //        }
        //    }
        //    return lowestFacility;
        //}
        //public List<LocationGridTile> GetUnoccupiedFurnitureSpotsThatCanProvide(FACILITY_TYPE type) {
        //    List<LocationGridTile> validTiles = new List<LocationGridTile>();
        //    for (int i = 0; i < tiles.Count; i++) {
        //        LocationGridTile currTile = tiles[i];
        //        if (currTile.objHere == null && currTile.hasFurnitureSpot && currTile.furnitureSpot.allowedFurnitureTypes != null) {
        //            for (int j = 0; j < currTile.furnitureSpot.allowedFurnitureTypes.Length; j++) {
        //                FURNITURE_TYPE furnitureType = currTile.furnitureSpot.allowedFurnitureTypes[j];
        //                TILE_OBJECT_TYPE tileObject = furnitureType.ConvertFurnitureToTileObject();
        //                if (tileObject.CanProvideFacility(type)) {
        //                    validTiles.Add(currTile);
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //    return validTiles;
        //}
        #endregion

        #region Rooms
        public void CreateRoomsBasedOnStructureObject(LocationStructureObject structureObject) {
            if (structureObject.roomTemplates == null || structureObject.roomTemplates.Length == 0) { return; }
            rooms = new StructureRoom[structureObject.roomTemplates.Length];
            for (int i = 0; i < rooms.Length; i++) {
                RoomTemplate roomTemplate = structureObject.roomTemplates[i];
                StructureRoom newRoom =
                    CreteNewRoomForStructure(structureObject.GetTilesOccupiedByRoom(location.innerMap, roomTemplate));
                rooms[i] = newRoom;
            }
        }
        protected virtual StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom) { return null; }
        public bool IsTilePartOfARoom(LocationGridTile tile, out StructureRoom room) {
            if (rooms == null) {
                room = null;
                return false;
            }
            for (int i = 0; i < rooms.Length; i++) {
                StructureRoom currentRoom = rooms[i];
                if (currentRoom.tilesInRoom.Contains(tile)) {
                    room = currentRoom;
                    return true;
                }
            }
            room = null;
            return false;
        }
        #endregion

        //#region Faction
        //public void SetOwner(Faction owner) {
        //    _owner = owner;
        //}
        //#endregion

        public virtual void OnCharacterUnSeizedHere(Character character) { }
        
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

    public LocationStructure Load(Region location) {
        LocationStructure createdStructure = LandmarkManager.Instance.CreateNewStructureAt(location, structureType);
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