﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine;
using UnityEngine.Assertions;

public class RepairStructure : GoapAction {

    private Precondition _stonePrecondition;
    private Precondition _woodPrecondition;
    private Precondition _metalPrecondition;

    public RepairStructure() : base(INTERACTION_TYPE.REPAIR_STRUCTURE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};

        _stonePrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
        _woodPrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
        _metalPrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Metal Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource);

    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        //AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", false, GOAP_EFFECT_TARGET.TARGET));
        // AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
    }
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        Assert.IsTrue(target is StructureTileObject, $"Repair structure is being advertised by something that is not a StructureTileObject! {target}");
        //List<Precondition> baseP = base.GetPrecondition(actor, target, otherData, out isOverridden);
        //List<Precondition> p = ObjectPoolManager.Instance.CreateNewPreconditionsList();
        //p.AddRange(baseP);

        Precondition p = null;

        StructureTileObject structureTileObject = target as StructureTileObject;
        if (structureTileObject.structureParent is ManMadeStructure manMadeStructure) {
            switch (manMadeStructure.wallsAreMadeOf) {
                case RESOURCE.WOOD:
                    p = _woodPrecondition;
                    break;
                case RESOURCE.STONE:
                    p = _stonePrecondition;
                    break;
                case RESOURCE.METAL:
                    p = _metalPrecondition;
                    break;
                default:
                    p = _woodPrecondition;
                    break;
            }
        }
        isOverridden = true;
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
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
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
    private bool HasResource(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        StructureTileObject tileObj = poiTarget as StructureTileObject;
        Assert.IsNotNull(tileObj, $"Target of repair is not Structure Tile Object! {poiTarget}");

        ManMadeStructure manMadeStructure = tileObj.structureParent as ManMadeStructure;
        Assert.IsNotNull(manMadeStructure, $"Parent structure is not Man Made structure! {tileObj.structureParent}");
        RESOURCE neededResourceType = manMadeStructure.wallsAreMadeOf;
        
        if (poiTarget.HasResourceAmount(neededResourceType, manMadeStructure.structureObj.repairCost)) {
            return true;
        }
        
        if (actor.carryComponent.isCarryingAnyPOI) {
            switch (neededResourceType) {
                case RESOURCE.WOOD:
                    return actor.carryComponent.carriedPOI is WoodPile;
                case RESOURCE.METAL:
                    return actor.carryComponent.carriedPOI is MetalPile;
                case RESOURCE.STONE:
                    return actor.carryComponent.carriedPOI is StonePile;
            }
            return false;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreRepairSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.structure, goapNode.poiTarget.gridTileLocation.structure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        if (goapNode.actor.carryComponent.carriedPOI != null) {
            ResourcePile carriedPile = goapNode.actor.carryComponent.carriedPOI as ResourcePile;
            //place needed resources at structure tile object, this is so that if a character has already started 
            //repairing a particular structure, he/she will not need to get more resources if ever he/she is stopped.
            goapNode.poiTarget.AdjustResource(carriedPile.providedResource, carriedPile.resourceInPile);
            carriedPile.AdjustResourceInPile(-carriedPile.resourceInPile);
        }
    }
    public void AfterRepairSuccess(ActualGoapNode goapNode) {
        LocationStructure structure = goapNode.poiTarget.gridTileLocation.structure;
        for (int i = 0; i < structure.tiles.Count; i++) {
            LocationGridTile tile = structure.tiles.ElementAt(i);
            tile.genericTileObject.AdjustHP(tile.genericTileObject.maxHP, ELEMENTAL_TYPE.Normal);
            tile.genericTileObject.traitContainer.RemoveTrait(tile.genericTileObject, "Burnt");
            for (int j = 0; j < tile.walls.Count; j++) {
                ThinWall structureWall = tile.walls[j];
                structureWall.traitContainer.RemoveTrait(structureWall, "Burnt");
                structureWall.AdjustHP(structureWall.maxHP, ELEMENTAL_TYPE.Normal);
            }
        }
        if (goapNode.poiTarget is StructureTileObject structureTileObject && structureTileObject.structureParent is ManMadeStructure manMadeStructure) {
            //clear out resources stored at structure tile object
            RESOURCE neededResourceType = manMadeStructure.wallsAreMadeOf;
            goapNode.poiTarget.SetResource(neededResourceType, 0);
        }
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job) {
        bool requirementsSatisfied = base.AreRequirementsSatisfied(actor, target, otherData, job);
        if (requirementsSatisfied) {
            if (target is StructureTileObject structureTileObject) {
                if (structureTileObject.gridTileLocation == null || structureTileObject.structureParent.hasBeenDestroyed
                    || structureTileObject.structureParent is ManMadeStructure == false) {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
    #endregion
}
