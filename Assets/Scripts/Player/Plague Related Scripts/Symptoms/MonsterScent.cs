using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class MonsterScent : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Monster_Scent;

        protected override void ActivateSymptom(Character p_character) {
            if(p_character.currentRegion != null && p_character.gridTileLocation != null) {
                Character chosenMonster = p_character.currentRegion.GetRandomCharacterThatMeetCriteria(c => !c.isDead
                && c.limiterComponent.canPerform
                && c.limiterComponent.canMove
                && c.movementComponent.HasPathTo(p_character.gridTileLocation)
                && (c is Summon)
                && !(c is Animal));

                if(chosenMonster != null) {
                    chosenMonster.combatComponent.Fight(p_character, CombatManager.Monster_Scent);
                }
            }
            Debug.Log("Activated Monster Scent Symptom");
        }
        public override void HourStarted (Character p_character, int p_numOfHoursPassed) {
            if (GameUtilities.RollChance(5)) {
                ActivateSymptom(p_character);
            }
        }
    }
}