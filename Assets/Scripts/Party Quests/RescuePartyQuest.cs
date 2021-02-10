using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class RescuePartyQuest : PartyQuest {

    public Character targetCharacter { get; private set; }
    public bool isReleasing { get; private set; }
    public bool isSearching { get; private set; }

    #region getters
    public override IPartyQuestTarget target => targetCharacter;
    public override System.Type serializedData => typeof(SaveDataRescuePartyQuest);
    #endregion

    public RescuePartyQuest() : base(PARTY_QUEST_TYPE.Rescue) {
        minimumPartySize = 1;
        //waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(RescueBehaviour);
        //jobQueueOwnerType = JOB_OWNER.FACTION;
    }
    public RescuePartyQuest(SaveDataRescuePartyQuest data) : base(data) {
        isReleasing = data.isReleasing;
        isSearching = data.isSearching;
    }

    #region Overrides
    public override IPartyTargetDestination GetTargetDestination() {
        if(targetCharacter.currentStructure != null && targetCharacter.currentStructure.structureType != STRUCTURE_TYPE.WILDERNESS) {
            return targetCharacter.currentStructure;
        } else if(targetCharacter.gridTileLocation != null) {
            return targetCharacter.hexTileLocation;
        }
        return base.GetTargetDestination();
    }
    public override string GetPartyQuestTextInLog() {
        return "Rescue " + targetCharacter.name;
    }
    public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
        base.OnAssignedPartySwitchedState(fromState, toState);
        if (toState == PARTY_STATE.Working) {
            StartSearchTimer();
        }
    }
    //public override bool IsAllowedToJoin(Character character) {
    //    return (character.characterClass.IsCombatant() && character.characterClass.identifier == "Normal") || character.characterClass.className == "Noble"
    //        || (character.isNormalCharacter && character.relationshipContainer.GetOpinionLabel(targetCharacter) == RelationshipManager.Close_Friend);
    //}
    //protected override void OnWaitTimeOver() {
    //    base.OnWaitTimeOver();
    //    Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    //    for (int i = 0; i < members.Count; i++) {
    //        Character member = members[i];
    //        member.traitContainer.AddTrait(member, "Travelling");
    //    }
    //}
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
    //protected override void OnDisbandParty() {
    //    base.OnDisbandParty();
    //    if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_ARRIVED_AT_STRUCTURE)) {
    //        Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    //    }
    //}
    #endregion

    #region General
    private void ProcessDisbandment() {
        if (isReleasing) {
            StartSearchTimer();
            return;
        }
        if(assignedParty != null && assignedParty.isActive && assignedParty.currentQuest == this) {
            assignedParty.GoBackHomeAndEndQuest();
        }
    }
    public void SetTargetCharacter(Character character) {
        targetCharacter = character;
    }
    public void SetIsReleasing(bool state) {
        isReleasing = state;
    }
    #endregion

    #region Rescue Timer
    private void StartSearchTimer() {
        if (!isSearching) {
            isSearching = true;
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
            SchedulingManager.Instance.AddEntry(dueDate, DoneSearching, this);
        }
    }
    private void DoneSearching() {
        isSearching = false;
        ProcessDisbandment();
    }
    #endregion

    //#region Listeners
    //private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
    //    if (targetCharacter.currentStructure == structure) {
    //        if (IsMember(character)) {
    //            StartSearchTimer();
    //        }
    //    }
    //}
    //#endregion

    #region Loading
    public override void LoadReferences(SaveDataPartyQuest data) {
        base.LoadReferences(data);
        if (data is SaveDataRescuePartyQuest subData) {
            if (!string.IsNullOrEmpty(subData.targetCharacter)) {
                targetCharacter = CharacterManager.Instance.GetCharacterByPersistentID(subData.targetCharacter);
            }
            //if (isWaitTimeOver && !isDisbanded) {
            //    Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            //}
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataRescuePartyQuest : SaveDataPartyQuest {
    public string targetCharacter;
    public bool isReleasing;
    public bool isSearching;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        if (data is RescuePartyQuest subData) {
            isReleasing = subData.isReleasing;
            isSearching = subData.isSearching;

            if (subData.targetCharacter != null) {
                targetCharacter = subData.targetCharacter.persistentID;
            }
        }
    }
    #endregion
}