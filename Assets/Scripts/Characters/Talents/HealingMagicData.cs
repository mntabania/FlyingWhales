﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character_Talents {
    public class HealingMagicData : CharacterTalentData {
        public HealingMagicData() : base(CHARACTER_TALENT.Healing_Magic) {
            description = $"Mastery of restorative magic to cure physical and magical afflictions.";
            addOnBonusSetDisplay = new List<string>(new string[] {
                "No healing abilities.",
                "Can work at the Hospice and heal Injuries.\nCan cast Basic Heal in combat.",
                "Can work at the Hospice and heal Injuries and Plague.\nCan cast Expert Heal in combat.",
                "Can work at the Hospice and heal Injuries and Plague.\nCan cast Max Heal in combat.",
                "Can work at the Hospice and heal Injuries and Plague.\nCan cast Group Heal in combat.",
            });
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
            //Can cure @Injured 
        }
        private void Level2(Character p_character) {
            /*
             * - If character is a Magic-based combatant (Mage, Shaman, Druid), can build and work on a [Hospice](https://www.notion.so/Hospice-952a54abe7754360bb035f69ef4bdc53)
                - If character is a Magic-based combatant (Mage, Shaman, Druid), can use [Basic Heal](https://www.notion.so/Basic-Heal-38b32bad680d435496d9f1cb8962440c) in combat.
            */
        }
        private void Level3(Character p_character) {
            /*
             * - Can cure [Plagued](https://www.notion.so/Plagued-be20b6d4f7464591a29ab6ea1af514f7)
                - Increase Healing by 25% (change Heal Skill)
            */
        }
        private void Level4(Character p_character) {
            /*
             * - Can perform [Remove Flaw](https://www.notion.so/Remove-Flaw-ba785123e8fc46a69fc3d3897e872ef3)
                - Increase Healing by 25% (change Heal Skill)
            */
        }
        private void Level5(Character p_character) {
            /*
             * - Can cure Vampirism and Lycanthropy ([Dispel](https://www.notion.so/Dispel-a1744f1b131a4c3e8d60487d68d87950))
                - If character is a Magic-based combatant, replace [Basic Heal](https://www.notion.so/Basic-Heal-38b32bad680d435496d9f1cb8962440c) with Group Heal in combat.
            */
        }
        #endregion
    }
}