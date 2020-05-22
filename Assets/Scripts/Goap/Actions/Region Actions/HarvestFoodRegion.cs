using System;
using Inner_Maps;
using Traits;

public class HarvestFoodRegion : GoapAction {
    
    public HarvestFoodRegion() : base(INTERACTION_TYPE.HARVEST_FOOD_REGION) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
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
        return 25;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //**Requirements:** Actor is a Worker. Region has Farm Landmark. Region is owned by Actor's Faction or Actor's Home's Ruling Faction.
            var region = poiTarget.gridTileLocation.parentMap.region.coreTile.region;
            // return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && actor.traitContainer.GetNormalTrait<Trait>("Worker") != null 
            //        && region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.FARM && (region.owner == actor.faction || region.owner == actor.homeRegion.owner);
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreHarvestSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.parentMap.region.coreTile.region, goapNode.poiTarget.gridTileLocation.parentMap.region.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterHarvestSuccess(ActualGoapNode goapNode) {
        //**After Effect 1**: Produce Food random between 200 - 500
        var randomFood = UnityEngine.Random.Range(200, 501);
        var foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.ANIMAL_MEAT);
        foodPile.SetResourceInPile(randomFood);
        goapNode.actor.gridTileLocation.structure.AddPOI(foodPile);
        goapNode.descriptionLog.AddToFillers(null, randomFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    #endregion
}