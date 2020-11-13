using UnityEngine;
using UtilityScripts;
namespace Plague.Fatality {
    public class TotalOrganFailure : Fatality {
        public override PLAGUE_FATALITY fatalityType => PLAGUE_FATALITY.Total_Organ_Failure;
        
        protected override void ActivateFatality(Character p_character) {
            p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Total_Organ_Failure, p_character);
            Debug.Log("Activated Total Organ Failure Fatality");
        }
        public override void CharacterStartedPerformingAction(Character p_character) {
            if (GameUtilities.RollChance(1)) {
                ActivateFatality(p_character);
            }
        }
    }
}