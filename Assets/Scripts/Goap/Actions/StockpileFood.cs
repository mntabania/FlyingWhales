using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class StockpileFood : GoapAction {
    public StockpileFood() : base(INTERACTION_TYPE.STOCKPILE_FOOD) {
        actionIconString = GoapActionStateDB.Haul_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.BUY_OBJECT, "Food Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasItemInInventory);
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        return node.actor.homeStructure;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Stockpile Success", goapNode);
    }
    public override void OnActionStarted(ActualGoapNode node) {
        FoodPile foodPile = node.actor.GetItem<FoodPile>();
        if (foodPile != null) {
            node.actor.ShowItemVisualCarryingPOI(foodPile);    
        }
    }
    #endregion
    
    #region Preconditions
    private bool HasItemInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.HasItem<FoodPile>();
    }
    #endregion
    
    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor.homeStructure != null && actor == poiTarget;
        }
        return false;
    }
    #endregion
    
    #region State Effects
    public void AfterStockpileSuccess(ActualGoapNode goapNode) {
        //Drop all food at home
        List<FoodPile> allFood = RuinarchListPool<FoodPile>.Claim();
        goapNode.actor.PopulateItemsOfType(allFood);

        for (int i = 0; i < allFood.Count; i++) {
            TileObject food = allFood[i];
            LocationGridTile tile = goapNode.actor.gridTileLocation;
            if(tile != null && tile.tileObjectComponent.objHere != null) {
                tile = goapNode.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject(thisStructureOnly: true);
                if (tile == null) {
                    //in case no tile was found inside structure
                    tile = goapNode.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();    
                }
            }
            bool addToLocation = tile != null;
            goapNode.actor.UncarryPOI(food, addToLocation: addToLocation, dropLocation: tile);
        }
        
        RuinarchListPool<FoodPile>.Release(allFood);
    }
    #endregion
}
