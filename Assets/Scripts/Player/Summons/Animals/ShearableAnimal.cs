using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShearableAnimal : Animal {
    public int count { set; get; }
    public bool isAvailableForShearing { set; get; }

    public override System.Type serializedData => typeof(SaveDataShearableAnimal);

    public ShearableAnimal(SUMMON_TYPE summonType, string className, RACE race) : base(summonType, className, race) {
        count = 80;
    }
    public ShearableAnimal(SaveDataShearableAnimal data) : base(data) {
        SaveDataShearableAnimal sd = data as SaveDataShearableAnimal;
        count = sd.count;
        isAvailableForShearing = sd.isAvailableForShearing;
    }

    public override void SubscribeToSignals() {
        base.SubscribeToSignals();
        Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
    }

    public override void UnsubscribeSignals() {
        base.UnsubscribeSignals();
        Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
    }

    void OnDayStarted() {
        if (!isDead) {
            isAvailableForShearing = true;
        }

    }
}

#region Save Data
public class SaveDataShearableAnimal : SaveDataSummon {
    
    public int count;
    public bool isAvailableForShearing;

    public override void Save(Character character) {
        base.Save(character);
        ShearableAnimal obj = character as ShearableAnimal;
        count = obj.count;
        isAvailableForShearing = obj.isAvailableForShearing;
    }

    public override Character Load() {
        ShearableAnimal obj = base.Load() as ShearableAnimal;
        obj.count = count;
        obj.isAvailableForShearing = isAvailableForShearing;
        return obj as Character;
    }
}
#endregion