public class WaterCrystal : ElementalCrystal{

    public WaterCrystal() : base(ELEMENTAL_TYPE.Water) {
        Initialize(TILE_OBJECT_TYPE.WATER_CRYSTAL);
    }
    public WaterCrystal(SaveDataTileObject saveDataTileObject) : base(ELEMENTAL_TYPE.Water) { }
}
