using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Grid_Tile_Features;
using Inner_Maps.Location_Structures;
using Locations.Region_Features;
using Locations.Settlements;
using Logs;
using PathFind;
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
    public Area[,] areaMap => GridMap.Instance.map;
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; private set; }
    public List<LocationStructure> allStructures { get; private set; }
    public List<LocationStructure> allSpecialStructures { get; private set; }
    public List<BaseSettlement> settlementsInRegion { get; private set; }


    //Components
    public RegionFeatureComponent regionFeatureComponent { get; }
    public BiomeDivisionComponent biomeDivisionComponent { get; }
    public GridTileFeatureComponent gridTileFeatureComponent { get; }
    /// <summary>
    /// Number of tile objects in this region categorized by type.
    /// NOTE: This isn't saved/loaded since this is updated every time a new tile object is placed.
    /// So loading this from a file would result in duplicates, since we still go through the process of
    /// placing tile objects when loading them.
    /// NOTE: This just includes BUILT objects!
    /// </summary>
    public Dictionary<TILE_OBJECT_TYPE, int> objectsInRegionCount { get; private set; }
    public LocationStructure wilderness { get; private set; }
    public List<VillageSpot> villageSpots { get; private set; }
    

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
        villageSpots = new List<VillageSpot>();
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
        gridTileFeatureComponent = new GridTileFeatureComponent();
        gridTileFeatureComponent.Initialize();
#if DEBUG_LOG
        Debug.Log($"Created region {this.name} with core tile {coreTile.ToString()}");
#endif
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
        gridTileFeatureComponent = data.gridTileFeatureComponent.Load();
    }

