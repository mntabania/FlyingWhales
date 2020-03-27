public class Pew : TileObject{
    public Pew() {
        Initialize(TILE_OBJECT_TYPE.PEW);
    }
    public Pew(SaveDataTileObject data) {
        Initialize(data);
    }
}