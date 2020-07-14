using System.Collections.Generic;
using Traits;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Interrupts {
    public class BeingBrainwashed : Interrupt {
        
        public BeingBrainwashed() : base(INTERRUPT.Being_Brainwashed) {
            duration = 24;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Sad_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            WeightedDictionary<bool> brainwashWeightedDictionary = new WeightedDictionary<bool>();

            int failWeight = 100;
            int successWeight = 20;

            if (interruptHolder.actor.moodComponent.moodState == MOOD_STATE.LOW || interruptHolder.actor.moodComponent.moodState == MOOD_STATE.CRITICAL) {
                if (interruptHolder.actor.moodComponent.moodState == MOOD_STATE.LOW) {
                    successWeight += 50;
                } else if (interruptHolder.actor.moodComponent.moodState == MOOD_STATE.CRITICAL) {
                    successWeight += 200;
                }

                if (interruptHolder.actor.traitContainer.HasTrait("Evil")) {
                    successWeight += 100;
                }
                if (interruptHolder.actor.traitContainer.HasTrait("Treacherous")) {
                    successWeight += 100;
                }
                if (interruptHolder.actor.traitContainer.HasTrait("Betrayed")) {
                    successWeight += 100;
                }
                if (interruptHolder.actor.isFactionLeader) {
                    failWeight += 600;
                }
                if (interruptHolder.actor.isSettlementRuler) {
                    failWeight += 600;
                }
            }

            brainwashWeightedDictionary.AddElement(true, successWeight);
            brainwashWeightedDictionary.AddElement(false, failWeight);

            brainwashWeightedDictionary.LogDictionaryValues($"{GameManager.Instance.TodayLogString()}{interruptHolder.actor.name} brainwash weights:");
            
            Log log;
            if (brainwashWeightedDictionary.PickRandomElementGivenWeights()) {
                //successfully converted
                interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Cultist");
                log = new Log(GameManager.Instance.Today(), "Interrupt", "Being Brainwashed", "converted");
            } else {
                interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Unconscious");
                log = new Log(GameManager.Instance.Today(), "Interrupt", "Being Brainwashed", "not_converted");
            }
            log.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            interruptHolder.actor.logComponent.RegisterLog(log, onlyClickedCharacter: false);

            return true;
        }
        #endregion
    }
}