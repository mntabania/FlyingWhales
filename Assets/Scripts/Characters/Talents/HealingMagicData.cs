using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character_Talents {
    public class HealingMagicData : CharacterTalentData {
        public override bool hasReevaluation => true;
        public HealingMagicData() : base(CHARACTER_TALENT.Healing_Magic) {
            description = $"Mastery of restorative magic to cure physical and magical afflictions.";
            addOnBonusSetDisplay = new List<string>(new string[] {
                "No healing abilities.",
                "Can work at the Hospice and heal Injuries.\nIf magic-user, Can cast Basic Heal in combat.",
                "Can work at the Hospice and heal Injuries and Plague.\nIf magic-user, Can cast Expert Heal in combat.",
                "Can work at the Hospice and heal Injuries and Plague.\nIf magic-user, Can cast Max Heal in combat.",
                "Can work at the Hospice and heal Injuries and Plague.\nIf magic-user, Can cast Group Heal in combat.",
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
            if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL) {
                p_character.combatComponent.specialSkillParent.SetSpecialSkill(COMBAT_SPECIAL_SKILL.Heal);
            }
            /*
             * - If character is a Magic-based combatant (Mage, Shaman, Druid), can build and work on a [Hospice](https://www.notion.so/Hospice-952a54abe7754360bb035f69ef4bdc53)
                - If character is a Magic-based combatant (Mage, Shaman, Druid), can use [Basic Heal](https://www.notion.so/Basic-Heal-38b32bad680d435496d9f1cb8962440c) in combat.
            */
        }
        private void Level3(Character p_character) {
            if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL) {
                p_character.combatComponent.specialSkillParent.SetSpecialSkill(COMBAT_SPECIAL_SKILL.Strong_Heal);
            }
            /*
             * - Can cure [Plagued](https://www.notion.so/Plagued-be20b6d4f7464591a29ab6ea1af514f7)
                - Increase Healing by 25% (change Heal Skill)
            */
        }
        private void Level4(Character p_character) {
            if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL) {
                p_character.combatComponent.specialSkillParent.SetSpecialSkill(COMBAT_SPECIAL_SKILL.Max_Heal);
            }
            /*
             * - Can perform [Remove Flaw](https://www.notion.so/Remove-Flaw-ba785123e8fc46a69fc3d3897e872ef3)
                - Increase Healing by 25% (change Heal Skill)
            */
        }
        private void Level5(Character p_character) {
            if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL) {
                p_character.combatComponent.specialSkillParent.SetSpecialSkill(COMBAT_SPECIAL_SKILL.Group_Heal);
            }
            /*
             * - Can cure Vampirism and Lycanthropy ([Dispel](https://www.notion.so/Dispel-a1744f1b131a4c3e8d60487d68d87950))
                - If character is a Magic-based combatant, replace [Basic Heal](https://www.notion.so/Basic-Heal-38b32bad680d435496d9f1cb8962440c) with Group Heal in combat.
            */
        }
        #endregion

        #region Reevaluation
        public override void OnReevaluateTalent(Character p_character, int level) {
            switch (level) {
                case 1:
                    ReevaluateLevel1(p_character);
                    break;
                case 2:
                    ReevaluateLevel2(p_character);
                    break;
                case 3:
                    ReevaluateLevel3(p_character);
                    break;
                case 4:
                    ReevaluateLevel4(p_character);
                    break;
                case 5:
                    ReevaluateLevel5(p_character);
                    break;
            }
        }
        private void ReevaluateLevel1(Character p_character) {
            //N/A
        }
        private void ReevaluateLevel2(Character p_character) {
            if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL) {
                p_character.combatComponent.specialSkillParent.SetSpecialSkill(COMBAT_SPECIAL_SKILL.Heal);
            }
            //- If character is a Magic-based combatant (Mage, Shaman, Druid), can build and work on a [Hospice](https://www.notion.so/Hospice-952a54abe7754360bb035f69ef4bdc53)
        }
        private void ReevaluateLevel3(Character p_character) {
            if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL) {
                p_character.combatComponent.specialSkillParent.SetSpecialSkill(COMBAT_SPECIAL_SKILL.Strong_Heal);
            }
        }
        private void ReevaluateLevel4(Character p_character) {
            if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL) {
                p_character.combatComponent.specialSkillParent.SetSpecialSkill(COMBAT_SPECIAL_SKILL.Max_Heal);
            }
        }
        private void ReevaluateLevel5(Character p_character) {
            if (p_character.classComponent.characterClass.attackType == ATTACK_TYPE.MAGICAL) {
                p_character.combatComponent.specialSkillParent.SetSpecialSkill(COMBAT_SPECIAL_SKILL.Group_Heal);
            }
        }
        #endregion
    }
}