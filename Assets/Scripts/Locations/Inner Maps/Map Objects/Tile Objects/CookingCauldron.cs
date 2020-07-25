
public class CookingCauldron : TileObject {

    public CookingCauldron() {
        Initialize(TILE_OBJECT_TYPE.COOKING_CAULDRON);
        //AddAdvertisedAction(INTERACTION_TYPE.COOK);
    }
    public CookingCauldron(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.COOK);
    }
}
