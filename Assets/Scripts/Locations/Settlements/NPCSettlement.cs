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
    //public LocationStructure exterminateTargetStructure { get; private set; }
    public override SettlementResources SettlementResources => m_settlementResources ?? (m_settlementResources = new SettlementResources());

    //structures
    public List<JobQueueItem> availableJobs { get; }
    public LocationEventManager eventManager { get; private set; }
    public SettlementType settlementType { get; private set; }
    public GameDate plaguedExpiryDate { get; private set; }
    //public SettlementClassTracker settlementClassTracker { get; }//Removed this, data moved to SettlementClassComponent
    public NPCSettlementEventDispatcher npcSettlementEventDispatcher { get; }
    public bool hasPeasants { get; private set; }
    public bool hasWorkers { get; private set; }
    /// <summary>
    /// What village spot does this village occupy?
    /// Note: At the time of making, this variable is only set once and is never cleared since Villages City Center cannot be destroyed.
    /// Note: This value is only set on VILLAGE type settlements.
    /// </summary>
    public VillageSpot occupiedVillageSpot { get; private set; }

    //Components
    public SettlementJobTriggerComponent settlementJobTriggerComponent { get; }
    //public SettlementJobPriorityComponent jobPriorityComponent { get; }
    public SettlementVillageMigrationComponent migrationComponent { get; private set; }
    public SettlementResourcesComponent resourcesComponent { get; private set; }
    public SettlementClassComponent classComponent { get; private set; }
    public SettlementPartyComponent partyComponent { get; private set; }
    public SettlementStructureComponent structureComponent { get; private set; }

    private readonly Region _region;
    private readonly WeightedDictionary<Character> newRulerDesignationWeights;
    private int newRulerDesignationChance;
    private string _plaguedExpiryKey;
    private readonly List<TILE_OBJECT_TYPE> _neededObjects;
    private SettlementResources m_settlementResources;

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
        //jobPriorityComponent = new SettlementJobPriorityComponent(this);
        settlementJobTriggerComponent = new SettlementJobTriggerComponent(this);
        //settlementClassTracker = new SettlementClassTracker();
        npcSettlementEventDispatcher = new NPCSettlementEventDispatcher();
        _plaguedExpiryKey = string.Empty;
        _neededObjects = new List<TILE_OBJECT_TYPE>() {
            // TILE_OBJECT_TYPE.HEALING_POTION,
            // TILE_OBJECT_TYPE.TOOL,
            // TILE_OBJECT_TYPE.ANTIDOTE
        };

        migrationComponent = new SettlementVillageMigrationComponent(); migrationComponent.SetOwner(this);
        resourcesComponent = new SettlementResourcesComponent(); resourcesComponent.SetOwner(this);
        classComponent = new SettlementClassComponent(); classComponent.SetOwner(this);
        classComponent.InitialMorningScheduleProcessingOfNeededClasses();
        classComponent.InitialAfternoonScheduleProcessingOfNeededClasses();
        partyComponent = new SettlementPartyComponent(); partyComponent.SetOwner(this);
        partyComponent.InitialScheduleProcessingOfPartyQuests();
        structureComponent = new SettlementStructureComponent(); structureComponent.SetOwner(this);
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
        //jobPriorityComponent = new SettlementJobPriorityComponent(this);
        settlementJobTriggerComponent = new SettlementJobTriggerComponent(this);
        //settlementClassTracker = new SettlementClassTracker(saveData.classTracker);
        npcSettlementEventDispatcher = new NPCSettlementEventDispatcher();
        _plaguedExpiryKey = string.Empty;
        _neededObjects = new List<TILE_OBJECT_TYPE>(saveData.neededObjects);

        migrationComponent = saveData.migrationComponent.Load(); migrationComponent.SetOwner(this);
        resourcesComponent = saveData.resourcesComponent.Load(); resourcesComponent.SetOwner(this);
        classComponent = saveData.classComponent.Load(); classComponent.SetOwner(this);
        partyComponent = saveData.partyComponent.Load(); partyComponent.SetOwner(this);
        structureComponent = saveData.structureComponent.Load(); structureComponent.SetOwner(this);
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
            migrationComponent.LoadReferences(saveDataNpcSettlement);
            resourcesComponent.LoadReferences(saveDataNpcSettlement.resourcesComponent);
            classComponent.LoadReferences(saveDataNpcSettlement.classComponent);
            partyComponent.LoadReferences(saveDataNpcSettlement.partyComponent);
            structureComponent.LoadReferences(saveDataNpcSettlement.structureComponent);

            if (saveDataNpcSettlement.hasOccupiedVillageSpot) {
                Area area = GameUtilities.GetHexTileGivenCoordinates(saveDataNpcSettlement.occupiedVillageSpot, GridMap.Instance.map);
                Assert.IsNotNull(area, $"{name}'s save data has occupied village spot but no area is at {saveDataNpcSettlement.occupiedVillageSpot.ToString()}");
                occupiedVillageSpot = region.GetVillageSpotOnArea(area);
                Assert.IsNotNull(occupiedVillageSpot, $"{name}'s save data has occupied village spot but no village spot could be found on {area}");
            }
        }
    }
    private void LoadJobs(SaveDataNPCSettlement data) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            job.ForceCancelJob();
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
        //settlementJobTriggerComponent.HookToSettlementClassTrackerEvents(settlementClassTracker);
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
        //settlementJobTriggerComponent.UnHookToSettlementClassTrackerEvents(settlementClassTracker);
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
        //onSettlementBuilt?.Invoke();
    }
    protected override void SettlementWipedOut() {
        base.SettlementWipedOut();
        UnsubscribeToSignals();
        eventManager.OnSettlementDestroyed();
    }
    private void SetIsUnderSiege(bool state) {
        if(isUnderSiege != state) {
            isUnderSiege = state;
#if DEBUG_LOG
            Debug.Log($"{GameManager.Instance.TodayLogString()}{name} Under Siege state changed to {isUnderSiege.ToString()}");
#endif
            Messenger.Broadcast(SettlementSignals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, this, isUnderSiege);
            //if (!isUnderSiege) {
            //    if(exterminateTargetStructure != null) {
            //        if(owner != null && !owner.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Extermination, exterminateTargetStructure)) {
            //            if(exterminateTargetStructure.settlementLocation == null || exterminateTargetStructure.settlementLocation.HasResidentThatIsNotDeadThatIsHostileWithFaction(owner)) {
            //                owner.partyQuestBoard.CreateExterminatePartyQuest(null, this, exterminateTargetStructure, this);
            //            }
            //        }
            //    }
            //}
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
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}{name} Plagued state changed to {isPlagued.ToString()}");
#endif
        
    }
    private void OnTickEnded() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"Settlement On Tick Ended");
