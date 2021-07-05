using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;
using Logs;
using Object_Pools;
using UnityEngine.Profiling;
using System;
public class Party : ILogFiller, ISavable, IJobOwner, IBookmarkable {

    public Action onQuestSucceed;
    public Action onQuestFailed;

    public interface PartyEventsIListener {
        void OnQuestSucceed();
        void OnQuestFailed();
    }
    
    public string persistentID { get; private set; }
    public string partyName { get; private set; }
    public PARTY_STATE partyState { get; private set; }
    public bool startedTrueRestingState { get; private set; } //True resting state starts when party already has a campfire
    public bool isDisbanded { get; private set; }
    public bool hasChangedTargetDestination { get; private set; }
    //public int perHourElapsedInWaiting { get; private set; }
    public BaseSettlement partySettlement { get; private set; }
    public Faction partyFaction { get; private set; }
    public LocationStructure meetingPlace { get; private set; }
    public LocationStructure targetRestingTavern { get; private set; }
    public Area targetCamp { get; private set; }
    public IPartyTargetDestination targetDestination { get; private set; }
    public PartyQuest currentQuest { get; private set; }
    public PARTY_QUEST_TYPE prevQuestType { get; private set; }
    public float partyWalkSpeed { get; private set; } //Do not save this because this is updated once all members in the party is loaded from save data. see LoadReferences

    public PARTY_QUEST_TYPE plannedPartyType { get; set; }
    //public Character campSetter { get; private set; }
    //public Character foodProducer { get; private set; }

    public GameDate waitingEndDate { get; private set; }
    public List<Character> members { get; private set; }
    public List<Character> membersThatJoinedQuest { get; private set; }

    public List<Character> deadMembers { get; private set; }

    //public bool cannotProduceFoodThisRestPeriod { get; private set; }
    //public bool hasStartedAcceptingQuests { get; private set; }
    public GameDate nextQuestCheckDate { get; private set; }
    //public bool canAcceptQuests { get; private set; }
    //public GameDate canAcceptQuestsAgainDate { get; private set; }
    public GameDate nextWaitingCheckDate { get; private set; }
    public bool hasSetNextSwitchToWaitingStateTrigger { get; private set; }
    public GameDate endQuestDate { get; private set; }
    public bool hasSetEndQuestDate { get; private set; }
    public int chanceToRetreatUponKnockoutOrDeath { get; private set; }

    public JobBoard jobBoard { get; private set; }
    public List<JobQueueItem> forcedCancelJobsOnTickEnded { get; private set; }

    //IBookmarkable
    public BookmarkableEventDispatcher bookmarkEventDispatcher { get; }
    
    //Components
    public PartyBeaconComponent beaconComponent { get; private set; }

    private List<Character> _activeMembers;
    private PartyJobTriggerComponent _jobComponent;

    public PartyDamageAccumulator damageAccumulator;

    #region getters
    public BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text;
    public string name => partyName;
    public string bookmarkName => partyName;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Party;
    public System.Type serializedData => typeof(SaveDataParty);
    public bool isActive => currentQuest != null;
    public bool isPlayerParty => partyFaction != null && partyFaction.isPlayerFaction;
    public List<Character> activeMembers => GetActiveMembers();
    public JOB_OWNER ownerType => JOB_OWNER.PARTY;
    public JobTriggerComponent jobTriggerComponent => _jobComponent;
    public PartyJobTriggerComponent jobComponent => _jobComponent;
    #endregion

    public Party() {
        members = new List<Character>();
        membersThatJoinedQuest = new List<Character>();
        _activeMembers = new List<Character>();
        deadMembers = new List<Character>();
        forcedCancelJobsOnTickEnded = new List<JobQueueItem>();
        _jobComponent = new PartyJobTriggerComponent(this);
        jobBoard = new JobBoard();
        beaconComponent = new PartyBeaconComponent(); beaconComponent.SetOwner(this);
        bookmarkEventDispatcher = new BookmarkableEventDispatcher();
        damageAccumulator = new PartyDamageAccumulator();
    }

    public bool IsPartyTheSameAsThisParty(Party p_party) {
        if (p_party == null) {
            return false;
        }
        return persistentID == p_party.persistentID;
    }

	void InitializePartyName(Character partyCreator) {
        if (plannedPartyType == PARTY_QUEST_TYPE.Demon_Snatch || plannedPartyType == PARTY_QUEST_TYPE.Demon_Defend ||
                plannedPartyType == PARTY_QUEST_TYPE.Demon_Raid ||
                plannedPartyType == PARTY_QUEST_TYPE.Demon_Rescue) {
            partyName = PartyManager.Instance.GetNewPartyNameForPlayerParty(partyCreator, plannedPartyType);
        } else {
            partyName = PartyManager.Instance.GetNewPartyName(partyCreator);
        }
    }

    public void Initialize(Character partyCreator) { //In order to create a party, there must always be a party creator
        InitializePartyName(partyCreator);
        persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        if (partyCreator.faction != null && partyCreator.faction.isPlayerFaction) {
            partySettlement = PlayerManager.Instance.player.playerSettlement;
        } else {
            partySettlement = partyCreator.homeSettlement;
        }
        partyFaction = partyCreator.faction;
        isDisbanded = false;
        //hasRested = true;
        //canAcceptQuests = true;
        //perHourElapsedInWaiting = 0;
        forcedCancelJobsOnTickEnded.Clear();
        jobBoard.Initialize();
        beaconComponent.Initialize();

        SetPartyState(PARTY_STATE.None);
        //SetTakeQuestSchedule();
        //SetRestSchedule();
        //SetEndRestSchedule();

        AddMember(partyCreator);
        partySettlement.AddParty(this);
        Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
        Messenger.AddListener<JobQueueItem, JobBoard>(JobSignals.JOB_REMOVED_FROM_JOB_BOARD, OnJobRemovedFromJobBoard);
        DatabaseManager.Instance.partyDatabase.AddParty(this);

        if (!isPlayerParty) {
            InitialScheduleToCheckQuest();
        }
    }

