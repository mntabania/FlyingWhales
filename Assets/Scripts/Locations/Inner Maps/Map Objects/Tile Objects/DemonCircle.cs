public class DemonCircle : TileObject{
    public DemonCircle() {
        Initialize(TILE_OBJECT_TYPE.DEMON_CIRCLE);
    }
    public DemonCircle(SaveDataTileObject data) {
        Initialize(data);
    }
}