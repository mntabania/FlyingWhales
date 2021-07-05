
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;

public class RegainEnergy : GoapAction {

    public RegainEnergy() : base(INTERACTION_TYPE.REGAIN_ENERGY) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.DEMON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Regain Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
#endregion

#region State Effects
    //public void PerTickRegainSuccess(ActualGoapNode goapNode) {
    //    if(goapNode.actor.necromancerTrait != null && goapNode.actor.necromancerTrait.energy < 5) {
    //        goapNode.actor.necromancerTrait.AdjustEnergy(1);
    //    }
    //}
    public void AfterRegainSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.necromancerTrait != null) {
            goapNode.actor.necromancerTrait.AdjustEnergy(2);
        }
    }
    #endregion

}