#region Loading
    public void LoadWilderness(Wilderness p_wilderness) {
        wilderness = p_wilderness;
    }
    public void LoadReferences(SaveDataRegion saveDataRegion) {
#if DEBUG_LOG
        string summary = $"Loading {name} references:";
        summary = $"{summary}\nLoading Residents:";
#endif
        for (int i = 0; i < saveDataRegion.residentIDs.Length; i++) {
            string residentID = saveDataRegion.residentIDs[i];
            Character resident = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(residentID);
            if (resident != null) {
                residents.Add(resident);
#if DEBUG_LOG
                summary = $"{summary}\n- {resident.name}";
#endif
            } else {
#if DEBUG_LOG
                Debug.LogWarning($"Trying to add resident at {name} with ID {residentID} but could not find character with that ID");
#endif
            }

        }
#if DEBUG_LOG
        summary = $"{summary}\nLoading characters at Location:";
#endif
        for (int i = 0; i < saveDataRegion.charactersAtLocationIDs.Length; i++) {
            string charactersAtLocationID = saveDataRegion.charactersAtLocationIDs[i];
            Character character = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(charactersAtLocationID);
            if (character != null) {
                charactersAtLocation.Add(character);
#if DEBUG_LOG
                summary = $"{summary}\n- {character.name}";
#endif
            } else {
#if DEBUG_LOG
                Debug.LogWarning($"Trying to add character at location {name} with ID {charactersAtLocationID} but could not find character with that ID");
#endif
            }
        }
        for (int i = 0; i < saveDataRegion.factionsHereIDs.Length; i++) {
            string factionHereID = saveDataRegion.factionsHereIDs[i];
            Faction faction = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(factionHereID);
            factionsHere.Add(faction);
        }
        //village spots
        for (int i = 0; i < saveDataRegion.villageSpots.Length; i++) {
            VillageSpot villageSpot = saveDataRegion.villageSpots[i].Load();
            villageSpots.Add(villageSpot);
        }
        gridTileFeatureComponent.LoadReferences(saveDataRegion.gridTileFeatureComponent);
#if DEBUG_LOG
        Debug.Log(summary);
#endif
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
    public Area GetAreaWithFeatureThatIsNearestTo(string featureName, Character p_character) {
        float nearestDistance = 0f;
        Area nearestArea = null;
        for (int i = 0; i < areas.Count; i++) {
            Area a = areas[i];
            if (a.featureComponent.HasFeature(featureName)) {
                float distance = Vector2.Distance(a.gridTileComponent.centerGridTile.centeredWorldLocation, p_character.worldPosition);
                if (nearestArea == null || distance < nearestDistance) {
                    nearestArea = a;
                    nearestDistance = distance;
                }
            }
        }
        return nearestArea;
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
    public Character GetRandomCharacterForSuccubusMakeLove(Character p_succubus) {
        List<Character> validCharacters = RuinarchListPool<Character>.Claim();
        Character chosenCharacter = null;
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character c = charactersAtLocation[i];
            if (c.gender == GENDER.MALE && !c.isDead && (p_succubus.tileObjectComponent.primaryBed != null || c.tileObjectComponent.primaryBed != null) && c.homeSettlement != null && !c.partyComponent.isActiveMember) {
                if (c.limiterComponent.canPerform && !c.combatComponent.isInCombat && !c.hasBeenRaisedFromDead && !c.carryComponent.masterCharacter.movementComponent.isTravellingInWorld && c.currentRegion == p_succubus.currentRegion) {
                    if (c.homeSettlement.GetFirstBuiltBedThatIsAvailableAndNoActiveUsers() != null) {
                        validCharacters.Add(c);
                    }
                }
            }
        }
        if (validCharacters != null) {
            chosenCharacter = UtilityScripts.CollectionUtilities.GetRandomElement(validCharacters);
        }
        RuinarchListPool<Character>.Release(validCharacters);
        return chosenCharacter;
    }
    public Character GetRandomCharacterForSuccubusMakeLoveTamed(Character p_succubus) {
        List<Character> validCharacters = RuinarchListPool<Character>.Claim();
        Character chosenCharacter = null;
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character c = charactersAtLocation[i];
            if (c.gender == GENDER.MALE && !c.isDead && (p_succubus.tileObjectComponent.primaryBed != null || c.tileObjectComponent.primaryBed != null) && c.homeSettlement != null && !c.partyComponent.isActiveMember && c.faction != p_succubus.faction && c.homeRegion == p_succubus.currentRegion) {
                if (c.limiterComponent.canPerform && !c.combatComponent.isInCombat && !c.hasBeenRaisedFromDead && !c.carryComponent.masterCharacter.movementComponent.isTravellingInWorld && c.currentRegion == p_succubus.currentRegion) {
                    if (c.homeSettlement.GetFirstBuiltBedThatIsAvailableAndNoActiveUsers() != null) {
                        validCharacters.Add(c);
                    }
                }
            }
        }
        if (validCharacters != null) {
            chosenCharacter = UtilityScripts.CollectionUtilities.GetRandomElement(validCharacters);
        }
        RuinarchListPool<Character>.Release(validCharacters);
        return chosenCharacter;
    }
    public Character GetRandomCharacterThatIsFemaleVillagerAndNotDead() {
        List<Character> validCharacters = RuinarchListPool<Character>.Claim();
        Character chosenCharacter = null;
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character c = charactersAtLocation[i];
            if (!c.isDead && c.isNormalCharacter && c.gender == GENDER.FEMALE) {
                validCharacters.Add(c);
            }
        }
        if (validCharacters != null) {
            chosenCharacter = UtilityScripts.CollectionUtilities.GetRandomElement(validCharacters);
        }
        RuinarchListPool<Character>.Release(validCharacters);
        return chosenCharacter;
    }
    public Character GetRandomCharacterThatIsMaleVillagerAndNotDeadAndFactionIsNotTheSameAs(Character p_character) {
        List<Character> validCharacters = RuinarchListPool<Character>.Claim();
        Character chosenCharacter = null;
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character c = charactersAtLocation[i];
            if (c.gender == GENDER.MALE && c.isNormalCharacter && !c.isDead && c.homeRegion == p_character.currentRegion && c.faction != p_character.faction) {
                validCharacters.Add(c);
            }
        }
        if (validCharacters != null) {
            chosenCharacter = UtilityScripts.CollectionUtilities.GetRandomElement(validCharacters);
        }
        RuinarchListPool<Character>.Release(validCharacters);
        return chosenCharacter;
    }
    public Character GetRandomCharacterThatIsNotDeadAndInDemonFactionAndHasPathTo(Character p_character) {
        List<Character> validCharacters = RuinarchListPool<Character>.Claim();
        Character chosenCharacter = null;
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character c = charactersAtLocation[i];
            if (c != p_character && !c.isDead && c.movementComponent.HasPathTo(p_character.gridTileLocation) && c.faction?.factionType.type == FACTION_TYPE.Demons) {
                validCharacters.Add(c);
            }
        }
        if (validCharacters != null) {
            chosenCharacter = UtilityScripts.CollectionUtilities.GetRandomElement(validCharacters);
        }
        RuinarchListPool<Character>.Release(validCharacters);
        return chosenCharacter;
    }
    public Character GetRandomCharacterForMonsterScent(Character p_character) {
        List<Character> validCharacters = RuinarchListPool<Character>.Claim();
        Character chosenCharacter = null;
        for (int i = 0; i < charactersAtLocation.Count; i++) {
            Character c = charactersAtLocation[i];
            if (!c.isDead
                && c.limiterComponent.canPerform
                && c.limiterComponent.canMove
                && c.movementComponent.HasPathTo(p_character.gridTileLocation)
                && !c.movementComponent.isStationary
                && (c is Summon)
                && !(c is Animal)
                && !c.isInLimbo
                && !CharacterManager.Instance.IsCharacterTheSameLycan(p_character, c)
                && !c.partyComponent.hasParty) {
                validCharacters.Add(c);
            }
        }
        if (validCharacters != null) {
            chosenCharacter = UtilityScripts.CollectionUtilities.GetRandomElement(validCharacters);
        }
        RuinarchListPool<Character>.Release(validCharacters);
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
        allSpecialStructures = new List<LocationStructure>();
    }
    public void GenerateStructures() {
        CreateStructureList();
        wilderness = LandmarkManager.Instance.CreateNewStructureAt(this, STRUCTURE_TYPE.WILDERNESS);
    }
    public void AddStructure(LocationStructure structure) {
        Debug.Assert(!structure.hasBeenDestroyed, $"Structure {structure} has been destroyed but is being added to {name}");
        if (!structures.ContainsKey(structure.structureType)) {
            structures.Add(structure.structureType, new List<LocationStructure>());
        }

        if (!structures[structure.structureType].Contains(structure)) {
            structures[structure.structureType].Add(structure);
            allStructures.Add(structure);
            if (structure.structureType.IsSpecialStructure()) {
                allSpecialStructures.Add(structure);
            }
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
                if (structure.structureType.IsSpecialStructure()) {
                    allSpecialStructures.Remove(structure);
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
    public LocationStructure GetRandomStructureThatIsInADungeonAndHasPassableTiles() {
        List<LocationStructure> structureChoices = RuinarchListPool<LocationStructure>.Claim();
        LocationStructure chosenStructure = null;
        for (int i = 0; i < allStructures.Count; i++) {
            LocationStructure currStructure = allStructures[i];
            if (currStructure.settlementLocation != null && currStructure.settlementLocation.locationType == LOCATION_TYPE.DUNGEON && currStructure.passableTiles.Count > 0) {
                structureChoices.Add(currStructure);
            }
        }
        if (structureChoices != null && structureChoices.Count > 0) {
            chosenStructure = structureChoices[UnityEngine.Random.Range(0, structureChoices.Count)];
        }
        RuinarchListPool<LocationStructure>.Release(structureChoices);
        return chosenStructure;
    }
    public LocationStructure GetRandomStructureThatIsInAnUnoccupiedDungeonAndHasPassableTiles() {
        List<LocationStructure> structureChoices = RuinarchListPool<LocationStructure>.Claim();
        LocationStructure chosenStructure = null;
        for (int i = 0; i < allStructures.Count; i++) {
            LocationStructure currStructure = allStructures[i];
            if (!currStructure.IsOccupied() && currStructure.settlementLocation != null && currStructure.settlementLocation.locationType == LOCATION_TYPE.DUNGEON && currStructure.passableTiles.Count > 0) {
                structureChoices.Add(currStructure);
            }
        }
        if (structureChoices != null && structureChoices.Count > 0) {
            chosenStructure = structureChoices[UnityEngine.Random.Range(0, structureChoices.Count)];
        }
        RuinarchListPool<LocationStructure>.Release(structureChoices);
        return chosenStructure;
    }
    public LocationStructure GetRandomStructureThatIsHabitableAndUnoccupiedButNot(LocationStructure p_exceptionStructure) {
        List<LocationStructure> structureChoices = RuinarchListPool<LocationStructure>.Claim();
        LocationStructure chosenStructure = null;
        for (int i = 0; i < allStructures.Count; i++) {
            LocationStructure currStructure = allStructures[i];
            if (!currStructure.IsOccupied() && currStructure.HasStructureTag(STRUCTURE_TAG.Shelter) && p_exceptionStructure != currStructure) {
                structureChoices.Add(currStructure);
            }
        }
        if (structureChoices != null && structureChoices.Count > 0) {
            chosenStructure = structureChoices[UnityEngine.Random.Range(0, structureChoices.Count)];
        }
        RuinarchListPool<LocationStructure>.Release(structureChoices);
        return chosenStructure;
    }
    public LocationStructure GetRandomStructureOfTypeThatHasTombstone(STRUCTURE_TYPE type) {
        List<LocationStructure> structureChoices = RuinarchListPool<LocationStructure>.Claim();
        LocationStructure chosenStructure = null;
        if (structures.ContainsKey(type)) {
            List<LocationStructure> structuresOfType = structures[type];
            for (int j = 0; j < structuresOfType.Count; j++) {
                LocationStructure possibleStructure = structuresOfType[j];
                if (possibleStructure.HasTileObjectOfType(TILE_OBJECT_TYPE.TOMBSTONE)) {
                    structureChoices.Add(possibleStructure);
                }
            }
        }
        if (structureChoices != null && structureChoices.Count > 0) {
            chosenStructure = structureChoices[UnityEngine.Random.Range(0, structureChoices.Count)];
        }
        RuinarchListPool<LocationStructure>.Release(structureChoices);
        return chosenStructure;
    }
    public LocationStructure GetRandomSpecialStructure() {
        List<LocationStructure> specialStructures = RuinarchListPool<LocationStructure>.Claim();
        LocationStructure chosenStructure = null;
        for (int i = 0; i < allSpecialStructures.Count; i++) {
            LocationStructure currStructure = allSpecialStructures[i];
            if (currStructure.passableTiles.Count > 0) {
                specialStructures.Add(currStructure);
            }
        }
        if (specialStructures != null && specialStructures.Count > 0) {
            chosenStructure = specialStructures[UnityEngine.Random.Range(0, specialStructures.Count)];
        }
        RuinarchListPool<LocationStructure>.Release(specialStructures);
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
    public List<LocationStructure> GetStructuresAtLocation(STRUCTURE_TYPE type) {
        List<LocationStructure> structuresAtLocation = null;
        if (structures.ContainsKey(type)) {
            structuresAtLocation = structures[type];
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
    public void PopulateTileObjectsOfType(List<TileObject> p_tileObjects, TILE_OBJECT_TYPE type) {
        for (int i = 0; i < allStructures.Count; i++) {
            List<TileObject> tileObjects = allStructures[i].GetTileObjectsOfType(type);
            if (tileObjects != null && tileObjects.Count > 0) {
                p_tileObjects.AddRange(tileObjects);
            }
        }
    }
    public bool HasTileObjectOfType(TILE_OBJECT_TYPE type) {
        for (int i = 0; i < allStructures.Count; i++) {
            LocationStructure structure = allStructures[i];
            if (structure.HasTileObjectOfType(type)) {
                return true;
            }
        }
        return false;
    }
    public void PopulateTileObjectsOfType<T>(List<TileObject> objs) where T : TileObject{
        for (int i = 0; i < allStructures.Count; i++) {
            allStructures[i].PopulateTileObjectsOfType<T>(objs);
        }
    }
    public void LinkAllUnlinkedSpecialStructures() {
        for (int i = 0; i < allSpecialStructures.Count; i++) {
            LocationStructure s = allSpecialStructures[i];
            if (s.linkedSettlement == null) {
                s.LinkThisStructureToAVillage();
            }
        }
    }
#endregion

#region Areas
    public Area GetRandomAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage() {
        List<Area> pool = RuinarchListPool<Area>.Claim();
        Area chosenArea = null;
        for (int i = 0; i < areas.Count; i++) {
            Area a = areas[i];
            if (a.elevationType != ELEVATION.WATER 
                && a.elevationType != ELEVATION.MOUNTAIN 
                && !a.structureComponent.HasStructureInArea() 
                && !a.IsNextToOrPartOfVillage() 
                && !a.gridTileComponent.HasCorruption()) {
                pool.Add(a);
            }
        }
        if (pool != null && pool.Count > 0) {
            chosenArea = CollectionUtilities.GetRandomElement(pool);
        }
        RuinarchListPool<Area>.Release(pool);
        return chosenArea;
    }
    public Area GetRandomAreaThatIsNotMountainWaterAndNoStructureAndNoCorruption() {
        List<Area> pool = RuinarchListPool<Area>.Claim();
        Area chosenArea = null;
        for (int i = 0; i < areas.Count; i++) {
            Area a = areas[i];
            if (a.elevationType != ELEVATION.WATER 
                && a.elevationType != ELEVATION.MOUNTAIN 
                && !a.structureComponent.HasStructureInArea() 
                && !a.gridTileComponent.HasCorruption()) {
                pool.Add(a);
            }
        }
        if (pool != null && pool.Count > 0) {
            chosenArea = CollectionUtilities.GetRandomElement(pool);
        }
        RuinarchListPool<Area>.Release(pool);
        return chosenArea;
    }
    public Area GetRandomAreaThatIsNotMountainWaterAndNoCorruption() {
        List<Area> pool = RuinarchListPool<Area>.Claim();
        Area chosenArea = null;
        for (int i = 0; i < areas.Count; i++) {
            Area a = areas[i];
            if (a.elevationType != ELEVATION.WATER 
                && a.elevationType != ELEVATION.MOUNTAIN 
                && !a.gridTileComponent.HasCorruption()) {
                pool.Add(a);
            }
        }
        if (pool != null && pool.Count > 0) {
            chosenArea = CollectionUtilities.GetRandomElement(pool);
        }
        RuinarchListPool<Area>.Release(pool);
        return chosenArea;
    }
    public Area GetRandomAreaThatIsUncorruptedFullyPlainNoStructureAndNotNextToOrPartOfVillage() {
        List<Area> pool = RuinarchListPool<Area>.Claim();
        Area chosenArea = null;
        for (int i = 0; i < areas.Count; i++) {
            Area a = areas[i];
            if (a.elevationComponent.IsFully(ELEVATION.PLAIN) 
                && !a.structureComponent.HasStructureInArea() 
                && !a.IsNextToOrPartOfVillage() 
                && !a.gridTileComponent.HasCorruption()) {
                pool.Add(a);
            }
        }
        if (pool != null && pool.Count > 0) {
            chosenArea = CollectionUtilities.GetRandomElement(pool);
        }
        RuinarchListPool<Area>.Release(pool);
        return chosenArea;
    }
    public Area GetRandomAreaThatIsNotMountainAndWaterAndNoSettlement() {
        List<Area> pool = RuinarchListPool<Area>.Claim();
        Area chosenArea = null;
        for (int i = 0; i < areas.Count; i++) {
            Area a = areas[i];
            if (a.settlementOnArea == null && a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN) {
                pool.Add(a);
            }
        }
        if (pool != null && pool.Count > 0) {
            chosenArea = CollectionUtilities.GetRandomElement(pool);
        }
        RuinarchListPool<Area>.Release(pool);
        return chosenArea;
    }
    public Area GetRandomAreaThatIsNotWater() {
        List<Area> pool = RuinarchListPool<Area>.Claim();
        Area chosenArea = null;
        for (int i = 0; i < areas.Count; i++) {
            Area a = areas[i];
            if (a.elevationType != ELEVATION.WATER) {
                pool.Add(a);
            }
        }
        if (pool != null && pool.Count > 0) {
            chosenArea = CollectionUtilities.GetRandomElement(pool);
        }
        RuinarchListPool<Area>.Release(pool);
        return chosenArea;
    }
    public Area GetRandomAreaThatIsNextToAVillageButNotMountainAndWaterAndNoSettlementAndWithPathTo(Character p_character) {
        List<Area> pool = RuinarchListPool<Area>.Claim();
        Area chosenArea = null;
        for (int i = 0; i < areas.Count; i++) {
            Area a = areas[i];
            if (a.elevationType != ELEVATION.MOUNTAIN
                && a.elevationType != ELEVATION.WATER 
                && a.neighbourComponent.IsNextToVillage() 
                && a.settlementOnArea == null
                && p_character.movementComponent.HasPathTo(a)) {
                pool.Add(a);
            }
        }
        if (pool != null && pool.Count > 0) {
            chosenArea = CollectionUtilities.GetRandomElement(pool);
        }
        RuinarchListPool<Area>.Release(pool);
        return chosenArea;
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
    public void PopulateSettlementsInRegionForInvadeBehaviour(List<BaseSettlement> settlements) {
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement settlement = settlementsInRegion[i];
            if (settlement.HasResidentForInvadeBehaviour()) {
                settlements.Add(settlement);
            }
        }
    }
    public void PopulateSettlementsInRegionForPestBehaviour(List<BaseSettlement> settlements, Character p_character) {
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement settlement = settlementsInRegion[i];
            if (settlement.HasResidentThatIsNotDead(p_character)) {
                settlements.Add(settlement);
            }
        }
    }
    public void PopulateSettlementsInRegionForGettingGeneralVillageTargets(List<BaseSettlement> settlements) {
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement settlement = settlementsInRegion[i];
            if (settlement.HasResidentForGettingGeneralVillageTargets()) {//settlement.residents.Count > 0 && settlement.residents.Count(c => c != null && c.isNormalCharacter && !c.isAlliedWithPlayer && c.IsAble()) > 0
                settlements.Add(settlement);
            }
        }
    }
    public void PopulateSettlementsInRegionThatHasAliveResidentExcept(List<BaseSettlement> settlements, Character exception, BaseSettlement exceptionSettlement) {
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement settlement = settlementsInRegion[i];
            if(settlement != exceptionSettlement) {
                if (settlement.HasResidentThatIsNotDead(exception)) {
                    settlements.Add(settlement);
                }
            }
        }
    }
    public void PopulateVillagesInRegionThatAreOwnedByFactionOrNotHostileToIt(List<NPCSettlement> settlements, Faction p_faction) {
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement settlement = settlementsInRegion[i];
            if (settlement.owner != null && settlement.locationType == LOCATION_TYPE.VILLAGE && settlement is NPCSettlement npcSettlement) {
                if (settlement.owner == p_faction || !settlement.owner.IsHostileWith(p_faction)) {
                    settlements.Add(npcSettlement);
                }    
            }
        }
    }
    public BaseSettlement GetFirstSettlementInRegionThatIsAUnoccupiedOrFactionlessResidentVillageThatIsNotHomeOf(Character p_character) {
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement s = settlementsInRegion[i];
            if (s.locationType == LOCATION_TYPE.VILLAGE && p_character.previousCharacterDataComponent.previousHomeSettlement != s && p_character.homeSettlement != s && (!s.HasResidents() || s.AreAllResidentsVagrantOrFactionless())) {
                return s;
            }
        }
        return null;
    }
    public BaseSettlement GetFirstSettlementInRegionThatIsAUnoccupiedVillageThatIsNotPreviousHomeOf(Character p_character) {
        for (int i = 0; i < settlementsInRegion.Count; i++) {
            BaseSettlement s = settlementsInRegion[i];
            if (s.locationType == LOCATION_TYPE.VILLAGE && !s.HasResidents() && s != p_character.previousCharacterDataComponent.previousHomeSettlement) {
                return s;
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
        return count >= villageSpots.Count;
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

#region Village Spots
    public void SetVillageSpots(List<VillageSpot> p_villageSpots) {
        villageSpots.Clear();
        villageSpots.AddRange(p_villageSpots);
    }
    public VillageSpot GetFirstUnoccupiedVillageSpot() {
        for (int i = 0; i < villageSpots.Count; i++) {
            VillageSpot villageSpot = villageSpots[i];
            if (!villageSpot.mainSpot.structureComponent.HasStructureInArea() && !villageSpot.mainSpot.IsNextToOrPartOfVillage() && !villageSpot.mainSpot.gridTileComponent.HasCorruption()) {
                return villageSpot;
            }
        }
        return null;
    }
    public VillageSpot GetFirstUnoccupiedVillageSpotThatCanAccomodateFaction(FACTION_TYPE p_factionType) {
        for (int i = 0; i < villageSpots.Count; i++) {
            VillageSpot villageSpot = villageSpots[i];
            if (!villageSpot.mainSpot.structureComponent.HasStructureInArea() && !villageSpot.mainSpot.IsNextToOrPartOfVillage() && 
                !villageSpot.mainSpot.gridTileComponent.HasCorruption() && villageSpot.CanAccommodateFaction(p_factionType)) {
                return villageSpot;
            }
        }
        return null;
    }
    public VillageSpot GetVillageSpotOnArea(Area p_area) {
        for (int i = 0; i < villageSpots.Count; i++) {
            VillageSpot villageSpot = villageSpots[i];
            if (villageSpot.mainSpot == p_area) {
                return villageSpot;
            }
        }
        return null;
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
        villageSpots?.Clear();
        villageSpots = null;
    }
    
}