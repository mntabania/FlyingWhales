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
using Locations.Area_Features;

namespace Locations.Settlements {
    public abstract class BaseSettlement : IPartyQuestTarget, IPartyTargetDestination, IGatheringTarget, ILogFiller, IPlayerActionTarget, ILocation, IStoredTarget {

        //public static Action onSettlementBuilt;
        
        public string persistentID { get; private set; }
        public int id { get; }
        public LOCATION_TYPE locationType { get; private set; }
        public string name { get; private set; }
        public bool isStoredAsTarget { get; private set; }
        public Faction owner { get; private set; }
        public List<Area> areas { get; }
        public List<Character> residents { get; protected set; }
        public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures { get; protected set; }
        public List<IPointOfInterest> firesInSettlement { get; }
        public List<LocationStructure> allStructures { get; protected set; }
        public List<Party> parties { get; protected set; }
        public List<PLAYER_SKILL_TYPE> actions { get; private set; }
        public virtual SettlementResources SettlementResources { get; protected set; }
        public string bookmarkName => $"{iconRichText} {name}";
        public BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text_With_Cancel;
        public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }

        #region getters
        public OBJECT_TYPE objectType => OBJECT_TYPE.Settlement;
        public STORED_TARGET_TYPE storedTargetType => STORED_TARGET_TYPE.Village;
        public bool isTargetted { set; get; }

        public string iconRichText => UtilityScripts.Utilities.VillageIcon();
        public virtual Type serializedData => typeof(SaveDataBaseSettlement);
        public virtual Region region => null;
        public string locationName => name;
        public LocationStructure currentStructure => null;
        public BaseSettlement currentSettlement => this;
        public bool hasBeenDestroyed => false;
        public PARTY_TARGET_DESTINATION_TYPE partyTargetDestinationType => PARTY_TARGET_DESTINATION_TYPE.Settlement;
        #endregion

