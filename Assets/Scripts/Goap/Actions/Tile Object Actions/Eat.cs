using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps.Location_Structures;
using Inner_Maps;
using Locations.Settlements;
using UtilityScripts;

public class Eat : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.CONSUME; } }

    private Precondition _foodPrecondition;

    public Eat() : base(INTERACTION_TYPE.EAT) {
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Eat_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.KOBOLD, RACE.RAT, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};

        _foodPrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Food Pile" /*+ (int)otherData[0]*/, false, GOAP_EFFECT_TARGET.TARGET), HasFood);
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        if (node.crimeType != CRIME_TYPE.None && node.crimeType != CRIME_TYPE.Unset) {
            return true;
        }
        return base.ShouldActionBeAnIntel(node);
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        if (node.target is Table) {
            log.AddToFillers(node.target, "at a Table", LOG_IDENTIFIER.TARGET_CHARACTER);
        } else {
            log.AddToFillers(node.target, node.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.STAMINA_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        if (target is Table && !(actor is Summon) && jobType != JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT) {
            //Only let the character deposit food to table if the job type is not recovery on sight because if it is, it means that is his fullness recovery is an urgent one, so only eat on those tables that already have enough food
            Precondition p = _foodPrecondition;
            isOverridden = true;
            return p;
        }
        return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Eat Success", goapNode);
    }
    
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        if (actor.traitContainer.HasTrait("Enslaved")) {
            if(target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)) {
#if DEBUG_LOG
                costLog += $" +2000(Slave, target is not in actor's home)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
        //if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive) {
        //    if (actor.partyComponent.isActiveMember) {
        //        if (target.gridTileLocation != null && target.gridTileLocation.collectionOwner.isPartOfParentRegionMap && actor.gridTileLocation != null
        //        && actor.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
        //            LocationGridTile centerGridTileOfTarget = target.gridTileLocation.hexTileOwner.GetCenterLocationGridTile();
        //            LocationGridTile centerGridTileOfActor = actor.gridTileLocation.hexTileOwner.GetCenterLocationGridTile();
        //            float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
        //            int distanceToCheck = (InnerMapManager.BuildingSpotSize.x * 2) * 3;

        //            if (distance > distanceToCheck) {
        //                target is at structure that character is avoiding
        //                costLog += $" +2000(Active Party, Location of target too far from actor)";
        //                actor.logComponent.AppendCostLog(costLog);
        //                return 2000;
        //            }
        //        }
        //    }
        //}
        int cost = 0;
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            if (!actor.partyComponent.hasParty) {
                //target is at structure that character is avoiding
                cost = 2000;
#if DEBUG_LOG
                costLog += $" +{cost}(Location of target is in avoid structure)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return cost;
            }
        }
        if (actor is Rat) {
            if (target is FoodPile) {
                cost += UtilityScripts.Utilities.Rng.Next(20, 51);
            } else if (target is Table) {
                cost += UtilityScripts.Utilities.Rng.Next(10, 61);
            } else {
                return 2000;
            }
        } else if (actor.race == RACE.RATMAN && actor.faction?.factionType.type == FACTION_TYPE.Ratmen) {
            BaseSettlement settlement = null;
            if (target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(out settlement)) {
                Faction targetFaction = settlement.owner;
                if (targetFaction != null && actor.faction != targetFaction) {
                    //Do not drink on hostile faction's taverns
#if DEBUG_LOG
                    costLog += $" +2000(Ratman, Location of target is in faction different from actor)";
                    actor.logComponent.AppendCostLog(costLog);
#endif
                    return 2000;
                } else if (targetFaction == null || targetFaction.factionType.type != FACTION_TYPE.Ratmen) {
                    //Do not drink on hostile faction's taverns
                    cost += UtilityScripts.Utilities.Rng.Next(850, 951);
#if DEBUG_LOG
                    costLog += $" +{cost}(Ratman, Target is in a faction not owned by Ratmen)";
#endif
                } else {
                    cost += UtilityScripts.Utilities.Rng.Next(800, 851);
#if DEBUG_LOG
                    costLog += $" +{cost}(Ratman)";
#endif
                }
            } else {
                cost += UtilityScripts.Utilities.Rng.Next(800, 851);
#if DEBUG_LOG
                costLog += $" +{cost}(Ratman)";
#endif
            }
        } else {
            if (target is Table table) {
                bool isTrapped = actor.trapStructure.IsTrapStructure(table.gridTileLocation.structure) || actor.trapStructure.IsTrapArea(table.gridTileLocation.area);
                BaseSettlement settlementLocation = null;
                if (table.gridTileLocation != null && table.gridTileLocation.IsPartOfSettlement(out settlementLocation) && actor.faction != null && settlementLocation.owner != null && settlementLocation.owner.IsHostileWith(actor.faction)) {
                    cost += 2000;
#if DEBUG_LOG
                    costLog += $" +{cost}(Table is inside settlement owned by hostile faction)";
#endif
                } else if (isTrapped) {
                    cost = UtilityScripts.Utilities.Rng.Next(50, 71);
#if DEBUG_LOG
                    costLog += $" +{cost}(Actor is currently visiting)";
#endif
                } else if (actor.traitContainer.HasTrait("Travelling")) {
                    if (table.structureLocation.structureType == STRUCTURE_TYPE.TAVERN || table.structureLocation == actor.homeStructure) {
                        cost = UtilityScripts.Utilities.Rng.Next(400, 451);
#if DEBUG_LOG
                        costLog += $" +{cost}(Travelling, Inside Tavern or in actor home structure)\n";
#endif
                    } else if (actor.needsComponent.isStarving) {
                        Character tableOwner = table.characterOwner;
                        if (tableOwner != null) {
                            if (tableOwner == actor) {
                                cost = UtilityScripts.Utilities.Rng.Next(400, 451);
#if DEBUG_LOG
                                costLog += $" +{cost}(Travelling, Table is personally owned)";
#endif
                            } else if (actor.relationshipContainer.IsEnemiesWith(tableOwner)) {
                                cost = 996;
#if DEBUG_LOG
                                costLog += $" +{cost}(Travelling, Table is owned by friend/close friend and actor is starving)";
#endif
                            } else {
                                cost = 994;
#if DEBUG_LOG
                                costLog += $" +{cost}(Travelling, Table is not owned by actor and is not owned by Enemy/Rival)";
#endif
                            }
                        } else {
                            cost = 994;
#if DEBUG_LOG
                            costLog += $" +{cost}(Travelling, Table is not owned)";
#endif
                        }
                    } else {
                        cost += 2000;
#if DEBUG_LOG
                        costLog += $" +{cost}(Travelling but not starving and target is table)";
#endif
                    }
                } else if (table.structureLocation == actor.homeStructure) {
                    cost = UtilityScripts.Utilities.Rng.Next(20, 36);
#if DEBUG_LOG
                    costLog += $" +{cost}(Table is in actor's home)";
#endif
                } else if (settlementLocation == actor.homeSettlement) {
                    if (actor.needsComponent.isStarving) {
                        Character tableOwner = table.characterOwner;
                        if (tableOwner != null) {
                            if (actor.relationshipContainer.IsFriendsWith(tableOwner)) {
                                cost = UtilityScripts.Utilities.Rng.Next(70, 81);
#if DEBUG_LOG
                                costLog += $" +{cost}(Table is owned by friend/close friend and actor is starving)";
#endif
                            } else if (actor.relationshipContainer.IsEnemiesWith(tableOwner)) {
                                cost = 300;
#if DEBUG_LOG
                                costLog += $" +{cost}(Table is owned by friend/close friend and actor is starving)";
#endif
                            } else {
                                cost = UtilityScripts.Utilities.Rng.Next(50, 71);
#if DEBUG_LOG
                                costLog += $" +{cost}(Table owned by someone that is not friend or enemy and actor is starving)";
#endif
                            }
                        } else {
                            cost = UtilityScripts.Utilities.Rng.Next(50, 71);
#if DEBUG_LOG
                            costLog += $" +{cost}(Table not owned)";
#endif
                        }
                    } else {
                        //not starving
                        if (table.characterOwner != null && !table.IsOwnedBy(actor)
                            && table.characterOwner.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR) == false
                            && table.characterOwner.relationshipContainer.IsFamilyMember(actor) == false) {
                            cost += 2000;
#if DEBUG_LOG
                            costLog += $" +{cost}(Table personally owned by someone else who is not the Actor's Lover, Affair or Relative)";
#endif
                        } else {
                            cost = UtilityScripts.Utilities.Rng.Next(50, 71);
#if DEBUG_LOG
                            costLog += $" +{cost}(Table not owned)";
#endif
                        }
                    }
                } else {
                    if (table.characterOwner != null && !table.IsOwnedBy(actor) && actor.relationshipContainer.IsFriendsWith(table.characterOwner)) {
                        cost = 988;
#if DEBUG_LOG
                        costLog += $" {cost}(Otherwise, if Table personally owned by Friend or Close Friend)";
#endif
                    } else {
                        cost = 996;
#if DEBUG_LOG
                        costLog += $" {cost}(Otherwise, if Table is NOT owned by Friend or Close Friend)";
#endif
                    }
                }
            } else {
                //target is not a table
                if (target is ElfMeat || target is HumanMeat) {
                    if (actor.traitContainer.HasTrait("Cannibal")) {
                        cost = UtilityScripts.Utilities.Rng.Next(450, 551);
#if DEBUG_LOG
                        costLog += $" +{cost}(Target is human/elven meat and actor is cannibal)";
#endif
                    } else {
                        if (actor.needsComponent.isStarving) {
                            cost = UtilityScripts.Utilities.Rng.Next(970, 981);
#if DEBUG_LOG
                            costLog += $" +{cost}(Target is human/elven meat and actor is not cannibal but is starving)";
#endif
                        } else {
                            cost = 2000;
#if DEBUG_LOG
                            costLog += $" +{cost}(Target is human/elven meat and actor is not cannibal and is not starving)";
#endif
                        }
                    }
                } else if (target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(out var settlement) && settlement.owner != null && actor.faction != null) {
                    if (!actor.faction.IsHostileWith(settlement.owner)) {
                        if (target.gridTileLocation.structure.structureType == STRUCTURE_TYPE.TAVERN) {
                            cost = UtilityScripts.Utilities.Rng.Next(600, 651);
#if DEBUG_LOG
                            costLog += $" +{cost}(Target is inside of tavern owned by a non hostile faction)";
#endif
                        } else if (target.gridTileLocation.structure.structureType.IsSpecialStructure()) {
                            cost = UtilityScripts.Utilities.Rng.Next(700, 751);
#if DEBUG_LOG
                            costLog += $" +{cost}(Target is inside a special structure owned by a non hostile faction)";
#endif
                        } else if (target.gridTileLocation.structure is ManMadeStructure manMadeStructure && target is FoodPile) {
                            if (manMadeStructure == actor.homeStructure) {
                                cost = UtilityScripts.Utilities.Rng.Next(100, 151);
#if DEBUG_LOG
                                costLog += $" +{cost}(Target is food pile inside actors home)";
#endif
                            } else if (manMadeStructure.CanPurchaseFromHere(actor, out var needsToPay, out _)) {
                                if (needsToPay) {
                                    if (actor.moneyComponent.CanAfford(BuyFood.FoodCost)) {
                                        cost = UtilityScripts.Utilities.Rng.Next(600, 651);
#if DEBUG_LOG
                                        costLog += $" +{cost}(Target is a food pile inside food producing structure and actor NEEDS TO PAY and CAN AFFORD it)";
#endif                                        
                                    } else {
                                        cost = 2000;
#if DEBUG_LOG
                                        costLog += $" +{cost}(Target is a food pile inside food producing structure and actor NEEDS TO PAY and CANNOT AFFORD it)";
#endif                    
                                    }
                                } else {
                                    cost = UtilityScripts.Utilities.Rng.Next(100, 151);
#if DEBUG_LOG
                                    costLog += $" +{cost}(Target is a food pile inside food producing structure and actor DOES NOT NEED TO PAY)";
#endif                                    
                                }
                            } else {
                                cost += 2000;
#if DEBUG_LOG
                                costLog += $" +{cost}(Target is a food pile inside manmade structure)";
#endif
                            }
                        } else {
                            if (actor.needsComponent.isStarving) {
                                cost = UtilityScripts.Utilities.Rng.Next(970, 981);
#if DEBUG_LOG
                                costLog += $" +{cost}(Actor is starving and is inside a structure owned by a non hostile faction)";
#endif
                            } else {
                                cost += 2000;
#if DEBUG_LOG
                                costLog += $" +{cost}(Target is inside settlement owned by a non hostile faction)";
#endif
                            }
                        }
                    } else {
                        cost += 2000;
#if DEBUG_LOG
                        costLog += $" +{cost}(Target is inside settlement owned by a hostile faction)";
#endif
                    }
                } else {
                    cost = UtilityScripts.Utilities.Rng.Next(950, 961);
#if DEBUG_LOG
                    costLog += $" +{cost}(Target is not Human/Elf Meat and is not part of a settlement owned by a faction)";
#endif
                }
            }
        }