    public void Initialize(SaveDataParty data) { //In order to create a party, there must always be a party creator
        persistentID = data.persistentID;
        partyName = data.partyName;
        partyState = data.partyState;
        //takeQuestSchedule = data.takeQuestSchedule;
        //restSchedule = data.restSchedule;
        //endRestSchedule = data.endRestSchedule;
        //hasRested = data.hasRested;
        startedTrueRestingState = data.startedTrueRestingState;
        isDisbanded = data.isDisbanded;
        //cannotProduceFoodThisRestPeriod = data.cannotProduceFoodThisRestPeriod;
        hasChangedTargetDestination = data.hasChangedTargetDestination;
        //perHourElapsedInWaiting = data.perHourElapsedInWaiting;
        waitingEndDate = data.waitingEndDate;
        //hasStartedAcceptingQuests = data.hasStartedAcceptingQuests;
        nextQuestCheckDate = data.nextQuestCheckDate;

        //canAcceptQuests = data.canAcceptQuests;
        //canAcceptQuestsAgainDate = data.canAcceptQuestsAgainDate;

        hasSetNextSwitchToWaitingStateTrigger = data.hasSetNextSwitchToWaitingStateTrigger;
        nextWaitingCheckDate = data.nextWaitingCheckDate;

        hasSetEndQuestDate = data.hasSetEndQuestDate;
        endQuestDate = data.endQuestDate;

        prevQuestType = data.prevQuestType;
        plannedPartyType = data.plannedPartyType;

        chanceToRetreatUponKnockoutOrDeath = data.chanceToRetreatUponKnockoutOrDeath;

        jobBoard.InitializeFromSaveData(data.jobBoard);
        beaconComponent.Initialize(data.beaconComponent);
        damageAccumulator.Initialize(data.damageAccumulator);

        if (partyName != string.Empty) {
            Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
            Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
            Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
            Messenger.AddListener<JobQueueItem, JobBoard>(JobSignals.JOB_REMOVED_FROM_JOB_BOARD, OnJobRemovedFromJobBoard);
            DatabaseManager.Instance.partyDatabase.AddParty(this);
        }
        if (!isDisbanded) {
            SchedulingManager.Instance.AddEntry(nextQuestCheckDate, TryAcceptQuest, null);
        }
        //if (!canAcceptQuests) {
        //    SchedulingManager.Instance.AddEntry(canAcceptQuestsAgainDate, () => SetCanAcceptQuests(true), null);
        //}
        if (partyState == PARTY_STATE.Waiting) {
            SchedulingManager.Instance.AddEntry(waitingEndDate, WaitingEndedDecisionMaking, this);
        }
        if (hasSetNextSwitchToWaitingStateTrigger) {
            SchedulingManager.Instance.AddEntry(nextWaitingCheckDate, TryStartToWaitQuest, null);
        }
        if (hasSetEndQuestDate) {
            SchedulingManager.Instance.AddEntry(endQuestDate, TryScheduledEndQuest, null);
        }
    }

    #region Listeners
    private void OnStructureDestroyed(LocationStructure structure) {
        OnMeetingPlaceDestroyed(structure);
    }
    private void OnTickEnded() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"Party On Tick Ended");
#endif
        ProcessForcedCancelJobsOnTickEnded();
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void OnHourStarted() {
        if (isActive) {
            //This arrangement of calls is intended
            //The waiting state call is last so that if the party decided to switch to moving state while in waiting state, it will not immediately check the PerHourInMovingState
            //it will instead wait for 1 hour before calling the PerHourInMovingState because of this arrangement
            
            //Note: Parties should no longer switch to resting state so these calls are now useless
            //PerHourInRestingState();
            //PerHourInMovingState();
            PerHourInWaitingState();
        }
    }
    private void OnJobRemovedFromJobBoard(JobQueueItem p_job, JobBoard p_jobBoard) {
        if(jobBoard == p_jobBoard) {
            JobRemovedFromJobBoard(p_job);
        }
    }
#endregion

