using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class BuildTrollCauldron : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public BuildTrollCauldron() : base(INTERACTION_TYPE.BUILD_TROLL_CAULDRON) {
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        actionIconString = GoapActionStateDB.Build_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Build Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override void PopulateNearbyLocation(List<LocationGridTile> gridTiles, ActualGoapNode goapNode) {
        goapNode.actor.gridTileLocation.PopulateTilesInRadius(gridTiles, 3, includeImpassable: false, includeTilesWithObject: false);
    }
#endregion

#region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
#endregion

#region Effects
    public void AfterBuildSuccess(ActualGoapNode goapNode) {
        TrollCauldron cauldron = InnerMapManager.Instance.CreateNewTileObject<TrollCauldron>(TILE_OBJECT_TYPE.TROLL_CAULDRON);
        goapNode.actor.gridTileLocation.structure.AddPOI(cauldron, goapNode.actor.gridTileLocation);
        // cauldron.logComponent.AddHistory(goapNode.descriptionLog);
    }
#endregion
}