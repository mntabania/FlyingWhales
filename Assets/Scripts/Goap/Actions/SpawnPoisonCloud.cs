
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;

public class SpawnPoisonCloud : GoapAction {

    public SpawnPoisonCloud() : base(INTERACTION_TYPE.SPAWN_POISON_CLOUD) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.DEMON };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Spawn Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
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
        PoisonCloudTileObject poisonCloudTileObject = new PoisonCloudTileObject();
        poisonCloudTileObject.SetDurationInTicks(GameManager.Instance.GetTicksBasedOnHour(Random.Range(2, 6)));
        poisonCloudTileObject.SetGridTileLocation(goapNode.actor.gridTileLocation);
        poisonCloudTileObject.OnPlacePOI();
        poisonCloudTileObject.SetStacks(UnityEngine.Random.Range(3, 9));
    }
    #endregion

}