#region General
    //private void SetTakeQuestSchedule() {
    //    takeQuestSchedule = GameManager.GetRandomTickFromTimeInWords(TIME_IN_WORDS.MORNING);
    //}
    //private void SetRestSchedule() {
    //    if (GameUtilities.RollChance(50)) {
    //        restSchedule = GameManager.GetRandomTickFromTimeInWords(TIME_IN_WORDS.LATE_NIGHT);
    //    } else {
    //        restSchedule = GameManager.GetRandomTickFromTimeInWords(TIME_IN_WORDS.AFTER_MIDNIGHT);
    //    }
    //}
    //private void SetEndRestSchedule() {
    //    endRestSchedule = GameManager.GetRandomTicokFromTimeInWords(TIME_IN_WORDS.MORNING);
    //}
    public void SetMeetingPlace() {
        if (partySettlement != null) {
            if (partySettlement.locationType == LOCATION_TYPE.DUNGEON) {
                meetingPlace = partySettlement.GetRandomStructure();
            } else {
                meetingPlace = partySettlement.GetRandomStructureWithTypeWhereAPartyHasPathTo(STRUCTURE_TYPE.TAVERN, this);
                if(meetingPlace == null){
                    meetingPlace = partySettlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
                    if(meetingPlace == null) {
                        if (isPlayerParty) {
                            meetingPlace = partySettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL);
                        } else {
                            meetingPlace = partySettlement.GetRandomStructure();
                        }
                    }
                }
            }
        }
    }
    private void OnMeetingPlaceDestroyed(LocationStructure structure) {
        if(meetingPlace == structure) {
            SetMeetingPlace();
        }
    }
    //private void PerTickEndedWhileInactive() {
        //Note: Commented this because we no longer have a fixed take quest schedule, the new algo for taking quest is every [X] hours after creation of party so that the party will often take quests
        //if (takeQuestSchedule == GameManager.Instance.currentTick && canAcceptQuests) {
        //    TryAcceptQuest();
        //}
    //}
    public void TryAcceptQuest() {
        if (isDisbanded) {
            return;
        }
        if (!isActive) {
            //bool acceptQuest = false;
            //if (isPlayerParty) {
            //    acceptQuest = true;
            //} else {
            //    //TIME_IN_WORDS currentTimeInWords = GameManager.Instance.GetCurrentTimeInWordsOfTick();
            //    acceptQuest = canAcceptQuests; //&& (currentTimeInWords == TIME_IN_WORDS.MORNING || currentTimeInWords == TIME_IN_WORDS.LUNCH_TIME || currentTimeInWords == TIME_IN_WORDS.AFTERNOON);
            //}
            //if (acceptQuest) {

            //Randomize getting of party quests
            //https://trello.com/c/hyMbiwX6/4864-revisit-system-on-how-parties-choose-which-quest-to-take-first
            PartyQuest quest = partyFaction.partyQuestBoard.GetRandomUnassignedPartyQuestFor(this);
            if (quest != null) {
                //hasStartedAcceptingQuests = false;
                AcceptQuest(quest);
            }
            //}
        }
        if (!isPlayerParty) {
            ScheduleNextDateToCheckQuest();
        }
    }
    private void TryStartToWaitQuest() {
        if (hasSetNextSwitchToWaitingStateTrigger) {
            hasSetNextSwitchToWaitingStateTrigger = false;
            if (partyState == PARTY_STATE.None && isActive) {
                if (currentQuest.workingStateImmediately) {
                    SetPartyState(PARTY_STATE.Working);
                } else {
                    SetPartyState(PARTY_STATE.Waiting);
                }
            }
        }
    }
    private void TryScheduledEndQuest() {
        if (hasSetEndQuestDate) {
            hasSetEndQuestDate = false;
            if (isActive) {
                currentQuest.EndQuest("Done working");
            }
        }
    }
    private void InitialScheduleToCheckQuest() {
        int minimumTick = GameManager.Instance.GetTicksBasedOnHour(5); //5 AM in ticks
        int maximumTick = GameManager.Instance.GetTicksBasedOnHour(7); //7 AM in ticks

        int scheduledTick = GameUtilities.RandomBetweenTwoNumbers(minimumTick, maximumTick);
        GameDate schedule = GameManager.Instance.Today().AddDays(1);
        schedule.SetTicks(scheduledTick);
        nextQuestCheckDate = schedule;
        SchedulingManager.Instance.AddEntry(nextQuestCheckDate, TryAcceptQuest, null);
    }
    private void ScheduleNextDateToCheckQuest() {
        //This schedules the next time when the party will try to take a quest
        nextQuestCheckDate = GameManager.Instance.Today().AddDays(1); //.AddTicks(GameManager.Instance.GetTicksBasedOnHour(4));
        SchedulingManager.Instance.AddEntry(nextQuestCheckDate, TryAcceptQuest, null);
    }
    private void ScheduleToStartWaitingQuest(Character partyMember) {
        if (hasSetNextSwitchToWaitingStateTrigger) {
            return;
        }
        hasSetNextSwitchToWaitingStateTrigger = true;
        int tick = partyMember.dailyScheduleComponent.schedule.GetStartTickOfScheduleType(DAILY_SCHEDULE.Work);
        GameDate schedule = GameManager.Instance.Today();
        schedule.SetTicks(tick);
        nextWaitingCheckDate = schedule;
        SchedulingManager.Instance.AddEntry(schedule, TryStartToWaitQuest, null);
    }
    private void ScheduleToEndQuest(Character partyMember) {
        if (hasSetEndQuestDate) {
            return;
        }
        hasSetEndQuestDate = true;
        int startTick = partyMember.dailyScheduleComponent.schedule.GetStartTickOfScheduleType(DAILY_SCHEDULE.Work);
        int endTick = partyMember.dailyScheduleComponent.schedule.GetEndTickOfScheduleType(DAILY_SCHEDULE.Work);
        GameDate schedule;
        if (endTick < startTick) {
            //This means that the end tick is already the next day
            schedule = GameManager.Instance.Today().AddDays(1);
        } else {
            schedule = GameManager.Instance.Today();
        }
        schedule.SetTicks(endTick);
        endQuestDate = schedule;
        SchedulingManager.Instance.AddEntry(schedule, TryScheduledEndQuest, null);
    }
    //private void PerTickEndedWhileActive() {
    //if (restSchedule == GameManager.Instance.currentTick && partyState != PARTY_STATE.Resting) {
    //    hasRested = false;
    //}
    //}
    private LocationStructure GetStructureToCheckFromSettlement(BaseSettlement settlement) {
        LocationStructure structure = settlement.GetFirstStructureOfType(STRUCTURE_TYPE.CITY_CENTER);
        if(structure == null) {
            structure = settlement.GetRandomStructure();
        }
        return structure;
    }
    public void GoBackHomeAndEndQuest() {
        //Party no longer goes back home to end quest, instead, with the new changes in party behaviour, upon finishing quest, party will end quest immediately
        currentQuest.EndQuest("Finished quest");
        //SetPartyState(PARTY_STATE.Moving, true);
    }
    public void SetTargetDestination(IPartyTargetDestination target) {
        if(targetDestination != target) {
            targetDestination = target;

            //This is used so that every time the party changes destination and it is in the Working state, the party will switch to Moving state, if this is switched on
            //The reason for this is so the party will move to the new destination, since they will not go there if we do not switch the state to Moving
            SetHasChangedTargetDestination(true);
        }
    }
    //public void SetCannotProduceFoodThisRestPeriod(bool state) {
    //    cannotProduceFoodThisRestPeriod = state;
    //}
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
        beaconComponent.UpdateBeaconCharacter();
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
        if (isPlayerParty) {
            StartWaitTimer();
        } else {
            //Cancel all tiredness recovery upon switching to waiting state, so that the members of the party must go to the waiting place immediately
            //Only tiredness recovery are cancelled because typically they take around 8 hours which by then the quest will be dropped already if all members are asleep
            CancelAllTirednessRecoveryJobsOfMembers();
            SetMeetingPlace();
            StartWaitTimer();
        }

    }
    private void OnSwitchFromWaitingState(PARTY_STATE prevState) {
    }
    private void StartWaitTimer() {
        //perHourElapsedInWaiting = 0;
        int ticksToWait = 1; //If Demon Party, only wait for 1 tick before moving because we assume that this party is premade already
        if (!isPlayerParty) {
            ticksToWait = GameManager.Instance.GetTicksBasedOnHour(2); //Waiting should be 2 hours only
        }
        waitingEndDate = GameManager.Instance.Today().AddTicks(ticksToWait);
        SchedulingManager.Instance.AddEntry(waitingEndDate, WaitingEndedDecisionMaking, this);
    }
    private void PerHourInWaitingState() {
        if (partyState == PARTY_STATE.Waiting) {
            ////Every hour after 2 hours, we must check if the members that joined is already enough so that the party will start the quest immediately, so we do not need to wait for the end of waiting period if the minimum party size of the quest is already met
            //perHourElapsedInWaiting++;
            //if (perHourElapsedInWaiting > 2) {
            //    if (isActive && membersThatJoinedQuest.Count >= currentQuest.minimumPartySize) {
            //        WaitingEndedDecisionMaking();
            //    }
            //}
            //No more checking only after 2 hours because the waiting time is now reduced to 2 hours
            if (isActive && membersThatJoinedQuest.Count >= currentQuest.minimumPartySize) {
                WaitingEndedDecisionMaking();
            }
        }
    }
    private void WaitingEndedDecisionMaking() {
        if (partyState == PARTY_STATE.Waiting && !isDisbanded && isActive) {
            //Messenger.RemoveListener(Signals.HOUR_STARTED, WaitingPerHour); //Removed this because there is already a call on OnSwitchFromWaitingState

            if (membersThatJoinedQuest.Count >= currentQuest.minimumPartySize || isPlayerParty) {
                for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
                    Character member = membersThatJoinedQuest[i];
                    //Member can only dig once the party is travelling
                    if (!member.traitContainer.HasTrait("Travelling")) {
                        member.movementComponent.SetEnableDigging(true);
                        member.traitContainer.AddTrait(member, "Travelling");
                    }
                    //member.interruptComponent.TriggerInterrupt(INTERRUPT.Morale_Boost, member);
                }
                if (currentQuest.waitingToWorkingStateImmediately) {
                    SetPartyState(PARTY_STATE.Working);
                } else {
                    SetPartyState(PARTY_STATE.Moving);
                }
            } else {
                //Drop quest only instead of ending quest so that the quest can still be taken by other parties
                currentQuest.EndQuest("Not enough members joined");
            }
        }
    }
    public bool CanAMemberGoTo(LocationStructure structure) {
        LocationGridTile tile = structure.GetRandomPassableTile();
        if (tile != null) {
            for (int i = 0; i < members.Count; i++) {
                if (members[i].movementComponent.HasPathToEvenIfDiffRegion(tile)) {
                    return true;
                }
            }
        }
        return false;
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
        //Currently if the party switches to moving state, we must check if the previous target destination is already the home settlement of the party, if it is, all moving state of the party must be to go home
        //So that the party member will not go to the working destination again
        //This is also because once the party decides to go home, it means that the party is done with the quest so they must always go home every time the party switches to moving state
        if(targetDestination == partySettlement) {
            goHome = true;
        }
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

            //if (targetDestination != null && !targetDestination.hasBeenDestroyed) {
            //    for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            //        Character member = membersThatJoinedQuest[i];
            //        if (IsMemberActive(member)) {
            //            if (!targetDestination.IsAtTargetDestination(member) && (member.currentJob == null || (member.currentJob.jobType != JOB_TYPE.KIDNAP_RAID && member.currentJob.jobType != JOB_TYPE.STEAL_RAID))) {
            //                LocationGridTile tile = targetDestination.GetRandomPassableTile();
            //                if (tile != null) {
            //                    member.jobComponent.CreatePartyGoToJob(tile);
            //                }
            //            }
            //        }
            //    }
            //}
        }
    }
    private void OnSwitchFromMovingState(PARTY_STATE prevState) {
    }
    private void PerHourInMovingState() {
        if (partyState == PARTY_STATE.Moving && !isPlayerParty) {
            if (HasActiveMemberThatMustDoCriticalNeedsRecovery()) {
                SetPartyState(PARTY_STATE.Resting);
            }
        }
    }
