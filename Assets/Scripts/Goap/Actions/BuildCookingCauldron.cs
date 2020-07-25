using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class BuildCookingCauldron : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public BuildCookingCauldron() : base(INTERACTION_TYPE.BUILD_COOKING_CAULDRON) {
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        actionIconString = GoapActionStateDB.Build_Icon;
        showNotification = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Build Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override List<LocationGridTile> NearbyLocationGetter(ActualGoapNode goapNode) {
        List<LocationGridTile> tiles = goapNode.actor.gridTileLocation.GetTilesInRadius(3);
        for (int i = 0; i < tiles.Count; i++) {
            if(tiles[i].objHere != null) {
                tiles.RemoveAt(i);
                i--;
            }
        }
        return tiles;
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion

    #region Effects
    public void AfterBuildSuccess(ActualGoapNode goapNode) {
        CookingCauldron cauldron = InnerMapManager.Instance.CreateNewTileObject<CookingCauldron>(TILE_OBJECT_TYPE.COOKING_CAULDRON);
        goapNode.actor.gridTileLocation.structure.AddPOI(cauldron, goapNode.actor.gridTileLocation);
    }
    #endregion
}