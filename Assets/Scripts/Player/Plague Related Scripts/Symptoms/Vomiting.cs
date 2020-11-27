using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class Vomiting : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Vomiting;

        protected override void ActivateSymptom(Character p_character) {
            p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, p_character, "Plague");
            if (PlayerManager.Instance.player.plagueComponent.CanGainPlaguePoints()) {
                PlayerManager.Instance.player.plagueComponent.GainPlaguePointFromCharacter(1, p_character);
            }
            Debug.Log("Activated Vomiting Symptom");
        }
        public override void PerTickWhileStationaryOrUnoccupied(Character p_character) {
            if (GameUtilities.RollChance(2)) {
                ActivateSymptom(p_character);
            }
        }
    }
}