using UnityEngine;
using UtilityScripts;
namespace Plague.Fatality {
    public class TotalOrganFailure : Fatality {
        public override FATALITY fatalityType => FATALITY.Total_Organ_Failure;
        
        protected override void ActivateFatality(Character p_character) {
            //TODO: Trigger Total Organ Failure interrupt
            Debug.Log("Activated Total Organ Failure Fatality");
        }
        public override void CharacterStartedPerformingAction(Character p_character) {
            if (GameUtilities.RollChance(1)) {
                ActivateFatality(p_character);
            }
        }
    }
}