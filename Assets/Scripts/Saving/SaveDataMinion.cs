using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataMinion : SaveData<Minion> {
    //public int characterID; //Does not need to save character id because the process will be handled by SaveDataCharacter
    public bool isSummoned;
    public SPELL_TYPE minionPlayerSkillType;

    #region Saving
    public override void Save(Minion minion) {
        isSummoned = minion.isSummoned;
        minionPlayerSkillType = minion.minionPlayerSkillType;
    }
    #endregion

    public Minion Load(Character character) {
        Minion minion = CharacterManager.Instance.CreateNewMinion(character, keepData: true);
        return minion;
    }
}