#endregion

#region Resting State
    private void OnSwitchToRestingState(PARTY_STATE prevState) {
        //hasRested = true;
        SetStartedTrueRestingState(false);
        targetRestingTavern = null;
        targetCamp = null;
        //cannotProduceFoodThisRestPeriod = false;
        FindNearbyTavernOrCamp();
        if(targetRestingTavern == null && targetCamp == null) {
            //No tavern and camp found, this means that the party is near their home settlement and the target destination is the home settlement, so instead of camping, the party will just go home
            SetPartyState(PARTY_STATE.Moving);
        } else {
            if(targetCamp != null) {
                //If the party will rest in a camp, add jobs to create a campfire and produce food for camp
                _jobComponent.CreateBuildCampfireJob(JOB_TYPE.BUILD_CAMP);
                //Removed creating produce food for camp because right now we already have a food pile created after building a camp
                //_jobComponent.CreateProduceFoodForCampJob();
            } else if (targetRestingTavern != null) {
                //If party decided on tavern instead of a camp, started true resting state must be switched on so that it will 
                SetStartedTrueRestingState(true);
            }
        }
    }
    private void OnSwitchFromRestingState(PARTY_STATE prevState) {
    }
    private void PerHourInRestingState() {
        if (partyState == PARTY_STATE.Resting && startedTrueRestingState) {
            if (!HasActiveMemberThatMustDoNeedsRecovery()) {
                //Messenger.RemoveListener(Signals.HOUR_STARTED, RestingPerHour); //Removed this because there is already a call on OnSwitchFromRestingState
                SetPartyState(PARTY_STATE.Moving);
            }
        }
    }
    private void FindNearbyTavernOrCamp() {
        Character firstActiveMember = null;
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character member = membersThatJoinedQuest[i];
            if (member.gridTileLocation != null) {
                if (IsMemberActive(member)) {
                    firstActiveMember = member;
                    break;
                }
            }
        }
        if (firstActiveMember != null) {
            Area activeMemberCurrentArea = firstActiveMember.gridTileLocation.area;
            if(activeMemberCurrentArea != null && activeMemberCurrentArea.settlementOnArea != null && activeMemberCurrentArea.settlementOnArea.locationType == LOCATION_TYPE.VILLAGE) {
                //Area within a village cannot be a camp
                activeMemberCurrentArea = null;
            }
            List<Area> nearbyAreas = ObjectPoolManager.Instance.CreateNewAreaList();
            firstActiveMember.gridTileLocation.area.PopulateAreasInRange(nearbyAreas, 3);
            if (nearbyAreas != null && nearbyAreas.Count > 0) {
                for (int i = 0; i < nearbyAreas.Count; i++) {
                    Area area = nearbyAreas[i];
                    BaseSettlement settlement;
                    if (area.IsPartOfVillage(out settlement)) {
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
                        if(activeMemberCurrentArea == null && area.elevationType != ELEVATION.WATER && (area.settlementOnArea == null || area.settlementOnArea.locationType != LOCATION_TYPE.VILLAGE)) {
                            activeMemberCurrentArea = area;
                        }
                    }
                }
            }

            if (targetRestingTavern == null) {
                targetCamp = activeMemberCurrentArea;
            }
        }
    }
    //public void SetCampSetter(Character character) {
    //    campSetter = character;
    //}
    //public void SetFoodProducer(Character character) {
    //    foodProducer = character;
    //}
    private void SetStartedTrueRestingState(bool p_state) {
        startedTrueRestingState = p_state;
    }
