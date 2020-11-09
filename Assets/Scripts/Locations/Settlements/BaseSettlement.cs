using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using Traits;
using UnityEngine;
using UtilityScripts;
using Logs;

namespace Locations.Settlements {
    public abstract class BaseSettlement : IPartyQuestTarget, IPartyTargetDestination, IGatheringTarget, ISavable, ILogFiller {
        public string persistentID { get; private set; }
        public int id { get; }
        public LOCATION_TYPE locationType { get; private set; }
        public string name { get; private set; }
        public Faction owner { get; private set; }
        public List<HexTile> tiles { get; }
        public List<Character> residents { get; protected set; }
        public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; protected set; }
        public List<IPointOfInterest> firesInSettlement { get; }
        public List<LocationStructure> allStructures { get; protected set; }
        public List<Party> parties { get; protected set; }
        
        #region getters
        public OBJECT_TYPE objectType => OBJECT_TYPE.Settlement;
        public virtual Type serializedData => typeof(SaveDataBaseSettlement);
        public virtual Region region => null;
        public LocationStructure currentStructure => null;
        public BaseSettlement currentSettlement => this;
        public bool hasBeenDestroyed => false;
        public PARTY_TARGET_DESTINATION_TYPE partyTargetDestinationType => PARTY_TARGET_DESTINATION_TYPE.Settlement;
        #endregion

        protected BaseSettlement(LOCATION_TYPE locationType) {
            persistentID = UtilityScripts.Utilities.GetNewUniqueID();
            id = UtilityScripts.Utilities.SetID(this);
            SetName(RandomNameGenerator.GenerateCityName(RACE.HUMANS));
            tiles = new List<HexTile>();
            residents = new List<Character>();
            structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
            firesInSettlement = new List<IPointOfInterest>();
            allStructures = new List<LocationStructure>();
            parties = new List<Party>();
            SetLocationType(locationType);
            StartListeningForFires();
        }
        protected BaseSettlement(SaveDataBaseSettlement saveDataBaseSettlement) {
            persistentID = saveDataBaseSettlement._persistentID;
            SetName(saveDataBaseSettlement.name);
            id = UtilityScripts.Utilities.SetID(this, saveDataBaseSettlement.id);
            tiles = new List<HexTile>();
            residents = new List<Character>();
            structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
            firesInSettlement = new List<IPointOfInterest>();
            allStructures = new List<LocationStructure>();
            parties = new List<Party>();
            SetLocationType(saveDataBaseSettlement.locationType);
            StartListeningForFires();
        }

        #region Settlement Info
        private void SetLocationType(LOCATION_TYPE locationType) {
            this.locationType = locationType;
        }
        public void SetName(string name) {
            this.name = name;
        }
        #endregion
        
