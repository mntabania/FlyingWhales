public class Eyeball : TileObject{
    public Eyeball() {
        Initialize(TILE_OBJECT_TYPE.EYEBALL);
    }
    public Eyeball(SaveDataTileObject data) {
        Initialize(data);
    }
}