#endregion

#region Working State
    private void OnSwitchToWorkingState(PARTY_STATE prevState) {
        if (prevState == PARTY_STATE.Waiting) {
            SetTargetDestination(currentQuest.GetTargetDestination());
            CancelAllJobsOfMembersThatJoinedQuest();
        }
        //When the party switches to Working state always switch off the changed target destination because this means that the party has already reached the destination and must not switch to Moving state at the start of Working state
        SetHasChangedTargetDestination(false);
    }
    private void OnSwitchFromWorkingState(PARTY_STATE prevState) {
    }
#endregion

    #region Quest
    private void AcceptQuest(PartyQuest quest) {
        if (!isActive && quest != null) {
            SetCurrentQuest(quest);
            currentQuest.SetAssignedParty(this);

            if (isPlayerParty) {
                SetPartyState(PARTY_STATE.Waiting);
            } else {
                SetPartyState(PARTY_STATE.None);
            }


            //Note: Accepting quest should no longer show notification and log, but for testing we should enable it
#if DEBUG_LOG
            if (!partyFaction.isPlayerFaction) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Party", "Quest", "accept_quest", null, LogUtilities.Party_Quest_Tags);
                log.AddToFillers(this, partyName, LOG_IDENTIFIER.PARTY_1);
                log.AddToFillers(null, currentQuest.GetPartyQuestTextInLog(), LOG_IDENTIFIER.STRING_2);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                LogPool.Release(log);
            }
#endif

            OnAcceptQuest(quest);
            quest.OnAcceptQuest(this);
            if (!isPlayerParty) {
                ScheduleToStartWaitingQuest(members[0]);
                ScheduleToEndQuest(members[0]);
                SetChanceToRetreatUponKnockoutOrDeath(ChanceData.GetChance(CHANCE_TYPE.Party_Quest_First_Knockout)); //25
            }
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
            PartyQuest prevQuest = currentQuest;
            if (!partyFaction.isPlayerFaction) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Party", "Quest", "drop_quest", providedTags: LOG_TAG.Party);
                log.AddToFillers(this, partyName, LOG_IDENTIFIER.PARTY_1);
                log.AddToFillers(null, currentQuest.GetPartyQuestTextInLog(), LOG_IDENTIFIER.STRING_1);
                log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_2);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            }

            OnDropQuest(currentQuest);
            if (prevQuest.isSuccessful) {
                MembersThatJoinedQuestGainsGold();
            }
            ClearMembersThatJoinedQuest(shouldDropQuest: false);
            partyFaction.partyQuestBoard.RemovePartyQuest(currentQuest);
            SetPartyState(PARTY_STATE.None);
            currentQuest.SetAssignedParty(null);
            SetCurrentQuest(null);
            targetRestingTavern = null;
            meetingPlace = null;
            targetCamp = null;
            targetDestination = null;
            SetHasChangedTargetDestination(false);
            hasSetEndQuestDate = false;

            if (prevQuest.isSuccessful) {
                Messenger.Broadcast(PartySignals.PARTY_QUEST_FINISHED_SUCCESSFULLY, this);
                onQuestSucceed?.Invoke();
            } else {
                Messenger.Broadcast(PartySignals.PARTY_QUEST_FAILED, this);
                onQuestFailed?.Invoke();
            }

            OnAfterDropQuest(prevQuest);
        }
    }
    private void SetCurrentQuest(PartyQuest p_quest) {
        if(currentQuest != p_quest) {
            if (currentQuest != null) {
                prevQuestType = currentQuest.partyQuestType;
            } else {
                prevQuestType = PARTY_QUEST_TYPE.None;
            }
            currentQuest = p_quest;
        }
    }
    private void OnAcceptQuest(PartyQuest quest) {
        for (int i = 0; i < members.Count; i++) {
            Character c = members[i];
            c.dailyScheduleComponent.OnPartyAcceptedQuest(c, quest);
        }
    }
    private void OnDropQuest(PartyQuest quest) {
        //Every time a quest is dropped, always clear out the party jobs
        ForceCancelAllJobs();
        CancellAllPartyGoToJobsOfMembers();

        //Already removed no quest cooldown because accepting quest is now a different system
        ////Do not start the 12-hour cooldown if party is already disbanded
        //if (!isDisbanded) {
        //    //After a party drops quest, the party must not take quest for 12 hours, so that they can recupirate
        //    StartNoQuestCooldown();
        //}
    }
    private void OnAfterDropQuest(PartyQuest quest) {
        for (int i = 0; i < members.Count; i++) {
            Character c = members[i];
            c.dailyScheduleComponent.OnPartyEndQuest(c, quest);
        }
    }
    //private void StartNoQuestCooldown() {
    //    if (canAcceptQuests) {
    //        SetCanAcceptQuests(false);
    //        canAcceptQuestsAgainDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(12));
    //        SchedulingManager.Instance.AddEntry(canAcceptQuestsAgainDate, () => SetCanAcceptQuests(true), null);
    //    }
    //}
    //private void SetCanAcceptQuests(bool state) {
    //    canAcceptQuests = state;
    //}
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
        if (isActive) {
            if (!membersThatJoinedQuest.Contains(character)) {
                membersThatJoinedQuest.Add(character);
                OnAddMemberThatJoinedQuest(character);
            }
        }
    }
    public void ClearMembersThatJoinedQuest(bool shouldDropQuest = true) {
        while (membersThatJoinedQuest.Count > 0) {
            RemoveMemberThatJoinedQuest(membersThatJoinedQuest[0], false, shouldDropQuest);
        }
        membersThatJoinedQuest.Clear();
        Messenger.Broadcast(PartySignals.CLEAR_MEMBERS_THAT_JOINED_QUEST, this);
    }
    public void MembersThatJoinedQuestGainsGold() {
        if (isPlayerParty) {
            return;
        }
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            membersThatJoinedQuest[i].moneyComponent.AdjustCoins(83);
        }
    }
    public bool RemoveMemberThatJoinedQuest(Character character, bool broadcastSignal = true, bool shouldDropQuest = true) {
        if (membersThatJoinedQuest.Remove(character)) {
            OnRemoveMemberThatJoinedQuest(character, broadcastSignal);
            if((membersThatJoinedQuest.Count <= 0 || (!HasActiveMemberThatJoinedQuest() && !isPlayerParty)) && shouldDropQuest){
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
        UpdatePartyWalkSpeed();
        character.combatComponent.combatBehaviourParent.currentCombatBehaviour?.OnCharacterJoinedPartyQuest(character, currentQuest.partyQuestType);
        character.behaviourComponent.AddBehaviourComponent(currentQuest.relatedBehaviour);
        Messenger.Broadcast(PartySignals.CHARACTER_JOINED_PARTY_QUEST, this, character);
    }
    private void OnRemoveMemberThatJoinedQuest(Character character, bool broadcastSignal) {
        character.movementComponent.UpdateSpeed();
        if (isActive) {
            character.combatComponent.combatBehaviourParent.currentCombatBehaviour?.OnCharacterLeftPartyQuest(character, currentQuest.partyQuestType);
        } else {
            character.combatComponent.combatBehaviourParent.currentCombatBehaviour?.OnCharacterLeftPartyQuest(character, prevQuestType);
        }
        if (character.traitContainer.HasTrait("Travelling")) {
            character.movementComponent.SetEnableDigging(false);
            character.traitContainer.RemoveTrait(character, "Travelling");
        }
        character.behaviourComponent.RemoveBehaviourComponent(currentQuest.relatedBehaviour);
        beaconComponent.OnRemoveMemberThatJoinedQuest(character);
        //Remove trap structure every time a character is remove from the quest so that he will return to normal behaviour
        //character.trapStructure.SetForcedStructure(null);
        //character.trapStructure.SetForcedHex(null);
        if (isActive) {
            currentQuest.OnRemoveMemberThatJoinedQuest(character);
        }
        if (broadcastSignal) {
            Messenger.Broadcast(PartySignals.CHARACTER_LEFT_PARTY_QUEST, this, character);
        }
    }
    private void OnAddMember(Character character) {
        character.partyComponent.SetCurrentParty(this);
        character.behaviourComponent.AddBehaviourComponent(typeof(PartyBehaviour));
        character.dailyScheduleComponent.OnCharacterJoinedParty(character);
        Messenger.Broadcast(PartySignals.CHARACTER_JOINED_PARTY, this, character);
    }
    private void OnRemoveMember(Character character) {
        character.partyComponent.SetCurrentParty(null);
        character.behaviourComponent.RemoveBehaviourComponent(typeof(PartyBehaviour));
        character.dailyScheduleComponent.OnCharacterLeftParty(character);
        character.jobQueue.CancelAllPartyJobs();
        if (character.isDead) {
            CharacterDies(character);
        }
        RemoveMemberThatJoinedQuest(character);
        Messenger.Broadcast(PartySignals.CHARACTER_LEFT_PARTY, this, character);
    }
    private void OnRemoveMemberOnDisband(Character character) {
        character.partyComponent.SetCurrentParty(null);
        character.behaviourComponent.RemoveBehaviourComponent(typeof(PartyBehaviour));
        character.dailyScheduleComponent.OnCharacterLeftParty(character);
        character.jobQueue.CancelAllPartyJobs();
        RemoveMemberThatJoinedQuest(character);
        Messenger.Broadcast(PartySignals.CHARACTER_LEFT_PARTY_DISBAND, this, character);
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
        if (character.limiterComponent.canMove && character.carryComponent.IsNotBeingCarried() && !character.isBeingSeized) {
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
                    if (character.gridTileLocation != null && character.gridTileLocation.area == targetCamp) {
                        isActive = true;
                    } else {
                        LocationGridTile tile = targetCamp.gridTileComponent.centerGridTile;
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
            if (member.currentActionNode != null && member.currentJob != null && member.currentActionNode.action.goapType.IsRestingAction()) {
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
            member.trapStructure.ResetTrapArea();
        }
    }
    private bool HasActiveMemberThatMustDoNeedsRecovery() {
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character member = membersThatJoinedQuest[i];
            if (IsMemberActive(member)) {
                if (((member.needsComponent.isStarving || member.needsComponent.isHungry) && member.limiterComponent.canDoFullnessRecovery)
                    || ((member.needsComponent.isExhausted || member.needsComponent.isTired) && member.limiterComponent.canDoTirednessRecovery)
                    || ((member.needsComponent.isSulking || member.needsComponent.isBored) && member.limiterComponent.canDoHappinessRecovery)) {
                    return true;
                }
            }
        }
        return false;
    }
    private bool HasActiveMemberThatMustDoCriticalNeedsRecovery() {
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character member = membersThatJoinedQuest[i];
            if (IsMemberActive(member)) {
                if ((member.needsComponent.isStarving && member.limiterComponent.canDoFullnessRecovery) 
                    || (member.needsComponent.isExhausted && member.limiterComponent.canDoTirednessRecovery)
                    || (member.needsComponent.isSulking && member.limiterComponent.canDoHappinessRecovery)) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasActiveMemberThatJoinedQuest() {
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character member = membersThatJoinedQuest[i];
            if (IsMemberActive(member)) {
                return true;
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
        if (membersThatJoinedQuest.Contains(character)) {
#if DEBUG_LOG
            Debug.Log("TO BE ADDED: " + character.nameWithID);
#endif
            if (!deadMembers.Contains(character)) {
                deadMembers.Add(character);
#if DEBUG_LOG
                Debug.Log("ADDED: " + character.nameWithID);
#endif
                
            }
        }
        if (currentQuest != null) {
            currentQuest.OnCharacterDeath(character);  
        }
    }
    public bool HasMemberThatJoinedQuestThatIsInRangeOfCharacterThatConsidersCrimeTypeACrime(Character character, CRIME_TYPE crimeType) {
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character member = membersThatJoinedQuest[i];
            if (character != member && member.limiterComponent.canWitness) {
                bool isInVision = character.marker && character.marker.IsPOIInVision(member);
                if (isInVision) {
                    CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(member, character, character, crimeType);
                    if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
#endregion

#region Disbandment
    private void DisbandParty() {
        if (isDisbanded) { return; }
        
        if (members.Count > 0) {
            for (int i = 0; i < members.Count; i++) {
                OnRemoveMemberOnDisband(members[i]);
            }
            members.Clear();
        }
        // Debug.LogError("TEST");
        OnDisbandParty();
    }
    private void OnDisbandParty() {
        isDisbanded = true;
        if (isActive) {
            //unassign party from quest when they disband, if any.
            currentQuest.EndQuest("Party disbanded");
        }
        Messenger.Broadcast(PartySignals.DISBAND_PARTY, this);
        DestroyParty();
    }
    public void AllMembersThatJoinedQuestGainsRandomCoinAmount(int p_minAmount, int p_maxAmount) {
        if (isPlayerParty) {
            return;
        }
        for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
            Character c = membersThatJoinedQuest[i];
            c.moneyComponent.AdjustCoins(GameUtilities.RandomBetweenTwoNumbers(p_minAmount, p_maxAmount));
        }
    }
    #endregion

    #region Party Walk Speed
    private void UpdatePartyWalkSpeed() {
        if (!isPlayerParty) { return; } //Party Walk Speed applies only on demon parties for now
        if(membersThatJoinedQuest.Count > 0) {
            //get lowest walk speed in party, excluding 0 (Wurms)
            //This is so that if a wurm is part of a party but the party still has other walking members
            //then this party will get that speed instead.
            partyWalkSpeed = float.MaxValue;
            bool wasWalkSpeedSet = false;
            for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
                Character member = membersThatJoinedQuest[i];
                if (member.movementComponent.walkSpeed > 0f && member.movementComponent.walkSpeed < partyWalkSpeed) {
                    wasWalkSpeedSet = true;
                    partyWalkSpeed = member.movementComponent.walkSpeed;
                }
            }
            if (!wasWalkSpeedSet) {
                //this can only happen if all members of the party has 0 or less walk speed
                partyWalkSpeed = 0;
            }
            // partyWalkSpeed = membersThatJoinedQuest.Min(c => c.movementComponent.walkSpeed);
            for (int i = 0; i < membersThatJoinedQuest.Count; i++) {
                membersThatJoinedQuest[i].movementComponent.UpdateSpeed();
            }
        }
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
            targetCamp = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.targetCamp);
        }
        if (!string.IsNullOrEmpty(data.targetDestination)) {
            if(data.targetDestinationType == PARTY_TARGET_DESTINATION_TYPE.Area) {
                targetDestination = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(data.targetDestination);
            } else if (data.targetDestinationType == PARTY_TARGET_DESTINATION_TYPE.Structure) {
                targetDestination = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.targetDestination);
            } else if (data.targetDestinationType == PARTY_TARGET_DESTINATION_TYPE.Settlement) {
                targetDestination = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.targetDestination);
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
        if (data.deadmembers != null) {
            deadMembers = SaveUtilities.ConvertIDListToCharacters(data.deadmembers);
        }
        if (!string.IsNullOrEmpty(data.partySettlement)) {
            partySettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.partySettlement);
        }
        if (!string.IsNullOrEmpty(data.partyFaction)) {
            partyFaction = FactionManager.Instance.GetFactionByPersistentID(data.partyFaction);
        }
        beaconComponent.LoadReferences(data.beaconComponent);
        UpdatePartyWalkSpeed();

        if (!string.IsNullOrEmpty(data.currentQuest)) {
            currentQuest = DatabaseManager.Instance.partyQuestDatabase.GetPartyQuestByPersistentID(data.currentQuest);
            if (currentQuest != null) {
                currentQuest.OnAcceptQuestFromSaveData(this);
            }
        }
    }
#endregion

#region IJobOwner
    private void JobRemovedFromJobBoard(JobQueueItem job) {
        if(job.jobType == JOB_TYPE.BUILD_CAMP) {
            SetStartedTrueRestingState(true);
        }
    }
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
            if (jobBoard.availableJobs[i].ForceCancelJob()) {
                i--;
            }
        }
        for (int i = 0; i < forcedCancelJobsOnTickEnded.Count; i++) {
            forcedCancelJobsOnTickEnded[i].ForceCancelJob();
        }
        forcedCancelJobsOnTickEnded.Clear();
    }
#endregion

#region IBookmarkable
    public void OnSelectBookmark() {
        CenterOnParty();
        UIManager.Instance.ShowPartyInfo(this);
    }
    public void RemoveBookmark() {
        PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this);
    }
    public void OnHoverOverBookmarkItem(UIHoverPosition p_pos) {
        string text = string.Empty;
        if (currentQuest is DemonSnatchPartyQuest demonSnatchPartyQuest) {
            text = $"{partyName} has been assigned to snatch {demonSnatchPartyQuest.targetCharacter?.bookmarkName}";
        } else if (currentQuest is DemonRaidPartyQuest demonRaidPartyQuest) {
            text = $"{partyName} has been assigned to harass {demonRaidPartyQuest.targetSettlement?.bookmarkName}";
        } else if (currentQuest is DemonDefendPartyQuest defendPartyQuest) {
            text = $"{partyName} has been assigned to defend {defendPartyQuest.targetStructure?.bookmarkName}";
        }
        UIManager.Instance.ShowSmallInfo(text, pos: p_pos, autoReplaceText: false);
    }
    public void OnHoverOutBookmarkItem() {
        UIManager.Instance.HideSmallInfo();
    }
#endregion

#region Utilities
    public void CenterOnParty() {
        if (beaconComponent.currentBeaconCharacter != null) {
            beaconComponent.currentBeaconCharacter.CenterOnCharacter();
        } else if (activeMembers.Count > 0) {
            activeMembers[0].CenterOnCharacter();
        } else if (members.Count > 0) {
            members[0].CenterOnCharacter();
        }
    }
    public void SetChanceToRetreatUponKnockoutOrDeath(int p_chance) {
        chanceToRetreatUponKnockoutOrDeath = p_chance;
    }
#endregion

#region Object Pool
    private void DestroyParty() {
        beaconComponent.OnDestroyParty();
        ObjectPoolManager.Instance.ReturnPartyToPool(this);
    }
    public void Reset() {
        partySettlement.RemoveParty(this);
        partyName = string.Empty;
        partyState = PARTY_STATE.None;
        //takeQuestSchedule = -1;
        //restSchedule = -1;
        //hasRested = false;
        partySettlement = null;
        partyFaction = null;
        targetRestingTavern = null;
        targetCamp = null;
        targetDestination = null;
        currentQuest = null;
        prevQuestType = PARTY_QUEST_TYPE.None;
        meetingPlace = null;
        //campSetter = null;
        //foodProducer = null;
        //cannotProduceFoodThisRestPeriod = false;
        hasChangedTargetDestination = false;
        //canAcceptQuests = false;
        //perHourElapsedInWaiting = 0;
        bookmarkEventDispatcher.ClearAll();
        damageAccumulator?.Reset();
        members.Clear();
        deadMembers.Clear();
        onQuestFailed = null;
        onQuestSucceed = null;
        hasSetNextSwitchToWaitingStateTrigger = false;
        hasSetEndQuestDate = false;
        chanceToRetreatUponKnockoutOrDeath = ChanceData.GetChance(CHANCE_TYPE.Party_Quest_First_Knockout); //25
        ClearMembersThatJoinedQuest(shouldDropQuest: false);
        _activeMembers.Clear();
        ForceCancelAllJobsImmediately();
        Messenger.RemoveListener(Signals.TICK_ENDED, OnTickEnded);
        Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
        Messenger.RemoveListener<JobQueueItem, JobBoard>(JobSignals.JOB_REMOVED_FROM_JOB_BOARD, OnJobRemovedFromJobBoard);
        Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        DatabaseManager.Instance.partyDatabase.RemoveParty(this);
        Messenger.Broadcast(PartySignals.PARTY_DESTROYED, this);
    }
#endregion
        
#region Subscribe/Unsubscribe
    public void Subscribe(PartyEventsIListener p_iListener) {
        onQuestFailed += p_iListener.OnQuestFailed;
        onQuestSucceed += p_iListener.OnQuestSucceed;
    }

    public void Unsubscribe(PartyEventsIListener p_iListener) {
        onQuestFailed -= p_iListener.OnQuestFailed;
        onQuestSucceed -= p_iListener.OnQuestSucceed;
    }
#endregion
}


