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
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        //racesThatCanDoAction = new RACE[] {
        //    RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
        //    RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON, RACE.MIMIC, RACE.PIG, RACE.SHEEP, RACE.ENT, RACE.WISP,
        //    RACE.GHOST, RACE.NYMPH, RACE.SLIME, RACE.SLUDGE, RACE.CHICKEN, RACE.ELEMENTAL, RACE.ABOMINATION, RACE.ANGEL, RACE.DEMON, RACE.REVENANT
        //};
        isNotificationAnIntel = true;
        doesNotStopTargetCharacter = true;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.STARTS_COMBAT, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (actor.IsHealthCriticallyLow()) {
                //only block assault action if character is not berserked
                if (actor.traitContainer.HasTrait("Berserked") == false) {
                    goapActionInvalidity.isInvalid = true;    
                }
            }
        }
        return goapActionInvalidity;
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
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        if (status == REACTION_STATUS.INFORMED) {
            if (actor.faction != null && actor.faction.isMajorNonPlayer && !actor.IsHostileWith(witness)) {
                if (target is Character targetCharacter) {
                    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                    if (node.associatedJobType == JOB_TYPE.APPREHEND) {
                        bool targetHasHeinousOrSeriousCrime = false;
                        if (targetCharacter.traitContainer.HasTrait("Criminal")) {
                            targetHasHeinousOrSeriousCrime = targetCharacter.traitContainer.GetNormalTrait<Criminal>("Criminal").HasCrime(CRIME_SEVERITY.Serious, CRIME_SEVERITY.Heinous);
                            //targetHasHeinousOrSeriousCrime = crimeData.crimeSeverity == CRIME_SEVERITY.Serious || crimeData.crimeSeverity == CRIME_SEVERITY.Heinous;
                        }
                        if (targetHasHeinousOrSeriousCrime) {
                            if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, actor, status, node);
                            } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                && opinionLabel != RelationshipManager.Rival) {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, actor, status, node);
                            } else {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
                            }
                        } else {
                            if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                            } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                && opinionLabel != RelationshipManager.Rival) {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                            } else {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
                            }
                        }
                    } else {
                        if (opinionLabel == RelationshipManager.Acquaintance) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                        } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                        } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                && opinionLabel != RelationshipManager.Rival) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                        } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
                        } else if (!targetCharacter.isNormalCharacter) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disinterest, witness, actor, status, node);
                        } 
                        //else {
                        //    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
                        //}
                    }
                    if (node.associatedJobType != JOB_TYPE.APPREHEND && !actor.IsHostileWith(targetCharacter)) {
                        CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);
                    }
                } else if (target is TileObject targetTileObject) {
                    if (node.associatedJobType != JOB_TYPE.APPREHEND) {
                        if (targetTileObject.IsOwnedBy(witness)) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, actor, status, node);
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                        } else if (targetTileObject.tileObjectType == TILE_OBJECT_TYPE.TOMBSTONE) { //TODO: Human Meat, Elven Meat
                            Character characterRef = null;
                            if(targetTileObject is Tombstone tombstone) {
                                characterRef = tombstone.character;
                            }
                            string refOpinionLabel = witness.relationshipContainer.GetOpinionLabel(characterRef);
                            if (refOpinionLabel == RelationshipManager.Acquaintance) {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, actor, status, node);
                            } else if (refOpinionLabel == RelationshipManager.Friend || refOpinionLabel == RelationshipManager.Close_Friend) {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, actor, status, node);
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, witness, actor, status, node);
                            } else if ((witness.relationshipContainer.IsFamilyMember(characterRef) || witness.relationshipContainer.HasRelationshipWith(characterRef, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                    && refOpinionLabel != RelationshipManager.Rival) {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Resentment, witness, actor, status, node);
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, witness, actor, status, node);
                            } else if (refOpinionLabel == RelationshipManager.Enemy || refOpinionLabel == RelationshipManager.Rival) {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                            }
                        } else {
                            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
                            if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status, node);
                            } else if ((witness.relationshipContainer.IsFamilyMember(actor) || witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                    && opinionLabel != RelationshipManager.Rival) {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, actor, status, node);
                            } else {
                                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                            }
                        }
                    }
                }
            }
        }
        return response;
    }
    public override string ReactionToTarget(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToTarget(actor, target, witness, node, status);
        if(status == REACTION_STATUS.INFORMED) {
            if (target is Character targetCharacter && targetCharacter.faction != null && targetCharacter.faction.isMajorNonPlayer && !witness.IsHostileWith(targetCharacter)) {
                if (node.associatedJobType == JOB_TYPE.APPREHEND) {
                    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                    bool targetHasHeinousOrSeriousCrime = false;
                    bool targetHasMisdemeanour = false;
                    if (targetCharacter.traitContainer.HasTrait("Criminal")) {
                        Criminal criminalTrait = targetCharacter.traitContainer.GetNormalTrait<Criminal>("Criminal");
                        targetHasHeinousOrSeriousCrime = criminalTrait.HasCrime(CRIME_SEVERITY.Serious, CRIME_SEVERITY.Heinous);
                        targetHasMisdemeanour = criminalTrait.HasCrime(CRIME_SEVERITY.Misdemeanor);

                        //CrimeData crimeData = targetCharacter.traitContainer.GetNormalTrait<Criminal>("Criminal").dataCrime;
                        //targetHasHeinousOrSeriousCrime = crimeData.crimeSeverity == CRIME_SEVERITY.Serious || crimeData.crimeSeverity == CRIME_SEVERITY.Heinous;
                        //targetHasMisdemeanour = crimeData.crimeSeverity == CRIME_SEVERITY.Misdemeanor;
                    }
                    if (targetHasHeinousOrSeriousCrime) {
                        if (opinionLabel == RelationshipManager.Acquaintance) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, targetCharacter, status, node);
                        } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, targetCharacter, status, node);
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, targetCharacter, status, node);
                        } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                       && opinionLabel != RelationshipManager.Rival) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, targetCharacter, status, node);
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, targetCharacter, status, node);
                        } else {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, targetCharacter, status, node);
                        }
                    } else if (targetHasMisdemeanour) {
                        if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, targetCharacter, status, node);
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, targetCharacter, status, node);
                        } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                       && opinionLabel != RelationshipManager.Rival) {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, targetCharacter, status, node);
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, targetCharacter, status, node);
                        } else {
                            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, targetCharacter, status, node);
                        }
                    }
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, targetCharacter, status, node);
                }
            }
        }
        return response;
    }
    public override void OnStoppedInterrupt(ActualGoapNode node) {
        base.OnStoppedInterrupt(node);
        node.actor.combatComponent.RemoveHostileInRange(node.poiTarget);
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.associatedJobType == JOB_TYPE.APPREHEND || node.associatedJobType == JOB_TYPE.KNOCKOUT) {
            return REACTABLE_EFFECT.Neutral;
        } else {
            return REACTABLE_EFFECT.Negative;
        }
    }
    public override bool IsInvalidOnVision(ActualGoapNode node) {
        return false;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if(crime.associatedJobType != JOB_TYPE.APPREHEND) {
            return CRIME_TYPE.Assault;
        }
        return base.GetCrimeType(actor, target, crime);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget is TileObject tileObject) {
                return tileObject.gridTileLocation != null && !actor.IsHealthCriticallyLow();
            }
            return !actor.IsHealthCriticallyLow();
        }
        return false;
    }
    #endregion

    #region Effects
    public void PreCombatStart(ActualGoapNode goapNode) {
        Debug.Log($"{goapNode.actor} will start combat towards {goapNode.poiTarget.name}");
        string combatReason = CombatManager.Action;
        bool isLethal = goapNode.associatedJobType.IsJobLethal();
        if(goapNode.associatedJobType == JOB_TYPE.DEMON_KILL) {
            combatReason = CombatManager.Demon_Kill;
        }
        //goapNode.actor.combatComponent.SetActionAndJobThatTriggeredCombat(goapNode, goapNode.actor.currentJob as GoapPlanJob);
        goapNode.actor.combatComponent.Fight(goapNode.poiTarget, combatReason, connectedAction: goapNode, isLethal: isLethal);

        string key = goapNode.actor.combatComponent.GetCombatLogKeyReason(goapNode.poiTarget);
        JOB_TYPE jobType = goapNode.associatedJobType;
        if(LocalizationManager.Instance.HasLocalizedValue("Character", "Combat", key)) {
            string reason = LocalizationManager.Instance.GetLocalizedValue("Character", "Combat", key);
            goapNode.descriptionLog.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
        } else {
            goapNode.descriptionLog.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(jobType.ToString()) + ".", LOG_IDENTIFIER.STRING_1);
        }
        // if(goapNode.poiTarget is Character) {
        //     Character targetCharacter = goapNode.poiTarget as Character;
        //     if (goapNode.associatedJobType != JOB_TYPE.APPREHEND && !goapNode.actor.IsHostileWith(targetCharacter)) {
        //         CrimeManager.Instance.ReactToCrime(targetCharacter, goapNode.actor, goapNode, goapNode.associatedJobType, CRIME_TYPE.MISDEMEANOR);
        //     }
        // }
    }
    #endregion
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