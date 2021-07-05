
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;

public class CraftEquipment : GoapAction {

    public CraftEquipment() : base(INTERACTION_TYPE.CRAFT_EQUIPMENT) {
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetDead);
    //    AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.ABSORB_LIFE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    //}
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        TileObject obj = node.poiTarget as TileObject;
        log.AddToFillers(null, UtilityScripts.Utilities.GetArticleForWord(obj.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(obj.tileObjectType.ToString()), LOG_IDENTIFIER.ITEM_1);
    }
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        if(target is TileObject tileObject) {
            List<CONCRETE_RESOURCES> concreteResourcesNeeded = EquipmentDataHandler.Instance.GetResourcesNeeded(tileObject.tileObjectType);
            RESOURCE generalResource = EquipmentDataHandler.Instance.GetGeneralResourcesNeeded(tileObject.tileObjectType);
            int resourcesNeededAmount = EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType);
            Workshop workshop = actor.structureComponent.workPlaceStructure as Workshop;
            Assert.IsNotNull(workshop);
            Precondition p = null;
            if (concreteResourcesNeeded != null && concreteResourcesNeeded.Count > 0) {
                if (workshop.CanBeCrafted(concreteResourcesNeeded, resourcesNeededAmount, out var foundResourcePile)) {
                    p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, foundResourcePile.name, false, GOAP_EFFECT_TARGET.ACTOR), HasResource);
                    isOverridden = true;
                    return p;
                }
            } else if (generalResource != RESOURCE.NONE) {
                switch (generalResource) {
                    case RESOURCE.WOOD:
                        p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasWood);
                        break;
                    case RESOURCE.STONE:
                        p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasStone);
                        break;
                    case RESOURCE.METAL:
                        p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Metal Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasMetal);
                        break;
                    case RESOURCE.CLOTH:
                        p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Cloth Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasCloth);
                        break;
                    case RESOURCE.LEATHER:
                        p = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Leather Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasLeather);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                isOverridden = true;
                return p;
            }
        }
        return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Craft Equipment Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor.structureComponent.HasWorkPlaceStructure() && actor.structureComponent.workPlaceStructure is Workshop;
        }
        return false;
    }

    // bool CheckIfCanBeCrafted(ActualGoapNode p_node) {
    //     EquipmentItem equipment = p_node.target as EquipmentItem;
    //     Workshop workShop = p_node.actor.structureComponent.workPlaceStructure as Workshop;
    //     if (workShop.CanBeCrafted(equipment.equipmentData.specificResource, equipment.equipmentData.resourceAmount)) {
    //         return true;
    //     }
    //     if (workShop.CanBeCrafted(equipment.equipmentData.resourceType, equipment.equipmentData.resourceAmount)) {
    //         return true;
    //     }
    //     return false;
    // }
    #endregion

    #region Preconditions
    private bool HasWood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget is TileObject tileObject && actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) is ResourcePile pile && 
               pile.resourceInPile >= EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType); 
    }
    private bool HasStone(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget is TileObject tileObject && actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) is ResourcePile pile && 
               pile.resourceInPile >= EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType); 
    }
    private bool HasMetal(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget is TileObject tileObject && actor.GetItem<MetalPile>() is ResourcePile pile && 
               pile.resourceInPile >= EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType); 
    }
    private bool HasCloth(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget is TileObject tileObject && actor.GetItem<ClothPile>() is ResourcePile pile && 
               pile.resourceInPile >= EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType); 
    }
    private bool HasLeather(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget is TileObject tileObject && actor.GetItem<LeatherPile>() is ResourcePile pile && 
               pile.resourceInPile >= EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType); 
    }
    private bool HasResource(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        if (poiTarget is TileObject tileObject) {
            List<CONCRETE_RESOURCES> concreteResourcesNeeded = EquipmentDataHandler.Instance.GetResourcesNeeded(tileObject.tileObjectType);
            int resourcesNeededAmount = EquipmentDataHandler.Instance.GetResourcesNeededAmount(tileObject.tileObjectType);
            for (int i = 0; i < concreteResourcesNeeded.Count; i++) {
                CONCRETE_RESOURCES concreteResources = concreteResourcesNeeded[i];
                ResourcePile resourcePile = actor.GetItem(concreteResources.ConvertResourcesToTileObjectType()) as ResourcePile;
                if (resourcePile != null && resourcePile.resourceInPile >= resourcesNeededAmount) {
                    return true;    
                }
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreCraftEquipmentSuccess(ActualGoapNode p_node) {
        if (p_node.actor.carryComponent.carriedPOI is ResourcePile resourcePile) {
            resourcePile.AdjustResourceInPile(-resourcePile.resourceInPile);
        }
    }
    public void AfterCraftEquipmentSuccess(ActualGoapNode p_node) {
        TileObject target = p_node.target as TileObject;
        Assert.IsNotNull(target);
        target.SetMapObjectState(MAP_OBJECT_STATE.BUILT);
        EquipmentItem targetItem = target as EquipmentItem;
        Assert.IsNotNull(targetItem);
        p_node.actor.moneyComponent.AdjustCoins(28);
        if (p_node.actor.talentComponent.GetTalent(CHARACTER_TALENT.Crafting).level >= 5) {
            if (GameUtilities.RollChance(20)) {
                // Debug.LogError(targetItem.name + " Crafted as Premium Quality Item");
                targetItem.MakeQualityPremium();
            } else if (GameUtilities.RollChance(20)) {
                // Debug.LogError(targetItem.name + " Crafted as High Quality Item");
                targetItem.MakeQualityHigh();
            }

        } else if (p_node.actor.talentComponent.GetTalent(CHARACTER_TALENT.Crafting).level >= 3) {
            if (GameUtilities.RollChance(20)) {
                // Debug.LogError(targetItem.name + " Crafted as High Quality Item");
                targetItem.MakeQualityHigh();
            }
        } else {
            // Debug.LogError(targetItem.name + " Crafted as normal Item");
        }
        if (p_node.actor.structureComponent.workPlaceStructure is Workshop workshop) {
            workshop.RemoveFirstRequestThatIsFulfilledBy(target);
        }
        p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Crafting).AdjustExperience(25, p_node.actor);
       
    }
    #endregion

}