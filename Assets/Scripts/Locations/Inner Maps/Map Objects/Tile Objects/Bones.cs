public class Bones : TileObject{
    public Bones() {
        Initialize(TILE_OBJECT_TYPE.BONES);
    }
    public Bones(SaveDataTileObject data) {
        Initialize(data);
    }
}