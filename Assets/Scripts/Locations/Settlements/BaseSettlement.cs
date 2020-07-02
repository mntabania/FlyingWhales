using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UtilityScripts;
namespace Locations.Settlements {
    public class BaseSettlement {
        public int id { get; }
        public LOCATION_TYPE locationType { get; private set; }
        public string name { get; private set; }
        public Faction owner { get; private set; }
        public Faction previousOwner { get; private set; }
        public List<HexTile> tiles { get; }
        public List<Character> residents { get; }
        public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; protected set; }
        public List<IPointOfInterest> firesInSettlement { get; }
        public List<LocationStructure> allStructures { get; protected set; }
        
        protected BaseSettlement(LOCATION_TYPE locationType) {
            id = UtilityScripts.Utilities.SetID(this);
            SetName(RandomNameGenerator.GenerateCityName(RACE.HUMANS));
            tiles = new List<HexTile>();
            residents = new List<Character>();
            structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
            firesInSettlement = new List<IPointOfInterest>();
            allStructures = new List<LocationStructure>();
            SetLocationType(locationType);
            StartListeningForFires();
        }
        protected BaseSettlement(SaveDataArea saveDataArea) {
            SetName(RandomNameGenerator.GenerateCityName(RACE.HUMANS));
            id = UtilityScripts.Utilities.SetID(this, saveDataArea.id);
            tiles = new List<HexTile>();
            residents = new List<Character>();
            structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
            firesInSettlement = new List<IPointOfInterest>();
            allStructures = new List<LocationStructure>();
            SetLocationType(saveDataArea.locationType);
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
        
        #region Residents
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
                Debug.LogWarning(
                    $"{name} doesn't have any dwellings for {character.name} because structures have not been generated yet");
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
            if(character.isFriendlyFactionless || character.isFactionless || (character.faction != null && !character.faction.isMajorFaction)) {
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
                allStructures.Add(structure);
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
            return CollectionUtilities.GetRandomElement(allStructures);;
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
        #endregion

        #region Fire
        private void StartListeningForFires() {
            Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
            Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
        }
        private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
            //added checker for null so that if an object has been destroyed and lost the burning trait, it will still be removed from the list
            if (trait is Burning && (traitable.gridTileLocation == null || traitable.gridTileLocation.IsPartOfSettlement(this))) {
                RemoveObjectOnFire(traitable);
            }
        }
        private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
            if (trait is Burning && traitable.gridTileLocation.IsPartOfSettlement(this)) {
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
            //default to true even if there are nounoccupied tiles in settlement 
            return true;
        }
        #endregion
    }
}