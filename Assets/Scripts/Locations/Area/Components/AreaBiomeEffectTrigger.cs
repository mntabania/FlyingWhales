using System;

public class AreaBiomeEffectTrigger : AreaComponent {

    public bool hasBeenInitialized { get; private set; }

    public AreaBiomeEffectTrigger() {
        hasBeenInitialized = false;
    }
    public AreaBiomeEffectTrigger(SaveDataAreaBiomeEffectTrigger data) {
        hasBeenInitialized = data.hasBeenInitialized;
    }

    #region Events
    public void Initialize() {
        AddListenersBasedOnBiome();
    }
    public void ProcessBeforeBiomeChange() {
        if (hasBeenInitialized) {
            RemoveListenersBasedOnBiome();    
        }
    }
    public void ProcessAfterBiomeChange() {
        if (hasBeenInitialized) {
            AddListenersBasedOnBiome();    
        }
    }
    #endregion

    #region Listeners
    private void AddListenersBasedOnBiome() {
        switch (owner.areaData.biomeType) {
            case BIOMES.GRASSLAND:
                break;
            case BIOMES.SNOW:
            case BIOMES.TUNDRA:
                Messenger.AddListener(Signals.HOUR_STARTED, TryFreezeWetObjects);
                break;
            case BIOMES.DESERT:
                Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, TryRemoveFreezing);
                break;
            case BIOMES.FOREST:
                break;
        }
    }
    private void RemoveListenersBasedOnBiome() {
        switch (owner.areaData.biomeType) {
            case BIOMES.GRASSLAND:
                break;
            case BIOMES.SNOW:
            case BIOMES.TUNDRA:
                Messenger.RemoveListener(Signals.HOUR_STARTED, TryFreezeWetObjects);
                break;
            case BIOMES.DESERT:
                Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, TryRemoveFreezing);
                break;
            case BIOMES.FOREST:
                break;
        }
    }
    #endregion

    #region Snow
    private void TryFreezeWetObjects() {
        Messenger.Broadcast(AreaSignals.FREEZE_WET_OBJECTS_IN_AREA, owner);
    }
    #endregion
    
    #region Desert
    private void TryRemoveFreezing(Character character, Area p_area) {
        if (owner == p_area) {
            character.traitContainer.RemoveTrait(character, "Freezing");
            character.traitContainer.RemoveTrait(character, "Frozen");
        }
    }
    #endregion
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