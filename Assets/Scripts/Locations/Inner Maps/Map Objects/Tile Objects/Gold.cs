public class Gold : TileObject{
    public Gold() {
        Initialize(TILE_OBJECT_TYPE.GOLD);
    }
    public Gold(SaveDataTileObject data) {
        Initialize(data);
    }
}