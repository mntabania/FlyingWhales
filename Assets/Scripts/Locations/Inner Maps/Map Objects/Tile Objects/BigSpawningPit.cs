public class BigSpawningPit : TileObject{
    public BigSpawningPit() {
        Initialize(TILE_OBJECT_TYPE.BIG_SPAWNING_PIT);
    }
    public BigSpawningPit(SaveDataTileObject data) {
        Initialize(data);
    }
}