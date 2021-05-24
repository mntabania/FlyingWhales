using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using UnityEngine.Assertions;

public class Repair : GoapAction {
    
    private Precondition _stonePrecondition;
    private Precondition _woodPrecondition;
    private Precondition _metalPrecondition;
    
    public Repair() : base(INTERACTION_TYPE.REPAIR) {
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Repair_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
        
        _stonePrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
        _woodPrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        // SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasSupply);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Burnt", false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        Precondition p = null;

        if (actor.homeSettlement != null) {
            //if actor has a home settlement then check for available resource piles. Any will do
            //Reference: https://trello.com/c/efBw7BWE/4166-repair-can-use-both-stone-and-wood-and-metal-for-structures-and-objects
            if (actor.homeSettlement.mainStorage.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE)) {
                p = _woodPrecondition;
            } else if (actor.homeSettlement.mainStorage.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE)) {
                p = _stonePrecondition;
            }
        } else {
            //if character doesn't have a home settlement then default to wood.
            p = _woodPrecondition;
        }
        isOverridden = true;
        return p;
    }
    //public override List<Precondition> GetPreconditions(IPointOfInterest poiTarget, object[] otherData) {
    //    List <Precondition> p = new List<Precondition>(base.GetPreconditions(poiTarget, otherData));
    //    TileObject tileObj = poiTarget as TileObject;
    //    TileObjectData data = TileObjectDB.GetTileObjectData(tileObj.tileObjectType);
    //    int craftCost = (int)(data.constructionCost * 0.5f);
    //    p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_WOOD, craftCost.ToString(), true, GOAP_EFFECT_TARGET.ACTOR), HasSupply));
    //    return p;
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Repair Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsRepairTargetMissing(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName, "target_unavailable");
        //if (defaultTargetMissing == false) {
        //    //check the target's traits, if any of them can make this action invalid
        //    for (int i = 0; i < poiTarget.traitContainer.allTraits.Count; i++) {
        //        Trait trait = poiTarget.traitContainer.allTraits[i];
        //        if (trait.TryStopAction(goapType, actor, poiTarget, ref goapActionInvalidity)) {
        //            break; //a trait made this action invalid, stop loop
        //        }
        //    }
        //}
        return goapActionInvalidity;
    }
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, target, otherData, job);
        if (satisfied) {
            return target.gridTileLocation != null;
        }
        return false;
    }
    private bool IsRepairTargetMissing(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion) {
            return true;
        }
        if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != poiTarget.gridTileLocation && actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation, true) == false) {
                if (actor.hasMarker && actor.marker.IsCharacterInLineOfSightWith(poiTarget)) {
                    return false;
                }
                return true;
            }
        } else if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile, true) == false) {
                return true;
            }
        }
        return false;
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

    #region State Effects
    public void PreRepairSuccess(ActualGoapNode goapNode) {
        //goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        TileObject obj = goapNode.poiTarget as TileObject;
        Character actor = goapNode.actor;
        TileObjectData data = TileObjectDB.GetTileObjectData(obj.tileObjectType);
        int cost = data.repairCost;
        if (goapNode.actor.carryComponent.carriedPOI != null) {
            ResourcePile carriedPile = goapNode.actor.carryComponent.carriedPOI as ResourcePile;
            carriedPile.AdjustResourceInPile(-cost);
            obj.resourceStorageComponent.AdjustResource(carriedPile.specificProvidedResource, cost);
        }
        // TileObjectData data = TileObjectDB.GetTileObjectData(obj.tileObjectType);
        // if (data != null && data.craftRecipes != null) {
        //     TileObjectRecipe recipe = data.mainRecipe;
        //     if (!string.IsNullOrEmpty(recipe.ingredient.ingredientName)) {
        //         TileObjectRecipeIngredient ingredient = recipe.ingredient;
        //         string neededItem = ingredient.ingredientName;
        //         if (neededItem == "Wood Pile") {
        //             ResourcePile resourcePile = actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) as ResourcePile;
        //             resourcePile?.AdjustResourceInPile(-ingredient.amount);
        //             obj.AdjustResource(RESOURCE.WOOD, ingredient.amount);
        //         } else if (neededItem == "Stone Pile") {
        //             ResourcePile resourcePile = actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) as ResourcePile;
        //             resourcePile?.AdjustResourceInPile(-ingredient.amount);
        //             obj.AdjustResource(RESOURCE.STONE, ingredient.amount);
        //         } else if (neededItem == "Metal Pile") {
        //             ResourcePile resourcePile = actor.GetItem(TILE_OBJECT_TYPE.METAL_PILE) as ResourcePile;
        //             resourcePile?.AdjustResourceInPile(-ingredient.amount);
        //             obj.AdjustResource(RESOURCE.METAL, ingredient.amount);
        //         } else {
        //             actor.UnobtainItem(neededItem);
        //         }
        //     }
        // }
    }
    // public void PerTickRepairSuccess(ActualGoapNode goapNode) {
    //     goapNode.poiTarget.AdjustHP(50, ELEMENTAL_TYPE.Normal, showHPBar: true);
    // }
    public void AfterRepairSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Burnt");
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Damaged");

        TileObject obj = goapNode.poiTarget as TileObject;
        Character actor = goapNode.actor;
        obj.resourceStorageComponent.ClearAllResources();

        // TileObjectData data = TileObjectDB.GetTileObjectData(obj.tileObjectType);
        // if (data != null && data.craftRecipes != null) {
        //     TileObjectRecipe recipe = data.mainRecipe;
        //     if(!string.IsNullOrEmpty(recipe.ingredient.ingredientName)) {
        //         TileObjectRecipeIngredient ingredient = recipe.ingredient;
        //         string neededItem = ingredient.ingredientName;
        //         if (neededItem == "Wood Pile") {
        //             obj.AdjustResource(RESOURCE.WOOD, -ingredient.amount);
        //         } else if (neededItem == "Stone Pile") {
        //             obj.AdjustResource(RESOURCE.STONE, -ingredient.amount);
        //         } else if (neededItem == "Metal Pile") {
        //             obj.AdjustResource(RESOURCE.METAL, -ingredient.amount);
        //         }
        //     }
        // }

        int missingHP = obj.maxHP - obj.currentHP;
        obj.AdjustHP(missingHP, ELEMENTAL_TYPE.Normal);

        //goapNode.actor.AdjustSupply((int) (data.constructionCost * 0.5f));

    }
    #endregion

    #region Preconditions
    // private bool HasSupply(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
    //     TileObject obj = poiTarget as TileObject;
    //     TileObjectData data = TileObjectDB.GetTileObjectData(obj.tileObjectType);
    //     int craftCost = data.repairCost;
    //     if (poiTarget.HasResourceAmount(RESOURCE.WOOD, craftCost)) {
    //         return true;
    //     }
    //
    //     if (actor.carryComponent.isCarryingAnyPOI && actor.carryComponent.carriedPOI is WoodPile) {
    //         //ResourcePile carriedPile = actor.ownParty.carriedPOI as ResourcePile;
    //         //return carriedPile.resourceInPile >= craftCost;
    //         return true;
    //     }
    //     return false;
    //     //return actor.supply >= craftCost;
    // }
    private bool HasResource(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        TileObject obj = poiTarget as TileObject;
        Assert.IsNotNull(obj);
        TileObjectData data = TileObjectDB.GetTileObjectData(obj.tileObjectType);
        int craftCost = data.repairCost;
        
        if (poiTarget.resourceStorageComponent.HasResourceAmount(RESOURCE.WOOD, craftCost) || 
            poiTarget.resourceStorageComponent.HasResourceAmount(RESOURCE.STONE, craftCost) || 
            poiTarget.resourceStorageComponent.HasResourceAmount(RESOURCE.METAL, craftCost)) {
            return true; //if structure tile object already has needed resources then precondition is met
        }
        //allow precondition if actor is already carrying any resource pile that is not food pile.
        //Reference: https://trello.com/c/efBw7BWE/4166-repair-can-use-both-stone-and-wood-and-metal-for-structures-and-objects
        return actor.carryComponent.carriedPOI is ResourcePile && !(actor.carryComponent.carriedPOI is FoodPile);
    }
    #endregion

}