using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class Seizure : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Seizure;

        protected override void ActivateSymptom(Character p_character) {
            p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Seizure, p_character);
            if (PlayerManager.Instance.player.plagueComponent.CanGainPlaguePoints()) {
                PlayerManager.Instance.player.plagueComponent.GainPlaguePointFromCharacter(2, p_character);    
            }
            
            Debug.Log("Activated Seizure Symptom");
        }
        public override void PerTickMovement(Character p_character) {
            if (GameUtilities.RollChance(1.5f)) {
                ActivateSymptom(p_character);
            }
        }
    }
}