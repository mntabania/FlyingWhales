using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using UtilityScripts;

//will be branded criminal if anybody witnesses or after combat
public class Assault : GoapAction {

    //private Character winner;
    //private Character loser;

    public Assault() : base(INTERACTION_TYPE.ASSAULT) {
        actionLocationType = ACTION_LOCATION_TYPE.TARGET_IN_VISION;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        //racesThatCanDoAction = new RACE[] {
        //    RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
        //    RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON, RACE.MIMIC, RACE.PIG, RACE.SHEEP, RACE.ENT, RACE.WISP,
        //    RACE.GHOST, RACE.NYMPH, RACE.SLIME, RACE.SLUDGE, RACE.CHICKEN, RACE.ELEMENTAL, RACE.ABOMINATION, RACE.ANGEL, RACE.DEMON, RACE.REVENANT
        //};
        doesNotStopTargetCharacter = true;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        logTags = new[] {LOG_TAG.Combat};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.STARTS_COMBAT, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
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
                    goapActionInvalidity.reason = "low_health";
                }
            }
        }
        return goapActionInvalidity;
    }
    public override void Perform(ActualGoapNode actionNode) {
        base.Perform(actionNode);
        SetState("Combat Start", actionNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +50(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 50;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (node.crimeType == CRIME_TYPE.Vampire) {
            if (target is Character targetCharacter) {
                string opinionOfTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, node.crimeType);
                if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                    if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                        reactions.Add(EMOTION.Rage);
                        reactions.Add(EMOTION.Threatened);
                    } else if (opinionOfTarget == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Disapproval);
                        reactions.Add(EMOTION.Anger);
                        reactions.Add(EMOTION.Threatened);
                    } else if (actor.traitContainer.HasTrait("Cultist") && witness.traitContainer.HasTrait("Cultist")) {
                        reactions.Add(EMOTION.Approval);
                        if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                            int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                            if (GameUtilities.RollChance(chance)) {
                                reactions.Add(EMOTION.Arousal);
                            }
                        }
                    } else if (opinionOfTarget == RelationshipManager.Friend || opinionOfTarget == RelationshipManager.Acquaintance) {
                        reactions.Add(EMOTION.Disapproval);
                        reactions.Add(EMOTION.Threatened);
                    } else if (targetCharacter == witness) {
                        if (GameUtilities.RollChance(50)) {
                            reactions.Add(EMOTION.Anger);
                        } else {
                            reactions.Add(EMOTION.Resentment);
                        }
                    }
                } else {
                    if (witness.traitContainer.HasTrait("Hemophiliac")) {
                        if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                            reactions.Add(EMOTION.Arousal);
                        } else {
                            reactions.Add(EMOTION.Approval);
                        }
                    } else if (witness.traitContainer.HasTrait("Hemophobic")) {
                        reactions.Add(EMOTION.Threatened);
                    }
                }
            }
        } else if (actor.faction != null && actor.faction.isMajorNonPlayer && !actor.IsHostileWith(witness)) {
            if (target is Character targetCharacter) {
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (node.associatedJobType == JOB_TYPE.APPREHEND) {
                    bool targetHasHeinousOrSeriousCrime = targetCharacter.crimeComponent.HasCrime(CRIME_SEVERITY.Serious, CRIME_SEVERITY.Heinous);

                    if (targetHasHeinousOrSeriousCrime) {
                        if (opinionLabel == RelationshipManager.Close_Friend) {
                            reactions.Add(EMOTION.Resentment);
                        } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                            reactions.Add(EMOTION.Resentment);
                        } else if (actor.traitContainer.HasTrait("Cultist") && witness.traitContainer.HasTrait("Cultist")) {
                            reactions.Add(EMOTION.Approval);
                            if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                                int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                                if (GameUtilities.RollChance(chance)) {
                                    reactions.Add(EMOTION.Arousal);
                                }
                            }
                        } else if (opinionLabel == RelationshipManager.Friend) {
                            reactions.Add(EMOTION.Resentment);
                        } else {
                            reactions.Add(EMOTION.Approval);
                        }
                    } else {
                        if (opinionLabel == RelationshipManager.Close_Friend) {
                            reactions.Add(EMOTION.Disapproval);
                            reactions.Add(EMOTION.Anger);
                        } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                            reactions.Add(EMOTION.Disapproval);
                            reactions.Add(EMOTION.Anger);
                        } else if (actor.traitContainer.HasTrait("Cultist") && witness.traitContainer.HasTrait("Cultist")) {
                            reactions.Add(EMOTION.Approval);
                            if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                                int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                                if (GameUtilities.RollChance(chance)) {
                                    reactions.Add(EMOTION.Arousal);
                                }
                            }
                        } else if (opinionLabel == RelationshipManager.Friend) {
                            reactions.Add(EMOTION.Disapproval);
                            reactions.Add(EMOTION.Anger);
                        } else {
                            reactions.Add(EMOTION.Approval);
                        }
                    }
                } else {
                    if (opinionLabel == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Disapproval);
                        reactions.Add(EMOTION.Anger);
                    } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                        reactions.Add(EMOTION.Disapproval);
                        reactions.Add(EMOTION.Anger);
                    } else if (actor.traitContainer.HasTrait("Cultist") && witness.traitContainer.HasTrait("Cultist")) {
                        reactions.Add(EMOTION.Approval);
                        if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                            int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                            if (GameUtilities.RollChance(chance)) {
                                reactions.Add(EMOTION.Arousal);
                            }
                        }
                    } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Acquaintance) {
                        reactions.Add(EMOTION.Disapproval);
                    } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
                        reactions.Add(EMOTION.Approval);
                    } else if (!targetCharacter.isNormalCharacter) {
                        reactions.Add(EMOTION.Disinterest);
                    }
                }
            } else if (target is TileObject targetTileObject) {
                if (targetTileObject.IsOwnedBy(witness)) {
                    reactions.Add(EMOTION.Resentment);
                    reactions.Add(EMOTION.Anger);
                } else if (actor.traitContainer.HasTrait("Cultist") && witness.traitContainer.HasTrait("Cultist")) {
                    reactions.Add(EMOTION.Disinterest);
                } else if (targetTileObject.tileObjectType == TILE_OBJECT_TYPE.TOMBSTONE) { //TODO: Human Meat, Elven Meat
                    Character characterRef = null;
                    if (targetTileObject is Tombstone tombstone) {
                        characterRef = tombstone.character;
                    }
                    string refOpinionLabel = witness.relationshipContainer.GetOpinionLabel(characterRef);
                    if (refOpinionLabel == RelationshipManager.Acquaintance) {
                        reactions.Add(EMOTION.Resentment);
                    } else if (refOpinionLabel == RelationshipManager.Friend || refOpinionLabel == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Resentment);
                        reactions.Add(EMOTION.Rage);
                    } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(characterRef)) {
                        reactions.Add(EMOTION.Resentment);
                        reactions.Add(EMOTION.Rage);
                    } else if (refOpinionLabel == RelationshipManager.Enemy || refOpinionLabel == RelationshipManager.Rival) {
                        reactions.Add(EMOTION.Disapproval);
                    }
                } else {
                    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
                    if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Concern);
                    } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(actor)) {
                        reactions.Add(EMOTION.Concern);
                    } else {
                        reactions.Add(EMOTION.Disapproval);
                    }
                }
            }
        }
    }
    public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
        if (target is Character targetCharacter && targetCharacter.faction != null && targetCharacter.faction.isMajorNonPlayer && !witness.IsHostileWith(targetCharacter)) {
            if (node.associatedJobType == JOB_TYPE.APPREHEND) {
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                bool targetHasHeinousOrSeriousCrime = targetCharacter.crimeComponent.HasCrime(CRIME_SEVERITY.Serious, CRIME_SEVERITY.Heinous);
                bool targetHasMisdemeanour = targetCharacter.crimeComponent.HasCrime(CRIME_SEVERITY.Misdemeanor);

                if (targetHasHeinousOrSeriousCrime) {
                    if (opinionLabel == RelationshipManager.Acquaintance) {
                        reactions.Add(EMOTION.Disappointment);
                    } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Disappointment);
                        reactions.Add(EMOTION.Shock);
                    } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                   && opinionLabel != RelationshipManager.Rival) {
                        reactions.Add(EMOTION.Disappointment);
                        reactions.Add(EMOTION.Anger);
                    } else {
                        reactions.Add(EMOTION.Disgust);
                    }
                } else if (targetHasMisdemeanour) {
                    if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Disappointment);
                        reactions.Add(EMOTION.Concern);
                    } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                   && opinionLabel != RelationshipManager.Rival) {
                        reactions.Add(EMOTION.Disappointment);
                        reactions.Add(EMOTION.Concern);
                    } else {
                    }
                }
            } else {
                reactions.Add(EMOTION.Concern);
            }
        }
    }
    public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
        if (target is Character targetCharacter) {
            if (node.crimeType != CRIME_TYPE.None && node.crimeType != CRIME_TYPE.Unset) {
                CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(targetCharacter, actor, target, node.crimeType);
                if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                    reactions.Add(EMOTION.Resentment);
                    if (targetCharacter.relationshipContainer.IsFriendsWith(actor)) {
                        reactions.Add(EMOTION.Betrayal);
                    } else if (targetCharacter.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(actor)) {
                        reactions.Add(EMOTION.Betrayal);
                    }
                }
            }
        }
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
    public override bool IsInvalidOnVision(ActualGoapNode node, out string reason) {
        reason = string.Empty;
        return false;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if(crime.associatedJobType == JOB_TYPE.SNATCH) {
            //Crime is Assault if the job is Snatch because Snatch jobs are supposed to be Stealth, and Stealth only works if the action is a crime
            return CRIME_TYPE.Assault;
        }
        if(target is Character targetCharacter) {
            if (targetCharacter.race.IsSapient()) {
                if (crime.associatedJobType != JOB_TYPE.APPREHEND && crime.associatedJobType != JOB_TYPE.RESTRAIN) {
                    //since there is no drink blood job (it uses fullness recovery), to check if job is from drink blood, just check if the associated job type is knockout
                    //since knockout will only ever be used for fullness recovery if it is for Drinking Blood 
                    bool isDrinkBloodJob = crime.associatedJobType.IsFullnessRecoveryTypeJob(); 
                    if (isDrinkBloodJob || crime.associatedJobType == JOB_TYPE.IMPRISON_BLOOD_SOURCE) {
                        return CRIME_TYPE.Vampire;
                    } else {
                        CombatData combatDataAgainstPOIHit = actor.combatComponent.GetCombatData(targetCharacter);
                        if (combatDataAgainstPOIHit != null && (combatDataAgainstPOIHit.reasonForCombat == CombatManager.Retaliation || combatDataAgainstPOIHit.reasonForCombat == CombatManager.Hostility)) {
                            //if combat came from retaliation, do no consider assault as a crime.
                            return CRIME_TYPE.None;
                        }
                        //if (!actor.IsHostileWith(targetCharacter)) {
                            return CRIME_TYPE.Assault;
                        //}
                    }
                }
            }
            // if(crime.associatedJobType != JOB_TYPE.APPREHEND && target is Character targetCharacter && targetCharacter.isNormalCharacter) {
            //     return CRIME_TYPE.Assault;
            // }
        } else if(target is TileObject targetTileObject && targetTileObject.characterOwner != null && !targetTileObject.IsOwnedBy(actor)) {
            return CRIME_TYPE.Disturbances;
        }
        return base.GetCrimeType(actor, target, crime);
    }
    public override string GetActionIconString(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest target = node.poiTarget;
        return actor.combatComponent.GetCombatStateIconString(target, node);
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        string reason = GetReason(node);
        log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget == null || poiTarget.mapObjectVisual == null) {
                //Cannot assault characters/objects that has no visual object
                return false;
            }
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
#if DEBUG_LOG
        Debug.Log($"{goapNode.actor} will start combat towards {goapNode.poiTarget.name}");
