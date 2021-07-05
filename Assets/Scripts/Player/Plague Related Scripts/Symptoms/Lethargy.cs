using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class Lethargy : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Lethargy;

        protected override void ActivateSymptom(Character p_character) {
            p_character.traitContainer.AddTrait(p_character, "Lethargic");
#if DEBUG_LOG
            Debug.Log("Activated Lethargy Symptom");
#endif
        }
        public override void CharacterDonePerformingAction(Character p_character, INTERACTION_TYPE p_actionPerformed) {
            base.CharacterDonePerformingAction(p_character, p_actionPerformed);
            if (p_actionPerformed.IsRestingAction() || p_actionPerformed == INTERACTION_TYPE.SIT) {
                ActivateSymptomOn(p_character);
            }
        }
    }
}