public class Grimoire : TileObject{
    public Grimoire() {
        Initialize(TILE_OBJECT_TYPE.GRIMOIRE);
    }
    public Grimoire(SaveDataTileObject data) {
        Initialize(data);
    }
}