        protected BaseSettlement(LOCATION_TYPE locationType) {
            persistentID = UtilityScripts.Utilities.GetNewUniqueID();
            id = UtilityScripts.Utilities.SetID(this);
            SetName(RandomNameGenerator.GenerateSettlementName(RACE.HUMANS));
            areas = new List<Area>();
            residents = new List<Character>();
            structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
            firesInSettlement = new List<IPointOfInterest>();
            allStructures = new List<LocationStructure>();
            parties = new List<Party>();
            bookmarkEventDispatcher = new BookmarkableEventDispatcher();
            SetLocationType(locationType);
            StartListeningForFires();
            ConstructDefaultActions();
        }
        protected BaseSettlement(SaveDataBaseSettlement data) {
            persistentID = data._persistentID;
            SetName(data.name);
            id = UtilityScripts.Utilities.SetID(this, data.id);
            isStoredAsTarget = data.isStoredAsTarget;
            areas = new List<Area>();
            residents = new List<Character>();
            structures = new Dictionary<STRUCTURE_TYPE, List<LocationStructure>>();
            firesInSettlement = new List<IPointOfInterest>();
            allStructures = new List<LocationStructure>();
            parties = new List<Party>();
            bookmarkEventDispatcher = new BookmarkableEventDispatcher();
            SetLocationType(data.locationType);
            StartListeningForFires();
            ConstructDefaultActions();
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
#if DEBUG_LOG
                        Debug.LogWarning(
                            $"{GameManager.Instance.TodayLogString()}Cannot add {character.name} as resident of {name} because residency is already full!");
#endif
                        return false; //npcSettlement is at capacity
                    }
                }
                if (!CanCharacterBeAddedAsResidentBasedOnFaction(character)) {
#if DEBUG_LOG
                    character.logComponent.PrintLogIfActive(
                        $"{character.name} tried to become a resident of {name} but their factions conflicted");
#endif
                    return false;
                }
                //region.AddResident(character);
                residents.Add(character);
                AssignCharacterToDwellingInArea(character, chosenHome);
                if(owner == null && character.faction != null && (character.faction.isMajorNonPlayer || character.faction.factionType.type == FACTION_TYPE.Ratmen)) {
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
                //Once a character leaves a village, he should unclaim work structure also
                if (character.structureComponent.workPlaceStructure != null) {
                    character.structureComponent.workPlaceStructure.RemoveAssignedWorker(character);
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
#if DEBUG_LOG
                Debug.LogWarning($"{name} doesn't have any dwellings for {character.name} because structures have not been generated yet");
#endif
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
            
            //Removed this because we expect that villagers will need to buy their homes.
//             LocationStructure chosenDwelling = dwellingOverride;
//             if (chosenDwelling == null) {
//                 Character lover = CharacterManager.Instance.GetCharacterByID(character.relationshipContainer.GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER));
//                 if (lover != null && lover.faction.id == character.faction.id && residents.Contains(lover) && lover.homeStructure.tiles.Count > 0) { //check if the character has a lover that lives in the npcSettlement
//                     chosenDwelling = lover.homeStructure;
//                 }
//                 if (chosenDwelling == null && structures.ContainsKey(STRUCTURE_TYPE.DWELLING) && (character.homeStructure == null || character.homeStructure.region.id != id)) { //else, find an unoccupied dwelling (also check if the character doesn't already live in this npcSettlement)
//                     List<LocationStructure> structureList = structures[STRUCTURE_TYPE.DWELLING];
//                     for (int i = 0; i < structureList.Count; i++) {
//                         LocationStructure currDwelling = structureList[i];
//                         if (currDwelling.CanBeResidentHere(character)) {
//                             chosenDwelling = currDwelling;
//                             break;
//                         }
//                     }
//                 }
//             }
//
//             if (chosenDwelling == null) {
//                 //if the code reaches here, it means that the npcSettlement could not find a dwelling for the character
// #if DEBUG_LOG
//                 Debug.LogWarning(
//                     $"{GameManager.Instance.TodayLogString()}Could not find a dwelling for {character.name} at {name}, setting home to Town Center");
// #endif
//                 LocationStructure cityCenter = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
//                 if (cityCenter != null) {
//                     chosenDwelling = cityCenter;
//                 } else {
//                     //If there is not city center, assign random structure as home
//                     //This is usually for dungeon type settlements like caves and monster lairs
//                     chosenDwelling = GetRandomStructure();
//                 }
//             }
//             character.ChangeHomeStructure(chosenDwelling);
            if (dwellingOverride != null) {
                character.ChangeHomeStructure(dwellingOverride); 
            }
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
        public bool HasResidentWithRace(RACE p_race, Character p_exception = null) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if ((p_exception == null || p_exception != resident) && resident.race == p_race) {
                    return true;
                }
            }
            return false;
        }
        public bool HasResidentThatIsNotDead(Character p_exception = null) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if ((p_exception == null || p_exception != resident) && !resident.isDead) {
                    return true;
                }
            }
            return false;
        }
        public bool HasResidentThatIsVillagerAndNotDead() {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (!resident.isDead && resident.isNormalCharacter) {
                    return true;
                }
            }
            return false;
        }
        public bool HasResidentThatIsSapientAndInsideSettlementOrHasJoinedQuest() {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (resident.race.IsSapient() && ((resident.gridTileLocation != null && resident.gridTileLocation.IsPartOfSettlement(this)) || resident.partyComponent.isMemberThatJoinedQuest)) {
                    return true;
                }
            }
            return false;
        }
        public bool HasResidentThatIsNotDeadThatIsHostileWithFaction(Faction p_faction, Character p_exception = null) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if ((p_exception == null || resident != p_exception) && !resident.isDead
                    && (resident.faction == null || p_faction == null || p_faction.IsHostileWith(resident.faction))) {
                    return true;
                }
            }
            return false;
        }
        public bool HasResidentForExterminationPartyQuest(BaseSettlement p_settlement, Faction p_faction, Party p_party) {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (!resident.isDead
                    && !resident.partyComponent.IsAMemberOfParty(p_party)
                    && !resident.isBeingSeized
                    && resident.gridTileLocation != null
                    && resident.gridTileLocation.IsPartOfSettlement(p_settlement)
                    && (resident.faction == null || p_faction == null || p_faction.IsHostileWith(resident.faction))
                    && !resident.traitContainer.HasTrait("Hibernating")) {
                    return true;
                }
            }
            return false;
        }
        public Character GetRandomCharacterThatIsVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(Character p_character) {
            Character chosenCharacter = null;
            for (int i = 0; i < allStructures.Count; i++) {
                chosenCharacter = allStructures[i].GetRandomCharacterThatIsVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(p_character);
                if(chosenCharacter != null) {
                    return chosenCharacter;
                }
            }
            return chosenCharacter;
        }
        public Character GetRandomCharacterThatIsAliveVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(Character p_character) {
            Character chosenCharacter = null;
            for (int i = 0; i < allStructures.Count; i++) {
                chosenCharacter = allStructures[i].GetRandomCharacterThatIsAliveVillagerAndNotSeizedOrCarriedAndNotTargetedByProduceFoodAndIsRestrainedAndNot(p_character);
                if (chosenCharacter != null) {
                    return chosenCharacter;
                }
            }
            return chosenCharacter;
        }
        public Character GetRandomResidentForInvasionTargetThatIsInsideSettlement(BaseSettlement p_settlement, Character p_exception = null) {
            Character chosenCharacter = null;
            List<Character> choices = RuinarchListPool<Character>.Claim();
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if ((p_exception == null || p_exception != resident) 
                    && !resident.isDead 
                    && !resident.isBeingSeized 
                    && resident.gridTileLocation != null 
                    && resident.gridTileLocation.IsPartOfSettlement(p_settlement) 
                    && !resident.traitContainer.HasTrait("Hibernating", "Indestructible")) {
                    choices.Add(resident);
                }
            }
            if (choices != null && choices.Count > 0) {
                chosenCharacter = CollectionUtilities.GetRandomElement(choices);
            }
            RuinarchListPool<Character>.Release(choices);
            return chosenCharacter;
        }
        public Character GetRandomResidentForInvasionTargetThatIsInsideSettlementAndHostileWithFaction(BaseSettlement p_settlement, Faction p_faction, Character p_exception = null) {
            Character chosenCharacter = null;
            List<Character> choices = RuinarchListPool<Character>.Claim();
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if ((p_exception == null || p_exception != resident)
                    && !resident.isBeingSeized
                    && !resident.isDead
                    && resident.gridTileLocation != null
                    && resident.gridTileLocation.IsPartOfSettlement(p_settlement)
                    && (resident.faction == null || p_faction == null || p_faction.IsHostileWith(resident.faction))
                    && !resident.traitContainer.HasTrait("Hibernating", "Indestructible")) {
                    choices.Add(resident);
                }
            }
            if (choices != null && choices.Count > 0) {
                chosenCharacter = CollectionUtilities.GetRandomElement(choices);
            }
            RuinarchListPool<Character>.Release(choices);
            return chosenCharacter;
        }
        public Character GetRandomResidentForRescue() {
            Character chosenCharacter = null;
            List<Character> choices = RuinarchListPool<Character>.Claim();
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (!resident.isBeingSeized
                    && !resident.isDead
                    && resident.gridTileLocation != null
                    && !resident.gridTileLocation.IsNextToOrPartOfSettlement(this)
                    && resident.traitContainer.HasTrait("Restrained", "Paralyzed")) {
                    choices.Add(resident);
                }
            }
            if (choices != null && choices.Count > 0) {
                chosenCharacter = CollectionUtilities.GetRandomElement(choices);
            }
            RuinarchListPool<Character>.Release(choices);
            return chosenCharacter;
        }
        public Character GetFirstResidentThatIsAbleAndCanBecomeClass(string p_className) {
            for (int i = 0; i < residents.Count; i++) {
                Character c = residents[i];
                if (!c.isDead && c.classComponent.HasAbleClass(p_className)) {
                    return c;
                }
            }
            return null;
        }

        public int GetNumOfResidentsThatHasRaceAndClassOf(RACE p_race, string p_className, Type p_behaviourTypeException = null) {
            int count = 0;
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (resident.race == p_race 
                    && resident.characterClass.className == p_className 
                    && (p_behaviourTypeException == null || !resident.behaviourComponent.HasBehaviour(p_behaviourTypeException))) {
                    count++;
                }
            }
            return count;
        }
        public int GetNumOfResidentsThatIsAliveCombatant() {
            int count = 0;
            for (int i = 0; i < residents.Count; i++) {
                Character c = residents[i];
                if (!c.isDead && c.characterClass.IsCombatant()) {
                    count++;
                }
            }
            return count;
        }
        public int GetNumberOfResidentsThatIsAliveMonsterAndMonsterTypeIs(SUMMON_TYPE p_summonType) {
            int count = 0;
            for (int i = 0; i < residents.Count; i++) {
                Character c = residents[i];
                if (!c.isDead && c is Summon summon && summon.summonType == p_summonType) {
                    count++;
                }
            }
            return count;
        }
        public int GetNumberOfResidentsThatHasTrait(string p_traitName) {
            int count = 0;
            for (int i = 0; i < residents.Count; i++) {
                Character c = residents[i];
                if (c.traitContainer.HasTrait(p_traitName)) {
                    count++;
                }
            }
            return count;
        }
        public int GetNumberOfResidentsThatIsAliveVillager() {
            int count = 0;
            for (int i = 0; i < residents.Count; i++) {
                Character c = residents[i];
                if (!c.isDead && c.isNormalCharacter) {
                    count++;
                }
            }
            return count;
        }
        public bool HasResidents() {
            return residents.Count > 0;
        }
        public bool HasResidentForInvadeBehaviour() {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (resident.isNormalCharacter 
                    && resident.isDead == false 
                    && resident.isAlliedWithPlayer == false 
                    && !resident.traitContainer.HasTrait("Hibernating", "Indestructible") 
                    && !resident.isInLimbo 
                    && !resident.isBeingSeized 
                    && resident.carryComponent.IsNotBeingCarried()) {
                    return true;
                }
            }
            return false;
        }
        public bool HasResidentForGettingGeneralVillageTargets() {
            for (int i = 0; i < residents.Count; i++) {
                Character resident = residents[i];
                if (resident.isNormalCharacter && !resident.isAlliedWithPlayer && resident.IsAble()) {
                    return true;
                }
            }
            return false;
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
        public virtual void SetOwner(Faction p_newOwner) {
            this.owner = p_newOwner;
        
            bool isCorrupted = this.owner != null && this.owner.isPlayerFaction;
            for (int i = 0; i < areas.Count; i++) {
                Area area = areas[i];
                //area.SetCorruption(isCorrupted);
                //if (area.landmarkOnTile != null) {
                //    area.UpdateLandmarkVisuals();
                //    area.landmarkOnTile?.nameplate.UpdateVisuals();
                //}
                area.areaItem.UpdatePathfindingGraph();
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
                Debug.Log($"Added {structure.name} to {name}");
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
                    Debug.Log($"Removed {structure.name} from {name}");
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
        public LocationStructure GetRandomDwellingOrResourceProducingStructure() {
            LocationStructure chosenStructure = null;
            List<LocationStructure> pool = RuinarchListPool<LocationStructure>.Claim();
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure s = allStructures[i];
                if (s is Dwelling || s.structureType.IsFoodProducingStructure() || s.structureType.IsResourceProducingStructure()) {
                    pool.Add(s);
                }
            }
            if (pool.Count > 0) {
                chosenStructure = pool[GameUtilities.RandomBetweenTwoNumbers(0, pool.Count - 1)];
            }
            RuinarchListPool<LocationStructure>.Release(pool);
            return chosenStructure;
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
        public LocationStructure GetFirstStructureThatIsUnoccupiedDwelling(LocationStructure p_exception = null) {
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure s = allStructures[i];
                if ((p_exception == null || p_exception != s) && !s.IsOccupied() && s is Dwelling) {
                    return s;
                }
            }
            return null;
        }
        public LocationStructure GetFirstStructureWithStructureType(STRUCTURE_TYPE p_type, LocationStructure p_exception1 = null, LocationStructure p_exception2 = null) {
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure s = allStructures[i];
                if ((p_exception1 == null || p_exception1 != s) 
                    && (p_exception2 == null || p_exception2 != s)
                    && s.structureType == p_type) {
                    return s;
                }
            }
            return null;
        }
        public bool HasStructure(STRUCTURE_TYPE type1, STRUCTURE_TYPE type2, STRUCTURE_TYPE type3) {
            if (HasStructure(type1) || HasStructure(type2) || HasStructure(type3)) {
                return true;
            }
            return false;
        }
        public bool HasStructure(STRUCTURE_TYPE type) {
            return structures.ContainsKey(type);
        }
        public bool HasStructureClaimedByNonEnemyOrSelf(STRUCTURE_TYPE p_type, Character p_character, out LocationStructure foundStructure) {
            if (HasStructure(p_type)) {
                List<LocationStructure> structuresOfType = structures[p_type];
                for (int i = 0; i < structuresOfType.Count; i++) {
                    LocationStructure structure = structuresOfType[i];
                    if (structure is ManMadeStructure manMadeStructure) {
                        if (manMadeStructure.HasAssignedWorker() && (manMadeStructure.DoesCharacterWorkHere(p_character) ||  manMadeStructure.HasWorkerThatIsNotAnEnemyOfCharacter(p_character))) {
                            foundStructure = structure;
                            return true;
                        }
                    }
                }
            }
            foundStructure = null;
            return false;
        }
        public bool HasStructureForProducingResource(RESOURCE resourceType) {
            switch (resourceType) {
                case RESOURCE.FOOD:
                    return HasStructure(STRUCTURE_TYPE.HUNTER_LODGE, STRUCTURE_TYPE.FARM, STRUCTURE_TYPE.FISHERY);
                case RESOURCE.WOOD:
                    return HasStructure(STRUCTURE_TYPE.LUMBERYARD);
                case RESOURCE.STONE:
                    return HasStructure(STRUCTURE_TYPE.QUARRY);
                case RESOURCE.METAL:
                    return HasStructure(STRUCTURE_TYPE.MINE);
                default:
                    return false;
            }
        }
        public LocationStructure GetRandomStructureThatCharacterCanBeResidentAndIsNot(Character p_character, STRUCTURE_TYPE p_exceptionType) {
            List<LocationStructure> choices = RuinarchListPool<LocationStructure>.Claim();
            LocationStructure chosenStructure = null;
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure structure = allStructures[i];
                if (structure.structureType != p_exceptionType && structure.CanBeResidentHere(p_character)) {
                    choices.Add(structure);
                }
            }
            if (choices.Count > 0) {
                chosenStructure = CollectionUtilities.GetRandomElement(choices);
            }
            RuinarchListPool<LocationStructure>.Release(choices);
            return chosenStructure;
        }
        public LocationStructure GetRandomStructureWithTypeWhereAPartyHasPathTo(STRUCTURE_TYPE p_type, Party p_party) {
            List<LocationStructure> choices = RuinarchListPool<LocationStructure>.Claim();
            LocationStructure chosenStructure = null;
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure structure = allStructures[i];
                if (structure.structureType == STRUCTURE_TYPE.TAVERN && p_party.CanAMemberGoTo(structure)) {
                    choices.Add(structure);
                }
            }
            if (choices.Count > 0) {
                chosenStructure = CollectionUtilities.GetRandomElement(choices);
            }
            RuinarchListPool<LocationStructure>.Release(choices);
            return chosenStructure;
        }
        public LocationStructure GetRandomStructureThatCharacterHasPathTo(Character p_character, LocationStructure p_exception1 = null, LocationStructure p_exception2 = null) {
            List<LocationStructure> choices = RuinarchListPool<LocationStructure>.Claim();
            LocationStructure chosenStructure = null;
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure s = allStructures[i];
                if ((p_exception1 == null || p_exception1 != s)
                    && (p_exception2 == null || p_exception2 != s)
                    && p_character.movementComponent.HasPathToEvenIfDiffRegion(s.GetRandomPassableTile())) {
                    choices.Add(s);
                }
            }
            if (choices.Count > 0) {
                chosenStructure = CollectionUtilities.GetRandomElement(choices);
            }
            RuinarchListPool<LocationStructure>.Release(choices);
            return chosenStructure;
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
        public bool HasUnclaimedDwellingThatIsNotPreviousHome(Character p_character, out LocationStructure foundStructure) {
            List<LocationStructure> dwellings = GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
            if (dwellings != null) {
                for (int i = 0; i < dwellings.Count; i++) {
                    LocationStructure dwelling = dwellings[i];
                    if (dwelling != p_character.previousCharacterDataComponent.previousHomeStructure && dwelling.residents.Count <= 0) {
                        foundStructure = dwelling;
                        return true;
                    }
                }    
            }
            foundStructure = null;
            return false;
        }
        #endregion

        #region Tiles
        public void AddAreaToSettlement(Area p_area) {
            if (p_area.settlementOnArea != null) {
                //allow villages to overwrite settlement on area that is set to a cave or a special structure 
                if (p_area.settlementOnArea.locationType == LOCATION_TYPE.VILLAGE) {
#if DEBUG_LOG
                    Debug.LogWarning($"Could not add {p_area} to settlement {name} because it is already part of {p_area.settlementOnArea.name}");
#endif
                    return;    
                }
            }
            if (areas.Contains(p_area) == false) {
                areas.Add(p_area);
#if DEBUG_LOG
                Debug.Log($"Added tile {p_area.ToString()} to settlement {name}");
#endif
                p_area.SetSettlementOnArea(this);
                Messenger.Broadcast(SettlementSignals.SETTLEMENT_ADDED_AREA, p_area, this);    
                
            }
        }
        public void AddAreaToSettlement(params Area[] p_areas) {
            for (int i = 0; i < p_areas.Length; i++) {
                Area area = p_areas[i];
                AddAreaToSettlement(area);
            }
        }
        public virtual bool RemoveAreaFromSettlement(Area p_area) {
            if (areas.Remove(p_area)) {
#if DEBUG_LOG
                Debug.Log($"Removed tile {p_area.ToString()} from settlement {name}");
#endif
                p_area.SetSettlementOnArea(null);
                Messenger.Broadcast(SettlementSignals.SETTLEMENT_REMOVED_AREA, p_area, this);
                //if (locationType == LOCATION_TYPE.DEMONIC_INTRUSION) {
                //    p_area.SetCorruption(false);
                //}
                if (areas.Count <= 0) {
                    //when a settlement loses all its tiles consider it as wiped out
                    SettlementWipedOut();
                }
                return true;
            }
            return false;
        }
        //public bool HasAreaInRegion(Region region) {
        //    for (int i = 0; i < areas.Count; i++) {
        //        Area area = areas[i];
        //        if (area.region == region) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        public Area GetRandomArea() {
            return areas[UnityEngine.Random.Range(0, areas.Count)];
        }
        public LocationGridTile GetRandomPassableGridTileInSettlementStructuresThatCharacterHasPathTo(Character p_character) {
            LocationGridTile chosenTile = null;
            List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure structure = allStructures[i];
                for (int j = 0; j < structure.passableTiles.Count; j++) {
                    LocationGridTile t = structure.passableTiles[j];
                    if (p_character.movementComponent.HasPathToEvenIfDiffRegion(t)) {
                        tiles.Add(t);
                    }
                }
            }
            if(tiles.Count > 0) {
                chosenTile = tiles[GameUtilities.RandomBetweenTwoNumbers(0, tiles.Count - 1)];
            }
            RuinarchListPool<LocationGridTile>.Release(tiles);
            return chosenTile;
        }
        public void PopulatePassableTilesList(List<LocationGridTile> p_tiles) {
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure structure = allStructures[i];
                p_tiles.AddRange(structure.passableTiles);
            }
        }
        private void PopulateGridTilesInSettlementThatIsPassable(List<LocationGridTile> gridTiles) {
            for (int i = 0; i < areas.Count; i++) {
                Area area = areas[i];
                for (int j = 0; j < area.gridTileComponent.gridTiles.Count; j++) {
                    LocationGridTile t = area.gridTileComponent.gridTiles[j];
                    if (t.IsPassable()) {
                        gridTiles.Add(t);
                    }
                }
            }
        }
        public Area GetAPlainAdjacentArea() {
            List<Area> choices = ObjectPoolManager.Instance.CreateNewAreaList();
            Area chosenArea = null;
            for (int i = 0; i < areas.Count; i++) {
                Area area = areas[i];
                for (int j = 0; j < area.neighbourComponent.neighbours.Count; j++) {
                    Area neighbour = area.neighbourComponent.neighbours[j];
                    if (neighbour.region != area.region) {
                        continue; //skip tiles that are not part of the region if settlement is an NPC Settlement 
                    }
                    if (neighbour.elevationType != ELEVATION.MOUNTAIN && neighbour.elevationType != ELEVATION.WATER && neighbour.settlementOnArea == null) {
                        if (!areas.Contains(neighbour)) {
                            choices.Add(neighbour);
                        }
                    }
                }
            }
            if(choices != null && choices.Count > 0) {
                chosenArea = choices[UnityEngine.Random.Range(0, choices.Count)];
            }
            ObjectPoolManager.Instance.ReturnAreaListToPool(choices);
            return chosenArea;
        }
        #endregion

        #region Fire
        private void StartListeningForFires() {
            Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
            Messenger.AddListener<ITraitable, Trait, Character>(TraitSignals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
            Messenger.AddListener<TileObject>(TileObjectSignals.DESTROY_TILE_OBJECT, OnTileObjectDestroyed);
        }
        private void OnTileObjectDestroyed(TileObject p_tileObject) {
            if (firesInSettlement.Contains(p_tileObject)) {
                RemoveObjectOnFire(p_tileObject);
            }
        }
        private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
            //added checker for null so that if an object has been destroyed and lost the burning trait, it will still be removed from the list
            if (trait is Burning && firesInSettlement.Contains(traitable)) {
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
            bool hasPath = false;
            List<LocationGridTile> locationGridTilesInSettlement = RuinarchListPool<LocationGridTile>.Claim();
            PopulateGridTilesInSettlementThatIsPassable(locationGridTilesInSettlement);
            if (locationGridTilesInSettlement.Count > 0) {
                for (int i = 0; i < tileCount; i++) {
                    if (locationGridTilesInSettlement.Count == 0) {
                        //no more unoccupied tiles, but other tiles passed, return true
                        //hasPath = true;
                        break;
                    }
                    int index = GameUtilities.RandomBetweenTwoNumbers(0, locationGridTilesInSettlement.Count - 1);
                    LocationGridTile randomTile = locationGridTilesInSettlement[index];
                    if (character.movementComponent.HasPathToEvenIfDiffRegion(randomTile)) {
                        //no path towards random unoccupied tile in settlement, return false
                        hasPath = true;
                        break;
                    } else {
                        hasPath = false;
                    }
                    locationGridTilesInSettlement.RemoveAt(index);
                }    
            }
            RuinarchListPool<LocationGridTile>.Release(locationGridTilesInSettlement);
            //default to true even if there are no unoccupied tiles in settlement 
            return hasPath;
        }
        public void PopulateSurroundingAreas(List<Area> areas) {
            for (int i = 0; i < this.areas.Count; i++) {
                Area area = this.areas[i];
                if (this is NPCSettlement npcSettlement && area.region != npcSettlement.region) {
                    continue; //skip tiles that are not part of the region if settlement is an NPC Settlement 
                }
                for (int j = 0; j < area.neighbourComponent.neighbours.Count; j++) {
                    Area neighbour = area.neighbourComponent.neighbours[j];
                    if (neighbour.settlementOnArea == null || neighbour.settlementOnArea != this) {
                        areas.Add(neighbour);
                    }
                }
            }
        }
        public void PopulateSurroundingAreasInSameRegionWithLessThanNumOfFreezingTraps(List<Area> areas, Region region, int numOfFreezingTraps) {
            for (int i = 0; i < this.areas.Count; i++) {
                Area area = this.areas[i];
                if (this is NPCSettlement npcSettlement && area.region != npcSettlement.region) {
                    continue; //skip tiles that are not part of the region if settlement is an NPC Settlement 
                }
                for (int j = 0; j < area.neighbourComponent.neighbours.Count; j++) {
                    Area neighbour = area.neighbourComponent.neighbours[j];
                    if (neighbour.settlementOnArea == null || neighbour.settlementOnArea != this) {
                        if(neighbour.region == region && neighbour.freezingTraps < numOfFreezingTraps) {
                            areas.Add(neighbour);
                        }
                    }
                }
            }
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
        public TileObject GetRandomTileObject() {
            List<Area> areaChoices = RuinarchListPool<Area>.Claim();
            areaChoices.AddRange(areas);
            TileObject chosenTileObject = null;
            while (chosenTileObject == null && areaChoices.Count > 0) {
                int areaIndex = GameUtilities.RandomBetweenTwoNumbers(0, areaChoices.Count - 1);
                Area randomArea = areaChoices[areaIndex];
                if (randomArea != null) {
                    chosenTileObject = randomArea.tileObjectComponent.GetRandomTileObject();
                    if(chosenTileObject == null) {
                        areaChoices.RemoveAt(areaIndex);
                    }
                } else {
                    break;
                }
            }
            RuinarchListPool<Area>.Release(areaChoices);
            return chosenTileObject;
        }
        public TileObject GetRandomTileObjectForRaidAttack() {
            List<Area> areaChoices = RuinarchListPool<Area>.Claim();
            areaChoices.AddRange(areas);
            TileObject chosenTileObject = null;
            while (chosenTileObject == null && areaChoices.Count > 0) {
                int areaIndex = GameUtilities.RandomBetweenTwoNumbers(0, areaChoices.Count - 1);
                Area randomArea = areaChoices[areaIndex];
                if (randomArea != null) {
                    chosenTileObject = randomArea.tileObjectComponent.GetRandomTileObjectForRaidAttack();
                    if (chosenTileObject == null) {
                        areaChoices.RemoveAt(areaIndex);
                    }
                } else {
                    break;
                }
            }
            RuinarchListPool<Area>.Release(areaChoices);
            return chosenTileObject;
        }
        public T GetFirstTileObjectOfType<T>(TILE_OBJECT_TYPE type) where T : TileObject {
            for (int i = 0; i < allStructures.Count; i++) {
                T obj = allStructures[i].GetFirstTileObjectOfType<T>(type);
                if (obj != null) {
                    return obj;
                }
            }
            return null;
        }
        public T GetFirstTileObjectOfType<T>(TILE_OBJECT_TYPE type1, TILE_OBJECT_TYPE type2, TILE_OBJECT_TYPE type3, TILE_OBJECT_TYPE type4) where T : TileObject {
            for (int i = 0; i < allStructures.Count; i++) {
                T obj = allStructures[i].GetFirstTileObjectOfType<T>(type1, type2, type3, type4);
                if(obj != null) {
                    return obj;
                }
            }
            return null;
        }
        public T GetFirstTileObjectOfTypeThatIsAvailable<T>() where T : TileObject {
            for (int i = 0; i < allStructures.Count; i++) {
                T structureTileObject = allStructures[i].GetFirstTileObjectOfTypeThatIsAvailable<T>();
                if (structureTileObject != null) {
                    return structureTileObject;
                }
            }
            return null;
        }
        public Bed GetFirstBuiltBedThatIsAvailableAndNoActiveUsers() {
            for (int i = 0; i < allStructures.Count; i++) {
                Bed structureTileObject = allStructures[i].GetFirstBuiltBedThatIsAvailableAndNoActiveUsers();
                if (structureTileObject != null) {
                    return structureTileObject;
                }
            }
            return null;
        }
        public void PopulateTileObjectsOfType<T>(List<TileObject> objs) where T : TileObject {
            for (int i = 0; i < allStructures.Count; i++) {
                allStructures[i].PopulateTileObjectsOfType<T>(objs);
            }
        }
        public int GetNumberOfTileObjects(TILE_OBJECT_TYPE tileObjectType) {
            int count = 0;
            for (int i = 0; i < allStructures.Count; i++) {
                count += allStructures[i].GetNumberOfTileObjects(tileObjectType);
            }
            return count;
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
        public int GetPartyCount() {
            return parties.Count;
        }
        #endregion

        #region IPartyTargetDestination
        public LocationGridTile GetRandomPassableTile() {
            LocationStructure structure = GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
            if(structure == null) {
                structure = GetRandomStructure();
            }
            if(structure == null) {
                return null;
            }
            return structure.GetRandomPassableTile();
        }
        public bool IsAtTargetDestination(Character character) {
            return character.currentSettlement == this;
        }
        #endregion

        #region Player Action Target
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

        #region IStoredTarget
        public bool CanBeStoredAsTarget() {
            return true;
        }
        public void SetAsStoredTarget(bool p_state) {
            isStoredAsTarget = p_state;
        }
        #endregion

        #region IBookmarkable
        public void OnSelectBookmark() {
            UIManager.Instance.ShowSettlementInfo(this);
        }
        public void RemoveBookmark() {
            PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this);
        }
        public void OnHoverOverBookmarkItem(UIHoverPosition p_pos) { }
        public void OnHoverOutBookmarkItem() { }
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