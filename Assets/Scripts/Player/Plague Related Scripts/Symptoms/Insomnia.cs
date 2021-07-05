using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class Insomnia : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Insomnia;

        protected override void ActivateSymptom(Character p_character) {
            p_character.traitContainer.AddTrait(p_character, "Insomnia");
#if DEBUG_LOG
            Debug.Log("Activated Insomnia Symptom");
#endif
        }
        public override void CharacterStartedPerformingAction(Character p_character, ActualGoapNode p_action) {
            if (p_action.associatedJobType.IsTirednessRecoveryTypeJob()/* || p_action.goapType.IsRestingAction()*/) {
                if (GameUtilities.RollChance(25)) {
                    ActivateSymptomOn(p_character);
                }
            }
        }
    }
}