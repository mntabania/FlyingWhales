using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class ExplorationPartyQuest : PartyQuest {

    public LocationStructure targetStructure { get; private set; }

    //public List<LocationStructure> alreadyExplored { get; private set; }
    public bool isExploring { get; private set; }
    public GameDate expiryDate { get; private set; }
    //public int currentChance { get; private set; }
    //public Region regionRefForGettingNewStructure { get; private set; }

    #region getters
    public override IPartyQuestTarget target => targetStructure;
    public override System.Type serializedData => typeof(SaveDataExplorationPartyQuest);
    public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;
    #endregion

    public ExplorationPartyQuest() : base(PARTY_QUEST_TYPE.Exploration) {
        minimumPartySize = 3;
        //waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(ExploreBehaviour);
        //jobQueueOwnerType = JOB_OWNER.SETTLEMENT;
        //alreadyExplored = new List<LocationStructure>();
    }
    public ExplorationPartyQuest(SaveDataExplorationPartyQuest data) : base(data) {
        //alreadyExplored = new List<LocationStructure>();
        isExploring = data.isExploring;
        expiryDate = data.expiryDate;
        //currentChance = data.currentChance;
    }

    #region Overrides
    //public override void OnAcceptQuest(Party partyThatAcceptedQuest) {
    //    base.OnAcceptQuest(partyThatAcceptedQuest);
    //    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerPerform);
    //    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterNoLongerMove);
    //}
    //public override void OnAcceptQuestFromSaveData(Party partyThatAcceptedQuest) {
    //    base.OnAcceptQuestFromSaveData(partyThatAcceptedQuest);
    //    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerPerform);
    //    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterNoLongerMove);
    //}
    public override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        //currentChance = 100;
        //Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        //ProcessExplorationOrDisbandment();
    }
    public override IPartyTargetDestination GetTargetDestination() {
        return targetStructure;
    }
    public override string GetPartyQuestTextInLog() {
        return "Explore Regions Quest";
    }
    protected override void OnEndQuest() {
        base.OnEndQuest();
        //Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerPerform);
        //Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterNoLongerMove);
        //if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_ARRIVED_AT_STRUCTURE)) {
        //    Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        //}
        if (Messenger.eventTable.ContainsKey(StructureSignals.STRUCTURE_DESTROYED)) {
            Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        }
    }
    public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
        base.OnAssignedPartySwitchedState(fromState, toState);
        if (toState == PARTY_STATE.Working) {
            SetIsSuccessful(true);
            StartExplorationTimer();
        }
    }
    #endregion

    #region General
    private void ProcessExplorationOrDisbandment() {
        if (assignedParty != null && assignedParty.isActive && assignedParty.currentQuest == this) {
            EndQuest("Finished quest");
            //if (GameUtilities.RollChance(currentChance)) {
            //    ProcessSettingTargetStructure();
            //    if (targetStructure == null) {
            //        assignedParty.GoBackHomeAndEndQuest();
            //    } else {
            //        assignedParty.SetPartyState(PARTY_STATE.Moving);
            //    }
            //} else {
            //    assignedParty.GoBackHomeAndEndQuest();
            //}
        }
    }
    //public void SetRegionRefForGettingNewStructure(Region region) {
    //    regionRefForGettingNewStructure = region;
    //}
    //public void ProcessSettingTargetStructure() {
    //    //List<Region> adjacentRegions = new List<Region>(regionRefForGettingNewStructure.neighbours);
    //    LocationStructure target = null;
    //    //if (adjacentRegions != null) {
    //    //    adjacentRegions.Add(regionRefForGettingNewStructure);
    //    //    while (target == null && adjacentRegions.Count > 0) {
    //    //        Region chosenRegion = adjacentRegions[UnityEngine.Random.Range(0, adjacentRegions.Count)];
    //    //        //target = chosenRegion.GetRandomSpecialStructure();
    //    //        target = chosenRegion.GetRandomSpecialStructureExcept(alreadyExplored);
    //    //        if (target == null) {
    //    //            adjacentRegions.Remove(chosenRegion);
    //    //        }
    //    //    }
    //    //} else {
    //    //    //target = regionRefForGettingNewStructure.GetRandomSpecialStructure();
    //    //    target = regionRefForGettingNewStructure.GetRandomSpecialStructureExcept(alreadyExplored);
    //    //}
    //    target = regionRefForGettingNewStructure.GetRandomSpecialStructureExcept(alreadyExplored);
    //    SetTargetStructure(target);
    //}
    public void SetTargetStructure(LocationStructure structure) {
        if(targetStructure != structure) {
            targetStructure = structure;
            //if (targetStructure != null) {
            //    alreadyExplored.Add(targetStructure);
            //    regionRefForGettingNewStructure = targetStructure.region;
            //}
        }
    }
    #endregion

    #region Exploration Timer
    private void StartExplorationTimer() {
        if (!isExploring) {
            isExploring = true;
            expiryDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
            SchedulingManager.Instance.AddEntry(expiryDate, DoneExplorationTimer, this);
        }
    }
    private void DoneExplorationTimer() {
        if (isExploring) {
            isExploring = false;
            //currentChance -= 35;
            ProcessExplorationOrDisbandment();
        }
    }
    #endregion

    #region Listeners
    private void OnStructureDestroyed(LocationStructure structure) {
        if (targetStructure == structure) {
            EndQuest("Structure is destroyed");
        }
    }
    //private void OnCharacterNoLongerPerform(Character character) {
    //    if (character.limiterComponent.canMove) {
    //        //If character can still move even if he/she cannot perform, do not end quest
    //        //In order for the quest to be ended, character must be both cannot perform and move
    //        //The reason is so the quest will not end if the character only sleeps or rests
    //        return;
    //    }
    //    if (GameUtilities.RollChance(15)) {
    //        if (assignedParty != null && assignedParty.membersThatJoinedQuest.Contains(character)) {
    //            EndQuest(character.name + " is incapacitated");
    //            return;
    //        }
    //    }
    //    if (assignedParty != null) {
    //        if (assignedParty.DidMemberJoinQuest(character) && !assignedParty.HasActiveMemberThatJoinedQuest()) {
    //            EndQuest("Members are incapacitated");
    //        }
    //    }
    //}
    //private void OnCharacterNoLongerMove(Character character) {
    //    if (character.limiterComponent.canPerform) {
    //        //If character can still perform even if he/she cannot move, do not end quest
    //        //In order for the quest to be ended, character must be both cannot perform and move
    //        //The reason is so the quest will not end if the character only sleeps or rests
    //        return;
    //    }
    //    if (GameUtilities.RollChance(15)) {
    //        if (assignedParty != null && assignedParty.membersThatJoinedQuest.Contains(character)) {
    //            EndQuest(character.name + " is incapacitated");
    //            return;
    //        }
    //    }
    //    if (assignedParty != null) {
    //        if (assignedParty.DidMemberJoinQuest(character) && !assignedParty.HasActiveMemberThatJoinedQuest()) {
    //            EndQuest("Members are incapacitated");
    //        }
    //    }
    //}
    //public override void OnCharacterDeath(Character p_character) {
    //    base.OnCharacterDeath(p_character);
    //    if (GameUtilities.RollChance(25)) {
    //        if (assignedParty != null && assignedParty.membersThatJoinedQuest.Contains(p_character)) {
    //            EndQuest(p_character.name + " died");
    //        }
    //    }
    //}
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataPartyQuest data) {
        base.LoadReferences(data);
        if (data is SaveDataExplorationPartyQuest subData) {
            if (!string.IsNullOrEmpty(subData.targetStructure)) {
                targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.targetStructure);
            }
            //for (int i = 0; i < subData.alreadyExplored.Count; i++) {
            //    LocationStructure structure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.alreadyExplored[i]);
            //    alreadyExplored.Add(structure);
            //}
            //if (!string.IsNullOrEmpty(subData.regionRefForGettingNewStructure)) {
            //    regionRefForGettingNewStructure = DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(subData.regionRefForGettingNewStructure);
            //}
            if (isWaitTimeOver && assignedParty != null) {
                //Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
                Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
            }
            if (isExploring) {
                SchedulingManager.Instance.AddEntry(expiryDate, DoneExplorationTimer, this);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataExplorationPartyQuest : SaveDataPartyQuest {
    public string targetStructure;
    //public List<string> alreadyExplored;
    public bool isExploring;
    public GameDate expiryDate;
    //public int currentChance;
    //public string regionRefForGettingNewStructure;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        if (data is ExplorationPartyQuest subData) {
            if (subData.targetStructure != null) {
                targetStructure = subData.targetStructure.persistentID;
            }

            //alreadyExplored = new List<string>();
            //for (int i = 0; i < subData.alreadyExplored.Count; i++) {
            //    alreadyExplored.Add(subData.alreadyExplored[i].persistentID);
            //}

            isExploring = subData.isExploring;
            expiryDate = subData.expiryDate;
            //currentChance = subData.currentChance;

            //if (subData.regionRefForGettingNewStructure != null) {
            //    regionRefForGettingNewStructure = subData.regionRefForGettingNewStructure.persistentID;
            //}
        }
    }
    #endregion
}