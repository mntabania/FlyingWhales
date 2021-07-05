using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class ExterminationPartyQuest : PartyQuest {

    public LocationStructure targetStructure { get; private set; }
    //public HexTile waitingArea { get; private set; }
    //public bool isExterminating { get; private set; }
    public NPCSettlement originSettlement { get; private set; }

    #region getters
    public override IPartyQuestTarget target => targetStructure;
    //public override HexTile waitingHexArea => waitingArea;
    public override System.Type serializedData => typeof(SaveDataExterminationPartyQuest);
    public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;
    #endregion

    public ExterminationPartyQuest() : base(PARTY_QUEST_TYPE.Extermination) {
        minimumPartySize = 3;
        //waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(ExterminateBehaviour);
        //jobQueueOwnerType = JOB_OWNER.FACTION;
    }
    public ExterminationPartyQuest(SaveDataExterminationPartyQuest data) : base(data) {
        //isExterminating = data.isExterminating;
    }

    #region Overrides
    //public override bool IsAllowedToJoin(Character character) {
    //    return (character.characterClass.IsCombatant() && character.characterClass.identifier == "Normal") || character.characterClass.className == "Noble";
    //}
    public override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        if (targetStructure == null || targetStructure.hasBeenDestroyed || targetStructure.tiles.Count <= 0) {
            EndQuest("Target is destroyed");
        } else {
            Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
            //Removed this because the quest should start timer when the characters arrived at target location
            //StartExterminationTimer();
        }
    }
    public override IPartyTargetDestination GetTargetDestination() {
        return targetStructure;
    }
    public override string GetPartyQuestTextInLog() {
        return "Exterminate " + targetStructure.name;
    }
    public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
        base.OnAssignedPartySwitchedState(fromState, toState);
        if (toState == PARTY_STATE.Working) {
            SetIsSuccessful(true);
            //StartExterminationTimer();
        }
    }
    //protected override void OnAddMember(Character member) {
    //    base.OnAddMember(member);
    //    member.movementComponent.SetEnableDigging(true);
    //    member.traitContainer.AddTrait(member, "Travelling");
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
        //isExterminating = false;
        //if (originSettlement.exterminateTargetStructure == targetStructure) {
        //    originSettlement.SetExterminateTarget(null);
        //}
        if (Messenger.eventTable.ContainsKey(StructureSignals.STRUCTURE_DESTROYED)) {
            Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        }
    }
    #endregion

    #region General
    private void ProcessExterminationOrDisbandment() {
        if (assignedParty != null && assignedParty.isActive && assignedParty.currentQuest == this) {
            Faction faction = assignedParty.partySettlement.owner;
            if (targetStructure == null 
                || targetStructure.hasBeenDestroyed 
                || targetStructure.tiles.Count <= 0 
                || !targetStructure.settlementLocation.HasResidentForExterminationPartyQuest(targetStructure.settlementLocation, faction, assignedParty)) {
                assignedParty.GoBackHomeAndEndQuest();
            } else {
                StartExterminationTimer();
            }
        }
    }
    public void SetTargetStructure(LocationStructure structure) {
        if (targetStructure != structure) {
            targetStructure = structure;
            //if (targetStructure != null) {
            //    SetWaitingArea();
            //}
        }
    }
    public void SetOriginSettlement(NPCSettlement settlement) {
        originSettlement = settlement;
    }
    //private void SetWaitingArea() {
    //    waitingArea = targetStructure.settlementLocation.GetAPlainAdjacentHextile();
    //}
    private void OnStructureDestroyed(LocationStructure structure) {
        if (targetStructure == structure) {
            EndQuest("Target is destroyed");
        }
    }
    #endregion

    #region Extermination Timer
    private void StartExterminationTimer() {
        //if (!isExterminating) {
        //    isExterminating = true;
        //    GameDate dueDate = GameManager.Instance.Today();
        //    dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30));
        //    SchedulingManager.Instance.AddEntry(dueDate, DoneExterminationTimer, this);
        //}
    }
    private void DoneExterminationTimer() {
        //if (isExterminating) {
        //    isExterminating = false;
        //    ProcessExterminationOrDisbandment();
        //}
    }
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataPartyQuest data) {
        base.LoadReferences(data);
        if (data is SaveDataExterminationPartyQuest subData) {
            if (!string.IsNullOrEmpty(subData.targetStructure)) {
                targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.targetStructure);
            }
            //if (!string.IsNullOrEmpty(subData.waitingArea)) {
            //    waitingArea = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(subData.waitingArea);
            //}
            if (!string.IsNullOrEmpty(subData.originSettlement)) {
                originSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(subData.originSettlement) as NPCSettlement;
            }
            if (isWaitTimeOver && assignedParty != null) {
                //Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
                Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataExterminationPartyQuest : SaveDataPartyQuest {
    public string targetStructure;
    //public string waitingArea;
    //public bool isExterminating;
    public string originSettlement;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        if (data is ExterminationPartyQuest subData) {
            //isExterminating = subData.isExterminating;

            if (subData.targetStructure != null) {
                targetStructure = subData.targetStructure.persistentID;
            }
            //if (subData.waitingArea != null) {
            //    waitingArea = subData.waitingArea.persistentID;
            //}
            if (subData.originSettlement != null) {
                originSettlement = subData.originSettlement.persistentID;
            }
        }
    }
    #endregion
}