[System.Serializable]
public class SaveDataParty : SaveData<Party>, ISavableCounterpart {
    public string persistentID { get; set; }
    public string partyName;
    public PARTY_STATE partyState;
    //public int takeQuestSchedule;
    //public int restSchedule;
    //public int endRestSchedule;
    //public bool hasRested;
    public bool startedTrueRestingState;
    public bool isDisbanded;
    //public bool cannotProduceFoodThisRestPeriod;
    public bool hasChangedTargetDestination;
    //public int perHourElapsedInWaiting;
    public string partySettlement;
    public string partyFaction;
    public string meetingPlace;
    public string targetRestingTavern;
    public string targetCamp;
    public string targetDestination;
    public PARTY_TARGET_DESTINATION_TYPE targetDestinationType;
    public string currentQuest;
    public PARTY_QUEST_TYPE prevQuestType;
    public PARTY_QUEST_TYPE plannedPartyType;
    //public bool hasStartedAcceptingQuests;
    public GameDate nextQuestCheckDate;
    //public bool canAcceptQuests;
    //public GameDate canAcceptQuestsAgainDate;
    public GameDate nextWaitingCheckDate;
    public bool hasSetNextSwitchToWaitingStateTrigger;
    public GameDate endQuestDate;
    public bool hasSetEndQuestDate;
    public int chanceToRetreatUponKnockoutOrDeath;

