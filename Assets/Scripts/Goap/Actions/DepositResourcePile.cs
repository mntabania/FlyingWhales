using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;

public class DepositResourcePile : GoapAction {
    public DepositResourcePile() : base(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE) {
        actionIconString = GoapActionStateDB.Haul_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        //AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), IsCarriedOrInInventory);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
        //AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_FOOD, GOAP_EFFECT_TARGET.TARGET));
        //AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_WOOD, GOAP_EFFECT_TARGET.TARGET));
        //AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_STONE, GOAP_EFFECT_TARGET.TARGET));
        //AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_METAL, GOAP_EFFECT_TARGET.TARGET));
    }
    //protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, object[] otherData) {
    //    List<GoapEffect> ee = base.GetExpectedEffects(actor, target, otherData);
    //    ResourcePile pile = target as ResourcePile;
    //    switch (pile.providedResource) {
    //        case RESOURCE.FOOD:
    //            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_FOOD, "0", true, GOAP_EFFECT_TARGET.TARGET));
    //            break;
    //        case RESOURCE.WOOD:
    //            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_WOOD, "0", true, GOAP_EFFECT_TARGET.TARGET));
    //            break;
    //        case RESOURCE.STONE:
    //            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_STONE, "0", true, GOAP_EFFECT_TARGET.TARGET));
    //            break;
    //        case RESOURCE.METAL:
    //            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_METAL, "0", true, GOAP_EFFECT_TARGET.TARGET));
    //            break;
    //    }
    //    return ee;
    //}
    public override List<Precondition> GetPreconditions(Character actor, IPointOfInterest target, object[] otherData) {
        List<Precondition> p = new List<Precondition>();
        p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, target.name, false, GOAP_EFFECT_TARGET.TARGET), IsCarriedOrInInventory));
        return p;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Deposit Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 3;
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        object[] otherData = node.otherData;
        if (otherData != null && otherData.Length == 1 && otherData[0] is IPointOfInterest) {
            IPointOfInterest poiToBeDeposited = otherData[0] as IPointOfInterest;
            if(poiToBeDeposited.gridTileLocation != null) {
                return poiToBeDeposited.gridTileLocation.structure;
            } else {
                //if the poi where the actor is supposed to deposit his carried pile has no grid tile location, this must mean that the pile is either destroyed or carried by another character
                //return the main storage so that the main storage will become the target structure
                return node.actor.homeSettlement.mainStorage;
            }
        } else {
            return node.actor.homeSettlement.mainStorage;
        }
        //return base.GetTargetStructure(node);
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        object[] otherData = goapNode.otherData;
        if (otherData != null && otherData.Length == 1 && otherData[0] is IPointOfInterest) {
            IPointOfInterest poiToBeDeposited = otherData[0] as IPointOfInterest;
            if(poiToBeDeposited.gridTileLocation == null) {
                //if the poi where the actor is supposed to deposit his carried pile has no grid tile location, this must mean that the pile is either destroyed or carried by another character
                //return null so that the actor will get a random tile from the target structure instead
                return null;
            } else {
                return poiToBeDeposited;
            }
        }
        return null;
    }
    public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
        //if the process goes through here, this must mean that the target poi where the actor is supposed to go has no grid tile location or is destroyed or is carried by another character
        //so, just return a random unoccupied tile from the target structure
        List<LocationGridTile> unoccupiedTiles = goapNode.targetStructure.unoccupiedTiles.ToList();
        return unoccupiedTiles[UnityEngine.Random.Range(0, unoccupiedTiles.Count)];
    }
    public override void OnStopWhileStarted(ActualGoapNode node) {
        base.OnStopWhileStarted(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        actor.UncarryPOI(poiTarget, dropLocation: actor.gridTileLocation);
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        Character targetCharacter = poiTarget as Character;
        actor.UncarryPOI(poiTarget);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = IsTargetMissingOverride(node);
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName);
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
    public override void AddFillersToLog(Log log, ActualGoapNode goapNode) {
        base.AddFillersToLog(log, goapNode);
        ResourcePile pile = goapNode.poiTarget as ResourcePile;
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(pile.providedResource.ToString()), LOG_IDENTIFIER.STRING_1);
    }
    public override void OnActionStarted(ActualGoapNode node) {
        node.actor.ShowItemVisualCarryingPOI(node.poiTarget as TileObject);
    }
    #endregion

    #region Preconditions
    //private bool IsActorWoodEnough(Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    if (actor.supply > actor.role.reservedSupply) {
    //        WoodPile supplyPile = poiTarget as WoodPile;
    //        int supplyToBeDeposited = actor.supply - actor.role.reservedSupply;
    //        if((supplyToBeDeposited + supplyPile.resourceInPile) >= 100) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //private bool IsActorFoodEnough(Character actor, IPointOfInterest poiTarget, object[] otherData) {
    //    return actor.food > 0;
    //}
    private bool IsCarriedOrInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.IsPOICarriedOrInInventory(poiTarget);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (actor.IsPOICarriedOrInInventory(poiTarget)) {
                return true;
            }
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget.gridTileLocation.structure.settlementLocation != null) {
                if (poiTarget.gridTileLocation.structure == actor.homeSettlement.mainStorage) {
                    return false;
                }
                if (actor.homeSettlement.mainStorage.unoccupiedTiles.Count <= 0) {
                    return false;
                }
            } else {
                //Cannot be deposited if already in the storage
                LocationStructure structure = poiTarget.gridTileLocation.structure;
                if (structure == actor.homeSettlement.mainStorage) {
                    return false;
                }
                if (actor.homeSettlement.mainStorage != null && actor.homeSettlement.mainStorage.unoccupiedTiles.Count <= 0) {
                    return false;
                }
            }
            return actor.homeRegion == poiTarget.gridTileLocation.parentMap.region;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreDepositSuccess(ActualGoapNode goapNode) {
        ResourcePile pile = goapNode.poiTarget as ResourcePile;
        //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        //int givenSupply = goapNode.actor.supply - goapNode.actor.role.reservedSupply;
        //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        goapNode.descriptionLog.AddToFillers(null, pile.resourceInPile.ToString(), LOG_IDENTIFIER.STRING_1);
        goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(pile.providedResource.ToString()), LOG_IDENTIFIER.STRING_2);
    }
    public void AfterDepositSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        ResourcePile poiTarget = goapNode.poiTarget as ResourcePile;
        object[] otherData = goapNode.otherData;
        ResourcePile pileToBeDepositTo = null;
        if (otherData != null && otherData.Length == 1 && otherData[0] is ResourcePile) {
            pileToBeDepositTo = otherData[0] as ResourcePile;
        }
        if(pileToBeDepositTo != null && pileToBeDepositTo.gridTileLocation == goapNode.targetTile) {
            //Deposit resource pile
            if(pileToBeDepositTo.IsAtMaxResource(poiTarget.providedResource) == false) {
                if (pileToBeDepositTo.mapObjectState == MAP_OBJECT_STATE.UNBUILT) {
                    pileToBeDepositTo.SetMapObjectState(MAP_OBJECT_STATE.BUILT);
                }
                pileToBeDepositTo.AdjustResourceInPile(poiTarget.resourceInPile);
                TraitManager.Instance.CopyStatuses(poiTarget, pileToBeDepositTo);
                actor.UncarryPOI(poiTarget, addToLocation: false);
            } else {
                actor.UncarryPOI(poiTarget);
            }
        } else {
            actor.UncarryPOI(poiTarget);
        }
    }
    #endregion


    private bool IsTargetMissingOverride(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (actor.ownParty.IsPOICarried(poiTarget)) {
            return false;
        }
        if (poiTarget.IsAvailable() == false || poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion) {
            return true;
        }
        if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_TARGET) {
            //if the action type is NEAR_TARGET, then check if the actor is near the target, if not, this action is invalid.
            if (actor.gridTileLocation != poiTarget.gridTileLocation && actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation) == false) {
                return true;
            }
        } else if (actionLocationType == ACTION_LOCATION_TYPE.NEAR_OTHER_TARGET) {
            if (actor.gridTileLocation != node.targetTile && actor.gridTileLocation.IsNeighbour(node.targetTile) == false) {
                return true;
            }
        }
        return false;
    }
}
