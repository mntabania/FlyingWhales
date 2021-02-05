﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class BuildWolfLair : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.INDIRECT;
    
    public BuildWolfLair() : base(INTERACTION_TYPE.BUILD_WOLF_LAIR) {
        actionIconString = GoapActionStateDB.Build_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.WOLF };
        logTags = new[] {LOG_TAG.Life_Changes};
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
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        OtherData[] otherData = node.otherData;
        if (otherData != null) {
            if (otherData.Length == 1 && otherData[0].obj is LocationGridTile) {
                return (otherData[0].obj as LocationGridTile).structure;
            }
        }
        return base.GetTargetStructure(node);
    }
    public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        if (otherData != null) {
            if (otherData.Length == 1 && otherData[0].obj is LocationGridTile) {
                return otherData[0].obj as LocationGridTile;
            }
        }
        return null;
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        return null;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        if (goapActionInvalidity.isInvalid == false) {
            OtherData[] otherData = node.otherData;
            if (otherData != null) {
                if (otherData.Length == 1 && otherData[0].obj is LocationGridTile targetTile) {
                    if (node.actor.gridTileLocation != targetTile && !node.actor.gridTileLocation.IsNeighbour(targetTile)) {
                        goapActionInvalidity.isInvalid = true;        
                    }
                }
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Effects
    public void AfterBuildSuccess(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        Character actor = goapNode.actor;

        LocationGridTile targetTile = otherData[0].obj as LocationGridTile;
        HexTile targetHex = targetTile.collectionOwner.partOfHextile.hexTileOwner;
        LandmarkManager.Instance.CreateNewLandmarkOnTile(targetHex, LANDMARK_TYPE.MONSTER_LAIR);
        NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(targetHex.region, LOCATION_TYPE.DUNGEON, targetHex);

        LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(targetHex.region, STRUCTURE_TYPE.MONSTER_LAIR);
        settlement.GenerateStructures(structure);

        List<LocationGridTile> locationGridTiles = targetHex.locationGridTiles; // new List<LocationGridTile>(targetHex.locationGridTiles);

        LocationStructure wilderness = targetHex.region.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        InnerMapManager.Instance.MonsterLairCellAutomata(locationGridTiles, structure, targetHex.region, wilderness);

        structure.SetOccupiedHexTile(targetHex.innerMapHexTile);
        targetHex.innerMapHexTile.Occupy();
        
        List<BlockWall> walls = structure.GetTileObjectsOfType<BlockWall>();
        for (int i = 0; i < walls.Count; i++) {
            BlockWall blockWall = walls[i];
            blockWall.baseMapObjectVisual.ApplyGraphUpdate();
        }
        targetHex.UpdatePathfindingGraphCoroutine();

        goapNode.actor.MigrateHomeStructureTo(structure);
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (otherData != null) {
                if (otherData.Length == 1 && otherData[0].obj is LocationGridTile targetTile && targetTile.hasBlueprint) {
                    return false;
                }
            }
            return poiTarget == actor;
        }
        return false;
    }
    #endregion
}