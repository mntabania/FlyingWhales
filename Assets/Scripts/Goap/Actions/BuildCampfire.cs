using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class BuildCampfire : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public BuildCampfire() : base(INTERACTION_TYPE.BUILD_CAMPFIRE) {
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
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override void PopulateNearbyLocation(List<LocationGridTile> gridTiles, ActualGoapNode goapNode) {
        Area area = goapNode.actor.gridTileLocation.area;
        if (area != null) {
            area.gridTileComponent.PopulateUnoccupiedTiles(gridTiles);
            if(gridTiles.Count <= 0) {
                gridTiles = area.gridTileComponent.gridTiles;
            }
        } else {
            goapNode.actor.gridTileLocation.PopulateTilesInRadius(gridTiles, 3, includeImpassable: false, includeTilesWithObject: false);
        }
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
        Character actor = goapNode.actor;
        LocationGridTile targetTile = actor.gridTileLocation;

        if (targetTile != null && targetTile.objHere != null) {
            targetTile = targetTile.GetFirstNeighborThatMeetCriteria(x => x.objHere == null && x.IsPassable() && x.area == targetTile.area);
        }
        if (targetTile != null && targetTile.objHere != null) {
            targetTile = targetTile.GetFirstNeighborThatMeetCriteria(x => x.objHere == null && x.IsPassable());
        }
        if (targetTile != null && targetTile.objHere != null) {
            targetTile.structure.RemovePOI(targetTile.objHere);
        }
        Campfire campfire = InnerMapManager.Instance.CreateNewTileObject<Campfire>(TILE_OBJECT_TYPE.CAMPFIRE);
        targetTile.structure.AddPOI(campfire, targetTile);
        goapNode.descriptionLog.AddInvolvedObjectManual(campfire.persistentID);

        if (targetTile != null) {
            LocationGridTile foodPileTile = targetTile.GetFirstNeighborThatMeetCriteria(x => x.objHere == null && x.IsPassable() && x.area == targetTile.area);

            if(foodPileTile == null) {
                foodPileTile = targetTile.GetFirstNeighborThatMeetCriteria(x => x.objHere == null && x.IsPassable());
            }
            if (foodPileTile == null) {
                foodPileTile = targetTile.GetFirstNeighborThatMeetCriteria(x => x.IsPassable());
            }
            if(foodPileTile != null) {
                if(foodPileTile.objHere != null) {
                    foodPileTile.structure.RemovePOI(foodPileTile.objHere);
                }
                int food = 12;
                if (actor.partyComponent.isMemberThatJoinedQuest) {
                    food = actor.partyComponent.currentParty.membersThatJoinedQuest.Count * 12;
                }
                FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.ANIMAL_MEAT);
                foodPile.SetResourceInPile(food);
                foodPileTile.structure.AddPOI(foodPile, foodPileTile);
            }

        }
        // campfire.logComponent.AddHistory(goapNode.descriptionLog);
    }
    #endregion
}