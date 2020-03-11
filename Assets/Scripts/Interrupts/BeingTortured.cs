using System.Collections.Generic;
using Traits;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Interrupts {
    public class BeingTortured : Interrupt {

        private readonly string[] _negativeTraits = new[] {
            "Agoraphobic", "Pyrophobic", "Coward", "Hothead", "Psychopath", "Drunkard",
            "Glutton", "Suspicious", "Music Hater", "Evil"
        };
        private readonly string[] _negativeStatus = new[] {
            "Injured", "Traumatized", "Spooked", "Unconscious" //, "Sick"
        };
        
        public BeingTortured() : base(INTERRUPT.Being_Tortured) {
            duration = 24;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
            interruptIconString = GoapActionStateDB.Flirt_Icon;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            string randomNegativeTrait = GetRandomValidNegativeTrait(actor);
            string randomNegativeStatus = GetRandomValidNegativeStatus(actor);

            List<string> tortureTexts =
                LocalizationManager.Instance.GetKeysLike("Interrupt", "Being Tortured", "torture");
            string chosenTortureKey = CollectionUtilities.GetRandomElement(tortureTexts);
            Log randomTorture = new Log(GameManager.Instance.Today(), "Interrupt", "Being Tortured", chosenTortureKey);
            randomTorture.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            
            Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Being Tortured", "full_text");
            log.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(randomTorture), LOG_IDENTIFIER.APPEND);
            log.AddToFillers(randomTorture.fillers);
            log.AddToFillers(null, randomNegativeStatus, LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, randomNegativeTrait, LOG_IDENTIFIER.STRING_2);
            actor.logComponent.RegisterLog(log, onlyClickedCharacter: false);

            actor.traitContainer.AddTrait(actor, randomNegativeStatus);
            actor.traitContainer.AddTrait(actor, randomNegativeTrait);
            
            return base.ExecuteInterruptEndEffect(actor, target);
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
            Assert.IsTrue(validTraits.Count > 0, $"Trying to add random negative trait to {target.name}, but could not find any traits to add to him/her!");
            return CollectionUtilities.GetRandomElement(validTraits);
        }
        private string GetRandomValidNegativeStatus(Character target) {
            List<string> validTraits = new List<string>();
            for (int i = 0; i < _negativeStatus.Length; i++) {
                string traitName = _negativeStatus[i];
                if (TraitValidator.CanAddTrait(target, traitName, target.traitContainer)) {
                    validTraits.Add(traitName);
                }   
            }
            Assert.IsTrue(validTraits.Count > 0, $"Trying to add random negative status to {target.name}, but could not find any traits to add to him/her!");
            return CollectionUtilities.GetRandomElement(validTraits);
        }
    }
}