using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class DrinkBlood : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.CONSUME; } }

    public DrinkBlood() : base(INTERACTION_TYPE.DRINK_BLOOD) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Drink_Blood_Icon;
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET }, HasUnconsciousOrRestingTarget);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Lethargic", target = GOAP_EFFECT_TARGET.TARGET });
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drink Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 0;
        if (actor.moodComponent.moodState == MOOD_STATE.NORMAL) {
            cost = UtilityScripts.Utilities.Rng.Next(50, 61);
            costLog += $" +{cost}(Normal Mood)";
        } else if (actor.moodComponent.moodState == MOOD_STATE.LOW) {
            cost = UtilityScripts.Utilities.Rng.Next(20, 31);
            costLog += $" +{cost}(Low Mood)";
        } else if (actor.moodComponent.moodState == MOOD_STATE.CRITICAL) {
            cost = UtilityScripts.Utilities.Rng.Next(0, 11);
            costLog += $" +{cost}(Critical Mood)";
        }
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (targetCharacter.traitContainer.HasTrait("Vampiric")) {
                cost += 2000;
                costLog += " +2000(Vampire)";
                actor.logComponent.AppendCostLog(costLog);
                //Skip further cost processing
                return cost;
            }
            if (targetCharacter.canPerform && targetCharacter.canMove) {
                cost += 30;
                costLog += " +30(Can Perform)";
            }
            if (actor.needsComponent.isHungry || (!actor.needsComponent.isHungry && !actor.needsComponent.isStarving)) {
                if(actor.currentRegion != targetCharacter.currentRegion) {
                    cost += 2000;
                    costLog += " +2000(Hungry, Diff Region)";
                    actor.logComponent.AppendCostLog(costLog);
                    //Skip further cost processing
                    return cost;
                }
                string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    cost += 2000;
                    costLog += " +2000(Hungry, Friend/Close)";
                    actor.logComponent.AppendCostLog(costLog);
                    //Skip further cost processing
                    return cost;
                } else if (opinionLabel == RelationshipManager.Rival) {
                    cost += 0;
                    costLog += " +0(Hungry, Rival)";
                } else if (opinionLabel == RelationshipManager.Enemy) {
                    cost += 15;
                    costLog += " +15(Hungry, Enemy)";
                } else if (opinionLabel == RelationshipManager.Acquaintance) {
                    cost += 65;
                    costLog += " +65(Hungry, Acquaintance)";
                } else {
                    cost += 35;
                    costLog += " +35(Hungry, Other)";
                }
            } else if (actor.needsComponent.isStarving) {
                if (actor.currentRegion != targetCharacter.currentRegion) {
                    cost += 2000;
                    costLog += " +2000(Starving, Diff Region)";
                    actor.logComponent.AppendCostLog(costLog);
                    //Skip further cost processing
                    return cost;
                }
                string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == RelationshipManager.Close_Friend) {
                    cost += 60;
                    costLog += " +60(Starving, Close Friend)";
                } else if (opinionLabel == RelationshipManager.Friend) {
                    cost += 45;
                    costLog += " +45(Starving, Friend)";
                } else if (opinionLabel == RelationshipManager.Rival) {
                    cost += 0;
                    costLog += " +0(Starving, Rival)";
                } else if (opinionLabel == RelationshipManager.Enemy) {
                    cost += 5;
                    costLog += " +5(Starving, Enemy)";
                } else if (opinionLabel == RelationshipManager.Acquaintance) {
                    cost += 10;
                    costLog += " +10(Starving, Acquaintance)";
                } else {
                    cost += 5;
                    costLog += " +5(Starving, Other)";
                }
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
        GoapActionInvalidity actionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (actionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            if (targetCharacter.canMove && targetCharacter.canPerform/*|| targetCharacter.canWitness || targetCharacter.IsAvailable() == false*/) {
                actionInvalidity.isInvalid = true;
                actionInvalidity.stateName = "Drink Fail";
            }
        }
        return actionInvalidity;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (!witness.traitContainer.HasTrait("Vampiric")) {
            CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.HEINOUS);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);

            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status, node);
            }
            if(witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
            } else if (!witness.traitContainer.HasTrait("Psychopath")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
            }
        }
        if(target is Character) {
            Character targetCharacter = target as Character;
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
            } else if (opinionLabel == RelationshipManager.Acquaintance || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                }
            }
        }
        return response;
    }
    public override string ReactionOfTarget(ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionOfTarget(node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (target is Character) {
            Character targetCharacter = target as Character;
            CrimeManager.Instance.ReactToCrime(targetCharacter, actor, node, node.associatedJobType, CRIME_TYPE.HEINOUS);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, targetCharacter, actor, status, node);
            if (targetCharacter.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, targetCharacter, actor, status, node);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, targetCharacter, actor, status, node);
            }
            if (targetCharacter.relationshipContainer.IsFriendsWith(actor)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, targetCharacter, actor, status, node);
            }
        }
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //if (actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            //    return false;
            //}
            return actor != poiTarget && actor.traitContainer.HasTrait("Vampiric");
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasUnconsciousOrRestingTarget(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        Character target = poiTarget as Character;
        return target.traitContainer.HasTrait("Unconscious", "Resting");
    }
    #endregion

    #region Effects
    public void PreDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetHungry(1);
    }
    public void PerTickDrinkSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustFullness(17f);
    }
    public void AfterDrinkSuccess(ActualGoapNode goapNode) {
        //poiTarget.SetPOIState(POI_STATE.ACTIVE);
        goapNode.actor.needsComponent.AdjustDoNotGetHungry(-1);
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 80) {
            Lethargic lethargic = new Lethargic();
            goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, lethargic, goapNode.actor, goapNode);
        } else {
            Vampiric vampiric = new Vampiric();
            goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, vampiric, goapNode.actor);
            Log log = new Log(GameManager.Instance.Today(), "GoapAction", goapName, "contracted", goapNode);
            log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotificationFrom(goapNode.actor, log);
        }
    }
    #endregion

    //#region Intel Reactions
    //private List<string> DrinkBloodSuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;
    //    bool isRecipientVampire = recipient.traitContainer.GetNormalTrait<Trait>("Vampiric") != null;

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
    //                reactions.Add("I know what I did.");
    //            }
    //            //- Recipient is Target
    //            else if (recipient == targetCharacter) {
    //                if (isRecipientVampire) {
    //                    //- Recipient is a Vampire
    //                    reactions.Add(string.Format("{0} must be the one that turned me into this...", actor.name));
    //                } else {
    //                    //- Recipient is NOT a Vampire
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = false;
    //                    if (!hasCrimeBeenReported) {
    //                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    }
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                        } else {
    //                            reactions.Add(string.Format("I don't believe you! {0} is not a vampire.", actor.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("I knew something is off with that {0}!", actor.name));
    //                    } else {
    //                        reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                    }
    //                }
    //            }
    //            //- Recipient Has Positive Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                if (isRecipientVampire) {
    //                    //- Recipient is a Vampire
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs but {1} shouldn't have hurt {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
    //                            if(status != SHARE_INTEL_STATUS.WITNESSED) {
    //                                recipient.CreateKnockoutJob(actor);
    //                            }
    //                        } else {
    //                            reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs but {1} shouldn't have hurt {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
    //                        if (status != SHARE_INTEL_STATUS.WITNESSED) {
    //                            recipient.CreateKnockoutJob(actor);
    //                        }
    //                    } else {
    //                        reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs but {1} shouldn't have hurt {2}!", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false), targetCharacter.name));
    //                        if (status != SHARE_INTEL_STATUS.WITNESSED) {
    //                            recipient.CreateKnockoutJob(actor);
    //                        }
    //                    }
    //                } else {
    //                    //- Recipient is NOT a Vampire
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = false;
    //                    if (!hasCrimeBeenReported) {
    //                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    }
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                        } else {
    //                            reactions.Add(string.Format("I don't believe you! {0} is not a vampire.", actor.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("I knew something is off with that {0}!", actor.name));
    //                    } else {
    //                        reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                    }
    //                }
    //            }
    //            //- Recipient Has Negative Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                if (isRecipientVampire) {
    //                    //- Recipient is a Vampire
    //                    if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        if (recipient.marker.inVisionCharacters.Contains(actor)) {
    //                            recipient.combatComponent.AddAvoidInRange(actor, reason: "saw something shameful");
    //                        }
    //                    }
    //                    RELATIONSHIP_EFFECT relationshipWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
    //                    } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("I may dislike {0} but I can't report a fellow vampire.", actor.name));
    //                    } else {
    //                        reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
    //                    }
    //                } else {
    //                    //- Recipient is NOT a Vampire
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = false;
    //                    if (!hasCrimeBeenReported) {
    //                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    }
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                        } else {
    //                            reactions.Add(string.Format("I don't believe you! {0} is not a vampire.", actor.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                    } else {
    //                        reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                    }
    //                }
    //            }
    //            //- Recipient Has No Relationship with Target
    //            else {
    //                if (isRecipientVampire) {
    //                    //- Recipient is a Vampire
    //                    RELATIONSHIP_EFFECT relationshipWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
    //                    } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("I may dislike {0} but I can't report a fellow vampire.", actor.name));
    //                    } else {
    //                        reactions.Add(string.Format("I am also a vampire so I understand {0}'s unique needs.", actor.name));
    //                    }
    //                } else {
    //                    //- Recipient is NOT a Vampire
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = false;
    //                    if (!hasCrimeBeenReported) {
    //                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    }
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                        } else {
    //                            reactions.Add(string.Format("I don't believe you! {0} is not a vampire.", actor.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                    } else {
    //                        reactions.Add("Vampires are cursed beings that must be destroyed!");
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class DrinkBloodData : GoapActionData {
    public DrinkBloodData() : base(INTERACTION_TYPE.DRINK_BLOOD) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation == null || (actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure)) {
            return false;
        }
        if (actor != poiTarget) {
            return true;
        }
        return false;
    }
}
