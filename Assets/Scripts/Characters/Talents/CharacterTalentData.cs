using System;
using UnityEngine;
using System.Collections.Generic;

namespace Character_Talents {
    public abstract class CharacterTalentData {

        public List<string> addOnBonusSetDisplay = new List<string>();
        public CHARACTER_TALENT type { get; private set; }
        public string name { get; private set; }
        public string description { get; protected set; }

        public virtual string bonusDescription (int level) => GetBonusDescription(level);
        public virtual bool hasReevaluation => false;

        public string GetBonusDescription(int p_level) {
            /*if (addOnBonusSetDisplay.Count < p_level) {
                return name + " Lv. " + p_level + "\n\n" + description + "\n\n";
            }*/
            return addOnBonusSetDisplay[p_level - 1];
        }

        public CharacterTalentData(CHARACTER_TALENT p_talentType) {
            type = p_talentType;
            name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_talentType.ToString());
        }

        public abstract void OnLevelUp(Character p_character, int level);

        //This is used when the talent has not leveled up but we need to recheck if the effect still applies or not
        public abstract void OnReevaluateTalent(Character p_character, int level);
    }
}
