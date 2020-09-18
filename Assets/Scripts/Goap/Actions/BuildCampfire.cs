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
        showNotification = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
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
        LocationGridTile targetTile = goapNode.actor.gridTileLocation;
        if (targetTile.objHere != null) {
            targetTile.structure.RemovePOI(targetTile.objHere);
        }
        Campfire campfire = InnerMapManager.Instance.CreateNewTileObject<Campfire>(TILE_OBJECT_TYPE.CAMPFIRE);
        goapNode.actor.gridTileLocation.structure.AddPOI(campfire, targetTile);
        goapNode.descriptionLog.AddInvolvedObjectManual(campfire.persistentID);
        // campfire.logComponent.AddHistory(goapNode.descriptionLog);
    }
    #endregion
}