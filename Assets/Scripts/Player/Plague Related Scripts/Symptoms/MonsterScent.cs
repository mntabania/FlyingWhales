using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class MonsterScent : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Monster_Scent;

        protected override void ActivateSymptom(Character p_character) {
            if (p_character.currentRegion != null && p_character.gridTileLocation != null && !p_character.isInLimbo) {
                Character chosenMonster = p_character.currentRegion.GetRandomCharacterForMonsterScent(p_character);

                if(chosenMonster != null) {
                    chosenMonster.combatComponent.Fight(p_character, CombatManager.Monster_Scent);
                }
            }
#if DEBUG_LOG
            Debug.Log("Activated Monster Scent Symptom");
#endif
        }
        protected override bool CanActivateSymptomOn(Character p_character) {
            bool state = base.CanActivateSymptomOn(p_character);
            if (state) {
                return p_character.isNotSummonAndDemon;
            }
            return false;
        }
        public override void HourStarted (Character p_character, int p_numOfHoursPassed) {
            if (GameUtilities.RollChance(5)) {
                ActivateSymptomOn(p_character);
            }
        }
    }
}