using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  
using Traits;

public class Invite : GoapAction {

    public Invite() : base(INTERACTION_TYPE.INVITE) {
        actionIconString = GoapActionStateDB.Flirt_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON, RACE.LESSER_DEMON };
        logTags = new[] {LOG_TAG.Needs, LOG_TAG.Social};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.INVITED, string.Empty, false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Invite Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest poiTarget, JobQueueItem job,
        OtherData[] otherData) {
        return 1;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            if (targetCharacter.traitContainer.HasTrait("Unconscious")
                    || targetCharacter.combatComponent.isInCombat
                    || (targetCharacter.stateComponent.currentState != null && targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE)
                    || (targetCharacter.interruptComponent.isInterrupted && targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Cowering)) {
                string debugLog = $"{targetCharacter.name} is unconscious/in combat/in douse fire state/cowering. Invite fail.";
                actor.logComponent.PrintLogIfActive(debugLog);

                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Invite Fail";
            } else {
                string debugLog = $"{actor.name} invite to make love with {targetCharacter.name}";
                if (actor.reactionComponent.disguisedCharacter != null) {
                    actor = actor.reactionComponent.disguisedCharacter;
                }
                if (targetCharacter.reactionComponent.disguisedCharacter != null) {
                    targetCharacter = targetCharacter.reactionComponent.disguisedCharacter;
                }
                string chosen = "Reject";
                if (!targetCharacter.traitContainer.HasTrait("Unconscious")) {
                    WeightedDictionary<string> weights = new WeightedDictionary<string>();
                    int acceptWeight = 20;
                    int rejectWeight = 10;
                    Character targetLover = targetCharacter.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
                    if (targetLover != null && targetLover != actor) {
                        //Target has a different lover
                        debugLog += $"\n-Target has different lover";
                        acceptWeight = 0;
                        rejectWeight = 50;
                        debugLog += $"\n-Base accept weight: {acceptWeight}";
                        debugLog += $"\n-Base reject weight: {rejectWeight}";

                        if (targetCharacter.traitContainer.HasTrait("Unfaithful")) {
                            acceptWeight += 200;
                            debugLog += $"\n-Target is unfaithful: +200 to Accept Weight";
                            if (targetCharacter.traitContainer.HasTrait("Drunk")) {
                                acceptWeight += 100;
                                debugLog += $"\n-Target is drunk: +100 to Accept Weight";
                            }
                        } else {
                            if (targetCharacter.traitContainer.HasTrait("Treacherous", "Psychopath")) {
                                acceptWeight += 50;
                                debugLog += $"\n-Target is not unfaithful but treacherous/psychopath: +50 to Accept Weight";
                            } else {
                                rejectWeight += 100;
                                debugLog += $"\n-Target is not unfaithful/treacherous/psychopath: +100 to Reject Weight";
                            }
                            if (targetCharacter.traitContainer.HasTrait("Drunk")) {
                                acceptWeight += 50;
                                debugLog += $"\n-Target is drunk: +50 to Accept Weight";
                            }
                        }
                    } else {
                        debugLog += $"\n-Base accept weight: {acceptWeight}";
                        debugLog += $"\n-Base reject weight: {rejectWeight}";

                        //int targetOpinionToActor = 0;
                        //if (targetCharacter.relationshipContainer.HasRelationshipWith(actor)) {
                        //    targetOpinionToActor = targetCharacter.relationshipContainer.GetTotalOpinion(actor);
                        //}
                        int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(targetCharacter, actor);
                        acceptWeight += (10 * compatibility);
                        debugLog += $"\n-Target compatibility towards Actor: +(10 x {compatibility}) to Accept Weight";

                        if (targetCharacter.traitContainer.HasTrait("Drunk")) {
                            acceptWeight += 100;
                            debugLog += $"\n-Target is drunk: +100 to Accept Weight";
                        }
                    }

                    if (targetCharacter.traitContainer.HasTrait("Lustful")) {
                        acceptWeight += 100;
                        debugLog += "\n-Target is Lustful: +100 to Accept Weight";
                    } else if (targetCharacter.traitContainer.HasTrait("Chaste")) {
                        rejectWeight += 300;
                        debugLog += "\n-Target is Chaste: +300 to Reject Weight";
                    }

                    if (targetCharacter.moodComponent.moodState == MOOD_STATE.Bad) {
                        rejectWeight += 50;
                        debugLog += "\n-Target is Low mood: +50 to Reject Weight";
                    } else if (targetCharacter.moodComponent.moodState == MOOD_STATE.Critical) {
                        rejectWeight += 200;
                        debugLog += "\n-Target is Crit mood: +200 to Reject Weight";
                    }

                    weights.AddElement("Accept", acceptWeight);
                    weights.AddElement("Reject", rejectWeight);

                    debugLog += $"\n\n{weights.GetWeightsSummary("FINAL WEIGHTS")}";

                    chosen = weights.PickRandomElementGivenWeights();
                } else {
                    debugLog += "\n-Target is Unconscious: SURE REJECT";
                }
                debugLog += $"\n\nCHOSEN RESPONSE: {chosen}";

                if (chosen == "Reject") {
                    goapActionInvalidity.isInvalid = true;
                    goapActionInvalidity.stateName = "Invite Rejected";

                    actor.relationshipContainer.AdjustOpinion(actor, targetCharacter, "Base", -3, "rejected sexual advances");
                    actor.traitContainer.AddTrait(actor, "Annoyed");
                    if (actor.faction == FactionManager.Instance.disguisedFaction) {
                        actor.ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
                        if (targetCharacter.marker && !targetCharacter.marker.HasUnprocessedPOI(actor)) {
                            targetCharacter.marker.AddUnprocessedPOI(actor);
                        }
                    }
                }
            }
        }
        return goapActionInvalidity;
    }
    public override void OnInvalidAction(ActualGoapNode node) {
        base.OnInvalidAction(node);
        if (node.actor is SeducerSummon) {
            Character target = node.poiTarget as Character;
            target.combatComponent.Fight(node.actor, CombatManager.Hostility);
        }
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        if (target != witness && target is Character targetCharacter) {
            bool isActorLoverOrAffairOfWitness = witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
            bool isTargetLoverOrAffairOfWitness = witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);

            if (isActorLoverOrAffairOfWitness) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, witness, actor, status);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status);
            } else if (isTargetLoverOrAffairOfWitness) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, witness, actor, status);
                //response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status);
                if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status);
                }
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Embarassment, witness, actor, status);
                Character loverOfActor = actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
                if (loverOfActor != null && loverOfActor != targetCharacter) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor, status);
                } else if (witness.relationshipContainer.IsFriendsWith(actor)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Scorn, witness, actor, status);
                }
            }
        }
        return response;
    }
    #endregion

    #region Effects
    public void PreInviteSuccess(ActualGoapNode goapNode) {
        //goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Wooed", goapNode.actor);
        // goapNode.actor.ownParty.AddPOI(goapNode.poiTarget);
        goapNode.actor.CarryPOI(goapNode.poiTarget);
    }
    //public void PreInviteFail(ActualGoapNode goapNode) {
    //    currentState.SetIntelReaction(InviteFailReactions);
    //}
    public void AfterInviteFail(ActualGoapNode goapNode) {
        if (goapNode.actor is SeducerSummon) {
            //Start Combat between actor and target
            Character target = goapNode.poiTarget as Character;
            target.combatComponent.Fight(goapNode.actor, CombatManager.Hostility);
        } else {
            //**After Effect 1**: Actor gains Annoyed trait.
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Annoyed");
        }
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.collectionOwner.isPartOfParentRegionMap && actor.trapStructure.IsTrappedAndTrapHexIsNot(poiTarget.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner)) {
                return false;
            }
            Character target = poiTarget as Character;
            if (target == actor) {
                return false;
            }
            //if (target.currentAlterEgoName != CharacterManager.Original_Alter_Ego) { //do not woo characters that have transformed to other alter egos
            //    return false;
            //}
            if (!target.canPerform) { //target.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
                return false;
            }
            if (target.stateComponent.currentState is CombatState) { //do not invite characters that are currently in combat
                return false;
            }
            if (target.raisedFromDeadAsSkeleton) { //do not woo characters that have been raised from the dead
                return false;
            }
            if (target.carryComponent.masterCharacter.movementComponent.isTravellingInWorld || target.currentRegion != actor.currentRegion) {
                return false; //target is outside the map
            }
            return target.carryComponent.IsNotBeingCarried();
        }
        return false;
    }
    #endregion
}