using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class Depression : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Depression;

        protected override void ActivateSymptom(Character p_character) {
            p_character.traitContainer.AddTrait(p_character, "Depressed");
            Debug.Log("Activated Depression Symptom");
        }
        public override void CharacterStartedPerformingAction(Character p_character) {
            if (GameUtilities.RollChance(15)) {
                ActivateSymptom(p_character);
            }
        }
    }
}