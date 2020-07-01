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
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target,
            ref Log overrideEffectLog, ActualGoapNode goapNode = null) {
            if(target is Character targetCharacter) {
                string debugLog = $"{actor.name} invite to make love interrupt with {targetCharacter.name}";

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
                debugLog += $"\n-Base accept weight: {acceptWeight}";
                debugLog += $"\n-Base reject weight: {rejectWeight}";


                int targetOpinionToActor = 0;
                if (targetCharacter.relationshipContainer.HasRelationshipWith(actor)) {
                    targetOpinionToActor = targetCharacter.relationshipContainer.GetTotalOpinion(actor);
                }
                acceptWeight += (3 * targetOpinionToActor);
                debugLog += $"\n-Target opinion towards Actor: +(3 x {targetOpinionToActor}) to Accept Weight";

                Trait trait = targetCharacter.traitContainer.GetNormalTrait<Trait>("Chaste", "Lustful");
                if(trait != null) {
                    if(trait.name == "Lustful") {
                        acceptWeight += 100;
                        debugLog += "\n-Target is Lustful: +100 to Accept Weight";
                    } else {
                        rejectWeight += 100;
                        debugLog += "\n-Target is Lustful: +100 to Reject Weight";
                    }
                }

                if(targetCharacter.moodComponent.moodState == MOOD_STATE.LOW) {
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
                debugLog += $"\n\nCHOSEN RESPONSE: {chosen}";
                actor.logComponent.PrintLogIfActive(debugLog);


                overrideEffectLog = new Log(GameManager.Instance.Today(), "Interrupt", "Invite To Make Love", chosen);
                overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                overrideEffectLog.AddToFillers(target, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                //actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);

                actor.interruptComponent.SetIdentifier(chosen, true);
                if (chosen == "Reject") {
                    actor.relationshipContainer.AdjustOpinion(actor, targetCharacter, "Base", -3, "rejected sexual advances");
                    actor.traitContainer.AddTrait(actor, "Annoyed");
                    actor.currentJob.CancelJob(false);
                    if(actor.faction == FactionManager.Instance.disguisedFaction) {
                        actor.ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
                        if (!targetCharacter.marker.HasUnprocessedPOI(actor)) {
                            targetCharacter.marker.AddUnprocessedPOI(actor);
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
