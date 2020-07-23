using System;

public class HexTileBiomeEffectTrigger {

    private HexTile _hexTile;

    private bool _hasBeenInitialized;

    public HexTileBiomeEffectTrigger(HexTile hexTile) {
        _hexTile = hexTile;
        _hasBeenInitialized = false;
    }

    #region Events
    public void Initialize() {
        AddListenersBasedOnBiome();
    }
    public void ProcessBeforeBiomeChange() {
        if (_hasBeenInitialized) {
            RemoveListenersBasedOnBiome();    
        }
    }
    public void ProcessAfterBiomeChange() {
        if (_hasBeenInitialized) {
            AddListenersBasedOnBiome();    
        }
    }
    #endregion

    #region Listeners
    private void AddListenersBasedOnBiome() {
        switch (_hexTile.biomeType) {
            case BIOMES.GRASSLAND:
                break;
            case BIOMES.SNOW:
            case BIOMES.TUNDRA:
                Messenger.AddListener(Signals.HOUR_STARTED, TryFreezeWetObjects);
                break;
            case BIOMES.DESERT:
                Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, TryRemoveFreezing);
                break;
            case BIOMES.FOREST:
                break;
        }
    }
    private void RemoveListenersBasedOnBiome() {
        switch (_hexTile.biomeType) {
            case BIOMES.GRASSLAND:
                break;
            case BIOMES.SNOW:
            case BIOMES.TUNDRA:
                Messenger.RemoveListener(Signals.HOUR_STARTED, TryFreezeWetObjects);
                break;
            case BIOMES.DESERT:
                Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, TryRemoveFreezing);
                break;
            case BIOMES.FOREST:
                break;
        }
    }
    #endregion

    #region Snow
    private void TryFreezeWetObjects() {
        Messenger.Broadcast(Signals.FREEZE_WET_OBJECTS_IN_TILE, _hexTile);
    }
    #endregion
    
    #region Desert
    private void TryRemoveFreezing(Character character, HexTile hexTile) {
        if (_hexTile == hexTile) {
            character.traitContainer.RemoveTrait(character, "Freezing");
            character.traitContainer.RemoveTrait(character, "Frozen");
        }
    }
    #endregion
}
