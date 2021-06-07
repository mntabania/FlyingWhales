using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character_Talents {
    public class FoodData : CharacterTalentData {
        public FoodData() : base(CHARACTER_TALENT.Food) {
            description = $"Determines how quick they perform food producing tasks and how much quantity they obtain per action.";
            addOnBonusSetDisplay = new List<string>(new string[] {
                "Can harvest crops at a Farm.",
                "Can harvest crops at a Farm.\nCan fish at a Fishery.",
                "Can harvest crops at a Farm.\nCan fish at a Fishery.",
                "Can harvest crops at a Farm.\nCan fish at a Fishery.\nCan butcher for meat at a Butcher's Shop.",
                "Can harvest crops at a Farm.\nCan fish at a Fishery.\nCan butcher for meat at a Butcher's Shop.",
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
            p_character.classComponent.AddAbleClass("Farmer"); // and build farms
        }
        private void Level2(Character p_character) {
            p_character.classComponent.AddAbleClass("Fisher"); // and build fishery
        }
        private void Level3(Character p_character) {
            //Allows character to gather Food up to twice a day.
        }
        private void Level4(Character p_character) {
            p_character.classComponent.AddAbleClass("Butcher");//build a @Butcher's Shop and @Butcher Animals.
        }
        private void Level5(Character p_character) {
            //Allows character to gather Food up to 3x a day.
        }
        #endregion

        #region Reevaluation
        public override void OnReevaluateTalent(Character p_character, int level) {
        }
        #endregion
    }
}