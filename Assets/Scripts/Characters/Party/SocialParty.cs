using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SocialParty : Party {

    public LocationStructure targetStructure { get; private set; }

    #region getters
    public override IPartyTarget target => targetStructure;
    #endregion

    public SocialParty() : base(PARTY_TYPE.Social) {
        minimumPartySize = 5;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(4);
        relatedBehaviour = typeof(SocialPartyBehaviour);
        jobQueueOwnerType = JOB_OWNER.SETTLEMENT;
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        return !leader.relationshipContainer.IsEnemiesWith(character) && !character.traitContainer.HasTrait("Agoraphobic");
    }
    protected override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        DisbandParty();
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
    protected override void OnDisbandParty() {
        base.OnDisbandParty();
        targetStructure.SetHasActiveSocialParty(false);
    }
    #endregion

    #region General
    public void SetTargetStructure(LocationStructure structure) {
        if(targetStructure != structure) {
            targetStructure = structure;
            if(targetStructure != null) {
                targetStructure.SetHasActiveSocialParty(true);
            }
        }
    }
    #endregion
}
