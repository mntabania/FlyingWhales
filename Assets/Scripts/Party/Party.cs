using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;
using Logs;

public class Party : ILogFiller, ISavable, IJobOwner {
    public string persistentID { get; private set; }
    public string partyName { get; private set; }
    public PARTY_STATE partyState { get; private set; }
    //public int takeQuestSchedule { get; private set; }
    public int restSchedule { get; private set; }
    //public int endRestSchedule { get; private set; }
    public bool hasRested { get; private set; }
    public bool isDisbanded { get; private set; }
    public bool hasChangedTargetDestination { get; private set; }
    public int perHourElapsedInWaiting { get; private set; }
    public BaseSettlement partySettlement { get; private set; }
    public Faction partyFaction { get; private set; }
    public LocationStructure meetingPlace { get; private set; }
    public LocationStructure targetRestingTavern { get; private set; }
    public HexTile targetCamp { get; private set; }
    public IPartyTargetDestination targetDestination { get; private set; }
    public PartyQuest currentQuest { get; private set; }

    //public Character campSetter { get; private set; }
    //public Character foodProducer { get; private set; }

    public GameDate waitingEndDate { get; private set; }
    public List<Character> members { get; private set; }
    public List<Character> membersThatJoinedQuest { get; private set; }

    public bool cannotProduceFoodThisRestPeriod { get; private set; }
    //public bool hasStartedAcceptingQuests { get; private set; }
    public GameDate nextQuestCheckDate { get; private set; }

    public bool canAcceptQuests { get; private set; }
    public GameDate canAcceptQuestsAgainDate { get; private set; }

