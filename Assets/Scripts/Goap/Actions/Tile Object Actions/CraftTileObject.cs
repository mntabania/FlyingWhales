using System;
using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;  
using Traits;

public class CraftTileObject : GoapAction {

    public CraftTileObject() : base(INTERACTION_TYPE.CRAFT_TILE_OBJECT) {
        actionIconString = GoapActionStateDB.Build_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT };
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasSupply);
    //}
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        if(target is TileObject tileObject) {
            TileObjectRecipe recipe = default;
            if (otherData != null && otherData.Length == 1) {
                //preselected recipe
                recipe = (TileObjectRecipe)otherData[0].obj;
            } else {
                TileObjectData data = TileObjectDB.GetTileObjectData(tileObject.tileObjectType);
                if (data?.craftRecipes != null) {
                    data.TryGetPossibleRecipe(actor.currentRegion, out recipe);
                }
            }
            //List<Precondition> baseP = base.GetPrecondition(actor, target, otherData, out isOverridden);
            //List<Precondition> p = ObjectPoolManager.Instance.CreateNewPreconditionsList();
            Precondition p = null;
            //p.AddRange(baseP);
            if (recipe.hasValue) {
                if(!string.IsNullOrEmpty(recipe.ingredient.ingredientName)) {
                    TileObjectRecipeIngredient ingredient = recipe.ingredient;
                    string req = ingredient.ingredientName;
                    if (req == "Wood Pile") {
                        p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, req, false, GOAP_EFFECT_TARGET.ACTOR), HasWood);
                    } else if (req == "Stone Pile") {
                        p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, req, false, GOAP_EFFECT_TARGET.ACTOR), HasStone);
                    } else if (req == "Metal Pile") {
                        p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, req, false, GOAP_EFFECT_TARGET.ACTOR), HasStone);
                    } else {
                        p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, req, false, GOAP_EFFECT_TARGET.ACTOR), (thisActor, thisTarget, thisOtherData, thisJobType) => IsCarriedOrInInventory(thisActor, thisTarget, thisOtherData, req));
                    }
                }
                //for (int i = 0; i < recipe.ingredients.Length; i++) {
                //    TileObjectRecipeIngredient ingredient = recipe.ingredients[i];
                //    string req = ingredient.ingredientName;
                //    if (req == "Wood Pile") {
                //        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, req, false, GOAP_EFFECT_TARGET.ACTOR), HasWood));
                //    } else if (req == "Stone Pile") {
                //        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, req, false, GOAP_EFFECT_TARGET.ACTOR), HasStone));
                //    } else if (req == "Metal Pile") {
                //        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, req, false, GOAP_EFFECT_TARGET.ACTOR), HasStone));
                //    } else {
                //        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, req, false, GOAP_EFFECT_TARGET.ACTOR), (thisActor, thisTarget, thisOtherData, jobType) => IsCarriedOrInInventory(thisActor, thisTarget, thisOtherData, req)));    
                //    }
                //}    
            }
            isOverridden = true;
            return p;
        }
        return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Craft Success", goapNode);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        string stateName = "Target Missing";
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(false, stateName);
        //craft cannot be invalid because all cases are handled by the requirements of the action
        return goapActionInvalidity;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        TileObject obj = node.poiTarget as TileObject;
        log.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        int cost = UtilityScripts.Utilities.Rng.Next(150, 201);
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
        costLog += $" +{cost}(Initial)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
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
        (node.poiTarget as TileObject).SetMapObjectState(MAP_OBJECT_STATE.UNBUILT);    
    }
