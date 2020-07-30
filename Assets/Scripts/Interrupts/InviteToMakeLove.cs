using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

namespace Interrupts {
    public class InviteToMakeLove : Interrupt {
        public InviteToMakeLove() : base(INTERRUPT.Invite_To_Make_Love) {
            duration = 0;
            isSimulateneous = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            if(interruptHolder.target is Character targetCharacter) {
                string debugLog = $"{interruptHolder.actor.name} invite to make love interrupt with {targetCharacter.name}";
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


                //    overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Invite To Make Love", "Reject");
                //    overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                //    overrideEffectLog.AddToFillers(target, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                //    actor.currentJob.CancelJob(false);
                //    return false;
                //}

                WeightedDictionary<string> weights = new WeightedDictionary<string>();
                int acceptWeight = 50;
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
                    } else if (targetCharacter.traitContainer.HasTrait("Treacherous", "Psychopath")) {
                        acceptWeight += 50;
                        debugLog += $"\n-Target is not unfaithful but treacherous/psychopath: +50 to Accept Weight";
                    } else {
                        rejectWeight += 100;
                        debugLog += $"\n-Target is not unfaithful/treacherous/psychopath: +100 to Reject Weight";
                    }
                } else {

                    debugLog += $"\n-Base accept weight: {acceptWeight}";
                    debugLog += $"\n-Base reject weight: {rejectWeight}";

                    int targetOpinionToActor = 0;
                    if (targetCharacter.relationshipContainer.HasRelationshipWith(actor)) {
                        targetOpinionToActor = targetCharacter.relationshipContainer.GetTotalOpinion(actor);
                    }
                    acceptWeight += (3 * targetOpinionToActor);
                    debugLog += $"\n-Target opinion towards Actor: +(3 x {targetOpinionToActor}) to Accept Weight";
                }

                if (targetCharacter.traitContainer.HasTrait("Lustful")) {
                    acceptWeight += 100;
                    debugLog += "\n-Target is Lustful: +100 to Accept Weight";
                } else if (targetCharacter.traitContainer.HasTrait("Chaste")) {
                    rejectWeight += 300;
                    debugLog += "\n-Target is Chaste: +300 to Reject Weight";
                }

                if (targetCharacter.moodComponent.moodState == MOOD_STATE.LOW) {
                    rejectWeight += 50;
                    debugLog += "\n-Target is Low mood: +50 to Reject Weight";
                } else if (targetCharacter.moodComponent.moodState == MOOD_STATE.CRITICAL) {
                    rejectWeight += 200;
                    debugLog += "\n-Target is Crit mood: +200 to Reject Weight";
                }

                weights.AddElement("Accept", acceptWeight);
                weights.AddElement("Reject", rejectWeight);

                debugLog += $"\n\n{weights.GetWeightsSummary("FINAL WEIGHTS")}";

                string chosen = weights.PickRandomElementGivenWeights();
                if (interruptHolder.target.traitContainer.HasTrait("Unconscious")) {
                    debugLog += "\n-Target is Unconscious: SURE REJECT";
                    chosen = "Reject";
                }
                debugLog += $"\n\nCHOSEN RESPONSE: {chosen}";
                interruptHolder.actor.logComponent.PrintLogIfActive(debugLog);


                overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Invite To Make Love", chosen);
                overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                //actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);

                interruptHolder.SetIdentifier(chosen);
                if (chosen == "Reject") {
                    interruptHolder.actor.relationshipContainer.AdjustOpinion(interruptHolder.actor, targetCharacter, "Base", -3, "rejected sexual advances");
                    interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Annoyed");
                    interruptHolder.actor.currentJob.CancelJob(false);
                    if(interruptHolder.actor.faction == FactionManager.Instance.disguisedFaction) {
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
        #endregion
    }
}
