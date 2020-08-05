
public class TrollCauldron : TileObject {

    public TrollCauldron() {
        Initialize(TILE_OBJECT_TYPE.TROLL_CAULDRON);
        //AddAdvertisedAction(INTERACTION_TYPE.COOK);
    }
    public TrollCauldron(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.COOK);
    }
}