#endif
        string combatReason = CombatManager.Action;
        bool isLethal = goapNode.associatedJobType.IsJobLethal();
        if(goapNode.associatedJobType == JOB_TYPE.DEMON_KILL) {
            combatReason = CombatManager.Demon_Kill;
        }
        //goapNode.actor.combatComponent.SetActionAndJobThatTriggeredCombat(goapNode, goapNode.actor.currentJob as GoapPlanJob);
        goapNode.actor.combatComponent.Fight(goapNode.poiTarget, combatReason, connectedAction: goapNode, isLethal: isLethal);

        string reason = GetReason(goapNode);
        goapNode.descriptionLog.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
        // if(goapNode.poiTarget is Character) {
        //     Character targetCharacter = goapNode.poiTarget as Character;
        //     if (goapNode.associatedJobType != JOB_TYPE.APPREHEND && !goapNode.actor.IsHostileWith(targetCharacter)) {
        //         CrimeManager.Instance.ReactToCrime(targetCharacter, goapNode.actor, goapNode, goapNode.associatedJobType, CRIME_TYPE.MISDEMEANOR);
        //     }
        // }
    }
#endregion

    private string GetReason(ActualGoapNode action) {
        string key = action.actor.combatComponent.GetCombatActionReason(action, action.poiTarget);
        JOB_TYPE jobType = action.associatedJobType;
        string reason = string.Empty;
        if (!string.IsNullOrEmpty(key) && LocalizationManager.Instance.HasLocalizedValue("Character", "Combat", key)) {
            reason = LocalizationManager.Instance.GetLocalizedValue("Character", "Combat", key);
        } else {
            reason = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(jobType.ToString()) + ".";
        }
        return reason;
    }
}