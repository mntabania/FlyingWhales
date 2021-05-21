﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character_Talents {
    public class MartialArtsData : CharacterTalentData {
        public MartialArtsData() : base(CHARACTER_TALENT.Martial_Arts) {
            description = $"Prowess in physical combat.";
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
            p_character.classComponent.AddAbleClass("Marauder");
            p_character.classComponent.AddAbleClass("Archer");
        }
        private void Level2(Character p_character) {
            p_character.combatComponent.AdjustStrengthPercentModifier(15f);
            p_character.combatComponent.AdjustMaxHPPercentModifier(10f);
        }
        private void Level3(Character p_character) {
            p_character.classComponent.AddAbleClass("Barbarian");
            p_character.classComponent.AddAbleClass("Stalker");
        }
        private void Level4(Character p_character) {
            p_character.combatComponent.AdjustStrengthPercentModifier(15f);
            p_character.combatComponent.AdjustMaxHPPercentModifier(10f);
        }
        private void Level5(Character p_character) {
            p_character.classComponent.AddAbleClass("Knight");
            p_character.classComponent.AddAbleClass("Hunter");
        }
        #endregion
    }
}