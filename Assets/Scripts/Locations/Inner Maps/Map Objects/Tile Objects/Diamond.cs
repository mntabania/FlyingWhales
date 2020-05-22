public class Diamond : TileObject{
    public Diamond() {
        Initialize(TILE_OBJECT_TYPE.DIAMOND);
    }
    public Diamond(SaveDataTileObject data) {
        Initialize(data);
    }
}