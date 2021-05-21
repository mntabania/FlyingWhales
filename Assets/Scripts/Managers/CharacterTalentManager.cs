using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Character_Talents;
public class CharacterTalentManager : BaseMonoBehaviour {

    public const int CHARACTER_TALENT_MAX_EXP = 100;
    public const int CHARACTER_TALENT_MAX_LEVEL = 5;

    public CHARACTER_TALENT[] allTalentEnums;

    private Dictionary<CHARACTER_TALENT, CharacterTalentData> _characterTalentDataDictionary;

    #region General
    public void Initialize() {
        _characterTalentDataDictionary = new Dictionary<CHARACTER_TALENT, CharacterTalentData>();
    }
    #endregion

    #region Character Talents
    public CharacterTalentData GetOrCreateCharacterTalentData(CHARACTER_TALENT p_talentType) {
        if (_characterTalentDataDictionary.ContainsKey(p_talentType)) {
            return _characterTalentDataDictionary[p_talentType];
        }
        CharacterTalentData loadedData = CreateCharacterTalentData(p_talentType);
        if (loadedData == null) {
            throw new Exception($"There are no talent scriptable object for {p_talentType}");
        }
        _characterTalentDataDictionary.Add(p_talentType, loadedData);
        return loadedData;
    }
    private CharacterTalentData CreateCharacterTalentData(CHARACTER_TALENT p_talentType) {
        CharacterTalentData data = null;
        string talentTypeString = p_talentType.ToString();
        var typeName = $"Character_Talents.{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(talentTypeString)}Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        System.Type type = System.Type.GetType(typeName);
        if (type != null) {
           data = System.Activator.CreateInstance(type) as CharacterTalentData;
        }
        return data;
    }
    #endregion
}
