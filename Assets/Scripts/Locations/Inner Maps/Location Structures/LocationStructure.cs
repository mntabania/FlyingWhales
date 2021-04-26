using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Locations.Settlements;
using UnityEngine.Assertions;
using Logs;
using UtilityScripts;
using Locations;
namespace Inner_Maps.Location_Structures {

    [System.Serializable]
    public abstract class LocationStructure : IPlayerActionTarget, ISelectable, IPartyQuestTarget, IPartyTargetDestination, IGatheringTarget, ILogFiller, ILocation, IStoredTarget {
        public string persistentID { get; }
        public int id { get; private set; }
        public string name { get; protected set; }
        public string nameWithoutID { get; protected set; }
        public int maxResidentCapacity { get; protected set; }
        public STRUCTURE_TYPE structureType { get; private set; }
        public List<STRUCTURE_TAG> structureTags { get; protected set; }
        public List<Character> charactersHere { get; private set; }
        public Region region { get; private set; }
        public BaseSettlement settlementLocation { get; private set; }
        public HashSet<IPointOfInterest> pointsOfInterest { get; private set; }
        public Dictionary<TILE_OBJECT_TYPE, List<TileObject>> groupedTileObjects { get; private set; }
        public Area occupiedArea { get; private set; }
        //Inner Map
        public HashSet<LocationGridTile> tiles { get; private set; }
        public List<LocationGridTile> passableTiles { get; private set; }
        public List<LocationGridTile> unoccupiedTiles { get; private set; }
        public bool isInterior { get; private set; }
        public bool hasBeenDestroyed { get; private set; }
        public bool isStoredAsTarget { get; private set; }

        //HP
        public int maxHP { get; protected set; }
        public int currentHP { get; protected set; }
        public HashSet<IDamageable> objectsThatContributeToDamage { get; private set; }
        public List<Character> residents { get; protected set; }
        public StructureRoom[] rooms { get; protected set; }
        public bool hasActiveSocialGathering { get; protected set; }
        public LocationAwareness locationAwareness { get; protected set; }
        public LocationStructureEventDispatcher eventDispatcher { get; }

        //protected Faction _owner;
        
        /// <summary>
        /// List of areas that this structure has a tile on.
        /// NOTE: This can have duplicates of the same Area, this is so that there is
        /// no need to count the number of tiles that occupy a area, when trying to remove that area from the list
        /// so it is safe to assume that number of tiles = length of this list.
        /// NOTE: This is not filled out in wilderness structure! Because it is not needed.
        /// NOTE: This isn't saved because this is filled out anytime a tile is added to this structure, and since those tiles are saved, there is no need to save this.
        /// </summary>
        public List<Area> occupiedAreas { get; private set; }
        public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

        #region getters
        public string bookmarkName => $"{iconRichText} {name}";
        public BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text_With_Cancel;
        public virtual string nameplateName => name;
        public string locationName => ToString();
        public virtual bool isDwelling => false;
        public virtual Vector3 worldPosition { get; protected set; }
        public virtual Vector2 selectableSize => Vector2.zero;
        public virtual Type serializedData => typeof(SaveDataLocationStructure);
        public LocationStructure currentStructure => this;
        public BaseSettlement currentSettlement => settlementLocation;
        //public Faction owner => settlementLocation != null ? settlementLocation.owner : _owner;
        public OBJECT_TYPE objectType => OBJECT_TYPE.Structure;
        public STORED_TARGET_TYPE storedTargetType => STORED_TARGET_TYPE.Structures;

        public bool isTargetted { set; get; }

        public string iconRichText => UtilityScripts.Utilities.StructureIcon();
        public PARTY_TARGET_DESTINATION_TYPE partyTargetDestinationType => PARTY_TARGET_DESTINATION_TYPE.Structure;
        #endregion

        protected LocationStructure(STRUCTURE_TYPE structureType, Region location) {
            persistentID = Guid.NewGuid().ToString();
            id = UtilityScripts.Utilities.SetID(this);
            this.structureType = structureType;
            nameWithoutID = structureType.StructureName();
            name = $"{nameWithoutID} {id.ToString()}";
            this.region = location;
            charactersHere = new List<Character>();
            pointsOfInterest = new HashSet<IPointOfInterest>();
            groupedTileObjects = new Dictionary<TILE_OBJECT_TYPE, List<TileObject>>();
            tiles = new HashSet<LocationGridTile>();
            passableTiles = new List<LocationGridTile>();
            unoccupiedTiles = new List<LocationGridTile>();
            objectsThatContributeToDamage = new HashSet<IDamageable>();
            structureTags = new List<STRUCTURE_TAG>();
            residents = new List<Character>();
            occupiedAreas = new List<Area>();
            SetMaxHPAndReset(3000);
            //outerTiles = new List<LocationGridTile>();
            SetInteriorState(structureType.IsInterior());
            maxResidentCapacity = 5;

            locationAwareness = new LocationAwareness();
            eventDispatcher = new LocationStructureEventDispatcher();
            bookmarkEventDispatcher = new BookmarkableEventDispatcher();
        }
        protected LocationStructure(Region location, SaveDataLocationStructure data) {
            charactersHere = new List<Character>();
            pointsOfInterest = new HashSet<IPointOfInterest>();
            groupedTileObjects = new Dictionary<TILE_OBJECT_TYPE, List<TileObject>>();
            structureTags = new List<STRUCTURE_TAG>(data.structureTags);
            tiles = new HashSet<LocationGridTile>();
            passableTiles = new List<LocationGridTile>();
            unoccupiedTiles = new List<LocationGridTile>();
            objectsThatContributeToDamage = new HashSet<IDamageable>();
            residents = new List<Character>();
            occupiedAreas = new List<Area>();

            persistentID = data.persistentID;
            this.region = location;
            id = UtilityScripts.Utilities.SetID(this, data.id);
            structureType = data.structureType;
            name = data.name;
            nameWithoutID = data.nameWithoutID;
            isStoredAsTarget = data.isStoredAsTarget;
            maxHP = data.maxHP;
            currentHP = data.currentHP;
            SetInteriorState(data.isInterior);
            maxResidentCapacity = 5;
            hasBeenDestroyed = data.hasBeenDestroyed;
            
            locationAwareness = new LocationAwareness();
            eventDispatcher = new LocationStructureEventDispatcher();
            bookmarkEventDispatcher = new BookmarkableEventDispatcher();
        }

