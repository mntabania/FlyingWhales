public class SkullLamp : TileObject{
    public SkullLamp() {
        Initialize(TILE_OBJECT_TYPE.SKULL_LAMP);
    }
    public SkullLamp(SaveDataTileObject data) {
        Initialize(data);
    }
}