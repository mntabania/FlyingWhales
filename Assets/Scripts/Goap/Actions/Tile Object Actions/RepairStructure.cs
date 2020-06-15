using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;

public class RepairStructure : GoapAction {

    public RepairStructure() : base(INTERACTION_TYPE.REPAIR_STRUCTURE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        //AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", false, GOAP_EFFECT_TARGET.TARGET));
        // AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
    }
    public override List<Precondition> GetPreconditions(Character actor, IPointOfInterest target, object[] otherData) {
        Assert.IsTrue(target is StructureTileObject, $"Repair structure is being advertised by something that is not a StructureTileObject! {target}");
        StructureTileObject structureTileObject = target as StructureTileObject;
        List<Precondition> p = new List<Precondition>(base.GetPreconditions(actor, target, otherData));
        switch (structureTileObject.structureParent.structureObj.WallsMadeOf()) {
            case RESOURCE.WOOD:
                p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile" , false, GOAP_EFFECT_TARGET.ACTOR), HasResource));
                break;
            case RESOURCE.STONE:
                p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile" , false, GOAP_EFFECT_TARGET.ACTOR), HasResource));
                break;
            case RESOURCE.METAL:
                p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Metal Pile" , false, GOAP_EFFECT_TARGET.ACTOR), HasResource));
                break;
        }
        
        return p;
    }
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
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        actor.UncarryPOI();
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.UncarryPOI();
    }
    #endregion
    
    #region Preconditions
    private bool HasResource(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        StructureTileObject tileObj = poiTarget as StructureTileObject;
        TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
        int craftCost = (int)(data.constructionCost * 0.5f);

        RESOURCE neededResourceType = tileObj.structureParent.structureObj.WallsMadeOf();
        if (poiTarget.HasResourceAmount(neededResourceType, craftCost)) {
            return true;
        }
        
        if (actor.ownParty.isCarryingAnyPOI) {
            switch (neededResourceType) {
                case RESOURCE.WOOD:
                    return actor.ownParty.carriedPOI is WoodPile;
                case RESOURCE.METAL:
                    return actor.ownParty.carriedPOI is MetalPile;
                case RESOURCE.STONE:
                    return actor.ownParty.carriedPOI is StonePile;
            }
            return false;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreRepairSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.structure, goapNode.poiTarget.gridTileLocation.structure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        if (goapNode.actor.ownParty.carriedPOI != null) {
            ResourcePile carriedPile = goapNode.actor.ownParty.carriedPOI as ResourcePile;
            //place needed resources at structure tile object, this is so that if a character has already started 
            //repairing a particular structure, he/she will not need to get more resources if ever he/she is stopped.
            goapNode.poiTarget.AdjustResource(carriedPile.providedResource, carriedPile.resourceInPile);
            carriedPile.AdjustResourceInPile(-carriedPile.resourceInPile);
        }
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
        //clear out resources stored at structure tile object
        RESOURCE neededResourceType =
            (goapNode.poiTarget as StructureTileObject).structureParent.structureObj.WallsMadeOf();
        goapNode.poiTarget.SetResource(neededResourceType, 0);
    }
    #endregion
}
