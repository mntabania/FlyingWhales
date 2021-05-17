using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class HungerPangs : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Hunger_Pangs;

        protected override void ActivateSymptom(Character p_character) {
            p_character.needsComponent.AdjustFullness(-10);
#if DEBUG_LOG
            Debug.Log("Activated Hunger Pangs Symptom");
#endif
        }
        public override void PerTickWhileStationaryOrUnoccupied(Character p_character) {
            if (GameUtilities.RollChance(2.5f)) {
                ActivateSymptomOn(p_character);
            }
        }
    }
}