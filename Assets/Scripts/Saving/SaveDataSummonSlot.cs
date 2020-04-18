using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

[System.Serializable]
public class SaveDataSummon {
    public string firstName;
    public string surName;
    public string className;
    public RACE race;
    public GENDER gender;
    public SUMMON_TYPE summonType;
    public string[] traitNames;

    public SaveDataSummon(Summon summon) {
        firstName = summon.firstName;
        surName = summon.surName;
        className = summon.characterClass.className;
        race = summon.race;
        gender = summon.gender;
        summonType = summon.summonType;
        List<Trait> traits = summon.traitContainer.traits;
        if (traits != null && traits.Count > 0) {
            traitNames = new string[summon.traitContainer.traits.Count];
            for (int i = 0; i < traits.Count; i++) {
                traitNames[i] = traits[i].name;
            }
        }   
    }
}
