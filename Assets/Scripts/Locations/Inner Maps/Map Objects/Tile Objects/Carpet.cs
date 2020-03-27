public class Carpet : TileObject{
    public Carpet() {
        Initialize(TILE_OBJECT_TYPE.CARPET);
    }
    public Carpet(SaveDataTileObject data) {
        Initialize(data);
    }
}