    public JobBoard jobBoard { get; private set; }
    public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; private set; }

    private List<Character> _activeMembers;
    private PartyJobTriggerComponent _jobComponent;

    #region getters
    public string name => partyName;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Party;
    public System.Type serializedData => typeof(SaveDataParty);
    public bool isActive => currentQuest != null;
    public List<Character> activeMembers => GetActiveMembers();
    public JOB_OWNER ownerType => JOB_OWNER.PARTY;
    public JobTriggerComponent jobTriggerComponent => _jobComponent;
    public PartyJobTriggerComponent jobComponent => _jobComponent;
    #endregion

    public Party() {
        members = new List<Character>();
        membersThatJoinedQuest = new List<Character>();
        _activeMembers = new List<Character>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        _jobComponent = new PartyJobTriggerComponent(this);
        jobBoard = new JobBoard();
    }

    public void Initialize(Character partyCreator) { //In order to create a party, there must always be a party creator
        if (string.IsNullOrEmpty(persistentID)) {
            persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        }
        partyName = PartyManager.Instance.GetNewPartyName(partyCreator);
        partySettlement = partyCreator.homeSettlement;
        partyFaction = partyCreator.faction;
        isDisbanded = false;
        hasRested = true;
        canAcceptQuests = true;
        perHourElapsedInWaiting = 0;
        forcedCancelJobsOnTickEnded.Clear();
        jobBoard.Initialize();

        SetPartyState(PARTY_STATE.None);
        //SetTakeQuestSchedule();
        SetRestSchedule();
        //SetEndRestSchedule();

        AddMember(partyCreator);
        partySettlement.AddParty(this);
        Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        DatabaseManager.Instance.partyDatabase.AddParty(this);

        ScheduleNextDateToCheckQuest();
    }

    public void Initialize(SaveDataParty data) { //In order to create a party, there must always be a party creator
        persistentID = data.persistentID;
        partyName = data.partyName;
        partyState = data.partyState;
        //takeQuestSchedule = data.takeQuestSchedule;
        restSchedule = data.restSchedule;
        //endRestSchedule = data.endRestSchedule;
        hasRested = data.hasRested;
        isDisbanded = data.isDisbanded;
        cannotProduceFoodThisRestPeriod = data.cannotProduceFoodThisRestPeriod;
        hasChangedTargetDestination = data.hasChangedTargetDestination;
        perHourElapsedInWaiting = data.perHourElapsedInWaiting;
        waitingEndDate = data.waitingEndDate;
        //hasStartedAcceptingQuests = data.hasStartedAcceptingQuests;
        nextQuestCheckDate = data.nextQuestCheckDate;

        canAcceptQuests = data.canAcceptQuests;
        canAcceptQuestsAgainDate = data.canAcceptQuestsAgainDate;

        jobBoard.InitializeFromSaveData(data.jobBoard);

        if (partyName != string.Empty) {
            Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_DESTROYED, OnStructureDestroyed);
            Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
            DatabaseManager.Instance.partyDatabase.AddParty(this);
        }
        if (!isDisbanded) {
            SchedulingManager.Instance.AddEntry(nextQuestCheckDate, TryAcceptQuest, null);
        }
        if (!canAcceptQuests) {
            SchedulingManager.Instance.AddEntry(canAcceptQuestsAgainDate, () => SetCanAcceptQuests(true), null);
        }
    }

    #region Listeners
    private void OnStructureDestroyed(LocationStructure structure) {
        OnMeetingPlaceDestroyed(structure);
    }
    private void OnTickEnded() {
        if (isActive) {
            PerTickEndedWhileActive();
            PerTickEndedInMovingState();
        } else {
            PerTickEndedWhileInactive();
        }
        ProcessForcedCancelJobsOnTickEnded();
    }
    private void OnCharacterDeath(Character character) {
        CharacterDies(character);
    }
    private void OnCharacterNoLongerMove(Character character) {
        CharacterNoLongerMove(character);
    }
    private void OnCharacterNoLongerPerform(Character character) {
        CharacterNoLongerPerform(character);
    }
    #endregion

    #region General
    //private void SetTakeQuestSchedule() {
    //    takeQuestSchedule = GameManager.GetRandomTickFromTimeInWords(TIME_IN_WORDS.MORNING);
    //}
    private void SetRestSchedule() {
        if (GameUtilities.RollChance(50)) {
            restSchedule = GameManager.GetRandomTickFromTimeInWords(TIME_IN_WORDS.LATE_NIGHT);
        } else {
            restSchedule = GameManager.GetRandomTickFromTimeInWords(TIME_IN_WORDS.AFTER_MIDNIGHT);
        }
    }
    //private void SetEndRestSchedule() {
    //    endRestSchedule = GameManager.GetRandomTicokFromTimeInWords(TIME_IN_WORDS.MORNING);
    //}
    private void SetMeetingPlace() {
        if (partySettlement != null) {
            if (partySettlement.locationType == LOCATION_TYPE.DUNGEON) {
                meetingPlace = partySettlement.GetRandomStructure();
            } else {
                meetingPlace = partySettlement.GetFirstStructureOfType(STRUCTURE_TYPE.TAVERN);
                if (meetingPlace == null) {
                    meetingPlace = partySettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                }
            }
        }
    }
    private void OnMeetingPlaceDestroyed(LocationStructure structure) {
        if(meetingPlace == structure) {
            SetMeetingPlace();
        }
    }
    private void PerTickEndedWhileInactive() {
        //Note: Commented this because we no longer have a fixed take quest schedule, the new algo for taking quest is every [X] hours after creation of party so that the party will often take quests
        //if (takeQuestSchedule == GameManager.Instance.currentTick && canAcceptQuests) {
        //    TryAcceptQuest();
        //}
    }
    private void TryAcceptQuest() {
        if (isDisbanded) {
            return;
        }
        if (!isActive) {
            TIME_IN_WORDS currentTimeInWords = GameManager.GetCurrentTimeInWordsOfTick();
            if (canAcceptQuests && (currentTimeInWords == TIME_IN_WORDS.MORNING || currentTimeInWords == TIME_IN_WORDS.LUNCH_TIME || currentTimeInWords == TIME_IN_WORDS.AFTERNOON)) {
                PartyQuest quest = partyFaction.partyQuestBoard.GetFirstUnassignedPartyQuestFor(this);
                if (quest != null) {
                    //hasStartedAcceptingQuests = false;
                    AcceptQuest(quest);
                }
            }
        }
        ScheduleNextDateToCheckQuest();
    }
    private void ScheduleNextDateToCheckQuest() {
        //This schedules the next time when the party will try to take a quest
        nextQuestCheckDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(4));
        SchedulingManager.Instance.AddEntry(nextQuestCheckDate, TryAcceptQuest, null);
    }
    private void PerTickEndedWhileActive() {
        if (restSchedule == GameManager.Instance.currentTick && partyState != PARTY_STATE.Resting) {
            hasRested = false;
        }
    }
    private LocationStructure GetStructureToCheckFromSettlement(BaseSettlement settlement) {
        LocationStructure structure = settlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
        if(structure == null) {
            structure = settlement.GetRandomStructure();
        }
        return structure;
    }
    public void GoBackHomeAndEndQuest() {
        SetPartyState(PARTY_STATE.Moving, true);
    }
    public void SetTargetDestination(IPartyTargetDestination target) {
        if(targetDestination != target) {
            targetDestination = target;

            //This is used so that every time the party changes destination and it is in the Working state, the party will switch to Moving state, if this is switched on
            //The reason for this is so the party will move to the new destination, since they will not go there if we do not switch the state to Moving
            SetHasChangedTargetDestination(true);
        }
    }
    public void SetCannotProduceFoodThisRestPeriod(bool state) {
        cannotProduceFoodThisRestPeriod = state;
    }
    public void SetHasChangedTargetDestination(bool state) {
        hasChangedTargetDestination = state;
    }
    #endregion

    #region States
    public void SetPartyState(PARTY_STATE state, bool goHome = false) {
        if (partyState != state) {
            PARTY_STATE prevState = partyState;
            partyState = state;
            OnSwitchFromState(prevState);
            OnSwitchToState(state, prevState, goHome);
            if (isActive) {
                currentQuest.OnAssignedPartySwitchedState(prevState, partyState);
            }
        }
    }
    private void OnSwitchFromState(PARTY_STATE prevState) {
        if (prevState == PARTY_STATE.None) {
            OnSwitchFromNoneState(prevState);
        } else if (prevState == PARTY_STATE.Waiting) {
            OnSwitchFromWaitingState(prevState);
        } else if (prevState == PARTY_STATE.Moving) {
            OnSwitchFromMovingState(prevState);
        } else if (prevState == PARTY_STATE.Resting) {
            OnSwitchFromRestingState(prevState);
        } else if (prevState == PARTY_STATE.Working) {
            OnSwitchFromWorkingState(prevState);
        }
    }
    private void OnSwitchToState(PARTY_STATE state, PARTY_STATE prevState, bool goHome) {
        CancellAllPartyGoToJobsOfMembers();

        //Every switch of state we must cancel all jobs currently in the party, because we assume that the jobs in list is just relevant to the previous state
        ForceCancelAllJobs();
        if (state == PARTY_STATE.None) {
            OnSwitchToNoneState(prevState);
        } else if (state == PARTY_STATE.Waiting) {
            OnSwitchToWaitingState(prevState);
        } else if (state == PARTY_STATE.Moving) {
            OnSwitchToMovingState(prevState, goHome);
        } else if (state == PARTY_STATE.Resting) {
            OnSwitchToRestingState(prevState);
        } else if (state == PARTY_STATE.Working) {
            OnSwitchToWorkingState(prevState);
        }
    }
    #endregion

    #region None State
    private void OnSwitchToNoneState(PARTY_STATE prevState) {
        //DropQuest();
    }
    private void OnSwitchFromNoneState(PARTY_STATE prevState) {
        //DropQuest();
    }
    #endregion  

    #region Waiting State
    private void OnSwitchToWaitingState(PARTY_STATE prevState) {
        //Cancel all tiredness recovery upon switching to waiting state, so that the members of the party must go to the waiting place immediately
        //Only tiredness recovery are cancelled because typically they take around 8 hours which by then the quest will be dropped already if all members are asleep
        CancelAllTirednessRecoveryJobsOfMembers();
        SetMeetingPlace();
        StartWaitTimer();
    }
    private void OnSwitchFromWaitingState(PARTY_STATE prevState) {
        if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, WaitingPerHour);
        }
    }
    private void StartWaitTimer() {
        perHourElapsedInWaiting = 0;
        Messenger.AddListener(Signals.HOUR_STARTED, WaitingPerHour);
        waitingEndDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(5));
        SchedulingManager.Instance.AddEntry(waitingEndDate, WaitingEndedDecisionMaking, this);
    }
    private void WaitingPerHour() {
        //Every hour after 2 hours, we must check if the members that joined is already enough so that the party will start the quest immediately, so we do not need to wait for the end of waiting period if the minimum party size of the quest is already met
        perHourElapsedInWaiting++;
        if(perHourElapsedInWaiting > 2) {
            if (isActive && membersThatJoinedQuest.Count >= currentQuest.minimumPartySize) {
                WaitingEndedDecisionMaking();
            }
        }
    }
    private void WaitingEndedDecisionMaking() {
        if (partyState == PARTY_STATE.Waiting && !isDisbanded && isActive) {
            //Messenger.RemoveListener(Signals.HOUR_STARTED, WaitingPerHour); //Removed this because there is already a call on OnSwitchFromWaitingState

            if (membersThatJoinedQuest.Count >= currentQuest.minimumPartySize) {
                for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
                    Character member = membersThatJoinedQuest[i];
                    member.interruptComponent.TriggerInterrupt(INTERRUPT.Morale_Boost, member);
                }
                SetPartyState(PARTY_STATE.Moving);
            } else {
                //Drop quest only instead of ending quest so that the quest can still be taken by other parties
                currentQuest.EndQuest("Not enough members joined");
            }
        }
    }
    //private void PopulateMembersThatJoinedQuest() {
    //    membersThatJoinedQuest.Clear();
    //    for (int i = 0; i < members.Count; i++) {
    //        Character member = members[i];
    //        if (member.currentStructure == meetingPlace) {
    //            membersThatJoinedQuest.Add(member);
    //        }
    //    }
    //}
    #endregion

    #region Moving State
    private void OnSwitchToMovingState(PARTY_STATE prevState, bool goHome) {
        //Every time we switch to moving state we must set the target destination so that the members will know where to go
        if (goHome) {
            SetTargetDestination(partySettlement);
        } else {
            SetTargetDestination(currentQuest.GetTargetDestination());
        }

        //If the Moving state came from Waiting state, we must cancel all jobs of the members that joined the quest because this is the start of the quest
        //But if it is not, it means that the party is already in the middle of the quest and we must only cancel the jobs of those who are still active
        if (prevState == PARTY_STATE.Waiting) {
            CancelAllJobsOfMembersThatJoinedQuest();
        } else {
            //Will no longer cancel all jobs when switching to moving state
            //Reason: Raid party problem - when a character is being kidnap by a raid member, when the raid party quest timer runs out they will all switch to moving, so even the one doing to kidnap job will be stopped
            //CancelAllJobsOfMembersThatJoinedQuestThatAreStillActive();

            if (targetDestination != null && !targetDestination.hasBeenDestroyed) {
                for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
                    Character member = membersThatJoinedQuest[i];
                    if (IsMemberActive(member)) {
                        if (!targetDestination.IsAtTargetDestination(member) && (member.currentJob == null || (member.currentJob.jobType != JOB_TYPE.KIDNAP_RAID && member.currentJob.jobType != JOB_TYPE.STEAL_RAID))) {
                            LocationGridTile tile = targetDestination.GetRandomPassableTile();
                            if (tile != null) {
                                member.jobComponent.CreatePartyGoToJob(tile);
                            }
                        }
                    }
                }
            }
        }
    }
    private void OnSwitchFromMovingState(PARTY_STATE prevState) {
    }
    private void PerTickEndedInMovingState() {
        if(!hasRested && partyState == PARTY_STATE.Moving) {
            SetPartyState(PARTY_STATE.Resting);
        }
    }
    #endregion

    #region Resting State
    private void OnSwitchToRestingState(PARTY_STATE prevState) {
        hasRested = true;
        targetRestingTavern = null;
        targetCamp = null;
        cannotProduceFoodThisRestPeriod = false;
        FindNearbyTavernOrCamp();
        if(targetRestingTavern == null && targetCamp == null) {
            //No tavern and camp found, this means that the party is near their home settlement and the target destination is the home settlement, so instead of camping, the party will just go home
            SetPartyState(PARTY_STATE.Moving);
        } else {
            if(targetCamp != null) {
                //If the party will rest in a camp, add jobs to create a campfire and produce food for camp
                _jobComponent.CreateBuildCampfireJob(JOB_TYPE.BUILD_CAMP);
                _jobComponent.CreateProduceFoodForCampJob();
            }
            Messenger.AddListener(Signals.HOUR_STARTED, RestingPerHour);
        }
    }
    private void OnSwitchFromRestingState(PARTY_STATE prevState) {
        if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, RestingPerHour);
        }
    }
    private void RestingPerHour() {
        if (!HasActiveMemberThatMustDoNeedsRecovery()) {
            //Messenger.RemoveListener(Signals.HOUR_STARTED, RestingPerHour); //Removed this because there is already a call on OnSwitchFromRestingState
            SetPartyState(PARTY_STATE.Moving);
        }
    }
    private void FindNearbyTavernOrCamp() {
        Character firstActiveMember = null;
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character member = membersThatJoinedQuest[i];
            if (member.gridTileLocation != null && member.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                if (IsMemberActive(member)) {
                    firstActiveMember = member;
                    break;
                }
            }
        }
        if (firstActiveMember != null) {
            HexTile activeMemberCurrentHex = firstActiveMember.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
            if(activeMemberCurrentHex != null && activeMemberCurrentHex.settlementOnTile != null && activeMemberCurrentHex.settlementOnTile.locationType == LOCATION_TYPE.SETTLEMENT) {
                //Hex tile within a village cannot be a camp
                activeMemberCurrentHex = null;
            }
            List<HexTile> nearbyHexes = firstActiveMember.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetTilesInRange(3);
            if (nearbyHexes != null && nearbyHexes.Count > 0) {
                for (int i = 0; i < nearbyHexes.Count; i++) {
                    HexTile hex = nearbyHexes[i];
                    BaseSettlement settlement;
                    if (hex.IsPartOfVillage(out settlement)) {
                        if (settlement == partySettlement && targetDestination == partySettlement) {
                            //If the nearby tavern is in the home settlement of the party and the home settlement is the target destination (meaning the quest is done and the party is going home), return immeditately
                            //This would mean the no resting tavern or camp will be set
                            //If this happens, it means that their home is nearby and will go home instead of setting up a camp
                            return;
                        }
                        if (settlement.owner == null || settlement.owner == partySettlement.owner || !settlement.owner.IsHostileWith(partySettlement.owner)) {
                            LocationStructure tavern = settlement.GetRandomStructureOfType(STRUCTURE_TYPE.TAVERN);
                            if (tavern != null) {
                                targetRestingTavern = tavern;
                                break;
                            }
                        }
                    } else {
                        if(activeMemberCurrentHex == null && hex.elevationType != ELEVATION.WATER && (hex.settlementOnTile == null || hex.settlementOnTile.locationType != LOCATION_TYPE.SETTLEMENT)) {
                            activeMemberCurrentHex = hex;
                        }
                    }
                }
            }

            if (targetRestingTavern == null) {
                targetCamp = activeMemberCurrentHex;
            }
        }
    }
    //public void SetCampSetter(Character character) {
    //    campSetter = character;
    //}
    //public void SetFoodProducer(Character character) {
    //    foodProducer = character;
    //}
    #endregion

    #region Working State
    private void OnSwitchToWorkingState(PARTY_STATE prevState) {
        //When the party switches to Working state always switch off the changed target destination because this means that the party has already reached the destination and must not switch to Moving state at the start of Working state
        SetHasChangedTargetDestination(false);
    }
    private void OnSwitchFromWorkingState(PARTY_STATE prevState) {
    }
    #endregion

    #region Quest
    private void AcceptQuest(PartyQuest quest) {
        if (!isActive && quest != null) {
            currentQuest = quest;
            currentQuest.SetAssignedParty(this);
            SetPartyState(PARTY_STATE.Waiting);

            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Party", "Quest", "accept_quest", providedTags: LOG_TAG.Party);
            log.AddToFillers(this, partyName, LOG_IDENTIFIER.PARTY_1);
            log.AddToFillers(null, currentQuest.GetPartyQuestTextInLog(), LOG_IDENTIFIER.STRING_2);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);

            OnAcceptQuest(quest);
        }
    }
    //private void DistributeQuestToMembersThatJoinedParty() {
    //    if (isActive) {
    //        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
    //            Character member = membersThatJoinedQuest[i];
    //            member.behaviourComponent.AddBehaviourComponent(currentQuest.relatedBehaviour);
    //        }
    //    }
    //}
    public void DropQuest(string reason) {
        if (isActive) {
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Party", "Quest", "drop_quest", providedTags: LOG_TAG.Party);
            log.AddToFillers(this, partyName, LOG_IDENTIFIER.PARTY_1);
            log.AddToFillers(null, currentQuest.GetPartyQuestTextInLog(), LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_2);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);

            OnDropQuest(currentQuest);
            ClearMembersThatJoinedQuest(shouldDropQuest: false);
            partyFaction.partyQuestBoard.RemovePartyQuest(currentQuest);
            SetPartyState(PARTY_STATE.None);
            currentQuest.SetAssignedParty(null);
            currentQuest = null;
            targetRestingTavern = null;
            meetingPlace = null;
            targetCamp = null;
            targetDestination = null;
            SetHasChangedTargetDestination(false);
        }
    }
    private void OnAcceptQuest(PartyQuest quest) {
        if(quest.partyQuestType == PARTY_QUEST_TYPE.Exploration || quest.partyQuestType == PARTY_QUEST_TYPE.Rescue) {
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDeath);
            Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerMove);
            Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterNoLongerPerform);
        }
    }
    private void OnAcceptQuestFromSaveData(PartyQuest quest) {
        if (quest.partyQuestType == PARTY_QUEST_TYPE.Exploration || quest.partyQuestType == PARTY_QUEST_TYPE.Rescue) {
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDeath);
            Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerMove);
            Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterNoLongerPerform);
        }
    }
    private void OnDropQuest(PartyQuest quest) {
        if (quest.partyQuestType == PARTY_QUEST_TYPE.Exploration || quest.partyQuestType == PARTY_QUEST_TYPE.Rescue) {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDeath);
            Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerMove);
            Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterNoLongerPerform);
        }
        if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, WaitingPerHour);
        }
        if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, RestingPerHour);
        }
        //Every time a quest is dropped, always clear out the party jobs
        ForceCancelAllJobs();

        //Do not start the 12-hour cooldown if party is already disbanded
        if (!isDisbanded) {
            //After a party drops quest, the party must not take quest for 12 hours, so that they can recupirate
            StartNoQuestCooldown();
        }
    }
    private void StartNoQuestCooldown() {
        if (canAcceptQuests) {
            SetCanAcceptQuests(false);
            canAcceptQuestsAgainDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(12));
            SchedulingManager.Instance.AddEntry(canAcceptQuestsAgainDate, () => SetCanAcceptQuests(true), null);
        }
    }
    private void SetCanAcceptQuests(bool state) {
        canAcceptQuests = state;
    }
    #endregion

    #region Members
    public bool AddMember(Character character) {
        if (!members.Contains(character)) {
            members.Add(character);
            OnAddMember(character);
            return true;
        }
        return false;
    }
    public bool RemoveMember(Character character) {
        if (members.Remove(character)) {
            OnRemoveMember(character);
            if (members.Count <= 0) {
                DisbandParty();
            }
            return true;
        }
        return false;
    }
    public void AddMemberThatJoinedQuest(Character character) {
        if (!membersThatJoinedQuest.Contains(character)) {
            membersThatJoinedQuest.Add(character);
            OnAddMemberThatJoinedQuest(character);
        }
    }
    public void ClearMembersThatJoinedQuest(bool shouldDropQuest = true) {
        while (membersThatJoinedQuest.Count > 0) {
            RemoveMemberThatJoinedQuest(membersThatJoinedQuest[0], false, shouldDropQuest);
        }
        membersThatJoinedQuest.Clear();
        Messenger.Broadcast(Signals.CLEAR_MEMBERS_THAT_JOINED_QUEST, this);
    }
    public bool RemoveMemberThatJoinedQuest(Character character, bool broadcastSignal = true, bool shouldDropQuest = true) {
        if (membersThatJoinedQuest.Remove(character)) {
            OnRemoveMemberThatJoinedQuest(character, broadcastSignal);
            if(membersThatJoinedQuest.Count <= 0 && shouldDropQuest){
                //All members that joined the quest has left the quest, if there is still a quest, drop quest
                if (isActive) {
                    currentQuest.EndQuest("Finished quest");
                }
            }
            return true;
        }
        return false;
    }
    private void OnAddMemberThatJoinedQuest(Character character) {
        character.movementComponent.SetEnableDigging(true);
        character.traitContainer.AddTrait(character, "Travelling");
        character.behaviourComponent.AddBehaviourComponent(currentQuest.relatedBehaviour);
        Messenger.Broadcast(Signals.CHARACTER_JOINED_PARTY_QUEST, this, character);
    }
    private void OnRemoveMemberThatJoinedQuest(Character character, bool broadcastSignal) {
        character.movementComponent.SetEnableDigging(false);
        character.traitContainer.RemoveTrait(character, "Travelling");
        character.behaviourComponent.RemoveBehaviourComponent(currentQuest.relatedBehaviour);

        //Remove trap structure every time a character is remove from the quest so that he will return to normal behaviour
        character.trapStructure.SetForcedStructure(null);
        character.trapStructure.SetForcedHex(null);
        if (isActive) {
            currentQuest.OnRemoveMemberThatJoinedQuest(character);
        }
        if (broadcastSignal) {
            Messenger.Broadcast(Signals.CHARACTER_LEFT_PARTY_QUEST, this, character);
        }
    }
    private void OnAddMember(Character character) {
        character.partyComponent.SetCurrentParty(this);
        character.behaviourComponent.AddBehaviourComponent(typeof(PartyBehaviour));
        Messenger.Broadcast(Signals.CHARACTER_JOINED_PARTY, this, character);
    }
    private void OnRemoveMember(Character character) {
        character.partyComponent.SetCurrentParty(null);
        character.behaviourComponent.RemoveBehaviourComponent(typeof(PartyBehaviour));
        character.jobQueue.CancelAllPartyJobs();
        RemoveMemberThatJoinedQuest(character);
        Messenger.Broadcast(Signals.CHARACTER_LEFT_PARTY, this, character);
    }
    private void OnRemoveMemberOnDisband(Character character) {
        character.partyComponent.SetCurrentParty(null);
        character.behaviourComponent.RemoveBehaviourComponent(typeof(PartyBehaviour));
        character.jobQueue.CancelAllPartyJobs();
        RemoveMemberThatJoinedQuest(character);
        Messenger.Broadcast(Signals.CHARACTER_LEFT_PARTY_DISBAND, this, character);
    }
    private List<Character> GetActiveMembers() {
        _activeMembers.Clear();
        if (isActive) {
            for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
                Character member = membersThatJoinedQuest[i];
                if (IsMemberActive(member)) {
                    _activeMembers.Add(member);
                }
            }
        }
        return _activeMembers;
    }
    private int GetNumberOfMembersThatJoinedInMeetingPlace() {
        int count = 0;
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character member = membersThatJoinedQuest[i];
            if (member.currentStructure == meetingPlace) {
                count++;
            }
        }
        return count;
    }
    public bool IsMemberActive(Character character) {
        if (character.canMove && character.carryComponent.IsNotBeingCarried() && !character.isBeingSeized) {
            bool isActive = false;
            if(partyState == PARTY_STATE.Waiting) {
                if(meetingPlace != null && !meetingPlace.hasBeenDestroyed && meetingPlace.passableTiles.Count > 0) {
                    if(character.currentStructure == meetingPlace) {
                        isActive = true;
                    } else {
                        LocationGridTile tile = meetingPlace.passableTiles[0];
                        if (character.movementComponent.HasPathToEvenIfDiffRegion(tile)) {
                            isActive = true;
                        }
                    }
                }
            } else if (partyState == PARTY_STATE.Moving || partyState == PARTY_STATE.Working) {
                if (targetDestination != null && !targetDestination.hasBeenDestroyed) {
                    if (targetDestination.IsAtTargetDestination(character)) {
                        isActive = true;
                    } else {
                        LocationGridTile tile = targetDestination.GetRandomPassableTile();
                        if (character.movementComponent.HasPathToEvenIfDiffRegion(tile)) {
                            isActive = true;
                        }
                    }
                } 
                //else if (partySettlement != null && partyState == PARTY_STATE.Moving) {
                //    if (character.currentSettlement == partySettlement) {
                //        isActive = true;
                //    } else {
                //        LocationStructure structure = GetStructureToCheckFromSettlement(partySettlement);
                //        LocationGridTile tile = structure.GetRandomPassableTile();
                //        if (character.movementComponent.HasPathToEvenIfDiffRegion(tile)) {
                //            isActive = true;
                //        }
                //    }
            } else if (partyState == PARTY_STATE.Resting) {
                if (targetRestingTavern != null && !targetRestingTavern.hasBeenDestroyed && targetRestingTavern.passableTiles.Count > 0) {
                    if (character.currentStructure == targetRestingTavern) {
                        isActive = true;
                    } else {
                        LocationGridTile tile = targetRestingTavern.passableTiles[0];
                        if (character.movementComponent.HasPathToEvenIfDiffRegion(tile)) {
                            isActive = true;
                        }
                    }
                } else if (targetCamp != null) {
                    if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                        && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == targetCamp) {
                        isActive = true;
                    } else {
                        LocationGridTile tile = targetCamp.GetCenterLocationGridTile();
                        if (character.movementComponent.HasPathToEvenIfDiffRegion(tile)) {
                            isActive = true;
                        }
                    }
                } else {
                    LocationStructure structure = GetStructureToCheckFromSettlement(partySettlement);
                    LocationGridTile tile = structure.GetRandomPassableTile();
                    if (character.movementComponent.HasPathToEvenIfDiffRegion(tile)) {
                        isActive = true;
                    }
                }
            }
            return isActive;
            //if (isActive) {
            //    return members.Contains(character);
            //}
        }
        return false;
    }
    public bool DidMemberJoinQuest(Character member) {
        return membersThatJoinedQuest.Contains(member);
    }
    private void CancelAllTirednessRecoveryJobsOfMembers() {
        for (int i = 0; i < members.Count; i++) {
            Character member = members[i];
            if (member.currentActionNode != null && member.currentJob != null && InteractionManager.Instance.IsActionTirednessRecovery(member.currentActionNode.action)) {
                member.currentJob.CancelJob();
            }
            member.jobQueue.CancelAllJobs(JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT);
        }
    }
    private void CancelAllJobsOfMembersThatJoinedQuest() {
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character member = membersThatJoinedQuest[i];
            member.jobQueue.CancelAllJobs();
        }
    }
    private void CancelAllJobsOfMembersThatJoinedQuestThatAreStillActive() {
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character member = membersThatJoinedQuest[i];
            if (IsMemberActive(member)) {
                member.jobQueue.CancelAllJobs();
            }
        }
    }
    private void CancellAllPartyGoToJobsOfMembers() {
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character member = membersThatJoinedQuest[i];
            member.jobQueue.CancelAllJobs(JOB_TYPE.PARTY_GO_TO, JOB_TYPE.GO_TO_WAITING);
            member.trapStructure.ResetAllTrapStructures();
            member.trapStructure.ResetAllTrapHexes();
        }
    }
    private bool HasActiveMemberThatMustDoNeedsRecovery() {
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character member = membersThatJoinedQuest[i];
            if (IsMemberActive(member)) {
                if(member.needsComponent.isTired || member.needsComponent.isExhausted || member.needsComponent.isBored || member.needsComponent.isSulking) {
                    return true;
                } else if((member.needsComponent.isHungry || member.needsComponent.isStarving) && !cannotProduceFoodThisRestPeriod) {
                    return true;
                }
            }
        }
        return false;
    }
    public Character GetMemberInCombatExcept(Character character) {
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character member = membersThatJoinedQuest[i];
            if (member != character) {
                if (member.combatComponent.isInCombat) {
                    return member;
                }
            }
        }
        return null;
    }
    public bool IsMember(Character character) {
        return members.Contains(character);
    }
    private void CharacterDies(Character character) {
        if (currentQuest.partyQuestType == PARTY_QUEST_TYPE.Exploration || currentQuest.partyQuestType == PARTY_QUEST_TYPE.Rescue) {
            if (GameUtilities.RollChance(25)) {
                if (membersThatJoinedQuest.Contains(character)) {
                    currentQuest.EndQuest(character.name + " died");
                }
            }
        }
    }
    private void CharacterNoLongerPerform(Character character) {
        if (currentQuest.partyQuestType == PARTY_QUEST_TYPE.Exploration || currentQuest.partyQuestType == PARTY_QUEST_TYPE.Rescue) {
            if (GameUtilities.RollChance(15)) {
                if (membersThatJoinedQuest.Contains(character)) {
                    currentQuest.EndQuest(character.name + " is incapacitated");
                }
            }
        }
    }
    private void CharacterNoLongerMove(Character character) {
        if (currentQuest.partyQuestType == PARTY_QUEST_TYPE.Exploration || currentQuest.partyQuestType == PARTY_QUEST_TYPE.Rescue) {
            if (GameUtilities.RollChance(15)) {
                if (membersThatJoinedQuest.Contains(character)) {
                    currentQuest.EndQuest(character.name + " is incapacitated");
                }
            }
        }
    }
    #endregion

    #region Disbandment
    public void DisbandParty() {
        if (isDisbanded) { return; }
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Party", "General", "disband", providedTags: LOG_TAG.Party);
        log.AddToFillers(this, partyName, LOG_IDENTIFIER.PARTY_1);
        log.AddLogToDatabase();

        if (members.Count > 0) {
            for (int i = 0; i < members.Count; i++) {
                OnRemoveMemberOnDisband(members[i]);
            }
            members.Clear();
        }
        OnDisbandParty();
    }
    private void OnDisbandParty() {
        isDisbanded = true;
        if (isActive) {
            //unassign party from quest when they disband, if any.
            currentQuest.EndQuest("Party disbanded");
        }
        Messenger.Broadcast(Signals.DISBAND_PARTY, this);
        DestroyParty();
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataParty data) {
        jobBoard.LoadReferences(data.jobBoard);
        
        if(data.forcedCancelJobsOnTickEnded != null) {
            for (int i = 0; i < data.forcedCancelJobsOnTickEnded.Count; i++) {
                forcedCancelJobsOnTickEnded.Add(DatabaseManager.Instance.jobDatabase.GetJobWithPersistentID(data.forcedCancelJobsOnTickEnded[i]));
            }
        }

        if (!string.IsNullOrEmpty(data.meetingPlace)) {
            meetingPlace = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.meetingPlace);
        }
        if (!string.IsNullOrEmpty(data.targetRestingTavern)) {
            targetRestingTavern = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.targetRestingTavern);
        }
        if (!string.IsNullOrEmpty(data.targetCamp)) {
            targetCamp = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(data.targetCamp);
        }
        if (!string.IsNullOrEmpty(data.targetDestination)) {
            if(data.targetDestinationType == PARTY_TARGET_DESTINATION_TYPE.Hextile) {
                targetDestination = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(data.targetDestination);
            } else if (data.targetDestinationType == PARTY_TARGET_DESTINATION_TYPE.Structure) {
                targetDestination = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.targetDestination);
            } else if (data.targetDestinationType == PARTY_TARGET_DESTINATION_TYPE.Settlement) {
                targetDestination = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.targetDestination);
            }
        }
        if (!string.IsNullOrEmpty(data.currentQuest)) {
            currentQuest = DatabaseManager.Instance.partyQuestDatabase.GetPartyQuestByPersistentID(data.currentQuest);
            if(currentQuest != null) {
                OnAcceptQuestFromSaveData(currentQuest);
            }
        }
        //if (!string.IsNullOrEmpty(data.campSetter)) {
        //    campSetter = CharacterManager.Instance.GetCharacterByPersistentID(data.campSetter);
        //}
        //if (!string.IsNullOrEmpty(data.foodProducer)) {
        //    foodProducer = CharacterManager.Instance.GetCharacterByPersistentID(data.foodProducer);
        //}
        if(data.members != null) {
            members = SaveUtilities.ConvertIDListToCharacters(data.members);
        }
        if (data.membersThatJoinedQuest != null) {
            membersThatJoinedQuest = SaveUtilities.ConvertIDListToCharacters(data.membersThatJoinedQuest);
        }
        if (!string.IsNullOrEmpty(data.partySettlement)) {
            partySettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.partySettlement);
        }
        if (!string.IsNullOrEmpty(data.partyFaction)) {
            partyFaction = FactionManager.Instance.GetFactionByPersistentID(data.partyFaction);
        }

        if ((targetRestingTavern != null || targetCamp != null) && partyState == PARTY_STATE.Resting) {
            Messenger.AddListener(Signals.HOUR_STARTED, RestingPerHour);
        } else if (partyState == PARTY_STATE.Waiting) {
            Messenger.AddListener(Signals.HOUR_STARTED, WaitingPerHour);
        }
    }
    #endregion

    #region IJobOwner
    public void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character) {
        //RemoveFromAvailableJobs(job);
    }
    public void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character) {
        if (!job.IsJobStillApplicable() || job.shouldBeRemovedFromSettlementWhenUnassigned) {
            jobBoard.RemoveFromAvailableJobs(job);
        }
    }
    public bool ForceCancelJob(JobQueueItem job) {
        return jobBoard.RemoveFromAvailableJobs(job);
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
    private void ForceCancelAllJobsTargetingCharacter(IPointOfInterest target, string reason) {
        for (int i = 0; i < jobBoard.availableJobs.Count; i++) {
            JobQueueItem job = jobBoard.availableJobs[i];
            if (job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    AddForcedCancelJobsOnTickEnded(goapJob);
                }
            }
        }
    }
    private void ForceCancelJobTypesTargetingPOI(IPointOfInterest target, string reason, JOB_TYPE jobType) {
        for (int i = 0; i < jobBoard.availableJobs.Count; i++) {
            JobQueueItem job = jobBoard.availableJobs[i];
            if (job.jobType == jobType && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    AddForcedCancelJobsOnTickEnded(goapJob);
                }
            }
        }
    }
    public void ForceCancelJobTypesTargetingPOI(JOB_TYPE jobType, IPointOfInterest target) {
        for (int i = 0; i < jobBoard.availableJobs.Count; i++) {
            JobQueueItem job = jobBoard.availableJobs[i];
            if (job.jobType == jobType && job is GoapPlanJob) {
                GoapPlanJob goapJob = job as GoapPlanJob;
                if (goapJob.targetPOI == target) {
                    AddForcedCancelJobsOnTickEnded(goapJob);
                }
            }
        }
    }
    public void ForceCancelAllJobs() {
        for (int i = 0; i < jobBoard.availableJobs.Count; i++) {
            AddForcedCancelJobsOnTickEnded(jobBoard.availableJobs[i]);
        }
    }
    public void ForceCancelAllJobsImmediately() {
        for (int i = 0; i < jobBoard.availableJobs.Count; i++) {
            if (jobBoard.availableJobs[i].ForceCancelJob(false)) {
                i--;
            }
        }
        for (int i = 0; i < forcedCancelJobsOnTickEnded.Count; i++) {
            if (forcedCancelJobsOnTickEnded[i].ForceCancelJob(false)) {
                i--;
            }
        }
    }
    #endregion

    #region Object Pool
    private void DestroyParty() {
        ObjectPoolManager.Instance.ReturnPartyToPool(this);
    }
    public void Reset() {
        partySettlement.RemoveParty(this);
        DatabaseManager.Instance.partyDatabase.RemoveParty(this);
        partyName = string.Empty;
        partyState = PARTY_STATE.None;
        //takeQuestSchedule = -1;
        restSchedule = -1;
        hasRested = false;
        partySettlement = null;
        partyFaction = null;
        targetRestingTavern = null;
        targetCamp = null;
        targetDestination = null;
        currentQuest = null;
        meetingPlace = null;
        //campSetter = null;
        //foodProducer = null;
        cannotProduceFoodThisRestPeriod = false;
        hasChangedTargetDestination = false;
        canAcceptQuests = false;
        perHourElapsedInWaiting = 0;
        members.Clear();
        ClearMembersThatJoinedQuest(shouldDropQuest: false);
        _activeMembers.Clear();
        ForceCancelAllJobsImmediately();
        forcedCancelJobsOnTickEnded.Clear();
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, WaitingPerHour);
        }
        if (Messenger.eventTable.ContainsKey(Signals.HOUR_STARTED)) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, RestingPerHour);
        }
        DatabaseManager.Instance.partyDatabase.RemoveParty(this);
    }
    #endregion
}