#if DEBUG_LOG
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    //public override void OnStopWhilePerforming(ActualGoapNode node) {
    //    base.OnStopWhilePerforming(node);
    //    Character actor = node.actor;
    //    actor.needsComponent.AdjustDoNotGetHungry(-1);
    //}
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Eat Fail";
            } else {
                if (poiTarget is FoodPile foodPile && foodPile.structureLocation != null && foodPile.structureLocation is ManMadeStructure manMadeStructure && 
                    manMadeStructure.structureType.IsFoodProducingStructure()) {
                    if (manMadeStructure.CanPurchaseFromHere(node.actor, out var needsToPay, out _)) {
                        if (needsToPay) {
                            if (!node.actor.moneyComponent.CanAfford(BuyFood.FoodCost)) {
                                goapActionInvalidity.isInvalid = true;
                                goapActionInvalidity.stateName = "not_enough_money";                
                            }
                        }
                    } else {
                        goapActionInvalidity.isInvalid = true;
                        goapActionInvalidity.stateName = "cannot_buy";
                    }
                }  
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
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if (actor.race.IsSapient()) {
            if (target is Character targetCharacter && targetCharacter.race.IsSapient()) {
                return CRIME_TYPE.Cannibalism;    
            } else if (target is HumanMeat || target is ElfMeat) {
                return CRIME_TYPE.Cannibalism;
            }
        }
        return base.GetCrimeType(actor, target, crime);
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        Character targetCharacter = target as Character;
        if (!witness.traitContainer.HasTrait("Cannibal") && ((targetCharacter != null && targetCharacter.race.IsSapient()) || target is HumanMeat || target is ElfMeat)) {
            reactions.Add(EMOTION.Shock);
            reactions.Add(EMOTION.Disgust);

            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                reactions.Add(EMOTION.Disappointment);
            }
            if (!witness.traitContainer.HasTrait("Psychopath")) {
                reactions.Add(EMOTION.Fear);
            }
        }
        
        if (targetCharacter != null) {
            string witnessOpinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (witnessOpinionToTarget == RelationshipManager.Friend || witnessOpinionToTarget == RelationshipManager.Close_Friend) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Rage);
                }
            } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER))
                && witnessOpinionToTarget != RelationshipManager.Rival) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Rage);
                }
            } else if (witnessOpinionToTarget == RelationshipManager.Acquaintance
                || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Anger);
                }
            }
        }
    }
    public override bool IsFullnessRecoveryAction() {
        return true;
    }
