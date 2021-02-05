﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;

public class SpawnSkeleton : GoapAction {

    public SpawnSkeleton() : base(INTERACTION_TYPE.SPAWN_SKELETON) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.DEMON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.ABSORB_LIFE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Spawn Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

   // #region Requirements
   //protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
   //     bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
   //     if (satisfied) {
   //         return actor != poiTarget && poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER && poiTarget.mapObjectVisual;
   //     }
   //     return false;
   // }
   // #endregion

    #region State Effects
    public void AfterSpawnSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.necromancerTrait.AdjustLifeAbsorbed(-1);
        goapNode.actor.necromancerTrait.AdjustEnergy(-1);
        LocationGridTile gridTile = goapNode.actor.gridTileLocation.GetRandomUnoccupiedNeighbor();
        if(gridTile == null) {
            gridTile = goapNode.actor.gridTileLocation;
        }
        Summon skeleton = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, goapNode.actor.faction, homeRegion: gridTile.parentMap.region, bypassIdeologyChecking: true);
        CharacterManager.Instance.PlaceSummonInitially(skeleton, gridTile);
    }
    #endregion

}