        #region Loading
        public virtual void LoadReferences(SaveDataLocationStructure saveDataLocationStructure) {
            residents = SaveUtilities.ConvertIDListToCharacters(saveDataLocationStructure.residentIDs);
            charactersHere = SaveUtilities.ConvertIDListToCharacters(saveDataLocationStructure.charactersHereIDs);

            if (saveDataLocationStructure.structureRoomSaveData != null && rooms != null) {
                for (int i = 0; i < rooms.Length; i++) {
                    StructureRoom structureRoom = rooms[i];
                    SaveDataStructureRoom saveDataStructureRoom = saveDataLocationStructure.structureRoomSaveData[i];
                    structureRoom.LoadReferences(saveDataStructureRoom);
                }    
            }
            if (saveDataLocationStructure.tileObjectDamageContributors != null) {
                for (int i = 0; i < saveDataLocationStructure.tileObjectDamageContributors.Count; i++) {
                    string damageContributorID = saveDataLocationStructure.tileObjectDamageContributors[i];
                    objectsThatContributeToDamage.Add(DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(damageContributorID));
                }
            }
        }
        public virtual void LoadAdditionalReferences(SaveDataLocationStructure saveDataLocationStructure) {
            if (saveDataLocationStructure.structureRoomSaveData != null && rooms != null) {
                for (int i = 0; i < rooms.Length; i++) {
                    StructureRoom structureRoom = rooms[i];
                    SaveDataStructureRoom saveDataStructureRoom = saveDataLocationStructure.structureRoomSaveData[i];
                    structureRoom.LoadAdditionalReferences(saveDataStructureRoom);
                }    
            }
        }
        #endregion
        
        #region Virtuals
        /// <summary>
        /// Called when this structure is newly built.
        /// This function assumes that the structure that was built is in perfect condition.
        /// </summary>
        public virtual void OnBuiltNewStructure() { }
        /// <summary>
        /// Called when this structure has been fully loaded. (Tiles, StructureObject, Walls, etc.)
        /// NOTE: This is called instead of <see cref="OnBuiltNewStructure"/> when loading from save data.
        /// </summary>
        public virtual void OnDoneLoadStructure() { }
        protected virtual void OnAddResident(Character newResident) { }
        protected virtual void OnRemoveResident(Character newResident) {
            newResident.UnownOrTransferOwnershipOfItemsIn(this);
        }
        public virtual bool CanBeResidentHere(Character character) { return true; }
        #endregion

