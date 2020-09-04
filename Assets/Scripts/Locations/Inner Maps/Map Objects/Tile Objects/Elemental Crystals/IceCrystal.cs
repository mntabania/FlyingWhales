public class IceCrystal : ElementalCrystal{

    public IceCrystal() : base(ELEMENTAL_TYPE.Ice) {
        Initialize(TILE_OBJECT_TYPE.ICE_CRYSTAL);
    }
    public IceCrystal(SaveDataTileObject data) : base(data, ELEMENTAL_TYPE.Ice) { }

}
