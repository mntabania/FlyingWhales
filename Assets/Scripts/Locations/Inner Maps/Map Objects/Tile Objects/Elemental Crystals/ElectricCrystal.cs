public class ElectricCrystal : ElementalCrystal {

    public ElectricCrystal() : base(ELEMENTAL_TYPE.Electric) {
        Initialize(TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL);
    }
    public ElectricCrystal(SaveDataTileObject data, ELEMENTAL_TYPE _elementalType) : base(data, ELEMENTAL_TYPE.Electric) { }
}
