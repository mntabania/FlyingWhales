using System.Collections.Generic;

public class PlinthBook : TileObject{
    public PlinthBook() {
        Initialize(TILE_OBJECT_TYPE.PLINTH_BOOK);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
    public PlinthBook(SaveDataTileObject data) {
        Initialize(data);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
    }
}
