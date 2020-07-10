using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class RescueParty : Party {

    public Character targetCharacter { get; private set; }
    private bool isReleasing;

    #region getters
    public override IPartyTarget target => targetCharacter;
    #endregion

    public RescueParty() : base(PARTY_TYPE.Rescue) {
        minimumPartySize = 1;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(4);
        relatedBehaviour = typeof(RescueBehaviour);
        jobQueueOwnerType = JOB_OWNER.FACTION;
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        return (character.characterClass.IsCombatant() && character.characterClass.identifier == "Normal") || character.characterClass.className == "Noble"
            || (character.isNormalCharacter && character.relationshipContainer.GetOpinionLabel(targetCharacter) == RelationshipManager.Close_Friend);
    }
    protected override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        for (int i = 0; i < members.Count; i++) {
            Character member = members[i];
            member.traitContainer.AddTrait(member, "Travelling");
        }
    }
    protected override void OnAddMember(Character member) {
        base.OnAddMember(member);
        member.movementComponent.SetEnableDigging(true);
    }
    protected override void OnRemoveMember(Character member) {
        base.OnRemoveMember(member);
        member.movementComponent.SetEnableDigging(false);
        member.traitContainer.RemoveTrait(member, "Travelling");
    }
    protected override void OnDisbandParty() {
        base.OnDisbandParty();
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_ARRIVED_AT_STRUCTURE)) {
            Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        }
    }
    #endregion

    #region General
    private void ProcessDisbandment() {
        if(isReleasing) { return; }
        DisbandParty();
    }
    public void SetTargetCharacter(Character character) {
        targetCharacter = character;
    }
    public void SetIsReleasing(bool state) {
        isReleasing = state;
    }
    #endregion

    #region Extermination Timer
    private void StartSearchTimer() {
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
        SchedulingManager.Instance.AddEntry(dueDate, DoneSearching, this);
    }
    private void DoneSearching() {
        ProcessDisbandment();
    }
    #endregion

    #region Listeners
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if (targetCharacter.currentStructure == structure) {
            if (IsMember(character)) {
                StartSearchTimer();
            }
        }
    }
    #endregion
}
