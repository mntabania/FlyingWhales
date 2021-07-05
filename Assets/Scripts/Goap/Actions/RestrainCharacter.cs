using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RestrainCharacter : GoapAction {

    public RestrainCharacter() : base(INTERACTION_TYPE.RESTRAIN_CHARACTER) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Restrain_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF,
        //    RACE.SPIDER, RACE.DRAGON, RACE.GOLEM, RACE.DEMON, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC, RACE.ABOMINATION,
        //    RACE.CHICKEN, RACE.SHEEP, RACE.PIG, RACE.NYMPH, RACE.WISP, RACE.SLUDGE, RACE.GHOST, RACE.LESSER_DEMON, RACE.ANGEL };
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
        logTags = new[] {LOG_TAG.Work, LOG_TAG.Crimes, LOG_TAG.Combat};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET }, TargetUnconsciousOrParalyzed);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_TRAIT, "Restrained", false, GOAP_EFFECT_TARGET.TARGET));
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.CANNOT_MOVE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Restrain Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            Character target = poiTarget as Character;
            if (target.carryComponent.IsNotBeingCarried() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "target_carried";
            }
        }
        return goapActionInvalidity;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (target is Character targetCharacter) {
            if (targetCharacter.traitContainer.HasTrait("Criminal")) {
                if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
                    reactions.Add(EMOTION.Resentment);
                } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.AFFAIR)) &&
                           witness.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, RelationshipManager.Rival) == false) {
                    reactions.Add(EMOTION.Resentment);
                } else {
                    reactions.Add(EMOTION.Approval);
                }
            } else {
                if (!witness.relationshipContainer.IsEnemiesWith(targetCharacter) && !witness.IsHostileWith(targetCharacter)) {
                    if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
                        if (!witness.traitContainer.HasTrait("Psychopath")) {
                            reactions.Add(EMOTION.Resentment);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 35) {
                            if (!witness.traitContainer.HasTrait("Diplomatic")) {
                                reactions.Add(EMOTION.Anger);
                            }
                        }
                    } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.AFFAIR)) &&
                               witness.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, RelationshipManager.Rival) == false) {
                        if (!witness.traitContainer.HasTrait("Psychopath")) {
                            reactions.Add(EMOTION.Resentment);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 35) {
                            if (!witness.traitContainer.HasTrait("Diplomatic")) {
                                reactions.Add(EMOTION.Anger);
                            }
                        }
                    } else if (witness.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                        reactions.Add(EMOTION.Approval);
                    }
                }
            }
        }
    }
    public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
        if (target is Character targetCharacter) {
            if (node.associatedJobType == JOB_TYPE.APPREHEND) {
                if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        reactions.Add(EMOTION.Concern);
                        reactions.Add(EMOTION.Sadness);
                    }
                } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.AFFAIR)) &&
                           witness.relationshipContainer.HasOpinionLabelWithCharacter(targetCharacter, RelationshipManager.Rival) == false) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        reactions.Add(EMOTION.Concern);
                        reactions.Add(EMOTION.Sadness);
                    }
                } else {
                    if (UnityEngine.Random.Range(0, 100) < 30 && !witness.traitContainer.HasTrait("Diplomatic")) {
                        reactions.Add(EMOTION.Scorn);
                    }
                }
            } else {
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        reactions.Add(EMOTION.Distraught);
                    }
                } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.AFFAIR)) &&
                          opinionLabel != RelationshipManager.Rival) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        reactions.Add(EMOTION.Distraught);
                    }
                } else if (opinionLabel == RelationshipManager.Acquaintance) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        reactions.Add(EMOTION.Concern);
                    }
                } else if (((witness.faction != null && witness.faction.leader == targetCharacter) || (witness.homeSettlement != null && witness.homeSettlement.ruler == targetCharacter))
                    && opinionLabel != RelationshipManager.Rival) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        reactions.Add(EMOTION.Distraught);
                    }
                } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
                    if (!witness.traitContainer.HasTrait("Diplomatic")) {
                        reactions.Add(EMOTION.Scorn);
                    } else {
                        reactions.Add(EMOTION.Concern);
                    }
                }
            }
        }
    }
    public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
        if (target is Character targetCharacter) {
            if (!targetCharacter.IsHostileWith(actor)) {
                reactions.Add(EMOTION.Resentment);
                if (targetCharacter.traitContainer.HasTrait("Hothead") || UnityEngine.Random.Range(0, 100) < 35) {
                    reactions.Add(EMOTION.Anger);
                }
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.poiTarget is Character character) {
            if (node.poiTarget.traitContainer.HasTrait("Criminal") || witness.IsHostileWith(character)) {
                return REACTABLE_EFFECT.Positive;
            }
        }
        return REACTABLE_EFFECT.Negative;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (actor != poiTarget) {
                if(poiTarget is Character target) {
                    if (!target.isDead) {
                        if (target.traitContainer.HasTrait("Restrained")) {
                            //If character is already restrained we need to check if we can override the existing one
                            //To override, target must not be already a personal prisoner of the actor if the actor wants it to be a personal prisoner. This also applies to faction prisoner
                            Prisoner prisoner = target.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                            if (prisoner != null) {
                                if (ShouldBePersonalPrisoner(job.jobType, actor)) {
                                    if (!prisoner.IsPersonalPrisonerOf(actor)) {
                                        return true;
                                    }
                                } else {
                                    if (actor.faction != null && !prisoner.IsFactionPrisonerOf(actor.faction)) {
                                        return true;
                                    }
                                }
                            }
                            return false;
                        } else {
                            return true;
                        }
                    }
                    return false;
                    //return !target.isDead && !target.traitContainer.HasTrait("Restrained"); //&& !(target is Dragon);
                }
            }
            return false;
        }
        return false;
    }
