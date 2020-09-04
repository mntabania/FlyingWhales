public class ElectricCrystal : ElementalCrystal {

    public ElectricCrystal() : base(ELEMENTAL_TYPE.Electric) {
        Initialize(TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL);
    }
    public ElectricCrystal(SaveDataTileObject data) : base(data) {
        elementalType = ELEMENTAL_TYPE.Electric;
    }
}
