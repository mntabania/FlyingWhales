using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Databases;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations;
using Locations.Settlements;
using Locations.Settlements.Components;
using Locations.Settlements.Settlement_Types;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Traits;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public class NPCSettlement : BaseSettlement, IJobOwner {

    public LocationStructure prison { get; private set; }
    public LocationStructure mainStorage { get; private set; }
    public CityCenter cityCenter { get; private set; }
    
    public Character ruler { get; private set; }
    public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; }
    public bool isUnderSiege { get; private set; }
    public bool isPlagued { get; private set; }
    public bool hasTriedToStealCorpse { get; private set; }
    public LocationStructure exterminateTargetStructure { get; private set; }

    private SettlementResources m_settlementResources;
    public override SettlementResources SettlementResources {
        get {
            if (m_settlementResources == null) {
                m_settlementResources = new SettlementResources();
            }
            return m_settlementResources;
        }
    }

    //structures
    public List<JobQueueItem> availableJobs { get; }
    public LocationEventManager eventManager { get; private set; }
    public SettlementJobPriorityComponent jobPriorityComponent { get; }
    public SettlementType settlementType { get; private set; }
    public GameDate plaguedExpiryDate { get; private set; }
    public SettlementJobTriggerComponent settlementJobTriggerComponent { get; }
    public SettlementClassTracker settlementClassTracker { get; }
    public NPCSettlementEventDispatcher npcSettlementEventDispatcher { get; }
    public bool hasPeasants { get; private set; }
    public bool hasWorkers { get; private set; }

    //Components
    public SettlementVillageMigrationComponent migrationComponent { get; private set; }

    private readonly Region _region;
    private readonly WeightedDictionary<Character> newRulerDesignationWeights;
    private int newRulerDesignationChance;
    private string _plaguedExpiryKey;
    private readonly List<TILE_OBJECT_TYPE> _neededObjects;

    #region getters
    public override Type serializedData => typeof(SaveDataNPCSettlement);
    public override Region region => _region;
    public JobTriggerComponent jobTriggerComponent => settlementJobTriggerComponent;
    public List<TILE_OBJECT_TYPE> neededObjects => _neededObjects;
    public JOB_OWNER ownerType => JOB_OWNER.SETTLEMENT;
    #endregion

    public NPCSettlement(Region region, LOCATION_TYPE locationType) : base(locationType) {
        _region = region;
        newRulerDesignationWeights = new WeightedDictionary<Character>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        ResetNewRulerDesignationChance();
        availableJobs = new List<JobQueueItem>();
        eventManager = new LocationEventManager(this);
        jobPriorityComponent = new SettlementJobPriorityComponent(this);
        settlementJobTriggerComponent = new SettlementJobTriggerComponent(this);
        settlementClassTracker = new SettlementClassTracker();
        npcSettlementEventDispatcher = new NPCSettlementEventDispatcher();
        _plaguedExpiryKey = string.Empty;
        _neededObjects = new List<TILE_OBJECT_TYPE>() {
            TILE_OBJECT_TYPE.HEALING_POTION,
            TILE_OBJECT_TYPE.TOOL,
            TILE_OBJECT_TYPE.ANTIDOTE
        };

        migrationComponent = new SettlementVillageMigrationComponent(); migrationComponent.SetOwner(this);
    }
    public NPCSettlement(SaveDataBaseSettlement saveDataBaseSettlement) : base (saveDataBaseSettlement) {
        SaveDataNPCSettlement saveData = saveDataBaseSettlement as SaveDataNPCSettlement;
        System.Diagnostics.Debug.Assert(saveData != null, nameof(saveData) + " != null");
        hasTriedToStealCorpse = saveData.hasTriedToStealCorpse;
        _region = DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(saveData.regionID);
        newRulerDesignationWeights = new WeightedDictionary<Character>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        ResetNewRulerDesignationChance();
        availableJobs = new List<JobQueueItem>();
        // eventManager = new LocationEventManager(this, saveData.eventManager); //loaded event manager at LoadReferences
        jobPriorityComponent = new SettlementJobPriorityComponent(this);
        settlementJobTriggerComponent = new SettlementJobTriggerComponent(this);
        settlementClassTracker = new SettlementClassTracker(saveData.classTracker);
        npcSettlementEventDispatcher = new NPCSettlementEventDispatcher();
        _plaguedExpiryKey = string.Empty;
        _neededObjects = new List<TILE_OBJECT_TYPE>(saveData.neededObjects);

        migrationComponent = saveData.migrationComponent.Load(); migrationComponent.SetOwner(this);
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
            eventManager = saveDataNpcSettlement.eventManager != null ? new LocationEventManager(this, saveDataNpcSettlement.eventManager) : new LocationEventManager(this);
            LoadJobs(saveDataNpcSettlement);
            LoadRuler(saveDataNpcSettlement.rulerID);
            LoadResidents(saveDataNpcSettlement);
            LoadParties(saveDataNpcSettlement);
            if (saveDataNpcSettlement.settlementType != null) {
                settlementType = saveDataNpcSettlement.settlementType.Load();    
            }
            isUnderSiege = saveDataNpcSettlement.isUnderSiege;
            isPlagued = saveDataNpcSettlement.isPlagued;
            if (isPlagued) {
                //reschedule plague expiry
                GameDate expiryDate = saveDataNpcSettlement.plaguedExpiry;
                _plaguedExpiryKey = SchedulingManager.Instance.AddEntry(expiryDate, () => SetIsPlagued(false), this);
                plaguedExpiryDate = expiryDate;
            }
            hasPeasants = saveDataNpcSettlement.hasPeasants;
            hasWorkers = saveDataNpcSettlement.hasWorkers;
            Initialize();
            if (areas.Count <= 0) {
                UnsubscribeToSignals(); //make sure that settlements that have no more areas should no longer listen to signals.
            } 
            //else {
            //    //Update tile nameplates
            //    //Fix for: https://trello.com/c/gAqpeACf/3194-loading-the-game-erases-the-faction-symbol-on-the-world-map
            //    for (int i = 0; i < areas.Count; i++) {
            //        Area tile = areas[i];
            //        tile.landmarkOnTile?.nameplate.UpdateVisuals();
            //    }    
            //}
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
        if (locationType == LOCATION_TYPE.VILLAGE) {
            //only load rulers if location type is settlement
            if (!string.IsNullOrEmpty(rulerID)) {
                ruler = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(rulerID);
            } else {
                ruler = null;
            }    
        }
    }
    private void LoadResidents(SaveDataBaseSettlement data) {
        if(data.residents != null) {
            for (int i = 0; i < data.residents.Count; i++) {
                var residentData = data.residents[i];
                Character resident = CharacterManager.Instance.GetCharacterByPersistentID(residentData);
                if (resident != null) {
                    residents.Add(resident);    
                } else {
                    Debug.LogWarning($"{name} is trying to load a null resident with id {residentData}!");
                }
            }
        }
    }
    private void LoadParties(SaveDataBaseSettlement data) {
        if (data.parties != null) {
            for (int i = 0; i < data.parties.Count; i++) {
                Party party = DatabaseManager.Instance.partyDatabase.GetPartyByPersistentID(data.parties[i]);
                parties.Add(party);
            }
        }
    }
    #endregion

    #region Listeners
    private void SubscribeToSignals() {
        Messenger.AddListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.AddListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingCharacter);
        Messenger.AddListener<IPointOfInterest, string, JOB_TYPE>(CharacterSignals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_PRESUMED_DEAD, OnCharacterPresumedDead);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        // Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        Messenger.AddListener<Character, IPointOfInterest>(CharacterSignals.CHARACTER_SAW, OnCharacterSaw);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
        Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        //Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
        //Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
        //Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        if (locationType == LOCATION_TYPE.VILLAGE) {
            settlementJobTriggerComponent.SubscribeToVillageListeners();    
        } else if (locationType == LOCATION_TYPE.DUNGEON) {
            settlementJobTriggerComponent.SubscribeToDungeonListeners();
        }
        settlementJobTriggerComponent.HookToSettlementClassTrackerEvents(settlementClassTracker);
    }
    private void UnsubscribeToSignals() {
        Messenger.RemoveListener<Character, CharacterClass, CharacterClass>(CharacterSignals.CHARACTER_CLASS_CHANGE, OnCharacterClassChange);
        Messenger.RemoveListener<IPointOfInterest, string>(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, ForceCancelAllJobsTargetingCharacter);
        Messenger.RemoveListener<IPointOfInterest, string, JOB_TYPE>(CharacterSignals.FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI, ForceCancelJobTypesTargetingPOI);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_PRESUMED_DEAD, OnCharacterPresumedDead);
        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        // Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        Messenger.RemoveListener<Character, IPointOfInterest>(CharacterSignals.CHARACTER_SAW, OnCharacterSaw);
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
        Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
        Messenger.RemoveListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterCanNoLongerPerform);
        // Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
        // Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        if (locationType == LOCATION_TYPE.VILLAGE) {
            settlementJobTriggerComponent.UnsubscribeFromVillageListeners();
        } else if (locationType == LOCATION_TYPE.DUNGEON) {
            settlementJobTriggerComponent.UnsubscribeFromDungeonListeners();
        }
        settlementJobTriggerComponent.UnHookToSettlementClassTrackerEvents(settlementClassTracker);
    }
    private void OnCharacterAddedToFaction(Character character, Faction faction) {
        //once a resident character changes its faction, check if all the residents of this settlement share the same faction,
        //if it does, then update the owner of this settlement to use the new faction.
        //This was added because vampire lords start off as vagrants and can create vampire castles while they are vagrants, and we want to
        //update the castles that they've made to be owned by their new faction.
        //EDIT NOTE: This applies to dungeon and village settlements
        if(locationType == LOCATION_TYPE.DUNGEON) {
            if (residents.Contains(character) && faction.isMajorFaction) {
                bool areAllResidentsPartOfNewFaction = true;
                for (int i = 0; i < residents.Count; i++) {
                    Character resident = residents[i];
                    if (resident.faction != faction) {
                        areAllResidentsPartOfNewFaction = false;
                        break;
                    }
                }
                if (areAllResidentsPartOfNewFaction) {
                    LandmarkManager.Instance.OwnSettlement(faction, this);
                }
            }
        } else if (locationType == LOCATION_TYPE.VILLAGE) {
            //If a character joins a faction and his home settlement is a village that is not owned by any faction
            //and there are no other residents that is in a major/undead faction
            //claim the settlement as owned by the faction that the character joined
            if(character.homeSettlement == this && character.homeSettlement.owner == null) {
                bool areThereOtherResidentsThatIsInAMajorOrUndeadFaction = false;
                for (int i = 0; i < residents.Count; i++) {
                    Character resident = residents[i];
                    if (resident != character && resident.faction != faction) {
                        if(resident.faction != null && (resident.faction.isMajorNonPlayer || resident.faction.factionType.type == FACTION_TYPE.Undead)) {
                            areThereOtherResidentsThatIsInAMajorOrUndeadFaction = true;
                            break;
                        }
                    }
                }
                if (!areThereOtherResidentsThatIsInAMajorOrUndeadFaction) {
                    LandmarkManager.Instance.OwnSettlement(faction, this);
                }
            }
        }
    }
    #endregion

    #region Utilities
    public void Initialize() {
        SubscribeToSignals();
        onSettlementBuilt?.Invoke();
    }
    protected override void SettlementWipedOut() {
        base.SettlementWipedOut();
        UnsubscribeToSignals();
        eventManager.OnSettlementDestroyed();
    }
    private void SetIsUnderSiege(bool state) {
        if(isUnderSiege != state) {
            isUnderSiege = state;
            Debug.Log($"{GameManager.Instance.TodayLogString()}{name} Under Siege state changed to {isUnderSiege.ToString()}");
            Messenger.Broadcast(SettlementSignals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, this, isUnderSiege);
            if (!isUnderSiege) {
                if(exterminateTargetStructure != null) {
                    if(owner != null && !owner.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Extermination, exterminateTargetStructure)) {
                        if(exterminateTargetStructure.settlementLocation == null || exterminateTargetStructure.settlementLocation.HasResidentThatMeetsCriteria(resident => !resident.isDead
                    && (resident.faction == null || owner == null || owner.IsHostileWith(resident.faction)))) {
                            owner.partyQuestBoard.CreateExterminatePartyQuest(null, this, exterminateTargetStructure, this);
                        }
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
            plaguedExpiryDate = expiryDate;
        }

        isPlagued = state;
        Debug.Log($"{GameManager.Instance.TodayLogString()}{name} Plagued state changed to {isPlagued.ToString()}");
        
    }
    private void OnTickEnded() {
        Profiler.BeginSample($"Settlement On Tick Ended");
        ProcessForcedCancelJobsOnTickEnded();
        Profiler.EndSample();
    }
    private void OnDayStarted() {
        hasTriedToStealCorpse = false;
        ClearAllBlacklistToAllExistingJobs();
    }
    private void OnHourStarted() {
        Profiler.BeginSample($"{name} settlement OnHourStarted");
        CheckSlaveResidents();
        CheckForJudgePrisoners();
        if(locationType == LOCATION_TYPE.VILLAGE) {
            if (ruler == null && owner != null && owner.isMajorNonPlayer) {
                CheckForNewRulerDesignation();
            }
        }
        if (isUnderSiege) {
            CheckIfStillUnderSiege();
        }
        if(owner != null) {
            if (settlementType != null && settlementType.settlementType == SETTLEMENT_TYPE.Cult_Town) {
                //The checker here should be if the faction has Bone_Golem_Makers ideology, instead of checking if faction type is Demon Cult
                //The reason we did this is right now Demon Cult is the only faction type that has Bone Golem Makers ideology and we are sure the it has that ideology
                //This is done for performance reasons since checking if it has the ideology per hour will always loop through the ideologies of the faction type
                //and will be heavy on processing if there are many of this kind of settlement exists
                //So until we can have a more performant solution, checking for Demon Cult faction type is what we're doing right now
                if (owner.factionType.type == FACTION_TYPE.Demon_Cult) { 
                    LocationStructure cultTemple = GetFirstStructureOfType(STRUCTURE_TYPE.CULT_TEMPLE);
                    if (cultTemple != null) {
                        if (!hasTriedToStealCorpse) {
                            TIME_IN_WORDS currentTime = GameManager.GetCurrentTimeInWordsOfTick();
                            if (currentTime == TIME_IN_WORDS.MORNING) {
                                hasTriedToStealCorpse = true;
                                if (GameUtilities.RollChance(45)) {
                                    settlementJobTriggerComponent.CreateStealCorpseJob(cultTemple);
                                }
                            }
                        }
                        settlementJobTriggerComponent.CreateSummonBoneGolemJob(cultTemple);
                    }
                }
            }
        }
        npcSettlementEventDispatcher.ExecuteHourStartedEvent(this);
        migrationComponent.OnHourStarted();    
        Profiler.EndSample();
    }
    #endregion

    #region Tiles
    public override bool RemoveAreaFromSettlement(Area area) {
        if (base.RemoveAreaFromSettlement(area)) {
            npcSettlementEventDispatcher.ExecuteTileRemovedEvent(area, this);
            return true;
        }
        return false;
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
            settlementClassTracker.OnResidentChangedClass(previousClass.className, character);
            jobPriorityComponent.ChangeClassResidentResetPrimaryJob(character);
        }
    }
    public override bool AddResident(Character character, LocationStructure chosenHome = null, bool ignoreCapacity = true) {
        if (base.AddResident(character, chosenHome, ignoreCapacity)) {
            //region.AddResident(character);
            character.SetHomeSettlement(this);
            OnAddResident(character);
            if (character.race == RACE.DEMON || character is Summon) { return true; }
            if (character.isNormalCharacter && locationType == LOCATION_TYPE.VILLAGE) {
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
            OnRemoveResident(character);
            if (character.isNormalCharacter && locationType == LOCATION_TYPE.VILLAGE) {
                jobPriorityComponent.OnRemoveResident(character);
            }
            UnassignJobsTakenBy(character);
            return true;
        }
        return false;
    }
    private void OnCharacterPresumedDead(Character missingCharacter) {
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
        ruler = newRuler;
        if(previousRuler != null) {
            previousRuler.behaviourComponent.RemoveBehaviourComponent(typeof(SettlementRulerBehaviour));
            if (!previousRuler.isFactionLeader) {
                previousRuler.jobComponent.RemovePriorityJob(JOB_TYPE.JUDGE_PRISONER);
                previousRuler.jobComponent.RemovePriorityJob(JOB_TYPE.PLACE_BLUEPRINT);
            }
        }
        if(ruler != null) {
            ruler.behaviourComponent.AddBehaviourComponent(typeof(SettlementRulerBehaviour));
            ruler.jobComponent.AddPriorityJob(JOB_TYPE.JUDGE_PRISONER);
            ruler.jobComponent.AddPriorityJob(JOB_TYPE.PLACE_BLUEPRINT);
            //ResetNewRulerDesignationChance();
            Messenger.Broadcast(CharacterSignals.ON_SET_AS_SETTLEMENT_RULER, ruler, previousRuler);
        } else {
            Messenger.Broadcast(CharacterSignals.ON_SETTLEMENT_RULER_REMOVED, this, previousRuler);
        }
        npcSettlementEventDispatcher.ExecuteSettlementRulerChangedEvent(newRuler, this);
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
        if(owner == null) {
            return;
        }
        string log = $"{GameManager.Instance.TodayLogString()}Designating a new npcSettlement ruler for: {region.name}(chance it triggered: {newRulerDesignationChance})";
        newRulerDesignationWeights.Clear();
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            if(resident.faction != owner) {
                continue;
            }
            log += $"\n\n-{resident.name}";
            if(resident.isDead /*|| resident.isMissing*/ || resident.isBeingSeized) {
                log += "\nEither dead or missing or seized or enslaved, will not be part of candidates for ruler";
                continue;
            }

            if (owner != null && resident.crimeComponent.IsWantedBy(owner)) {
                log += "\nMember is wanted by the faction owner of this settlement " + owner.name + ", skipping...";
                continue;
            }
            bool isInsideSettlement = resident.gridTileLocation != null && resident.gridTileLocation.IsPartOfSettlement(this);
            bool isInAnActiveParty = resident.partyComponent.isMemberThatJoinedQuest;

            if(!isInsideSettlement && !isInAnActiveParty) {
                log += "\nMember is not inside settlement and not in active party, skipping...";
                continue;
            }

            int weight = 50;
            log += "\n  -Base Weight: +50";
            if (owner != null && owner.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Vampires)) {
                Vampire vampire = resident.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                if (vampire != null && vampire.DoesFactionKnowThisVampire(owner)) {
                    weight += 100;
                    log += "\n  -Faction reveres vampires and member is a known vampire: +100";
                }
            }
            if (owner != null && owner.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves)) {
                if (resident.isLycanthrope && resident.lycanData.DoesFactionKnowThisLycan(owner)) {
                    weight += 100;
                    log += "\n  -Faction reveres werewolves and member is a known Lycanthrope: +100";
                }
            }
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
            if (weight < 1) {
                weight = 1;
                log += "\n  -Weight cannot be less than 1, setting weight to 1";
            }
            if (resident.traitContainer.HasTrait("Ambitious")) {
                weight = Mathf.RoundToInt(weight * 1.5f);
                log += "\n  -Ambitious: x1.5";
            }
            if (resident is Summon || resident.characterClass.IsZombie()) {
                if(HasResidentThatMeetsCriteria(c => c.race.IsSapient() && ((c.gridTileLocation != null && c.gridTileLocation.IsPartOfSettlement(this)) || c.partyComponent.isMemberThatJoinedQuest))) {
                    weight *= 0;
                    log += "\n  -Resident is a Summon and there is atleast 1 Sapient resident inside settlement or in active party: x0";
                }
            }
            if (resident.traitContainer.HasTrait("Enslaved")) {
                weight *= 0;
                log += "\n  -Enslaved: x0";
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
                    IRelationshipData rel1Data = resident1.relationshipContainer.GetOrCreateRelationshipDataWith(resident1, resident2);
                    IRelationshipData rel2Data = resident2.relationshipContainer.GetOrCreateRelationshipDataWith(resident2, resident1);

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
            if(resident.limiterComponent.canPerform && !resident.isDead 
                && !resident.isBeingSeized
                && resident.gridTileLocation != null
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
    public bool HasHomelessResident() {
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            if (resident.homeStructure == null) {
                return true;
            }
        }
        return false;
    }
    private void CheckForJudgePrisoners() {
        if(prison != null) {
            //Will try to judge prisoners in prison per hour
            for (int i = 0; i < prison.charactersHere.Count; i++) {
                Character character = prison.charactersHere[i];
                settlementJobTriggerComponent.TryCreateJudgePrisoner(character);
            }
        }
    }
    private void OnAddResident(Character character) {
        eventManager.OnResidentAdded(character);
        settlementClassTracker.OnResidentAdded(character);
        if(residents.Count == 1 && locationType == LOCATION_TYPE.VILLAGE && GameManager.Instance.gameHasStarted) {
            //First resident
            ChangeSettlementTypeAccordingTo(character);
        }
        bool hasUpdatedPeasantSwitch = false;
        if (character.characterClass.className == "Peasant") {
            hasUpdatedPeasantSwitch = UpdateHasPeasants();
        }
        bool hasUpdatedWorkerSwitch = false;
        if (character.traitContainer.HasTrait("Worker")) {
            hasUpdatedWorkerSwitch = UpdateHasWorkers();
        }
        if (!hasUpdatedPeasantSwitch && !hasUpdatedWorkerSwitch) {
            //if neither the switches was updated, manually call the update resident able jobs since it is possible that one of the switches are already off
            //and not calling it will result in the new residents able jobs not being updated.
            UpdateAbleJobsOfResident(character);
        }
    }
    private void OnRemoveResident(Character character) {
        eventManager.OnResidentRemoved(character);
        settlementClassTracker.OnResidentRemoved(character);
        UnapplyAbleJobsFromSettlement(character);
        if (character.characterClass.className == "Peasant") {
            UpdateHasPeasants();
        }
        if (character.traitContainer.HasTrait("Worker")) {
            UpdateHasWorkers();
        }
    }
    private void CheckSlaveResidents() {
        if (AreAllResidentsSlaves()) {
            List<Character> settlementResidents = ObjectPoolManager.Instance.CreateNewCharactersList();
            settlementResidents.AddRange(residents);
            for (int i = 0; i < settlementResidents.Count; i++) {
                Character resident = settlementResidents[i];
                resident.traitContainer.RemoveTrait(resident, "Enslaved");
            }
            ObjectPoolManager.Instance.ReturnCharactersListToPool(settlementResidents);
        }

    }
    private bool AreAllResidentsSlaves() {
        for (int i = 0; i < residents.Count; i++) {
            if (!residents[i].traitContainer.HasTrait("Enslaved")) {
                return false;
            }
        }
        return true;
    }
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
        if (structure == mainStorage && neededObjects.Contains(item.tileObjectType)) {
            if (mainStorage.GetNumberOfTileObjectsThatMeetCriteria(item.tileObjectType, t => t.mapObjectState == MAP_OBJECT_STATE.BUILT) >= 2) {
                List<JobQueueItem> jobs = GetJobs(JOB_TYPE.CRAFT_OBJECT);
                for (int i = 0; i < jobs.Count; i++) {
                    JobQueueItem jqi = jobs[i];
                    if (jqi is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI is TileObject tileObject && tileObject.tileObjectType == item.tileObjectType) {
                        jqi.ForceCancelJob(false, "Settlement has enough");    
                    }
                }
            }
            // if (item.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION) {
            //     if (mainStorage.GetBuiltTileObjectsOfType<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION).Count >= 2) {
            //         List<JobQueueItem> jobs = GetJobs(JOB_TYPE.CRAFT_OBJECT);
            //         for (int i = 0; i < jobs.Count; i++) {
            //             JobQueueItem jqi = jobs[i];
            //             if (jqi is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI is TileObject tileObject && tileObject.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION) {
            //                 jqi.ForceCancelJob(false, "Settlement has enough healing potions");    
            //             }
            //         }
            //     }
            // } else if (item.tileObjectType == TILE_OBJECT_TYPE.TOOL) {
            //     if (mainStorage.GetBuiltTileObjectsOfType<TileObject>(TILE_OBJECT_TYPE.TOOL).Count >= 2) {
            //         List<JobQueueItem> jobs = GetJobs(JOB_TYPE.CRAFT_OBJECT);
            //         for (int i = 0; i < jobs.Count; i++) {
            //             JobQueueItem jqi = jobs[i];
            //             if (jqi is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI is TileObject tileObject && tileObject.tileObjectType == TILE_OBJECT_TYPE.TOOL) {
            //                 jqi.ForceCancelJob(false, "Settlement has enough tools");    
            //             }
            //         }
            //     }
            // }
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
    }
    protected override void OnStructureRemoved(LocationStructure structure) {
        base.OnStructureRemoved(structure);
        UpdatePrison();
        UpdateMainStorage();
    }
    public void OnStructureBuilt(LocationStructure structure) {
        migrationComponent.OnStructureBuilt(structure);
    }
    private void OnCharacterSaw(Character character, IPointOfInterest seenPOI) {
        if (character.homeSettlement == this && character.currentSettlement == this) {
            if (seenPOI is Character target) {
                if(target.reactionComponent.disguisedCharacter != null) {
                    target = target.reactionComponent.disguisedCharacter;
                }
                if(owner != null && target.gridTileLocation != null && target.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(this)) {
                    if (ShouldBeUnderSiegeIfCharacterEntersSettlement(target)) {
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
    private bool ShouldBeUnderSiegeIfCharacterEntersSettlement(Character p_character) {
        if (p_character.limiterComponent.canPerform && p_character.limiterComponent.canMove && !p_character.isDead && p_character.combatComponent.combatMode != COMBAT_MODE.Passive) {
            if (owner != null && p_character.faction != null && owner.IsHostileWith(p_character.faction)) {
                if (p_character.race == RACE.WOLF && owner.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves)) {
                    return false;
                }
                return true;
            }    
        }
        return false;
    }
    private void CheckIfStillUnderSiege() {
        bool stillUnderSiege = false;
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character character = region.charactersAtLocation[i];
            if(character.homeSettlement != this) {
                if (character.gridTileLocation != null && character.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(this) && !character.isDead 
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
    public StructureSetting GetMissingFacilityToBuildBasedOnWeights() {
        WeightedDictionary<StructureSetting> facilityWeights = new WeightedDictionary<StructureSetting>(settlementType.facilityWeights.dictionary);
        foreach (var kvp in settlementType.facilityWeights.dictionary) {
            int cap = settlementType.GetFacilityCap(kvp.Key);
            int currentAmount = GetStructureCount(kvp.Key.structureType);
            SettlementResources.StructureRequirement required = kvp.Key.structureType.GetRequiredObjectForBuilding();
            if (currentAmount >= cap || !m_settlementResources.IsRequirementAvailable(required)) {
                facilityWeights.SetElementWeight(kvp.Key, 0); //remove weight of object since it is already at max.
            }
        }
        if (facilityWeights.GetTotalOfWeights() > 0) {
            return facilityWeights.PickRandomElementGivenWeights();
        }
        return default;
    }
    public int GetUnoccupiedDwellingCount() {
        int count = 0;
        List<LocationStructure> dwellings = GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
        if (dwellings != null && dwellings.Count > 0) {
            for (int i = 0; i < dwellings.Count; i++) {
                LocationStructure dwelling = dwellings[i];
                if (!dwelling.IsOccupied()) {
                    count++;
                }
            }
        }
        return count;
    }
    public bool HasFoodProducingStructure() {
        return HasStructure(STRUCTURE_TYPE.HUNTER_LODGE) || HasStructure(STRUCTURE_TYPE.FARM) || HasStructure(STRUCTURE_TYPE.FISHING_SHACK);
    }
    public StructureSetting GetValidFoodProducingStructure() {
        Assert.IsNotNull(owner);
        List<Area> surroundingAreas = ObjectPoolManager.Instance.CreateNewAreaList();
        PopulateSurroundingAreas(surroundingAreas);
        WeightedDictionary<StructureSetting> choices = new WeightedDictionary<StructureSetting>();
        if (surroundingAreas.Count(t => t.elevationType == ELEVATION.WATER) > 0) {
            choices.AddElement(new StructureSetting(STRUCTURE_TYPE.FISHING_SHACK, owner.factionType.mainResource), 100);
        }
        if (HasAvailableStructureConnectorsBasedOnGameFeature()) {
            choices.AddElement(new StructureSetting(STRUCTURE_TYPE.HUNTER_LODGE, owner.factionType.mainResource), 20);    
        }
        choices.AddElement(new StructureSetting(STRUCTURE_TYPE.FARM, owner.factionType.mainResource), 20);
        ObjectPoolManager.Instance.ReturnAreaListToPool(surroundingAreas);
        return choices.PickRandomElementGivenWeights();
    }
    #endregion

    #region Inner Map
    public IEnumerator PlaceInitialObjectsCoroutine() {
        PlaceResourcePiles();
        yield return null;
    }
    public void PlaceInitialObjects() {
        PlaceResourcePiles();
    }
    private void PlaceResourcePiles() {
        WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
        mainStorage.AddPOI(woodPile);
        woodPile.SetResourceInPile(500);

        StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
        mainStorage.AddPOI(stonePile);
        stonePile.SetResourceInPile(500);

        MetalPile metalPile = InnerMapManager.Instance.CreateNewTileObject<MetalPile>(TILE_OBJECT_TYPE.METAL_PILE);
        mainStorage.AddPOI(metalPile);
        metalPile.SetResourceInPile(500);

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
            Messenger.Broadcast(SettlementSignals.SETTLEMENT_CHANGE_STORAGE, this);
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
    public void GetTileObjectsThatAdvertise(List<TileObject> p_objectList, params INTERACTION_TYPE[] types) {
        for (int i = 0; i < allStructures.Count; i++) {
            allStructures[i].PopulateTileObjectsThatAdvertise(p_objectList, types);
        }
    }
    public List<T> PopulateTileObjectsFromStructures<T>(STRUCTURE_TYPE structureType, Func<T, bool> validityChecker = null) where T : TileObject {
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
    public int GetNumberOfJobsThatMeetCriteria(Func<GoapPlanJob, bool> criteria) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job is GoapPlanJob goapJob && (criteria == null || criteria.Invoke(goapJob))) {
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
            if (job.assignedCharacter == null && character.jobQueue.AddJobInQueue(job)) {
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
                if (job.jobType == character.jobComponent.primaryJob) {
                    return job;
                } else if (chosenPriorityJob == null && character.characterClass.priorityJobs != null
                    && (character.characterClass.priorityJobs.Contains(job.jobType) || character.jobComponent.priorityJobs.Contains(job.jobType) || character.jobComponent.additionalPriorityJobs.Contains(job.jobType))) {
                    chosenPriorityJob = job;
                } else if (chosenSecondaryJob == null && character.characterClass.secondaryJobs != null && character.characterClass.secondaryJobs.Contains(job.jobType)) {
                    chosenSecondaryJob = job;
                } else if (chosenAbleJob == null) {
                    bool isAble = character.characterClass.ableJobs != null && character.characterClass.ableJobs.Contains(job.jobType);
                    if (isAble) {
                        chosenAbleJob = job;    
                    }
                }
            }
        }
        if (chosenPriorityJob != null) {
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
                if (goapJob.targetPOI != null && character.marker.IsPOIInVision(goapJob.targetPOI) &&
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
                if (goapJob.targetPOI != null && character.marker.IsPOIInVision(goapJob.targetPOI) &&
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
                if (goapJob.targetPOI != null && character.marker.IsPOIInVision(goapJob.targetPOI) &&
                    character.jobQueue.CanJobBeAddedToQueue(goapJob)) {
                    return goapJob;
                }
            }
        }
        return null;
    }
    public JobQueueItem GetFirstJobBasedOnVisionExcept(Character character, JOB_TYPE except) {
            for (int i = 0; i < availableJobs.Count; i++) {
                JobQueueItem job = availableJobs[i];
                if (job.assignedCharacter == null && job is GoapPlanJob goapJob && except != goapJob.jobType) {
                    if (goapJob.targetPOI != null && character.marker.IsPOIInVision(goapJob.targetPOI) &&
                        character.jobQueue.CanJobBeAddedToQueue(goapJob)) {
                        return goapJob;
                    }
                }
            }
            return null;
        }
    private void CheckAreaInventoryJobs(LocationStructure affectedStructure, TileObject objectThatTriggeredChange) {
        if (affectedStructure == mainStorage && (objectThatTriggeredChange == null || neededObjects.Contains(objectThatTriggeredChange.tileObjectType))) {
            for (int i = 0; i < neededObjects.Count; i++) {
                TILE_OBJECT_TYPE neededObject = neededObjects[i];
                int objectsCount = affectedStructure.GetNumberOfTileObjectsThatMeetCriteria(neededObject, null); //This includes unbuilt objects 
                int neededCount = 2;
                if (objectsCount < neededCount) {
                    int missing = neededCount - objectsCount;
                    for (int j = 0; j < missing; j++) {
                        //create an un crafted object and place it at the main storage structure, then use that as the target for the job.
                        TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(neededObject);
                        affectedStructure.AddPOI(item);
                        item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, item, this);
                        JobUtilities.PopulatePriorityLocationsForTakingNonEdibleResources(this, job, INTERACTION_TYPE.TAKE_RESOURCE);
                        switch (neededObject) {
                            case TILE_OBJECT_TYPE.HEALING_POTION:
                                job.SetCanTakeThisJobChecker(JobManager.Can_Brew_Potion);
                                break;
                            case TILE_OBJECT_TYPE.TOOL:
                                job.SetCanTakeThisJobChecker(JobManager.Can_Craft_Tool);
                                break;
                            case TILE_OBJECT_TYPE.ANTIDOTE:
                                job.SetCanTakeThisJobChecker(JobManager.Can_Brew_Antidote);
                                break;
                            case TILE_OBJECT_TYPE.PHYLACTERY:
                                job.SetCanTakeThisJobChecker(JobManager.Can_Craft_Phylactery);
                                // TileObjectData data = TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.PHYLACTERY);
                                // data.TryGetPossibleRecipe(region, out var recipeToUse); //get possible recipe and assign that to job
                                // job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[]{ recipeToUse });
                                break;
                        }
                        AddToAvailableJobs(job);    
                    }
                }
            }
            
            
            // //brew potion
            // if (affectedStructure.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.HEALING_POTION) < 2) {
            //     //create an un crafted potion and place it at the main storage structure, then use that as the target for the job.
            //     TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HEALING_POTION);
            //     item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
            //     affectedStructure.AddPOI(item);
            //
            //     GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, item, this);
            //     job.SetCanTakeThisJobChecker(JobManager.Can_Brew_Potion);
            //     AddToAvailableJobs(job);
            // }
            //
            // //craft tool
            // if (affectedStructure.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.TOOL) < 2) {
            //     if (!HasJob(JOB_TYPE.CRAFT_OBJECT)) {
            //         //create an un crafted potion and place it at the main storage structure, then use that as the target for the job.
            //         TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.TOOL);
            //         item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
            //         affectedStructure.AddPOI(item);
            //
            //         GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, item, this);
            //         job.SetCanTakeThisJobChecker(JobManager.Can_Craft_Tool);
            //         AddToAvailableJobs(job);
            //     }
            // }
            //
            // //brew antidote
            // if (affectedStructure.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.ANTIDOTE) < 2) {
            //     if (!HasJob(JOB_TYPE.CRAFT_OBJECT)) {
            //         //create an un crafted antidote and place it at the main storage structure, then use that as the target for the job.
            //         TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.ANTIDOTE);
            //         item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
            //         affectedStructure.AddPOI(item);
            //
            //         GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, item, this);
            //         job.SetCanTakeThisJobChecker(JobManager.Can_Brew_Antidote);
            //         AddToAvailableJobs(job);
            //     }
            // }
            //
            // //TODO: Delete after testing phylactery
            // //craft phylactery
            // if (affectedStructure.GetTileObjectsOfTypeCount(TILE_OBJECT_TYPE.PHYLACTERY) < 2) {
            //     if (!HasJob(JOB_TYPE.CRAFT_OBJECT)) {
            //         //create an un crafted antidote and place it at the main storage structure, then use that as the target for the job.
            //         TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.PHYLACTERY);
            //         item.SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);
            //         affectedStructure.AddPOI(item);
            //
            //         GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CRAFT_OBJECT, INTERACTION_TYPE.CRAFT_TILE_OBJECT, item, this);
            //         TileObjectData data = TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.PHYLACTERY);
            //         job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[]{ data.mainRecipe });
            //         job.SetCanTakeThisJobChecker(JobManager.Can_Craft_Phylactery);
            //         AddToAvailableJobs(job);
            //     }
            // }
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
            if (job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    if (goapJob.ForceCancelJob(false, reason)) {
                        i--;
                    }
                }
            }
        }
    }
    private void ForceCancelJobTypesTargetingPOI(IPointOfInterest target, string reason, JOB_TYPE jobType) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (!job.hasBeenReset && job.jobType == jobType && job is GoapPlanJob goapJob) {
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
    private void UnassignJobsTakenBy(Character character) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == character && job is GoapPlanJob goapJob) {
                goapJob.CancelJob(false, string.Empty);
            }
        }
    }
    /// <summary>
    /// Whenever a resident updates its class also update the
    /// hasPeasants and hasWorkers switch
    /// </summary>
    public void OnResidentUpdatedClass() {
        UpdateHasPeasants();
        UpdateHasWorkers();
    }
    /// <summary>
    /// Update the hasPeasants switch.
    /// </summary>
    /// <returns>Whether or not a change happened</returns>
    private bool UpdateHasPeasants() {
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            if (resident.characterClass.className == "Peasant") {
                return SetHasPeasants(true);
            }
        }
        return SetHasPeasants(false);
    }
    /// <summary>
    /// Update the hasWorkers switch.
    /// </summary>
    /// <returns>Whether or not a change happened</returns>
    private bool UpdateHasWorkers() {
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            if (resident.traitContainer.HasTrait("Worker")) {
                return SetHasWorkers(true);
            }
        }
        return SetHasWorkers(false);
    }
    /// <summary>
    /// Switch the has peasants switch on/off
    /// </summary>
    /// <param name="state">The state to switch to</param>
    /// <returns>Whether or not the switched was toggled</returns>
    private bool SetHasPeasants(bool state) {
        if (hasPeasants != state) {
            hasPeasants = state;
            UpdateAbleJobsOfAllResidents();
            return true;
        }
        return false;
    }
    /// <summary>
    /// Switch the has workers switch on/off
    /// </summary>
    /// <param name="state">The state to switch to</param>
    /// <returns>Whether or not the switched was toggled</returns>
    private bool SetHasWorkers(bool state) {
        if (hasWorkers != state) {
            hasWorkers = state;
            UpdateAbleJobsOfAllResidents();
            return true;
        }
        return false;
    }
    private void UpdateAbleJobsOfAllResidents() {
        if (owner != null && owner.factionType.type == FACTION_TYPE.Ratmen) { return; }
        for (int i = 0; i < residents.Count; i++) {
            Character character = residents[i];
            UpdateAbleJobsOfResident(character);
        }
    }
    public void UpdateAbleJobsOfResident(Character character) {
        if (owner != null && owner.factionType.type == FACTION_TYPE.Ratmen) { return; }
        if (!character.race.IsSapient() && character.minion == null) { return; }
        //update jobs based on hasPeasants switch
        if (!hasPeasants) {
            if (character.characterClass.className != "Noble") {
                CharacterClass peasantClass = CharacterManager.Instance.GetCharacterClass("Peasant");
                character.jobComponent.AddAdditionalPriorityJob(peasantClass.priorityJobs);
                character.jobComponent.AddAdditionalPriorityJob(peasantClass.secondaryJobs);
                character.jobComponent.AddAdditionalPriorityJob(peasantClass.ableJobs);
            }
        } else {
            if (character.characterClass.className != "Noble") {
                CharacterClass peasantClass = CharacterManager.Instance.GetCharacterClass("Peasant");
                character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.priorityJobs);
                character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.secondaryJobs);
                character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.ableJobs);
            }
        }
        
        //update jobs based on hasWorkers switch
        if (!hasWorkers) {
            if (character.characterClass.className == "Noble") {
                CharacterClass peasantClass = CharacterManager.Instance.GetCharacterClass("Peasant");
                character.jobComponent.AddAdditionalPriorityJob(peasantClass.priorityJobs);
                character.jobComponent.AddAdditionalPriorityJob(peasantClass.secondaryJobs);
                character.jobComponent.AddAdditionalPriorityJob(peasantClass.ableJobs);
            }
        } else {
            if (character.characterClass.className == "Noble") {
                CharacterClass peasantClass = CharacterManager.Instance.GetCharacterClass("Peasant");
                character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.priorityJobs);
                character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.secondaryJobs);
                character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.ableJobs);
            }
        }
    }
    public void UnapplyAbleJobsFromSettlement(Character character) {
        if (!character.race.IsSapient() && character.minion == null) { return; }
        if (!hasPeasants) {
            if (character.characterClass.className != "Noble") {
                CharacterClass peasantClass = CharacterManager.Instance.GetCharacterClass("Peasant");
                character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.priorityJobs);
                character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.secondaryJobs);
                character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.ableJobs);
            }
        }
        
        if (!hasWorkers) {
            if (character.characterClass.className == "Noble") {
                CharacterClass peasantClass = CharacterManager.Instance.GetCharacterClass("Peasant");
                character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.priorityJobs);
                character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.secondaryJobs);
                character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.ableJobs);
            }
        }
    }
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
    public override void SetOwner(Faction p_newOwner) {
        Faction previousOwner = this.owner;
        base.SetOwner(p_newOwner);
        if(p_newOwner != previousOwner) {
            migrationComponent.ResetLongTermModifier();
        }
        if (p_newOwner == null) {
            //if owner of settlement becomes null, then set the settlement as no longer under siege
            SetIsUnderSiege(false);
        }
        migrationComponent.ForceRandomizePerHourIncrement();
        npcSettlementEventDispatcher.ExecuteFactionOwnerChangedEvent(previousOwner, p_newOwner, this);
    }
    #endregion

    #region Settlement Type
    public void SetSettlementType(SETTLEMENT_TYPE type) {
        if (locationType == LOCATION_TYPE.VILLAGE) {
            //Only set settlement type for villages. Do not include Dungeons. NOTE: Might be better to separate villages and dungeons into their own classes.
            if(settlementType == null || settlementType.settlementType != type) {
                //Only change settlement type if the currrent type is not the same as the one being set, if for example, the current type is Human Village, and the type to be set is also Human Village, there is no need to change the settlement type because they are the same
                settlementType = LandmarkManager.Instance.CreateSettlementType(type);
                //NOTE: For now always apply default settings. This will change in the future.
                settlementType.ApplyDefaultSettings();

                migrationComponent.OnSettlementTypeChanged();
            }
  
        }
    }
    private void ChangeSettlementTypeAccordingTo(Character character) {
        SETTLEMENT_TYPE typeToSet = LandmarkManager.Instance.GetSettlementTypeForCharacter(character);
        SetSettlementType(typeToSet);
        // if (character.race == RACE.HUMANS && (settlementType == null || settlementType.settlementType != SETTLEMENT_TYPE.Default_Human)) {
        //     SetSettlementType(SETTLEMENT_TYPE.Default_Human);
        // } else if (character.race == RACE.ELVES && (settlementType == null || settlementType.settlementType != SETTLEMENT_TYPE.Default_Elf)) {
        //     SetSettlementType(SETTLEMENT_TYPE.Default_Elf);
        // }
    }
    #endregion

    #region Needed Items
    public void AddNeededItems(TILE_OBJECT_TYPE tileObjectType) {
        //Allowed duplicate entries because multiple events can add the same item, and we don't want that item to be removed when only one event ends
        //And this is alright since the inventory jobs can handle duplicate values.
        neededObjects.Add(tileObjectType);
        CheckAreaInventoryJobs(mainStorage, null);
    }
    public void RemoveNeededItems(TILE_OBJECT_TYPE tileObjectType) {
        neededObjects.Remove(tileObjectType);
    }
    #endregion

    #region Party Quests
    public void OnFinishedQuest(PartyQuest quest) {
        migrationComponent.OnFinishedQuest(quest);
    }
    #endregion

    #region IPlayerActionTarget
    public override void ConstructDefaultActions() {
        base.ConstructDefaultActions();
        AddPlayerAction(PLAYER_SKILL_TYPE.SCHEME);
        // AddPlayerAction(PLAYER_SKILL_TYPE.INDUCE_MIGRATION);
        // AddPlayerAction(PLAYER_SKILL_TYPE.STIFLE_MIGRATION);
    }
    #endregion

    public override string ToString() {
        return name;
    }
}