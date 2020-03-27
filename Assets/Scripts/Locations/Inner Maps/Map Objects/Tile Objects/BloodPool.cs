public class BloodPool : TileObject{
    public BloodPool() {
        Initialize(TILE_OBJECT_TYPE.BLOOD_POOL);
    }
    public BloodPool(SaveDataTileObject data) {
        Initialize(data);
    }
}