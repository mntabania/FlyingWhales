using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class MineMetal : GoapAction {
    //private const int MAX_SUPPLY = 50;
    //private const int MIN_SUPPLY = 20;

    public MineMetal() : base(INTERACTION_TYPE.MINE_METAL) {
        actionIconString = GoapActionStateDB.Mine_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_METAL, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Mine Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null /*&& actor.characterClass.CanDoJob(JOB_TYPE.PRODUCE_METAL)*/;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreMineSuccess(ActualGoapNode goapNode) {
        Ore ore = goapNode.poiTarget as Ore;
        goapNode.descriptionLog.AddToFillers(null, ore.yield.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterMineSuccess(ActualGoapNode goapNode) {
        Ore ore = goapNode.poiTarget as Ore;
        int metal = ore.yield;
        LocationGridTile tile = ore.gridTileLocation;
        ore.AdjustYield(-metal);

        MetalPile metalPile = InnerMapManager.Instance.CreateNewTileObject<MetalPile>(TILE_OBJECT_TYPE.METAL_PILE);
        metalPile.SetResourceInPile(metal);
        tile.structure.AddPOI(metalPile, tile);
        metalPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.METAL_PILE);
    }
    #endregion
}

public class MineMetalData : GoapActionData {
    public MineMetalData() : base(INTERACTION_TYPE.MINE_METAL) {
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}