#endregion

    #region State Effects
    public void PreCraftSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        TileObject obj = goapNode.poiTarget as TileObject;
        
        TileObjectRecipe recipe = default;
        if (goapNode.otherData != null && goapNode.otherData.Length == 1) {
            //preselected recipe
            recipe = (TileObjectRecipe)goapNode.otherData[0].obj;
        } else {
            TileObjectData data = TileObjectDB.GetTileObjectData(obj.tileObjectType);
            if (data?.craftRecipes != null) {
                if (actor.carryComponent.carriedPOI is TileObject tileObject) {
                    //get recipe that uses what the actor is currently carrying.
                    //TODO: Find a way to store the recipe that the character is using, since this cane be very fragile.
                    recipe = data.GetRecipeThatUses(tileObject.tileObjectType);
                } else {
                    recipe = data.mainRecipe;    
                }
            }
        }
        
        if (recipe.hasValue) {
            if (!string.IsNullOrEmpty(recipe.ingredient.ingredientName)) {
                TileObjectRecipeIngredient ingredient = recipe.ingredient;
                string neededItem = ingredient.ingredientName;
                if (neededItem == "Wood Pile") {
                    ResourcePile resourcePile = actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) as ResourcePile;
                    resourcePile?.AdjustResourceInPile(-ingredient.amount);
                } else if (neededItem == "Stone Pile") {
                    ResourcePile resourcePile = actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) as ResourcePile;
                    resourcePile?.AdjustResourceInPile(-ingredient.amount);
                } else {
                    TileObject objectToUse = actor.GetItem(neededItem);
                    if (objectToUse is ResourcePile resourcePile) {
                        resourcePile.AdjustResourceInPile(-ingredient.amount);
                    } else {
                        if (!actor.UnobtainItem(neededItem)) {
#if DEBUG_LOG
                            actor.logComponent.PrintLogErrorIfActive("Trying to craft " + obj.name + " but " + actor + " does not have " + neededItem);
#endif
                        }    
                    }
                    
                    
                }
            }
            //for (int i = 0; i < recipe.ingredients.Length; i++) {
            //    TileObjectRecipeIngredient ingredient = recipe.ingredients[i];
            //    string neededItem = ingredient.ingredientName;
            //    if (neededItem == "Wood Pile") {
            //        ResourcePile resourcePile = actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) as ResourcePile;
            //        resourcePile?.AdjustResourceInPile(-ingredient.amount);
            //    } else if (neededItem == "Stone Pile") {
            //        ResourcePile resourcePile = actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) as ResourcePile;
            //        resourcePile?.AdjustResourceInPile(-ingredient.amount);
            //    } else if (neededItem == "Metal Pile") {
            //        ResourcePile resourcePile = actor.GetItem(TILE_OBJECT_TYPE.METAL_PILE) as ResourcePile;
            //        resourcePile?.AdjustResourceInPile(-ingredient.amount);
            //    } else {
            //        if (!actor.UnobtainItem(neededItem)) {
            //            actor.logComponent.PrintLogErrorIfActive("Trying to craft " + obj.name + " but " + actor + " does not have " + neededItem);
            //        }
            //    }
            //}
        }
        obj.SetMapObjectState(MAP_OBJECT_STATE.BUILDING);
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
        if (goapNode.thoughtBubbleLog != null) {
            goapNode.thoughtBubbleLog.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
            goapNode.thoughtBubbleLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
        }
    }
    public void AfterCraftSuccess(ActualGoapNode goapNode) {
        TileObject tileObj = goapNode.poiTarget as TileObject;
        tileObj.SetMapObjectState(MAP_OBJECT_STATE.BUILT);
        if (goapNode.associatedJobType == JOB_TYPE.CRAFT_MISSING_FURNITURE) {
            //after character finishes building the target furniture, set it as owned by him/her
            tileObj.SetCharacterOwner(goapNode.actor);
        }
        //goapNode.actor.AdjustResource(RESOURCE.WOOD, -TileObjectDB.GetTileObjectData((goapNode.poiTarget as TileObject).tileObjectType).constructionCost);
        //int cost = TileObjectDB.GetTileObjectData((goapNode.poiTarget as TileObject).tileObjectType).constructionCost;
        //goapNode.poiTarget.AdjustResource(RESOURCE.WOOD, -cost);
        //ResourcePile carriedPile = goapNode.actor.ownParty.carriedPOI as ResourcePile;
        //carriedPile.AdjustResourceInPile(-TileObjectDB.GetTileObjectData((goapNode.poiTarget as TileObject).tileObjectType).constructionCost);
    }
    #endregion

    #region Preconditions
    //private bool HasSupply(Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    int cost = TileObjectDB.GetTileObjectData((poiTarget as TileObject).tileObjectType).constructionCost;
    //    if (poiTarget.HasResourceAmount(RESOURCE.WOOD, cost)) {
    //        return true;
    //    }
    //    if (actor.ownParty.isCarryingAnyPOI && actor.ownParty.carriedPOI is WoodPile) {
    //        //ResourcePile carriedPile = actor.ownParty.carriedPOI as ResourcePile;
    //        //return carriedPile.resourceInPile >= cost;
    //        return true;
    //    }
    //    return false;
    //    //return actor.supply >= TileObjectDB.GetTileObjectData((poiTarget as TileObject).tileObjectType).constructionCost;
    //}
    private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, string itemName) {
        return actor.IsPOICarriedOrInInventory(itemName);
    }
    private bool HasWood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget is TileObject tileObject && actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) is ResourcePile pile && 
               pile.resourceInPile >= TileObjectDB.GetTileObjectData(tileObject.tileObjectType).mainRecipe.GetNeededAmountForIngredient(TILE_OBJECT_TYPE.WOOD_PILE); 
    }
    private bool HasStone(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget is TileObject tileObject && actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) is ResourcePile pile && 
               pile.resourceInPile >= TileObjectDB.GetTileObjectData(tileObject.tileObjectType).mainRecipe.GetNeededAmountForIngredient(TILE_OBJECT_TYPE.STONE_PILE); 
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget is TileObject) {
                TileObject target = poiTarget as TileObject;
                return target.mapObjectState == MAP_OBJECT_STATE.UNBUILT;    
            }
        }
        return false;
    }
    #endregion

}