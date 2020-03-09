using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;

public class TakeResource : GoapAction {
    public TakeResource() : base(INTERACTION_TYPE.TAKE_RESOURCE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.TAKE_POI, GOAP_EFFECT_TARGET.ACTOR));
        // AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.TAKE_WOOD, GOAP_EFFECT_TARGET.ACTOR));
        // AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.TAKE_STONE, GOAP_EFFECT_TARGET.ACTOR));
        // AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.TAKE_METAL, GOAP_EFFECT_TARGET.ACTOR));
    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, object[] otherData) {
        List<GoapEffect> ee = base.GetExpectedEffects(actor, target, otherData);
        if(target is ResourcePile) {
            ResourcePile pile = target as ResourcePile;
            ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TAKE_POI, conditionKey = pile.name, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
            // switch (pile.providedResource) {
            //     case RESOURCE.FOOD:
            //         ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TAKE_FOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
            //         break;
            //     case RESOURCE.WOOD:
            //         ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TAKE_WOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
            //         break;
            //     case RESOURCE.STONE:
            //         ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TAKE_STONE, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
            //         break;
            //     case RESOURCE.METAL:
            //         ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TAKE_METAL, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
            //         break;
            // }
        } 
        //NOTE: UNCOMMENT THIS IF WE WANT CHARACTERS TO TAKE FOOD FROM OTHER TABLES
        //else if (target is Table) {
        //    ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
        //}
        return ee;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Take Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            ResourcePile pile = poiTarget as ResourcePile;
            if (pile.resourceInPile <= 0) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Take Fail";
            }
        }
        return goapActionInvalidity;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        ResourcePile resourcePile = node.poiTarget as ResourcePile;
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(resourcePile.providedResource.ToString()), LOG_IDENTIFIER.STRING_2);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null && poiTarget.isBeingCarriedBy != actor) {
                return false;
            }
            //if (actor.ownParty.isCarryingAnyPOI) {
            //    return false;
            //}
            return true;
            //if (poiTarget is ResourcePile) {
            //    ResourcePile pile = poiTarget as ResourcePile;
            //    if (pile.resourceInPile > 0) {
            //        return true;
            //    }
            //}
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreTakeSuccess(ActualGoapNode goapNode) {
        ResourcePile resourcePile = goapNode.poiTarget as ResourcePile;
        int takenResource;
        //bool hasNeededAmount = false;
        if (goapNode.otherData != null) {
            //hasNeededAmount = true;
            takenResource = (int)goapNode.otherData[0];
        } else {
            takenResource = Mathf.Min(20, resourcePile.resourceInPile);
        }
        //int amountAlreadyCarried = 0;
        //ResourcePile carriedResourcePile = null;
        //if (goapNode.actor.ownParty.isCarryingAnyPOI) {
        //    IPointOfInterest carriedPOI = goapNode.actor.ownParty.carriedPOI;
        //    if (carriedPOI is ResourcePile) {
        //        carriedResourcePile = carriedPOI as ResourcePile;
        //        if (carriedResourcePile.tileObjectType == resourcePile.tileObjectType) {
        //            amountAlreadyCarried = carriedResourcePile.resourceInPile;
        //        }
        //    }
        //}

        //if (hasNeededAmount && takenResource > amountAlreadyCarried) {
        //    takenResource -= amountAlreadyCarried;
        //}
        if (takenResource > resourcePile.resourceInPile) {
            takenResource = resourcePile.resourceInPile;
        }
        goapNode.descriptionLog.AddToFillers(null, takenResource.ToString(), LOG_IDENTIFIER.STRING_1);
        //goapNode.descriptionLog.AddToFillers(null, Utilities.NormalizeString(resourcePile.providedResource.ToString()), LOG_IDENTIFIER.STRING_2);
    }
    public void AfterTakeSuccess(ActualGoapNode goapNode) {
        ResourcePile resourcePile = goapNode.poiTarget as ResourcePile;
        int takenResource;
        //bool hasNeededAmount = false;
        if (goapNode.otherData != null) {
            //hasNeededAmount = true;
            takenResource = (int)goapNode.otherData[0];
        } else {
            takenResource = Mathf.Min(20, resourcePile.resourceInPile);
        }

        //int amountAlreadyCarried = 0;
        //ResourcePile carriedResourcePile = null;
        //if (goapNode.actor.ownParty.isCarryingAnyPOI) {
        //    IPointOfInterest carriedPOI = goapNode.actor.ownParty.carriedPOI;
        //    if (carriedPOI is ResourcePile) {
        //        carriedResourcePile = carriedPOI as ResourcePile;
        //        if (carriedResourcePile.tileObjectType == resourcePile.tileObjectType) {
        //            amountAlreadyCarried = carriedResourcePile.resourceInPile;
        //        }
        //    }
        //}

        //if(hasNeededAmount && takenResource > amountAlreadyCarried) {
        //    takenResource -= amountAlreadyCarried;
        //}
        if (takenResource > resourcePile.resourceInPile) {
            takenResource = resourcePile.resourceInPile;
        }
        //I think possible errors with this is going to be on the part where the npcSettlement doesn't know that this new pile exist because we directly add the new pile to the actor's party, we don't add the new pile to the structure, thus, it won't be added to the list of poi
        //Hence, if it becomes a complication to the game, what we must do is add the new pile to the structure without actually placing the object to the tile, so I think we will have to create a new function wherein we don't actually place the poi on the tile but it will still be added to the list of poi in the structure
        //EDIT: Taking resource does not mean that the character must not be carrying any poi, when the actor takes a resource and it is the same type as the one he/she is carrying, just add the amount, if it is not the same type, replace the carried one with the new type
        //If the actor is not carrying anything, create new object to be carried
        //if(carriedResourcePile != null) {
        //    if(carriedResourcePile.tileObjectType != resourcePile.tileObjectType) {
        //        goapNode.actor.UncarryPOI(bringBackToInventory: true);
        //        CarryResourcePile(goapNode.actor, resourcePile, takenResource);
        //    } else {
        //        carriedResourcePile.AdjustResourceInPile(takenResource);
        //    }
        //} else {
        //    goapNode.actor.UncarryPOI(bringBackToInventory: true);
        //    CarryResourcePile(goapNode.actor, resourcePile, takenResource);
        //}
        goapNode.actor.UncarryPOI(bringBackToInventory: true);
        CarryResourcePile(goapNode.actor, resourcePile, takenResource);
        //goapNode.actor.AdjustResource(resourcePile.providedResource, takenResource);

        //goapNode.descriptionLog.AddToFillers(null, takenResource.ToString(), LOG_IDENTIFIER.STRING_1);
        //goapNode.descriptionLog.AddToFillers(null, Utilities.NormalizeString(resourcePile.providedResource.ToString()), LOG_IDENTIFIER.STRING_2);
    }
    #endregion

    private void CarryResourcePile(Character carrier, ResourcePile pile, int amount) {
        if (pile.isBeingCarriedBy == null || pile.isBeingCarriedBy != carrier) {
            ResourcePile newPile = InnerMapManager.Instance.CreateNewTileObject<ResourcePile>(pile.tileObjectType);
            newPile.SetResourceInPile(amount);
            //newPile.SetGridTileLocation(pile.gridTileLocation);
            //newPile.gridTileLocation.structure.location.AddAwareness(newPile);
            //newPile.SetGridTileLocation(null);

            //This can be made into a function in the IPointOfInterest interface
            newPile.SetGridTileLocation(pile.gridTileLocation);
            newPile.InitializeMapObject(newPile);
            newPile.SetPOIState(POI_STATE.ACTIVE);
            newPile.gridTileLocation.structure.location.AddAwareness(newPile);
            newPile.SetGridTileLocation(null);

            // carrier.ownParty.AddPOI(newPile);
            carrier.CarryPOI(newPile);
            carrier.ShowItemVisualCarryingPOI(newPile);
            pile.AdjustResourceInPile(-amount);
        } else {
            carrier.ShowItemVisualCarryingPOI(pile);
        }
    }
}
