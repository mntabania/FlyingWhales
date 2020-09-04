public class IceCrystal : ElementalCrystal{

    public IceCrystal() : base(ELEMENTAL_TYPE.Ice) {
        Initialize(TILE_OBJECT_TYPE.ICE_CRYSTAL);
    }
    public IceCrystal(SaveDataTileObject data, ELEMENTAL_TYPE _elementalType) : base(data, ELEMENTAL_TYPE.Ice) { }

}
