public class PlinthOrb : TileObject{
    public PlinthOrb() {
        Initialize(TILE_OBJECT_TYPE.PLINTH_ORB);
    }
    public PlinthOrb(SaveDataTileObject data) {
        Initialize(data);
    }
}