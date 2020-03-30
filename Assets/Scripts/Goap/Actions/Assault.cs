using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

//will be branded criminal if anybody witnesses or after combat
public class Assault : GoapAction {

    //private Character winner;
    private Character loser;

    public Assault() : base(INTERACTION_TYPE.ASSAULT) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON, RACE.SPIDER };
        isNotificationAnIntel = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.STARTS_COMBAT, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode actionNode) {
        base.Perform(actionNode);
        SetState("Combat Start", actionNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +50(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 50;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (!witness.IsHostileWith(actor)) {
            if (target is Character) {
                Character targetCharacter = target as Character;
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == OpinionComponent.Enemy || opinionLabel == OpinionComponent.Rival) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status);
                } else if (node.associatedJobType != JOB_TYPE.APPREHEND) {
                    if (opinionLabel == OpinionComponent.Acquaintance) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
                    } else if (opinionLabel == OpinionComponent.Friend || opinionLabel == OpinionComponent.Close_Friend) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status);
                    }
                }
                if (node.associatedJobType != JOB_TYPE.APPREHEND && !actor.IsHostileWith(targetCharacter)) {
                    CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.MISDEMEANOR);
                }
            }
        }
        return response;
    }
    public override string ReactionToTarget(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToTarget(witness, node, status);
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        if (node.associatedJobType == JOB_TYPE.APPREHEND) {
            Character targetCharacter = target as Character;
            string opinionLabel = witness.opinionComponent.GetOpinionLabel(targetCharacter);
            if (opinionLabel == OpinionComponent.Acquaintance) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, targetCharacter, status);
            } else if (opinionLabel == OpinionComponent.Friend || opinionLabel == OpinionComponent.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, targetCharacter, status);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, targetCharacter, status);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, targetCharacter, status);
            }
        }
        return response;
    }
    #endregion

    #region Effects
    public void AfterCombatStart(ActualGoapNode goapNode) {
        Debug.Log($"{goapNode.actor} will start combat towards {goapNode.poiTarget.name}");
        bool isLethal = goapNode.associatedJobType.IsJobLethal();
        goapNode.actor.combatComponent.Fight(goapNode.poiTarget, isLethal: isLethal);
        // if(goapNode.poiTarget is Character) {
        //     Character targetCharacter = goapNode.poiTarget as Character;
        //     if (goapNode.associatedJobType != JOB_TYPE.APPREHEND && !goapNode.actor.IsHostileWith(targetCharacter)) {
        //         CrimeManager.Instance.ReactToCrime(targetCharacter, goapNode.actor, goapNode, goapNode.associatedJobType, CRIME_TYPE.MISDEMEANOR);
        //     }
        // }
    }
    #endregion

    //#region Overrides
    ////protected override void ConstructRequirement() {
    ////    _requirementAction = Requirement;
    ////}
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_VISION, targetPOI = poiTarget }, IsInVision);
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", targetPOI = poiTarget });
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget });
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = poiTarget });
    //}
    ////protected override void MoveToDoAction(Character targetCharacter) {
    ////    base.MoveToDoAction(targetCharacter);
    ////    //change end reached distance to the characters attack range. NOTE: Make sure to return the range to default after this action is done.
    ////    actor.marker.pathfindingAI.SetEndReachedDistance(actor.characterClass.attackRange);
    ////    //isPerformingActualAction = true;
    ////    //actorAlterEgo = actor.currentAlterEgo;
    ////    //AddAwareCharacter(actor);

    ////    //bool isLethal = true;
    ////    //if (parentPlan != null && parentPlan.job != null && (parentPlan.job.jobType == JOB_TYPE.UNDERMINE_ENEMY || parentPlan.job.jobType == JOB_TYPE.APPREHEND || parentPlan.job.targetInteractionType == INTERACTION_TYPE.DRINK_BLOOD)) {
    ////    //    //Assaulting characters for imprisonment of criminals and undermining enemies must be non lethal
    ////    //    isLethal = false;
    ////    //}
    ////    //if (actor.combatComponent.AddHostileInRange(targetCharacter, false, isLethal: isLethal)) {
    ////    //    Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState); //set this to signal because adding a character as hostile, is no longer sure to return a new CharacterState
    ////    //    SetState("In Progress");
    ////    //} else {
    ////    //    Debug.LogWarning(GameManager.Instance.TodayLogString() + actor.name + " did was unable to add target as hostile when reacting to " + poiTarget.name + " in assault action!");
    ////    //    SetState("Target Missing");
    ////    //}

    ////}
    //public override bool ShouldBeStoppedWhenSwitchingStates() {
    //    return false;
    //}
    //public override void Perform(ActualGoapNode goapNode) {
    //    base.Perform(goapNode);
    //    SetCannotCancelAction(true);
    //    //actor.marker.pathfindingAI.ResetEndReachedDistance();

    //    Character targetCharacter = poiTarget as Character;
    //    if (targetCharacter.specificLocation == actor.specificLocation && !targetCharacter.currentParty.icon.isTravellingOutside) {
    //        if (actor.IsCombatReady()) {
    //            CharacterState characterState;
    //            if (!actor.combatComponent.hostilesInRange.Contains(targetCharacter)) {
    //                bool isLethal = true;
    //                if (parentPlan != null && parentPlan.job != null && (parentPlan.job.jobType == JOB_TYPE.UNDERMINE_ENEMY || parentPlan.job.jobType == JOB_TYPE.APPREHEND || parentPlan.job.targetInteractionType == INTERACTION_TYPE.DRINK_BLOOD)) {
    //                    //Assaulting characters for imprisonment of criminals and undermining enemies must be non lethal
    //                    isLethal = false;
    //                }
    //                if (actor.combatComponent.AddHostileInRange(targetCharacter, out characterState, false, isLethal: isLethal)) {
    //                    Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState); //set this to signal because adding a character as hostile, is no longer sure to return a new CharacterState
    //                    SetState("In Progress");
    //                } else {
    //                    Debug.LogWarning(GameManager.Instance.TodayLogString() + actor.name + " did was unable to add target as hostile when reacting to " + poiTarget.name + " in assault action!");
    //                    SetState("Target Missing");
    //                }
    //            } else if (actor.stateComponent.currentState is CombatState) {
    //                characterState = actor.stateComponent.currentState as CombatState; //target character is already in the actor's hostile range so I assume that the actor is in combat state
    //                CombatState combatState = characterState as CombatState;
    //                combatState.SetActionThatTriggeredThisState(this);
    //                combatState.SetEndStateAction(OnFinishCombatState);
    //                SetState("In Progress");
    //            } else {
    //                Debug.LogWarning(actor.name + " is not in combat state, but it has " + targetCharacter.name + "in it's hostile range");
    //                SetState("Assault Failed");
    //            }
    //            //if (characterState is CombatState) {
    //            //    CombatState realCombatState = characterState as CombatState;
    //            //    realCombatState.SetActionThatTriggeredThisState(this);
    //            //    realCombatState.SetOnEndStateAction(OnFinishCombatState);
    //            //    SetState("In Progress");
    //            //} else {
    //            //    Debug.LogWarning(GameManager.Instance.TodayLogString() + actor.name + " did not return a combat state when reacting to " + poiTarget.name + " in assault action!");
    //            //    SetState("Target Missing");
    //            //}
    //        } else {
    //            SetState("Assault Failed");
    //        }
    //    } else {
    //        SetState("Target Missing");
    //    }
    //}
    //private void OnCharacterStartedState(Character character, CharacterState state) {
    //    if (character == actor) {
    //        CombatState combatState = state as CombatState;
    //        combatState.SetActionThatTriggeredThisState(this);
    //        combatState.SetEndStateAction(OnFinishCombatState);
    //        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
    //    }
    //}
    //protected override void OnCancelActionTowardsTarget() {
    //    //actor.marker.pathfindingAI.ResetEndReachedDistance();
    //    Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
    //    base.OnCancelActionTowardsTarget();
    //}
    //public override void OnStopWhileStarted() {
    //    //actor.marker.pathfindingAI.ResetEndReachedDistance();
    //    Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
    //    base.OnStopWhileStarted();
    //}
    //private void OnFinishCombatState() {
    //    //Debug.Log(actor.name + " finished combat state!");
    //    Character target = poiTarget as Character;
    //    loser = target;
    //    //winner = actor;
    //    if (target.traitContainer.GetNormalTrait<Trait>("Dead") != null) {
    //        SetState("Target Killed");
    //    } else if (target.traitContainer.GetNormalTrait<Trait>("Unconscious") != null) {
    //        SetState("Target Knocked Out");
    //    } else if (target.traitContainer.GetNormalTrait<Trait>("Injured") != null) {
    //        SetState("Target Injured");
    //    } else if (actor.specificLocation != target.specificLocation) {
    //        SetState("Target Missing");
    //    } else {
    //        loser = actor;
    //        //winner = target;
    //        SetState("Assault Failed");
    //    }
    //}
    //protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
    //    return 1;
    //}
    ////public override void FailAction() {
    ////    base.FailAction();
    ////    SetState("Target Missing");
    ////}
    //public override void DoAction() {
    //    SetTargetStructure();
    //    base.DoAction();
    //}
    //public override LocationGridTile GetTargetLocationTile() {
    //    return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    //}
    //public override void OnStopWhilePerforming() {
    //    //actor.marker.pathfindingAI.ResetEndReachedDistance();
    //    Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
    //}
    ////public override void OnResultReturnedToActor() {
    ////    //actor.marker.pathfindingAI.ResetEndReachedDistance();
    ////    Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
    ////}
    //public override int GetArrangedLogPriorityIndex(string priorityID) {
    //    if(priorityID == "description") {
    //        return 0;
    //    }else if (priorityID == "injured") {
    //        return 1;
    //    } else if (priorityID == "unconscious") {
    //        return 2;
    //    }
    //    return base.GetArrangedLogPriorityIndex(priorityID);
    //}
    //#endregion

    //#region Preconditions
    //protected bool IsInVision() {
    //    return actor.marker.inVisionPOIs.Contains(poiTarget);
    //}
    //#endregion

    //#region State Effects
    //public void PreTargetInjured() {
    //    //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is an Assault crime
    //    if (!actor.IsHostileWith(poiTarget as Character)
    //       //Assaulting a criminal as part of apprehending him should not be considered a crime
    //       && !IsFromApprehendJob()) {
    //        SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
    //    }
    //    goapNode.descriptionLog.AddToFillers(loser, loser.name, LOG_IDENTIFIER.CHARACTER_3);
    //    //AddTraitTo(winner, "Combat Recovery", loser);
    //    //Injured injured = new Injured();
    //    //AddTraitTo(loser, injured, winner);
    //    RelationshipManager.Instance.RelationshipDegradation(actor, poiTarget as Character, this);
    //    currentState.SetIntelReaction(TargetInjuredKnockOutReactions);
    //}
    //public void AfterTargetInjured() {
    //    //moved this to pre effect, because if put here, will cause infinite loop:
    //    // - loser will gain injured trait
    //    // - if the loser is the actor of this action
    //    // - will switch to flee state
    //    // - which will then stop the current action which is this, but still perform the after action
    //    // - so this loop will never end.

    //    //Injured injured = new Injured();
    //    //AddTraitTo(loser, injured, winner);
    //}
    //public void PreTargetKnockedOut() {
    //    //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is an Assault crime
    //    if (!actor.IsHostileWith(poiTarget as Character)
    //        //Assaulting a criminal as part of apprehending him should not be considered a crime
    //        && !IsFromApprehendJob()) {
    //        SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
    //    }
    //    goapNode.descriptionLog.AddToFillers(loser, loser.name, LOG_IDENTIFIER.CHARACTER_3);
    //    //AddTraitTo(winner, "Combat Recovery", loser);
    //    RelationshipManager.Instance.RelationshipDegradation(actor, poiTarget as Character, this);
    //    currentState.SetIntelReaction(TargetInjuredKnockOutReactions);
    //}
    //public void AfterTargetKnockedOut() {
    //    if(parentPlan.job != null) {
    //        parentPlan.job.SetCannotCancelJob(true);
    //    }
    //    resumeTargetCharacterState = false; //do not resume the target character's current state after being knocked out
    //    //Character target = poiTarget as Character;
    //    //Unconscious unconscious = new Unconscious();
    //    //AddTraitTo(loser, unconscious, winner);
    //}
    //public void PreTargetKilled() {
    //    //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is a Murder crime
    //    if (!actor.IsHostileWith(poiTarget as Character)
    //        //Assaulting a criminal as part of apprehending him should not be considered a crime
    //        && !IsFromApprehendJob()) {
    //        SetCommittedCrime(CRIME.MURDER, new Character[] { actor });
    //    }
    //    goapNode.descriptionLog.AddToFillers(loser, loser.name, LOG_IDENTIFIER.CHARACTER_3);
    //    //AddTraitTo(winner, "Combat Recovery", loser);
    //    currentState.SetIntelReaction(TargetKilledReactions);
    //}
    //public void AfterTargetKilled() {
    //    if (parentPlan.job != null) {
    //        parentPlan.job.SetCannotCancelJob(true);
    //    }
    //    SetCannotCancelAction(true);
    //    //loser.Death(deathFromAction: this);
    //}
    //public void PreAssaultFailed() {
    //    //**Note**: If the actor is from the same faction as the witness and the target is not considered hostile, this is an Assault crime
    //    if (!actor.IsHostileWith(poiTarget as Character)
    //        //Assaulting a criminal as part of apprehending him should not be considered a crime
    //        && !IsFromApprehendJob()) {
    //        SetCommittedCrime(CRIME.ASSAULT, new Character[] { actor });
    //    }
    //}
    ////public void PreTargetMissing() {
    ////    goapNode.descriptionLog.AddToFillers(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    ////}
    //#endregion

    //#region Combat
    //private string CombatEncounterEvents(Character actor, Character target, bool actorWon) {
    //    //Reference: https://trello.com/c/uY7JokJn/1573-combat-encounter-event

    //    if (actorWon) {
    //        //winner = actor;
    //        loser = target;
    //    } else {
    //        //winner = target;
    //        loser = actor;
    //    }

    //    //**Character That Lost**
    //    //40 Weight: Gain Unconscious trait (reduce to 0 if already Unconscious)
    //    //10 Weight: Gain Injured trait and enter Flee mode (reduce to 0 if already Injured)
    //    //5 Weight: death
    //    WeightedDictionary<string> loserResults = new WeightedDictionary<string>();
    //    if (loser.traitContainer.GetNormalTrait<Trait>("Unconscious") == null) {
    //        loserResults.AddElement("Unconscious", 40);
    //    }
    //    if (loser.traitContainer.GetNormalTrait<Trait>("Injured") == null) {
    //        loserResults.AddElement("Injured", 10);
    //    }
    //    loserResults.AddElement("Death", 5);

    //    string result = loserResults.PickRandomElementGivenWeights();
    //    switch (result) {
    //        case "Unconscious":
    //            return "Target Knocked Out";
    //        case "Injured":
    //            return "Target Injured";
    //        case "Death":
    //            return "Target Killed";
    //    }
    //    throw new System.Exception("No state for result " + result);
    //}
    //#endregion

    //#region Intel Reactions
    //private List<string> TargetInjuredKnockOutReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;

    //    if (isOldNews) {
    //        //Old News
    //        reactions.Add("This is old news.");
    //    } else {
    //        CHARACTER_MOOD recipientMood = recipient.currentMoodType;
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
    //                if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo)) {
    //                    reactions.Add("Please don't remind me of that altercation.");
    //                    AddTraitTo(recipient, "Annoyed");
    //                } else if (recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    //- Has Negative Relationship
    //                    reactions.Add(string.Format("Now that you've reminded me about that, I think I should get back at {0}.", actor.name));
    //                    recipient.CreateUndermineJobOnly(actor, "informed", status);
    //                } else if (recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    //- Has Positive Relationship
    //                    if (recipientMood == CHARACTER_MOOD.BAD || recipientMood == CHARACTER_MOOD.DARK) {
    //                        //- No Relationship (Negative Mood)
    //                        if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                            reactions.Add(string.Format("Now that you've reminded me about that, I think I really should start avoiding {0}!", actor.name));
    //                        } else {
    //                            reactions.Add("Past is past.");
    //                        }
    //                    } else {
    //                        //- No Relationship (Positive Mood)
    //                        reactions.Add("Past is past.");
    //                    }
    //                }
    //            }
    //            //- Recipient Has Positive Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                if (committedCrime == CRIME.NONE) {
    //                    reactions.Add(string.Format("There's a reason {0} did that.", actor.name));
    //                } else {
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = false;
    //                    if (!hasCrimeBeenReported) {
    //                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    }
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add(string.Format("I may have been fond of {0} but I can't allow such violence!", actor.name));
    //                        } else {
    //                            reactions.Add(string.Format("{0} assaulted {1}? I don't believe that.", actor.name, targetCharacter.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("{0} assaulted {1}? Why am I not surprised?", actor.name, targetCharacter.name));
    //                    } else {
    //                        reactions.Add(string.Format("Poor {1}. {0} must pay.", actor.name, targetCharacter.name));
    //                    }
    //                }
    //            }
    //            //- Recipient Has Negative Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    reactions.Add(string.Format("{0} deserves to be beaten.", targetCharacter.name));
    //                    AddTraitTo(recipient, "Satisfied");
    //                    //if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    //    if (recipient.marker.inVisionPOIs.Contains(targetCharacter)) {
    //                    //        recipient.combatComponent.AddHostileInRange(targetCharacter, checkHostility: false);
    //                    //    }
    //                    //}
    //                } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    reactions.Add("Those misfits are always up to no good.");
    //                    //if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    //    if (recipient.marker.inVisionPOIs.Contains(actor)) {
    //                    //        recipient.combatComponent.AddAvoidInRange(actor);
    //                    //    }
    //                    //}
    //                } else {
    //                    reactions.Add(string.Format("{0} deserves to be beaten.", targetCharacter.name));
    //                    AddTraitTo(recipient, "Satisfied");
    //                }
    //            }
    //            //- Recipient Has No Relationship with Target
    //            else {
    //                if (committedCrime == CRIME.NONE) {
    //                    reactions.Add(string.Format("There's a reason {0} did that.", actor.name));
    //                } else {
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = false;
    //                    if (!hasCrimeBeenReported) {
    //                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    }
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add(string.Format("I may have been fond of {0} but I can't allow such violence!", actor.name));
    //                        } else {
    //                            reactions.Add(string.Format("{0} assaulted {1}? I don't believe that.", actor.name, targetCharacter.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("{0} assaulted {1}? Why am I not surprised?", actor.name, targetCharacter.name));
    //                    } else {
    //                        reactions.Add(string.Format("Poor {1}. {0} must pay.", actor.name, targetCharacter.name));
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //private List<string> TargetKilledReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;

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
    //                reactions.Add("That altercation ended my past life.");
    //                AddTraitTo(recipient, "Heartbroken");
    //            }
    //            //- Recipient Has Positive Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                if (committedCrime == CRIME.NONE) {
    //                    reactions.Add(string.Format("There's a reason {0} did that.", actor.name));
    //                } else {
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = false;
    //                    if (!hasCrimeBeenReported) {
    //                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    }
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add(string.Format("I may have been fond of {0} but I can't condone murder!", actor.name));
    //                        } else {
    //                            reactions.Add(string.Format("{0} killed {1}? I don't believe that.", actor.name, targetCharacter.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("{0} murdered {1}? Why am I not surprised?", actor.name, targetCharacter.name));
    //                    } else {
    //                        reactions.Add(string.Format("{1} was killed? {0} must pay.", actor.name, targetCharacter.name));
    //                    }
    //                }
    //            }
    //            //- Recipient Has Negative Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                RELATIONSHIP_EFFECT relationshipWithActor = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                if (relationshipWithActor == RELATIONSHIP_EFFECT.POSITIVE) {
    //                    reactions.Add(string.Format("Suits {0} right.", Utilities.GetPronounString(targetCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //                    AddTraitTo(recipient, "Satisfied");
    //                    //if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    //    if (recipient.marker.inVisionPOIs.Contains(actor)) {
    //                    //        recipient.combatComponent.AddAvoidInRange(actor);
    //                    //    }
    //                    //}
    //                } else if (relationshipWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                    reactions.Add("My enemies are killing each other. Isn't that funny?");
    //                    //if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                    //    if (recipient.marker.inVisionPOIs.Contains(actor)) {
    //                    //        recipient.combatComponent.AddAvoidInRange(actor);
    //                    //    }
    //                    //}
    //                } else {
    //                    if (RelationshipManager.Instance.RelationshipImprovement(actor, recipient, this)) {
    //                        reactions.Add(string.Format("I am happy {0} was able to do what I cannot.", actor.name));
    //                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        //    if (recipient.marker.inVisionPOIs.Contains(actor)) {
    //                        //        recipient.combatComponent.AddAvoidInRange(actor);
    //                        //    }
    //                        //}
    //                    } else {
    //                        reactions.Add(string.Format("Though I dislike {1}, I am appalled at {0}'s actions.", actor.name, targetCharacter.name));
    //                        if (!hasCrimeBeenReported) {
    //                            recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                        }
    //                        //if (status == SHARE_INTEL_STATUS.WITNESSED) {
    //                        //    if (recipient.marker.inVisionPOIs.Contains(actor)) {
    //                        //        recipient.combatComponent.AddAvoidInRange(actor);
    //                        //    }
    //                        //}
    //                    }
    //                    AddTraitTo(recipient, "Satisfied");
    //                }
    //            }
    //            //- Recipient Has No Relationship with Target
    //            else {
    //                if (committedCrime == CRIME.NONE) {
    //                    reactions.Add(string.Format("There's a reason {0} did that.", actor.name));
    //                } else {
    //                    RELATIONSHIP_EFFECT relationshipWithActorBeforeDegradation = recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo);
    //                    bool hasRelationshipDegraded = false;
    //                    if (!hasCrimeBeenReported) {
    //                        hasRelationshipDegraded = recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //                    }
    //                    if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.POSITIVE) {
    //                        if (hasRelationshipDegraded) {
    //                            reactions.Add(string.Format("I may be fond of {0} but I can't condone murder!", actor.name));
    //                        } else {
    //                            reactions.Add(string.Format("{0} killed {1}? I don't believe that.", actor.name, targetCharacter.name));
    //                        }
    //                    } else if (relationshipWithActorBeforeDegradation == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                        reactions.Add(string.Format("{0} is truly wicked.", actor.name));
    //                    } else {
    //                        reactions.Add(string.Format("Poor {1}. {0} must pay.", actor.name, targetCharacter.name));
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class AssaultData : GoapActionData {
    public AssaultData() : base(INTERACTION_TYPE.ASSAULT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget is Character && actor != poiTarget) {
            Character target = poiTarget as Character;
            if (target.canPerform) { //!target.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
                return true;
            }
        }
        return false;
    }
}