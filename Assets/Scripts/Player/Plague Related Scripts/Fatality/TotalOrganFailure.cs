using UnityEngine;
using UtilityScripts;
namespace Plague.Fatality {
    public class TotalOrganFailure : Fatality {
        public override PLAGUE_FATALITY fatalityType => PLAGUE_FATALITY.Total_Organ_Failure;
        
        protected override void ActivateFatality(Character p_character) {
            p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Total_Organ_Failure, p_character);
            PlagueDisease.Instance.UpdateDeathsOnCharacterDied(p_character);
        }
        public override bool CharacterStartedPerformingAction(Character p_character, ActualGoapNode p_action) {
            if (GameUtilities.RollChance(1)) { //1
                ActivateFatality(p_character);
                return false;
            }
            return true;
        }
    }
}