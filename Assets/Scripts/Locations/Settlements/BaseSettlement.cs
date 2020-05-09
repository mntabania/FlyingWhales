using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Locations.Settlements {
    public class BaseSettlement {
        public int id { get; }
        public LOCATION_TYPE locationType { get; private set; }
        public int citizenCount { get; private set; }
        public string name { get; private set; }
        public Faction owner { get; private set; }
        public Faction previousOwner { get; private set; }
        public List<HexTile> tiles { get; }
        public List<Character> residents { get; }
        public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; protected set; }
        public Region region { get; protected set; }
        private List<LocationStructure> _allStructures;
        
        protected BaseSettlement(LOCATION_TYPE locationType, int citizenCount) {
            id = UtilityScripts.Utilities.SetID(this);
            SetName(RandomNameGenerator.GenerateCityName(RACE.HUMANS));
            this.citizenCount = citizenCount;
            tiles = new List<HexTile>();
            residents = new List<Character>();
            structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
            _allStructures = new List<LocationStructure>();
            SetLocationType(locationType);
        }
        protected BaseSettlement(SaveDataArea saveDataArea) {
            SetName(RandomNameGenerator.GenerateCityName(RACE.HUMANS));
            id = UtilityScripts.Utilities.SetID(this, saveDataArea.id);
            citizenCount = saveDataArea.citizenCount;
            tiles = new List<HexTile>();
            residents = new List<Character>();
            structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
            _allStructures = new List<LocationStructure>();
            SetLocationType(saveDataArea.locationType);
        }

        #region Settlement Info
        private void SetLocationType(LOCATION_TYPE locationType) {
            this.locationType = locationType;
        }
        public void SetName(string name) {
            this.name = name;
        }
        #endregion
        

        #region Residents
        public void SetInitialResidentCount(int count) {
            citizenCount = count;
        }
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
                return true;
            }
            return false;
        }
        public virtual void AssignCharacterToDwellingInArea(Character character, LocationStructure dwellingOverride = null) {
            if (structures == null) {
                Debug.LogWarning(
                    $"{name} doesn't have any dwellings for {character.name} because structures have not been generated yet");
                return;
            }
            //Note: Removed this because, even if there are no dwellings left, home structure should be set to city center
            // if (!character.isFactionless && !structures.ContainsKey(STRUCTURE_TYPE.DWELLING)) {
            //     Debug.LogWarning($"{name} doesn't have any dwellings for {character.name}");
            //     return;
            // }
            if (character.isFactionless) {
                character.SetHomeStructure(null);
                return;
            }
            LocationStructure chosenDwelling = dwellingOverride;
            if (chosenDwelling == null) {
                Character lover = CharacterManager.Instance.GetCharacterByID(character.relationshipContainer
                    .GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
                if (lover != null && lover.faction.id == character.faction.id && residents.Contains(lover) && lover.homeStructure.tiles.Count > 0) { //check if the character has a lover that lives in the npcSettlement
                    chosenDwelling = lover.homeStructure;
                }
                if (chosenDwelling == null && (character.homeStructure == null || character.homeStructure.location.id != id)) { //else, find an unoccupied dwelling (also check if the character doesn't already live in this npcSettlement)
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
            if (owner != null && character.faction != null) {
                //If character's faction is hostile with region's ruling faction, character cannot be a resident
                return !owner.IsHostileWith(character.faction);
            }
            if (owner != null && character.faction == null) {
                //If character has no faction and region has faction, character cannot be a resident
                return false;
            }
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
                    && resident.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                    && resident.IsInHomeSettlement()) {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Faction
        public void SetOwner(Faction owner) {
            SetPreviousOwner(this.owner);
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
        private void SetPreviousOwner(Faction faction) {
            previousOwner = faction;
        }
        #endregion

        #region Structures
        public void GenerateStructures(params LocationStructure[] preCreatedStructures) {
            for (int i = 0; i < preCreatedStructures.Length; i++) {
                LocationStructure structure = preCreatedStructures[i];
                AddStructure(structure);
            }
        }
        protected virtual void LoadStructures(SaveDataArea data) {
            structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
            // for (int i = 0; i < data.structures.Count; i++) {
            //     LandmarkManager.Instance.LoadStructureAt(this, data.structures[i]);
            // }
        }
        public void AddStructure(LocationStructure structure) {
            if (!structures.ContainsKey(structure.structureType)) {
                structures.Add(structure.structureType, new List<LocationStructure>());
            }
            if (!structures[structure.structureType].Contains(structure)) {
                structures[structure.structureType].Add(structure);
                _allStructures.Add(structure);
                OnStructureAdded(structure);
            }
        }
        public void RemoveStructure(LocationStructure structure) {
            if (structures.ContainsKey(structure.structureType)) {
                if (structures[structure.structureType].Remove(structure)) {
                    _allStructures.Remove(structure);
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
            return CollectionUtilities.GetRandomElement(_allStructures);;
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
        public bool HasStructure(STRUCTURE_TYPE type) {
            return structures.ContainsKey(type);
        }
        #endregion

        #region Tiles
        public void AddTileToSettlement(HexTile tile) {
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
        public HexTile GetRandomHexTile() {
            return tiles[UnityEngine.Random.Range(0, tiles.Count)];
        }
        #endregion
    }
}