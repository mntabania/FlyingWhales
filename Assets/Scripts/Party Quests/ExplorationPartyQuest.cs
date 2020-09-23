using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class ExplorationPartyQuest : PartyQuest {

    public LocationStructure targetStructure { get; private set; }

    public List<LocationStructure> alreadyExplored { get; private set; }
    public bool isExploring { get; private set; }
    public int currentChance { get; private set; }
    public Region regionRefForGettingNewStructure { get; private set; }

    #region getters
    public override IPartyQuestTarget target => targetStructure;
    public override System.Type serializedData => typeof(SaveDataExplorationPartyQuest);
    #endregion

    public ExplorationPartyQuest() : base(PARTY_QUEST_TYPE.Exploration) {
        minimumPartySize = 3;
        //waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(ExploreBehaviour);
        //jobQueueOwnerType = JOB_OWNER.SETTLEMENT;
        alreadyExplored = new List<LocationStructure>();
    }
    public ExplorationPartyQuest(SaveDataExplorationPartyQuest data) : base(data) {
        alreadyExplored = new List<LocationStructure>();
        isExploring = data.isExploring;
        currentChance = data.currentChance;
    }

    #region Overrides
    //public override bool IsAllowedToJoin(Character character) {
    //    return (character.characterClass.IsCombatant() && character.characterClass.identifier == "Normal") || character.characterClass.className == "Noble";
    //}
    public override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        currentChance = 100;
        //Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        ProcessExplorationOrDisbandment();
    }
    public override IPartyTargetDestination GetTargetDestination() {
        return targetStructure;
    }
    public override string GetPartyQuestTextInLog() {
        return "Exploration Quest";
    }
    //protected override void OnAddMember(Character member) {
    //    base.OnAddMember(member);
    //    member.movementComponent.SetEnableDigging(true);
    //}
    //protected override void OnRemoveMember(Character member) {
    //    base.OnRemoveMember(member);
    //    member.movementComponent.SetEnableDigging(false);
    //    member.traitContainer.RemoveTrait(member, "Travelling");
    //}
    //protected override void OnRemoveMemberOnDisband(Character member) {
    //    base.OnRemoveMemberOnDisband(member);
    //    member.movementComponent.SetEnableDigging(false);
    //    member.traitContainer.RemoveTrait(member, "Travelling");
    //}
    protected override void OnEndQuest() {
        base.OnEndQuest();
        //if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_ARRIVED_AT_STRUCTURE)) {
        //    Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        //}
        if (Messenger.eventTable.ContainsKey(Signals.STRUCTURE_DESTROYED)) {
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        }
    }
    public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
        base.OnAssignedPartySwitchedState(fromState, toState);
        if(toState == PARTY_STATE.Working) {
            StartExplorationTimer();
        }
    }
    //protected override void OnSetLeader() {
    //    base.OnSetLeader();
    //    if (leader != null) {
    //        regionRefForGettingNewStructure = leader.currentRegion;
    //    }
    //}
    #endregion

    #region General
    private void ProcessExplorationOrDisbandment() {
        if (GameUtilities.RollChance(currentChance)) {
            ProcessSettingTargetStructure();
            if(targetStructure == null) {
                assignedParty.GoBackHomeAndEndQuest();
            } else {
                assignedParty.SetPartyState(PARTY_STATE.Moving);
            }
        } else {
            assignedParty.GoBackHomeAndEndQuest();
        }
    }
    public void SetRegionRefForGettingNewStructure(Region region) {
        regionRefForGettingNewStructure = region;
    }
    public void ProcessSettingTargetStructure() {
        List<Region> adjacentRegions = regionRefForGettingNewStructure.AdjacentRegions();
        LocationStructure target = null;
        if (adjacentRegions != null) {
            adjacentRegions.Add(regionRefForGettingNewStructure);
            while (target == null && adjacentRegions.Count > 0) {
                Region chosenRegion = adjacentRegions[UnityEngine.Random.Range(0, adjacentRegions.Count)];
                //target = chosenRegion.GetRandomSpecialStructure();
                target = chosenRegion.GetRandomSpecialStructureExcept(alreadyExplored);
                if (target == null) {
                    adjacentRegions.Remove(chosenRegion);
                }
            }
        } else {
            //target = regionRefForGettingNewStructure.GetRandomSpecialStructure();
            target = regionRefForGettingNewStructure.GetRandomSpecialStructureExcept(alreadyExplored);
        }
        SetTargetStructure(target);
    }
    private void SetTargetStructure(LocationStructure structure) {
        if(targetStructure != structure) {
            targetStructure = structure;
            if (targetStructure != null) {
                alreadyExplored.Add(targetStructure);
                regionRefForGettingNewStructure = targetStructure.region;
            }
        }
    }
    #endregion

    #region Exploration Timer
    private void StartExplorationTimer() {
        if (!isExploring) {
            isExploring = true;
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(4));
            SchedulingManager.Instance.AddEntry(dueDate, DoneExplorationTimer, this);
        }
    }
    private void DoneExplorationTimer() {
        if (isExploring) {
            isExploring = false;
            currentChance -= 35;
            ProcessExplorationOrDisbandment();
        }
    }
    #endregion

    #region Listeners
    //private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
    //    if (targetStructure == structure) {
    //        if (character.partyComponent.IsAMemberOfParty(assignedParty)) {
    //            StartExplorationTimer();
    //        }
    //    }
    //}
    private void OnStructureDestroyed(LocationStructure structure) {
        if (targetStructure == structure) {
            ProcessSettingTargetStructure();
            alreadyExplored.Remove(structure);
        }
    }
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataPartyQuest data) {
        base.LoadReferences(data);
        if (data is SaveDataExplorationPartyQuest subData) {
            if (!string.IsNullOrEmpty(subData.targetStructure)) {
                targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.targetStructure);
            }
            for (int i = 0; i < subData.alreadyExplored.Count; i++) {
                LocationStructure structure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.alreadyExplored[i]);
                alreadyExplored.Add(structure);
            }
            if (!string.IsNullOrEmpty(subData.regionRefForGettingNewStructure)) {
                regionRefForGettingNewStructure = DatabaseManager.Instance.regionDatabase.GetRegionByPersistentID(subData.regionRefForGettingNewStructure);
            }
            if (isWaitTimeOver && assignedParty != null) {
                //Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
                Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_DESTROYED, OnStructureDestroyed);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataExplorationPartyQuest : SaveDataPartyQuest {
    public string targetStructure;
    public List<string> alreadyExplored;
    public bool isExploring;
    public int currentChance;
    public string regionRefForGettingNewStructure;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        if (data is ExplorationPartyQuest subData) {
            if (subData.targetStructure != null) {
                targetStructure = subData.targetStructure.persistentID;
            }

            alreadyExplored = new List<string>();
            for (int i = 0; i < subData.alreadyExplored.Count; i++) {
                alreadyExplored.Add(subData.alreadyExplored[i].persistentID);
            }

            isExploring = subData.isExploring;
            currentChance = subData.currentChance;

            if (subData.regionRefForGettingNewStructure != null) {
                regionRefForGettingNewStructure = subData.regionRefForGettingNewStructure.persistentID;
            }
        }
    }
    #endregion
}