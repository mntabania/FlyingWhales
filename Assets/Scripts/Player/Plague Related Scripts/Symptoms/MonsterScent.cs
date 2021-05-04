using Traits;
using UnityEngine;
using UtilityScripts;

namespace Plague.Symptom {
    public class MonsterScent : PlagueSymptom {
        
        public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Monster_Scent;

        protected override void ActivateSymptom(Character p_character) {
            if (p_character.currentRegion != null && p_character.gridTileLocation != null && !p_character.isInLimbo) {
                Character chosenMonster = p_character.currentRegion.GetRandomCharacterThatMeetCriteria(c => !c.isDead
                && c.limiterComponent.canPerform
                && c.limiterComponent.canMove
                && c.movementComponent.HasPathTo(p_character.gridTileLocation)
                && !c.movementComponent.isStationary
                && (c is Summon)
                && !(c is Animal)
                && !c.isInLimbo
                && !IsCharacterTheSameLycan(p_character, c)
                && (!c.partyComponent.hasParty));

                if(chosenMonster != null) {
                    chosenMonster.combatComponent.Fight(p_character, CombatManager.Monster_Scent);
                }
            }
            Debug.Log("Activated Monster Scent Symptom");
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

        private bool IsCharacterTheSameLycan(Character character1, Character character2) {
            LycanthropeData lycanData = null;
            if (character1.isLycanthrope) {
                lycanData = character1.lycanData;
            } else if (character2.isLycanthrope) {
                lycanData = character2.lycanData;
            }
            if(lycanData != null) {
                return (lycanData.originalForm == character1 || lycanData.lycanthropeForm == character1) && (lycanData.originalForm == character2 || lycanData.lycanthropeForm == character2);
            }
            return false;
        }
    }
}