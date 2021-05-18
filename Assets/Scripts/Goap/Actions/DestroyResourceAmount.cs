using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyResourceAmount : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public DestroyResourceAmount() : base(INTERACTION_TYPE.DESTROY_RESOURCE_AMOUNT) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.DEMON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Combat};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Destroy Success", goapNode);
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
    //protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
    //    if (satisfied) {
    //    }
    //    return false;
    //}
#endregion

#region State Effects
    public void PreDestroySuccess(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        int amountToReduce = 0;
        if (otherData != null && otherData.Length == 1) {
            amountToReduce = (int) otherData[0].obj;
        }
        goapNode.descriptionLog.AddToFillers(null, amountToReduce.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterDestroySuccess(ActualGoapNode goapNode) {
        ResourcePile pile = goapNode.poiTarget as ResourcePile;
        OtherData[] otherData = goapNode.otherData;
        int amountToReduce = 0;
        if(otherData != null && otherData.Length == 1) {
            amountToReduce = (int) otherData[0].obj;
        }
        pile.AdjustResourceInPile(-amountToReduce);
    }
#endregion
}