#endregion

    #region State Effects
    public void AfterRestrainSuccess(ActualGoapNode goapNode) {
        //**Effect 1**: Target gains Restrained trait.
        Faction factionThatImprisoned = null;
        Character characterThatImprisoned = null;
        if (ShouldBePersonalPrisoner(goapNode.associatedJobType, goapNode.actor)) {
            characterThatImprisoned = goapNode.actor;
        } else {
            if (goapNode.associatedJobType == JOB_TYPE.SNATCH || goapNode.associatedJobType == JOB_TYPE.SNATCH_RESTRAIN) {
                factionThatImprisoned = PlayerManager.Instance.player.playerFaction;
            } else {
                factionThatImprisoned = goapNode.actor.faction;
            }
        }
        //if (goapNode.associatedJobType == JOB_TYPE.APPREHEND
        //       || goapNode.associatedJobType == JOB_TYPE.KIDNAP_RAID
        //       || (goapNode.actor.faction?.factionType.type == FACTION_TYPE.Ratmen && goapNode.associatedJobType == JOB_TYPE.MONSTER_ABDUCT)
        //       || goapNode.associatedJobType == JOB_TYPE.RESTRAIN) {
        //    factionThatImprisoned = goapNode.actor.faction;
        //} else if (goapNode.associatedJobType == JOB_TYPE.SNATCH || goapNode.associatedJobType == JOB_TYPE.SNATCH_RESTRAIN) {
        //    factionThatImprisoned = PlayerManager.Instance.player.playerFaction;
        //} else {
        //    characterThatImprisoned = goapNode.actor;
        //}

        goapNode.poiTarget.traitContainer.RestrainAndImprison(goapNode.poiTarget, goapNode.actor, factionThatImprisoned, characterThatImprisoned);
    }
    #endregion

    #region Preconditions
    private bool TargetUnconsciousOrParalyzed(Character actor, IPointOfInterest target, object[] otherData, JOB_TYPE jobType) {
        return target.traitContainer.HasTrait("Unconscious", "Paralyzed");
    }
    #endregion

    private bool ShouldBePersonalPrisoner(JOB_TYPE jobType, Character p_actor) {
        if (jobType == JOB_TYPE.APPREHEND
              || jobType == JOB_TYPE.KIDNAP_RAID
              || (p_actor.faction?.factionType.type == FACTION_TYPE.Ratmen && jobType == JOB_TYPE.MONSTER_ABDUCT)
              || jobType == JOB_TYPE.RESTRAIN
              || jobType == JOB_TYPE.SNATCH 
              || jobType == JOB_TYPE.SNATCH_RESTRAIN) {
            return false;
        }
        return true;
    }
}