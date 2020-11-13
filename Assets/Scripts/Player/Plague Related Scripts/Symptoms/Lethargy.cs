using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class Lethargy : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Lethargy;

        protected override void ActivateSymptom(Character p_character) {
            p_character.traitContainer.AddTrait(p_character, "Lethargic");
            Debug.Log("Activated Lethargy Symptom");
        }
        public override void CharacterDonePerformingAction(Character p_character, ActualGoapNode p_actionPerformed) {
            base.CharacterDonePerformingAction(p_character, p_actionPerformed);
            if (p_actionPerformed.goapType.IsRestingAction() || p_actionPerformed.goapType == INTERACTION_TYPE.SIT) {
                ActivateSymptom(p_character);
            }
        }
    }
}