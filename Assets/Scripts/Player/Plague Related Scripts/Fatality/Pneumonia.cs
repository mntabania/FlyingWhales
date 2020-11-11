using UnityEngine;
using UtilityScripts;

namespace Plague.Fatality {
    public class Pneumonia : Fatality {
        
        public override FATALITY fatalityType => FATALITY.Pneumonia;
        
        protected override void ActivateFatality(Character p_character) {
            //TODO: Trigger Pneumonia Interrupt
            Debug.Log("Activated Pneumonia Fatality");
        }
        public override void PerTickMovement(Character p_character) {
            if (GameUtilities.RollChance(0.5f)) {
                ActivateFatality(p_character);
            }
        }
    }
}