        #region Characters
        public virtual bool AddResident(Character character, LocationStructure chosenHome = null, bool ignoreCapacity = true) {
            if (!residents.Contains(character)) {
                if (!ignoreCapacity) {
                    if (IsResidentsFull()) {
                        Debug.LogWarning(
                            $"{GameManager.Instance.TodayLogString()}Cannot add {character.name} as resident of {name} because residency is already full!");
                        return false; //npcSettlement is at capacity
                    }
                }
                if (!CanCharacterBeAddedAsResidentBasedOnFaction(character)) {
                    character.logComponent.PrintLogIfActive(
                        $"{character.name} tried to become a resident of {name} but their factions conflicted");
                    return false;
                }
                //region.AddResident(character);
                residents.Add(character);
                AssignCharacterToDwellingInArea(character, chosenHome);
                if(owner == null && character.faction != null && character.faction.isMajorNonPlayer) {
                    //If a character becomes a resident and he/she has a faction and this settlement has no faction owner yet, set it as the faction owner
                    LandmarkManager.Instance.OwnSettlement(character.faction, this);
                }
                return true;
            }
            return false;
        }
        public virtual bool RemoveResident(Character character) {
            if (residents.Remove(character)) {
                //regio.RemoveResident(character);
                if (character.homeStructure != null && character.homeSettlement == this) {
                    character.ChangeHomeStructure(null);
                }
                if(residents.Count <= 0 && owner != null) {
                    //if all residents of a settlement is removed, then remove faction owner
                    LandmarkManager.Instance.UnownSettlement(this);
                }
                return true;
            }
            return false;
        }
        public virtual void AssignCharacterToDwellingInArea(Character character, LocationStructure dwellingOverride = null) {
            if (structures == null) {
                Debug.LogWarning($"{name} doesn't have any dwellings for {character.name} because structures have not been generated yet");
                return;
            }
            //Note: Removed this because, even if there are no dwellings left, home structure should be set to city center
            // if (!character.isFactionless && !structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
            //     Debug.LogWarning($"{name} doesn't have any dwellings for {character.name}");
            //     return;
            // }
            // if (character.isFactionless) {
            //     character.SetHomeStructure(null);
            //     return;
            // }
            LocationStructure chosenDwelling = dwellingOverride;
            if (chosenDwelling == null) {
                Character lover = CharacterManager.Instance.GetCharacterByID(character.relationshipContainer
                    .GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
                if (lover != null && lover.faction.id == character.faction.id && residents.Contains(lover) && lover.homeStructure.tiles.Count > 0) { //check if the character has a lover that lives in the npcSettlement
                    chosenDwelling = lover.homeStructure;
                }
                if (chosenDwelling == null && structures.ContainsKey(STRUCTURE_TYPE.DWELLING) && (character.homeStructure == null || character.homeStructure.region.id != id)) { //else, find an unoccupied dwelling (also check if the character doesn't already live in this npcSettlement)
                    List<LocationStructure> structureList = structures[STRUCTURE_TYPE.DWELLING];
                    for (int i = 0; i < structureList.Count; i++) {
                        LocationStructure currDwelling = structureList[i];
                        if (currDwelling.CanBeResidentHere(character)) {
                            chosenDwelling = currDwelling;
                            break;
                        }
                    }
                }
            }

            if (chosenDwelling == null) {
                //if the code reaches here, it means that the npcSettlement could not find a dwelling for the character
                Debug.LogWarning(
                    $"{GameManager.Instance.TodayLogString()}Could not find a dwelling for {character.name} at {name}, setting home to Town Center");
                chosenDwelling = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER) as CityCenter;
            }
            character.ChangeHomeStructure(chosenDwelling);
        }
        private bool CanCharacterBeAddedAsResidentBasedOnFaction(Character character) {
            if(character.isVagrantOrFactionless || character.isFactionless || (character.faction != null && !character.faction.isMajorFaction)) {
                if(owner == null) {
                    return true;
                }
            } else if (character.faction.isPlayerFaction && owner != null && owner.isPlayerFaction) {
                return true;
            } else if (character.faction != null && character.faction == owner) {
                return true;
            }
            //if (owner != null && character.faction != null) {
            //    //If character's faction is hostile with region's ruling faction, character cannot be a resident
            //    return !owner.IsHostileWith(character.faction);
            //}
            //if (owner != null && character.faction == null) {
            //    //If character has no faction and region has faction, character cannot be a resident
            //    return false;
            //}
            return true;
        }
        protected virtual bool IsResidentsFull() {
            if (structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
                List<LocationStructure> dwellings = structures[STRUCTURE_TYPE.DWELLING];
                for (int i = 0; i < dwellings.Count; i++) {
                    if (!dwellings[i].IsOccupied()) {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool HasResidentInsideSettlement() {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (resident.gridTileLocation != null
                    && !resident.isBeingSeized
                    && resident.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                    && resident.IsInHomeSettlement()) {
                    return true;
                }
            }
            return false;
        }
        public bool HasAliveResidentInsideSettlement() {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (!resident.isDead
                    && !resident.isBeingSeized
                    && resident.gridTileLocation != null
                    && resident.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                    && resident.gridTileLocation.IsPartOfSettlement(this)) {
                    return true;
                }
            }
            return false;
        }
        public bool HasAliveVillagerResident() {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (!resident.isDead && resident.isNormalCharacter) {
                    return true;
                }
            }
            return false;
        }
        public Character GetRandomAliveResidentInsideSettlement() {
            List<Character> choices = null;
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (!resident.isDead
                    && !resident.isBeingSeized
                    && resident.gridTileLocation != null
                    && resident.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                    && resident.gridTileLocation.IsPartOfSettlement(this)) {
                    if(choices == null) { choices = new List<Character>(); }
                    choices.Add(resident);
                }
            }
            if(choices != null && choices.Count > 0) {
                return choices[UnityEngine.Random.Range(0, choices.Count)];
            }
            return null;
        }
        public Character GetRandomCharacterThatMeetCriteria(System.Func<Character, bool> criteria) {
            Character chosenCharacter = null;
            for (int i = 0; i < allStructures.Count; i++) {
                chosenCharacter = allStructures[i].GetRandomCharacterThatMeetCriteria(criteria);
                if(chosenCharacter != null) {
                    return chosenCharacter;
                }
            }
            return null;
        }
        public int GetNumOfResidentsThatMeetCriteria(System.Func<Character, bool> criteria) {
            int count = 0;
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (criteria.Invoke(resident)) {
                    count++;
                }
            }
            return count;
        }
        public bool HasResidentThatMeetsCriteria(System.Func<Character, bool> criteria) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (criteria.Invoke(resident)) {
                    return true;
                }
            }
            return false;
        }
        public bool HasAliveResidentInsideSettlementThatIsHostileWith(Faction faction) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (!resident.isDead
                    && !resident.isBeingSeized
                    && resident.gridTileLocation != null
                    && resident.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                    && resident.gridTileLocation.IsPartOfSettlement(this)
                    && (resident.faction == null || faction == null || faction.IsHostileWith(resident.faction))) {
                    return true;
                }
            }
            return false;
        }
        public bool HasAliveResidentThatIsHostileWith(Faction faction) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (!resident.isDead
                    && (resident.faction == null || faction == null || faction.IsHostileWith(resident.faction))) {
                    return true;
                }
            }
            return false;
        }
        public Character GetRandomAliveResidentInsideSettlementThatIsHostileWith(Character character) {
            List<Character> choices = null;
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (character != resident
                    && !resident.isBeingSeized
                    && !resident.isDead
                    && resident.gridTileLocation != null
                    && resident.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                    && resident.gridTileLocation.IsPartOfSettlement(this)
                    && (resident.faction == null || character.faction == null || character.faction.IsHostileWith(resident.faction))) {
                    if (choices == null) { choices = new List<Character>(); }
                    choices.Add(resident);
                }
            }
            if (choices != null && choices.Count > 0) {
                return choices[UnityEngine.Random.Range(0, choices.Count)];
            }
            return null;
        }
        public bool AreAllResidentsVagrantOrFactionless() {
            for (int i = 0; i < residents.Count; i++) {
                if (!residents[i].isVagrantOrFactionless) {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Faction
        public virtual void SetOwner(Faction owner) {
            this.owner = owner;
        
            bool isCorrupted = this.owner != null && this.owner.isPlayerFaction;
            for (int i = 0; i < tiles.Count; i++) {
                HexTile tile = tiles[i];
                tile.SetCorruption(isCorrupted);
                if (tile.landmarkOnTile != null) {
                    tile.UpdateLandmarkVisuals();
                }
            }
        }
        #endregion

        #region Structures
        public void GenerateStructures(params LocationStructure[] preCreatedStructures) {
            for (int i = 0; i < preCreatedStructures.Length; i++) {
                LocationStructure structure = preCreatedStructures[i];
                AddStructure(structure);
            }
        }
        public void AddStructure(LocationStructure structure) {
            Debug.Assert(!structure.hasBeenDestroyed, $"Structure {structure} has been destroyed but is being added to {name}");
            if (!structures.ContainsKey(structure.structureType)) {
                structures.Add(structure.structureType, new List<LocationStructure>());
            }
            if (!structures[structure.structureType].Contains(structure)) {
                structures[structure.structureType].Add(structure);
                allStructures.Add(structure);
                structure.SetSettlementLocation(this);
                OnStructureAdded(structure);
            }
        }
        public void RemoveStructure(LocationStructure structure) {
            if (structures.ContainsKey(structure.structureType)) {
                if (structures[structure.structureType].Remove(structure)) {
                    allStructures.Remove(structure);
                    if (structures[structure.structureType].Count == 0) { //this is only for optimization
                        structures.Remove(structure.structureType);
                    }
                    OnStructureRemoved(structure);
                }
            }
        }
        protected virtual void OnStructureAdded(LocationStructure structure) { }
        protected virtual void OnStructureRemoved(LocationStructure structure) { }
        public LocationStructure GetRandomStructureOfType(STRUCTURE_TYPE type) {
            if (HasStructure(type)) {
                return structures[type][UtilityScripts.Utilities.Rng.Next(0, structures[type].Count)];
            }
            return null;
        }
        public LocationStructure GetRandomStructure() {
            return CollectionUtilities.GetRandomElement(allStructures);
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
        public List<LocationStructure> GetStructuresOfType(STRUCTURE_TYPE structureType) {
            if (HasStructure(structureType)) {
                return structures[structureType];
            }
            return null;
        }
        public LocationStructure GetFirstStructureOfTypeWithNoActiveSocialParty(STRUCTURE_TYPE type) {
            if (HasStructure(type)) {
                List<LocationStructure> structuresOfType = structures[type];
                for (int i = 0; i < structuresOfType.Count; i++) {
                    if (!structuresOfType[i].hasActiveSocialGathering) {
                        return structuresOfType[i];
                    }
                }
            }
            return null;
        }
        public LocationStructure GetFirstStructureOfType(STRUCTURE_TYPE type) {
            if (HasStructure(type)) {
                List<LocationStructure> structuresOfType = structures[type];
                if(structuresOfType != null && structuresOfType.Count > 0) {
                    return structuresOfType[0];
                }
            }
            return null;
        }
        public LocationStructure GetFirstUnoccupiedStructureOfType(STRUCTURE_TYPE type) {
            if (HasStructure(type)) {
                List<LocationStructure> structuresOfType = structures[type];
                for (int i = 0; i < structuresOfType.Count; i++) {
                    LocationStructure structure = structuresOfType[i];
                    if (structure.residents.Count == 0) {
                        return structure;
                    }
                }
            }
            return null;
        }
        public bool HasStructure(STRUCTURE_TYPE type) {
            return structures.ContainsKey(type);
        }
        public LocationStructure GetRandomStructure(System.Func<LocationStructure, bool> criteria) {
            List<LocationStructure> choices = new List<LocationStructure>();
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure structure = allStructures[i];
                if (criteria.Invoke(structure)) {
                    choices.Add(structure);
                }
            }
            if (choices.Count > 0) {
                return CollectionUtilities.GetRandomElement(choices);
            }
            return null;
        }
        public List<StructureConnector> GetAvailableStructureConnectors() {
            List<StructureConnector> connectors = new List<StructureConnector>();
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure structure = allStructures[i];
                if (structure is ManMadeStructure manMadeStructure && manMadeStructure.structureObj != null) {
                    for (int j = 0; j < manMadeStructure.structureObj.connectors.Length; j++) {
                        StructureConnector connector = manMadeStructure.structureObj.connectors[j];
                        if (connector.isOpen) {
                            connectors.Add(connector);    
                        }
                    }
                }
            }
            return connectors;
        }
        public int GetStructureCount(STRUCTURE_TYPE structureType) {
            if (HasStructure(structureType)) {
                return structures[structureType].Count;
            }
            return 0;
        }
        public int GetFacilityCount() {
            int count = 0;
            foreach (var kvp in structures) {
                if (kvp.Key.IsFacilityStructure()) {
                    count += kvp.Value.Count;
                }
            }
            return count;
        }
        #endregion

        #region Tiles
        public void AddTileToSettlement(HexTile tile) {
            if (tile.settlementOnTile != null) {
                return;
            }
            if (tiles.Contains(tile) == false) {
                tiles.Add(tile);
                tile.SetSettlementOnTile(this);
                if (locationType == LOCATION_TYPE.DEMONIC_INTRUSION) {
                    tile.SetCorruption(true);
                }
                if (tile.landmarkOnTile != null) {
                    tile.UpdateLandmarkVisuals();    
                }
            }
        }
        public void AddTileToSettlement(params HexTile[] tiles) {
            for (int i = 0; i < tiles.Length; i++) {
                HexTile tile = tiles[i];
                AddTileToSettlement(tile);
            }
        }
        public void RemoveTileFromSettlement(HexTile tile) {
            if (tiles.Remove(tile)) {
                tile.SetSettlementOnTile(null);
                if (locationType == LOCATION_TYPE.DEMONIC_INTRUSION) {
                    tile.SetCorruption(false);
                }
                if (tiles.Count <= 0) {
                    //when a settlement loses all its tiles consider it as wiped out
                    SettlementWipedOut();
                }
            }
        }
        public bool HasTileInRegion(Region region) {
            for (int i = 0; i < tiles.Count; i++) {
                HexTile tile = tiles[i];
                if (tile.region == region) {
                    return true;
                }
            }
            return false;
        }
        public HexTile GetRandomUnoccupiedHexTile() {
            List<HexTile> choices = new List<HexTile>();
            for (int i = 0; i < tiles.Count; i++) {
                HexTile tile = tiles[i];
                if (tile.innerMapHexTile.isOccupied == false) {
                    choices.Add(tile);
                }
            }
            return UtilityScripts.CollectionUtilities.GetRandomElement(choices);
        }
        public HexTile GetFirstUnoccupiedHexTile() {
            List<HexTile> choices = new List<HexTile>();
            for (int i = 0; i < tiles.Count; i++) {
                HexTile tile = tiles[i];
                if (tile.innerMapHexTile.isOccupied == false) {
                    return tile;
                }
            }
            return null;
        }
        public HexTile GetRandomHexTile() {
            return tiles[UnityEngine.Random.Range(0, tiles.Count)];
        }
        public LocationGridTile GetFirstPassableGridTileInSettlementThatMeetCriteria(System.Func<LocationGridTile, bool> validityChecker) {
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure structure = allStructures[i];
                for (int j = 0; j < structure.passableTiles.Count; j++) {
                    LocationGridTile locationGridTile = structure.passableTiles[j];
                    if (validityChecker.Invoke(locationGridTile)) {
                        return locationGridTile;
                    }
                }
            }
            return null;
        }
        public LocationGridTile GetRandomPassableGridTileInSettlementThatMeetCriteria(System.Func<LocationGridTile, bool> validityChecker) {
            List<LocationGridTile> locationGridTiles = null;
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure structure = allStructures[i];
                for (int j = 0; j < structure.passableTiles.Count; j++) {
                    LocationGridTile locationGridTile = structure.passableTiles[j];
                    if (validityChecker.Invoke(locationGridTile)) {
                        if(locationGridTiles == null) { locationGridTiles = new List<LocationGridTile>(); }
                        locationGridTiles.Add(locationGridTile);
                    }
                }
            }
            if(locationGridTiles != null && locationGridTiles.Count > 0) {
                return locationGridTiles[UnityEngine.Random.Range(0, locationGridTiles.Count)];
            }
            return null;
        }
        private List<LocationGridTile> GetLocationGridTilesInSettlement(System.Func<LocationGridTile, bool> validityChecker) {
            List<LocationGridTile> locationGridTiles = new List<LocationGridTile>();
            for (int i = 0; i < tiles.Count; i++) {
                HexTile tile = tiles[i];
                for (int j = 0; j < tile.locationGridTiles.Count; j++) {
                    LocationGridTile locationGridTile = tile.locationGridTiles[j];
                    if (validityChecker.Invoke(locationGridTile)) {
                        locationGridTiles.Add(locationGridTile);
                    }
                }
            }
            return locationGridTiles;
        }
        public HexTile GetAPlainAdjacentHextile() {
            List<HexTile> choices = null;
            for (int i = 0; i < tiles.Count; i++) {
                HexTile hex = tiles[i];
                for (int j = 0; j < hex.AllNeighbours.Count; j++) {
                    HexTile neighbour = hex.AllNeighbours[j];
                    if (neighbour.region != hex.region) {
                        continue; //skip tiles that are not part of the region if settlement is an NPC Settlement 
                    }
                    if (neighbour.elevationType != ELEVATION.MOUNTAIN && neighbour.elevationType != ELEVATION.WATER && neighbour.settlementOnTile == null) {
                        if (!tiles.Contains(neighbour)) {
                            if(choices == null) { choices = new List<HexTile>(); }
                            choices.Add(neighbour);
                        }
                    }
                }
            }
            if(choices != null && choices.Count > 0) {
                return choices[UnityEngine.Random.Range(0, choices.Count)];
            }
            return null;
        }
        public HexTile GetAPlainAdjacentHextileThatMeetCriteria(System.Func<HexTile, bool> checker) {
            List<HexTile> choices = null;
            for (int i = 0; i < tiles.Count; i++) {
                HexTile hex = tiles[i];
                for (int j = 0; j < hex.AllNeighbours.Count; j++) {
                    HexTile neighbour = hex.AllNeighbours[j];
                    if (neighbour.region != hex.region) {
                        continue; //skip tiles that are not part of the region if settlement is an NPC Settlement 
                    }
                    if (neighbour.elevationType != ELEVATION.MOUNTAIN && neighbour.elevationType != ELEVATION.WATER && neighbour.settlementOnTile == null) {
                        if (!tiles.Contains(neighbour)) {
                            if (checker.Invoke(neighbour)) {
                                if (choices == null) { choices = new List<HexTile>(); }
                                choices.Add(neighbour);
                            }
                        }
                    }
                }
            }
            if (choices != null && choices.Count > 0) {
                return choices[UnityEngine.Random.Range(0, choices.Count)];
            }
            return null;
        }
        #endregion

        #region Fire
        private void StartListeningForFires() {
            Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
            Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
        }
        private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
            //added checker for null so that if an object has been destroyed and lost the burning trait, it will still be removed from the list
            if (trait is Burning && (traitable.gridTileLocation == null || traitable.gridTileLocation.IsPartOfSettlement(this))) {
                RemoveObjectOnFire(traitable);
            }
        }
        private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
            if (trait is Burning && traitable.gridTileLocation != null && traitable.gridTileLocation.IsPartOfSettlement(this)) {
                AddObjectOnFire(traitable);
            }
        }
        private void AddObjectOnFire(ITraitable traitable) {
            if (traitable is IPointOfInterest fire && firesInSettlement.Contains(fire) == false) {
                firesInSettlement.Add(fire);
            }
        }
        private void RemoveObjectOnFire(ITraitable traitable) {
            if (traitable is IPointOfInterest poi) {
                firesInSettlement.Remove(poi);    
            }
            
        }
        #endregion

        #region Utilities
        protected virtual void SettlementWipedOut() { }
        public bool HasPathTowardsTileInSettlement(Character character, int tileCount) {
            List<LocationGridTile> locationGridTilesInSettlement = GetLocationGridTilesInSettlement(tile => tile.isOccupied == false);
            if (locationGridTilesInSettlement.Count > 0) {
                for (int i = 0; i < tileCount; i++) {
                    if (locationGridTilesInSettlement.Count == 0) {
                        //no more unoccupied tiles, but other tiles passed, return true
                        return true;
                    }
                    LocationGridTile randomTile = CollectionUtilities.GetRandomElement(locationGridTilesInSettlement);
                    if (character.movementComponent.HasPathToEvenIfDiffRegion(randomTile) == false) {
                        //no path towards random unoccupied tile in settlement, return false
                        return false;
                    }
                    locationGridTilesInSettlement.Remove(randomTile);
                }    
            }
            //default to true even if there are no unoccupied tiles in settlement 
            return true;
        }
        public List<HexTile> GetSurroundingAreas() {
            List<HexTile> areas = new List<HexTile>();
            for (int i = 0; i < tiles.Count; i++) {
                HexTile tile = tiles[i];
                if (this is NPCSettlement npcSettlement && tile.region != npcSettlement.region) {
                    continue; //skip tiles that are not part of the region if settlement is an NPC Settlement 
                }
                for (int j = 0; j < tile.AllNeighbours.Count; j++) {
                    HexTile neighbour = tile.AllNeighbours[j];
                    if (neighbour.settlementOnTile == null || neighbour.settlementOnTile != this) {
                        areas.Add(neighbour);
                    }
                }
            }
            return areas;
        }
        #endregion

        #region Tile Object
        public bool HasTileObjectOfType(TILE_OBJECT_TYPE type) {
            for (int i = 0; i < allStructures.Count; i++) {
                if (allStructures[i].HasTileObjectOfType(type)) {
                    return true;
                }
            }
            return false;
        }
        public T GetTileObjectOfType<T>(TILE_OBJECT_TYPE type) where T : TileObject {
            for (int i = 0; i < allStructures.Count; i++) {
                T obj = allStructures[i].GetTileObjectOfType<T>(type);
                if(obj != null) {
                    return obj as T;
                }
            }
            return null;
        }
        public T GetRandomTileObjectOfTypeThatMeetCriteria<T>(System.Func<T, bool> validityChecker) where T : TileObject {
            List<T> objs = null;
            for (int i = 0; i < allStructures.Count; i++) {
                List<T> structureTileObjects = allStructures[i].GetTileObjectsOfType(validityChecker);
                if (structureTileObjects != null && structureTileObjects.Count > 0) {
                    if (objs == null) {
                        objs = new List<T>();
                    }
                    objs.AddRange(structureTileObjects);
                }
            }
            if(objs != null && objs.Count > 0) {
                return objs[UnityEngine.Random.Range(0, objs.Count)];
            }
            return null;
        }
        public T GetFirstTileObjectOfTypeThatMeetCriteria<T>(System.Func<T, bool> validityChecker) where T : TileObject {
            for (int i = 0; i < allStructures.Count; i++) {
                T structureTileObject = allStructures[i].GetFirstTileObjectOfTypeThatMeetCriteria(validityChecker);
                if (structureTileObject != null) {
                    return structureTileObject;
                }
            }
            return null;
        }
        public List<T> GetTileObjectsOfTypeThatMeetCriteria<T>(System.Func<T, bool> validityChecker) where T : TileObject {
            List<T> objs = null;
            for (int i = 0; i < allStructures.Count; i++) {
                List<T> structureTileObjects = allStructures[i].GetTileObjectsOfType(validityChecker);
                if (structureTileObjects != null && structureTileObjects.Count > 0) {
                    if (objs == null) {
                        objs = new List<T>();
                    }
                    objs.AddRange(structureTileObjects);
                }
            }
            return objs;
        }
        #endregion

        #region Party
        public void AddParty(Party party) {
            if (!parties.Contains(party)) {
                parties.Add(party);
            }
        }
        public bool RemoveParty(Party party) {
            return parties.Remove(party);
        }
        public Party GetFirstUnfullParty() {
            for (int i = 0; i < parties.Count; i++) {
                Party party = parties[i];
                if (party.members.Count < PartyManager.MAX_MEMBER_CAPACITY && party.members.Count > 0) {
                    return party;
                }
            }
            return null;
        }
        #endregion

        #region IPartyTargetDestination
        public LocationGridTile GetRandomPassableTile() {
            LocationStructure structure = GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
            if(structure == null) {
                structure = GetRandomStructure();
            }
            return structure.GetRandomPassableTile();
        }
        public bool IsAtTargetDestination(Character character) {
            return character.currentSettlement == this;
        }
        #endregion

        #region Loading
        public virtual void LoadReferences(SaveDataBaseSettlement data) {
            if (!string.IsNullOrEmpty(data.factionOwnerID)) {
                owner =  DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(data.factionOwnerID);    
            }
        }
        #endregion
    }
}