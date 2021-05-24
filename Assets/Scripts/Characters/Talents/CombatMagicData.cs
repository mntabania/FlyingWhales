using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character_Talents {
    public class CombatMagicData : CharacterTalentData {
        public CombatMagicData() : base(CHARACTER_TALENT.Combat_Magic) {
            description = $"Mastery of destructive magic used in combat.";
        }

        #region Levels
        public override void OnLevelUp(Character p_character, int level) {
            switch (level) {
                case 1:
                Level1(p_character);
                break;
                case 2:
                Level2(p_character);
                break;
                case 3:
                Level3(p_character);
                break;
                case 4:
                Level4(p_character);
                break;
                case 5:
                Level5(p_character);
                break;
            }
        }
        private void Level1(Character p_character) {
            p_character.classComponent.AddAbleClass("Druid");
        }
        private void Level2(Character p_character) {
            p_character.combatComponent.AdjustIntelligencePercentModifier(15f);
            p_character.combatComponent.AdjustCritRate(5);
        }
        private void Level3(Character p_character) {
            p_character.classComponent.AddAbleClass("Shaman");
        }
        private void Level4(Character p_character) {
            p_character.combatComponent.AdjustIntelligencePercentModifier(15f);
            p_character.combatComponent.AdjustCritRate(5);
        }
        private void Level5(Character p_character) {
            p_character.classComponent.AddAbleClass("Mage");
        }
        #endregion
    }
}