#endif
        ProcessForcedCancelJobsOnTickEnded();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void OnDayStarted() {
        hasTriedToStealCorpse = false;
        ClearAllBlacklistToAllExistingJobs();
    }
    private void OnHourStarted() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"{name} settlement OnHourStarted");
#endif
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
                            TIME_IN_WORDS currentTime = GameManager.Instance.GetCurrentTimeInWordsOfTick();
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
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void OnSettlementAbandoned() {
        if (!GameManager.Instance.gameHasStarted) {
            return;
        }
        structureComponent.RelinkAllLinkedStructures();
    }
    private void OnSettlementUnabandoned() {
        if (!GameManager.Instance.gameHasStarted) {
            return;
        }
        region.LinkAllUnlinkedSpecialStructures();
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
            classComponent.OnResidentChangedClass(previousClass.className, character);
            //jobPriorityComponent.ChangeClassResidentResetPrimaryJob(character);
        }
    }
    public override bool AddResident(Character character, LocationStructure chosenHome = null, bool ignoreCapacity = true) {
        if (base.AddResident(character, chosenHome, ignoreCapacity)) {
            //region.AddResident(character);
            OnAddResident(character);
            character.SetHomeSettlement(this);
            if (residents.Count == 1) {
                OnSettlementUnabandoned();
            }
            //if (character.race == RACE.DEMON || character is Summon) { return true; }
            //if (character.isNormalCharacter && locationType == LOCATION_TYPE.VILLAGE) {
            //    jobPriorityComponent.OnAddResident(character);    
            //}
            return true;
        }
        return false;
    }
    public override bool RemoveResident(Character character) {
        if (base.RemoveResident(character)) {
            //region.RemoveResident(character);
            character.SetHomeSettlement(null);
            OnRemoveResident(character);
            //if (character.isNormalCharacter && locationType == LOCATION_TYPE.VILLAGE) {
            //    jobPriorityComponent.OnRemoveResident(character);
            //}
            UnassignJobsTakenBy(character);
            if (residents.Count <= 0) {
                OnSettlementAbandoned();
            }
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
                previousRuler.jobComponent.RemoveAbleJob(JOB_TYPE.JUDGE_PRISONER);
                previousRuler.jobComponent.RemoveAbleJob(JOB_TYPE.PLACE_BLUEPRINT);
            }
        }
        if(ruler != null) {
            ruler.behaviourComponent.AddBehaviourComponent(typeof(SettlementRulerBehaviour));
            ruler.jobComponent.AddAbleJob(JOB_TYPE.JUDGE_PRISONER);
            ruler.jobComponent.AddAbleJob(JOB_TYPE.PLACE_BLUEPRINT);
            //ResetNewRulerDesignationChance();
            Messenger.Broadcast(CharacterSignals.ON_SET_AS_SETTLEMENT_RULER, ruler, previousRuler);
        } else {
            Messenger.Broadcast(CharacterSignals.ON_SETTLEMENT_RULER_REMOVED, this, previousRuler);
        }
        npcSettlementEventDispatcher.ExecuteSettlementRulerChangedEvent(newRuler, this);
    }
    private void CheckForNewRulerDesignation() {
        int chance = Random.Range(0, 100);
#if DEBUG_LOG
        string debugLog =
            $"{GameManager.Instance.TodayLogString()}Checking for new npcSettlement ruler designation for {name}";
        debugLog += $"\n-Chance: {newRulerDesignationChance.ToString()}";
        debugLog += $"\n-Roll: {chance.ToString()}";
        Debug.Log(debugLog);
#endif
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
#if DEBUG_LOG
        string log = $"{GameManager.Instance.TodayLogString()}Designating a new npcSettlement ruler for: {region.name}(chance it triggered: {newRulerDesignationChance})";
#endif
        newRulerDesignationWeights.Clear();
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            if(resident.faction != owner) {
                continue;
            }
#if DEBUG_LOG
            log += $"\n\n-{resident.name}";
#endif
            if (resident.isDead /*|| resident.isMissing*/ || resident.isBeingSeized) {
#if DEBUG_LOG
                log += "\nEither dead or missing or seized or enslaved, will not be part of candidates for ruler";
#endif
                continue;
            }

            if (owner != null && resident.crimeComponent.IsWantedBy(owner)) {
#if DEBUG_LOG
                log += "\nMember is wanted by the faction owner of this settlement " + owner.name + ", skipping...";
#endif
                continue;
            }
            bool isInsideSettlement = resident.gridTileLocation != null && resident.gridTileLocation.IsPartOfSettlement(this);
            bool isInAnActiveParty = resident.partyComponent.isMemberThatJoinedQuest;

            if(!isInsideSettlement && !isInAnActiveParty) {
#if DEBUG_LOG
                log += "\nMember is not inside settlement and not in active party, skipping...";
#endif
                continue;
            }

            int weight = 50;
#if DEBUG_LOG
            log += "\n  -Base Weight: +50";
#endif
            if (owner != null && owner.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Vampires)) {
                Vampire vampire = resident.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                if (vampire != null && vampire.DoesFactionKnowThisVampire(owner)) {
                    weight += 100;
#if DEBUG_LOG
                    log += "\n  -Faction reveres vampires and member is a known vampire: +100";
#endif
                }
            }
            if (owner != null && owner.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves)) {
                if (resident.isLycanthrope && resident.lycanData.DoesFactionKnowThisLycan(owner)) {
                    weight += 100;
#if DEBUG_LOG
                    log += "\n  -Faction reveres werewolves and member is a known Lycanthrope: +100";
#endif
                }
            }
            if (resident.isFactionLeader) {
                weight += 100;
#if DEBUG_LOG
                log += "\n  -Faction Leader: +100";
#endif
            }
            if (resident.characterClass.className == "Noble") {
                weight += 40;
#if DEBUG_LOG
                log += "\n  -Noble: +40";
#endif
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
#if DEBUG_LOG
                log += $"\n  -Num of Friend/Close Friend in the NPCSettlement: {numberOfFriends}, +{weightToAdd}";
#endif
            }

            // if(numberOfFriends > 0) {
            //     weight += (numberOfFriends * 20);
            //     log +=
            //         $"\n  -Num of Friend/Close Friend in the NPCSettlement: {numberOfFriends}, +{(numberOfFriends * 20)}";
            // }
            if (resident.traitContainer.HasTrait("Inspiring")) {
                weight += 25;
#if DEBUG_LOG
                log += "\n  -Inspiring: +25";
#endif
            }
            if (resident.traitContainer.HasTrait("Authoritative")) {
                weight += 50;
#if DEBUG_LOG
                log += "\n  -Authoritative: +50";
#endif
            }


            if (numberOfEnemies > 0) {
                weight += (numberOfEnemies * -10);
#if DEBUG_LOG
                log += $"\n  -Num of Enemies/Rivals in the NPCSettlement: {numberOfEnemies}, +{(numberOfEnemies * -10)}";
#endif
            }
            if (resident.traitContainer.HasTrait("Unattractive")) {
                weight += -20;
#if DEBUG_LOG
                log += "\n  -Unattractive: -20";
#endif
            }
            if (resident.hasUnresolvedCrime) {
                weight += -50;
#if DEBUG_LOG
                log += "\n  -Has Unresolved Crime: -50";
#endif
            }
            if (resident.traitContainer.HasTrait("Worker")) {
                weight += -40;
#if DEBUG_LOG
                log += "\n  -Civilian: -40";
#endif
            }
            if (weight < 1) {
                weight = 1;
#if DEBUG_LOG
                log += "\n  -Weight cannot be less than 1, setting weight to 1";
#endif
            }
            if (resident.traitContainer.HasTrait("Ambitious")) {
                weight = Mathf.RoundToInt(weight * 1.5f);
#if DEBUG_LOG
                log += "\n  -Ambitious: x1.5";
#endif
            }
            if (resident is Summon || resident.characterClass.IsZombie()) {
                if(HasResidentThatIsSapientAndInsideSettlementOrHasJoinedQuest()) {
                    weight *= 0;
#if DEBUG_LOG
                    log += "\n  -Resident is a Summon and there is atleast 1 Sapient resident inside settlement or in active party: x0";
#endif
                }
            }
            if (resident.traitContainer.HasTrait("Enslaved")) {
                weight *= 0;
#if DEBUG_LOG
                log += "\n  -Enslaved: x0";
#endif
            }
