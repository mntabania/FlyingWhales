public class CorruptedPit : TileObject{
    public CorruptedPit() {
        Initialize(TILE_OBJECT_TYPE.CORRUPTED_PIT);
    }
    public CorruptedPit(SaveDataTileObject data) {
        Initialize(data);
    }
}