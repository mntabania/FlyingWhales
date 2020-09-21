using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps.Location_Structures;

public class Eat : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.CONSUME; } }

    public Eat() : base(INTERACTION_TYPE.EAT) {
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Eat_Icon;
        showNotification = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.KOBOLD };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.STAMINA_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override List<Precondition> GetPreconditions(Character actor, IPointOfInterest target, OtherData[] otherData) {
        if (target is Table) { // || target is FoodPile
            List<Precondition> p = new List<Precondition>(base.GetPreconditions(actor, target, otherData));
            p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile" /*+ (int)otherData[0]*/, false, GOAP_EFFECT_TARGET.TARGET), HasFood));
            return p;
        }
        return base.GetPreconditions(actor, target, otherData);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Eat Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 0;
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            if (!actor.partyComponent.hasParty) {
                //target is at structure that character is avoiding
                cost = 2000;
                costLog += $" +{cost}(Location of target is in avoid structure)";
                actor.logComponent.AppendCostLog(costLog);
                return cost;
            }
        }
        if (target is Table table) {
            bool isTrapped = actor.trapStructure.IsTrapStructure(table.gridTileLocation.structure)
                || (table.gridTileLocation.collectionOwner.isPartOfParentRegionMap && actor.trapStructure.IsTrapHex(table.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner));
            if (isTrapped) {
                cost = UtilityScripts.Utilities.Rng.Next(50, 71);
                costLog += $" +{cost}(Actor is currently visiting)";
            } else if (actor.traitContainer.HasTrait("Travelling")) {
                if(table.structureLocation.structureType == STRUCTURE_TYPE.TAVERN || table.structureLocation == actor.homeStructure) {
                    cost = UtilityScripts.Utilities.Rng.Next(400, 451);
                    costLog += $" +{cost}(Travelling, Inside Tavern or in actor home structure)\n";
                } else if (actor.needsComponent.isStarving) {
                    Character tableOwner = table.characterOwner;
                    if (tableOwner != null) {
                        if (actor.relationshipContainer.IsFriendsWith(tableOwner)) {
                            cost = UtilityScripts.Utilities.Rng.Next(400, 451);
                            costLog += $" +{cost}(Travelling, Table is owned by friend/close friend and actor is starving)";
                        } else if (actor.relationshipContainer.IsEnemiesWith(tableOwner)) {
                            cost = 800;
                            costLog += $" +{cost}(Travelling, Table is owned by friend/close friend and actor is starving)";
                        } else {
                            cost = UtilityScripts.Utilities.Rng.Next(450, 501);
                            costLog += $" +{cost}(Travelling, Table owned by someone that is not friend or enemy and actor is starving)";
                        }
                    } else {
                        cost = UtilityScripts.Utilities.Rng.Next(450, 501);
                        costLog += $" +{cost}(Travelling, Table not owned)";
                    }
                } else {
                    cost += 2000;
                    costLog += $" +{cost}(Travelling but not starving)";
                } 
            } else if (table.gridTileLocation != null && table.structureLocation.settlementLocation != null && table.structureLocation.settlementLocation == actor.homeSettlement) {
                if (table.structureLocation == actor.homeStructure) {
                    cost = UtilityScripts.Utilities.Rng.Next(20, 36);
                    costLog += $" +{cost}(Table is in actor's home)";
                } else {
                    if (actor.needsComponent.isStarving) {
                        Character tableOwner = table.characterOwner;
                        if (tableOwner != null) {
                            if (actor.relationshipContainer.IsFriendsWith(tableOwner)) {
                                cost = UtilityScripts.Utilities.Rng.Next(70, 81);
                                costLog += $" +{cost}(Table is owned by friend/close friend and actor is starving)";
                            } else if (actor.relationshipContainer.IsEnemiesWith(tableOwner)) {
                                cost = 300;
                                costLog += $" +{cost}(Table is owned by friend/close friend and actor is starving)";
                            } else {
                                cost = UtilityScripts.Utilities.Rng.Next(50, 71);
                                costLog += $" +{cost}(Table owned by someone that is not friend or enemy and actor is starving)";
                            }
                        } else {
                            cost = UtilityScripts.Utilities.Rng.Next(50, 71);
                            costLog += $" +{cost}(Table not owned)";
                        }
                    } else {
                        //not starving
                        if (table.characterOwner != null && !table.IsOwnedBy(actor)
                            && table.characterOwner.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR) == false
                            && table.characterOwner.relationshipContainer.IsFamilyMember(actor) == false) {
                            cost += 2000;
                            costLog += $" +{cost}(Table personally owned by someone else who is not the Actor's Lover, Affair or Relative)";
                        } else {
                            cost = UtilityScripts.Utilities.Rng.Next(50, 71);
                            costLog += $" +{cost}(Table not owned)";
                        }
                    }
                }
            } else {
                if (table.characterOwner != null && !table.IsOwnedBy(actor) && actor.relationshipContainer.IsFriendsWith(table.characterOwner)) {
                    cost = UtilityScripts.Utilities.Rng.Next(500, 551);;
                    costLog += $" {cost}(Otherwise, if Table personally owned by Friend or Close Friend)";
                } else {
                    cost += UtilityScripts.Utilities.Rng.Next(800, 851);;
                    costLog += $" {cost}(Otherwise, if Table is NOT owned by Friend or Close Friend)";
                }
            }
        } else {
            if (target is ElfMeat || target is HumanMeat) {
                if (actor.traitContainer.HasTrait("Cannibal")) {
                    cost = UtilityScripts.Utilities.Rng.Next(450, 551);
                    costLog += $" +{cost}(Target is human/elven meat and actor is cannibal)";
                } else {
                    if (actor.needsComponent.isStarving) {
                        cost = UtilityScripts.Utilities.Rng.Next(700, 751);
                        costLog += $" +{cost}(Target is human/elven meat and actor is not cannibal but is starving)";    
                    } else {
                        cost = 2000;
                        costLog += $" +{cost}(Target is human/elven meat and actor is not cannibal and is not starving)";    
                    }
                }
            } else {
                cost = UtilityScripts.Utilities.Rng.Next(400, 451);
                costLog += $" +{cost}(Not Table)";    
            }
            
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.needsComponent.AdjustDoNotGetHungry(-1);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Eat Fail";
            }
        }
        return goapActionInvalidity;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.poiTarget is TileObject tileObject) {
            if (tileObject.characterOwner != null && !tileObject.IsOwnedBy(node.actor)
                && tileObject.characterOwner.relationshipContainer.HasRelationshipWith(node.actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR) == false
                && tileObject.characterOwner.relationshipContainer.IsFamilyMember(node.actor) == false) {
                return REACTABLE_EFFECT.Negative;        
            }
        }
        return REACTABLE_EFFECT.Neutral;
    }
    #endregion

    #region Effects
    public void PreEatSuccess(ActualGoapNode goapNode) {
        //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        //goapNode.poiTarget.SetPOIState(POI_STATE.INACTIVE);
        goapNode.actor.needsComponent.AdjustDoNotGetHungry(1);
        //actor.traitContainer.AddTrait(actor,"Eating");
    }
    //public void PerTickEatSuccess(ActualGoapNode goapNode) {
    //    //goapNode.actor.AdjustFullness(520);
    //}
    public void AfterEatSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetHungry(-1);
        //goapNode.poiTarget.SetPOIState(POI_STATE.ACTIVE);
        if (goapNode.actor.traitContainer.HasTrait("Cannibal") == false && 
            (goapNode.poiTarget is ElfMeat || goapNode.poiTarget is HumanMeat)) {
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Cannibal");
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "became_cannibal", goapNode);
            log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, goapNode.poiTarget.name, LOG_IDENTIFIER.STRING_1);
            log.AddLogToInvolvedObjects();
        }
    }
    //public void PreEatFail(ActualGoapNode goapNode) {
    //    GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
    //    goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void PreTargetMissing(ActualGoapNode goapNode) {
    //    goapNode.descriptionLog.AddToFillers(goapNode.actor.currentStructure.location, goapNode.actor.currentStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (!poiTarget.IsAvailable()) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.collectionOwner.isPartOfParentRegionMap && actor.trapStructure.IsTrappedAndTrapHexIsNot(poiTarget.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner)) {
                return false;
            }
            if (actor.traitContainer.HasTrait("Vampiric")) {
                return false;
            }
            if(poiTarget is BerryShrub && !actor.needsComponent.isStarving) {
                //If plant or animal, only eat if the actor is homeless
                if(actor.homeStructure != null) {
                    return false;
                }
            }
            if (poiTarget is Table) {
                //if target is table, do not allow if actor is a monster
                if (UtilityScripts.GameUtilities.IsRaceBeast(actor.race) || actor.isNormalCharacter == false) {
                    return false;
                }
            }
            // else {
            //     if(poiTarget.storedResources[RESOURCE.FOOD] < 12) {
            //         return false;
            //     }
            // }
            if (poiTarget.gridTileLocation != null) {
                return true;
            }
        }
        return false;
    }
    #endregion
    
    #region Preconditions
    private bool HasFood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget.HasResourceAmount(RESOURCE.FOOD, 12);
    }
    #endregion
}