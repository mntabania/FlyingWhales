using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RemoveRestrained : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public RemoveRestrained() : base(INTERACTION_TYPE.REMOVE_RESTRAINED) {
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Restrained", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Remove Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region State Effects
    public void AfterRemoveSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Restrained");
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor != poiTarget && !poiTarget.traitContainer.HasTrait("Criminal");
        }
        return false;
    }
    #endregion
}