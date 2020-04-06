using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class Butcher : GoapAction {

    public Butcher() : base(INTERACTION_TYPE.BUTCHER) {
        actionIconString = GoapActionStateDB.Work_Icon;
        canBeAdvertisedEvenIfActorIsUnavailable = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetDead);
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
                if (actor.traitContainer.HasTrait("Cannibal")) {
                    if (actor.relationshipContainer.IsFriendsWith(deadCharacter)) {
                        cost += 2000;
                        costLog += " +2000(Cannibal, Friend/Close)";
                    } else if ((deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) &&
                               !actor.needsComponent.isStarving) {
                        cost += 2000;
                        costLog += " +2000(Cannibal, Human/Elf, not Starving)";
                    }
                } else {
                    if (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) {
                        cost += 2000;
                        costLog += " +2000(not Cannibal, Human/Elf)";
                    }
                }
            }
            if(deadCharacter.race == RACE.HUMANS) {
                cost += UtilityScripts.Utilities.Rng.Next(40, 51);
                costLog += $" +{cost}(Human)";
            } else if (deadCharacter.race == RACE.ELVES) {
                cost += UtilityScripts.Utilities.Rng.Next(40, 51);
                costLog += $" +{cost}(Elf)";
            } else if (deadCharacter.race == RACE.WOLF) {
                cost += UtilityScripts.Utilities.Rng.Next(20, 31);
                costLog += $" +{cost}(Wolf)";
            } else if (deadCharacter.race == RACE.DEMON) {
                cost += UtilityScripts.Utilities.Rng.Next(80, 91);
                costLog += $" +{cost}(Demon)";
            }
        }
        if(target is SmallAnimal) {
            cost += UtilityScripts.Utilities.Rng.Next(60, 71);
            costLog += $" +{cost}(Small Animal)";
        } else if (target is Animal) {
            cost += UtilityScripts.Utilities.Rng.Next(0, 11);
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
              || !(actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) || (poiTarget.poiType == POINT_OF_INTEREST_TYPE.CHARACTER && !(poiTarget as Character).isDead);
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
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor, status);
            
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
                if (opinionLabel == OpinionComponent.Acquaintance || opinionLabel == OpinionComponent.Friend || opinionLabel == OpinionComponent.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, actor, status);
                }
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status);
                }
            }
            string witnessOpinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (witnessOpinionToTarget == OpinionComponent.Friend || witnessOpinionToTarget == OpinionComponent.Close_Friend || witnessOpinionToTarget == OpinionComponent.Acquaintance 
                || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status);
                }
            }
        }
        return response;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            if(poiTarget is SmallAnimal || poiTarget is Animal) {
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
    private bool IsTargetDead(Character actor, IPointOfInterest poiTarget, object[] otherData) {
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
        int transformedFood = CharacterManager.Instance.GetFoodAmountTakenFromDead(deadCharacter);

        //if (deadCharacter.race == RACE.HUMANS || deadCharacter.race == RACE.ELVES) {
        //    currentState.SetIntelReaction(CannibalTransformSuccessIntelReaction);
        //} else {
        //    currentState.SetIntelReaction(NormalTransformSuccessIntelReaction);
        //}
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        goapNode.descriptionLog.AddToFillers(null, transformedFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterTransformSuccess(ActualGoapNode goapNode) {
        IPointOfInterest poiTarget = goapNode.poiTarget;
        LocationGridTile tileLocation = poiTarget.gridTileLocation;
        Character deadCharacter = GetDeadCharacter(poiTarget);
        int transformedFood = CharacterManager.Instance.GetFoodAmountTakenFromDead(deadCharacter);
        //goapNode.actor.AdjustFood(transformedFood);

        if (poiTarget is Character character) {
            character.DestroyMarker();
        } else {
            tileLocation.structure.RemovePOI(poiTarget, goapNode.actor);
        }

        FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.FOOD_PILE);
        foodPile.SetResourceInPile(transformedFood);
        tileLocation.structure.AddPOI(foodPile, tileLocation);
        foodPile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.FOOD_PILE);
    }
    //public void PreTargetMissing() {
    //    goapNode.descriptionLog.AddToFillers(deadCharacter, deadCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    #endregion
    //#region Intel Reactions
    //private List<string> NormalTransformSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();

    //    if (isOldNews) {
    //        //Old News
    //        reactions.Add("This is old news.");
    //    } else {
    //        //Not Yet Old News
    //        if (awareCharactersOfThisAction.Contains(recipient)) {
    //            //- If Recipient is Aware
    //            reactions.Add("I know that already.");
    //        } else {
    //            reactions.Add("This isn't important.");
    //        }
    //    }
    //    return reactions;
    //}
    //private List<string> CannibalTransformSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;

    //    RELATIONSHIP_EFFECT relWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //    RELATIONSHIP_EFFECT relWithTarget = recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo);

    //    if (isOldNews) {
    //        //Old News
    //        reactions.Add("This is old news.");
    //    } else {
    //        //Not Yet Old News
    //        if (awareCharactersOfThisAction.Contains(recipient)) {
    //            //- If Recipient is Aware
    //            reactions.Add("I know that already.");
    //        } else {
    //            //- Recipient is Actor
    //            if (recipient == actor) {
    //                reactions.Add("Do not tell anybody, please!");
    //            }
    //            //- Positive Relationship with Actor
    //            else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                }
    //                reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //            }
    //            //- Negative Relationship with Actor
    //            else if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed");
    //                    }
    //                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //                } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //                } else {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed");
    //                    }
    //                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //                }
    //            }
    //            //- No Relationship with Actor
    //            else if (relWithActor == RELATIONSHIP_EFFECT.NONE) {
    //                if (relWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.CreateKnockoutJob(actor);
    //                    } else if (status == SHARE_INTEL_STATUS.INFORMED) {
    //                        recipient.CreateUndermineJobOnly(actor, "informed");
    //                    }
    //                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //                } else if (relWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //                } else {
    //                    recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                    }
    //                    reactions.Add(string.Format("What a sick monster! {0} should be restrained!", actor.name));
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
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
