using System;
using UnityEngine;

namespace Character_Talents {
    public abstract class CharacterTalentData {
        public CHARACTER_TALENT type { get; private set; }
        public string name { get; private set; }
        public string description { get; protected set; }

        public CharacterTalentData(CHARACTER_TALENT p_talentType) {
            type = p_talentType;
            name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(p_talentType.ToString());
        }

        public abstract void OnLevelUp(Character p_character, int level);
    }
}
