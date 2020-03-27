public class SpawningPit : TileObject{
    public SpawningPit() {
        Initialize(TILE_OBJECT_TYPE.SPAWNING_PIT);
    }
    public SpawningPit(SaveDataTileObject data) {
        Initialize(data);
    }
}