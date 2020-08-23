using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class HuntHeirloom : GoapAction {

    public HuntHeirloom() : base(INTERACTION_TYPE.HUNT_HEIRLOOM) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Hunt Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return !actor.partyComponent.hasParty;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterRescueSuccess(ActualGoapNode goapNode) {
        Party party = CharacterManager.Instance.CreateNewParty(PARTY_TYPE.Heirloom_Hunt, goapNode.actor);
        HeirloomHuntParty heirloomParty = party as HeirloomHuntParty;
        heirloomParty.SetTargetHeirloom(goapNode.poiTarget as Heirloom);
        heirloomParty.SetRegionToSearch(goapNode.otherData[0] as Region);
    }
    #endregion

}