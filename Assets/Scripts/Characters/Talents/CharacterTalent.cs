using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character_Talents;

namespace Character_Talents {
    //This is a wrapper for the CharacterTalentData
    public class CharacterTalent {
        public CHARACTER_TALENT talentType { get; private set; }
        public int experience { get; private set; }
        public int level { get; private set; }

        #region Talent
        public void SetTalentType(CHARACTER_TALENT p_talentType) {
            talentType = p_talentType;
        }
        #endregion

        #region Experience
        public void SetExperience(int p_amount) {
            experience = p_amount;
        }
        public void AdjustExperience(int p_amount, Character p_character) {
            experience += p_amount;
            if (experience < 0) {
                experience = 0;
            } else if (experience > CharacterTalentManager.CHARACTER_TALENT_MAX_EXP) {
                experience = CharacterTalentManager.CHARACTER_TALENT_MAX_EXP;
            }
            if (experience == CharacterTalentManager.CHARACTER_TALENT_MAX_EXP) {
                //Level Up
                LevelUp(p_character);
                SetExperience(0);
            }
        }
        #endregion

        #region Level
        public void SetLevel(int p_amount) {
            level = p_amount;
            if (level < 1) {
                level = 1;
            } else if (level > CharacterTalentManager.CHARACTER_TALENT_MAX_LEVEL) {
                level = CharacterTalentManager.CHARACTER_TALENT_MAX_LEVEL;
            }
        }
        public void SetLevel(int p_amount, Character p_character) {
            SetLevel(p_amount);
            ApplyEffectsBasedOnLevel(p_character);
        }
        public void LevelUp(Character p_character) {
            SetLevel(level + 1);
            CharacterTalentData talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talentType);
            talentData.OnLevelUp(p_character, level);
            if (p_character.race == RACE.HUMANS) { 
                p_character.ApplyClassBonusOnLevelUp(p_character.classComponent.characterClass.className);
            }
        }
        public void LevelUpForInitialVillagersInWorldGen(Character p_character) {
            SetLevel(level + 1);
            CharacterTalentData talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talentType);
            talentData.OnLevelUp(p_character, level);
        }
        public void ApplyEffectsBasedOnLevel(Character p_character) {
            CharacterTalentData talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talentType);
            for (int i = 1; i <= level; i++) {
                talentData.OnLevelUp(p_character, i);
            }
        }
        public void ReevaluateTalent(Character p_character) {
            CharacterTalentData talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talentType);
            if (talentData.hasReevaluation) {
                for (int i = 1; i <= level; i++) {
                    talentData.OnReevaluateTalent(p_character, i);
                }
            }
        }
        #endregion

        #region Object Pool
        public void Reset() {
            talentType = CHARACTER_TALENT.None;
            experience = 0;
            level = 1;
        }
        #endregion
    }
}

public class SaveDataCharacterTalent : SaveData<CharacterTalent> {
    public CHARACTER_TALENT talentType;
    public int experience;
    public int level;

    #region Overrides
    public override void Save(CharacterTalent data) {
        talentType = data.talentType;
        experience = data.experience;
        level = data.level;
    }

    public override CharacterTalent Load() {
        CharacterTalent data = ObjectPoolManager.Instance.CreateNewCharacterTalent();
        data.SetTalentType(talentType);
        data.SetExperience(experience);
        data.SetLevel(level);
        return data;
    }
    #endregion
}
