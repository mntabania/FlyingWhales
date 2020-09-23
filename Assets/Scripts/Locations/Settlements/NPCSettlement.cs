using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Databases;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Locations.Settlements.Settlement_Types;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Traits;
using Random = UnityEngine.Random;

public class NPCSettlement : BaseSettlement, IJobOwner {
    public LocationStructure prison { get; private set; }
    public LocationStructure mainStorage { get; private set; }
    public CityCenter cityCenter { get; private set; }

    //Data that are only referenced from this npcSettlement's region
    //These are only getter data, meaning it cannot be stored
    public Character ruler { get; private set; }
    public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; }
    public bool isUnderSiege { get; private set; }
    public bool isPlagued { get; private set; }
    public LocationStructure exterminateTargetStructure { get; private set; }

    //structures
    public List<JobQueueItem> availableJobs { get; }
    public JOB_OWNER ownerType => JOB_OWNER.SETTLEMENT;
    public LocationClassManager classManager { get; }
    public LocationEventManager eventManager { get; }
    public SettlementJobPriorityComponent jobPriorityComponent { get; }
    public SettlementType settlementType { get; private set; }

    private Region _region;
    private readonly WeightedDictionary<Character> newRulerDesignationWeights;
    private int newRulerDesignationChance;
    private int _isBeingHarassedCount;
    private int _isBeingInvadedCount;
    private string _plaguedExpiryKey;

    #region getters
    public override Type serializedData => typeof(SaveDataNPCSettlement);
    public override Region region => _region;
    public JobTriggerComponent jobTriggerComponent => settlementJobTriggerComponent;
    public SettlementJobTriggerComponent settlementJobTriggerComponent { get; }
    public bool isBeingHarassed => _isBeingHarassedCount > 0;
    public bool isBeingInvaded => _isBeingInvadedCount > 0;
    #endregion

    public NPCSettlement(Region region, LOCATION_TYPE locationType) : base(locationType) {
        _region = region;
        newRulerDesignationWeights = new WeightedDictionary<Character>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        ResetNewRulerDesignationChance();
        availableJobs = new List<JobQueueItem>();
        classManager = new LocationClassManager();
        eventManager = new LocationEventManager(this);
        jobPriorityComponent = new SettlementJobPriorityComponent(this);
        settlementJobTriggerComponent = new SettlementJobTriggerComponent(this);
        _plaguedExpiryKey = string.Empty;
    }
    public NPCSettlement(SaveDataBaseSettlement saveDataBaseSettlement) : base (saveDataBaseSettlement) {
        //NOTE: This assumes that all tiles in this settlement is part of the same region.
        _region = GameUtilities.GetHexTilesGivenCoordinates(saveDataBaseSettlement.tileCoordinates, GridMap.Instance.map)[0].region;
        newRulerDesignationWeights = new WeightedDictionary<Character>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        ResetNewRulerDesignationChance();
        availableJobs = new List<JobQueueItem>();
        classManager = new LocationClassManager();
        eventManager = new LocationEventManager(this);
        jobPriorityComponent = new SettlementJobPriorityComponent(this);
        settlementJobTriggerComponent = new SettlementJobTriggerComponent(this);
        _plaguedExpiryKey = string.Empty;
    }

    #region Loading
    public override void LoadReferences(SaveDataBaseSettlement data) {
        base.LoadReferences(data);
        if (data is SaveDataNPCSettlement saveDataNpcSettlement) {
            if (!string.IsNullOrEmpty(saveDataNpcSettlement.prisonID)) {
                LocationStructure p = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataNpcSettlement.prisonID);
                LoadPrison(p);
            }
            if (!string.IsNullOrEmpty(saveDataNpcSettlement.mainStorageID)) {
                LocationStructure storage = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(saveDataNpcSettlement.mainStorageID);
                LoadMainStorage(storage);
            }
            LoadJobs(saveDataNpcSettlement);
            LoadRuler(saveDataNpcSettlement.rulerID);
            LoadResidents(saveDataNpcSettlement);
            LoadPartiesAndQuests(saveDataNpcSettlement);
            if (saveDataNpcSettlement.settlementType != null) {
                settlementType = saveDataNpcSettlement.settlementType.Load();    
            }
            SubscribeToSignals();
        }
    }
    private void LoadJobs(SaveDataNPCSettlement data) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            job.ForceCancelJob(false);
            i--;
        }
        for (int i = 0; i < data.jobIDs.Count; i++) {
            string jobID = data.jobIDs[i];
            JobQueueItem jobQueueItem = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(jobID);
            availableJobs.Add(jobQueueItem);
        }
        for (int i = 0; i < data.forceCancelJobIDs.Count; i++) {
            string jobID = data.forceCancelJobIDs[i];
            JobQueueItem jobQueueItem = DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(jobID);
            forcedCancelJobsOnTickEnded.Add(jobQueueItem);
        }
    }
    private void LoadRuler(string rulerID) {
        if (locationType == LOCATION_TYPE.SETTLEMENT) {
            //only load rulers if location type is settlement
            if (!string.IsNullOrEmpty(rulerID)) {
                ruler = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(rulerID);
            } else {
                ruler = null;
                Messenger.AddListener(Signals.HOUR_STARTED, CheckForNewRulerDesignation);
            }    
        }
    }
    private void LoadResidents(SaveDataBaseSettlement data) {
        if(data.residents != null) {
            for (int i = 0; i < data.residents.Count; i++) {
                Character resident = CharacterManager.Instance.GetCharacterByPersistentID(data.residents[i]);
                residents.Add(resident);
            }
        }
    }
    private void LoadPartiesAndQuests(SaveDataBaseSettlement data) {
        if (data.parties != null) {
            for (int i = 0; i < data.parties.Count; i++) {
                Party party = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.parties[i]);
                parties.Add(party);
            }
        }
        if (data.availablePartyQuests != null) {
            for (int i = 0; i < data.availablePartyQuests.Count; i++) {
                PartyQuest quest = DatabaseManager.Instance.partyQuestDatabase.GetPartyQuestByPersistentID(data.availablePartyQuests[i]);
                availablePartyQuests.Add(quest);
            }
        }
    }
    #endregion

    #region Listeners
    private void SubscribeToSignals() {
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.AddListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingCharacter);
        //Messenger.AddListener<IPointOfInterest, string, JOB_TYPE>(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
        Messenger.AddListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        // Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        Messenger.AddListener<Character, IPointOfInterest>(Signals.CHARACTER_SAW, OnCharacterSaw);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
        //Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
        //Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
        //Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        if (locationType == LOCATION_TYPE.SETTLEMENT) {
            settlementJobTriggerComponent.SubscribeToListeners();    
        }
    }
    private void UnsubscribeToSignals() {
        Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(Signals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.RemoveListener<IPointOfInterest, string>(Signals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingCharacter);
        //Messenger.RemoveListener<IPointOfInterest, string, JOB_TYPE>(Signals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        // Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        Messenger.RemoveListener<Character, IPointOfInterest>(Signals.CHARACTER_SAW, OnCharacterSaw);
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
        // Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
        // Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        if (locationType == LOCATION_TYPE.SETTLEMENT) {
            settlementJobTriggerComponent.UnsubscribeListeners();
        }
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
            if (isUnderSiege) {
                Messenger.AddListener(Signals.HOUR_STARTED, CheckIfStillUnderSiege);
            } else {
                Messenger.RemoveListener(Signals.HOUR_STARTED, CheckIfStillUnderSiege);
                if(exterminateTargetStructure != null) {
                    if(!HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Extermination, exterminateTargetStructure)) {
                        PartyManager.Instance.CreateExterminatePartyQuest(this, exterminateTargetStructure, this);
                    }
                    //settlementJobTriggerComponent.TriggerExterminationJob(exterminateTargetStructure);
                }
            }
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
            if (character.isNormalCharacter && locationType == LOCATION_TYPE.SETTLEMENT) {
                classManager.OnAddResident(character);
                jobPriorityComponent.OnAddResident(character);    
            }
            return true;
        }
        return false;
    }
    public override bool RemoveResident(Character character) {
        if (base.RemoveResident(character)) {
            //region.RemoveResident(character);
            character.SetHomeSettlement(null);
            if (character.isNormalCharacter && locationType == LOCATION_TYPE.SETTLEMENT) {
                classManager.OnRemoveResident(character);
                jobPriorityComponent.OnRemoveResident(character);
            }
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
        Character previousRuler = ruler; 
        ruler?.SetRuledSettlement(null);
        ruler = newRuler;
        if(ruler != null) {
            ruler.SetRuledSettlement(this);
            //ResetNewRulerDesignationChance();
            if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
                Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForNewRulerDesignation);
            }
            Messenger.Broadcast(Signals.ON_SET_AS_SETTLEMENT_RULER, ruler, previousRuler);
        } else {
            Messenger.Broadcast(Signals.ON_SETTLEMENT_RULER_REMOVED, this, previousRuler);
            Messenger.AddListener(Signals.HOUR_STARTED, CheckForNewRulerDesignation);
        }
    }
    private void CheckForNewRulerDesignation() {
        string debugLog =
            $"{GameManager.Instance.TodayLogString()}Checking for new npcSettlement ruler designation for {name}";
        debugLog += $"\n-Chance: {newRulerDesignationChance.ToString()}";
        int chance = Random.Range(0, 100);
        debugLog += $"\n-Roll: {chance.ToString()}";
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

            if (owner != null && resident.IsWantedBy(owner)) {
                log += "\nMember is wanted by the faction owner of this settlement " + owner.name + ", skipping...";
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
            
            if (numberOfFriends > 0) {
                int weightToAdd = 0;
                if (resident.traitContainer.HasTrait("Worker")) {
                    weightToAdd = Mathf.FloorToInt((numberOfFriends * 20) * 0.2f);
                } else {
                    weightToAdd = (numberOfFriends * 20);    
                }
                weight += weightToAdd;
                log += $"\n  -Num of Friend/Close Friend in the NPCSettlement: {numberOfFriends}, +{weightToAdd}";
            }
            
            // if(numberOfFriends > 0) {
            //     weight += (numberOfFriends * 20);
            //     log +=
            //         $"\n  -Num of Friend/Close Friend in the NPCSettlement: {numberOfFriends}, +{(numberOfFriends * 20)}";
            // }
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
            if (resident.traitContainer.HasTrait("Unattractive")) {
                weight += -20;
                log += "\n  -Unattractive: -20";
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
            if(character.reactionComponent.disguisedCharacter != null) {
                character = character.reactionComponent.disguisedCharacter;
            }
            if (!character.isDead && character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(this)
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
    //public bool HasAResidentThatIsAPartyLeader(PARTY_QUEST_TYPE partyType) {
    //    for (int i = 0; i < residents.Count; i++) {
    //        Character resident = residents[i];
    //        if(resident.partyComponent.hasParty && resident.partyComponent.currentParty.IsLeader(resident) && resident.partyComponent.currentParty.partyType == partyType) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    #endregion

    #region Tile Objects
    public void OnItemAddedToLocation(TileObject item, LocationStructure structure) {
        CheckIfInventoryJobsAreStillValid(item, structure);
        settlementJobTriggerComponent.OnItemAddedToStructure(item, structure);
    }
    public void OnItemRemovedFromLocation(TileObject item, LocationStructure structure, LocationGridTile tile) {
        CheckAreaInventoryJobs(structure, item);
        settlementJobTriggerComponent.OnItemRemovedFromStructure(item, structure, tile);
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
    #endregion

    #region Structures
    protected override void OnStructureAdded(LocationStructure structure) {
        base.OnStructureAdded(structure);
        if(cityCenter == null && structure.structureType == STRUCTURE_TYPE.CITY_CENTER) {
            cityCenter = structure as CityCenter;
        }
        UpdatePrison();
        UpdateMainStorage();
        switch (structure.structureType) {
            // case STRUCTURE_TYPE.FARM:
            //     classManager.AddCombatantClass("Druid");
            //     break;
            // case STRUCTURE_TYPE.LUMBERYARD:
            //     classManager.AddCombatantClass("Archer");
            //     break;
            // case STRUCTURE_TYPE.CEMETERY:
            //     classManager.AddCombatantClass("Stalker");
            //     break;
            // case STRUCTURE_TYPE.HUNTER_LODGE:
            //     classManager.AddCombatantClass("Hunter");
            //     break;
            // case STRUCTURE_TYPE.PRISON:
            //     classManager.AddCombatantClass("Knight");
            //     break;
            // case STRUCTURE_TYPE.MAGE_QUARTERS:
            //     classManager.AddCombatantClass("Mage");
            //     break;
            // case STRUCTURE_TYPE.APOTHECARY:
            //     classManager.AddCombatantClass("Shaman");
            //     break;
            case STRUCTURE_TYPE.MINE_SHACK:
                classManager.AddCivilianClass("Miner");
                break;
        }
    }
    protected override void OnStructureRemoved(LocationStructure structure) {
        base.OnStructureRemoved(structure);
        UpdatePrison();
        UpdateMainStorage();
        switch (structure.structureType) {
            // case STRUCTURE_TYPE.FARM:
            //     classManager.RemoveCombatantClass("Druid");
            //     break;
            // case STRUCTURE_TYPE.LUMBERYARD:
            //     classManager.RemoveCombatantClass("Archer");
            //     break;
            // case STRUCTURE_TYPE.CEMETERY:
            //     classManager.RemoveCombatantClass("Stalker");
            //     break;
            // case STRUCTURE_TYPE.HUNTER_LODGE:
            //     classManager.RemoveCombatantClass("Hunter");
            //     break;
            // case STRUCTURE_TYPE.PRISON:
            //     classManager.RemoveCombatantClass("Knight");
            //     break;
            // case STRUCTURE_TYPE.MAGE_QUARTERS:
            //     classManager.RemoveCombatantClass("Mage");
            //     break;
            // case STRUCTURE_TYPE.APOTHECARY:
            //     classManager.RemoveCombatantClass("Shaman");
            //     break;
            case STRUCTURE_TYPE.ABANDONED_MINE:
                classManager.RemoveCivilianClass("Miner");
                break;
        }
    }
    private void OnCharacterSaw(Character character, IPointOfInterest seenPOI) {
        if (character.homeSettlement == this && character.currentSettlement == this) {
            if (seenPOI is Character target) {
                if(target.reactionComponent.disguisedCharacter != null) {
                    target = target.reactionComponent.disguisedCharacter;
                }
                if(owner != null 
                   && target.gridTileLocation != null 
                   && target.gridTileLocation.IsPartOfSettlement(this)
                   && target.canPerform && target.canMove
                   //&& target.traitContainer.HasTrait("Unconscious") == false
                   && target.isDead == false
                   && target.combatComponent.combatMode != COMBAT_MODE.Passive) {
                    if (owner.IsHostileWith(target.faction)) {
                        SetIsUnderSiege(true);
                        if(target.homeStructure != null 
                           && target.homeStructure.settlementLocation != null 
                           && target.homeStructure.settlementLocation.locationType == LOCATION_TYPE.DUNGEON
                           && exterminateTargetStructure == null) {
                            exterminateTargetStructure = target.homeStructure;
                        }
                    }
                }
            }	
        }
    }
    // private void OnCharacterArrivedAtStructure(Character target, LocationStructure structure) {
    //     if(target.reactionComponent.disguisedCharacter != null) {
    //         target = target.reactionComponent.disguisedCharacter;
    //     }
    //     if(owner != null 
    //         && target.gridTileLocation != null 
    //         && target.gridTileLocation.IsPartOfSettlement(this)
    //         && target.traitContainer.HasTrait("Unconscious") == false
    //         && target.isDead == false
    //         && target.combatComponent.combatMode != COMBAT_MODE.Passive) {
    //         if (owner.IsHostileWith(target.faction)) {
    //             SetIsUnderSiege(true);
    //             if(target.homeStructure != null 
    //                 && target.homeStructure.settlementLocation != null 
    //                 && target.homeStructure.settlementLocation.locationType == LOCATION_TYPE.DUNGEON
    //                 && exterminateTargetStructure == null) {
    //                 exterminateTargetStructure = target.homeStructure;
    //             }
    //         }
    //     }
    // }
    private void CheckIfStillUnderSiege() {
        bool stillUnderSiege = false;
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character character = region.charactersAtLocation[i];
            if(character.homeSettlement != this) {
                if (character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(this) && !character.isDead 
                    && !character.traitContainer.HasTrait("Restrained", "Paralyzed") && character.combatComponent.combatMode != COMBAT_MODE.Passive) {
                    if (owner.IsHostileWith(character.faction)) {
                        stillUnderSiege = true;
                        break;
                    }
                }
            }
        }
        if (!stillUnderSiege) {
            SetIsUnderSiege(false);
        }
    }
    public void SetExterminateTarget(LocationStructure target) {
        exterminateTargetStructure = target;
    }
    #endregion

    #region Inner Map
    public IEnumerator PlaceInitialObjects() {
        PlaceResourcePiles();
        yield return null;
    }
    private void PlaceResourcePiles() {
        WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
        mainStorage.AddPOI(woodPile);

        StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
        mainStorage.AddPOI(stonePile);

        MetalPile metalPile = InnerMapManager.Instance.CreateNewTileObject<MetalPile>(TILE_OBJECT_TYPE.METAL_PILE);
        mainStorage.AddPOI(metalPile);

        FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.ANIMAL_MEAT);
        mainStorage.AddPOI(foodPile);
    }
    private void UpdatePrison() {
        LocationStructure chosenPrison = GetRandomStructureOfType(STRUCTURE_TYPE.PRISON);
        if (chosenPrison != null) {
            SetPrison(chosenPrison);
        } else {
            chosenPrison = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
            if (chosenPrison != null) {
                SetPrison(chosenPrison);
            } else {
                foreach (var kvp in structures) {
                    if (kvp.Key != STRUCTURE_TYPE.WILDERNESS) {
                        SetPrison(kvp.Value[0]);
                        break;
                    }
                } 
            }
        }
    }
    private void SetPrison(LocationStructure locationStructure) {
        prison = locationStructure;
    }
    private void UpdateMainStorage() {
        //try to assign warehouse, if no warehouse then assign main storage to city center, if no city center then set main storage to first structure that is not wilderness
        LocationStructure newStorage = null;
        if (HasStructure(STRUCTURE_TYPE.WAREHOUSE)) {
            newStorage = GetRandomStructureOfType(STRUCTURE_TYPE.WAREHOUSE);
        } else if (HasStructure(STRUCTURE_TYPE.CITY_CENTER)) {
            newStorage = GetRandomStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
        } else {
            foreach (var kvp in structures) {
                if (kvp.Key != STRUCTURE_TYPE.WILDERNESS) {
                    newStorage = kvp.Value[0];
                    break;
                }
            }
        }
        //only set main storage if: Its value is null, or the new storage is a different structure type than the current one. 
        if (mainStorage == null || (newStorage != null && newStorage.structureType != mainStorage.structureType)) {
            SetMainStorage(newStorage);
        }
    }
    private void SetMainStorage(LocationStructure structure) {
        bool shouldCheckResourcePiles = mainStorage != null && structure != null && mainStorage != structure;
        mainStorage = structure;
        if (shouldCheckResourcePiles) {
            Messenger.Broadcast(Signals.SETTLEMENT_CHANGE_STORAGE, this);
        }
    }
    public void LoadPrison(LocationStructure prison) {
        SetPrison(prison);
    }
    public void LoadMainStorage(LocationStructure mainStorage) {
        SetMainStorage(mainStorage);
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
    public int GetNumberOfJobsWith(JOB_TYPE type) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            if (availableJobs[i].jobType == type) {
                count++;
            }
        }
        return count;
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
    public JobQueueItem GetFirstJobBasedOnVisionExcept(Character character, params JOB_TYPE[] jobTypes) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && job is GoapPlanJob goapJob && !jobTypes.Contains(goapJob.jobType)) {
                if (goapJob.targetPOI != null && character.marker.inVisionPOIs.Contains(goapJob.targetPOI) &&
                    character.jobQueue.CanJobBeAddedToQueue(goapJob)) {
                    return goapJob;
                }
            }
        }
        return null;
    }
    private void CheckAreaInventoryJobs(LocationStructure affectedStructure, TileObject objectThatTriggeredChange) {
        if (affectedStructure == mainStorage && 
            (objectThatTriggeredChange == null || objectThatTriggeredChange.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION 
                                               || objectThatTriggeredChange.tileObjectType == TILE_OBJECT_TYPE.TOOL
                                               || objectThatTriggeredChange.tileObjectType == TILE_OBJECT_TYPE.ANTIDOTE)) {
            //brew potion
            if (affectedStructure.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.HEALING_POTION) < 2) {
                //create an un crafted potion and place it at the main storage structure, then use that as the target for the job.
                TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION);
                item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
                affectedStructure.AddPOI(item);

                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, item, this);
                job.SetCanTakeThisJobChecker(JobManager.Can_Brew_Potion);
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
                    job.SetCanTakeThisJobChecker(JobManager.Can_Craft_Tool);
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
                    job.SetCanTakeThisJobChecker(JobManager.Can_Brew_Antidote);
                    AddToAvailableJobs(job);
                }
            }
        }
    }
    private void OnJobRemovedFromAvailableJobs(JobQueueItem job) {
        JobManager.Instance.OnFinishJob(job);
        if (job.jobType == JOB_TYPE.CRAFT_OBJECT) {
            CheckAreaInventoryJobs(mainStorage, null);
        }
    }
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
    public void ForceCancelJobTypesTargetingPOI(JOB_TYPE jobType, IPointOfInterest target) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.jobType == jobType && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    AddForcedCancelJobsOnTickEnded(goapJob);
                }
            }
        }
    }
    private void ClearAllBlacklistToAllExistingJobs() {
        for (int i = 0; i < availableJobs.Count; i++) {
            availableJobs[i].ClearBlacklist();
        }
    }
    //public bool HasActiveParty(PARTY_QUEST_TYPE partyType) {
    //    for (int i = 0; i < residents.Count; i++) {
    //        Character character = residents[i];
    //        if (character.partyComponent.hasParty && character.partyComponent.currentParty.partyType == partyType) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    #endregion

    #region IJobOwner
    public void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character) {
        //RemoveFromAvailableJobs(job);
    }
    public void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character) {
        if (!job.IsJobStillApplicable() || job.shouldBeRemovedFromSettlementWhenUnassigned) {
            RemoveFromAvailableJobs(job);
        }
    }
    public bool ForceCancelJob(JobQueueItem job) {
        return RemoveFromAvailableJobs(job);
    }
    public void AddForcedCancelJobsOnTickEnded(JobQueueItem job) {
        if (!forcedCancelJobsOnTickEnded.Contains(job)) {
            Debug.Log(GameManager.Instance.TodayLogString() + " " + name + " added to forced cancel job " + job.name);
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

    #region Faction
    public override void SetOwner(Faction owner) {
        base.SetOwner(owner);
        if (owner == null) {
            //if owner of settlement becomes null, then set the settlement as no longer under siege
            SetIsUnderSiege(false);
        } else {
            //whenever a new owner is set, then redetermine settlement type
            if (owner.race == RACE.HUMANS) {
                SetSettlementType(SETTLEMENT_TYPE.Default_Human);
            } else if (owner.race == RACE.ELVES) {
                SetSettlementType(SETTLEMENT_TYPE.Default_Elf);
            }
        }
    }
    #endregion

    #region Settlement Type
    private void SetSettlementType(SETTLEMENT_TYPE settlementType) {
        if (locationType == LOCATION_TYPE.SETTLEMENT) {
            //Only set settlement type for villages. Do not include Dungeons. NOTE: Might be better to separate villages and dungeons into their own classes.
            this.settlementType = LandmarkManager.Instance.CreateSettlementType(settlementType);
            //NOTE: For now always apply default settings. This will change in the future.
            this.settlementType.ApplyDefaultSettings();    
        }
    }
    #endregion

    public override string ToString() {
        return name;
    }
}