using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;
public class BuildLair : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public BuildLair() : base(INTERACTION_TYPE.BUILD_LAIR) {
        actionIconString = GoapActionStateDB.Build_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Life_Changes};
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
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, target, otherData, job);
        if (satisfied) {
            if (otherData != null) {
                if (otherData.Length == 1 && otherData[0].obj is LocationGridTile targetTile) {
                    return !targetTile.hasBlueprint;
                }
            }
            return true;
        }
        return false;
    }
#endregion

#region Effects
    public void AfterBuildSuccess(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        Character actor = goapNode.actor;

        //actor.ChangeFactionTo(FactionManager.Instance.undeadFaction);
        //FactionManager.Instance.undeadFaction.OnlySetLeader(actor);

        //if necromancer is a cultist then make the undead faction friendly towards the player.
        if (actor.traitContainer.HasTrait("Cultist")) {
            FactionManager.Instance.undeadFaction.SetRelationshipFor(PlayerManager.Instance.player.playerFaction, FACTION_RELATIONSHIP_STATUS.Friendly);
        }
        
        LocationGridTile targetTile = otherData[0].obj as LocationGridTile;
        Area targetArea = targetTile.area;
        //LandmarkManager.Instance.CreateNewLandmarkOnTile(targetArea, LANDMARK_TYPE.MONSTER_LAIR);
        NPCSettlement settlement = LandmarkManager.Instance.CreateNewSettlement(targetArea.region, LOCATION_TYPE.DUNGEON, targetArea);

        LocationStructure structure = LandmarkManager.Instance.CreateNewStructureAt(targetArea.region, STRUCTURE_TYPE.MONSTER_LAIR);
        settlement.GenerateStructures(structure);

        List<LocationGridTile> locationGridTiles = targetArea.gridTileComponent.gridTiles; //.ToList()// new List<LocationGridTile>(targetHex.locationGridTiles);

        LocationStructure wilderness = targetArea.region.wilderness;
        InnerMapManager.Instance.MonsterLairCellAutomata(locationGridTiles, structure, targetArea.region, wilderness);

        structure.SetOccupiedArea(targetArea);

        List<TileObject> walls = RuinarchListPool<TileObject>.Claim();
        structure.PopulateTileObjectsOfType(walls, TILE_OBJECT_TYPE.BLOCK_WALL);
        for (int i = 0; i < walls.Count; i++) {
            TileObject blockWall = walls[i];
            blockWall.baseMapObjectVisual.ApplyGraphUpdate();
        }
        RuinarchListPool<TileObject>.Release(walls);
        targetArea.areaItem.UpdatePathfindingGraph();
        //targetHex.UpdatePathfindingGraphCoroutine();

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