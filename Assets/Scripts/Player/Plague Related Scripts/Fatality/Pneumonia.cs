using UnityEngine;
using UtilityScripts;

namespace Plague.Fatality {
    public class Pneumonia : Fatality {
        
        public override PLAGUE_FATALITY fatalityType => PLAGUE_FATALITY.Pneumonia;
        
        protected override void ActivateFatality(Character p_character) {
            p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Pneumonia, p_character);
            PlagueDisease.Instance.UpdateDeathsOnCharacterDied(p_character);
        }
        public override void PerTickMovement(Character p_character) {
            if (GameUtilities.RollChance(0.5f)) { //0.5f
                ActivateFatality(p_character);
            }
        }
    }
}