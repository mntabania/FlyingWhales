using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class BuildLair : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public BuildLair() : base(INTERACTION_TYPE.BUILD_LAIR) {
        actionIconString = GoapActionStateDB.Build_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
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
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        object[] otherData = node.otherData;
        if (otherData != null) {
            if (otherData.Length == 1 && otherData[0] is LocationGridTile) {
                return (otherData[0] as LocationGridTile).structure;
            }
        }
        return base.GetTargetStructure(node);
    }
    public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        object[] otherData = goapNode.otherData;
        if (otherData != null) {
            if (otherData.Length == 1 && otherData[0] is LocationGridTile) {
                return otherData[0] as LocationGridTile;
            }
        }
        return null;
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        return null;
    }
    #endregion

    #region Effects
    public void AfterBuildSuccess(ActualGoapNode goapNode) {
        object[] otherData = goapNode.otherData;
        Character actor = goapNode.actor;

        actor.ChangeFactionTo(FactionManager.Instance.undeadFaction);
        FactionManager.Instance.undeadFaction.OnlySetLeader(actor);

        LocationGridTile targetTile = otherData[0] as LocationGridTile;
        HexTile targetHex = targetTile.collectionOwner.partOfHextile.hexTileOwner;
        LandmarkManager.Instance.CreateNewLandmarkOnTile(targetHex, LANDMARK_TYPE.MONSTER_LAIR);
        NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(targetHex.region, LOCATION_TYPE.DUNGEON, 0, targetHex);

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

        goapNode.actor.necromancerTrait.SetLairStructure(structure);
        goapNode.actor.MigrateHomeStructureTo(structure);
    }
    #endregion

    //#region Requirement
    //protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
    //    if (satisfied) {
    //        if (poiTarget.gridTileLocation != null) { //&& poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DWELLING
    //            return poiTarget.IsAvailable();
    //        }
    //    }
    //    return false;
    //}
    //#endregion
}