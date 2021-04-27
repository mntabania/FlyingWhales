using System;
using System.Collections.Generic;
using Traits;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Interrupts {
    public class BeingTortured : Interrupt {

        private readonly string[] _negativeTraits = new[] {
            "Agoraphobic", "Pyrophobic", "Coward", "Hothead", "Alcoholic", //"Psychopath"
            "Glutton", "Suspicious", "Music Hater", "Evil"
        };
        private readonly string[] _negativeStatus = new[] {
            "Injured", "Traumatized", "Spooked", "Unconscious" //, "Sick"
        };
        
        public BeingTortured() : base(INTERRUPT.Being_Tortured) {
            duration = 20;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Injured_Icon;
            logTags = new[] {LOG_TAG.Life_Changes, LOG_TAG.Player};
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder) {
            string randomNegativeTrait = GetRandomValidNegativeTrait(interruptHolder.actor);
            string randomNegativeStatus = GetRandomValidNegativeStatus(interruptHolder.actor);

            //At some point, if a character is tortured again and again, statuses/traits can no longer be added to him because in CanAddTrait checking if a status/trait is not stacking and it already has that status. it cannot be added anymore
            //So there will come a time that there will be no more status/trait that can be added to the character
            //So we need to check if it can no longer find a random negative trait/status to add
            //If not, add a dfferent log
            //https://trello.com/c/8sAgvnbE/2210-torture-argumentexception
            if (string.IsNullOrEmpty(randomNegativeTrait) || string.IsNullOrEmpty(randomNegativeStatus)) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Being Tortured", "cannot_torture", null, logTags);
                log.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                interruptHolder.actor.logComponent.RegisterLog(log, true);
            } else {
                List<string> tortureTexts = LocalizationManager.Instance.GetKeysLike("Interrupt", "Being Tortured", "torture");
                string chosenTortureKey = CollectionUtilities.GetRandomElement(tortureTexts);
                Log randomTorture = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Being Tortured", chosenTortureKey, null, logTags);
                randomTorture.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Being Tortured", "full_text", null, logTags);
                log.AddToFillers(null, randomTorture.unReplacedText, LOG_IDENTIFIER.APPEND);
                log.AddToFillers(randomTorture.fillers);
                log.AddToFillers(null, randomNegativeStatus, LOG_IDENTIFIER.STRING_1);
                log.AddToFillers(null, randomNegativeTrait, LOG_IDENTIFIER.STRING_2);
                interruptHolder.actor.logComponent.RegisterLog(log, true);

                interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, randomNegativeStatus);
                interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, randomNegativeTrait);

                interruptHolder.actor.AddAfflictionByPlayer(randomNegativeStatus);
                interruptHolder.actor.AddAfflictionByPlayer(randomNegativeTrait);

                //Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, interruptHolder.actor.marker.transform.position, UnityEngine.Random.Range(2, 4), interruptHolder.actor.currentRegion.innerMap);
            }

            return base.ExecuteInterruptEndEffect(interruptHolder);
        }
        #endregion

        private string GetRandomValidNegativeTrait(Character target) {
            List<string> validTraits = new List<string>();
            for (int i = 0; i < _negativeTraits.Length; i++) {
                string traitName = _negativeTraits[i];
                if (TraitValidator.CanAddTrait(target, traitName, target.traitContainer)) {
                    validTraits.Add(traitName);
                }   
            }
            // Assert.IsTrue(validTraits.Count > 0, $"Trying to add random negative trait to {target.name}, but could not find any traits to add to him/her!");
            if (validTraits.Count > 0) {
                return CollectionUtilities.GetRandomElement(validTraits);    
            }
            return string.Empty;
        }
        private string GetRandomValidNegativeStatus(Character target) {
            List<string> validTraits = new List<string>();
            for (int i = 0; i < _negativeStatus.Length; i++) {
                string traitName = _negativeStatus[i];
                if (TraitValidator.CanAddTrait(target, traitName, target.traitContainer)) {
                    validTraits.Add(traitName);
                }   
            }
            // Assert.IsTrue(validTraits.Count > 0, $"Trying to add random negative status to {target.name}, but could not find any traits to add to him/her!");
            if (validTraits.Count > 0) {
                return CollectionUtilities.GetRandomElement(validTraits);    
            }
            return string.Empty;
        }
    }
}