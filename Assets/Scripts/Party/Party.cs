using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class Party : ISavable {
    public string persistentID { get; private set; }
    public string partyName { get; private set; }
    public PARTY_STATE partyState { get; private set; }
    public int takeQuestSchedule { get; private set; }
    public int restSchedule { get; private set; }
    //public int endRestSchedule { get; private set; }
    public bool hasRested { get; private set; }
    public bool isDisbanded { get; private set; }
    public bool hasChangedTargetDestination { get; private set; }
    public int perHourElapsedInWaiting { get; private set; }
    public BaseSettlement partySettlement { get; private set; }
    public LocationStructure meetingPlace { get; private set; }
    public LocationStructure targetRestingTavern { get; private set; }
    public HexTile targetCamp { get; private set; }
    public IPartyTargetDestination targetDestination { get; private set; }
    public PartyQuest currentQuest { get; private set; }

    public Character campSetter { get; private set; }
    public Character foodProducer { get; private set; }

    public GameDate waitingEndDate { get; private set; }
    public List<Character> members { get; private set; }
    public List<Character> membersThatJoinedQuest { get; private set; }

    public bool cannotProduceFoodThisRestPeriod { get; private set; }

    private List<Character> _activeMembers;

    #region getters
    public OBJECT_TYPE objectType => OBJECT_TYPE.Party;
    public System.Type serializedData => typeof(SaveDataParty);
    public bool isActive => currentQuest != null;
    public List<Character> activeMembers => GetActiveMembers();
    #endregion

    public Party() {
        members = new List<Character>();
        membersThatJoinedQuest = new List<Character>();
        _activeMembers = new List<Character>();
    }

    public void Initialize(Character partyCreator) { //In order to create a party, there must always be a party creator
        if (string.IsNullOrEmpty(persistentID)) {
            persistentID = UtilityScripts.Utilities.GetNewUniqueID();
        }
        partyName = PartyManager.Instance.GetNewPartyName(partyCreator);
        partySettlement = partyCreator.homeSettlement;
        isDisbanded = false;
        hasRested = true;
        perHourElapsedInWaiting = 0;

        SetPartyState(PARTY_STATE.None);
        SetTakeQuestSchedule();
        SetRestSchedule();
        //SetEndRestSchedule();

        AddMember(partyCreator);
        partySettlement.AddParty(this);
        Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
        DatabaseManager.Instance.partyDatabase.AddParty(this);
    }

    public void Initialize(SaveDataParty data) { //In order to create a party, there must always be a party creator
        persistentID = data.persistentID;
        partyName = data.partyName;
        partyState = data.partyState;
        takeQuestSchedule = data.takeQuestSchedule;
        restSchedule = data.restSchedule;
        //endRestSchedule = data.endRestSchedule;
        hasRested = data.hasRested;
        isDisbanded = data.isDisbanded;
        cannotProduceFoodThisRestPeriod = data.cannotProduceFoodThisRestPeriod;
        hasChangedTargetDestination = data.hasChangedTargetDestination;
        perHourElapsedInWaiting = data.perHourElapsedInWaiting;
        waitingEndDate = data.waitingEndDate;

        if (partyName != string.Empty) {
            Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_DESTROYED, OnStructureDestroyed);
            Messenger.AddListener(Signals.TICK_ENDED, OnTickEnded);
            DatabaseManager.Instance.partyDatabase.AddParty(this);
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
    }
    #endregion

    #region General
    private void SetTakeQuestSchedule() {
        takeQuestSchedule = GameManager.GetRandomTickFromTimeInWords(TIME_IN_WORDS.MORNING);
    }
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
        if (takeQuestSchedule == GameManager.Instance.currentTick) {
            PartyQuest quest = partySettlement.GetFirstUnassignedPartyQuest();
            if(quest != null) {
                AcceptQuest(quest);
            }
        }
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
        SetPartyState(PARTY_STATE.Moving);
        SetTargetDestination(partySettlement);
    }
    public void SetTargetDestination(IPartyTargetDestination target) {
        if(targetDestination != target) {
            targetDestination = target;
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
    public void SetPartyState(PARTY_STATE state) {
        if (partyState != state) {
            PARTY_STATE prevState = partyState;
            partyState = state;
            OnSwitchToState(state, prevState);
            if (isActive) {
                currentQuest.OnAssignedPartySwitchedState(prevState, partyState);
            }
        }
    }
    private void OnSwitchToState(PARTY_STATE state, PARTY_STATE prevState) {
        CancellAllPartyGoToJobsOfMembers();
        if (state == PARTY_STATE.None) {
            OnSwitchToNoneState(prevState);
        } else if (state == PARTY_STATE.Waiting) {
            OnSwitchToWaitingState(prevState);
        } else if (state == PARTY_STATE.Moving) {
            OnSwitchToMovingState(prevState);
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
    #endregion  

    #region Waiting State
    private void OnSwitchToWaitingState(PARTY_STATE prevState) {
        CancelAllTirednessRecoveryJobsOfMembers();
        SetMeetingPlace();
        StartWaitTimer();
    }
    private void StartWaitTimer() {
        perHourElapsedInWaiting = 0;
        Messenger.AddListener(Signals.HOUR_STARTED, WaitingPerHour);
        waitingEndDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(5));
        SchedulingManager.Instance.AddEntry(waitingEndDate, WaitingEndedDecisionMaking, this);
    }
    private void WaitingPerHour() {
        perHourElapsedInWaiting++;
        if(perHourElapsedInWaiting > 2) {
            if (isActive && membersThatJoinedQuest.Count >= currentQuest.minimumPartySize) {
                WaitingEndedDecisionMaking();
            }
        }
    }
    private void WaitingEndedDecisionMaking() {
        if (partyState == PARTY_STATE.Waiting && !isDisbanded && isActive) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, WaitingPerHour);
            //PopulateMembersThatJoinedQuest();
            if(membersThatJoinedQuest.Count >= currentQuest.minimumPartySize) {
                SetPartyState(PARTY_STATE.Moving);
            } else {
                //Drop quest only instead of ending quest so that the quest can still be taken by other parties
                currentQuest.EndQuest();
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
    private void OnSwitchToMovingState(PARTY_STATE prevState) {
        SetTargetDestination(currentQuest.GetTargetDestination());
        if(prevState == PARTY_STATE.Waiting) {
            //DistributeQuestToMembersThatJoinedParty();
            CancelAllJobsOfMembersThatJoinedQuest();
        } else {
            CancelAllJobsOfMembersThatJoinedQuestThatAreStillActive();
        }
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
            Messenger.AddListener(Signals.HOUR_STARTED, RestingPerHour);
        }
    }
    private void RestingPerHour() {
        if (!HasActiveMemberThatMustDoNeedsRecovery()) {
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
            List<HexTile> nearbyHexes = activeMemberCurrentHex.GetTilesInRange(3);
            if (nearbyHexes != null && nearbyHexes.Count > 0) {
                for (int i = 0; i < nearbyHexes.Count; i++) {
                    HexTile hex = nearbyHexes[i];
                    BaseSettlement settlement;
                    if (hex.IsPartOfVillage(out settlement)) {
                        if(settlement == partySettlement && targetDestination == partySettlement) {
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
                    }
                }
            }

            if (targetRestingTavern == null) {
                targetCamp = activeMemberCurrentHex;
            }
        }
    }
    public void SetCampSetter(Character character) {
        campSetter = character;
    }
    public void SetFoodProducer(Character character) {
        foodProducer = character;
    }
    #endregion

    #region Working State
    private void OnSwitchToWorkingState(PARTY_STATE prevState) {
        SetHasChangedTargetDestination(false);
    }
    #endregion

    #region Quest
    private void AcceptQuest(PartyQuest quest) {
        if (!isActive && quest != null) {
            currentQuest = quest;
            currentQuest.SetAssignedParty(this);
            SetPartyState(PARTY_STATE.Waiting);
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
    public void DropQuest() {
        if (isActive) {
            ClearMembersThatJoinedQuest();
            partySettlement.RemovePartyQuest(currentQuest);
            SetPartyState(PARTY_STATE.None);
            currentQuest.SetAssignedParty(null);
            currentQuest = null;
            targetRestingTavern = null;
            meetingPlace = null;
            targetCamp = null;
            targetDestination = null;
        }
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
    public void ClearMembersThatJoinedQuest() {
        while (membersThatJoinedQuest.Count > 0) {
            RemoveMemberThatJoinedQuest(membersThatJoinedQuest[0]);
        }
        membersThatJoinedQuest.Clear();
    }
    public bool RemoveMemberThatJoinedQuest(Character character) {
        if (membersThatJoinedQuest.Remove(character)) {
            OnRemoveMemberThatJoinedQuest(character);
            return true;
        }
        return false;
    }
    private void OnAddMemberThatJoinedQuest(Character character) {
        character.movementComponent.SetEnableDigging(true);
        character.traitContainer.AddTrait(character, "Travelling");
        character.behaviourComponent.AddBehaviourComponent(currentQuest.relatedBehaviour);
    }
    private void OnRemoveMemberThatJoinedQuest(Character character) {
        character.movementComponent.SetEnableDigging(false);
        character.traitContainer.RemoveTrait(character, "Travelling");
        character.behaviourComponent.RemoveBehaviourComponent(currentQuest.relatedBehaviour);
        if (isActive) {
            currentQuest.OnRemoveMemberThatJoinedQuest(character);
        }
    }
    private void OnAddMember(Character character) {
        character.partyComponent.SetCurrentParty(this);
        character.behaviourComponent.AddBehaviourComponent(typeof(PartyBehaviour));
    }
    private void OnRemoveMember(Character character) {
        character.partyComponent.SetCurrentParty(null);
        character.behaviourComponent.RemoveBehaviourComponent(typeof(PartyBehaviour));
        character.jobQueue.CancelAllPartyJobs();
        RemoveMemberThatJoinedQuest(character);
    }
    private void OnRemoveMemberOnDisband(Character character) {
        character.partyComponent.SetCurrentParty(null);
        character.behaviourComponent.RemoveBehaviourComponent(typeof(PartyBehaviour));
        character.jobQueue.CancelAllPartyJobs();
        RemoveMemberThatJoinedQuest(character);
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
    #endregion

    #region Disbandment
    public void DisbandParty() {
        if (isDisbanded) { return; }
        for (int i = 0; i < members.Count; i++) {
            OnRemoveMemberOnDisband(members[i]);
        }
        members.Clear();
        OnDisbandParty();
    }
    private void OnDisbandParty() {
        Log log = new Log(GameManager.Instance.Today(), "Party", "General", "disband", providedTags: LOG_TAG.Party);
        log.AddToFillers(null, partyName, LOG_IDENTIFIER.STRING_1);
        log.AddLogToDatabase();

        isDisbanded = true;
        DestroyParty();
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataParty data) {
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
        }
        if (!string.IsNullOrEmpty(data.campSetter)) {
            campSetter = CharacterManager.Instance.GetCharacterByPersistentID(data.campSetter);
        }
        if (!string.IsNullOrEmpty(data.foodProducer)) {
            foodProducer = CharacterManager.Instance.GetCharacterByPersistentID(data.foodProducer);
        }
        if(data.members != null) {
            members = SaveUtilities.ConvertIDListToCharacters(data.members);
        }
        if (data.membersThatJoinedQuest != null) {
            membersThatJoinedQuest = SaveUtilities.ConvertIDListToCharacters(data.membersThatJoinedQuest);
        }
        if (!string.IsNullOrEmpty(data.partySettlement)) {
            partySettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.partySettlement);
        }
        
        if ((targetRestingTavern != null || targetCamp != null) && partyState == PARTY_STATE.Resting) {
            Messenger.AddListener(Signals.HOUR_STARTED, RestingPerHour);
        } else if (partyState == PARTY_STATE.Waiting) {
            Messenger.AddListener(Signals.HOUR_STARTED, WaitingPerHour);
        }
    }
    #endregion

    #region Object Pool
    private void DestroyParty() {
        ObjectPoolManager.Instance.ReturnPartyToPool(this);
    }
    public void Reset() {
        partySettlement.RemoveParty(this);
        partyName = string.Empty;
        partyState = PARTY_STATE.None;
        takeQuestSchedule = -1;
        restSchedule = -1;
        hasRested = false;
        partySettlement = null;
        targetRestingTavern = null;
        targetCamp = null;
        targetDestination = null;
        currentQuest = null;
        meetingPlace = null;
        campSetter = null;
        foodProducer = null;
        cannotProduceFoodThisRestPeriod = false;
        hasChangedTargetDestination = false;
        perHourElapsedInWaiting = 0;
        members.Clear();
        ClearMembersThatJoinedQuest();
        _activeMembers.Clear();
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
    public int takeQuestSchedule;
    public int restSchedule;
    //public int endRestSchedule;
    public bool hasRested;
    public bool isDisbanded;
    public bool cannotProduceFoodThisRestPeriod;
    public bool hasChangedTargetDestination;
    public int perHourElapsedInWaiting;
    public string partySettlement;
    public string meetingPlace;
    public string targetRestingTavern;
    public string targetCamp;
    public string targetDestination;
    public PARTY_TARGET_DESTINATION_TYPE targetDestinationType;
    public string currentQuest;

    public string campSetter;
    public string foodProducer;

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
        takeQuestSchedule = data.takeQuestSchedule;
        restSchedule = data.restSchedule;
        //endRestSchedule = data.endRestSchedule;
        hasRested = data.hasRested;
        isDisbanded = data.isDisbanded;
        cannotProduceFoodThisRestPeriod = data.cannotProduceFoodThisRestPeriod;
        hasChangedTargetDestination = data.hasChangedTargetDestination;
        perHourElapsedInWaiting = data.perHourElapsedInWaiting;
        partySettlement = data.partySettlement.persistentID;

        waitingEndDate = data.waitingEndDate;

        members = SaveUtilities.ConvertSavableListToIDs(data.members);
        membersThatJoinedQuest = SaveUtilities.ConvertSavableListToIDs(data.membersThatJoinedQuest);

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
        if (data.campSetter != null) {
            campSetter = data.campSetter.persistentID;
        }
        if (data.foodProducer != null) {
            foodProducer = data.foodProducer.persistentID;
        }
    }

    public override Party Load() {
        Party party = PartyManager.Instance.CreateNewParty(this);
        return party;
    }
    #endregion
}