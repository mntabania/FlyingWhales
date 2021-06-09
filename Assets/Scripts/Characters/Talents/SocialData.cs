using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character_Talents {
    public class SocialData : CharacterTalentData {
        public SocialData() : base(CHARACTER_TALENT.Social) {
            description = $"Aptitude in trading, leadership and scheming.";
            addOnBonusSetDisplay = new List<string>(new string[] {
                "",
                "Unlocks the Merchant class.",
                "Unlocks the Merchant class.",
                "Unlocks the Merchant class.",
                "Unlocks the Merchant class.",
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
            //no bonus
        }
        private void Level2(Character p_character) {
            //+50 Weight in Popularity Succession
            p_character.classComponent.AddAbleClass("Merchant");
            p_character.faction?.successionComponent.UpdateSuccessors();
        }
        private void Level3(Character p_character) {
            //+100 Weight in Popularity Succession
            p_character.faction?.successionComponent.UpdateSuccessors();
        }
        private void Level4(Character p_character) {
            //+250 Weight in Popularity Succession
            p_character.faction?.successionComponent.UpdateSuccessors();
        }
        private void Level5(Character p_character) {
            //+250 Weight in Popularity Succession
            p_character.faction?.successionComponent.UpdateSuccessors();
        }
        #endregion

        #region Reevaluation
        public override void OnReevaluateTalent(Character p_character, int level) {
        }
        #endregion
    }
}