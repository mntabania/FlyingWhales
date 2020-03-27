public class GroundBolt : TileObject{
    public GroundBolt() {
        Initialize(TILE_OBJECT_TYPE.GROUND_BOLT);
    }
    public GroundBolt(SaveDataTileObject data) {
        Initialize(data);
    }
}