[System.Serializable]
public class SaveDataParty : SaveData<Party>, ISavableCounterpart {
    public string persistentID { get; set; }
    public string partyName;
    public PARTY_STATE partyState;
    //public int takeQuestSchedule;
    public int restSchedule;
    //public int endRestSchedule;
    public bool hasRested;
    public bool isDisbanded;
    public bool cannotProduceFoodThisRestPeriod;
    public bool hasChangedTargetDestination;
    public int perHourElapsedInWaiting;
    public string partySettlement;
    public string partyFaction;
    public string meetingPlace;
    public string targetRestingTavern;
    public string targetCamp;
    public string targetDestination;
    public PARTY_TARGET_DESTINATION_TYPE targetDestinationType;
    public string currentQuest;
    //public bool hasStartedAcceptingQuests;
    public GameDate nextQuestCheckDate;
    public bool canAcceptQuests;
    public GameDate canAcceptQuestsAgainDate;


    public string campSetter;
    public string foodProducer;

    public SaveDataJobBoard jobBoard;
    public List<string> forcedCancelJobsOnTickEnded;

    public GameDate waitingEndDate;
    public List<string> members;
    public List<string> membersThatJoinedQuest;

    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Party;
    #endregion

    #region Overrides
    public override void Save(Party data) {
        persistentID = data.persistentID;
        partyName = data.partyName;
        partyState = data.partyState;
        //takeQuestSchedule = data.takeQuestSchedule;
        restSchedule = data.restSchedule;
        //endRestSchedule = data.endRestSchedule;
        hasRested = data.hasRested;
        isDisbanded = data.isDisbanded;
        cannotProduceFoodThisRestPeriod = data.cannotProduceFoodThisRestPeriod;
        hasChangedTargetDestination = data.hasChangedTargetDestination;
        perHourElapsedInWaiting = data.perHourElapsedInWaiting;
        partySettlement = data.partySettlement.persistentID;
        partyFaction = data.partyFaction.persistentID;

        //hasStartedAcceptingQuests = data.hasStartedAcceptingQuests;
        nextQuestCheckDate = data.nextQuestCheckDate;

        canAcceptQuests = data.canAcceptQuests;
        canAcceptQuestsAgainDate = data.canAcceptQuestsAgainDate;

        waitingEndDate = data.waitingEndDate;

        members = SaveUtilities.ConvertSavableListToIDs(data.members);
        membersThatJoinedQuest = SaveUtilities.ConvertSavableListToIDs(data.membersThatJoinedQuest);

        jobBoard = new SaveDataJobBoard();
        jobBoard.Save(data.jobBoard);

        if(data.forcedCancelJobsOnTickEnded.Count > 0) {
            forcedCancelJobsOnTickEnded = new List<string>();
            for (int i = 0; i < data.forcedCancelJobsOnTickEnded.Count; i++) {
                JobQueueItem job = data.forcedCancelJobsOnTickEnded[i];
                forcedCancelJobsOnTickEnded.Add(job.persistentID);
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(job);
            }
        }

        if (data.meetingPlace != null) {
            meetingPlace = data.meetingPlace.persistentID;
        }
        if (data.targetRestingTavern != null) {
            targetRestingTavern = data.targetRestingTavern.persistentID;
        }
        if (data.targetCamp != null) {
            targetCamp = data.targetCamp.persistentID;
        }
        if (data.targetDestination != null) {
            targetDestination = data.targetDestination.persistentID;
            targetDestinationType = data.targetDestination.partyTargetDestinationType;
        }
        if (data.currentQuest != null) {
            currentQuest = data.currentQuest.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentQuest);
        }
        //if (data.campSetter != null) {
        //    campSetter = data.campSetter.persistentID;
        //}
        //if (data.foodProducer != null) {
        //    foodProducer = data.foodProducer.persistentID;
        //}
    }

    public override Party Load() {
        Party party = PartyManager.Instance.CreateNewParty(this);
        return party;
    }
    #endregion
}