#if DEBUG_LOG
            log += $"\n  -TOTAL WEIGHT: {weight}";
#endif
            if (weight > 0) {
                newRulerDesignationWeights.AddElement(resident, weight);
            }
        }
        if(newRulerDesignationWeights.Count > 0) {
            Character chosenRuler = newRulerDesignationWeights.PickRandomElementGivenWeights();
            if (chosenRuler != null) {
#if DEBUG_LOG
                log += $"\nCHOSEN RULER: {chosenRuler.name}";
#endif
                if (willLog) {
                    chosenRuler.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Settlement_Ruler, chosenRuler);
                } else {
                    SetRuler(chosenRuler);
                }
            } else {
#if DEBUG_LOG
                log += "\nCHOSEN RULER: NONE";
#endif
            }
        } else {
#if DEBUG_LOG
            log += "\nCHOSEN RULER: NONE";
#endif
        }
        ResetNewRulerDesignationChance();
#if DEBUG_LOG
        Debug.Log(log);
#endif
    }
    private void ResetNewRulerDesignationChance() {
        newRulerDesignationChance = 5;
    }
    public Character GetFirstHostileCharacterInSettlement() {
        for (int i = 0; i < region.charactersAtLocation.Count; i++) {
            Character character = region.charactersAtLocation[i];
            if(character.reactionComponent.disguisedCharacter != null) {
                character = character.reactionComponent.disguisedCharacter;
            }
            if (!character.isDead && character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(this)
            && owner.IsHostileWith(character.faction) 
            && !character.traitContainer.HasTrait("Restrained")
            && character.combatComponent.combatMode != COMBAT_MODE.Passive
            && !character.traitContainer.HasTrait("Enslaved")) {
                return character;
            }
        }
        return null;
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
        classComponent.OnResidentAdded(character);
        if (residents.Count == 1 && locationType == LOCATION_TYPE.VILLAGE && GameManager.Instance.gameHasStarted) {
            //First resident
            ChangeSettlementTypeAccordingTo(character);
        }
        //bool hasUpdatedPeasantSwitch = false;
        //if (character.characterClass.className == "Peasant") {
        //    hasUpdatedPeasantSwitch = UpdateHasPeasants();
        //}
        //bool hasUpdatedWorkerSwitch = false;
        //if (character.traitContainer.HasTrait("Worker")) {
        //    hasUpdatedWorkerSwitch = UpdateHasWorkers();
        //}
        //if (!hasUpdatedPeasantSwitch && !hasUpdatedWorkerSwitch) {
        //    //if neither the switches was updated, manually call the update resident able jobs since it is possible that one of the switches are already off
        //    //and not calling it will result in the new residents able jobs not being updated.
        //    UpdateAbleJobsOfResident(character);
        //}
    }
    private void OnRemoveResident(Character character) {
        eventManager.OnResidentRemoved(character);
        classComponent.OnResidentRemoved(character);
        //UnapplyAbleJobsFromSettlement(character);
        //if (character.characterClass.className == "Peasant") {
        //    UpdateHasPeasants();
        //}
        //if (character.traitContainer.HasTrait("Worker")) {
        //    UpdateHasWorkers();
        //}
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
    public bool HasResidentThatIsOrCanBecomeClass(string p_className) {
        for (int i = 0; i < residents.Count; i++) {
            Character character = residents[i];
            if (character.characterClass.className == p_className || character.classComponent.HasAbleClass(p_className)) {
                return true;
            }
        }
        return false;
    }

    public void PopulateResidentsCurrentlyInsideVillage(List<Character> p_characters) {
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            if (resident.currentSettlement == this) {
                p_characters.Add(resident);
            }
        }
    }
    public void PopulateAbleClassesOfAllResidents(List<string> p_ableClasses) {
        for (int i = 0; i < residents.Count; i++) {
            Character resident = residents[i];
            for (int j = 0; j < resident.classComponent.ableClasses.Count; j++) {
                string ableClass = resident.classComponent.ableClasses[j];
                if (!p_ableClasses.Contains(ableClass)) {
                    p_ableClasses.Add(ableClass);
                }
            }
        }
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
            if (mainStorage.GetNumberOfBuiltTileObjects(item.tileObjectType) >= 2) {
                List<JobQueueItem> jobs = RuinarchListPool<JobQueueItem>.Claim();
                PopulateJobsOfType(jobs, JOB_TYPE.CRAFT_OBJECT);
                for (int i = 0; i < jobs.Count; i++) {
                    JobQueueItem jqi = jobs[i];
                    if (jqi is GoapPlanJob goapPlanJob && goapPlanJob.targetPOI is TileObject tileObject && tileObject.tileObjectType == item.tileObjectType) {
                        jqi.ForceCancelJob("Settlement has enough");    
                    }
                }
                RuinarchListPool<JobQueueItem>.Release(jobs);
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
                        //if(target.homeStructure != null 
                        //   && target.homeStructure.settlementLocation != null 
                        //   && target.homeStructure.settlementLocation.locationType == LOCATION_TYPE.DUNGEON
                        //   && exterminateTargetStructure == null) {
                        //    exterminateTargetStructure = target.homeStructure;
                        //}
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
    //public void SetExterminateTarget(LocationStructure target) {
    //    exterminateTargetStructure = target;
    //}
    public StructureSetting GetMissingFacilityToBuildBasedOnWeights() {
        WeightedDictionary<StructureSetting> facilityWeights = new WeightedDictionary<StructureSetting>(settlementType.facilityWeights.dictionary);
        foreach (var kvp in settlementType.facilityWeights.dictionary) {
            int cap = settlementType.GetFacilityCap(kvp.Key);
            int currentAmount = GetStructureCount(kvp.Key.structureType);
            SettlementResources.StructureRequirement required = kvp.Key.structureType.GetRequiredObjectForBuilding();
            if (currentAmount >= cap || !m_settlementResources.IsRequirementAvailable(required, this)) {
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
        return HasStructure(STRUCTURE_TYPE.HUNTER_LODGE, STRUCTURE_TYPE.FARM, STRUCTURE_TYPE.FISHERY);
    }
    public bool HasBasicResourceProducingStructure() {
        if (owner != null) {
            if (owner.factionType.type == FACTION_TYPE.Human_Empire) {
                return HasStructure(STRUCTURE_TYPE.MINE);
            } else if (owner.factionType.type == FACTION_TYPE.Elven_Kingdom) {
                return HasStructure(STRUCTURE_TYPE.LUMBERYARD);
            } else if (owner.factionType.type == FACTION_TYPE.Demon_Cult || owner.factionType.type == FACTION_TYPE.Lycan_Clan || owner.factionType.type == FACTION_TYPE.Vampire_Clan) {
                return HasStructure(STRUCTURE_TYPE.MINE) || HasStructure(STRUCTURE_TYPE.LUMBERYARD);
            }
        }
        return false;
    }

    public void PopulateStructureConnectorsForStructureType(List<StructureConnector> p_connectors, STRUCTURE_TYPE p_structureType) {
        switch (p_structureType) {
            case STRUCTURE_TYPE.FISHERY:
                PopulateAvailableFishingSpotConnectors(p_connectors);
                break;
            case STRUCTURE_TYPE.LUMBERYARD:
                PopulateAvailableTreeConnectors(p_connectors);
                break;
            case STRUCTURE_TYPE.MINE:
                PopulateAvailableMineShackConnectors(p_connectors);
                break;
            default:
                PopulateAvailableStructureConnectors(p_connectors);
                break;
        } 
    }
    private void PopulateAvailableStructureConnectors(List<StructureConnector> connectors) {
        for (int i = 0; i < allStructures.Count; i++) {
            LocationStructure structure = allStructures[i];
            if (structure is ManMadeStructure manMadeStructure && manMadeStructure.structureObj != null) {
                for (int j = 0; j < manMadeStructure.structureObj.connectors.Length; j++) {
                    StructureConnector connector = manMadeStructure.structureObj.connectors[j];
                    //Limit village expansion to within reserved spots
                    //Reference: https://trello.com/c/qBvoisWj/4699-world-gen-updates
                    if (connector.isOpen) {
                        bool shouldAddConnector;
                        if (connector.tileLocation != null) {
                            shouldAddConnector = occupiedVillageSpot.reservedAreas.Contains(connector.tileLocation.area);
                        } else {
                            LocationGridTile tileLocation = connector.GetLocationGridTileGivenCurrentPosition(region.innerMap);
                            shouldAddConnector = tileLocation != null && occupiedVillageSpot.reservedAreas.Contains(tileLocation.area);
                        }
                        if (shouldAddConnector) {
                            connectors.Add(connector);    
                        }
                    }
                }
            }
        }
    }
    private void PopulateAvailableTreeConnectors(List<StructureConnector> connectors) {
        List<TileObject> allTrees = RuinarchListPool<TileObject>.Claim();
        SettlementResources.PopulateAllTrees(allTrees, this);
        for (int i = 0; i < allTrees.Count; i++) {
            TreeObject treeObject = allTrees[i] as TreeObject;
            if (treeObject.structureConnector != null && treeObject.structureConnector.isOpen) {
                connectors.Add(treeObject.structureConnector);
            }
        }
        for (int i = 0; i < occupiedVillageSpot.reservedAreas.Count; i++) {
            Area area = occupiedVillageSpot.reservedAreas[i];
            for (int j = 0; j < area.tileObjectComponent.trees.Count; j++) {
                TreeObject treeObject = area.tileObjectComponent.trees[j];
                if (treeObject.structureConnector != null && treeObject.structureConnector.isOpen &&
                    !connectors.Contains(treeObject.structureConnector)) {
                    connectors.Add(treeObject.structureConnector);
                }
            }
        }
        RuinarchListPool<TileObject>.Release(allTrees);
    }
    private void PopulateAvailableFishingSpotConnectors(List<StructureConnector> connectors) {
        List<TileObject> allFishingSpots = RuinarchListPool<TileObject>.Claim();
        SettlementResources.PopulateAllFishingSpots(allFishingSpots, this);
        for (int i = 0; i < allFishingSpots.Count; i++) {
            FishingSpot fishingSpot = allFishingSpots[i] as FishingSpot;
            if (fishingSpot.structureConnector != null && fishingSpot.structureConnector.isOpen) {
                connectors.Add(fishingSpot.structureConnector);
            }
        }
        for (int i = 0; i < occupiedVillageSpot.reservedAreas.Count; i++) {
            Area area = occupiedVillageSpot.reservedAreas[i];
            for (int j = 0; j < area.tileObjectComponent.fishingSpots.Count; j++) {
                FishingSpot fishingSpot = area.tileObjectComponent.fishingSpots[j];
                if (fishingSpot.structureConnector != null && fishingSpot.structureConnector.isOpen &&
                    !connectors.Contains(fishingSpot.structureConnector)) {
                    connectors.Add(fishingSpot.structureConnector);
                }
            }
        }
        RuinarchListPool<TileObject>.Release(allFishingSpots);
    }
    private void PopulateAvailableMineShackConnectors(List<StructureConnector> connectors) {
        List<LocationGridTile> allMineShackSpots = RuinarchListPool<LocationGridTile>.Claim();
        SettlementResources.PopulateAllMineShackSpots(allMineShackSpots, this);
        for (int i = 0; i < allMineShackSpots.Count; i++) {
            LocationGridTile oreVein = allMineShackSpots[i];
            if (oreVein.structure is Cave cave && oreVein.tileObjectComponent.genericTileObject.structureConnector != null && 
                oreVein.tileObjectComponent.genericTileObject.structureConnector.isOpen && !cave.IsConnectedToSettlement(this)) {
                connectors.Add(oreVein.tileObjectComponent.genericTileObject.structureConnector);
            }
        }
        for (int i = 0; i < occupiedVillageSpot.reservedAreas.Count; i++) {
            Area area = occupiedVillageSpot.reservedAreas[i];
            for (int j = 0; j < area.structureComponent.structureConnectors.Count; j++) {
                StructureConnector connector = area.structureComponent.structureConnectors[j];
                if (connector.tileLocation != null && connector.isOpen && !connectors.Contains(connector) && 
                    connector.tileLocation.structure is Cave cave && !cave.IsConnectedToSettlement(this)) {
                    connectors.Add(connector);
                }
            }
        }
        RuinarchListPool<LocationGridTile>.Release(allMineShackSpots);
    }
    public bool HasReservedSpotWithFeature(string p_feature) {
        for (int i = 0; i < occupiedVillageSpot.reservedAreas.Count; i++) {
            Area area = occupiedVillageSpot.reservedAreas[i];
            if (area.featureComponent.HasFeature(p_feature)) {
                return true;
            }
        }
        return false;
    }

    public bool HasStructureOfTypeThatIsAssigned(STRUCTURE_TYPE p_type) {
        List<LocationStructure> s = GetStructuresOfType(p_type);
        if (s != null) {
            for (int i = 0; i < s.Count; i++) {
                LocationStructure structure = s[i];
                if (structure is ManMadeStructure manMadeStructure && manMadeStructure.HasAssignedWorker()) {
                    return true;
                }
            }
        }

        return false;
    }
    public LocationStructure GetFirstStructureOfTypeThatCanAcceptWorkerAndIsNotReserved(STRUCTURE_TYPE type) {
        if (HasStructure(type)) {
            List<LocationStructure> structuresOfType = structures[type];
            if (structuresOfType != null && structuresOfType.Count > 0) {
                for (int i = 0; i < structuresOfType.Count; i++) {
                    ManMadeStructure s = structuresOfType[i] as ManMadeStructure;
                    if (s.CanHireAWorker()) {
                        if (!availableJobs.HasJobWithOtherData(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, s)) {
                            return s;
                        }
                    }
                }
            }
        }
        return null;
    }
    public LocationStructure GetFirstStructureOfTypeThatHasNoWorkerAndIsNotReserved(STRUCTURE_TYPE type) {
        if (HasStructure(type)) {
            List<LocationStructure> structuresOfType = structures[type];
            if (structuresOfType != null && structuresOfType.Count > 0) {
                for (int i = 0; i < structuresOfType.Count; i++) {
                    ManMadeStructure s = structuresOfType[i] as ManMadeStructure;
                    if (!s.HasAssignedWorker()) {
                        if (!availableJobs.HasJobWithOtherData(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, s)) {
                            return s;
                        }
                    }
                }
            }
        }
        return null;
    }
    public LocationStructure GetFirstStructureOfTypeThatHasWorkerOrIsReserved(STRUCTURE_TYPE type) {
        if (HasStructure(type)) {
            List<LocationStructure> structuresOfType = structures[type];
            if (structuresOfType != null && structuresOfType.Count > 0) {
                for (int i = 0; i < structuresOfType.Count; i++) {
                    ManMadeStructure s = structuresOfType[i] as ManMadeStructure;
                    if (s.HasAssignedWorker() || availableJobs.HasJobWithOtherData(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, s)) {
                        return s;
                    }
                }
            }
        }
        return null;
    }
    public LocationStructure GetRandomStructureOfTypeThatCanAcceptWorkerAndIsNotReserved(STRUCTURE_TYPE type) {
        if (HasStructure(type)) {
            List<LocationStructure> structuresOfType = structures[type];
            if (structuresOfType != null && structuresOfType.Count > 0) {
                List<LocationStructure> assignedStructures = RuinarchListPool<LocationStructure>.Claim();
                List<LocationStructure> unAssignedStructures = RuinarchListPool<LocationStructure>.Claim();
                for (int i = 0; i < structuresOfType.Count; i++) {
                    ManMadeStructure s = structuresOfType[i] as ManMadeStructure;
                    if (s.CanHireAWorker()) {
                        if (!availableJobs.HasJobWithOtherData(JOB_TYPE.CHANGE_CLASS, INTERACTION_TYPE.CHANGE_CLASS, s)) {
                            if (s.HasAssignedWorker()) {
                                assignedStructures.Add(s);    
                            } else {
                                unAssignedStructures.Add(s);
                            }
                        }
                    }
                }
                if (unAssignedStructures.Count > 0) {
                    LocationStructure chosenStructure = CollectionUtilities.GetRandomElement(unAssignedStructures);
                    RuinarchListPool<LocationStructure>.Release(assignedStructures);
                    RuinarchListPool<LocationStructure>.Release(unAssignedStructures);
                    return chosenStructure;
                }
                if (assignedStructures.Count > 0) {
                    LocationStructure chosenStructure = CollectionUtilities.GetRandomElement(assignedStructures);
                    RuinarchListPool<LocationStructure>.Release(assignedStructures);
                    RuinarchListPool<LocationStructure>.Release(unAssignedStructures);
                    return chosenStructure;
                }
                RuinarchListPool<LocationStructure>.Release(assignedStructures);
                RuinarchListPool<LocationStructure>.Release(unAssignedStructures);
            }
            
        }
        return null;
    }
    public bool HasBlueprintOnTileForStructure(STRUCTURE_TYPE p_type) {
        for (int i = 0; i < availableJobs.Count; i++) {
            GoapPlanJob job = availableJobs[i] as GoapPlanJob;
            if (job != null && job.jobType == JOB_TYPE.BUILD_BLUEPRINT) {
                GenericTileObject jobTarget = job.poiTarget as GenericTileObject;
                if (jobTarget != null && jobTarget.blueprintOnTile != null && jobTarget.blueprintOnTile.structureType == p_type) {
                    return true;
                }
            }
        }
        return false;
    }
    public int GetNumberOfBlueprintOnTileForStructure(STRUCTURE_TYPE p_type) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            GoapPlanJob job = availableJobs[i] as GoapPlanJob;
            if (job != null && job.jobType == JOB_TYPE.BUILD_BLUEPRINT) {
                GenericTileObject jobTarget = job.poiTarget as GenericTileObject;
                if (jobTarget != null && jobTarget.blueprintOnTile != null && jobTarget.blueprintOnTile.structureType == p_type) {
                    count++;
                }
            }
        }
        return count;
    }
    #endregion

    #region Inner Map
    public IEnumerator PlaceInitialObjectsForWorldGenCoroutine() {
        if (HasStructure(STRUCTURE_TYPE.LUMBERYARD)) {
            List<LocationStructure> lumberyards = GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
            for (int i = 0; i < lumberyards.Count; i++) {
                LocationStructure lumberyard = lumberyards[i];
                WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
                woodPile.SetResourceInPile(GameUtilities.RandomBetweenTwoNumbers(50, 100));
                lumberyard.AddPOI(woodPile);
            }
        }
        if (HasStructure(STRUCTURE_TYPE.MINE)) {
            List<LocationStructure> mines = GetStructuresOfType(STRUCTURE_TYPE.MINE);
            for (int i = 0; i < mines.Count; i++) {
                LocationStructure lumberyard = mines[i];
                StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
                stonePile.SetResourceInPile(GameUtilities.RandomBetweenTwoNumbers(50, 100));
                lumberyard.AddPOI(stonePile);
            }
        }
        List<TILE_OBJECT_TYPE> spawnedFoodTypes = RuinarchListPool<TILE_OBJECT_TYPE>.Claim();
        if (HasStructure(STRUCTURE_TYPE.FARM)) {
            List<LocationStructure> farms = GetStructuresOfType(STRUCTURE_TYPE.FARM);
            for (int i = 0; i < farms.Count; i++) {
                Farm farm = farms[i] as Farm;
                List<TILE_OBJECT_TYPE> cropChoices = RuinarchListPool<TILE_OBJECT_TYPE>.Claim();
                for (int j = 0; j < farm.farmTiles.Count; j++) {
                    LocationGridTile farmTile = farm.farmTiles[j];
                    if (farmTile.tileObjectComponent.objHere is Crops crops && 
                        !cropChoices.Contains(crops.producedObjectOnHarvest)) {
                        cropChoices.Add(crops.producedObjectOnHarvest);
                    }
                }
                TILE_OBJECT_TYPE chosenCrop = CollectionUtilities.GetRandomElement(cropChoices);
                if (!spawnedFoodTypes.Contains(chosenCrop)) {
                    spawnedFoodTypes.Add(chosenCrop);
                }
                RuinarchListPool<TILE_OBJECT_TYPE>.Release(cropChoices);
                FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(chosenCrop);
                foodPile.SetResourceInPile(GameUtilities.RandomBetweenTwoNumbers(50, 100));
                farm.AddPOI(foodPile);
            }
        }
        if (HasStructure(STRUCTURE_TYPE.FISHERY)) {
            spawnedFoodTypes.Add(TILE_OBJECT_TYPE.FISH_PILE);
            List<LocationStructure> fisheries = GetStructuresOfType(STRUCTURE_TYPE.FISHERY);
            for (int i = 0; i < fisheries.Count; i++) {
                LocationStructure fishery = fisheries[i];
                FishPile stonePile = InnerMapManager.Instance.CreateNewTileObject<FishPile>(TILE_OBJECT_TYPE.FISH_PILE);
                stonePile.SetResourceInPile(GameUtilities.RandomBetweenTwoNumbers(50, 100));
                fishery.AddPOI(stonePile);
            }
        }
        if (HasStructure(STRUCTURE_TYPE.BUTCHERS_SHOP)) {
            spawnedFoodTypes.Add(TILE_OBJECT_TYPE.ANIMAL_MEAT);
            List<LocationStructure> butcherShops = GetStructuresOfType(STRUCTURE_TYPE.BUTCHERS_SHOP);
            for (int i = 0; i < butcherShops.Count; i++) {
                LocationStructure butcherShop = butcherShops[i];
                AnimalMeat animalMeat = InnerMapManager.Instance.CreateNewTileObject<AnimalMeat>(TILE_OBJECT_TYPE.ANIMAL_MEAT);
                animalMeat.SetResourceInPile(GameUtilities.RandomBetweenTwoNumbers(50, 100));
                butcherShop.AddPOI(animalMeat);
            }
        }
        if (HasStructure(STRUCTURE_TYPE.DWELLING) && spawnedFoodTypes.Count > 0) {
            List<LocationStructure> dwellings = GetStructuresOfType(STRUCTURE_TYPE.DWELLING);
            for (int i = 0; i < dwellings.Count; i++) {
                LocationStructure dwelling = dwellings[i];
                TILE_OBJECT_TYPE randomFood = CollectionUtilities.GetRandomElement(spawnedFoodTypes);
                if (GameUtilities.RollChance(50)) {
                    FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(randomFood);
                    foodPile.SetResourceInPile(GameUtilities.RandomBetweenTwoNumbers(20, 60));
                    dwelling.AddPOI(foodPile);    
                }
                Table tableInDwelling = dwelling.GetTileObjectOfType<Table>();
                if (tableInDwelling != null) {
                    tableInDwelling.resourceStorageComponent.SetResource(CONCRETE_RESOURCES.Animal_Meat, 0);
                    CONCRETE_RESOURCES chosenFoodType = CONCRETE_RESOURCES.Animal_Meat;
                    switch (randomFood) {
                        case TILE_OBJECT_TYPE.CORN:
                            chosenFoodType = CONCRETE_RESOURCES.Corn;
                            break;
                        case TILE_OBJECT_TYPE.FISH_PILE:
                            chosenFoodType = CONCRETE_RESOURCES.Fish;
                            break;
                        case TILE_OBJECT_TYPE.HYPNO_HERB:
                            chosenFoodType = CONCRETE_RESOURCES.Hypno_Herb;
                            break;
                        case TILE_OBJECT_TYPE.ICEBERRY:
                            chosenFoodType = CONCRETE_RESOURCES.Iceberry;
                            break;
                        case TILE_OBJECT_TYPE.PINEAPPLE:
                            chosenFoodType = CONCRETE_RESOURCES.Pineapple;
                            break;
                        case TILE_OBJECT_TYPE.POTATO:
                            chosenFoodType = CONCRETE_RESOURCES.Potato;
                            break;
                        case TILE_OBJECT_TYPE.VEGETABLES:
                            chosenFoodType = CONCRETE_RESOURCES.Vegetables;
                            break;
                    }
                    tableInDwelling.SetFood(chosenFoodType, UnityEngine.Random.Range(20, 81));
                }
            }
        }
        RuinarchListPool<TILE_OBJECT_TYPE>.Release(spawnedFoodTypes);
        // PlaceResourcePiles();
        yield return null;
    }
    public void PlaceInitialObjects() {
        PlaceResourcePiles();
    }
    private void PlaceResourcePiles() {
        WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
        mainStorage.AddPOI(woodPile);
        woodPile.SetResourceInPile(180);

        StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
        mainStorage.AddPOI(stonePile);
        stonePile.SetResourceInPile(180);

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
                for (int i = 0; i < allStructures.Count; i++) {
                    LocationStructure s = allStructures[i];
                    if (s.structureType != STRUCTURE_TYPE.WILDERNESS) {
                        SetPrison(s);
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
            for (int i = 0; i < allStructures.Count; i++) {
                LocationStructure s = allStructures[i];
                if (s.structureType != STRUCTURE_TYPE.WILDERNESS) {
                    newStorage = s;
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
    public void PopulateTileObjectsFromStructures<T>(List<TileObject> objs, STRUCTURE_TYPE structureType) where T : TileObject {
        if (HasStructure(structureType)) {
            List<LocationStructure> structureList = structures[structureType];
            for (int i = 0; i < structureList.Count; i++) {
                structureList[i].PopulateTileObjectsOfType<T>(objs);
            }
        }
    }
    public T GetFirstTileObjectFromStructuresThatIsUntended<T>(STRUCTURE_TYPE structureType) where T : TileObject {
        if (HasStructure(structureType)) {
            List<LocationStructure> structureList = structures[structureType];
            for (int i = 0; i < structureList.Count; i++) {
                T obj = structureList[i].GetFirstTileObjectsOfTypeThatIsUntended<T>();
                if (obj != null) {
                    return obj;
                }
            }
        }
        return null;
    }
    #endregion

    #region Jobs
    public void AddToAvailableJobs(JobQueueItem job, int position = -1) {
        if (position == -1) {
            availableJobs.Add(job);
        } else {
            availableJobs.Insert(position, job);
        }
#if DEBUG_LOG
        if (job is GoapPlanJob goapJob) {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{goapJob} targeting {goapJob.targetPOI} was added to {name}'s available jobs");
        } else {
            Debug.Log($"{GameManager.Instance.TodayLogString()}{job} was added to {name}'s available jobs");
        }
#endif
    }
    public bool RemoveFromAvailableJobs(JobQueueItem job) {
        if (availableJobs.Remove(job)) {
            if (job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
#if DEBUG_LOG
                Debug.Log($"{GameManager.Instance.TodayLogString()}{goapJob} targeting {goapJob.targetPOI?.name} was removed from {name}'s available jobs");
#endif
            } else {
#if DEBUG_LOG
                Debug.Log($"{GameManager.Instance.TodayLogString()}{job} was removed from {name}'s available jobs");
#endif
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
    public int GetNumberOfJobsThatTargetsTileObjectOfType(TILE_OBJECT_TYPE p_type) {
        int count = 0;
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job is GoapPlanJob goapJob && goapJob.poiTarget is TileObject to && to.tileObjectType == p_type) {
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
    public void PopulateJobsOfType(List<JobQueueItem> jobs, JOB_TYPE jobType) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.jobType == jobType) {
                jobs.Add(job);
            }
        }
    }
    public void PopulateJobsOfType(List<JobQueueItem> jobs, JOB_TYPE jobType1, JOB_TYPE jobType2) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.jobType == jobType1 || job.jobType == jobType2) {
                jobs.Add(job);
            }
        }
    }
    public JobQueueItem GetFirstJobOfTypeThatCanBeAssignedTo(JOB_TYPE jobType, Character p_character) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.jobType == jobType && job.assignedCharacter == null && p_character.jobQueue.CanJobBeAddedToQueue(job)) {
                return job;
            }
        }
        return null;
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
        //JobQueueItem chosenPriorityJob = null;
        //JobQueueItem chosenSecondaryJob = null;
        //JobQueueItem chosenAbleJob = null;

        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.assignedCharacter == null && character.jobQueue.CanJobBeAddedToQueue(job)) {
                ////if (job.jobType == character.jobComponent.primaryJob) {
                ////    return job;
                ////} else 
                //if (chosenPriorityJob == null && (character.jobComponent.ableJobs.Contains(job.jobType) || character.jobComponent.additionalPriorityJobs.Contains(job.jobType))) {
                //    chosenPriorityJob = job;
                //} 
                ////else if (chosenSecondaryJob == null && character.characterClass.secondaryJobs != null && character.characterClass.secondaryJobs.Contains(job.jobType)) {
                ////    chosenSecondaryJob = job;
                ////} 
                //else if (chosenAbleJob == null) {
                //    bool isAble = character.characterClass.ableJobs != null && character.characterClass.ableJobs.Contains(job.jobType);
                //    if (isAble) {
                //        chosenAbleJob = job;    
                //    }
                //}

                if (character.characterClass.CanDoJob(job.jobType) || character.jobComponent.ableJobs.Contains(job.jobType)) {
                    return job;
                }
            }
        }
        //if (chosenPriorityJob != null) {
        //    return chosenPriorityJob;
        //} else if (chosenSecondaryJob != null) {
        //    return chosenSecondaryJob;
        //} else if (chosenAbleJob != null) {
        //    return chosenAbleJob;
        //}
        return null;
    }
    public bool AssignCharacterToJobBasedOnVision(Character character) {
        List<JobQueueItem> choices = RuinarchListPool<JobQueueItem>.Claim();
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
        JobQueueItem chosenJob = null;
        if (choices.Count > 0) {
            chosenJob = CollectionUtilities.GetRandomElement(choices);
        }
        RuinarchListPool<JobQueueItem>.Release(choices);
        return chosenJob != null && character.jobQueue.AddJobInQueue(chosenJob);
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
                int objectsCount = affectedStructure.GetNumberOfTileObjects(neededObject); //This includes unbuilt objects 
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
                    if (goapJob.ForceCancelJob(reason)) {
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
                    if (goapJob.ForceCancelJob(reason)) {
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
    public void ForceCancelJobTypes(JOB_TYPE jobType) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.jobType == jobType && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                AddForcedCancelJobsOnTickEnded(goapJob);
            }
        }
    }
    public void ForceCancelJobTypesImmediately(JOB_TYPE jobType) {
        for (int i = 0; i < availableJobs.Count; i++) {
            JobQueueItem job = availableJobs[i];
            if (job.jobType == jobType && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.ForceCancelJob()) {
                    i--;
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
                goapJob.CancelJob(string.Empty);
            }
        }
    }

    //NOTE: Removed this section since the design has changed, the one that will handles this is now the Change To Needed Class processings
    /// <summary>
    /// Whenever a resident updates its class also update the
    /// hasPeasants and hasWorkers switch
    /// </summary>
    //public void OnResidentUpdatedClass() {
    //    UpdateHasPeasants();
    //    UpdateHasWorkers();
    //}
    /// <summary>
    /// Update the hasPeasants switch.
    /// </summary>
    /// <returns>Whether or not a change happened</returns>
    //private bool UpdateHasPeasants() {
    //    for (int i = 0; i < residents.Count; i++) {
    //        Character resident = residents[i];
    //        if (resident.characterClass.className == "Peasant") {
    //            return SetHasPeasants(true);
    //        }
    //    }
    //    return SetHasPeasants(false);
    //}
    /// <summary>
    /// Update the hasWorkers switch.
    /// </summary>
    /// <returns>Whether or not a change happened</returns>
    //private bool UpdateHasWorkers() {
    //    for (int i = 0; i < residents.Count; i++) {
    //        Character resident = residents[i];
    //        if (resident.traitContainer.HasTrait("Worker")) {
    //            return SetHasWorkers(true);
    //        }
    //    }
    //    return SetHasWorkers(false);
    //}
    /// <summary>
    /// Switch the has peasants switch on/off
    /// </summary>
    /// <param name="state">The state to switch to</param>
    /// <returns>Whether or not the switched was toggled</returns>
    //private bool SetHasPeasants(bool state) {
    //    if (hasPeasants != state) {
    //        hasPeasants = state;
    //        UpdateAbleJobsOfAllResidents();
    //        return true;
    //    }
    //    return false;
    //}
    /// <summary>
    /// Switch the has workers switch on/off
    /// </summary>
    /// <param name="state">The state to switch to</param>
    /// <returns>Whether or not the switched was toggled</returns>
    //private bool SetHasWorkers(bool state) {
    //    if (hasWorkers != state) {
    //        hasWorkers = state;
    //        UpdateAbleJobsOfAllResidents();
    //        return true;
    //    }
    //    return false;
    //}
    //private void UpdateAbleJobsOfAllResidents() {
    //    if (owner != null && owner.factionType.type == FACTION_TYPE.Ratmen) { return; }
    //    for (int i = 0; i < residents.Count; i++) {
    //        Character character = residents[i];
    //        UpdateAbleJobsOfResident(character);
    //    }
    //}
    //public void UpdateAbleJobsOfResident(Character character) {
    //    if (owner != null && owner.factionType.type == FACTION_TYPE.Ratmen) { return; }
    //    if (!character.race.IsSapient() && character.minion == null) { return; }
    //    //update jobs based on hasPeasants switch
    //    if (!hasPeasants) {
    //        if (character.characterClass.className != "Noble") {
    //            CharacterClass peasantClass = CharacterManager.Instance.GetCharacterClass("Peasant");
    //            //character.jobComponent.AddAdditionalPriorityJob(peasantClass.priorityJobs);
    //            //character.jobComponent.AddAdditionalPriorityJob(peasantClass.secondaryJobs);
    //            character.jobComponent.AddAdditionalPriorityJob(peasantClass.ableJobs);
    //        }
    //    } else {
    //        if (character.characterClass.className != "Noble") {
    //            CharacterClass peasantClass = CharacterManager.Instance.GetCharacterClass("Peasant");
    //            //character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.priorityJobs);
    //            //character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.secondaryJobs);
    //            character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.ableJobs);
    //        }
    //    }
        
    //    //update jobs based on hasWorkers switch
    //    if (!hasWorkers) {
    //        if (character.characterClass.className == "Noble") {
    //            CharacterClass peasantClass = CharacterManager.Instance.GetCharacterClass("Peasant");
    //            //character.jobComponent.AddAdditionalPriorityJob(peasantClass.priorityJobs);
    //            //character.jobComponent.AddAdditionalPriorityJob(peasantClass.secondaryJobs);
    //            character.jobComponent.AddAdditionalPriorityJob(peasantClass.ableJobs);
    //        }
    //    } else {
    //        if (character.characterClass.className == "Noble") {
    //            CharacterClass peasantClass = CharacterManager.Instance.GetCharacterClass("Peasant");
    //            //character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.priorityJobs);
    //            //character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.secondaryJobs);
    //            character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.ableJobs);
    //        }
    //    }
    //}
    //public void UnapplyAbleJobsFromSettlement(Character character) {
    //    if (!character.race.IsSapient() && character.minion == null) { return; }
    //    if (!hasPeasants) {
    //        if (character.characterClass.className != "Noble") {
    //            CharacterClass peasantClass = CharacterManager.Instance.GetCharacterClass("Peasant");
    //            //character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.priorityJobs);
    //            //character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.secondaryJobs);
    //            character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.ableJobs);
    //        }
    //    }
        
    //    if (!hasWorkers) {
    //        if (character.characterClass.className == "Noble") {
    //            CharacterClass peasantClass = CharacterManager.Instance.GetCharacterClass("Peasant");
    //            //character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.priorityJobs);
    //            //character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.secondaryJobs);
    //            character.jobComponent.RemoveAdditionalPriorityJob(peasantClass.ableJobs);
    //        }
    //    }
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
#if DEBUG_LOG
            Debug.Log(GameManager.Instance.TodayLogString() + " " + name + " added to forced cancel job " + job.name);
#endif
            forcedCancelJobsOnTickEnded.Add(job);
        }
    }
    public void ProcessForcedCancelJobsOnTickEnded() {
        if (forcedCancelJobsOnTickEnded.Count > 0) {
            for (int i = 0; i < forcedCancelJobsOnTickEnded.Count; i++) {
                forcedCancelJobsOnTickEnded[i].ForceCancelJob();
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
        AddPlayerAction(PLAYER_SKILL_TYPE.INDUCE_MIGRATION);
        AddPlayerAction(PLAYER_SKILL_TYPE.STIFLE_MIGRATION);
        //AddPlayerAction(PLAYER_SKILL_TYPE.SCHEME);
    }
    #endregion

    #region Village Spot
    public void SetOccupiedVillageSpot(VillageSpot p_spot) {
        occupiedVillageSpot = p_spot;
    }
    #endregion

    public override string ToString() {
        return name;
    }
}