        #region Initialization
        public virtual void Initialize() {
            Debug.Assert(!hasBeenDestroyed, $"Destroyed structure {this} is being initialized!");
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
                case STRUCTURE_TYPE.TAVERN:
                    return "the tavern";
                case STRUCTURE_TYPE.WAREHOUSE:
                    return $"the {settlementLocation.name} warehouse";
                case STRUCTURE_TYPE.PRISON:
                    return $"the {settlementLocation.name} prison";
                case STRUCTURE_TYPE.WILDERNESS:
                    return $"the outskirts of {region.name}";
                case STRUCTURE_TYPE.CEMETERY:
                    return $"the cemetery of {settlementLocation.name}";
                case STRUCTURE_TYPE.POND:
                    return region.name;
                case STRUCTURE_TYPE.CITY_CENTER:
                    return $"{settlementLocation.name}";
                default:
                    // string normalizedStructure =
                    //     UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString());
                    if (nameWithoutID.Contains("The")) {
                        return nameWithoutID;
                    } else {
                        return
                            $"the {nameWithoutID}";    
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
        public void PopulateOuterTiles(List<LocationGridTile> outerTiles) {
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles.ElementAt(i);
                if (currTile.HasDifferentDwellingOrOutsideNeighbour()) {
                    outerTiles.Add(currTile);
                }
            }
        }
        public void DoCleanup() {
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i);
                if (poi is TileObject tileObject) {
                    tileObject.DoCleanup();
                }
            }
        }
        public void SetSettlementLocation(BaseSettlement npcSettlement) {
            settlementLocation = npcSettlement;
        }
        public void SetInteriorState(bool _isInterior) {
            isInterior = _isInterior;
        }
        public abstract void CenterOnStructure();
        public abstract void ShowSelectorOnStructure();
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
            return $"{structureType.ToString()} {id.ToString()} at {region.name}";
        }
        public void SetHasActiveSocialGathering(bool state) {
            hasActiveSocialGathering = state;
        }
        public virtual bool HasTileOnArea(Area p_area) {
            return (occupiedArea != null && occupiedArea == p_area) || occupiedAreas.Contains(p_area);
        }
        #endregion

        #region Characters
        public virtual void AddCharacterAtLocation(Character character, LocationGridTile tile = null) {
            bool wasAdded = false;
            if (!charactersHere.Contains(character)) {
                wasAdded = true;
                charactersHere.Add(character);
                //location.AddCharacterToLocation(character);
                AddPOI(character, tile);
            }
            character.SetCurrentStructureLocation(this);
            if (wasAdded) {
                AfterCharacterAddedToLocation(character);
            }
        }
        public void RemoveCharacterAtLocation(Character character) {
            if (charactersHere.Remove(character)) {
                character.SetCurrentStructureLocation(null);
                RemovePOI(character);
                AfterCharacterRemovedFromLocation(character);
            }
        }
        protected virtual void AfterCharacterAddedToLocation(Character p_character) {}
        protected virtual void AfterCharacterRemovedFromLocation(Character p_character) {}
        public int GetNumberOfSummonsHere() {
            int count = 0;
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (character.gridTileLocation != null && character is Summon && !character.isDead) {
                    count++;
                }
            }
            return count;
        }
        public void PopulateCharacterListThatMeetCriteria(List<Character> characterList, System.Func<Character, bool> criteria) {
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (criteria.Invoke(character)) {
                    characterList.Add(character);
                }
            }
        }
        public Character GetRandomCharacterThatMeetCriteria(System.Func<Character, bool> criteria) {
            List<Character> characters = ObjectPoolManager.Instance.CreateNewCharactersList();
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (criteria.Invoke(character)) {
                    characters.Add(character);
                }
            }
            Character chosen = null;
            if(characters != null && characters.Count > 0) {
                chosen = characters[UnityEngine.Random.Range(0, characters.Count)];
            }
            ObjectPoolManager.Instance.ReturnCharactersListToPool(characters);
            return chosen;
        }
        public int GetNumOfCharactersThatMeetCriteria(System.Func<Character, bool> criteria) {
            int count = 0;
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                if (criteria.Invoke(character)) {
                    count++;
                }
            }
            return count;
        }
        #endregion

        #region Tile Objects
        protected void ProcessAllTileObjects(Action<TileObject> action) {
            //List<TileObject> objs = new List<TileObject>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest currPOI = pointsOfInterest.ElementAt(i);
                if (currPOI is TileObject poi) {
                    action.Invoke(poi);
                    //objs.Add(poi);
                }
            }
            //return objs;
        }
        public void PopulateTileObjectsThatAdvertise(List<TileObject> p_objectList, params INTERACTION_TYPE[] types) {
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest currPOI = pointsOfInterest.ElementAt(i);
                if (currPOI is TileObject obj) {
                    if (obj.IsAvailable() && obj.AdvertisesAll(types)) {
                        p_objectList.Add(obj);
                    }
                }
            }
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles.ElementAt(i);
                if (currTile.tileObjectComponent.genericTileObject.IsAvailable() && currTile.tileObjectComponent.genericTileObject.AdvertisesAll(types)) {
                    p_objectList.Add(currTile.tileObjectComponent.genericTileObject);
                }
            }
        }
        public TileObject GetUnoccupiedTileObject(params TILE_OBJECT_TYPE[] type) {
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i);
                if (poi.IsAvailable() && poi is TileObject tileObj) {
                    if (type.Contains(tileObj.tileObjectType) && tileObj.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                        return tileObj;
                    }
                }
            }
            return null;
        }
        public TileObject GetUnoccupiedTileObject(TILE_OBJECT_TYPE type) {
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i);
                if (poi.IsAvailable() && poi is TileObject tileObj) {
                    if (type == tileObj.tileObjectType && tileObj.mapObjectState == MAP_OBJECT_STATE.BUILT) {
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
        public List<TileObject> GetTileObjectsOfType(TILE_OBJECT_TYPE type) {
            if (groupedTileObjects.ContainsKey(type)) {
                return groupedTileObjects[type];
            }
            return null;
            //List<TileObject> objs = new List<TileObject>();
            //for (int i = 0; i < pointsOfInterest.Count; i++) {
            //    IPointOfInterest poi = pointsOfInterest.ElementAt(i);
            //    if (poi is TileObject obj) {
            //        if (obj.tileObjectType == type) {
            //            objs.Add(obj);
            //        }
            //    }
            //}
            //return objs;
        }
        public bool HasTileObjectOfType(TILE_OBJECT_TYPE type) {
            if (groupedTileObjects.ContainsKey(type)) {
                return groupedTileObjects[type].Count > 0;
            }
            return false;
            //for (int i = 0; i < pointsOfInterest.Count; i++) {
            //    IPointOfInterest poi = pointsOfInterest.ElementAt(i);
            //    if (poi is TileObject obj) {
            //        if (obj.tileObjectType == type) {
            //            return true;
            //        }
            //    }
            //}
            //return false;
        }
        public bool HasBuiltTileObjectOfType(TILE_OBJECT_TYPE type) {
            if (groupedTileObjects.ContainsKey(type)) {
                return groupedTileObjects[type].Any(t => t.mapObjectState == MAP_OBJECT_STATE.BUILT);
            }
            return false;
        }
        public bool HasTileObjectThatMeetCriteria(Func<TileObject, bool> criteria) {
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i);
                if (poi is TileObject obj) {
                    if (criteria.Invoke(obj)) {
                        return true;
                    }
                }
            }
            return false;
        }
        //public List<T> GetTileObjectsOfType<T>(TILE_OBJECT_TYPE type) where T : TileObject {
        //    List<T> objs = new List<T>();
        //    for (int i = 0; i < pointsOfInterest.Count; i++) {
        //        IPointOfInterest poi = pointsOfInterest.ElementAt(i);
        //        if (poi is TileObject) {
        //            TileObject obj = poi as TileObject;
        //            if (obj.tileObjectType == type) {
        //                objs.Add(obj as T);
        //            }
        //        }
        //    }
        //    return objs;
        //}
        //public List<T> GetBuiltTileObjectsOfType<T>(TILE_OBJECT_TYPE type) where T : TileObject {
        //    List<T> objs = new List<T>();
        //    for (int i = 0; i < pointsOfInterest.Count; i++) {
        //        IPointOfInterest poi = pointsOfInterest.ElementAt(i);
        //        if (poi is TileObject) {
        //            TileObject obj = poi as TileObject;
        //            if (obj.tileObjectType == type && obj.mapObjectState == MAP_OBJECT_STATE.BUILT) {
        //                objs.Add(obj as T);
        //            }
        //        }
        //    }
        //    return objs;
        //}
        public void PopulateTileObjectsList(List<TileObject> tileObjects, TILE_OBJECT_TYPE type, System.Func<TileObject, bool> validityChecker) {
            if (groupedTileObjects.ContainsKey(type)) {
                List<TileObject> objs = groupedTileObjects[type];
                if(objs != null) {
                    for (int i = 0; i < objs.Count; i++) {
                        TileObject t = objs[i];
                        if (validityChecker == null || validityChecker.Invoke(t)) {
                            tileObjects.Add(t);
                        }
                    }
                }
            }
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
        public T GetRandomTileObjectOfTypeThatMeetCriteria<T>(System.Func<T, bool> validityChecker) where T : TileObject {
            List<T> objs = null;
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i);
                if (poi is T obj) {
                    if (validityChecker.Invoke(obj)) {
                        if (objs == null) { objs = new List<T>(); }
                        objs.Add(obj);
                    }
                }
            }
            if (objs != null && objs.Count > 0) {
                return objs[UnityEngine.Random.Range(0, objs.Count)];
            }
            return null;
        }
        public T GetFirstTileObjectOfTypeThatMeetCriteria<T>(System.Func<T, bool> validityChecker) where T : TileObject {
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i);
                if (poi is T obj) {
                    if (validityChecker.Invoke(obj)) {
                        return obj;
                    }
                }
            }
            return null;
        }
        public T GetFirstTileObjectOfType<T>(params TILE_OBJECT_TYPE[] types) where T : TileObject {
            //List<TileObject> objs = new List<TileObject>();
            for (int i = 0; i < types.Length; i++) {
                TILE_OBJECT_TYPE type = types[i];
                if (groupedTileObjects.ContainsKey(type)) {
                    List<TileObject> objs = groupedTileObjects[type];
                    if (objs != null) {
                        for (int j = 0; j < objs.Count; j++) {
                            TileObject t = objs[j];
                            if (t is T converted) {
                                return converted;
                            }
                        }
                    }
                }
            }
            //for (int i = 0; i < pointsOfInterest.Count; i++) {
            //    IPointOfInterest poi = pointsOfInterest.ElementAt(i);
            //    if (poi is TileObject) {
            //        TileObject obj = poi as TileObject;
            //        if (obj.tileObjectType == type) {
            //            return obj as T;
            //        }
            //    }
            //}
            return null;
        }
        public T GetTileObjectOfType<T>() where T : TileObject {
            //List<TileObject> objs = new List<TileObject>();
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i);
                if (poi is T obj) {
                    return obj;
                }
            }
            return null;
        }
        public bool AnyTileObjectsOfType<T>(TILE_OBJECT_TYPE tileObjectType, System.Func<T, bool> validityChecker = null) where T : TileObject {
            if (groupedTileObjects.ContainsKey(tileObjectType)) {
                List<TileObject> tileObjects = groupedTileObjects[tileObjectType];
                if (validityChecker != null) {
                    for (int i = 0; i < tileObjects.Count; i++) {
                        TileObject tileObject = tileObjects[i];
                        if (tileObject is T obj) {
                            if (validityChecker.Invoke(obj)) {
                                return true;
                            }
                        }

                    }
                } else {
                    //if no validity checker was provided then check if count of tile objects is greater than 0.
                    return tileObjects.Count > 0;
                }

            }
            return false;
        }
        public bool AnyTileObjectsOfType<T>(TILE_OBJECT_TYPE tileObjectType, out string log, System.Func<T, bool> validityChecker = null) where T : TileObject {
            log = $"Checking for tile objects of type {tileObjectType.ToString()} at {ToString()}";
            if (groupedTileObjects.ContainsKey(tileObjectType)) {
                List<TileObject> tileObjects = groupedTileObjects[tileObjectType];
                if (validityChecker != null) {
                    log += $"\nFound {tileObjects.Count.ToString()}, checking validity...";
                    for (int i = 0; i < tileObjects.Count; i++) {
                        TileObject tileObject = tileObjects[i];
                        if (tileObject is T obj) {
                            log += $"\nChecking validity of {obj.nameWithID}";
                            if (validityChecker.Invoke(obj)) {
                                log += $"\n{obj.nameWithID} is valid! Returning true!";
                                return true;
                            } else {
                                log += $"\n{obj.nameWithID} is not valid! Map Object State {obj.mapObjectState.ToString()}. Character Owner {obj.characterOwner?.name}";
                            }
                        }

                    }
                } else {
                    //if no validity checker was provided then check if count of tile objects is greater than 0.
                    return tileObjects.Count > 0;
                }
            }
            return false;
        }
        public int GetNumberOfTileObjectsThatMeetCriteria(TILE_OBJECT_TYPE type, Func<TileObject, bool> criteria) {
            if (groupedTileObjects.ContainsKey(type)) {
                List<TileObject> tileObjects = groupedTileObjects[type];
                if (tileObjects != null) {
                    if (criteria == null) {
                        return tileObjects.Count;
                    } else {
                        int count = 0;
                        for (int i = 0; i < tileObjects.Count; i++) {
                            TileObject t = tileObjects[i];
                            if (criteria.Invoke(t)) {
                                count++;
                            }
                        }
                        return count;
                    }
                } 
            }
            return 0;
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
                    if (chosenPile == null || obj.resourceInPile <= lowestCount) {
                        chosenPile = obj;
                        lowestCount = obj.resourceInPile;
                    }
                }
            }
            return chosenPile;
        }
        public ResourcePile GetResourcePileObjectWithLowestCount(TILE_OBJECT_TYPE tileObjectType, bool excludeMaximum = true) {
            ResourcePile chosenPile = null;
            int lowestCount = 0;
            for (int i = 0; i < pointsOfInterest.Count; i++) {
                IPointOfInterest poi = pointsOfInterest.ElementAt(i);
                if (poi is ResourcePile obj && obj.tileObjectType == tileObjectType) {
                    if (excludeMaximum && obj.IsAtMaxResource(obj.providedResource)) {
                        continue; //skip
                    }
                    if (chosenPile == null || obj.resourceInPile <= lowestCount) {
                        chosenPile = obj;
                        lowestCount = obj.resourceInPile;
                    }
                }
            }
            return chosenPile;
        }
        public void PopulateTileObjectsListWithAllTileObjects(List<TileObject> p_tileObjects) {
            foreach (var kvp in groupedTileObjects) {
                p_tileObjects.AddRange(kvp.Value);
            }
        }
        #endregion

        #region Points Of Interest
        public virtual bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null) {
            if (!pointsOfInterest.Contains(poi)) {
                pointsOfInterest.Add(poi);
                //if (poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER) {
                //    if (!PlaceAreaObjectAtAppropriateTile(poi, tileLocation)) {
                //        pointsOfInterest.Remove(poi);
                //        return false;
                //    }
                //}
                if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    TileObject tileObject = poi as TileObject;
                    if (!PlaceAreaObjectAtAppropriateTile(tileObject, tileLocation)) {
                        pointsOfInterest.Remove(poi);
                        return false;
                    }
                    if (groupedTileObjects.ContainsKey(tileObject.tileObjectType)) {
                        groupedTileObjects[tileObject.tileObjectType].Add(tileObject);
                    } else {
                        groupedTileObjects.Add(tileObject.tileObjectType, new List<TileObject>() { tileObject });
                    }
                    if (tileObject.gridTileLocation != null && tileObject.gridTileLocation.area.settlementOnArea is NPCSettlement npcSettlement) {
                        npcSettlement.OnItemAddedToLocation(tileObject, this);
                    }
                    // if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                    //     region.AddTileObjectInRegion(tileObject);    
                    // }
                }
                return true;
            }
            return false;
        }
        public void OnlyAddPOIToList(IPointOfInterest p_poi) {
            if (!pointsOfInterest.Contains(p_poi)) {
                Debug.Log($"Added {p_poi.name} to {name}");
                pointsOfInterest.Add(p_poi);
                if (p_poi is TileObject tileObject) {
                    if (groupedTileObjects.ContainsKey(tileObject.tileObjectType)) {
                        groupedTileObjects[tileObject.tileObjectType].Add(tileObject);
                    } else {
                        groupedTileObjects.Add(tileObject.tileObjectType, new List<TileObject>() { tileObject });
                    }    
                }
            }
        }
        public void OnlyRemovePOIFromList(IPointOfInterest p_poi) {
            if (pointsOfInterest.Remove(p_poi)) {
                Debug.Log($"Removed {p_poi.name} from {name}");
                if (p_poi is TileObject tileObject) {
                    if (groupedTileObjects.ContainsKey(tileObject.tileObjectType)) {
                        groupedTileObjects[tileObject.tileObjectType].Remove(tileObject);
                    }    
                }
            }
        }
        public virtual bool LoadPOI(TileObject poi, LocationGridTile tileLocation) {
            if (!pointsOfInterest.Contains(poi)) {
                pointsOfInterest.Add(poi);
                if (poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER) {
                    region.innerMap.LoadObject(poi, tileLocation);
                }
                if (poi is TileObject tileObject) {
                    if (groupedTileObjects.ContainsKey(tileObject.tileObjectType)) {
                        groupedTileObjects[tileObject.tileObjectType].Add(tileObject);
                    } else {
                        groupedTileObjects.Add(tileObject.tileObjectType, new List<TileObject>() { tileObject });
                    }
                    // if (tileObject.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                    //     region.AddTileObjectInRegion(tileObject);    
                    // }
                }
                return true;
            }
            return false;
        }
        public virtual bool RemovePOI(IPointOfInterest poi, Character removedBy = null) {
            LocationGridTile tileLocation = poi.gridTileLocation;
            TileObject tileObj = poi as TileObject;
            if (tileObj != null && tileObj.isHidden) {
                if (tileLocation != null && tileLocation.tileObjectComponent.hiddenObjHere == tileObj) {
                    tileLocation.tileObjectComponent.RemoveHiddenObjectHere(removedBy);
                    return true;
                }
            } else {
                if (pointsOfInterest.Remove(poi)) {
                    if (tileObj != null) {
                        groupedTileObjects[tileObj.tileObjectType].Remove(tileObj);
                    }
                    if (tileLocation != null) {
                        // Debug.Log("Removed " + poi.ToString() + " from " + poi.gridTileLocation.ToString() + " at " + this.ToString());
                        if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                            //location.areaMap.RemoveCharacter(poi.gridTileLocation, poi as Character);
                        } else {
                            region.innerMap.RemoveObject(tileLocation, removedBy);
                        }
                        //throw new System.Exception("Provided tile of " + poi.ToString() + " is null!");
                    }
                    if (tileObj != null) {
                        if (tileLocation.area.settlementOnArea is NPCSettlement npcSettlement) {
                            npcSettlement.OnItemRemovedFromLocation(tileObj, this, tileLocation);
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        public virtual bool RemovePOIWithoutDestroying(IPointOfInterest poi) {
            if (pointsOfInterest.Remove(poi)) {
                if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                    TileObject tileObject = poi as TileObject;
                    groupedTileObjects[tileObject.tileObjectType].Remove(tileObject);
                }
                if (poi.gridTileLocation != null) {
                    if (poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER) {
                        region.innerMap.RemoveObjectWithoutDestroying(poi.gridTileLocation);
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
                    groupedTileObjects[tileObject.tileObjectType].Remove(tileObject);
                    if (poi.gridTileLocation.area.settlementOnArea is NPCSettlement npcSettlement) {
                        npcSettlement.OnItemRemovedFromLocation(tileObject, this, poi.gridTileLocation);    
                    }
                }
                if (poi.gridTileLocation != null) {
                    if (poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER) {
                        region.innerMap.RemoveObjectDestroyVisualOnly(poi.gridTileLocation, remover);
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
        private bool PlaceAreaObjectAtAppropriateTile(TileObject poi, LocationGridTile tile) {
            if (tile != null) {
                region.innerMap.PlaceObject(poi, tile);
                return true;
            } else {
                List<LocationGridTile> tilesToUse = RuinarchListPool<LocationGridTile>.Claim();
                PopulateValidTilesToPlace(tilesToUse, poi);
                LocationGridTile chosenTile = null;
                if (tilesToUse.Count > 0) {
                    chosenTile = tilesToUse[UnityEngine.Random.Range(0, tilesToUse.Count)];
                } else if (unoccupiedTiles.Count > 0) {
                    chosenTile = unoccupiedTiles[UnityEngine.Random.Range(0, unoccupiedTiles.Count)];
                }
                RuinarchListPool<LocationGridTile>.Release(tilesToUse);
                if (chosenTile != null) {
                    region.innerMap.PlaceObject(poi, chosenTile);
                    return true;
                }
                // else {
                //     Debug.LogWarning("There are no tiles at " + structureType.ToString() + " at " + location.name + " for " + poi.ToString());
                // }
            }
            return false;
        }
        private void PopulateValidTilesToPlace(List<LocationGridTile> tiles, IPointOfInterest poi) {
            switch (poi.poiType) {
                case POINT_OF_INTEREST_TYPE.TILE_OBJECT:
                    if (poi is MagicCircle) {
                        for (int i = 0; i < unoccupiedTiles.Count; i++) {
                            LocationGridTile x = unoccupiedTiles[i];
                            if(!x.HasOccupiedNeighbour() && x.groundType != LocationGridTile.Ground_Type.Cave 
                                && x.groundType != LocationGridTile.Ground_Type.Water
                                && x.area.elevationType == ELEVATION.PLAIN
                                && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Wall)
                                && !x.HasNeighbourOfType(LocationGridTile.Ground_Type.Cave)
                                && !x.HasNeighbourOfType(LocationGridTile.Ground_Type.Water)
                                && !x.HasNeighbourOfElevation(ELEVATION.MOUNTAIN)
                                && !x.HasNeighbourOfElevation(ELEVATION.WATER)) {
                                tiles.Add(x);
                            }
                        }
                    } else if (poi is WaterWell) {
                        for (int i = 0; i < unoccupiedTiles.Count; i++) {
                            LocationGridTile x = unoccupiedTiles[i];
                            if (!x.HasOccupiedNeighbour() && !x.HasNeighbouringWalledStructure()) { //&& !x.GetTilesInRadius(3).Any(y => y.tileObjectComponent.objHere is WaterWell)
                                List<LocationGridTile> tilesInRadius = RuinarchListPool<LocationGridTile>.Claim();
                                x.PopulateTilesInRadius(tilesInRadius, 3);
                                if (!tilesInRadius.Any(y => y.tileObjectComponent.objHere is WaterWell)) {
                                    tiles.Add(x);
                                }
                                RuinarchListPool<LocationGridTile>.Release(tilesInRadius);
                            }
                        }
                    } else if (poi is GoddessStatue) {
                        for (int i = 0; i < unoccupiedTiles.Count; i++) {
                            LocationGridTile x = unoccupiedTiles[i];
                            if (!x.HasOccupiedNeighbour() && !x.HasNeighbouringWalledStructure()) { //&& !x.GetTilesInRadius(3).Any(y => y.tileObjectComponent.objHere is GoddessStatue)
                                List<LocationGridTile> tilesInRadius = RuinarchListPool<LocationGridTile>.Claim();
                                x.PopulateTilesInRadius(tilesInRadius, 3);
                                if (!tilesInRadius.Any(y => y.tileObjectComponent.objHere is GoddessStatue)) {
                                    tiles.Add(x);
                                }
                                RuinarchListPool<LocationGridTile>.Release(tilesInRadius);
                            }
                        }
                    } else if (poi is TreasureChest || poi is ElementalCrystal) {
                        for (int i = 0; i < unoccupiedTiles.Count; i++) {
                            LocationGridTile x = unoccupiedTiles[i];
                            if (x.IsPartOfSettlement() == false) {
                                tiles.Add(x);
                            }
                        }
                    } else if (poi is Guitar || poi is Bed || poi is Table) {
                        List<LocationGridTile> outerTiles = RuinarchListPool<LocationGridTile>.Claim();
                        PopulateOuterTiles(outerTiles);
                        for (int i = 0; i < outerTiles.Count; i++) {
                            LocationGridTile tile = outerTiles[i];
                            if (tile.tileState == LocationGridTile.Tile_State.Empty) {
                                //unoccupied tile
                                tiles.Add(tile);
                            }
                        }
                        RuinarchListPool<LocationGridTile>.Release(outerTiles);
                        //return GetOuterTiles().Where(x => unoccupiedTiles.Contains(x)).ToList();
                    }
                    break;
                    //else {
                    //    return unoccupiedTiles.ToList();
                    //}
                //case POINT_OF_INTEREST_TYPE.CHARACTER:
                //    return unoccupiedTiles.ToList();
                default:
                    for (int i = 0; i < unoccupiedTiles.Count; i++) {
                        LocationGridTile x = unoccupiedTiles[i];
                        if (!x.IsAdjacentTo(typeof(MagicCircle))) {
                            tiles.Add(x);
                        }
                    }
                    break;
                    //return unoccupiedTiles.Where(x => !x.IsAdjacentTo(typeof(MagicCircle))).ToList();
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
        protected virtual void OnTileAddedToStructure(LocationGridTile tile) {
            if (structureType != STRUCTURE_TYPE.WILDERNESS) {
                tile.area.structureComponent.AddStructureInArea(this);    
            }
        }
        protected virtual void OnTileRemovedFromStructure(LocationGridTile tile) {
            if (structureType != STRUCTURE_TYPE.WILDERNESS && !HasTileOnArea(tile.area)) {
                tile.area.structureComponent.RemoveStructureInArea(this);
            }
        }
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
                // if (structureType != STRUCTURE_TYPE.WILDERNESS && tile.IsPartOfSettlement(out var settlement)) {
                //     SetSettlementLocation(settlement);
                // }
                if (structureType != STRUCTURE_TYPE.WILDERNESS) {
                    AddOccupiedArea(tile.area);
                }
                OnTileAddedToStructure(tile);
            }
        }
        public void RemoveTile(LocationGridTile tile) {
            if (tiles.Remove(tile)) {
                OnTileRemovedFromStructure(tile);
                if (structureType != STRUCTURE_TYPE.WILDERNESS) {
                    RemoveOccupiedArea(tile.area);
                }
            }
            RemovePassableTile(tile);
            RemoveUnoccupiedTile(tile);
        }
        public void AddPassableTile(LocationGridTile tile) {
            passableTiles.Add(tile);
            //Debug.Log(name + " added passable tile: " + tile.ToString());
        }
        public void RemovePassableTile(LocationGridTile tile) {
            passableTiles.Remove(tile);
            //Debug.Log(name + " removed passable tile: " + tile.ToString());
        }
        public void AddUnoccupiedTile(LocationGridTile tile) {
            unoccupiedTiles.Add(tile);
        }
        public void RemoveUnoccupiedTile(LocationGridTile tile) {
            unoccupiedTiles.Remove(tile);
        }
        public LocationGridTile GetRandomTile() {
            if (tiles.Count <= 0) {
                return null;
            }
            return CollectionUtilities.GetRandomElement(tiles); //tiles[UtilityScripts.Utilities.Rng.Next(0, tiles.Count)];
        }
        public LocationGridTile GetRandomPassableTile() {
            if (passableTiles.Count <= 0) {
                return null;
            }
            return passableTiles[UtilityScripts.Utilities.Rng.Next(0, passableTiles.Count)];
        }
        public LocationGridTile GetRandomPassableTileThatMeetCriteria(Func<LocationGridTile, bool> criteria) {
            if (passableTiles.Count <= 0) {
                return null;
            }
            List<LocationGridTile> filteredList = ObjectPoolManager.Instance.CreateNewGridTileList();
            for (int i = 0; i < passableTiles.Count; i++) {
                LocationGridTile tile = passableTiles[i];
                if (criteria.Invoke(tile)) {
                    filteredList.Add(tile);
                }
            }
            LocationGridTile chosenTile = null;
            if(filteredList.Count > 0) {
                chosenTile = filteredList[UtilityScripts.Utilities.Rng.Next(0, filteredList.Count)];
            }
            ObjectPoolManager.Instance.ReturnGridTileListToPool(filteredList);
            return chosenTile;
        }
        public LocationGridTile GetRandomUnoccupiedTile() {
            if (unoccupiedTiles.Count <= 0) {
                return null;
            }
            return unoccupiedTiles.ElementAt(UnityEngine.Random.Range(0, unoccupiedTiles.Count));
        }
        public virtual void OnTileDamaged(LocationGridTile tile, int amount) { }
        public virtual void OnTileRepaired(LocationGridTile tile, int amount) { }
        private void AddOccupiedArea(Area p_area) {
            occupiedAreas.Add(p_area);
        }
        private void RemoveOccupiedArea(Area p_area) {
            occupiedAreas.Remove(p_area);
        }
        #endregion

        #region Structure Objects
        public void SetOccupiedArea(Area p_area) {
            occupiedArea = p_area;
            Debug.Log($"Set Occupied area of {name} to {occupiedArea}");
        }
        private void OnClickStructure() {
            Selector.Instance.Select(this);
        }
        #endregion

        #region Destroy
        protected virtual void DestroyStructure(Character p_responsibleCharacter = null) {
            if (hasBeenDestroyed) {
                return;
            }
            hasBeenDestroyed = true;
            Debug.Log($"{GameManager.Instance.TodayLogString()}{ToString()} was destroyed!");

            //transfer tiles to either the wilderness or work npcSettlement
            List<LocationGridTile> tilesInStructure = new List<LocationGridTile>(tiles);
            LocationStructure wilderness = region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            for (int i = 0; i < tilesInStructure.Count; i++) {
                LocationGridTile tile = tilesInStructure[i];
                tile.tileObjectComponent.ClearWallObjects();
                IPointOfInterest obj = tile.tileObjectComponent.objHere;
                if (obj != null) {
                    if (obj is TileObject tileObject && tileObject.traitContainer.HasTrait("Indestructible")) {
                        if (tileObject.isPreplaced) {
                            LocationStructureObject structureObject = null;
                            if (this is DemonicStructure demonicStructure) {
                                structureObject = demonicStructure.structureObj;
                            } else if (this is ManMadeStructure manMadeStructure) {
                                structureObject = manMadeStructure.structureObj;
                            }
                            if (structureObject != null) {
                                if (structureObject.HasPreplacedObjectOfType(tileObject.tileObjectType)) {
                                    //if indestructible object is part of this structures template, then remove it
                                    obj.gridTileLocation?.structure.RemovePOI(obj);
                                } else {
                                    //otherwise, transfer it to the wilderness structure
                                    OnlyRemovePOIFromList(tileObject);
                                    wilderness.OnlyAddPOIToList(tileObject);        
                                }
                            }
                        } else {
                            //tile object is not pre placed and is indestructible, do not destroy
                            OnlyRemovePOIFromList(tileObject);
                            wilderness.OnlyAddPOIToList(tileObject);    
                        }
                        
                    } else {
                        // obj.AdjustHP(-tile.tileObjectComponent.objHere.maxHP, ELEMENTAL_TYPE.Normal, showHPBar: true);
                        obj.gridTileLocation?.structure.RemovePOI(obj); //because sometimes adjusting the hp of the object to 0 does not remove it?    
                    }
                }
                
                tile.SetStructure(wilderness);
                tile.SetTileType(LocationGridTile.Tile_Type.Empty);
                if (tile.groundType.IsStructureType()) {
                    tile.tileObjectComponent.genericTileObject.AdjustHP(-tile.tileObjectComponent.genericTileObject.maxHP, ELEMENTAL_TYPE.Normal);
                }
                if (structureType.IsPlayerStructure()) {
                    //once demonic structure is destroyed, revert all tiles to corrupted.
                    tile.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.corruptedTile);
                }
            }
            //transfer characters here to wilderness
            for (int i = 0; i < charactersHere.Count; i++) {
                Character character = charactersHere[i];
                RemoveCharacterAtLocation(character);
                character.gridTileLocation?.structure.AddCharacterAtLocation(character);
            }
            charactersHere.Clear();
            
            if (rooms != null) {
                for (int i = 0; i < rooms.Length; i++) {
                    StructureRoom room = rooms[i];
                    room.OnParentStructureDestroyed();
                }
            }
            AfterStructureDestruction(p_responsibleCharacter);
        }
        protected virtual void AfterStructureDestruction(Character p_responsibleCharacter = null) {
            //disable game object. Destruction of structure game object is handled by it's parent structure template.
            region.RemoveStructure(this);
            settlementLocation.RemoveStructure(this);
            Messenger.Broadcast(StructureSignals.STRUCTURE_OBJECT_REMOVED, this, occupiedArea);
            SetOccupiedArea(null);
            UnsubscribeListeners();
            Messenger.Broadcast(StructureSignals.STRUCTURE_DESTROYED, this);
            if (p_responsibleCharacter != null) {
                Messenger.Broadcast(StructureSignals.STRUCTURE_DESTROYED_BY, this, p_responsibleCharacter);
            }
            eventDispatcher.ExecuteStructureDestroyed(this);
        }
        #endregion

        #region Player Action Target
        public List<PLAYER_SKILL_TYPE> actions { get; private set; }

        public virtual void ConstructDefaultActions() {
            actions = new List<PLAYER_SKILL_TYPE>();
        }
        public void AddPlayerAction(PLAYER_SKILL_TYPE action) {
            if (actions.Contains(action) == false) {
                actions.Add(action);
                Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_ADDED_TO_TARGET, action, this as IPlayerActionTarget);    
            }
        }
        public void RemovePlayerAction(PLAYER_SKILL_TYPE action) {
            if (actions.Remove(action)) {
                Messenger.Broadcast(PlayerSkillSignals.PLAYER_ACTION_REMOVED_FROM_TARGET, action, this as IPlayerActionTarget);
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
            // CenterOnStructure();
            UIManager.Instance.ShowStructureInfo(this);
        }
        public void RightSelectAction() {
            if(structureType.IsPlayerStructure() || structureType == STRUCTURE_TYPE.CITY_CENTER) {
                Vector3 worldPos = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
                IPlayerActionTarget target = this;
                if (structureType == STRUCTURE_TYPE.CITY_CENTER) {
                    //Right clicking on the city center should show right click actions for the settlement instead of the structure
                    target = settlementLocation;
                }
                UIManager.Instance.ShowPlayerActionContextMenu(target, worldPos, false);
            }
        }
        public void MiddleSelectAction() { }
        public bool CanBeSelected() {
            return true;
        }
        #endregion

        #region HP
        public void AddObjectAsDamageContributor(IDamageable damageable) {
            objectsThatContributeToDamage.Add(damageable);
            if(damageable is TileObject to) {
                to.SetAsDamageContributorToStructure(true);
            }
        }
        protected void OnObjectDamaged(TileObject tileObject, int amount) {
            if (objectsThatContributeToDamage.Contains(tileObject)) {
                AdjustHP(amount);
                if (!objectsThatContributeToDamage.Any(o => o.currentHP > 0)) {
                    //if this structure no longer has any objects that have hp, then destroy this structure
                    AdjustHP(-currentHP);
                }
            }
        }
        protected void OnObjectDamagedBy(TileObject tileObject, int amount, Character p_responsibleCharacter) {
            if (objectsThatContributeToDamage.Contains(tileObject)) {
                AdjustHP(amount, p_responsibleCharacter);
                if (!objectsThatContributeToDamage.Any(o => o.currentHP > 0)) {
                    //if this structure no longer has any objects that have hp, then destroy this structure
                    AdjustHP(-currentHP, p_responsibleCharacter);
                }
            }
        }
        protected void OnObjectRepaired(TileObject tileObject, int amount) {
            if (objectsThatContributeToDamage.Contains(tileObject)) {
                AdjustHP(amount);
            }
        }
        public void AdjustHP(int amount, Character p_responsibleCharacter = null) {
            if (hasBeenDestroyed) { return; }
            currentHP += amount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            if (currentHP == 0) {
                DestroyStructure(p_responsibleCharacter);
            }
        }
        public void SetMaxHP(int amount) {
            maxHP = amount;
        }
        public void SetMaxHPAndReset(int amount) {
            SetMaxHP(amount);
            ResetHP();
        }
        public void ResetHP() {
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
                Messenger.Broadcast(StructureSignals.ADDED_STRUCTURE_RESIDENT, character, this);
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
                Messenger.Broadcast(StructureSignals.REMOVED_STRUCTURE_RESIDENT, character, this);
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
                        if(character.relationshipContainer.IsFamilyMember(resident)) {
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
        public bool HasReachedMaxResidentCapacity() {
            return residents.Count >= maxResidentCapacity;
        }
        public bool HasResidentThatMeetCriteria(Func<Character, bool> checker) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (checker.Invoke(resident)) {
                    return true;
                }
            }
            return false;
        }
        public int GetNumberOfReidentsThatMeetCriteria(System.Func<Character, bool> criteria) {
            int count = 0;
            for (int i = 0; i < residents.Count; i++) {
                Character character = residents[i];
                if (criteria.Invoke(character)) {
                    count++;
                }
            }
            return count;
        }
        #endregion

        #region Rooms
        public void CreateRoomsBasedOnStructureObject(LocationStructureObject structureObject) {
            if (structureObject.roomTemplates == null || structureObject.roomTemplates.Length == 0) { return; }
            rooms = new StructureRoom[structureObject.roomTemplates.Length];
            for (int i = 0; i < rooms.Length; i++) {
                RoomTemplate roomTemplate = structureObject.roomTemplates[i];
                StructureRoom newRoom = CreteNewRoomForStructure(structureObject.GetTilesOccupiedByRoom(region.innerMap, roomTemplate));
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

        #region IPartyTargetDestination
        public virtual bool IsAtTargetDestination(Character character) {
            return character.currentStructure == this;
        }
        #endregion

        #region Building
        public void OnBuiltNewStructureFromBlueprint() {
            if (settlementLocation is NPCSettlement settlement) {
                settlement.OnStructureBuilt(this);
            }
        }
        #endregion

        public virtual void OnCharacterUnSeizedHere(Character character) { }

        #region Testing
        public virtual string GetTestingInfo() {
            string summary = $"{name} Info:";
            summary += "\nDamage Contributing Objects:";
            for (int i = 0; i < objectsThatContributeToDamage.Count; i++) {
                IDamageable damageable = objectsThatContributeToDamage.ElementAt(i);
                summary += $"\n\t- {damageable}";
            }
            return summary;
        }
        #endregion
        
        #region IStoredTarget
        public bool CanBeStoredAsTarget() {
            return !hasBeenDestroyed;
        }
        public void SetAsStoredTarget(bool p_state) {
            isStoredAsTarget = p_state;
        }
        #endregion

        #region IBookmarkable
        public void OnSelectBookmark() {
            LeftSelectAction();
        }
        public void RemoveBookmark() {
            PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this);
        }
        public void OnHoverOverBookmarkItem() { }
        public void OnHoverOutBookmarkItem() { }
        #endregion
    }
}