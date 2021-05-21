public class WolfDen : TileObject {
    public WolfDen() {
        Initialize(TILE_OBJECT_TYPE.WOLF_DEN);
    }
    public WolfDen(SaveDataTileObject data) : base(data) {
        
    }
}