    public string campSetter;
    public string foodProducer;

    public SaveDataJobBoard jobBoard;
    public List<string> forcedCancelJobsOnTickEnded;

    public GameDate waitingEndDate;
    public List<string> members;
    public List<string> membersThatJoinedQuest;
    public List<string> deadmembers;

    //Components
    public SaveDataPartyBeaconComponent beaconComponent;
    public SaveDataPartyDamageAccumulator damageAccumulator;

#region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Party;
#endregion

#region Overrides
    public override void Save(Party data) {
        persistentID = data.persistentID;
        partyName = data.partyName;
        partyState = data.partyState;
        //takeQuestSchedule = data.takeQuestSchedule;
        //restSchedule = data.restSchedule;
        //endRestSchedule = data.endRestSchedule;
        //hasRested = data.hasRested;
        startedTrueRestingState = data.startedTrueRestingState;
        isDisbanded = data.isDisbanded;
        //cannotProduceFoodThisRestPeriod = data.cannotProduceFoodThisRestPeriod;
        hasChangedTargetDestination = data.hasChangedTargetDestination;
        //perHourElapsedInWaiting = data.perHourElapsedInWaiting;
        if(data.partySettlement == null) {
#if UNITY_EDITOR
            string log = "Saving Party Error, No Party Settlement!";
            log += "\nName: " + data.partyName;
            log += "\nFaction: " + data.partyFaction?.name;
            log += "\nState: " + data.partyState.ToString();
            log += "\nDisbanded: " + data.isDisbanded;
            log += "\nMembers:";
            for (int i = 0; i < data.members.Count; i++) {
                log += " " + data.members[i].name;
            }
            log += "\nMembers With Quest:";
            for (int i = 0; i < data.membersThatJoinedQuest.Count; i++) {
                log += " " + data.membersThatJoinedQuest[i].name;
            }
            log += "\nQuest: " + data.currentQuest?.partyQuestType.ToString();
            log += "\nTarget Destination: " + data.targetDestination?.name;
            log += "\nMeeting Place: " + data.meetingPlace?.name;
            log += "\nResting Tavern: " + data.targetRestingTavern?.name;
            log += "\nCamp: " + data.targetCamp?.name;
            Debug.LogError(log);
#endif
        } else {
            partySettlement = data.partySettlement.persistentID;
        }
        partyFaction = data.partyFaction?.persistentID;

        //hasStartedAcceptingQuests = data.hasStartedAcceptingQuests;
        nextQuestCheckDate = data.nextQuestCheckDate;

        //canAcceptQuests = data.canAcceptQuests;
        //canAcceptQuestsAgainDate = data.canAcceptQuestsAgainDate;

        hasSetNextSwitchToWaitingStateTrigger = data.hasSetNextSwitchToWaitingStateTrigger;
        nextWaitingCheckDate = data.nextWaitingCheckDate;

        hasSetEndQuestDate = data.hasSetEndQuestDate;
        endQuestDate = data.endQuestDate;

        waitingEndDate = data.waitingEndDate;

        prevQuestType = data.prevQuestType;
        plannedPartyType = data.plannedPartyType;

        chanceToRetreatUponKnockoutOrDeath = data.chanceToRetreatUponKnockoutOrDeath;

        members = SaveUtilities.ConvertSavableListToIDs(data.members);
        membersThatJoinedQuest = SaveUtilities.ConvertSavableListToIDs(data.membersThatJoinedQuest);
        deadmembers = SaveUtilities.ConvertSavableListToIDs(data.deadMembers);

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

        beaconComponent = new SaveDataPartyBeaconComponent();
        beaconComponent.Save(data.beaconComponent);

        damageAccumulator = new SaveDataPartyDamageAccumulator();
        damageAccumulator.Save(data.damageAccumulator);
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