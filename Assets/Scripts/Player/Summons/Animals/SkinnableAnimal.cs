using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnableAnimal : Summon {
    public int count { set; get; }

    public override System.Type serializedData => typeof(SaveDataSkinnableAnimal);

    public SkinnableAnimal(SUMMON_TYPE summonType, string className, RACE race, GENDER gender) : base(summonType, className, race, gender) {
        count = 80;
    }
    public SkinnableAnimal(SaveDataSkinnableAnimal data) : base(data) {
        SaveDataSkinnableAnimal sd = data as SaveDataSkinnableAnimal;
        count = sd.count;
    }
}

#region Save Data
public class SaveDataSkinnableAnimal : SaveDataSummon {

    public int count;
    public bool isAvailableForShearing;

    public override void Save(Character character) {
        base.Save(character);
        SkinnableAnimal obj = character as SkinnableAnimal;
        count = obj.count;
    }

    public override Character Load() {
        SkinnableAnimal obj = base.Load() as SkinnableAnimal;
        obj.count = count;
        return obj as Character;
    }
}
#endregion