
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class AbsorbLife : GoapAction {

    public AbsorbLife() : base(INTERACTION_TYPE.ABSORB_LIFE) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.DEMON };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.ABSORB_LIFE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Absorb Success", goapNode);
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
            return actor != poiTarget && poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER && poiTarget.mapObjectVisual;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterAbsorbSuccess(ActualGoapNode goapNode) {
        goapNode.actor.necromancerTrait.AdjustLifeAbsorbed(1);
        (goapNode.poiTarget as Character).Death(deathFromAction: goapNode);
    }
    #endregion

}