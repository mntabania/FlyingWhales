using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class Sneezing : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Sneezing;

        protected override void ActivateSymptom(Character p_character) {
            p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Sneeze, p_character);
            if (PlayerManager.Instance.player.plagueComponent.CanGainPlaguePoints()) {
                PlayerManager.Instance.player.plagueComponent.GainPlaguePointFromCharacter(1, p_character);
            }
#if DEBUG_LOG
            Debug.Log("Activated Sneezing Symptom");
#endif
        }
        public override void PerTickWhileStationaryOrUnoccupied(Character p_character) {
            if (GameUtilities.RollChance(1.5f)) { //1
                ActivateSymptomOn(p_character);
            }
        }
    }
}