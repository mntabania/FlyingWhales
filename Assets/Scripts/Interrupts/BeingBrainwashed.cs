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

            if (actor.moodComponent.moodState == MOOD_STATE.LOW || actor.moodComponent.moodState == MOOD_STATE.CRITICAL) {
                if (actor.moodComponent.moodState == MOOD_STATE.LOW) {
                    successWeight += 50;
                } else if (actor.moodComponent.moodState == MOOD_STATE.CRITICAL) {
                    successWeight += 200;
                }

                if (actor.traitContainer.HasTrait("Evil")) {
                    successWeight += 100;
                }
                if (actor.traitContainer.HasTrait("Treacherous")) {
                    successWeight += 100;
                }
                if (actor.traitContainer.HasTrait("Betrayed")) {
                    successWeight += 100;
                }
                if (actor.isFactionLeader) {
                    failWeight += 600;
                }
                if (actor.isSettlementRuler) {
                    failWeight += 600;
                }
            }

            brainwashWeightedDictionary.AddElement(true, successWeight);
            brainwashWeightedDictionary.AddElement(false, failWeight);

            brainwashWeightedDictionary.LogDictionaryValues($"{GameManager.Instance.TodayLogString()}{actor.name} brainwash weights:");
            
            Log log;
            if (brainwashWeightedDictionary.PickRandomElementGivenWeights()) {
                //successfully converted
                actor.traitContainer.AddTrait(actor, "Cultist");
                log = new Log(GameManager.Instance.Today(), "Interrupt", "Being Brainwashed", "converted");
            } else {
                actor.traitContainer.AddTrait(actor, "Unconscious");
                log = new Log(GameManager.Instance.Today(), "Interrupt", "Being Brainwashed", "not_converted");
            }
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            actor.logComponent.RegisterLog(log, onlyClickedCharacter: false);

            return true;
        }
        #endregion
    }
}