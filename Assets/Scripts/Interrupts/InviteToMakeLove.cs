using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class InviteToMakeLove : Interrupt {
        public InviteToMakeLove() : base(INTERRUPT.Invite_To_Make_Love) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
            logTags = new[] {LOG_TAG.Social, LOG_TAG.Needs};
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            if(interruptHolder.target is Character targetCharacter) {
#if DEBUG_LOG
                string debugLog = $"{interruptHolder.actor.name} invite to make love interrupt with {targetCharacter.name}";
#endif
                Character actor = interruptHolder.actor;
                if (actor.reactionComponent.disguisedCharacter != null) {
                    actor = actor.reactionComponent.disguisedCharacter;
                }
                if (targetCharacter.reactionComponent.disguisedCharacter != null) {
                    targetCharacter = targetCharacter.reactionComponent.disguisedCharacter;
                }
                //if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Unconscious") != null 
                //    || targetCharacter.combatComponent.isInCombat
                //    || (targetCharacter.stateComponent.currentState != null && targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.DOUSE_FIRE)
                //    || (targetCharacter.interruptComponent.isInterrupted && targetCharacter.interruptComponent.currentInterrupt.interrupt == INTERRUPT.Cowering)) {
                //    debugLog += $"{targetCharacter.name} is unconscious/in combat/in douse fire state/cowering. Invite rejected.";
                //    actor.logComponent.PrintLogIfActive(debugLog);


                //    overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Invite To Make Love", "Reject");
                //    overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                //    overrideEffectLog.AddToFillers(target, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                //    actor.currentJob.CancelJob(false);
                //    return false;
                //}
                string chosen = "Reject";
                if (!interruptHolder.target.traitContainer.HasTrait("Unconscious")) {
                    WeightedDictionary<string> weights = new WeightedDictionary<string>();
                    int acceptWeight = 20;
                    int rejectWeight = 10;
                    Character targetLover = targetCharacter.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
                    if (targetLover != null && targetLover != actor) {
                        //Target has a different lover
#if DEBUG_LOG
                        debugLog += $"\n-Target has different lover";
#endif
                        acceptWeight = 0;
                        rejectWeight = 50;
#if DEBUG_LOG
                        debugLog += $"\n-Base accept weight: {acceptWeight}";
                        debugLog += $"\n-Base reject weight: {rejectWeight}";
#endif

                        if (targetCharacter.traitContainer.HasTrait("Unfaithful")) {
                            acceptWeight += 200;
#if DEBUG_LOG
                            debugLog += $"\n-Target is unfaithful: +200 to Accept Weight";
#endif
                            if (targetCharacter.traitContainer.HasTrait("Drunk")) {
                                acceptWeight += 100;
#if DEBUG_LOG
                                debugLog += $"\n-Target is drunk: +100 to Accept Weight";
#endif
                            }
                        } else {
                            if (targetCharacter.traitContainer.HasTrait("Treacherous", "Psychopath")) {
                                acceptWeight += 50;
#if DEBUG_LOG
                                debugLog += $"\n-Target is not unfaithful but treacherous/psychopath: +50 to Accept Weight";
#endif
                            } else {
                                rejectWeight += 100;
#if DEBUG_LOG
                                debugLog += $"\n-Target is not unfaithful/treacherous/psychopath: +100 to Reject Weight";
#endif
                            }
                            if (targetCharacter.traitContainer.HasTrait("Drunk")) {
                                acceptWeight += 50;
#if DEBUG_LOG
                                debugLog += $"\n-Target is drunk: +50 to Accept Weight";
#endif
                            }
                        }
                    } else {
#if DEBUG_LOG
                        debugLog += $"\n-Base accept weight: {acceptWeight}";
                        debugLog += $"\n-Base reject weight: {rejectWeight}";
#endif

                        //int targetOpinionToActor = 0;
                        //if (targetCharacter.relationshipContainer.HasRelationshipWith(actor)) {
                        //    targetOpinionToActor = targetCharacter.relationshipContainer.GetTotalOpinion(actor);
                        //}
                        int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(targetCharacter, actor);
                        acceptWeight += (10 * compatibility);
#if DEBUG_LOG
                        debugLog += $"\n-Target compatibility towards Actor: +(10 x {compatibility}) to Accept Weight";
#endif

                        if (targetCharacter.traitContainer.HasTrait("Drunk")) {
                            acceptWeight += 100;
#if DEBUG_LOG
                            debugLog += $"\n-Target is drunk: +100 to Accept Weight";
#endif
                        }
                    }

                    if (targetCharacter.traitContainer.HasTrait("Lustful")) {
                        acceptWeight += 100;
#if DEBUG_LOG
                        debugLog += "\n-Target is Lustful: +100 to Accept Weight";
#endif
                    } else if (targetCharacter.traitContainer.HasTrait("Chaste")) {
                        rejectWeight += 300;
#if DEBUG_LOG
                        debugLog += "\n-Target is Chaste: +300 to Reject Weight";
#endif
                    }

                    if (targetCharacter.moodComponent.moodState == MOOD_STATE.Bad) {
                        rejectWeight += 50;
#if DEBUG_LOG
                        debugLog += "\n-Target is Low mood: +50 to Reject Weight";
#endif
                    } else if (targetCharacter.moodComponent.moodState == MOOD_STATE.Critical) {
                        rejectWeight += 200;
#if DEBUG_LOG
                        debugLog += "\n-Target is Crit mood: +200 to Reject Weight";
#endif
                    }

                    weights.AddElement("Accept", acceptWeight);
                    weights.AddElement("Reject", rejectWeight);

#if DEBUG_LOG
                    debugLog += $"\n\n{weights.GetWeightsSummary("FINAL WEIGHTS")}";
#endif

                    chosen = weights.PickRandomElementGivenWeights();
                } else {
#if DEBUG_LOG
                    debugLog += "\n-Target is Unconscious: SURE REJECT";
#endif
                }
#if DEBUG_LOG
                debugLog += $"\n\nCHOSEN RESPONSE: {chosen}";
                interruptHolder.actor.logComponent.PrintLogIfActive(debugLog);
#endif

                if (overrideEffectLog != null) { LogPool.Release(overrideEffectLog); }
                overrideEffectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Invite To Make Love", chosen, null, logTags);
                overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                //actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);

                interruptHolder.SetIdentifier(chosen);
                if (chosen == "Reject") {
                    interruptHolder.actor.relationshipContainer.AdjustOpinion(interruptHolder.actor, targetCharacter, "Base", -3, "rejected sexual advances");
                    interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Annoyed", targetCharacter);
                    interruptHolder.actor.currentJob.CancelJob();
                    if(interruptHolder.actor.faction?.factionType.type == FACTION_TYPE.Disguised) {
                        interruptHolder.actor.ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
                        if (!targetCharacter.marker.HasUnprocessedPOI(interruptHolder.actor)) {
                            targetCharacter.marker.AddUnprocessedPOI(interruptHolder.actor);
                        }
                    }
                    return false;
                }
            }
            return true;
        }
        public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, InterruptHolder interrupt, REACTION_STATUS status) {
            base.PopulateReactionsToActor(reactions, actor, target, witness, interrupt, status);

            if (target != witness && target is Character targetCharacter) {
                bool isActorLoverOrAffairOfWitness = witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
                bool isTargetLoverOrAffairOfWitness = witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);

                if (isActorLoverOrAffairOfWitness) {
                    reactions.Add(EMOTION.Rage);
                    reactions.Add(EMOTION.Betrayal);
                } else if (isTargetLoverOrAffairOfWitness) {
                    reactions.Add(EMOTION.Rage);
                    if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.IsFamilyMember(actor)) {
                        reactions.Add(EMOTION.Betrayal);
                    }
                } else {
                    reactions.Add(EMOTION.Embarassment);
                    Character loverOfActor = actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
                    if (loverOfActor != null && loverOfActor != targetCharacter) {
                        reactions.Add(EMOTION.Disapproval);
                        reactions.Add(EMOTION.Disgust);
                    } else if (witness.relationshipContainer.IsFriendsWith(actor)) {
                        reactions.Add(EMOTION.Scorn);
                    }
                }
            }
        }
#endregion
    }
}
