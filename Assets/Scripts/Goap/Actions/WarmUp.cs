using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class WarmUp : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public WarmUp() : base(INTERACTION_TYPE.WARM_UP) {
        actionIconString = GoapActionStateDB.Happy_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Warm Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        int cost = 20;
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
        costLog += $" +20(Initial)";
#endif
        if (target is TileObject tileObject) {
            if (tileObject.characterOwner != null && actor.relationshipContainer.IsEnemiesWith(tileObject.characterOwner)) {
                cost += 2000;
#if DEBUG_LOG
                costLog += $" +2000(Owner is Enemy/Rival)";
#endif
            }
        }
#if DEBUG_LOG
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
#endregion

    //#region Requirement
    //protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
    //    if (satisfied) {
    //        return actor == poiTarget;
    //    }
    //    return false;
    //}
    //#endregion

#region Effects
    public void AfterWarmSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Freezing");
        goapNode.actor.traitContainer.RemoveStatusAndStacks(goapNode.actor, "Frozen");
        if(goapNode.poiTarget is TileObject tileObject) {
            if(tileObject.characterOwner == null) {
                tileObject.SetCharacterOwner(goapNode.actor);
            }
        }
    }
#endregion
}