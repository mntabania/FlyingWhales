public class PoisonCrystal : ElementalCrystal{

    public PoisonCrystal() : base(ELEMENTAL_TYPE.Poison) {
        Initialize(TILE_OBJECT_TYPE.POISON_CRYSTAL);
    }
    public PoisonCrystal(SaveDataTileObject data, ELEMENTAL_TYPE _elementalType) : base(data, ELEMENTAL_TYPE.Poison) { }
}
