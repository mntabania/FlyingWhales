using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class Depression : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Depression;

        protected override void ActivateSymptom(Character p_character) {
            p_character.traitContainer.AddTrait(p_character, "Depressed");
#if DEBUG_LOG
            Debug.Log("Activated Depression Symptom");
#endif
        }
        public override void CharacterStartedPerformingAction(Character p_character, ActualGoapNode p_action) {
            if (p_action.associatedJobType.IsHappinessRecoveryTypeJob()) {
                if (GameUtilities.RollChance(25)) {
                    ActivateSymptomOn(p_character);
                }
            }
        }
    }
}