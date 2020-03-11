using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyResourceAmount : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public DestroyResourceAmount() : base(INTERACTION_TYPE.DESTROY_RESOURCE_AMOUNT) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.DEMON };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Destroy Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
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
        object[] otherData = goapNode.otherData;
        int amountToReduce = 0;
        if (otherData != null && otherData.Length == 1) {
            amountToReduce = (int) otherData[0];
        }
        goapNode.descriptionLog.AddToFillers(null, amountToReduce.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterDestroySuccess(ActualGoapNode goapNode) {
        ResourcePile pile = goapNode.poiTarget as ResourcePile;
        object[] otherData = goapNode.otherData;
        int amountToReduce = 0;
        if(otherData != null && otherData.Length == 1) {
            amountToReduce = (int) otherData[0];
        }
        pile.AdjustResourceInPile(-amountToReduce);
        goapNode.actor.behaviourComponent.SetIsRaiding(false, null);
    }
    #endregion
}
