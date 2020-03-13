public class Cage : TileObject{
    
    public Cage() {
        Initialize(TILE_OBJECT_TYPE.CAGE);
    }
    public Cage(SaveDataTileObject data) {
        Initialize(data);
    }
}