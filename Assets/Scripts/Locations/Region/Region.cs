using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Region_Features;
using Locations.Settlements;
using Logs;
using PathFind;
using SpriteGlow;
using UnityEngine;
using UtilityScripts;
using Random = UnityEngine.Random;

public class Region : ISavable, ILogFiller {
    public string persistentID { get; }
    public int id { get; }
    public string name { get; private set; }
    public string description => GetDescription();
    public List<Area> areas { get; private set; }
    public Area coreTile { get; private set; }
    public Color regionColor { get; }
    public List<Faction> factionsHere { get; private set; }
    public List<Character> residents { get; private set; }
    public List<Character> charactersAtLocation { get; private set; }
    public Area[,] areaMap => GridMap.Instance.map; //TODO:
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; private set; }
    public List<LocationStructure> allStructures { get; private set; }
    public RegionFeatureComponent regionFeatureComponent { get; }
    public List<BaseSettlement> settlementsInRegion { get; private set; }
    public BiomeDivisionComponent biomeDivisionComponent { get; }
    /// <summary>
    /// Number of tile objects in this region categorized by type.
    /// NOTE: This isn't saved/loaded since this is updated every time a new tile object is placed.
    /// So loading this from a file would result in duplicates, since we still go through the process of
    /// placing tile objects when loading them.
    /// NOTE: This just includes BUILT objects!
    /// </summary>
    public Dictionary<TILE_OBJECT_TYPE, int> objectsInRegionCount { get; private set; }

    private RegionInnerTileMap _regionInnerTileMap; //inner map of the region, this should only be used if this region does not have an npcSettlement. 
    private string _activeEventAfterEffectScheduleId;
    //private int _canShowNotificationVotes;

    #region getter/setter
    public InnerTileMap innerMap => _regionInnerTileMap;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Region;
    public Type serializedData => typeof(SaveDataRegion);
    //public bool canShowNotifications => _canShowNotificationVotes > 0;
    #endregion

    private Region() {
        charactersAtLocation = new List<Character>();
        factionsHere = new List<Faction>();
        residents = new List<Character>();
        regionFeatureComponent = new RegionFeatureComponent();
        settlementsInRegion = new List<BaseSettlement>();
        objectsInRegionCount = new Dictionary<TILE_OBJECT_TYPE, int>();
    }
    public Region(Area coreTile, string p_name = "") : this() {
        persistentID = System.Guid.NewGuid().ToString();
        id = UtilityScripts.Utilities.SetID(this);
        name = string.IsNullOrEmpty(p_name) ? RandomNameGenerator.GetRegionName() : p_name;
        this.coreTile = coreTile;
        areas = new List<Area>();
        AddTile(coreTile);
        regionColor = GenerateRandomRegionColor();
        biomeDivisionComponent = new BiomeDivisionComponent();
        Debug.Log($"Created region {this.name} with core tile {coreTile.ToString()}");
    }
    public Region(SaveDataRegion data) : this() {
        persistentID = data.persistentID;
        id = UtilityScripts.Utilities.SetID(this, data.id);
        name = data.name;
        coreTile = GridMap.Instance.allAreas[data.coreTileID];
        areas = new List<Area>();
        regionColor = data.regionColor;
        objectsInRegionCount = new Dictionary<TILE_OBJECT_TYPE, int>();

        //Components
        biomeDivisionComponent = data.regionDivisionComponent.Load();
    }

    #region Loading
    public void LoadReferences(SaveDataRegion saveDataRegion) {
        string summary = $"Loading {name} references:";
        summary = $"{summary}\nLoading Residents:";
        for (int i = 0; i < saveDataRegion.residentIDs.Length; i++) {
            string residentID = saveDataRegion.residentIDs[i];
            Character resident = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(residentID);
            if (resident != null) {
                residents.Add(resident);
                summary = $"{summary}\n- {resident.name}";    
            } else {
                Debug.LogWarning($"Trying to add resident at {name} with ID {residentID} but could not find character with that ID");
            }
            
        }
        summary = $"{summary}\nLoading characters at Location:";
        for (int i = 0; i < saveDataRegion.charactersAtLocationIDs.Length; i++) {
            string charactersAtLocationID = saveDataRegion.charactersAtLocationIDs[i];
            Character character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(charactersAtLocationID);
            if (character != null) {
                charactersAtLocation.Add(character);
                summary = $"{summary}\n- {character.name}";    
            } else {
                Debug.LogWarning($"Trying to add character at location {name} with ID {charactersAtLocationID} but could not find character with that ID");
            }
        }
        for (int i = 0; i < saveDataRegion.factionsHereIDs.Length; i++) {
            string factionHereID = saveDataRegion.factionsHereIDs[i];
            Faction faction = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(factionHereID);
            factionsHere.Add(faction);
        }
        
        Debug.Log(summary);
    }
    #endregion

    #region Tiles
    public void AddTile(Area tile) {
        if (!areas.Contains(tile)) {
            areas.Add(tile);
            tile.SetRegion(this);
        }
    }
    #endregion

    #region Utilities
    public void SetName(string name) {
        this.name = name;
    }
    private Color GenerateRandomRegionColor() {
        if (id == 1) {
            return Color.cyan;
        } else if (id == 2) {
            return Color.yellow;
        } else if (id == 3) {
            return Color.green;
        } else if (id == 4) {
            return Color.red;
        } else if (id == 5) {
            return Color.magenta;
        }
        return Random.ColorHSV();
    }
    private string GetDescription() {
        // if (coreTile.isCorrupted) {
        //     if (mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
        //         return "This region is empty. You may assign a minion to build a demonic landmark here.";
        //     }
        // }
        // return LandmarkManager.Instance.GetLandmarkData(mainLandmark.specificLandmarkType).description;
        return string.Empty;
    }
    public void OnHoverOverAction() { }
    public void OnHoverOutAction() { }
    public bool HasTileWithFeature(string featureName) {
        for (int i = 0; i < areas.Count; i++) {
            Area tile = areas[i];
            if (tile.featureComponent.HasFeature(featureName)) {
                return true;
            }
        }
        return false;
    }
    public List<Area> GetTilesWithFeature(string featureName) {
        List<Area> tilesWithFeature = new List<Area>();
        for (int i = 0; i < areas.Count; i++) {
            Area tile = areas[i];
            if (tile.featureComponent.HasFeature(featureName)) {
                tilesWithFeature.Add(tile);
            }
        }
        return tilesWithFeature;
    }
    public void PopulateAreasOccupiedByVillagers(List<Area> areas) {
        for (int i = 0; i < residents.Count; i++) {
            Character regionResident = residents[i];
            if (regionResident.isNormalCharacter && regionResident.HasTerritory()) {
                if (areas.Contains(regionResident.territory) == false) {
                    areas.Add(regionResident.territory);
                }
            }
        }
    }
    #endregion

    #region Characters
    public void LoadCharacterHere(Character character) {
        charactersAtLocation.Add(character);
        character.SetRegionLocation(this);
        Messenger.Broadcast(RegionSignals.CHARACTER_ENTERED_REGION, character, this);
    }
    public void AddCharacterToLocation(Character character, LocationGridTile tileOverride = null, bool isInitial = false) {
        character.SetRegionLocation(this);
        if (!charactersAtLocation.Contains(character)) {
            charactersAtLocation.Add(character);
            Messenger.Broadcast(RegionSignals.CHARACTER_ENTERED_REGION, character, this);
        }
    }
    public void RemoveCharacterFromLocation(Character character) {
        if (charactersAtLocation.Remove(character)) {
            character.currentStructure?.RemoveCharacterAtLocation(character);
            // for (int i = 0; i < features.Count; i++) {
            //     features[i].OnRemoveCharacterFromRegion(this, character);
            // }
            character.SetRegionLocation(null);
            Messenger.Broadcast(RegionSignals.CHARACTER_EXITED_REGION, character, this);
        }
    }
    //public void RemoveCharacterFromLocation(Party party) {
    //    RemoveCharacterFromLocation(party.owner);
    //}
    public bool IsResident(Character character) {
        return residents.Contains(character);
    }
    public bool AddResident(Character character) {
        if (!residents.Contains(character)) {
            residents.Add(character);
            character.SetHomeRegion(this);
        }
        return false;
    }
    public void RemoveResident(Character character) {
        if (residents.Remove(character)) {
            character.SetHomeRegion(null);
        }
    }
    public int GetCountOfAliveCharacterWithSameTerritory(Character character) {
        int count = 0;
        if (character.HasTerritory()) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (resident != character && !resident.isDead && resident.HasTerritory()) {
                    if (resident.IsTerritory(character.territory)) {
                        count++;
                    }
                }
            }
        }
        return count;
    }
    public Character GetRandomCharacterWithPathAndFaction(Character source) {
        List<Character> validCharacters = ObjectPoolManager.Instance.CreateNewCharactersList();
        Character chosenCharacter = null;
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character character = charactersAtLocation[i];
            if (source != character && source.movementComponent.HasPathTo(character.gridTileLocation) && !character.isDead && character.faction == source.faction) {
                validCharacters.Add(character);
            }
        }
        if(validCharacters != null) {
            chosenCharacter = UtilityScripts.CollectionUtilities.GetRandomElement(validCharacters);
        }
        ObjectPoolManager.Instance.ReturnCharactersListToPool(validCharacters);
        return chosenCharacter;
    }
    public Character GetRandomCharacterThatMeetCriteria(System.Func<Character, bool> validityChecker) {
        List<Character> validCharacters = ObjectPoolManager.Instance.CreateNewCharactersList();
        Character chosenCharacter = null;
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character character = charactersAtLocation[i];
            if (validityChecker.Invoke(character)) {
                validCharacters.Add(character);
            }
        }
        if (validCharacters != null) {
            chosenCharacter = UtilityScripts.CollectionUtilities.GetRandomElement(validCharacters);
        }
        ObjectPoolManager.Instance.ReturnCharactersListToPool(validCharacters);
        return chosenCharacter;
    }
    #endregion

    #region Faction
    public void AddFactionHere(Faction faction) {
        if (!IsFactionHere(faction) && faction.isMajorFaction) {
            factionsHere.Add(faction);
            //Once a faction is added and there is no ruling faction yet, automatically let the added faction own the region
            // if(owner == null) {
            //     LandmarkManager.Instance.OwnRegion(faction, this);
            // }
        }
    }
    public void RemoveFactionHere(Faction faction) {
        if (factionsHere.Remove(faction)) {
            //If a faction is removed and it is the ruling faction, transfer ruling faction to the next faction on the list if there's any, if not make the region part of neutral faction
            // if(owner == faction) {
            //     LandmarkManager.Instance.UnownSettlement(this);
            //     if(factionsHere.Count > 0) {
            //         LandmarkManager.Instance.OwnRegion(factionsHere[0], this);
            //     } else {
            //         FactionManager.Instance.neutralFaction.AddToOwnedSettlements(this);
            //     }
            // }
        }
    }
    public bool IsFactionHere(Faction faction) {
        return factionsHere.Contains(faction);
    }
    #endregion
    
    #region Structures
    public void CreateStructureList() {
        structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
        allStructures = new List<LocationStructure>();
    }
    public void GenerateStructures() {
        CreateStructureList();
        LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WILDERNESS);
    }
    public void AddStructure(LocationStructure structure) {
        Debug.Assert(!structure.hasBeenDestroyed, $"Structure {structure} has been destroyed but is being added to {name}");
        if (!structures.ContainsKey(structure.structureType)) {
            structures.Add(structure.structureType, new List<LocationStructure>());
        }

        if (!structures[structure.structureType].Contains(structure)) {
            structures[structure.structureType].Add(structure);
            allStructures.Add(structure);
            // Debug.Log($"New structure {structure.name} was added to region {name}" );
        }
    }
    public void RemoveStructure(LocationStructure structure) {
        if (structures.ContainsKey(structure.structureType)) {
            if (structures[structure.structureType].Remove(structure)) {
                allStructures.Remove(structure);
                if (structures[structure.structureType].Count == 0) { //this is only for optimization
                    structures.Remove(structure.structureType);
                }
            }
        }
    }
    public LocationStructure GetRandomStructureOfType(STRUCTURE_TYPE type) {
        if (structures.ContainsKey(type)) {
            return structures[type][UtilityScripts.Utilities.Rng.Next(0, structures[type].Count)];
        }
        return null;
    }
    public LocationStructure GetFirstUnoccupiedStructureOfType(STRUCTURE_TYPE type) {
        if (structures.ContainsKey(type)) {
            List<LocationStructure> structuresOfType = structures[type];
            for (int i = 0; i < structuresOfType.Count; i++) {
                if (!structuresOfType[i].IsOccupied()) {
                    return structuresOfType[i];
                }
            }
        }
        return null;
    }
    public LocationStructure GetRandomStructure() {
        LocationStructure randomStructure = null;
        while (randomStructure == null) {
            KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp = structures.ElementAt(UnityEngine.Random.Range(0, structures.Count));
            if (kvp.Key != STRUCTURE_TYPE.CAVE && kvp.Key != STRUCTURE_TYPE.OCEAN && kvp.Value.Count > 0) {
                randomStructure = kvp.Value[UnityEngine.Random.Range(0, kvp.Value.Count)];
            }
        }
        return randomStructure;
        //Dictionary<STRUCTURE_TYPE, List<LocationStructure>> _allStructures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>(this.structures);
        //_allStructures.Remove(STRUCTURE_TYPE.CAVE);
        //_allStructures.Remove(STRUCTURE_TYPE.OCEAN);
        //int dictIndex = UnityEngine.Random.Range(0, _allStructures.Count);
        //int count = 0;
        //foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in _allStructures) {
        //    if (count == dictIndex) {
        //        return kvp.Value[UnityEngine.Random.Range(0, kvp.Value.Count)];
        //    }
        //    count++;
        //}
        //return null;
    }
    public LocationStructure GetRandomStructureThatMeetCriteria(System.Func<LocationStructure, bool> checker) {
        List<LocationStructure> structureChoices = ObjectPoolManager.Instance.CreateNewStructuresList();
        LocationStructure chosenStructure = null;
        for (int i = 0; i < allStructures.Count; i++) {
            LocationStructure currStructure = allStructures[i];
            if (checker.Invoke(currStructure)) {
                structureChoices.Add(currStructure);
            }
        }
        if (structureChoices != null && structureChoices.Count > 0) {
            chosenStructure = structureChoices[UnityEngine.Random.Range(0, structureChoices.Count)];
        }
        ObjectPoolManager.Instance.ReturnStructuresListToPool(structureChoices);
        return chosenStructure;
    }
    public LocationStructure GetRandomStructureOfTypeThatMeetCriteria(System.Func<LocationStructure, bool> checker, params STRUCTURE_TYPE[] type) {
        List<LocationStructure> structureChoices = ObjectPoolManager.Instance.CreateNewStructuresList();
        LocationStructure chosenStructure = null;
        for (int i = 0; i < type.Length; i++) {
            if (structures.ContainsKey(type[i])) {
                List<LocationStructure> structuresOfType = structures[type[i]];
                for (int j = 0; j < structuresOfType.Count; j++) {
                    LocationStructure possibleStructure = structuresOfType[j];
                    if (checker.Invoke(possibleStructure)) {
                        structureChoices.Add(possibleStructure);
                    }
                }
            }
        }
        if (structureChoices != null && structureChoices.Count > 0) {
            chosenStructure = structureChoices[UnityEngine.Random.Range(0, structureChoices.Count)];
        }
        ObjectPoolManager.Instance.ReturnStructuresListToPool(structureChoices);
        return chosenStructure;
    }
    public LocationStructure GetRandomSpecialStructureExcept(List<LocationStructure> exceptions) {
        List<LocationStructure> specialStructures = ObjectPoolManager.Instance.CreateNewStructuresList();
        LocationStructure chosenStructure = null;
        for (int i = 0; i < allStructures.Count; i++) {
            LocationStructure currStructure = allStructures[i];
            if (currStructure.settlementLocation != null && currStructure.settlementLocation.locationType == LOCATION_TYPE.DUNGEON && currStructure.passableTiles.Count > 0) {
                if(exceptions.Contains(currStructure)) { continue; }
                specialStructures.Add(currStructure);
            }
        }
        if (specialStructures != null && specialStructures.Count > 0) {
            chosenStructure = specialStructures[UnityEngine.Random.Range(0, specialStructures.Count)];
        }
        ObjectPoolManager.Instance.ReturnStructuresListToPool(specialStructures);
        return chosenStructure;
    }
    public LocationStructure GetStructureOfTypeWithoutSettlement(STRUCTURE_TYPE type) {
        if (structures.ContainsKey(type)) {
            List<LocationStructure> chosenStructures = structures[type];
            for (int i = 0; i < chosenStructures.Count; i++) {
                if(chosenStructures[i].settlementLocation == null) {
                    return chosenStructures[i];
                }
            }
        }
        return null;
    }
    public LocationStructure GetStructureByID(STRUCTURE_TYPE type, int id) {
        if (structures.ContainsKey(type)) {
            List<LocationStructure> locStructures = structures[type];
            for (int i = 0; i < locStructures.Count; i++) {
                if(locStructures[i].id == id) {
                    return locStructures[i];
                }
            }
        }
        return null;
    }
    public List<LocationStructure> GetStructuresAtLocation() {
        List<LocationStructure> structuresAtLocation = new List<LocationStructure>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in this.structures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStructure = kvp.Value[i];
                structuresAtLocation.Add(currStructure);
            }
        }
        return structuresAtLocation;
    }
    public List<T> GetStructuresAtLocation<T>(STRUCTURE_TYPE type) where T : LocationStructure{
        List<T> structuresAtLocation = new List<T>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in this.structures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                LocationStructure currStructure = kvp.Value[i];
                if (currStructure.structureType == type) {
                    structuresAtLocation.Add(currStructure as T);
                }
            }
        }
        return structuresAtLocation;
    }
    public bool HasStructure(STRUCTURE_TYPE type) {
        return structures.ContainsKey(type);
    }
    #endregion

    #region Inner Map
    public void SetRegionInnerMap(RegionInnerTileMap regionInnerTileMap) {
        _regionInnerTileMap = regionInnerTileMap;
    }
    //public bool AddSpecialTokenToLocation(SpecialToken token, LocationStructure structure = null, LocationGridTile gridLocation = null) {
    //    token.SetOwner(this.owner);
    //    if (innerMap != null) { //if the npcSettlement map of this npcSettlement has already been created.
    //        if (structure != null) {
    //            structure.AddItem(token, gridLocation);
    //        } else {
    //            //get structure for token
    //            LocationStructure chosen = InnerMapManager.Instance.GetRandomStructureToPlaceItem(this, token);
    //            chosen.AddItem(token);
    //        }
    //    }
    //    return true;
    //}
    //public void RemoveSpecialTokenFromLocation(SpecialToken token) {
    //    LocationStructure takenFrom = token.structureLocation;
    //    if (takenFrom != null) {
    //        takenFrom.RemoveItem(token);
    //    }
    //}
    public bool IsRequiredByLocation(TileObject item) {
        return false;
    }
    //public void AllowNotifications() {
    //    _canShowNotificationVotes++;
    //}
    //public void BlockNotifications() {
    //    _canShowNotificationVotes--;
    //}
    public List<TileObject> GetTileObjectsOfType(TILE_OBJECT_TYPE type) {
        List<TileObject> objs = new List<TileObject>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                List<TileObject> tileObjects = keyValuePair.Value[i].GetTileObjectsOfType(type);
                if(tileObjects != null) {
                    objs.AddRange(tileObjects);
                }
            }
        }
        return objs;
    }
    public bool HasTileObjectOfType(TILE_OBJECT_TYPE type) {
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                LocationStructure structure = keyValuePair.Value[i];
                if (structure.HasTileObjectOfType(type)) {
                    return true;
                }
            }
        }
        return false;
    }
    public List<T> GetTileObjectsOfType<T>() where T : TileObject{
        List<T> objs = new List<T>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                objs.AddRange(keyValuePair.Value[i].GetTileObjectsOfType<T>());
            }
        }
        return objs;
    }
    #endregion

    #region Hex Tile Map
    public Area GetRandomHexThatMeetCriteria(System.Func<Area, bool> validityChecker) {
        List<Area> hexes = RuinarchListPool<Area>.Claim();
        Area chosenHex = null;
        for (int i = 0; i < areas.Count; i++) {
            Area currHex = areas[i];
            if (validityChecker.Invoke(currHex)) {
                hexes.Add(currHex);
            }
        }
        if (hexes != null && hexes.Count > 0) {
            chosenHex = CollectionUtilities.GetRandomElement(hexes);
        }
        RuinarchListPool<Area>.Release(hexes);
        return chosenHex;
    }
    #endregion

    #region Settlements
    public void UpdateSettlementsInRegion() {
        settlementsInRegion.Clear();
        for (int i = 0; i < areas.Count; i++) {
            Area tile = areas[i];
            if (tile.settlementOnArea != null) {
                if (!settlementsInRegion.Contains(tile.settlementOnArea)) {
                    settlementsInRegion.Add(tile.settlementOnArea);    
                }
            }
        }
    }
    public List<BaseSettlement> GetSettlementsInRegion(System.Func<BaseSettlement, bool> validityChecker) {
        List<BaseSettlement> settlements = null;
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement settlement = settlementsInRegion[i];
            if (validityChecker.Invoke(settlement)) {
                if (settlements == null) {
                    settlements = new List<BaseSettlement>();
                }
                if (settlements.Contains(settlement) == false) {
                    settlements.Add(settlement);    
                }
            }
        }
        return settlements;
    }
    public BaseSettlement GetFirstSettlementInRegion(System.Func<BaseSettlement, bool> validityChecker) {
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement settlement = settlementsInRegion[i];
            if (validityChecker.Invoke(settlement)) {
                return settlement;
            }
        }
        return null;
    }
    public bool IsRegionVillageCapacityReached() {
        int count = 0;
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement settlement = settlementsInRegion[i];
            if (settlement.locationType == LOCATION_TYPE.VILLAGE) {
                count++;
            }
        }
        return count >= LandmarkManager.REGION_VILLAGE_CAPACITY;
    }
    #endregion

    #region Tile Objects
    public void AddTileObjectInRegion(TileObject tileObject) {
        if (!objectsInRegionCount.ContainsKey(tileObject.tileObjectType)) {
            objectsInRegionCount.Add(tileObject.tileObjectType, 0);
        }
        objectsInRegionCount[tileObject.tileObjectType] += 1;
        // if (tileObject.tileObjectType == TILE_OBJECT_TYPE.WOOD_PILE) {
        //     int count = objectsInRegionCount[tileObject.tileObjectType];
        //     Debug.Log($"Added {tileObject.nameWithID} to objects in region. Count is {count.ToString()}");    
        // }
    }
    public void RemoveTileObjectInRegion(TileObject tileObject) {
        if (objectsInRegionCount.ContainsKey(tileObject.tileObjectType)) {
            objectsInRegionCount[tileObject.tileObjectType] -= 1;
            // if (tileObject.tileObjectType == TILE_OBJECT_TYPE.WOOD_PILE) {
            //     Debug.Log($"Removed {tileObject.nameWithID} to objects in region. Count is {objectsInRegionCount[tileObject.tileObjectType].ToString()}");
            // }
            if (objectsInRegionCount[tileObject.tileObjectType] <= 0) {
                objectsInRegionCount.Remove(tileObject.tileObjectType);
            }
        }
    }
    public int GetTileObjectInRegionCount(TILE_OBJECT_TYPE tileObjectType) {
        if (objectsInRegionCount.ContainsKey(tileObjectType)) {
            return objectsInRegionCount[tileObjectType];
        }
        return 0;
    }
    #endregion

    public void CleanUp() {
        areas?.Clear();
        areas = null;
        coreTile = null;
        factionsHere?.Clear();
        factionsHere = null;
        residents?.Clear();
        residents = null;
        charactersAtLocation?.Clear();
        charactersAtLocation = null;
        structures?.Clear();
        structures = null;
        settlementsInRegion?.Clear();
        settlementsInRegion = null;
    }
}

public class Border {
    private SpriteRenderer borderSprite { get; }
    private SpriteGlowEffect glowEffect { get; }

    public Border(SpriteRenderer _borderSprite, SpriteGlowEffect _glowEffect) {
        borderSprite = _borderSprite;
        glowEffect = _glowEffect;
        SetGlowState(false);
    }
    
    public void SetBorderState(bool state) {
        borderSprite.gameObject.SetActive(state);
    }
    public void SetGlowState(bool state) {
        glowEffect.enabled = state;
    }

    public void SetColor(Color color) {
        borderSprite.color = color;
        glowEffect.GlowColor = color;
    }
}