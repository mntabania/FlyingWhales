using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;
using UtilityScripts;

public class HarvestPlant : GoapAction {

    public HarvestPlant() : base(INTERACTION_TYPE.HARVEST_PLANT) {
        actionIconString = GoapActionStateDB.Harvest_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_FOOD, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Harvest Success", goapNode);
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
            return poiTarget.IsAvailable() &&
                   poiTarget.gridTileLocation != null; //&& actor.traitContainer.HasTrait("Worker");
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreHarvestSuccess(ActualGoapNode goapNode) {
        //EdiblePlant plant = goapNode.poiTarget as EdiblePlant;
        //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        goapNode.descriptionLog.AddToFillers(null, "50", LOG_IDENTIFIER.STRING_1);
        //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterHarvestSuccess(ActualGoapNode goapNode) {
        IPointOfInterest poiTarget = goapNode.poiTarget;
        //EdiblePlant plant = goapNode.poiTarget as EdiblePlant;
        //goapNode.actor.AdjustSupply(ore.GetSupplyPerMine());
        if (poiTarget is Crops crop) {
            crop.SetGrowthState(Crops.Growth_State.Growing);
            
            List<LocationGridTile> choices = poiTarget.gridTileLocation.GetTilesInRadius(1, includeTilesInDifferentStructure: true, includeImpassable: false);
            if (choices.Count > 0) {
                FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.VEGETABLES);
                foodPile.SetResourceInPile(50);
                crop.gridTileLocation.structure.AddPOI(foodPile, CollectionUtilities.GetRandomElement(choices));    
            }
        }else {
            LocationGridTile tile = poiTarget.gridTileLocation;
            tile.structure.RemovePOI(poiTarget);
            
            FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.VEGETABLES);
            foodPile.SetResourceInPile(50);
            tile.structure.AddPOI(foodPile, tile);
        }
        

        
        // foodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.FOOD_PILE);
    }
    //public void PreTargetMissing(ActualGoapNode goapNode) {
    //    goapNode.descriptionLog.AddToFillers(goapNode.actor.currentStructure.location, goapNode.actor.currentStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    public void AfterTargetMissing(ActualGoapNode goapNode) {
    }
    #endregion
}