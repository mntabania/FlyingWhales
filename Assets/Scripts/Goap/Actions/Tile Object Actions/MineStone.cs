﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class MineStone : GoapAction {
    //private const int MAX_SUPPLY = 50;
    //private const int MIN_SUPPLY = 20;

    public MineStone() : base(INTERACTION_TYPE.MINE_STONE) {
        actionIconString = GoapActionStateDB.Mine_Icon;
        
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_STONE, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Mine Success", goapNode);
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
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null /*&& actor.characterClass.CanDoJob(JOB_TYPE.PRODUCE_STONE)*/;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreMineSuccess(ActualGoapNode goapNode) {
        Rock rock = goapNode.poiTarget as Rock;
        goapNode.descriptionLog.AddToFillers(null, rock.yield.ToString(), LOG_IDENTIFIER.STRING_1);
        //if (goapNode.actor.characterClass.IsCombatant()) {
        //    goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        //}
    }
    public void PerTickMineSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.characterClass.IsCombatant()) {
            goapNode.actor.needsComponent.AdjustHappiness(-2);
        }
    }
    public void AfterMineSuccess(ActualGoapNode goapNode) {
        //if (goapNode.actor.characterClass.IsCombatant()) {
        //    goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        //}
        Rock rock = goapNode.poiTarget as Rock;
        int stone = rock.yield;
        LocationGridTile tile = rock.gridTileLocation;
        rock.AdjustYield(-stone);

        // StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
        // stonePile.SetResourceInPile(stone);
        // tile.structure.AddPOI(stonePile, tile);
        
        InnerMapManager.Instance.CreateNewResourcePileAndTryCreateHaulJob<StonePile>(TILE_OBJECT_TYPE.STONE_PILE, stone, goapNode.actor, tile);
    }
    #endregion
}