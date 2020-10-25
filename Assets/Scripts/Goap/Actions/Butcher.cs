using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Logs;
using UnityEngine;  
using Traits;

public class Butcher : GoapAction {

    public Butcher() : base(INTERACTION_TYPE.BUTCHER) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.KOBOLD, RACE.TROLL };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, };
        isNotificationAnIntel = true;
        logTags = new[] {LOG_TAG.Work, LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetDead);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_FOOD, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Transform Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = "";
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            if (!actor.partyComponent.hasParty) {
                //target is at structure that character is avoiding
                costLog += $" +2000(Location of target is in avoid structure)";
                actor.logComponent.AppendCostLog(costLog);
                return 2000;
            }
        }
        if (job.jobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP) {
            if (target.gridTileLocation != null && target.gridTileLocation.collectionOwner.isPartOfParentRegionMap && actor.gridTileLocation != null
                && actor.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                LocationGridTile centerGridTileOfTarget = target.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetCenterLocationGridTile();
                LocationGridTile centerGridTileOfActor = actor.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetCenterLocationGridTile();
                float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                int distanceToCheck = (InnerMapManager.BuildingSpotSize.x * 2) * 3;

                if (distance > distanceToCheck) {
                    //target is at structure that character is avoiding
                    costLog += $" +2000(Location of target too far from actor)";
                    actor.logComponent.AppendCostLog(costLog);
                    return 2000;
                }
            }
            if (target is Character characterTarget) {
               if(actor.partyComponent.hasParty && actor.partyComponent.currentParty.IsMember(characterTarget)) {
                    //Should not butcher party members when party is camping
                    costLog += $" +2000(Cannot butcher party member when camping)";
                    actor.logComponent.AppendCostLog(costLog);
                    return 2000;
                }
            }
        }
        costLog = $"\n{name} {target.nameWithID}:";
        Character targetCharacter = GetDeadCharacter(target);
        int cost = 0;
        //int cost = GetFoodAmountTakenFromDead(deadCharacter);
        //costLog += " +" + cost + "(Initial)";
        if(targetCharacter != null) {
            if (actor == targetCharacter) {
                cost += 2000;
                costLog += " +2000(Actor/Target Same)";
            } else {
                if (actor.isNormalCharacter == false) {
                    cost += 10;
                    costLog += " +10(Actor is not a normal character)";
                } else {
                    bool isCannibal = actor.traitContainer.HasTrait("Cannibal");
                    if (job.jobType == JOB_TYPE.TRIGGER_FLAW && isCannibal) {
                        cost = UtilityScripts.Utilities.Rng.Next(450, 551);
                        costLog += $" {cost}(Actor is cannibal and job is trigger flaw)";
                        actor.logComponent.AppendCostLog(costLog);
                        return cost;
                    }
                    if (isCannibal) {
                        if (actor.traitContainer.HasTrait("Malnourished")) {
                            if (actor.relationshipContainer.IsFriendsWith(targetCharacter)) {
                                int currCost = UtilityScripts.Utilities.Rng.Next(100, 151);
                                cost += currCost;
                                costLog += $" +{currCost}(Cannibal, Malnourished, Friend/Close)";
                            } else if (targetCharacter.race == RACE.HUMANS || targetCharacter.race == RACE.ELVES) {
                                cost += 300;
                                costLog += " +300(Cannibal, Malnourished, Human/Elf)";
                            }
                        } else {
                            if (actor.relationshipContainer.IsFriendsWith(targetCharacter)) {
                                cost += 2000;
                                costLog += " +2000(Cannibal, Friend/Close)";
                            } else if ((targetCharacter.race == RACE.HUMANS || targetCharacter.race == RACE.ELVES) && !actor.needsComponent.isStarving) {
                                cost += 400;
                                costLog += " +2000(Cannibal, Human/Elf, not Starving)";
                            }
                        }
                    } else {
                        //not cannibal
                        if (actor.traitContainer.HasTrait("Malnourished")) {
                            if (actor.relationshipContainer.IsFriendsWith(targetCharacter)) {
                                int currCost = UtilityScripts.Utilities.Rng.Next(100, 151);
                                cost += currCost;
                                costLog += $" +{currCost}(not Cannibal, Malnourished, Friend/Close)";
                            } else if (targetCharacter.race == RACE.HUMANS || targetCharacter.race == RACE.ELVES) {
                                cost += 500;
                                costLog += " +500(not Cannibal, Malnourished, Human/Elf)";
                            }
                        } else {
                            if (actor.needsComponent.isStarving) {
                                if (actor.relationshipContainer.IsFriendsWith(targetCharacter)) {
                                    int currCost = 300;
                                    cost += currCost;
                                    costLog += $" +300(not Cannibal, not Malnourished but starving, Friend/Close)";
                                }
                            } else {
                                if (targetCharacter.race == RACE.HUMANS || targetCharacter.race == RACE.ELVES) {
                                    cost += 2000;
                                    costLog += " +2000(not Cannibal, not malnourished or starving, Human/Elf)";
                                }    
                            }
                        }
                    }
                }
            }
            if(targetCharacter.race == RACE.HUMANS) {
                int currCost = UtilityScripts.Utilities.Rng.Next(80, 91);
                cost += currCost;
                costLog += $" +{currCost}(Human)";
            } else if (targetCharacter.race == RACE.ELVES) {
                int currCost = UtilityScripts.Utilities.Rng.Next(80, 91);
                cost += currCost;
                costLog += $" +{currCost}(Elf)";
            }
            //else if (deadCharacter.race == RACE.WOLF) {
            //    int currCost = UtilityScripts.Utilities.Rng.Next(50, 61);
            //    cost += currCost;
            //    costLog += $" +{currCost}(Wolf)";
            //} 
            else if (targetCharacter.race == RACE.DEMON || targetCharacter.race == RACE.LESSER_DEMON) {
                int currCost = UtilityScripts.Utilities.Rng.Next(90, 111);
                cost += currCost;
                costLog += $" +{currCost}(Demon)";
            }
            if (!targetCharacter.isDead) {
                cost *= 2;
                costLog += $" {cost}(Still Alive)";
            }
        }
        if (targetCharacter is Animal || targetCharacter.race == RACE.WOLF || targetCharacter.race == RACE.SPIDER) {
            CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(actor, actor, targetCharacter, CRIME_TYPE.Animal_Killing);
            int currCost = 0;
            if(severity == CRIME_SEVERITY.Infraction) {
                currCost = UtilityScripts.Utilities.Rng.Next(80, 91);
                costLog += $" +{currCost}(Animal/Infraction)";
            } else if (severity == CRIME_SEVERITY.Misdemeanor || severity == CRIME_SEVERITY.Serious || severity == CRIME_SEVERITY.Heinous) {
                if (actor.traitContainer.HasTrait("Malnourished")) {
                    if (actor.relationshipContainer.IsFriendsWith(targetCharacter)) {
                        currCost = 200;
                        costLog += $" +{currCost}(Animal/Misdemeanor/Serious/Heinous/Malnourished/Friend/Close Friend)";
                    } else {
                        currCost = UtilityScripts.Utilities.Rng.Next(100, 111);
                        costLog += $" +{currCost}(Animal/Misdemeanor/Serious/Heinous/Malnourished)";
                    }
                } else {
                    currCost = 2000;
                    costLog += $" +{currCost}(Animal/Misdemeanor/Serious/Heinous/not Malnourished)";
                }
            } else {
                currCost = UtilityScripts.Utilities.Rng.Next(40, 51);
                costLog += $" +{currCost}(Animal/No Severity)";
            }
            cost += currCost;
            if (!targetCharacter.isDead) {
                cost *= 2;
                costLog += $" {cost}(Still Alive)";
            }
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void AddFillersToLog(ref Log log, ActualGoapNode node) {
        base.AddFillersToLog(ref log, node);
        IPointOfInterest poiTarget = node.poiTarget;
        if(node.poiTarget is Tombstone) {
            poiTarget = (node.poiTarget as Tombstone).character;
        }
        log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool defaultTargetMissing = this.IsTargetMissing(actor, poiTarget);
        return new GoapActionInvalidity(defaultTargetMissing, stateName);
    }
    private bool IsTargetMissing(Character actor, IPointOfInterest poiTarget) {
        return poiTarget.gridTileLocation == null || actor.currentRegion != poiTarget.currentRegion
              || !(actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) 
              || (poiTarget is Character character && !character.isDead) || poiTarget.numOfActionsBeingPerformedOnThis > 0;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        Character targetCharacter = GetDeadCharacter(target);
        if (targetCharacter != null) {
            if (!witness.traitContainer.HasTrait("Cannibal") &&
                (targetCharacter.race == RACE.HUMANS || targetCharacter.race == RACE.ELVES)) {
                //CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_SEVERITY.Heinous);
                CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);

                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor, status, node);
            
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
                if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, actor, status, node);
                }
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
                }
            }
            string witnessOpinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (witnessOpinionToTarget == RelationshipManager.Friend || witnessOpinionToTarget == RelationshipManager.Close_Friend) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, witness, actor, status, node);
                }
            } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER))
                && witnessOpinionToTarget != RelationshipManager.Rival) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, witness, actor, status, node);
                }
            } else if (witnessOpinionToTarget == RelationshipManager.Acquaintance 
                || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                }
            }
        }
        return response;
    }
    public override string ReactionToTarget(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToTarget(actor, target, witness, node, status);
        Character targetCharacter = GetDeadCharacter(target);
        if (targetCharacter != null) {
            string witnessOpinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (witnessOpinionToTarget == RelationshipManager.Friend || witnessOpinionToTarget == RelationshipManager.Close_Friend) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, targetCharacter, status, node);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Sadness, witness, targetCharacter, status, node);
                }
            } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                && witnessOpinionToTarget != RelationshipManager.Rival) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, targetCharacter, status, node);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Sadness, witness, targetCharacter, status, node);
                }
            }
        }
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.poiTarget is Character character) {
            if (character.isNormalCharacter) {
                return REACTABLE_EFFECT.Negative;
            }
        }
        return REACTABLE_EFFECT.Positive;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if (target is Character targetCharacter) {
            if ((actor.race == RACE.HUMANS || actor.race == RACE.ELVES) && (targetCharacter.race == RACE.HUMANS || targetCharacter.race == RACE.ELVES)) {
                return CRIME_TYPE.Cannibalism;
            } else if((actor.race == RACE.HUMANS || actor.race == RACE.ELVES) && (targetCharacter is Animal || targetCharacter.race == RACE.WOLF || targetCharacter.race == RACE.SPIDER || targetCharacter.race == RACE.KOBOLD)) {
                return CRIME_TYPE.Animal_Killing;
            }
        }
        return base.GetCrimeType(actor, target, crime);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            return true;
            //if(poiTarget is Animal) {
            //    return true;
            //} else if (actor.isNormalCharacter == false) {
            //    return true;
            //} else {
            //    Character deadCharacter = GetDeadCharacter(poiTarget);
            //    if (deadCharacter != null && (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES)
            //        && actor.faction != deadCharacter.faction && actor.homeSettlement != deadCharacter.homeSettlement) {
            //        if (actor.traitContainer.HasTrait("Cannibal")) {
            //            return true;
            //        }
            //        return false;
            //    }
            //}
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool IsTargetDead(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        if (poiTarget is Character character) {
            return character.isDead;
        }
        return true;
    }
    #endregion

    private Character GetDeadCharacter(IPointOfInterest poiTarget) {
        if (poiTarget is Character target) {
            return target;
        } else if (poiTarget is Tombstone tombstone) {
            return tombstone.character;
        }
        return null;
    }

    #region State Effects
    public void PreTransformSuccess(ActualGoapNode goapNode) {
        Character deadCharacter = GetDeadCharacter(goapNode.poiTarget);
        int transformedFood = CharacterManager.Instance.GetFoodAmountTakenFromPOI(deadCharacter);

        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        goapNode.descriptionLog.AddToFillers(null, transformedFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterTransformSuccess(ActualGoapNode goapNode) {
        IPointOfInterest poiTarget = goapNode.poiTarget;
        LocationGridTile tileLocation = poiTarget.gridTileLocation;

        tileLocation.structure.RemoveCharacterAtLocation(poiTarget as Character);
        if (poiTarget is Character character) {
            if (character.grave != null && character.grave.gridTileLocation != null) {
                //if character is at a tombstone, destroy tombstone and character marker.
                character.grave.SetRespawnCorpseOnDestroy(false);
                character.grave.gridTileLocation.structure.RemovePOI(character.grave);
            } else {
                if (character.currentRegion != null) {
                    character.currentRegion.RemoveCharacterFromLocation(character);
                }
                character.DestroyMarker();    
            }
        }

        FoodPile foodPile = CharacterManager.Instance.CreateFoodPileForPOI(poiTarget, tileLocation, false);
        if (goapNode.associatedJobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP) {
            if (goapNode.actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty.targetCamp != null) {
                goapNode.actor.partyComponent.currentParty.jobComponent.CreateHaulForCampJob(foodPile, goapNode.actor.partyComponent.currentParty.targetCamp);
                goapNode.actor.marker.AddPOIAsInVisionRange(foodPile); //automatically add pile to character's vision so he/she can take haul job immediately after
            }
        } else {
            if (foodPile != null && goapNode.actor.homeSettlement != null && goapNode.actor.isNormalCharacter && !(foodPile is HumanMeat) && !(foodPile is ElfMeat)) {
                goapNode.actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(foodPile);
                goapNode.actor.marker.AddPOIAsInVisionRange(foodPile); //automatically add pile to character's vision so he/she can take haul job immediately after
            }
        }
        if (foodPile != null) {
            goapNode.descriptionLog.AddInvolvedObjectManual(foodPile.persistentID);    
            //if produced human/elf meat and the actor is not a cannibal, make him/her traumatized
            if((foodPile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT || foodPile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT) 
               && !goapNode.actor.traitContainer.HasTrait("Cannibal") && goapNode.actor.isNormalCharacter) {
                goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Traumatized");
            }
        }
    }
    #endregion
}