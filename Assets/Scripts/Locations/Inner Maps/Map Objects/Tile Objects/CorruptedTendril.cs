public class CorruptedTendril : TileObject{
    public CorruptedTendril() {
        Initialize(TILE_OBJECT_TYPE.CORRUPTED_TENDRIL);
    }
    public CorruptedTendril(SaveDataTileObject data) {
        Initialize(data);
    }
}