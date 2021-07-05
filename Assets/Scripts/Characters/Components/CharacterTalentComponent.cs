using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Character_Talents;
using UtilityScripts;

public class CharacterTalentComponent : CharacterComponent {
    public List<CharacterTalent> allTalents { get; private set; }
    private Dictionary<CHARACTER_TALENT, CharacterTalent> _talentDictionary;

    public CharacterTalentComponent() {
    }

    public CharacterTalentComponent(SaveDataCharacterTalentComponent data) {
        LoadAllTalents(data);
    }

    #region General
    public void ConstructAllTalents() {
        allTalents = new List<CharacterTalent>();
        _talentDictionary = new Dictionary<CHARACTER_TALENT, CharacterTalent>();
        for (int i = 0; i < CharacterManager.Instance.talentManager.allTalentEnums.Length; i++) {
            CHARACTER_TALENT talentType = CharacterManager.Instance.talentManager.allTalentEnums[i];
            CharacterTalent talent = ObjectPoolManager.Instance.CreateNewCharacterTalent();
            talent.SetTalentType(talentType);
            talent.SetExperience(0);
            talent.SetLevel(1, owner);
            allTalents.Add(talent);
            _talentDictionary.Add(talentType, talent);
        }
    }
    public void RandomizeInitialTalents(Character p_character) {
        List<CHARACTER_TALENT> talentChoices = RuinarchListPool<CHARACTER_TALENT>.Claim();
        talentChoices.AddRange(CharacterManager.Instance.talentManager.allTalentEnums);
        //random talent +2 extra points
        CHARACTER_TALENT chosenTalent = CollectionUtilities.GetRandomElement(talentChoices);
        CharacterTalent talent = GetTalent(chosenTalent);
        for (int i = 0; i < 2; i++) {
            if (GameManager.Instance.gameHasStarted) {
                talent.LevelUp(p_character);    
            } else {
              talent.LevelUpForInitialVillagersInWorldGen(p_character);  
            }
        }
        talentChoices.Remove(chosenTalent);
        
        //random talent +1 extra point
        chosenTalent = CollectionUtilities.GetRandomElement(talentChoices);
        talent = GetTalent(chosenTalent);
        if (GameManager.Instance.gameHasStarted) {
            talent.LevelUp(p_character);    
        } else {
            talent.LevelUpForInitialVillagersInWorldGen(p_character);  
        }
    }
    private void LoadAllTalents(SaveDataCharacterTalentComponent data) {
        allTalents = new List<CharacterTalent>();
        _talentDictionary = new Dictionary<CHARACTER_TALENT, CharacterTalent>();
        for (int i = 0; i < data.allTalents.Count; i++) {
            CharacterTalent ct = data.allTalents[i].Load();
            allTalents.Add(ct);
            _talentDictionary.Add(ct.talentType, ct);
        }
    }
    public void ReevaluateAllTalents() {
        for (int i = 0; i < allTalents.Count; i++) {
            allTalents[i].ReevaluateTalent(owner);
        }
    }
    public CharacterTalent GetTalent(CHARACTER_TALENT p_talentType) {
        if (_talentDictionary.ContainsKey(p_talentType)) {
            return _talentDictionary[p_talentType];
        }
        return null;
    }
    public string GetTalentSummary() {
        string text = string.Empty;
        for (int i = 0; i < allTalents.Count; i++) {
            if (i > 0) {
                text += "\n";
            }
            CharacterTalent talent = allTalents[i];
            CharacterTalentData talentData = CharacterManager.Instance.talentManager.GetOrCreateCharacterTalentData(talent.talentType);
            text += "Lvl." + talent.level + " " + talentData.name + ": " + talent.experience + "/" + CharacterTalentManager.CHARACTER_TALENT_MAX_EXP;
        }
        return text;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCharacterTalentComponent data) {
    }
    #endregion
}

[System.Serializable]
public class SaveDataCharacterTalentComponent : SaveData<CharacterTalentComponent> {
    public List<SaveDataCharacterTalent> allTalents;

    #region Overrides
    public override void Save(CharacterTalentComponent data) {
        allTalents = new List<SaveDataCharacterTalent>();
        for (int i = 0; i < data.allTalents.Count; i++) {
            CharacterTalent ct = data.allTalents[i];
            SaveDataCharacterTalent sct = new SaveDataCharacterTalent();
            sct.Save(ct);
            allTalents.Add(sct);
        }
    }

    public override CharacterTalentComponent Load() {
        CharacterTalentComponent component = new CharacterTalentComponent(this);
        return component;
    }
    #endregion
}