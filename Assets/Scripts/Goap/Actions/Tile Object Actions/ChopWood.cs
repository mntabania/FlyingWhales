﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class ChopWood : GoapAction {
    //private const int MAX_SUPPLY = 50;

    public ChopWood() : base(INTERACTION_TYPE.CHOP_WOOD) {
        actionIconString = GoapActionStateDB.Chop_Icon;
        
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_WOOD, conditionKey = MAX_SUPPLY.ToString(), isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_WOOD, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Chop Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    //public override void OnStopWhilePerforming(ActualGoapNode node) {
    //    base.OnStopWhilePerforming(node);
    //    if (node.actor.characterClass.IsCombatant()) {
    //        node.actor.needsComponent.AdjustDoNotGetBored(-1);
    //    }
    //}
    public override bool IsHappinessRecoveryAction() {
        return true;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null /*&& actor.characterClass.CanDoJob(JOB_TYPE.PRODUCE_WOOD)*/;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreChopSuccess(ActualGoapNode goapNode) {
        TreeObject tree = goapNode.poiTarget as TreeObject;
        //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        goapNode.descriptionLog.AddToFillers(null, tree.yield.ToString(), LOG_IDENTIFIER.STRING_1);
        //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        //if (goapNode.actor.characterClass.IsCombatant()) {
        //    goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        //}
    }
    public void PerTickChopSuccess(ActualGoapNode goapNode) {
        TreeObject tree = goapNode.poiTarget as TreeObject;
        tree.AdjustHP(-1, ELEMENTAL_TYPE.Normal);
        if (goapNode.actor.characterClass.IsCombatant()) {
            goapNode.actor.needsComponent.AdjustHappiness(-1);
        }
    }
    
    public void AfterChopSuccess(ActualGoapNode goapNode) {
        //if (goapNode.actor.characterClass.IsCombatant()) {
        //    goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        //}
        TreeObject tree = goapNode.poiTarget as TreeObject;
        LocationGridTile tile = tree.gridTileLocation;
        int wood = tree.yield;
        tree.AdjustYield(-wood);

        // WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
        // woodPile.SetResourceInPile(wood);
        // tile.structure.AddPOI(woodPile, tile);

        InnerMapManager.Instance.CreateNewResourcePileAndTryCreateHaulJob<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE, wood,
            goapNode.actor, tile);
    }
    #endregion
}