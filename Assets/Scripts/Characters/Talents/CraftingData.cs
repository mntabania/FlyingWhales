using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character_Talents {
    public class CraftingData : CharacterTalentData {
        public CraftingData() : base(CHARACTER_TALENT.Crafting) {
            description = $"Governs quality and speed of building construction, item creation and repair.";
            addOnBonusSetDisplay = new List<string>(new string[] {
                "Can craft Weapons at a Workshop.",
                "Can craft Weapons and Armors at a Workshop.",
                "Can craft Weapons and Armors at a Workshop.\nSmall chance to produce High Quality products.",
                "Can craft Weapons, Armors and Accessories at a Workshop.\nSmall chance to produce High Quality products.",
                "Can craft Weapons, Armors and Accessories at a Workshop.\nSmall chance to produce Premium products.",
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
            p_character.classComponent.AddAbleClass("Crafter");
            /*
             * - 20% chance to add [High Quality](https://www.notion.so/High-Quality-f225af49428d479eb877fb489611f35a) Trait to crafted object
             * */
        }
        private void Level2(Character p_character) {
            /*
             * Character can now also craft Armor in @Workshop. 
             * */
        }
        private void Level3(Character p_character) {
            /*
             * 20% chance to add @High Quality Trait to crafted object
             * */
        }
        private void Level4(Character p_character) {
            /*
             * Character can now also craft Accessory in @Workshop
             * */
        }
        private void Level5(Character p_character) {
            /*
             * - 20% chance to add [Premium](https://www.notion.so/Premium-d6f2a5aee9f34dc697afbe5929eb7c96) Trait to crafted object
                - Otherwise, 20% chance to add [High Quality](https://www.notion.so/High-Quality-f225af49428d479eb877fb489611f35a) Trait to crafted object
            */
        }
        #endregion

        #region Reevaluation
        public override void OnReevaluateTalent(Character p_character, int level) {
        }
        #endregion
    }
}