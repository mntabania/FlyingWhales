public class Sigil : TileObject{
    public Sigil() {
        Initialize(TILE_OBJECT_TYPE.SIGIL);
    }
    public Sigil(SaveDataTileObject data) {
        Initialize(data);
    }
}