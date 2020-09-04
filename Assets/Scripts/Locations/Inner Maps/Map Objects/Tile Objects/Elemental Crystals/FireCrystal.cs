public class FireCrystal : ElementalCrystal{

    public FireCrystal() : base(ELEMENTAL_TYPE.Fire) {
        Initialize(TILE_OBJECT_TYPE.FIRE_CRYSTAL);
    }
    public FireCrystal(SaveDataTileObject data) : base(data, ELEMENTAL_TYPE.Fire) { }
}
