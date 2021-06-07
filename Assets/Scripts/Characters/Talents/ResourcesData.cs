using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character_Talents {
    public class ResourcesData : CharacterTalentData {
        public ResourcesData() : base(CHARACTER_TALENT.Resources) {
            description = $"Determines how quick they perform resource gathering tasks and how much quantity they obtain per action.";
            addOnBonusSetDisplay = new List<string>(new string[] {
                "Can chop for Wood at a Lumberyard.\nCan mine for Stone at a Mine.",
                "Can chop for Wood at a Lumberyard.\nCan mine for Stone at a Mine.\nCan produce Cloth and Leather at a Skinner's Lodge.",
                "Can chop for Wood at a Lumberyard.\nCan mine for Stone at a Mine.\nCan produce Cloth and Leather at a Skinner's Lodge.",
                "Can chop for Wood at a Lumberyard.\nCan mine for Stone and Metals at a Mine.\nCan produce Cloth and Leather at a Skinner's Lodge.",
                "Can chop for Wood at a Lumberyard.\nCan mine for Stone and Metals at a Mine.\nCan produce Cloth and Leather at a Skinner's Lodge.",
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
            p_character.classComponent.AddAbleClass("Logger"); // and build a Lumberyard.
            p_character.classComponent.AddAbleClass("Miner"); // and build a Mine Shack but cannot Mine for metals yet - just dig for Stone.
        }
        private void Level2(Character p_character) {
            p_character.classComponent.AddAbleClass("Skinner");
            //if (p_character.race == RACE.HUMANS) {
            //    p_character.classComponent.AddAbleClass("Trapper");
            //}
            //if (p_character.race == RACE.ELVES) {
            //    p_character.classComponent.AddAbleClass("Skinner");
            //}
        }
        private void Level3(Character p_character) {
            //character can gather resources up to twice a day
        }
        private void Level4(Character p_character) {
            //Character can now Mine for metals.
        }
        private void Level5(Character p_character) {
            //Character can gather resources up to 3x a day.
        }
        #endregion

        #region Reevaluation
        public override void OnReevaluateTalent(Character p_character, int level) {
        }
        #endregion
    }
}