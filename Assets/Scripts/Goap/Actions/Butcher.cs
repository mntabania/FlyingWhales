using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class Butcher : GoapAction {

    public Butcher() : base(INTERACTION_TYPE.BUTCHER) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.KOBOLD };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, };
        isNotificationAnIntel = true;
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
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        Character deadCharacter = GetDeadCharacter(target);
        int cost = 0;
        //int cost = GetFoodAmountTakenFromDead(deadCharacter);
        //costLog += " +" + cost + "(Initial)";
        if(deadCharacter != null) {
            if (actor == deadCharacter) {
                cost += 2000;
                costLog += " +2000(Actor/Target Same)";
            } else {
                if (actor.isNormalCharacter == false) {
                    cost += 10;
                    costLog += " +10(Actor is not a normal character)";
                } else if (actor.traitContainer.HasTrait("Cannibal")) {
                    if (actor.traitContainer.HasTrait("Malnourished")) {
                        if (actor.relationshipContainer.IsFriendsWith(deadCharacter)) {
                            int currCost = UtilityScripts.Utilities.Rng.Next(100, 151);
                            cost += currCost;
                            costLog += $" +{currCost}(Cannibal, Malnourished, Friend/Close)";
                        } else if (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) {
                            cost += 300;
                            costLog += " +300(Cannibal, Malnourished, Human/Elf)";
                        }
                    } else {
                        if (actor.relationshipContainer.IsFriendsWith(deadCharacter)) {
                            cost += 2000;
                            costLog += " +2000(Cannibal, Friend/Close)";
                        } else if ((deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) &&
                                   !actor.needsComponent.isStarving) {
                            cost += 2000;
                            costLog += " +2000(Cannibal, Human/Elf, not Starving)";
                        }
                    }
                } else {
                    if (actor.traitContainer.HasTrait("Malnourished")) {
                        if (actor.relationshipContainer.IsFriendsWith(deadCharacter)) {
                            int currCost = UtilityScripts.Utilities.Rng.Next(100, 151);
                            cost += currCost;
                            costLog += $" +{currCost}(not Cannibal, Malnourished, Friend/Close)";
                        } else if (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) {
                            cost += 500;
                            costLog += " +500(not Cannibal, Malnourished, Human/Elf)";
                        }
                    } else {
                        if (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) {
                            cost += 2000;
                            costLog += " +2000(not Cannibal, Human/Elf)";
                        }
                    }
                }
            }
            if(deadCharacter.race == RACE.HUMANS) {
                int currCost = UtilityScripts.Utilities.Rng.Next(80, 101);
                cost += currCost;
                costLog += $" +{currCost}(Human)";
            } else if (deadCharacter.race == RACE.ELVES) {
                int currCost = UtilityScripts.Utilities.Rng.Next(80, 101);
                cost += currCost;
                costLog += $" +{currCost}(Elf)";
            } else if (deadCharacter.race == RACE.WOLF) {
                int currCost = UtilityScripts.Utilities.Rng.Next(50, 81);
                cost += currCost;
                costLog += $" +{currCost}(Wolf)";
            } else if (deadCharacter.race == RACE.DEMON) {
                int currCost = UtilityScripts.Utilities.Rng.Next(90, 111);
                cost += currCost;
                costLog += $" +{currCost}(Demon)";
            }
        }
        if (target is Animal) {
            cost += UtilityScripts.Utilities.Rng.Next(40, 61);
            costLog += $" +{cost}(Animal)";
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
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
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        Character targetCharacter = GetDeadCharacter(target);
        if (targetCharacter != null) {
            if (!witness.traitContainer.HasTrait("Cannibal") &&
                (targetCharacter.race == RACE.HUMANS || targetCharacter.race == RACE.ELVES)) {
                CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.HEINOUS);
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
            if (witnessOpinionToTarget == RelationshipManager.Friend || witnessOpinionToTarget == RelationshipManager.Close_Friend || witnessOpinionToTarget == RelationshipManager.Acquaintance 
                || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
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
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            if(poiTarget is Animal) {
                return true;
            } else if (actor.isNormalCharacter == false) {
                return true;
            } else {
                Character deadCharacter = GetDeadCharacter(poiTarget);
                if (deadCharacter != null && (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES)
                    && actor.faction != deadCharacter.faction && actor.homeSettlement != deadCharacter.homeSettlement) {
                    if (actor.traitContainer.HasTrait("Cannibal")) {
                        return true;
                    }
                    return false;
                }
            }
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

        if (poiTarget is Character character) {
            character.DestroyMarker();
        } else {
            tileLocation.structure.RemovePOI(poiTarget, goapNode.actor);
        }

        FoodPile foodPile = CharacterManager.Instance.CreateFoodPileForPOI(poiTarget, tileLocation);

        //if produced human/elf meat and the actor is not a cannibal, make him/her traumatized
        if((foodPile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT || foodPile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT) && 
           !goapNode.actor.traitContainer.HasTrait("Cannibal")) {
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Traumatized");
        }
    }
    #endregion
}

public class ButcherData : GoapActionData {
    public ButcherData() : base(INTERACTION_TYPE.BUTCHER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        Character targetCharacter = null;
        if (poiTarget is Character) {
            targetCharacter = poiTarget as Character;
        } else if (poiTarget is Tombstone) {
            targetCharacter = (poiTarget as Tombstone).character;
        }
        if (targetCharacter != null) {
            if (targetCharacter.race == RACE.HUMANS || targetCharacter.race == RACE.ELVES) {
                //return true;
                if (actor.traitContainer.HasTrait("Cannibal")) {
                    return true;
                }
                return false;
            }
            return true;
        }
        return false;
    }
}
