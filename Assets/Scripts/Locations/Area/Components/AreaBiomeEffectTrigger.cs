using System;

public class AreaBiomeEffectTrigger : AreaComponent {

    public bool hasBeenInitialized { get; private set; }

    public AreaBiomeEffectTrigger() {
        hasBeenInitialized = false;
    }
    public AreaBiomeEffectTrigger(SaveDataAreaBiomeEffectTrigger data) {
        hasBeenInitialized = data.hasBeenInitialized;
    }
}

public class SaveDataAreaBiomeEffectTrigger : SaveData<AreaBiomeEffectTrigger> {
    public bool hasBeenInitialized;

    public override void Save(AreaBiomeEffectTrigger data) {
        base.Save(data);
        hasBeenInitialized = data.hasBeenInitialized;
    }
    public override AreaBiomeEffectTrigger Load() {
        AreaBiomeEffectTrigger component = new AreaBiomeEffectTrigger(this);
        return component;
    }
}