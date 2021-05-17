
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;
using UtilityScripts;

public class SpawnPoisonCloud : GoapAction {

    public SpawnPoisonCloud() : base(INTERACTION_TYPE.SPAWN_POISON_CLOUD) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.DEMON };
        logTags = new[] {LOG_TAG.Player};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Spawn Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion

    //#region Requirements
    //protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
    //    if (satisfied) {
    //        return actor.HasItem("Necronomicon");
    //    }
    //    return false;
    //}
    //#endregion

#region State Effects
    public void AfterSpawnSuccess(ActualGoapNode goapNode) {
        InnerMapManager.Instance.SpawnPoisonCloud(goapNode.actor.gridTileLocation, GameUtilities.RandomBetweenTwoNumbers(3, 8), GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(GameUtilities.RandomBetweenTwoNumbers(2, 5))));
    }
#endregion

}