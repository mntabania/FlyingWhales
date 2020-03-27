public class CorruptedSpike : TileObject{
    public CorruptedSpike() {
        Initialize(TILE_OBJECT_TYPE.CORRUPTED_SPIKE);
    }
    public CorruptedSpike(SaveDataTileObject data) {
        Initialize(data);
    }
}