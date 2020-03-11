using Inner_Maps;
using Traits;

public class ForageFoodRegion : GoapAction {
    
    public ForageFoodRegion() : base(INTERACTION_TYPE.FORAGE_FOOD_REGION) {
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
        SetState("Forage Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 25;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //**Requirements:** Actor is a Hunter. Region has a Game feature.
            var region = poiTarget.gridTileLocation.parentMap.location.coreTile.region;
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null &&
                   actor.traitContainer.HasTrait("Hunter") && 
                   region.HasTileWithFeature(TileFeatureDB.Game_Feature);
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreForageSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.parentMap.location.coreTile.region, goapNode.poiTarget.gridTileLocation.parentMap.location.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterForageSuccess(ActualGoapNode goapNode) {
        //**After Effect 1**: Produce Food random between 100 - 700
        var randomFood = UnityEngine.Random.Range(100, 701);
        var foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.FOOD_PILE);
        foodPile.SetResourceInPile(randomFood);
        goapNode.actor.gridTileLocation.structure.AddPOI(foodPile);
        goapNode.descriptionLog.AddToFillers(null, randomFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    #endregion
}
