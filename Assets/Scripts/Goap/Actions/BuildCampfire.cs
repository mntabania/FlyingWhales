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
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
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
    public override List<LocationGridTile> NearbyLocationGetter(ActualGoapNode goapNode) {
        HexTile hex = null;
        if (goapNode.actor.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
            hex = goapNode.actor.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
        }
        List<LocationGridTile> tiles = null;
        if (hex != null) {
            tiles = hex.GetUnoccupiedTiles();
            if(tiles != null && tiles.Count > 0) {
                return tiles;
            } else {
                return hex.locationGridTiles;
            }
        } else {
            tiles = goapNode.actor.gridTileLocation.GetTilesInRadius(3, includeImpassable: false);
            for (int i = 0; i < tiles.Count; i++) {
                if (tiles[i].objHere != null) {
                    tiles.RemoveAt(i);
                    i--;
                }
            }
        }
        return tiles;
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
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
            if (targetTile.collectionOwner.isPartOfParentRegionMap) {
                targetTile = targetTile.GetFirstNeighborThatMeetCriteria(x => x.objHere == null && x.IsPassable() && x.collectionOwner.isPartOfParentRegionMap && x.collectionOwner.partOfHextile.hexTileOwner == targetTile.collectionOwner.partOfHextile.hexTileOwner);
            }
        }
        if (targetTile != null && targetTile.objHere != null) {
            targetTile = targetTile.GetFirstNeighborThatMeetCriteria(x => x.objHere == null && x.IsPassable());
        }
        if (targetTile != null && targetTile.objHere != null) {
            targetTile.structure.RemovePOI(targetTile.objHere);
        }
        Campfire campfire = InnerMapManager.Instance.CreateNewTileObject<Campfire>(TILE_OBJECT_TYPE.CAMPFIRE);
        actor.gridTileLocation.structure.AddPOI(campfire, targetTile);
        goapNode.descriptionLog.AddInvolvedObjectManual(campfire.persistentID);

        if (targetTile != null) {
            LocationGridTile foodPileTile = null;
            if (targetTile.collectionOwner.isPartOfParentRegionMap) {
                foodPileTile = targetTile.GetFirstNeighborThatMeetCriteria(x => x.objHere == null && x.IsPassable() && x.collectionOwner.isPartOfParentRegionMap && x.collectionOwner.partOfHextile.hexTileOwner == targetTile.collectionOwner.partOfHextile.hexTileOwner);
            }

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
                targetTile.structure.AddPOI(foodPile, foodPileTile);
            }

        }
        // campfire.logComponent.AddHistory(goapNode.descriptionLog);
    }
    #endregion
}