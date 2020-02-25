using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class RepairStructure : GoapAction {

    public RepairStructure() : base(INTERACTION_TYPE.REPAIR_STRUCTURE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        //AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", false, GOAP_EFFECT_TARGET.TARGET));
    }
    //public override List<Precondition> GetPreconditions(IPointOfInterest poiTarget, object[] otherData) {
    //    List<Precondition> p = new List<Precondition>(base.GetPreconditions(poiTarget, otherData));
    //    TileObject tileObj = poiTarget as TileObject;
    //    TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
    //    int craftCost = (int)(data.constructionCost * 0.5f);
    //    p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_WOOD, craftCost.ToString(), true, GOAP_EFFECT_TARGET.ACTOR), HasSupply));

    //    return p;
    //}
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        log.AddToFillers(node.poiTarget.gridTileLocation.structure, node.poiTarget.gridTileLocation.structure.GetNameRelativeTo(node.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Repair Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 2;
    }
    #endregion

    #region State Effects
    public void PreRepairSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.structure, goapNode.poiTarget.gridTileLocation.structure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterRepairSuccess(ActualGoapNode goapNode) {
        LocationStructure structure = goapNode.poiTarget.gridTileLocation.structure;
        for (int i = 0; i < structure.tiles.Count; i++) {
            LocationGridTile tile = structure.tiles[i];
            tile.genericTileObject.AdjustHP(tile.genericTileObject.maxHP, ELEMENTAL_TYPE.Normal);
            tile.genericTileObject.traitContainer.RemoveTrait(tile.genericTileObject, "Burnt");
            for (int j = 0; j < tile.walls.Count; j++) {
                StructureWallObject structureWall = tile.walls[j];
                structureWall.traitContainer.RemoveTrait(structureWall, "Burnt");
                structureWall.AdjustHP(structureWall.maxHP, ELEMENTAL_TYPE.Normal);
            }
        }
    }
    #endregion
}
