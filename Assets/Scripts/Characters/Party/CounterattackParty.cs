using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class CounterattackParty : Party {

    public LocationStructure targetStructure { get; private set; }

    #region getters
    public override IPartyTarget target => leader.behaviourComponent.attackDemonicStructureTarget;
    #endregion

    public CounterattackParty() : base(PARTY_TYPE.Counterattack) {
        minimumPartySize = 3;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(2);
        relatedBehaviour = typeof(AttackDemonicStructureBehaviour);
        jobQueueOwnerType = JOB_OWNER.FACTION;
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        return (character.characterClass.IsCombatant() && character.characterClass.identifier == "Normal") || character.characterClass.className == "Noble";
    }
    protected override void OnWaitTimeOverButPartyIsDisbanded() {
        base.OnWaitTimeOverButPartyIsDisbanded();
    }
    protected override void OnAddMember(Character member) {
        base.OnAddMember(member);
        member.movementComponent.SetEnableDigging(true);
    }
    protected override void OnRemoveMember(Character member) {
        base.OnRemoveMember(member);
        member.movementComponent.SetEnableDigging(false);
    }
    #endregion

    #region General
    public void SetTargetStructure(LocationStructure structure) {
        if(targetStructure != structure) {
            targetStructure = structure;
        }
    }
    #endregion
}