#endregion

    #region Effects
    public void PreEatSuccess(ActualGoapNode goapNode) {
        // //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        // //goapNode.poiTarget.SetPOIState(POI_STATE.INACTIVE);
        // goapNode.actor.needsComponent.AdjustDoNotGetHungry(1);
        // //actor.traitContainer.AddTrait(actor,"Eating");
        IPointOfInterest poiTarget = goapNode.poiTarget;
        if (poiTarget is FoodPile foodPile && foodPile.structureLocation != null && foodPile.structureLocation is ManMadeStructure manMadeStructure && 
            manMadeStructure.structureType.IsFoodProducingStructure()) {
            if (manMadeStructure.CanPurchaseFromHere(goapNode.actor, out var needsToPay, out _)) {
                if (needsToPay) {
                    goapNode.actor.moneyComponent.AdjustCoins(-BuyFood.FoodCost);
                }
            }
        }
        LocationStructure targetStructure = goapNode.poiTarget.gridTileLocation?.structure;
        if (targetStructure != null && targetStructure.structureType == STRUCTURE_TYPE.TAVERN) {
            if (targetStructure is ManMadeStructure mmStructure) {
                if (mmStructure.HasAssignedWorker()) {
                    //only added coins to first worker since we expect that the tavern only has 1 worker.
                    //if that changes, this needs to be changed as well.
                    string assignedWorkerID = mmStructure.assignedWorkerIDs[0];
                    Character assignedWorker = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(assignedWorkerID);
                    assignedWorker.moneyComponent.AdjustCoins(33);
                }
                // Character assignedWorker = mmStructure.assignedWorker;
                // if (assignedWorker != null) {
                //     assignedWorker.moneyComponent.AdjustCoins(10);
                // }
            }
        }

    }
    //public void PerTickEatSuccess(ActualGoapNode goapNode) {
    //    //goapNode.actor.AdjustFullness(520);
    //}
    public void AfterEatSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.needsComponent.AdjustDoNotGetHungry(-1);
        //goapNode.poiTarget.SetPOIState(POI_STATE.ACTIVE);
        if (!goapNode.actor.traitContainer.HasTrait("Cannibal") && 
            (goapNode.poiTarget is ElfMeat || goapNode.poiTarget is HumanMeat) && goapNode.actor.isNotSummonAndDemon) {
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Cannibal");
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "became_cannibal", goapNode, LogUtilities.Become_Cannibal_Tags);
            log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, goapNode.poiTarget.name, LOG_IDENTIFIER.STRING_1);
            log.AddLogToDatabase(true);
        }
        if (goapNode.actor.race == RACE.ELVES && goapNode.poiTarget is RatMeat) {
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Poor Meal");
        }

        if (goapNode.poiTarget is Table table) {
            table.ApplyFoodEffectsToConsumer(goapNode.actor);
        } else if (goapNode.poiTarget is FoodPile foodPile) {
            foodPile.ApplyFoodEffectsToConsumer(goapNode.actor);
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
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (job.jobType.IsFullnessRecoveryTypeJob()) {
                LocationStructure structure = poiTarget.gridTileLocation?.structure;
                if (structure == null) { return false; }
                if (structure is Dwelling) {
                    if (!structure.IsResident(actor)) {
                        return false;
                    }
                } else if (structure.structureType.IsFoodProducingStructure() && structure is ManMadeStructure manMadeStructure) {
                    if (actor.homeStructure != null && !manMadeStructure.DoesCharacterWorkHere(actor)) {
                        //only limit eating from food producing structure if actor has a home.
                        //If the actor is homeless allow them to eat there.
                        return false;
                    }
                }
                // if (structure != null && actor.homeStructure != null) {
                //     //only perform this checking if the actor is NOT homeless,
                //     //because we want homeless villagers to be able to eat at other structures if they can pay for it.
                //     //Reference: https://trello.com/c/ZITxj5nD/4765-homeless-villagers-belonging-to-a-village-should-be-able-to-purchase-and-eat-food-owned-by-others
                //     if (structure is Dwelling) {
                //         if (!structure.IsResident(actor)) {
                //             return false;
                //         }
                //     } else if (structure.structureType.IsFoodProducingStructure()) {
                //         if (structure is ManMadeStructure manMadeStructure && manMadeStructure.assignedWorker != actor) {
                //             return false;
                //         }
                //     }
                // }
            }
            if (!poiTarget.IsAvailable()) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area)) {
                return false;
            }
            if (actor.traitContainer.HasTrait("Vampire")) {
                return false;
            }
            if(poiTarget is BerryShrub && !actor.needsComponent.isStarving) {
                //If plant or animal, only eat if the actor is homeless
                if(actor.homeStructure != null) {
                    return false;
                }
            }
            if (poiTarget is Table) {
                if (poiTarget.resourceStorageComponent.GetResourceValue(RESOURCE.FOOD) < 10 && job.jobType == JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT) {
                    return false;
                }
                //Allow rats to eat at table
                if (!(actor is Rat)) {
                    //if target is table, do not allow if actor is a monster
                    if (UtilityScripts.GameUtilities.IsRaceBeast(actor.race) || actor.isNormalCharacter == false) {
                        return false;
                    }
                }
            } else if (poiTarget is FoodPile) {
                if (poiTarget.resourceStorageComponent.GetResourceValue(RESOURCE.FOOD) < 10 && job.jobType == JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT) {
                    return false;
                }
            }
            if (poiTarget.gridTileLocation != null) {
                return true;
            }
        }
        return false;
    }
    #endregion
    
    #region Preconditions
    private bool HasFood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return poiTarget.resourceStorageComponent.HasResourceAmount(RESOURCE.FOOD, 20);
    }
    #endregion
}