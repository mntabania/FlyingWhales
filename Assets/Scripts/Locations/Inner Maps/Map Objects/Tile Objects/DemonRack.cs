public class DemonRack : TileObject{
    public DemonRack() {
        Initialize(TILE_OBJECT_TYPE.DEMON_RACK);
    }
    public DemonRack(SaveDataTileObject data) {
        Initialize(data);
    }
}