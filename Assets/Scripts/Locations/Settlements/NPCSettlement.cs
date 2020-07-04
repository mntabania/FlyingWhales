using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Random = UnityEngine.Random;

public class NPCSettlement : BaseSettlement, IJobOwner {
    public LocationStructure prison { get; private set; }
    public LocationStructure mainStorage { get; private set; }
    public CityCenter cityCenter { get; private set; }
    public Region region { get; private set; }

    //Data that are only referenced from this npcSettlement's region
    //These are only getter data, meaning it cannot be stored
    public Character ruler { get; private set; }
    public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; }
    public bool isUnderSiege { get; private set; }
    public bool isPlagued { get; private set; }

    //structures
    public List<JobQueueItem> availableJobs { get; }
    public JOB_OWNER ownerType => JOB_OWNER.QUEST;
    public LocationClassManager classManager { get; }
    public LocationEventManager eventManager { get; }
    public LocationJobManager jobManager { get; }
    public SettlementJobPriorityComponent jobPriorityComponent { get; }

    private readonly WeightedDictionary<Character> newRulerDesignationWeights;
    private int newRulerDesignationChance;
    private int _isBeingHarassedCount;
    private int _isBeingInvadedCount;
    private string _plaguedExpiryKey;

    #region getters
    public JobTriggerComponent jobTriggerComponent => settlementJobTriggerComponent;
    public SettlementJobTriggerComponent settlementJobTriggerComponent { get; }
    public bool isBeingHarassed => _isBeingHarassedCount > 0;
    //public bool isBeingRaided => _isBeingRaidedCount > 0;
    public bool isBeingInvaded => _isBeingInvadedCount > 0;
    #endregion

    public NPCSettlement(Region region, LOCATION_TYPE locationType) : base(locationType) {
        this.region = region;
        newRulerDesignationWeights = new WeightedDictionary<Character>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        ResetNewRulerDesignationChance();
        availableJobs = new List<JobQueueItem>();
        classManager = new LocationClassManager();
        eventManager = new LocationEventManager(this);
        jobManager = new LocationJobManager(this);
        jobPriorityComponent = new SettlementJobPriorityComponent(this);
        settlementJobTriggerComponent = new SettlementJobTriggerComponent(this);
        _plaguedExpiryKey = string.Empty;
    }
    public NPCSettlement(SaveDataArea saveDataArea) : base (saveDataArea){
        region = GridMap.Instance.GetRegionByID(saveDataArea.regionID);
        newRulerDesignationWeights = new WeightedDictionary<Character>();
        ResetNewRulerDesignationChance();
        LoadStructures(saveDataArea);
        _plaguedExpiryKey = string.Empty;
    }

    #region Listeners
    private void SubscribeToSignals() {
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.AddListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingCharacter);
        //Messenger.AddListener<IPointOfInterest, string, JOB_TYPE>(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
        Messenger.AddListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
        //Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
        // Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
        // Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        settlementJobTriggerComponent.SubscribeToListeners();
    }
    private void UnsubscribeToSignals() {
        Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.RemoveListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingCharacter);
        //Messenger.RemoveListener<IPointOfInterest, string, JOB_TYPE>(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
        // Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
        // Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        settlementJobTriggerComponent.UnsubscribeListeners();
    }
    #endregion

    #region Utilities
    public void Initialize() {
        SubscribeToSignals();
    }
    private void SetIsUnderSiege(bool state) {
        if(isUnderSiege != state) {
            isUnderSiege = state;
            Debug.Log($"{GameManager.Instance.TodayLogString()}{name} Under Siege state changed to {isUnderSiege.ToString()}");
            Messenger.Broadcast(Signals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, this, isUnderSiege);
        }
    }
    public void SetIsPlagued(bool state) {
        if (state) {
            if (string.IsNullOrEmpty(_plaguedExpiryKey) == false) {
                //if has schedule to expire plagued status, reset expiry date.
                SchedulingManager.Instance.RemoveSpecificEntry(_plaguedExpiryKey);
            }
            GameDate expiryDate = GameManager.Instance.Today();
            expiryDate.AddDays(2);
            _plaguedExpiryKey = SchedulingManager.Instance.AddEntry(expiryDate, () => SetIsPlagued(false), this);    
        }

        isPlagued = state;
        Debug.Log($"{GameManager.Instance.TodayLogString()}{name} Plagued state changed to {isPlagued.ToString()}");
        
    }
    public void IncreaseIsBeingHarassedCount() {
        _isBeingHarassedCount++;
    }
    public void DecreaseIsBeingHarassedCount() {
        _isBeingHarassedCount--;
    }
    //public void IncreaseIsBeingRaidedCount() {
    //    _isBeingRaidedCount++;
    //}
    //public void DecreaseIsBeingRaidedCount() {
    //    _isBeingRaidedCount--;
    //}
    public void IncreaseIsBeingInvadedCount() {
        _isBeingInvadedCount++;
    }
    public void DecreaseIsBeingInvadedCount() {
        _isBeingInvadedCount--;
    }
    private void OnTickEnded() {
        ProcessForcedCancelJobsOnTickEnded();
    }
    private void OnDayStarted() {
        ClearAllBlacklistToAllExistingJobs();
    }
    #endregion

    #region Characters
    public int GetNumberOfUnoccupiedStructure(STRUCTURE_TYPE structureType) {
        if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.playerSettlement.id == id) {
            return 0;
        }
        int num = 0;
        if (structures.ContainsKey(structureType)) {
            List<LocationStructure> structureList = structures[structureType];
            for (int i = 0; i < structureList.Count; i++) {
                if (!structureList[i].IsOccupied()) {
                    num++;
                }
            }
        }
        return num;
    }
    private void OnCharacterClassChange(Character character, CharacterClass previousClass, CharacterClass currentClass) {
        if (character.homeSettlement == this) {
            classManager.OnResidentChangeClass(character, previousClass, currentClass);
            jobPriorityComponent.ChangeClassResidentResetPrimaryJob(character);
        }
    }
    public override bool AddResident(Character character, LocationStructure chosenHome = null, bool ignoreCapacity = true) {
        if (base.AddResident(character, chosenHome, ignoreCapacity)) {
            //region.AddResident(character);
            character.SetHomeSettlement(this);
            if (character.race == RACE.DEMON || character is Summon) { return true; }
            classManager.OnAddResident(character);
            jobPriorityComponent.OnAddResident(character);
            return true;
        }
        return false;
    }
    public override bool RemoveResident(Character character) {
        if (base.RemoveResident(character)) {
            //region.RemoveResident(character);
            character.SetHomeSettlement(null);
            classManager.OnRemoveResident(character);
            jobPriorityComponent.OnRemoveResident(character);
            return true;
        }
        return false;
    }
    private void OnCharacterMissing(Character missingCharacter) {
        if (ruler != null && missingCharacter == ruler) {
            SetRuler(null);
        }
    }
    private void OnCharacterDied(Character deadCharacter) {
        if (ruler != null && deadCharacter == ruler) {
            SetRuler(null);
        }
        //if(deadCharacter.homeSettlement == this) {
        //    if (!HasCanPerformOrAliveResidentInsideSettlement()) {
        //        Messenger.Broadcast(Signals.NO_ABLE_CHARACTER_INSIDE_SETTLEMENT, this);
        //    }
        //}
    }
    public void SetRuler(Character newRuler) {
        ruler?.SetRuledSettlement(null);
        ruler = newRuler;
        if(ruler != null) {
            ruler.SetRuledSettlement(this);
            //ResetNewRulerDesignationChance();
            if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
                Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForNewRulerDesignation);
            }
            Messenger.Broadcast(Signals.ON_SET_AS_SETTLEMENT_RULER, ruler);
        } else {
            Messenger.AddListener(Signals.HOUR_STARTED, CheckForNewRulerDesignation);
        }
    }
    private void CheckForNewRulerDesignation() {
        string debugLog =
            $"{GameManager.Instance.TodayLogString()}Checking for new npcSettlement ruler designation for {name}";
        debugLog += $"\n-Chance: {newRulerDesignationChance}";
        int chance = Random.Range(0, 100);
        debugLog += $"\n-Roll: {chance}";
        Debug.Log(debugLog);
        if (chance < newRulerDesignationChance) {
            DesignateNewRuler();
        } else {
            newRulerDesignationChance += 2;
        }
    }
    public void DesignateNewRuler(bool willLog = true) {
        string log =
            $"{GameManager.Instance.TodayLogString()}Designating a new npcSettlement ruler for: {region.name}(chance it triggered: {newRulerDesignationChance})";
        newRulerDesignationWeights.Clear();
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            log += $"\n\n-{resident.name}";
            if(resident.isDead /*|| resident.isMissing*/ || resident.isBeingSeized) {
                log += "\nEither dead or missing or seized, will not be part of candidates for ruler";
                continue;
            }
            int weight = 50;
            log += "\n  -Base Weight: +50";
            if (resident.isFactionLeader) {
                weight += 100;
                log += "\n  -Faction Leader: +100";
            }
            if (resident.characterClass.className == "Noble") {
                weight += 40;
                log += "\n  -Noble: +40";
            }
            int numberOfFriends = 0;
            int numberOfEnemies = 0;
            for (int j = 0; j < resident.relationshipContainer.charactersWithOpinion.Count; j++) {
                Character otherCharacter = resident.relationshipContainer.charactersWithOpinion[j];
                if (otherCharacter.homeSettlement == this) {
                    if (otherCharacter.relationshipContainer.IsFriendsWith(resident)) {
                        numberOfFriends++;
                    }else if (otherCharacter.relationshipContainer.IsEnemiesWith(resident)) {
                        numberOfEnemies++;
                    }
                }
            }
            if(numberOfFriends > 0) {
                weight += (numberOfFriends * 20);
                log +=
                    $"\n  -Num of Friend/Close Friend in the NPCSettlement: {numberOfFriends}, +{(numberOfFriends * 20)}";
            }
            if (resident.traitContainer.HasTrait("Inspiring")) {
                weight += 25;
                log += "\n  -Inspiring: +25";
            }
            if (resident.traitContainer.HasTrait("Authoritative")) {
                weight += 50;
                log += "\n  -Authoritative: +50";
            }


            if (numberOfEnemies > 0) {
                weight += (numberOfEnemies * -10);
                log += $"\n  -Num of Enemies/Rivals in the NPCSettlement: {numberOfEnemies}, +{(numberOfEnemies * -10)}";
            }
            if (resident.traitContainer.HasTrait("Ugly")) {
                weight += -20;
                log += "\n  -Ugly: -20";
            }
            if (resident.hasUnresolvedCrime) {
                weight += -50;
                log += "\n  -Has Unresolved Crime: -50";
            }
            if (resident.traitContainer.HasTrait("Worker")) {
                weight += -40;
                log += "\n  -Civilian: -40";
            }
            if (resident.traitContainer.HasTrait("Ambitious")) {
                weight = Mathf.RoundToInt(weight * 1.5f);
                log += "\n  -Ambitious: x1.5";
            }
            if (weight < 1) {
                weight = 1;
                log += "\n  -Weight cannot be less than 1, setting weight to 1";
            }
            log += $"\n  -TOTAL WEIGHT: {weight}";
            if (weight > 0) {
                newRulerDesignationWeights.AddElement(resident, weight);
            }
        }
        if(newRulerDesignationWeights.Count > 0) {
            Character chosenRuler = newRulerDesignationWeights.PickRandomElementGivenWeights();
            if (chosenRuler != null) {
                log += $"\nCHOSEN RULER: {chosenRuler.name}";
                if (willLog) {
                    chosenRuler.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Settlement_Ruler, chosenRuler);
                } else {
                    SetRuler(chosenRuler);
                }
            } else {
                log += "\nCHOSEN RULER: NONE";
            }
        } else {
            log += "\nCHOSEN RULER: NONE";
        }
        ResetNewRulerDesignationChance();
        Debug.Log(log);
    }
    private void ResetNewRulerDesignationChance() {
        newRulerDesignationChance = 5;
    }
    public List<Character> GetHostileCharactersInSettlement() {
        List<Character> hostileCharacters = new List<Character>();
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character character = region.charactersAtLocation[i];
            if (character.gridTileLocation.IsPartOfSettlement(this)
            && owner.IsHostileWith(character.faction) 
            && character.traitContainer.HasTrait("Restrained") == false
            && character.combatComponent.combatMode != COMBAT_MODE.Passive) {
                hostileCharacters.Add(character);
            }
        }
        return hostileCharacters;
    }
    public void GenerateInitialOpinionBetweenResidents() {
        for (int i = 0; i < residents.Count; i++) {
            Character resident1 = residents[i];
            for (int j = 0; j < residents.Count; j++) {
                Character resident2 = residents[j];
                if (resident1 != resident2) {
                    IRelationshipData rel1Data =
                        resident1.relationshipContainer.GetOrCreateRelationshipDataWith(resident1, resident2);
                    IRelationshipData rel2Data =
                        resident2.relationshipContainer.GetOrCreateRelationshipDataWith(resident2, resident1);

                    int compatibilityValue;
                    if (rel1Data.opinions.compatibilityValue != -1) {
                        compatibilityValue = rel1Data.opinions.compatibilityValue;
                    } else if (rel2Data.opinions.compatibilityValue != -1) {
                        compatibilityValue = rel2Data.opinions.compatibilityValue;
                    } else {
                        compatibilityValue = Random.Range(RelationshipManager.MinCompatibility,
                            RelationshipManager.MaxCompatibility);  
                    }
                    rel1Data.opinions.SetCompatibilityValue(compatibilityValue);
                    rel2Data.opinions.SetCompatibilityValue(compatibilityValue);
                    
                    rel1Data.opinions.RandomizeBaseOpinionBasedOnCompatibility();
                    rel2Data.opinions.RandomizeBaseOpinionBasedOnCompatibility();
                }
            }
        }
    }
    //private void OnCharacterCanNoLongerPerform(Character character) {
    //    if (character.homeSettlement == this) {
    //        if (!HasCanPerformOrAliveResidentInsideSettlement()) {
    //            Messenger.Broadcast(Signals.NO_ABLE_CHARACTER_INSIDE_SETTLEMENT, this);
    //        }
    //    }
    //}
    public bool HasCanPerformOrAliveResidentInsideSettlement() {
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            if(resident.canPerform && !resident.isDead 
                && resident.gridTileLocation != null 
                && resident.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                && resident.gridTileLocation.IsPartOfSettlement(this)) {
                return true;
            }
        }
        return false;
    }
    public bool HasAliveResidentInsideSettlement() {
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            if (!resident.isDead
                && resident.gridTileLocation != null
                && resident.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                && resident.gridTileLocation.IsPartOfSettlement(this)) {
                return true;
            }
        }
        return false;
    }
    public bool HasAliveResident() {
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            if (!resident.isDead) {
                return true;
            }
        }
        return false;
    }
    protected override bool IsResidentsFull() {
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
    #endregion

    #region Tile Objects
    //public bool AddSpecialTokenToLocation(SpecialToken token, LocationStructure structure = null, LocationGridTile gridLocation = null) {
    //    if (!itemsInArea.Contains(token)) {
    //        itemsInArea.Add(token);
    //        token.SetOwner(this.owner);
    //        if (areaMap != null) { //if the npcSettlement map of this npcSettlement has already been created.
    //            //Debug.Log(GameManager.Instance.TodayLogString() + "Added " + token.name + " at " + name);
    //            if (structure != null) {
    //                structure.AddItem(token, gridLocation);
    //            } else {
    //                //get structure for token
    //                LocationStructure chosen = InnerMapManager.Instance.GetRandomStructureToPlaceItem(this, token);
    //                chosen.AddItem(token);
    //            }
    //            OnItemAddedToLocation(token, token.structureLocation);
    //        }
    //        Messenger.Broadcast(Signals.ITEM_ADDED_TO_AREA, this, token);
    //        return true;
    //    }
    //    return false;
    //}
    //public void RemoveSpecialTokenFromLocation(SpecialToken token) {
    //    if (itemsInArea.Remove(token)) {
    //        LocationStructure takenFrom = token.structureLocation;
    //        if (takenFrom != null) {
    //            takenFrom.RemoveItem(token);
    //            OnItemRemovedFromLocation(token, takenFrom);
    //        }
    //        //Debug.Log(GameManager.Instance.TodayLogString() + "Removed " + token.name + " from " + name);
    //        Messenger.Broadcast(Signals.ITEM_REMOVED_FROM_AREA, this, token);
    //    }
    //}
    //public bool IsItemInventoryFull() {
    //    return itemsInArea.Count >= MAX_ITEM_CAPACITY;
    //}
    //private int GetItemsInAreaCount(SPECIAL_TOKEN itemType) {
    //    int count = 0;
    //    for (int i = 0; i < itemsInArea.Count; i++) {
    //        SpecialToken currItem = itemsInArea[i];
    //        if (currItem.specialTokenType == itemType) {
    //            count++;
    //        }
    //    }
    //    return count;
    //}
    public void OnItemAddedToLocation(TileObject item, LocationStructure structure) {
        CheckIfInventoryJobsAreStillValid(item, structure);
    }
    private void CheckIfInventoryJobsAreStillValid(TileObject item, LocationStructure structure) {
        if (structure == mainStorage && (item.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION || item.tileObjectType == TILE_OBJECT_TYPE.TOOL)) {
            if (item.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION) {
                if (mainStorage.GetBuiltTileObjectsOfType<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION).Count >= 2) {
                    List<JobQueueItem> jobs = GetJobs(JOB_TYPE.CRAFT_OBJECT);
                    for (int i = 0; i < jobs.Count; i++) {
                        JobQueueItem jqi = jobs[i];
                        if (jqi is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI is TileObject tileObject && tileObject.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION) {
                            jqi.ForceCancelJob(false, "Settlement has enough healing potions");    
                        }
                    }
                }
            } else if (item.tileObjectType == TILE_OBJECT_TYPE.TOOL) {
                if (mainStorage.GetBuiltTileObjectsOfType<TileObject>(TILE_OBJECT_TYPE.TOOL).Count >= 2) {
                    List<JobQueueItem> jobs = GetJobs(JOB_TYPE.CRAFT_OBJECT);
                    for (int i = 0; i < jobs.Count; i++) {
                        JobQueueItem jqi = jobs[i];
                        if (jqi is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI is TileObject tileObject && tileObject.tileObjectType == TILE_OBJECT_TYPE.TOOL) {
                            jqi.ForceCancelJob(false, "Settlement has enough tools");    
                        }
                    }
                }
            }
        }
    }
    public void OnItemRemovedFromLocation(TileObject item, LocationStructure structure) {
        CheckAreaInventoryJobs(structure, item);
    }
    public bool IsRequiredByLocation(TileObject item) {
        if (item.gridTileLocation != null && item.gridTileLocation.structure == mainStorage) {
            if (item.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION) {
                return mainStorage.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.HEALING_POTION) <= 2; //item is required by warehouse.
            }
            if (item.tileObjectType == TILE_OBJECT_TYPE.TOOL) {
                return mainStorage.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.TOOL) <= 2; //item is required by warehouse.
            }
        }
        return false;
    }
    #endregion

    #region Structures
    protected override void OnStructureAdded(LocationStructure structure) {
        base.OnStructureAdded(structure);
        if(cityCenter == null && structure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
            cityCenter = structure as CityCenter;
        }
        AssignPrison();
        switch (structure.structureType) {
            case STRUCTURE_TYPE.FARM:
                classManager.AddCombatantClass("Druid");
                break;
            case STRUCTURE_TYPE.LUMBERYARD:
                classManager.AddCombatantClass("Archer");
                break;
            case STRUCTURE_TYPE.CEMETERY:
                classManager.AddCombatantClass("Stalker");
                break;
            case STRUCTURE_TYPE.HUNTER_LODGE:
                classManager.AddCombatantClass("Hunter");
                break;
            case STRUCTURE_TYPE.PRISON:
                classManager.AddCombatantClass("Knight");
                break;
            case STRUCTURE_TYPE.MAGE_QUARTERS:
                classManager.AddCombatantClass("Mage");
                break;
            case STRUCTURE_TYPE.APOTHECARY:
                classManager.AddCombatantClass("Shaman");
                break;
            case STRUCTURE_TYPE.ABANDONED_MINE:
                classManager.AddCivilianClass("Miner");
                break;
        }
    }
    protected override void OnStructureRemoved(LocationStructure structure) {
        base.OnStructureRemoved(structure);
        AssignPrison();
        switch (structure.structureType) {
            case STRUCTURE_TYPE.FARM:
                classManager.RemoveCombatantClass("Druid");
                break;
            case STRUCTURE_TYPE.LUMBERYARD:
                classManager.RemoveCombatantClass("Archer");
                break;
            case STRUCTURE_TYPE.CEMETERY:
                classManager.RemoveCombatantClass("Stalker");
                break;
            case STRUCTURE_TYPE.HUNTER_LODGE:
                classManager.RemoveCombatantClass("Hunter");
                break;
            case STRUCTURE_TYPE.PRISON:
                classManager.RemoveCombatantClass("Knight");
                break;
            case STRUCTURE_TYPE.MAGE_QUARTERS:
                classManager.RemoveCombatantClass("Mage");
                break;
            case STRUCTURE_TYPE.APOTHECARY:
                classManager.RemoveCombatantClass("Shaman");
                break;
            case STRUCTURE_TYPE.ABANDONED_MINE:
                classManager.RemoveCivilianClass("Miner");
                break;
        }
    }
    protected override void LoadStructures(SaveDataArea data) {
        base.LoadStructures(data);
        AssignPrison();
    }
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if(owner != null 
            && character.gridTileLocation != null 
            && character.gridTileLocation.IsPartOfSettlement(this) 
            // && character.canPerform 
            // && character.canMove
            && character.traitContainer.HasTrait("Unconscious") == false
            && character.combatComponent.combatMode != COMBAT_MODE.Passive) {
            if (owner.IsHostileWith(character.faction)) {
                SetIsUnderSiege(true);
            }
        }
    }
    #endregion

    #region Inner Map
    public IEnumerator PlaceObjects() {
        //pre placed objects
        // foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
        //     for (int i = 0; i < keyValuePair.Value.Count; i++) {
        //         LocationStructure structure = keyValuePair.Value[i];
        //         if (structure.structureObj != null) {
        //             structure.structureObj.RegisterPreplacedObjects(structure, structure.location.innerMap);    
        //         }
        //         yield return null;
        //     }
        // }

        PlaceResourcePiles();
        yield return null;
    }
    private void PlaceResourcePiles() {
        if (structures.ContainsKey(STRUCTURE_TYPE.WAREHOUSE)) {
            mainStorage = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        } else {
            mainStorage = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
        }
        WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
        mainStorage.AddPOI(woodPile);
        // woodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.WOOD_PILE);

        StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
        mainStorage.AddPOI(stonePile);
        // stonePile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.STONE_PILE);

        MetalPile metalPile = InnerMapManager.Instance.CreateNewTileObject<MetalPile>(TILE_OBJECT_TYPE.METAL_PILE);
        mainStorage.AddPOI(metalPile);
        // metalPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.METAL_PILE);

        FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.ANIMAL_MEAT);
        mainStorage.AddPOI(foodPile);
        // foodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.FOOD_PILE);
    }
    private void AssignPrison() {
        if (locationType == LOCATION_TYPE.DEMONIC_INTRUSION) {
            return;
        }
        LocationStructure chosenPrison = GetRandomStructureOfType(STRUCTURE_TYPE.PRISON);
        if (chosenPrison != null) {
            prison = chosenPrison;
        } else {
            chosenPrison = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
            if (chosenPrison != null) {
                prison = chosenPrison;
            } 
            // else {
            //     Debug.LogWarning($"Could not find valid prison for {name}");
            // }
        }
    }
    public void OnLocationStructureObjectPlaced(LocationStructure structure) {
        if (structure.structureType == STRUCTURE_TYPE.WAREHOUSE) {
            //if a warehouse was placed, and this npcSettlement does not yet have a main storage structure, or is using the city center as their main storage structure, then use the new warehouse instead.
            if (mainStorage == null || mainStorage.structureType == STRUCTURE_TYPE.CITY_CENTER) {
                SetMainStorage(structure);
            }
        } else if (structure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
            if (mainStorage == null) {
                SetMainStorage(structure);
            }
        }
    }
    private void SetMainStorage(LocationStructure structure) {
        bool shouldCheckResourcePiles = mainStorage != null && structure != null && mainStorage != structure;
        mainStorage = structure;
        if (shouldCheckResourcePiles) {
            Messenger.Broadcast(Signals.SETTLEMENT_CHANGE_STORAGE, this);
        }
    }
    #endregion

    #region POI
    public List<IPointOfInterest> GetPointOfInterestsOfType(POINT_OF_INTEREST_TYPE type) {
        List<IPointOfInterest> pois = new List<IPointOfInterest>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                pois.AddRange(keyValuePair.Value[i].GetPOIsOfType(type));
            }
        }
        return pois;
    }
    public List<TileObject> GetTileObjectsThatAdvertise(params INTERACTION_TYPE[] types) {
        List<TileObject> objs = new List<TileObject>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> keyValuePair in structures) {
            for (int i = 0; i < keyValuePair.Value.Count; i++) {
                objs.AddRange(keyValuePair.Value[i].GetTileObjectsThatAdvertise(types));
            }
        }
        return objs;
    }
    public List<T> GetTileObjectsFromStructures<T>(STRUCTURE_TYPE structureType, Func<T, bool> validityChecker = null) where T : TileObject {
        List<T> objs = new List<T>();
        if (HasStructure(structureType)) {
            List<LocationStructure> structureList = structures[structureType];
            for (int i = 0; i < structureList.Count; i++) {
                objs.AddRange(structureList[i].GetTileObjectsOfType<T>(validityChecker));
            }
        }
        return objs;
    }
    #endregion

    #region Jobs
    public void AddToAvailableJobs(JobQueueItem job, int position = -1) {
        if (position == -1) {
            availableJobs.Add(job);    
        } else {
            availableJobs.Insert(position, job);
        }
        jobManager.OnAddToAvailableJobs(job);
        if (job is GoapPlanJob goapJob) {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{goapJob} targeting {goapJob.targetPOI} was added to {name}'s available jobs");
        } else {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{job} was added to {name}'s available jobs");    
        }
        
    }
    public bool RemoveFromAvailableJobs(JobQueueItem job) {
        if (availableJobs.Remove(job)) {
            if (job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                Debug.Log($"{GameManager.Instance.TodayLogString()}{goapJob} targeting {goapJob.targetPOI?.name} was removed from {name}'s available jobs");
            } else {
                Debug.Log($"{GameManager.Instance.TodayLogString()}{job} was removed from {name}'s available jobs");    
            }
            OnJobRemovedFromAvailableJobs(job);
            return true;
        }
        return false;
    }
    public int GetNumberOfJobsWith(CHARACTER_STATE state) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            if (availableJobs[i] is CharacterStateJob) {
                CharacterStateJob job = availableJobs[i] as CharacterStateJob;
                if (job.targetState == state) {
                    count++;
                }
            }
        }
        return count;
    }
    public int GetNumberOfJobsWith(JOB_TYPE type) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            if (availableJobs[i].jobType == type) {
                count++;
            }
        }
        return count;
    }
    public int GetNumberOfJobsWith(Func<JobQueueItem, bool> checker) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            if (checker.Invoke(availableJobs[i])) {
                count++;
            }
        }
        return count;
    }
    public bool HasJob(JobQueueItem job) {
        for (int i = 0; i < availableJobs.Count; i++) {
            if (job == availableJobs[i]) {
                return true;
            }
        }
        return false;
    }
    public bool HasJob(JOB_TYPE job, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (job == gpj.jobType && target == gpj.targetPOI) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < availableJobs.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                if (availableJobs[i].jobType == jobTypes[j]) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJob(GoapEffect effect, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (effect.conditionType == gpj.goal.conditionType
                    && effect.conditionKey == gpj.goal.conditionKey
                    && effect.target == gpj.goal.target
                    && target == gpj.targetPOI) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasJobWithOtherData(JOB_TYPE jobType, object otherData) {
        for (int i = 0; i < availableJobs.Count; i++) {
            if (availableJobs[i].jobType == jobType && availableJobs[i] is GoapPlanJob) {
                GoapPlanJob job = availableJobs[i] as GoapPlanJob;
                if (job.allOtherData != null) {
                    for (int j = 0; j < job.allOtherData.Count; j++) {
                        object data = job.allOtherData[j];
                        if (data == otherData) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    public JobQueueItem GetJob(params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < availableJobs.Count; i++) {
            for (int j = 0; j < jobTypes.Length; j++) {
                JobQueueItem job = availableJobs[i];
                if (job.jobType == jobTypes[j]) {
                    return job;
                }
            }
        }
        return null;
    }
    public List<JobQueueItem> GetJobs(params JOB_TYPE[] jobTypes) {
        List<JobQueueItem> jobs = new List<JobQueueItem>();
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (jobTypes.Contains(job.jobType)) {
                jobs.Add(job);
            }
        }
        return jobs;
    }
    public JobQueueItem GetJob(JOB_TYPE job, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem jqi = availableJobs[i];
            if (jqi is GoapPlanJob) {
                GoapPlanJob gpj = jqi as GoapPlanJob;
                if (job == gpj.jobType && target == gpj.targetPOI) {
                    return gpj;
                }
            }
        }
        return null;
    }
    public bool AddFirstUnassignedJobToCharacterJob(Character character) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if(job.assignedCharacter == null && character.jobQueue.AddJobInQueue(job)) {
                return true;
            }
        }
        return false;
    }
    public JobQueueItem GetFirstUnassignedJobToCharacterJob(Character character) {
        JobQueueItem chosenPriorityJob = null;
        JobQueueItem chosenSecondaryJob = null;
        JobQueueItem chosenAbleJob = null;

        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && character.jobQueue.CanJobBeAddedToQueue(job)) {
                if(job.jobType == character.jobComponent.primaryJob) {
                    return job;
                } else if (chosenPriorityJob == null && character.characterClass.priorityJobs != null 
                    && (character.characterClass.priorityJobs.Contains(job.jobType) || character.jobComponent.priorityJobs.Contains(job.jobType))) {
                    chosenPriorityJob = job;
                } else if (chosenSecondaryJob == null && character.characterClass.secondaryJobs != null && character.characterClass.secondaryJobs.Contains(job.jobType)) {
                    chosenSecondaryJob = job;
                } else if (chosenAbleJob == null && character.characterClass.ableJobs != null && character.characterClass.ableJobs.Contains(job.jobType)) {
                    chosenAbleJob = job;
                }
            }
        }
        if(chosenPriorityJob != null) {
            return chosenPriorityJob;
        } else if (chosenSecondaryJob != null) {
            return chosenSecondaryJob;
        } else if (chosenAbleJob != null) {
            return chosenAbleJob;
        }
        return null;
    }
    public bool AssignCharacterToJobBasedOnVision(Character character) {
        List<JobQueueItem> choices = new List<JobQueueItem>();
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI != null && character.marker.inVisionPOIs.Contains(goapJob.targetPOI) &&
                    character.jobQueue.CanJobBeAddedToQueue(job)) {
                    choices.Add(job);
                }
            }
        }
        if (choices.Count > 0) {
            JobQueueItem job = CollectionUtilities.GetRandomElement(choices);
            return character.jobQueue.AddJobInQueue(job);
        }
        return false;
    }
    public JobQueueItem GetFirstJobBasedOnVision(Character character) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI != null && character.marker.inVisionPOIs.Contains(goapJob.targetPOI) &&
                    character.jobQueue.CanJobBeAddedToQueue(job)) {
                    return job;
                }
            }
        }
        return null;
    }
    private void CheckAreaInventoryJobs(LocationStructure affectedStructure, TileObject objectThatTriggeredChange) {
        if (affectedStructure == mainStorage && (objectThatTriggeredChange == null 
             || objectThatTriggeredChange.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION 
             || objectThatTriggeredChange.tileObjectType == TILE_OBJECT_TYPE.TOOL
             || objectThatTriggeredChange.tileObjectType == TILE_OBJECT_TYPE.ANTIDOTE)) {
            //brew potion
            if (affectedStructure.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.HEALING_POTION) < 2) {
                //create an un crafted potion and place it at the main storage structure, then use that as the target for the job.
                TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION);
                item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
                affectedStructure.AddPOI(item);

                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, item, this);
                //job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.HEALING_POTION).constructionCost });
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanBrewPotion);
                AddToAvailableJobs(job);
            }
            
            //craft tool
            if (affectedStructure.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.TOOL) < 2) {
                if (!HasJob(JOB_TYPE.CRAFT_OBJECT)) {
                    //create an un crafted potion and place it at the main storage structure, then use that as the target for the job.
                    TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TOOL);
                    item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
                    affectedStructure.AddPOI(item);

                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, item, this);
                    //job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.TOOL).constructionCost });
                    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCraftTool);
                    AddToAvailableJobs(job);
                }
            }
            
            //brew antidote
            if (affectedStructure.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.ANTIDOTE) < 2) {
                if (!HasJob(JOB_TYPE.CRAFT_OBJECT)) {
                    //create an un crafted antidote and place it at the main storage structure, then use that as the target for the job.
                    TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ANTIDOTE);
                    item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
                    affectedStructure.AddPOI(item);

                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, item, this);
                    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanBrewAntidote);
                    AddToAvailableJobs(job);
                }
            }
        }
    }
    private void OnJobRemovedFromAvailableJobs(JobQueueItem job) {
        jobManager.OnRemoveFromAvailableJobs(job);
        JobManager.Instance.OnFinishJob(job);
        if (job.jobType == JOB_TYPE.CRAFT_OBJECT) {
            CheckAreaInventoryJobs(mainStorage, null);
        }
    }
    //private void CreateReplaceTileObjectJob(TileObject removedObj, LocationGridTile removedFrom) {
    //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPLACE_TILE_OBJECT, INTERACTION_TYPE.REPLACE_TILE_OBJECT, new Dictionary<INTERACTION_TYPE, object[]>() {
    //                    { INTERACTION_TYPE.REPLACE_TILE_OBJECT, new object[]{ removedObj, removedFrom } },
    //    });
    //    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeReplaceTileObjectJob);
    //    AddToAvailableJobs(job);
    //}
    private void ForceCancelAllJobsTargetingCharacter(IPointOfInterest target, string reason) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if(job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if(goapJob.targetPOI == target) {
                    if(goapJob.ForceCancelJob(false, reason)) {
                        i--;
                    }
                }
            }
        }
    }
    private void ForceCancelJobTypesTargetingPOI(IPointOfInterest target, string reason, JOB_TYPE jobType) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.jobType == jobType && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    if (goapJob.ForceCancelJob(false, reason)) {
                        i--;
                    }
                }
            }
        }
    }
    private void ClearAllBlacklistToAllExistingJobs() {
        for (int i = 0; i < availableJobs.Count; i++) {
            availableJobs[i].ClearBlacklist();
        }
    }
    #endregion

    #region IJobOwner
    public void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character) {
        //RemoveFromAvailableJobs(job);
    }
    public void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character) {
        if (!job.IsJobStillApplicable()) {
            RemoveFromAvailableJobs(job);
        }
    }
    public bool ForceCancelJob(JobQueueItem job) {
        return RemoveFromAvailableJobs(job);
    }
    public void AddForcedCancelJobsOnTickEnded(JobQueueItem job) {
        if (!forcedCancelJobsOnTickEnded.Contains(job)) {
            forcedCancelJobsOnTickEnded.Add(job);
        }
    }
    public void ProcessForcedCancelJobsOnTickEnded() {
        if (forcedCancelJobsOnTickEnded.Count > 0) {
            for (int i = 0; i < forcedCancelJobsOnTickEnded.Count; i++) {
                forcedCancelJobsOnTickEnded[i].ForceCancelJob(false);
            }
            forcedCancelJobsOnTickEnded.Clear();
        }
    }
    #endregion

    public override string ToString() {
        return name;
    }
}