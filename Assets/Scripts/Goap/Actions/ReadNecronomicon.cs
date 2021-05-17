
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;

public class ReadNecronomicon : GoapAction {

    public ReadNecronomicon() : base(INTERACTION_TYPE.READ_NECRONOMICON) {
        actionIconString = GoapActionStateDB.Read_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.DEMON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Major};
        showNotification = true;
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Read Success", goapNode);
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
            return actor.HasItem("Necronomicon");
        }
        return false;
    }
#endregion

#region State Effects
    public void PreReadSuccess(ActualGoapNode goapNode) {
        TileObject item = goapNode.actor.GetItem("Necronomicon");
        if(item != null) {
            goapNode.actor.ShowItemVisualCarryingPOI(item);
        }
    }
    public void AfterReadSuccess(ActualGoapNode goapNode) {
        TileObject item = goapNode.actor.GetItem("Necronomicon");
        if (item != null) {
            goapNode.actor.UncarryPOI(item, bringBackToInventory: true);
        }
    }
#endregion

}