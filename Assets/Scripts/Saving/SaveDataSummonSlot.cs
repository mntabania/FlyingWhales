using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataSummonSlot {
    //Summon slot
    public int level;
    public int summonID;

    public void Save(SummonSlot summonSlot) {
        level = summonSlot.level;
        if(summonSlot.summon != null) {
            summonID = summonSlot.summon.id;
        } else {
            summonID = -1;
        }
    }

    public SummonSlot Load() {
        SummonSlot slot = new SummonSlot();
        //slot.SetLevel(level);
        if(summonID != -1) {
            slot.SetSummon(CharacterManager.Instance.GetCharacterByID(summonID) as Summon);
        }
        return slot;
    }
}
