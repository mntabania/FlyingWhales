
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
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetDead);
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
            if(poiTarget is Animal) {
                return actor != poiTarget && poiTarget.mapObjectVisual;
            } else if (poiTarget is Summon summon) {
                return actor != poiTarget && poiTarget.mapObjectVisual && summon.isDead;
            }
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool IsTargetDead(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        if (poiTarget is Character character) {
            return character.isDead;
        }
        return true;
    }
    #endregion

    #region State Effects
    public void AfterAbsorbSuccess(ActualGoapNode goapNode) {
        goapNode.actor.necromancerTrait.AdjustLifeAbsorbed(2);
        (goapNode.poiTarget as Character).DestroyMarker();
    }
    #endregion

}